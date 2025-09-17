// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using ThingsGateway.Foundation.Extension.String;
using ThingsGateway.Foundation.SiemensS7;

using TouchSocket.Core;

using Xunit.Abstractions;

namespace ThingsGateway.Foundation.Test;

public class SiemensS7Test
{

    public SiemensS7Test(ITestOutputHelper output)
    {
        _output = output;

    }
    private readonly ITestOutputHelper _output;


    [Theory]
    [InlineData("M100", true, "03 00 00 1B 02 F0 80 32 03 00 00 00 02 00 02 00 06 00 00 04 01 FF 04 00 10 00 00")]
    [InlineData("M100", false, "03 00 00 16 02 F0 80 32 03 00 00 00 02 00 02 00 01 00 00 05 01 FF", "1", DataTypeEnum.UInt16)]
    public async Task SiemensS7_ReadWrite_OK(string address, bool read, string data, string writeData = null, DataTypeEnum dataTypeEnum = DataTypeEnum.UInt16)
    {
        var siemensS7Channel = new TouchSocketConfig().GetChannel(new ChannelOptions()
        {
            ChannelType = ChannelTypeEnum.Other
        }) as IClientChannel;
        siemensS7Channel.Config.ConfigureContainer(a =>
        {
            a.AddEasyLogger((a, b, c, d) =>
            {
                _output.WriteLine($"{c}{Environment.NewLine}{d?.ToString()}");
            }, LogLevel.Trace);
        });

        var siemensS7Master = new SiemensS7Master() { SiemensS7Type = SiemensTypeEnum.S1200, Timeout = 10000 };
        siemensS7Master.InitChannel(siemensS7Channel);
        await siemensS7Channel.SetupAsync(siemensS7Channel.Config);
        await siemensS7Master.ConnectAsync(CancellationToken.None);
        var adapter = siemensS7Channel.ReadOnlyDataHandlingAdapter as SingleStreamDataHandlingAdapter;
        await siemensS7Master.ConnectAsync(CancellationToken.None);
        var task1 = Task.Run(async () =>
        {
            if (read)
            {
                var result = await siemensS7Master.ReadAsync(address, default).ConfigureAwait(false);
                Assert.True(result.IsSuccess, result.ToString());
            }
            else
            {
                var result = await siemensS7Master.WriteJTokenAsync(address, JTokenUtil.GetJTokenFromString(writeData), dataTypeEnum).ConfigureAwait(false);
                Assert.True(result.IsSuccess, result.ToString());
            }
        });
        await Task.Delay(500);

        var task2 = Task.Run(async () =>
        {
            SingleStreamDataHandlingAdapterTest singleStreamDataHandlingAdapterTest = new();
            await singleStreamDataHandlingAdapterTest.SendCallback(data.HexStringToBytes(), (a) => singleStreamDataHandlingAdapterTest.ReceivedAsync(adapter, CancellationToken.None), 1, CancellationToken.None).ConfigureAwait(false);
        });
        await Task.WhenAll(task1, task2);
    }


}
