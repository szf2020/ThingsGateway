//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

#pragma warning disable CA2007 // 考虑对等待的任务调用 ConfigureAwait
using Microsoft.JSInterop;

using TouchSocket.Core;

namespace ThingsGateway.Management.Razor;

internal sealed class ManagementExportService : IManagementExportService
{
    public ManagementExportService(IJSRuntime jSRuntime)
    {
        JSRuntime = jSRuntime;
    }

    private IJSRuntime JSRuntime { get; set; }

    public async Task<bool> OnManagementConfigExport(ManagementExportFilter exportFilter)
    {
        try
        {
            await using var ajaxJS = await JSRuntime.InvokeAsync<IJSObjectReference>("import", $"/_content/ThingsGateway.Razor/js/downloadFile.js");
            string url = "api/managementExport/managementConfig";
            string fileName = $"{DateTime.Now.ToFileDateTimeFormat()}.xlsx";
            return await ajaxJS.InvokeAsync<bool>("postJson_downloadFile", url, fileName, exportFilter.ToJsonString());
        }
        catch
        {
            return false;
        }
    }

}
