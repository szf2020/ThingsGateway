//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.JSInterop;

using ThingsGateway.Extension;
using ThingsGateway.Foundation;

namespace ThingsGateway.Debug;

public class PlatformService(IDownloadPlatformService downloadPlatformService) : IPlatformService
{
    public Task OnLogExport(string logPath)
    {
        var files = TextFileReader.GetLogFiles(logPath);
        if (!files.IsSuccess)
        {
            return Task.CompletedTask; ;
        }
        return downloadPlatformService.DownloadFile(files.Content);
    }
}
public class DownloadPlatformService(IJSRuntime JSRuntime) : IDownloadPlatformService
{
    public async Task DownloadFile(IEnumerable<string> path)
    {
        string url = "api/file/download";
        //统一web下载
        foreach (var item in path)
        {
            await using var jSObject = await JSRuntime.InvokeAsync<IJSObjectReference>("import", $"{WebsiteConst.DefaultResourceUrl}js/downloadFile.js");
            var path1 = Path.GetRelativePath("wwwroot", item);
            string fileName = DateTime.Now.ToFileDateTimeFormat();
            await jSObject.InvokeAsync<bool>("blazor_downloadFile", url, fileName, new { FileName = path1 });
        }
    }

}

