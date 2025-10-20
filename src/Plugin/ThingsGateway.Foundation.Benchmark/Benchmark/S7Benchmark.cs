//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

using S7.Net;

using ThingsGateway.Foundation.SiemensS7;

using TouchSocket.Core;

namespace ThingsGateway.Foundation;

//[SimpleJob(RuntimeMoniker.Net80)]
//[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class S7Benchmark : IDisposable
{
    public static int ClientCount = 5;
    public static int TaskNumberOfItems = 1;
    public static int NumberOfItems = 5;

    private List<SiemensS7Master> siemensS7s = new();

    private List<Plc> plcs = new();

    [GlobalSetup]
    public async Task Init()

    {
        {
            for (int i = 0; i < ClientCount; i++)
            {
                var clientConfig = new TouchSocket.Core.TouchSocketConfig();

                var clientChannel = clientConfig.GetChannel(new ChannelOptions() { ChannelType = ChannelTypeEnum.TcpClient, RemoteUrl = "127.0.0.1:102" });
                var siemensS7 = new SiemensS7Master()
                {
                    //modbus协议格式
                    SiemensS7Type = SiemensTypeEnum.S1500
                };
                siemensS7.InitChannel(clientChannel);
                await clientChannel.SetupAsync(clientChannel.Config);
                clientChannel.Logger.LogLevel = LogLevel.Warning;
                await siemensS7.ConnectAsync(CancellationToken.None);
                await siemensS7.ReadAsync("M1", 100);
                siemensS7s.Add(siemensS7);
            }

            for (int i = 0; i < ClientCount; i++)
            {
                var plc = new Plc(CpuType.S71500, "127.0.0.1", 102, 0, 0);
                await plc.OpenAsync();//打开plc连接
                await plc.ReadAsync(DataType.Memory, 1, 0, VarType.Byte, 100);
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
            for (int i = 0; i < TaskNumberOfItems; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    for (int i = 0; i < NumberOfItems; i++)
                    {
                        var result = await plc.ReadAsync(DataType.Memory, 1, 0, VarType.Byte, 100);
                    }
                }));
            }
        }
        await Task.WhenAll(tasks);
    }


    [Benchmark]
    public async Task ThingsGateway()
    {
        SiemensS7Address[] siemensS7Address = [SiemensS7Address.ParseFrom("M1", 100)];
        List<Task> tasks = new List<Task>();
        foreach (var siemensS7 in siemensS7s)
        {
            for (int i = 0; i < TaskNumberOfItems; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    for (int i = 0; i < NumberOfItems; i++)
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
        siemensS7s.ForEach(a => a.Channel.SafeDispose());
        siemensS7s.ForEach(a => a.SafeDispose());
    }
}