//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Components.Server.Circuits;

using ThingsGateway.Common;

namespace ThingsGateway.Server;
public class ConnectionLimiterCircuitHandler : CircuitHandler
{
    private int _currentConnectionCount = 0;
    private WebsiteOptions WebsiteOptions;


    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        WebsiteOptions ??= App.GetOptions<WebsiteOptions>();

        if (!WebsiteOptions.BlazorConnectionLimitEnable)
            return base.OnCircuitOpenedAsync(circuit, cancellationToken);


        if (Interlocked.Increment(ref _currentConnectionCount) > WebsiteOptions.MaxBlazorConnections)
        {
            // 已达上限，断开连接
            Interlocked.Decrement(ref _currentConnectionCount); // 回滚计数
            throw new InvalidOperationException("The server connection limit has been reached, please try again later.");
        }

        return base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        WebsiteOptions ??= App.GetOptions<WebsiteOptions>();

        if (!WebsiteOptions.BlazorConnectionLimitEnable)
            return base.OnCircuitClosedAsync(circuit, cancellationToken);

        Interlocked.Decrement(ref _currentConnectionCount);

        return base.OnCircuitClosedAsync(circuit, cancellationToken);
    }
}

