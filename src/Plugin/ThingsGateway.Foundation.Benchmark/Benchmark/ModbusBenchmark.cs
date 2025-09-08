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

using HslCommunication.ModBus;

using System.Net.Sockets;

using ThingsGateway.Foundation.Modbus;

using TouchSocket.Core;
using TouchSocket.Modbus;

using IModbusMaster = NModbus.IModbusMaster;
using ModbusMaster = ThingsGateway.Foundation.Modbus.ModbusMaster;

namespace ThingsGateway.Foundation;

[MemoryDiagnoser]
public class ModbusBenchmark : IDisposable
{
    private List<ModbusMaster> thingsgatewaymodbuss = new();
    private List<IModbusMaster> nmodbuss = new();
    private List<ModbusTcpNet> modbusTcpNets = new();
    private List<ModbusTcpMaster> modbusTcpMasters = new();

    public ModbusBenchmark()
    {
        for (int i = 0; i < Program.ClientCount; i++)
        {

            var clientConfig = new TouchSocket.Core.TouchSocketConfig();
            var clientChannel = clientConfig.GetTcpClientWithIPHost(new ChannelOptions() { RemoteUrl = "127.0.0.1:502", MaxConcurrentCount = 10 });
            var thingsgatewaymodbus = new ModbusMaster()
            {
                //modbus协议格式
                ModbusType = ModbusTypeEnum.ModbusTcp,
            };
            thingsgatewaymodbus.InitChannel(clientChannel);
            clientChannel.SetupAsync(clientChannel.Config).GetFalseAwaitResult();
            clientChannel.Logger.LogLevel = LogLevel.Warning;
            thingsgatewaymodbus.ConnectAsync(CancellationToken.None).GetFalseAwaitResult();
            thingsgatewaymodbus.ReadAsync("40001", 100).GetAwaiter().GetResult();
            thingsgatewaymodbuss.Add(thingsgatewaymodbus);
        }


        for (int i = 0; i < Program.ClientCount; i++)
        {

            var factory = new NModbus.ModbusFactory();
            var nmodbus = factory.CreateMaster(new TcpClient("127.0.0.1", 502));
            nmodbus.ReadHoldingRegistersAsync(1, 0, 100).GetFalseAwaitResult();
            nmodbuss.Add(nmodbus);
        }
        for (int i = 0; i < Program.ClientCount; i++)
        {
            ModbusTcpNet modbusTcpNet = new();
            modbusTcpNet.IpAddress = "127.0.0.1";
            modbusTcpNet.Port = 502;
            modbusTcpNet.ConnectServer();
            modbusTcpNet.ReadAsync("0", 100).GetFalseAwaitResult();
            modbusTcpNets.Add(modbusTcpNet);
        }

        for (int i = 0; i < Program.ClientCount; i++)
        {
            var client = new ModbusTcpMaster();
            client.SetupAsync(new TouchSocketConfig()
    .SetRemoteIPHost("127.0.0.1:502")).GetFalseAwaitResult();
            client.ConnectAsync(CancellationToken.None).GetFalseAwaitResult();
            client.ReadHoldingRegistersAsync(0, 100).GetFalseAwaitResult();
            modbusTcpMasters.Add(client);
        }
    }

    [Benchmark]
    public async Task ThingsGateway()
    {
        ModbusAddress addr = new ModbusAddress() { FunctionCode = 3, StartAddress = 0, Length = 100 };
        List<Task> tasks = new List<Task>();
        foreach (var thingsgatewaymodbus in thingsgatewaymodbuss)
        {

            for (int i = 0; i < Program.TaskNumberOfItems; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    for (int i = 0; i < Program.NumberOfItems; i++)
                    {
                        var result = await thingsgatewaymodbus.ModbusReadAsync(addr);
                        if (!result.IsSuccess)
                        {
                            throw new Exception(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff") + result.ToString());
                        }
                    }
                }));
            }
        }
        await Task.WhenAll(tasks);
    }



    [Benchmark]
    public async Task TouchSocket()
    {
        List<Task> tasks = new List<Task>();
        foreach (var modbusTcpMaster in modbusTcpMasters)
        {
            for (int i = 0; i < Program.TaskNumberOfItems; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    for (int i = 0; i < Program.NumberOfItems; i++)
                    {
                        var result = await modbusTcpMaster.ReadHoldingRegistersAsync(0, 100);
                        if (!result.IsSuccess)
                        {
                            throw new Exception(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff") + result.ToString());
                        }
                    }
                }));
            }
        }
        await Task.WhenAll(tasks);
    }


    [Benchmark]
    public async Task NModbus4()
    {
        List<Task> tasks = new List<Task>();
        foreach (var nmodbus in nmodbuss)
        {
            for (int i = 0; i < Program.TaskNumberOfItems; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    for (int i = 0; i < Program.NumberOfItems; i++)
                    {
                        var result = await nmodbus.ReadHoldingRegistersAsync(1, 0, 100);
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
    //    foreach (var modbusTcpNet in modbusTcpNets)
    //    {
    //        for (int i = 0; i < Program.TaskNumberOfItems; i++)
    //        {
    //            tasks.Add(Task.Run(async () =>
    //            {
    //                for (int i = 0; i < Program.NumberOfItems; i++)
    //                {
    //                    var result = await modbusTcpNet.ReadAsync("0", 100);
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

    public void Dispose()
    {
        thingsgatewaymodbuss?.ForEach(a => a.Channel.SafeDispose());
        thingsgatewaymodbuss?.ForEach(a => a.SafeDispose());
        nmodbuss?.ForEach(a => a.SafeDispose());
        modbusTcpNets?.ForEach(a => a.SafeDispose());
    }
}