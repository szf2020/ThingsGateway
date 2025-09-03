//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------



using ThingsGateway.NewLife;

namespace ThingsGateway.Foundation;

public class TextFileReadService : ITextFileReadService
{
    public Task<OperResult<List<string>>> GetLogFilesAsync(string directoryPath) => Task.FromResult(TextFileReader.GetLogFiles(directoryPath));

    public Task<OperResult<List<LogData>>> LastLogDataAsync(string file, int lineCount = 200) => Task.FromResult(TextFileReader.LastLogData(file, lineCount));


    public async Task DeleteLogDataAsync(string path)
    {
        if (path != null)
        {
            var files = TextFileReader.GetLogFiles(path);
            if (files.IsSuccess)
            {
                foreach (var item in files.Content)
                {
                    if (File.Exists(item))
                    {
                        int error = 0;
                        while (error < 3)
                        {
                            try
                            {
                                FileUtil.DeleteFile(item);
                                break;
                            }
                            catch
                            {
                                await Task.Delay(3000).ConfigureAwait(false);
                                error++;
                            }
                        }
                    }
                }
            }

        }
    }
}
