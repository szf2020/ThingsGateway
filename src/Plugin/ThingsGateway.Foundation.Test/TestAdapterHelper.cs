// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using ThingsGateway.Foundation;

using TouchSocket.Core;

internal static class TestAdapterHelper
{

    public static async Task ReceivedInputAsync(SingleStreamDataHandlingAdapter adapter, ReadOnlyMemory<byte> memory, CancellationToken token)
    {
        var offset = 0;
        while (true)
        {
            token.ThrowIfCancellationRequested();

            var remainingLength = memory.Length - offset;
            if (remainingLength <= 0)
            {
                break;
            }
            var sliceMemory = memory.Slice(offset, Math.Min(remainingLength, 1));
            var reader = new ClassBytesReader(sliceMemory);
            await adapter.ReceivedInputAsync(reader).ConfigureAwait(false);
            offset += sliceMemory.Length;
        }

    }

    public static async Task TestAdapter<T>(byte[] data, bool isSingleStreamData) where T : MessageBase, new()
    {
        bool isSuccess = false;
        MessageBase message = default;

        if (isSingleStreamData)
        {
            for (int bufferLength = 1; bufferLength < 256; bufferLength += 1)
            {
                SingleStreamDataAdapterTester tester = SingleStreamDataAdapterTester.CreateTester(new DeviceSingleStreamDataHandleAdapter<T>()
                    , (byteBlock, requestInfo) =>
                    {
                        //此处就是接收，如果是自定义适配器，可以将requestInfo强制转换为实际对象，然后判断数据的确定性
                        //if (byteBlock.Length != 15 || (!byteBlock.ToArray().SequenceEqual(data)))
                        if (((requestInfo is MessageBase messageBase)))
                        {
                            if (!messageBase.IsSuccess)
                            {
                                message = messageBase;
                                isSuccess = false;
                            }
                            else
                            {
                                isSuccess = true;
                            }
                        }
                        else
                        {
                            isSuccess = false;
                        }
                        return Task.CompletedTask;
                    });

                try
                {
                    using var cts = new CancellationTokenSource(1000 * 10);
                    var time = await tester.RunAsync(data, 2, 2, bufferLength, cts.Token).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Xunit.Assert.Fail($"{ex.Message} bufferLength: {bufferLength}");
                }
            }
        }
        else
        {
            var tester = UdpDataAdapterTester.CreateTester(new DeviceUdpDataHandleAdapter<T>(), 10, (byteBlock, requestInfo) =>
         {
             if (((requestInfo is MessageBase messageBase)))
             {
                 if (!messageBase.IsSuccess)
                 {
                     message = messageBase;
                     isSuccess = false;
                 }
                 else
                 {
                     isSuccess = true;
                 }
             }
             return Task.CompletedTask;
         });
            try
            {
                var time = await tester.RunAsync(data, 2, 2, 1000 * 10).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Xunit.Assert.Fail($"{ex.Message}");
            }
        }

        Xunit.Assert.True(isSuccess, message?.ErrorMessage);
    }
}
