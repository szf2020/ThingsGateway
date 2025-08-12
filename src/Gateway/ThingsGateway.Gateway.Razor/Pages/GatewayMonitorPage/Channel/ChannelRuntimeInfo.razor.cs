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

public partial class ChannelRuntimeInfo
{
#if !Management
    private string Height { get; set; } = "calc(100% - 270px)";
#else
    private string Height { get; set; } = "calc(100% - 330px)";

#endif
    [Parameter, EditorRequired]
    public ChannelRuntime ChannelRuntime { get; set; }

    public TouchSocket.Core.LogLevel LogLevel { get; set; }
    [Inject]
    IChannelPageService ChannelPageService { get; set; }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (ChannelRuntime?.Id > 0)
        {
            var logLevel = await ChannelPageService.ChannelLogLevelAsync(ChannelRuntime.Id);

            if (logLevel != LogLevel)
            {
                LogLevel = logLevel;
                await InvokeAsync(StateHasChanged);
            }


        }
        if (firstRender)
            await InvokeAsync(StateHasChanged);
        await base.OnAfterRenderAsync(firstRender);
    }

}
