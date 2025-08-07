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
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Razor;

public partial class RedundancyOptionsPage
{
    [Inject]
    [NotNull]
    private IStringLocalizer<RedundancyOptions>? RedundancyLocalizer { get; set; }

    [Parameter]
    public string ClassString { get; set; }

    [Parameter]
    public string HeaderText { get; set; }

    [Parameter, EditorRequired]
    public string LogPath { get; set; }

    private RedundancyOptions Model { get; set; }

    [Parameter, EditorRequired]
    public LogLevel LogLevel { get; set; }

    [Parameter, EditorRequired]
    public Func<LogLevel, Task> LogLevelChanged { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var logLevel = await RedundancyHostedService.RedundancyLogLevelAsync();
        if (logLevel != LogLevel)
        {
            LogLevel = logLevel;
            if (LogLevelChanged != null)
            {
                await LogLevelChanged?.Invoke(LogLevel);
            }
            await InvokeAsync(StateHasChanged);
        }
        if (firstRender)
            await InvokeAsync(StateHasChanged);
        await base.OnAfterRenderAsync(firstRender);
    }

    [Inject]
    [NotNull]
    public IStringLocalizer<ThingsGateway.Gateway.Razor._Imports> GatewayLocalizer { get; set; }


    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        HeaderText = GatewayLocalizer[nameof(HeaderText)];
        Model = (await RedundancyService.GetRedundancyAsync()).AdaptRedundancyOptions();
        await base.OnInitializedAsync();
    }

    [Inject]
    [NotNull]
    private IRedundancyHostedService? RedundancyHostedService { get; set; }

    [Inject]
    [NotNull]
    private IRedundancyService? RedundancyService { get; set; }




    [Inject]
    [NotNull]
    private SwalService? SwalService { get; set; }
    private async Task OnSaveRedundancy(EditContext editContext)
    {
        try
        {
            var ret = await SwalService.ShowModal(new SwalOption()
            {
                Category = SwalCategory.Warning,
                Title = RedundancyLocalizer["Restart"]
            });
            if (ret)
            {
                await RedundancyService.EditRedundancyOptionAsync(Model);
                await ToastService.Success(RedundancyLocalizer[nameof(RedundancyOptions)], $"{RazorLocalizer["Save"]}{RazorLocalizer["Success"]}");

                await RedundancyHostedService.StopRedundancyTaskAsync();
                await RedundancyHostedService.StartRedundancyTaskAsync();
                await ToastService.Success(RedundancyLocalizer[nameof(RedundancyOptions)], $"{RazorLocalizer["Success"]}");

                await InvokeAsync(StateHasChanged);
            }
        }
        catch (Exception ex)
        {
            await ToastService.Warning(RedundancyLocalizer[nameof(RedundancyOptions)], $"{RazorLocalizer["Save"]}{RazorLocalizer["Fail", ex]}");
        }
    }

    private async Task RedundancyForcedSync(MouseEventArgs args)
    {
        var ret = await SwalService.ShowModal(new SwalOption()
        {
            Category = SwalCategory.Warning,
            Title = RedundancyLocalizer["ForcedSyncWarning"]
        });
        if (ret)
        {
            await RedundancyHostedService.RedundancyForcedSync();
        }
    }
}
