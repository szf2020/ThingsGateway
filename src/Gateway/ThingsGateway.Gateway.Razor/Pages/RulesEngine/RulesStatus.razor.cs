//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Gateway.Razor;

public partial class RulesStatus
{
    private Rules? _rules { get; set; }

    [Parameter, EditorRequired]
    public long RulesId { get; set; }
    [Inject]
    IRulesEngineHostedService RulesEngineHostedService { get; set; }
    protected override async Task OnParametersSetAsync()
    {
        if (RulesId > 0)
            _rules = await RulesEngineHostedService.GetRuleRuntimesAsync(RulesId);
        else
            _rules = null;
        await base.OnParametersSetAsync();
    }
    public TouchSocket.Core.LogLevel LogLevel { get; set; }
    public string LogPath { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (RulesId > 0)
        {
            var path = await RulesEngineHostedService.RulesLogPathAsync(RulesId);
            var logLevel = await RulesEngineHostedService.RulesLogLevelAsync(RulesId);
            bool changed = false;
            if (path != LogPath)
            {
                LogPath = path;
                changed = true;
            }

            if (logLevel != LogLevel)
            {
                LogLevel = logLevel;
                changed = true;
            }

            if (changed)
            {
                await InvokeAsync(StateHasChanged);
            }

        }
        if (firstRender)
            await InvokeAsync(StateHasChanged);
        await base.OnAfterRenderAsync(firstRender);
    }


}
