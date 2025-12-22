//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ThingsGateway.Management.Application;

/// <summary>
/// 导出文件
/// </summary>
[ApiDescriptionSettings(false)]
[Route("api/managementExport")]
[IgnoreRolePermission]
[Authorize]
public class ManagementExportController : ControllerBase
{
    private readonly IImportExportService _importExportService;
    private readonly ManagementConfigService _managementConfigService;

    public ManagementExportController(
        ManagementConfigService managementConfigService,
        IImportExportService importExportService
        )
    {
        _managementConfigService = managementConfigService;
        _importExportService = importExportService;
    }


    /// <summary>
    /// 下载通道
    /// </summary>
    /// <returns></returns>
    [HttpPost("managementConfig")]
    public async Task<IActionResult> DownloadManagementConfigAsync([FromBody] ManagementExportFilter input)
    {
        input.QueryPageOptions.IsPage = false;
        input.QueryPageOptions.IsVirtualScroll = false;

        var sheets = await _managementConfigService.ExportManagementConfigAsync(input).ConfigureAwait(false);
        return await _importExportService.ExportAsync<ManagementConfig>(sheets, "ManagementConfig", false).ConfigureAwait(false);
    }

}
