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

namespace ThingsGateway.Management;

internal sealed class RedundancyHostedService : BackgroundService, IRedundancyHostedService
{
    private readonly ILogger _logger;
    /// <inheritdoc cref="RedundancyHostedService"/>
    public RedundancyHostedService(ILogger<RedundancyHostedService> logger)
    {
        _logger = logger;
        RedundancyTask = new RedundancyTask(_logger);
    }
    private RedundancyTask RedundancyTask;

    public TextFileLogger TextLogger => RedundancyTask.TextLogger;

    public string LogPath => RedundancyTask.LogPath;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        await RedundancyTask.StartTaskAsync(stoppingToken).ConfigureAwait(false);
    }
    public Task StartTaskAsync(CancellationToken cancellationToken) => RedundancyTask.StartTaskAsync(cancellationToken);
    public Task StopTaskAsync() => RedundancyTask.StopTaskAsync();

    public Task ForcedSync(CancellationToken cancellationToken = default) => RedundancyTask.ForcedSync(cancellationToken);


    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await RedundancyTask.DisposeAsync().ConfigureAwait(false);
        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }

}
