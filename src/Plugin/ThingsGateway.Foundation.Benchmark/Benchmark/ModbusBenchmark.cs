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
using BenchmarkDotNet.Jobs;

using Longbow.Modbus;
using Longbow.TcpSocket;

using Microsoft.Extensions.DependencyInjection;

using System.Net.Sockets;

using ThingsGateway.Foundation.Modbus;

using TouchSocket.Core;
using TouchSocket.Modbus;

using IModbusMaster = NModbus.IModbusMaster;
using ModbusMaster = ThingsGateway.Foundation.Modbus.ModbusMaster;

namespace ThingsGateway.Foundation;

[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class ModbusBenchmark : IDisposable
{
    private readonly List<IModbusClient> _lgbModbusClients = [];
    private List<ModbusMaster> thingsgatewaymodbuss = new();
    private List<IModbusMaster> nmodbuss = new();
    //private List<ModbusTcpNet> modbusTcpNets = new();
    private List<ModbusTcpMaster> modbusTcpMasters = new();

    [GlobalSetup]
    public async Task Init()
    {
        for (int i = 0; i < Program.ClientCount; i++)
        {

            var clientConfig = new TouchSocket.Core.TouchSocketConfig();

            var clientChannel = clientConfig.GetChannel(new ChannelOptions() { ChannelType = ChannelTypeEnum.TcpClient, RemoteUrl = "127.0.0.1:502", MaxConcurrentCount = 10 });
            var thingsgatewaymodbus = new ModbusMaster()
            {
                //modbus协议格式
                ModbusType = ModbusTypeEnum.ModbusTcp,
            };
            thingsgatewaymodbus.InitChannel(clientChannel);
         await   clientChannel.SetupAsync(clientChannel.Config);
            clientChannel.Logger.LogLevel = LogLevel.Warning;
         await   thingsgatewaymodbus.ConnectAsync(CancellationToken.None);
        await    thingsgatewaymodbus.ReadAsync("40001", 100);
            thingsgatewaymodbuss.Add(thingsgatewaymodbus);
        }


        for (int i = 0; i < Program.ClientCount; i++)
        {

            var factory = new NModbus.ModbusFactory();
            var nmodbus = factory.CreateMaster(new TcpClient("127.0.0.1", 502));
       await     nmodbus.ReadHoldingRegistersAsync(1, 0, 100);
            nmodbuss.Add(nmodbus);
        }
        //for (int i = 0; i < Program.ClientCount; i++)
        //{
        //    ModbusTcpNet modbusTcpNet = new();
        //    modbusTcpNet.IpAddress = "127.0.0.1";
        //    modbusTcpNet.Port = 502;
        //    modbusTcpNet.ConnectServer();
        //    modbusTcpNet.ReadAsync("0", 100);
        //    modbusTcpNets.Add(modbusTcpNet);
        //}

        for (int i = 0; i < Program.ClientCount; i++)
        {
            var client = new ModbusTcpMaster();
      await      client.SetupAsync(new TouchSocketConfig()
    .SetRemoteIPHost("127.0.0.1:502"));
        await    client.ConnectAsync(CancellationToken.None);
      await      client.ReadHoldingRegistersAsync(0, 100);
            modbusTcpMasters.Add(client);
        }

        {
            var sc = new ServiceCollection();
            sc.AddTcpSocketFactory();
            sc.AddModbusFactory();

            var provider = sc.BuildServiceProvider();
            var factory = provider.GetRequiredService<IModbusFactory>();

            for (int i = 0; i < Program.ClientCount; i++)
            {
                var client = factory.GetOrCreateTcpMaster();
            await    client.ConnectAsync("127.0.0.1", 502);
             await   client.ReadHoldingRegistersAsync(0x01, 0x00, 10);

                _lgbModbusClients.Add(client);
            }
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
                        var result = await thingsgatewaymodbus.ModbusReadAsync(addr).ConfigureAwait(false);
                        if (!result.IsSuccess)
                        {
                            throw new Exception(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff") + result.ToString());
                        }
                        var data = TouchSocketBitConverter.ConvertValues<byte, ushort>(result.Content.Span, EndianType.Little);
                    }
                }));
            }
        }
        await Task.WhenAll(tasks);
    }

    [Benchmark]
    public async Task LongbowModbus()
    {
        List<Task> tasks = new List<Task>();
        foreach (var _lgbModbusClient in _lgbModbusClients)
        {

            for (int i = 0; i < Program.TaskNumberOfItems; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    for (int i = 0; i < Program.NumberOfItems; i++)
                    {
                        using var cts = new CancellationTokenSource(3000);
                        var task = await _lgbModbusClient.ReadHoldingRegistersAsync(1, 0, 100, cts.Token).ConfigureAwait(false);
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
                        var result = await modbusTcpMaster.ReadHoldingRegistersAsync(0, 100).ConfigureAwait(false);
                        var data = TouchSocketBitConverter.ConvertValues<byte, ushort>(result.Data.Span, EndianType.Little);
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
                        var result = await nmodbus.ReadHoldingRegistersAsync(1, 0, 100).ConfigureAwait(false);
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
        //modbusTcpNets?.ForEach(a => a.SafeDispose());
        _lgbModbusClients?.ForEach(a => a.DisposeAsync());
    }

}