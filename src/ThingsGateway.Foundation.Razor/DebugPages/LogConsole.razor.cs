//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Components.Web;

using System.Text.RegularExpressions;
using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Common;
using ThingsGateway.Foundation.Common.Extension;

using TouchSocket.Core;

namespace ThingsGateway.Debug;

public partial class LogConsole : IDisposable
{

    private string PlayText { get; set; }
    private string PauseText { get; set; }
    private string ExportText { get; set; }
    private string DeleteText { get; set; }

    protected override void OnInitialized()
    {
        PlayText = RazorLocalizer["Play"];
        PauseText = RazorLocalizer["Pause"];
        ExportText = RazorLocalizer["Export"];
        DeleteText = RazorLocalizer["Delete"];

        _Timer = new TimerX(RunTimerAsync, null, 1_000, 1_000, nameof(LogConsole)) { Async = true };
        base.OnInitialized();
    }
    private TimerX _Timer;

    private bool Pause;

    public bool Disposed { get; set; }

    [Parameter, EditorRequired]
    public LogLevel LogLevel { get; set; }

    [Parameter]
    public EventCallback<LogLevel> LogLevelChanged { get; set; }

    [Parameter]
    public string HeaderText { get; set; } = "Log";

    [Parameter]
    public string HeightString { get; set; } = "calc(100% - 300px)";

    [Parameter, EditorRequired]
    public string LogPath { get; set; }

    /// <summary>
    /// 日志
    /// </summary>
    public ICollection<LogMessage> Messages { get; set; } = new List<LogMessage>();

    private ICollection<LogMessage> CurrentMessages => Pause ? PauseMessagesText : Messages;

    [Inject]
    private DownloadService DownloadService { get; set; }
    [Inject]
    private IStringLocalizer<ThingsGateway.Razor._Imports> RazorLocalizer { get; set; }

    /// <summary>
    /// 暂停缓存
    /// </summary>
    private ICollection<LogMessage> PauseMessagesText { get; set; } = new List<LogMessage>();

    [Inject]
    private IPlatformService PlatformService { get; set; }

    private string logPath;
    protected override async Task OnParametersSetAsync()
    {
        if (LogPath != logPath)
        {
            logPath = LogPath;
            Messages = new List<LogMessage>();
            _Timer?.SetNext(0);
        }

        await base.OnParametersSetAsync();
    }

    [Inject]
    private ToastService ToastService { get; set; }
    [Inject]
    ITextFileReadService TextFileReadService { get; set; }
    public void Dispose()
    {
        Disposed = true;
        _Timer?.SafeDispose();
        GC.SuppressFinalize(this);
    }
    protected async ValueTask ExecuteAsync()
    {
        try
        {

            if (LogPath != null)
            {
                var files = await TextFileReadService.GetLogFilesAsync(LogPath);
                if (!files.IsSuccess)
                {
                    Messages = new List<LogMessage>();
                    await Task.Delay(1000);
                }
                else
                {
                    var sw = ValueStopwatch.StartNew();
                    var result = await TextFileReadService.LastLogDataAsync(files.Content.FirstOrDefault());
                    if (result.IsSuccess)
                    {
                        Messages = result.Content.Where(a => a.LogLevel >= LogLevel).Select(a => new LogMessage((int)a.LogLevel, $"{a.LogTime} - {a.Message}{(a.ExceptionString.IsNullOrWhiteSpace() ? null : $"{Environment.NewLine}{a.ExceptionString}")}")).ToList();
                    }
                    else
                    {
                        Messages = Array.Empty<LogMessage>();
                    }
                    if (sw.GetElapsedTime().TotalMilliseconds > 500)
                    {
                        await Task.Delay(1000);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Foundation.Common.Log.XTrace.WriteException(ex);
        }

    }

    private async Task Delete()
    {
        await TextFileReadService.DeleteLogDataAsync(LogPath);
    }

    private async Task HandleOnExportClick(MouseEventArgs args)
    {
        try
        {
            if (Pause)
            {
                using var memoryStream = new MemoryStream();
                using StreamWriter writer = new(memoryStream);
                foreach (var item in PauseMessagesText)
                {
                    await writer.WriteLineAsync(item.Message);
                }
                await writer.FlushAsync();
                memoryStream.Seek(0, SeekOrigin.Begin);

                // 定义文件名称规则的正则表达式模式
                string pattern = @"[\\/:*?""<>|]";
                // 使用正则表达式将不符合规则的部分替换为下划线
                string sanitizedFileName = Regex.Replace(HeaderText, pattern, "_");
                await DownloadService.DownloadFromStreamAsync($"{sanitizedFileName}{DateTime.Now.ToFileDateTimeFormat()}.txt", memoryStream);
            }
            else
            {
                if (PlatformService != null)
                    await PlatformService.OnLogExport(LogPath);
            }
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
        }
    }
    private Task OnPause()
    {
        Pause = !Pause;
        if (Pause)
            PauseMessagesText = Messages.ToList();
        return Task.CompletedTask;
    }

    private async Task RunTimerAsync(object? state)
    {
        await ExecuteAsync();
        await InvokeAsync(StateHasChanged);
    }
}
