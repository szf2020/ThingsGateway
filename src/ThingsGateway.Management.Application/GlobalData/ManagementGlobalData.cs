//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Collections.Concurrent;

using TouchSocket.Core;

namespace ThingsGateway.Management.Application;


public static class ManagementGlobalData
{
    public static IReadOnlyDictionary<string, ManagementConfig> ReadOnlyManagementConfigs => ManagementConfigs;
    public static IReadOnlyDictionary<long, ManagementConfig> ReadOnlyIdManagementConfigs => IdManagementConfigs;

    internal static NonBlockingDictionary<string, ManagementConfig> ManagementConfigs = new();
    internal static NonBlockingDictionary<long, ManagementConfig> IdManagementConfigs = new();

    public static async Task<IEnumerable<ManagementConfig>> GetCurrentUserManagementConfigs()
    {
        var dataScope = await ManagementGlobalData.SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        return ReadOnlyIdManagementConfigs.WhereIF(dataScope != null && dataScope?.Count > 0, u => dataScope.Contains(u.Value.CreateOrgId))//在指定机构列表查询
          .WhereIF(dataScope?.Count == 0, u => u.Value.CreateUserId == UserManager.UserId).Select(a => a.Value).OrderBy(a => a.Id);
    }

    private static ILog logMessage;
    public static ILog LogMessage
    {
        get
        {
            if (logMessage == null)
            {
                logMessage = App.RootServices.GetRequiredService<ILog>();
            }
            return logMessage;
        }
    }
    #region 单例服务

    private static ManagementConfigService managementConfigService;
    public static ManagementConfigService ManagementConfigService
    {
        get
        {
            if (managementConfigService == null)
            {
                managementConfigService = App.RootServices.GetRequiredService<ManagementConfigService>();
            }
            return managementConfigService;
        }
    }


    private static ISysUserService sysUserService;
    public static ISysUserService SysUserService
    {
        get
        {
            if (sysUserService == null)
            {
                sysUserService = App.RootServices.GetRequiredService<ISysUserService>();
            }
            return sysUserService;
        }
    }

    private static IHardwareJob? hardwareJob;

    public static IHardwareJob HardwareJob
    {
        get
        {
            hardwareJob ??= App.RootServices.GetRequiredService<IHardwareJob>();
            return hardwareJob;
        }
    }

    #endregion


}
