// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using System.Net;

using TouchSocket.Core;

namespace ThingsGateway.Foundation.Test;

public class UdpDataHandlingAdapterTest
{
    public async Task SendCallback(UdpDataHandlingAdapter adapter, ReadOnlyMemory<byte> memory, CancellationToken token = default)
    {
        try
        {
            await ReceivedAsync(IPEndPoint.Parse("127.0.0.1:502"), adapter, memory, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
        }
    }
    public async Task ReceivedAsync(IPEndPoint iPEndPoint, UdpDataHandlingAdapter adapter, ReadOnlyMemory<byte> memory, CancellationToken token)
    {
        try
        {
            await adapter.ReceivedInputAsync(iPEndPoint, memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {

        }
    }
}
