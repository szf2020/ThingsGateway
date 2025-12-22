//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Debug;

public class ManagementPlatformService(ITextFileReadService textFileReadService, DmtpActorContext dmtpActorContext, IDownloadPlatformService downloadPlatformService) : IPlatformService
{
    public async Task OnLogExport(string logPath)
    {
        var files = await textFileReadService.GetLogFilesAsync(logPath);
        if (!files.IsSuccess)
        {
            return;
        }
        List<string> savePaths = new();
        string dir = PathHelper.CombinePathReplace("wwwroot", dmtpActorContext.Current.Id.SanitizeFileName(), logPath);
        foreach (var item in files.Content)
        {
            var savePath = PathHelper.CombinePathReplace("wwwroot", dmtpActorContext.Current.Id.SanitizeFileName(), logPath, Path.GetFileName(item));
            savePaths.Add(savePath);
            await FileServerHelpers.ClientPullFileFromService(dmtpActorContext.Current, item, savePath);
        }
        if (downloadPlatformService is HybridDownloadPlatformService)
        {
            await downloadPlatformService.DownloadFile([dir]);
        }
        else
        {
            await downloadPlatformService.DownloadFile(savePaths);
        }

    }
}
