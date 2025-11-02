//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Foundation.Modbus;
using ThingsGateway.NewLife.Json.Extension;

using TouchSocket.Sockets;

namespace ThingsGateway.Foundation.Demo;

#pragma warning disable CA1861 // 不要将常量数组作为参数

/// <summary>
/// ModbusMaster
/// </summary>
public class ModbusMasterDemo
{
    /// <summary>
    /// 新建链路
    /// </summary>
    /// <returns></returns>
    public IChannel GetChannel(ChannelOptions channelOptions)
    {
        TouchSocketConfig touchSocketConfig = new TouchSocketConfig();
        return touchSocketConfig.GetChannel(channelOptions);
    }

    /// <summary>
    /// 新建协议对象
    /// </summary>
    /// <param name="channel"></param>
    /// <returns></returns>
    public ModbusMaster GetDevice(IChannel channel)
    {
        var client = new ModbusMaster();
        client.InitChannel(channel);
        return client;
    }
    public async Task TestReadWrite()
    {

        //获取链路对象
        using var channel = GetChannel(new ChannelOptions()
        {
            ChannelType = ChannelTypeEnum.TcpClient,
            RemoteUrl = "127.0.0.1:502",
        });
        //配置其他属性，如日志等
        channel.Config.ConfigureContainer(a => a.AddConsoleLogger());

        //获取协议对象
        using var device = GetDevice(channel);
        await channel.SetupAsync(channel.Config);

        //读取具体类型数据
        var data = await device.ReadDoubleAsync("400001"); //通过字符串转化地址，读取保持寄存器地址0
        device.Logger?.Info($"读取到的数据：{data.ToJsonNetString()}");


        //读取原始字节数组

        var bytes = await device.ReadAsync("400001", 10); //通过字符串转化地址，读取保持寄存器地址0,10个寄存器
        device.Logger?.Info($"读取到的数据：{data.ToJsonNetString()}");

        bytes = await device.ModbusReadAsync(new ModbusAddress()
        {
            Station = 1,
            StartAddress = 0,
            FunctionCode = 3,
            Length = 10,
        }); //配置地址对象，读取保持寄存器地址0,10个寄存器

        if (bytes.IsSuccess)
        {
            //解析bytes字节数组
            var byteData = bytes.Content.Span;
            var data1 = device.ThingsGatewayBitConverter.ToDouble(byteData, 0);
            var data2 = device.ThingsGatewayBitConverter.ToDouble(byteData, 8);
            var data3 = device.ThingsGatewayBitConverter.ToUInt16(byteData, 16);
            device.Logger?.Info($"读取到的数据：{data1},{data2},{data3}");
        }


        //写入数据
        var write = await device.WriteAsync("400001", (double)123.456); //通过字符串转化地址，写入保持寄存器地址0
        device.Logger?.Info($"写入结果：{write.ToJsonNetString()}");
        write = await device.WriteAsync("400001", new double[] { 123.456, 123.456 }); //通过字符串转化地址，写入保持寄存器地址2,2个double寄存器
        device.Logger?.Info($"写入结果：{write.ToJsonNetString()}");

        write = await device.ModbusRequestAsync(new ModbusAddress()
        {
            Station = 1,
            StartAddress = 0,
            FunctionCode = 3,
            MasterWriteDatas = device.ThingsGatewayBitConverter.GetBytes(new double[] { 123.456, 123.456 })
        }, false); //通过字符串转化地址，写入保持寄存器地址2,2个double寄存器

        device.Logger?.Info($"写入结果：{write.ToJsonNetString()}");

    }
    public async Task TestMulRead()
    {

        //获取链路对象
        using var channel = GetChannel(new ChannelOptions()
        {
            ChannelType = ChannelTypeEnum.TcpClient,
            RemoteUrl = "127.0.0.1:502",
        });
        //配置其他属性，如日志等
        channel.Config.ConfigureContainer(a => a.AddConsoleLogger());

        //获取协议对象
        using var device = GetDevice(channel);


        //批量打包
        var variableRuntimes = new List<VariableClass>()
            {
                new VariableClass()
                {
                    DataType=DataTypeEnum.Double,
                    RegisterAddress="40001",
                    IntervalTime="1000",
                },
                new VariableClass()
                {
                    DataType=DataTypeEnum.UInt16,
                    RegisterAddress="40009",
                    IntervalTime="1000",
                },
                new VariableClass()
                {
                    DataType=DataTypeEnum.Double,
                    RegisterAddress="40005",
                    IntervalTime="1000",
                },

            };

        var deviceVariableSourceReads = device.LoadSourceRead<VariableSourceClass>(variableRuntimes, 125, "1000");
        foreach (var item in deviceVariableSourceReads)
        {
            var result = await device.ReadAsync(item.AddressObject);
            if (result.IsSuccess)
            {
                try
                {
                    var result1 = item.VariableRuntimes.PraseStructContent(device, result.Content.Span, exWhenAny: true);
                    if (!result1.IsSuccess)
                    {
                        item.LastErrorMessage = result1.ErrorMessage;
                        var time = DateTime.Now;
                        item.VariableRuntimes.ForEach(a => a.SetValue(null, time, isOnline: false));
                        device.Logger?.Warning(result1.ToString());
                    }
                }
                catch (Exception ex)
                {
                    device.Logger?.LogWarning(ex);
                }
            }
            else
            {
                item.LastErrorMessage = result.ErrorMessage;
                var time = DateTime.Now;
                item.VariableRuntimes.ForEach(a => a.SetValue(null, time, isOnline: false));
                device.Logger?.Warning(result.ToString());
            }
        }

        device.Logger?.Info($"批量读取到的数据：{variableRuntimes.Select(a => new { a.RegisterAddress, a.Value }).ToJsonNetString()}");

    }

    public async Task TestVariableObject()
    {

        //获取链路对象
        using var channel = GetChannel(new ChannelOptions()
        {
            ChannelType = ChannelTypeEnum.TcpClient,
            RemoteUrl = "127.0.0.1:502",
        });
        //配置其他属性，如日志等
        channel.Config.ConfigureContainer(a => a.AddConsoleLogger());

        //获取协议对象
        using var device = GetDevice(channel);


        //使用变量对象读取
        var testModbusObject = new TestModbusObject(device, 125);
        await testModbusObject.MultiReadAsync();
        device.Logger?.Info($"批量读取到的数据：{testModbusObject.ToJsonNetString()}");

        //源生成的写入方法
        var write = await testModbusObject.WriteDouble1Async(123.456);
        device.Logger?.Info($"写入结果：{write.ToJsonNetString()}");
    }
}
[GeneratorVariable]
public partial class TestModbusObject : VariableObject
{
    [VariableRuntime(RegisterAddress = "400001")]
    public double Double1 { get; set; }
    [VariableRuntime(RegisterAddress = "400005")]
    public double Double2 { get; set; }

    [VariableRuntime(RegisterAddress = "400009")]
    public ushort UShort3 { get; set; }
    [VariableRuntime(RegisterAddress = "4000010")]
    public ushort UShort4 { get; set; }

    public TestModbusObject(IDevice device, int maxPack) : base(device, maxPack)
    {
    }

}