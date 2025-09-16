//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BenchmarkConsoleApp;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

using HslCommunication.Profinet.Siemens;

using S7.Net;

using System.IO.Pipelines;

using ThingsGateway.Foundation.SiemensS7;

using TouchSocket.Core;

namespace ThingsGateway.Foundation;

[MemoryDiagnoser]
public class S7Benchmark : IDisposable
{
    private List<SiemensS7Master> siemensS7s = new();

    private List<Plc> plcs = new();
    private List<SiemensS7Net> siemensS7Nets = new();

    public S7Benchmark()

    {
        {
            for (int i = 0; i < Program.ClientCount; i++)
            {
                var clientConfig = new TouchSocket.Core.TouchSocketConfig();

                var clientChannel = clientConfig.GetChannel(new ChannelOptions() { ChannelType = ChannelTypeEnum.TcpClient, RemoteUrl = "127.0.0.1:102" });
                var siemensS7 = new SiemensS7Master()
                {
                    //modbus协议格式
                    SiemensS7Type = SiemensTypeEnum.S1500
                };
                siemensS7.InitChannel(clientChannel);
                clientChannel.SetupAsync(clientChannel.Config).GetFalseAwaitResult();
                clientChannel.Logger.LogLevel = LogLevel.Warning;
                siemensS7.ConnectAsync(CancellationToken.None).GetFalseAwaitResult();
                siemensS7.ReadAsync("M1", 100).GetAwaiter().GetResult();
                siemensS7s.Add(siemensS7);
            }
            for (int i = 0; i < Program.ClientCount; i++)
            {
                var siemensS7Net = new SiemensS7Net(SiemensPLCS.S1500, "127.0.0.1");
                siemensS7Net.ConnectServer();
                siemensS7Net.ReadAsync("M0", 100).GetFalseAwaitResult();
                siemensS7Nets.Add(siemensS7Net);
            }
            for (int i = 0; i < Program.ClientCount; i++)
            {
                var plc = new Plc(CpuType.S7300, "127.0.0.1", 102, 0, 0);
                plc.Open();//打开plc连接
                plc.ReadAsync(DataType.Memory, 1, 0, VarType.Byte, 100).GetFalseAwaitResult();
                plcs.Add(plc);
            }
        }
    }

    [Benchmark]
    public async Task S7netplus()
    {
        List<Task> tasks = new List<Task>();
        foreach (var plc in plcs)
        {
            for (int i = 0; i < Program.TaskNumberOfItems; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    for (int i = 0; i < Program.NumberOfItems; i++)
                    {
                        var result = await plc.ReadAsync(DataType.Memory, 1, 0, VarType.Byte, 100);
                    }
                }));
            }
        }
        await Task.WhenAll(tasks);
    }

    //并发失败
    //[Benchmark]
    //public async Task HslCommunication()
    //{
    //    List<Task> tasks = new List<Task>();
    //    foreach (var siemensS7Net in siemensS7Nets)
    //    {
    //        for (int i = 0; i < Program.TaskNumberOfItems; i++)
    //        {
    //            tasks.Add(Task.Run(async () =>
    //            {
    //                for (int i = 0; i < Program.NumberOfItems; i++)
    //                {
    //                    var result = await siemensS7Net.ReadAsync("M0", 100);
    //                    if (!result.IsSuccess)
    //                    {
    //                        throw new Exception(result.Message);
    //                    }
    //                }
    //            }));
    //        }
    //    }
    //    await Task.WhenAll(tasks);
    //}






    [Benchmark]
    public async Task ThingsGateway()
    {
        SiemensS7Address[] siemensS7Address = [SiemensS7Address.ParseFrom("M1", 100)];
        List<Task> tasks = new List<Task>();
        foreach (var siemensS7 in siemensS7s)
        {
            for (int i = 0; i < Program.TaskNumberOfItems; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    for (int i = 0; i < Program.NumberOfItems; i++)
                    {
                        var result = await siemensS7.S7ReadAsync(siemensS7Address);
                        if (!result.IsSuccess)
                        {
                            throw new Exception(result.ToString());
                        }
                    }
                }));
            }
        }
        await Task.WhenAll(tasks);
    }

    public void Dispose()
    {
        plcs.ForEach(a => a.SafeDispose());
        siemensS7Nets.ForEach(a => a.SafeDispose());
        siemensS7s.ForEach(a => a.Channel.SafeDispose());
        siemensS7s.ForEach(a => a.SafeDispose());
    }
}