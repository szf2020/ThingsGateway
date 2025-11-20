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

/// <summary>
/// 设备采集报警后台服务
/// </summary>
internal sealed class AlarmHostedService : BackgroundService, IAlarmHostedService
{
    private readonly ILogger _logger;
    /// <inheritdoc cref="AlarmHostedService"/>
    public AlarmHostedService(ILogger<AlarmHostedService> logger)
    {
        _logger = logger;
        AlarmTask = new AlarmTask(_logger);
    }
    private AlarmTask AlarmTask;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        AlarmTask.StartTask(stoppingToken);
    }
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        AlarmTask.Dispose();
        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }
    public void ConfirmAlarm(long variableId)
    {
        AlarmTask?.ConfirmAlarm(variableId);
    }
}
