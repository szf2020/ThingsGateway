//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ThingsGateway.Gateway.Application;

internal sealed class ManagementHostedService : BackgroundService
{

    public ManagementHostedService(ILoggerFactory loggerFactory)
    {
        var clientManagementOptions = App.GetOptions<RemoteClientManagementOptions>();
        var serverManagementOptions = App.GetOptions<RemoteServerManagementOptions>();
        RemoteClientManagementTask = new ManagementTask(loggerFactory.CreateLogger(nameof(RemoteClientManagementTask)), clientManagementOptions);
        RemoteServerManagementTask = new ManagementTask(loggerFactory.CreateLogger(nameof(RemoteServerManagementTask)), serverManagementOptions);
    }

    private ManagementTask RemoteClientManagementTask;
    private ManagementTask RemoteServerManagementTask;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        await RemoteClientManagementTask.StartAsync(stoppingToken).ConfigureAwait(false);
        await RemoteServerManagementTask.StartAsync(stoppingToken).ConfigureAwait(false);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await RemoteClientManagementTask.DisposeAsync().ConfigureAwait(false);
        await RemoteServerManagementTask.DisposeAsync().ConfigureAwait(false);
        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }
}
