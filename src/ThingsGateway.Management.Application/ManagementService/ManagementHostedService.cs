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

namespace ThingsGateway.Management.Application;

internal sealed class ManagementHostedService : BackgroundService
{
    public ILogger Logger { get; }
    public ManagementHostedService(ILogger<ManagementHostedService> logger)
    {
        Logger = logger;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        try
        {
            var managementConfigRuntimes = (await ManagementGlobalData.ManagementConfigService.GetFromDBAsync().ConfigureAwait(false));

            foreach (var managementConfigRuntime in managementConfigRuntimes)
            {
                try
                {
                    await managementConfigRuntime.InitAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Init");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Start error");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var item in ManagementGlobalData.ManagementConfigs)
        {
            await item.Value.DisposeAsync().ConfigureAwait(false);
        }
        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }
}
