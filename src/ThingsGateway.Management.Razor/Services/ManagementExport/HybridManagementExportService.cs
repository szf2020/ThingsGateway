//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Admin.Application;

namespace ThingsGateway.Management.Razor;

public sealed class HybridManagementExportService : IManagementExportService
{

    private readonly IImportExportService _importExportService;
    private readonly ManagementConfigService _managementConfigService;

    public HybridManagementExportService(
        ManagementConfigService managementConfigService,
        IImportExportService importExportService
        )
    {
        _managementConfigService = managementConfigService;
        _importExportService = importExportService;
    }

    public async Task<bool> OnManagementConfigExport(ManagementExportFilter exportFilter)
    {
        try
        {
            exportFilter.QueryPageOptions.IsPage = false;
            exportFilter.QueryPageOptions.IsVirtualScroll = false;

            var sheets = await _managementConfigService.ExportManagementConfigAsync(exportFilter).ConfigureAwait(false);
            var path = await _importExportService.CreateFileAsync<ManagementConfig>(sheets, "ManagementConfig", false).ConfigureAwait(false);

            Open(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool Open(string path)
    {
        path = System.IO.Path.GetDirectoryName(path); // Ensure the path is absolute

        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            System.Diagnostics.Process.Start("explorer.exe", path);
        }
        else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
        {
            System.Diagnostics.Process.Start("xdg-open", path);
        }
        else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
        {
            System.Diagnostics.Process.Start("open", path);
        }

        return true;
    }


}
