//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Components.Forms;

using ThingsGateway.Admin.Application;

namespace ThingsGateway.Admin.Razor;

public partial class SysUserEdit
{
    private List<SelectedItem> ModuleSelectedItems { get; set; }
    [Inject]
    private IStringLocalizer<ThingsGateway.Admin.Razor._Imports>? AdminLocalizer { get; set; }
    [Inject]
    private ISysPositionService? SysPositionService { get; set; }

    [Parameter]
    [NotNull]
    public SysUser? Model { get; set; }

    private List<CascaderItem> Items { get; set; }
    private List<SelectedItem> BoolItems;
    [Inject]
    private ISysResourceService SysResourceService { get; set; }
    protected override async Task OnInitializedAsync()
    {
        BoolItems = LocalizerUtil.GetBoolItems(Model.GetType(), nameof(Model.Status));
        var items = await SysPositionService.SelectorAsync(new PositionSelectorInput());
        Items = PositionUtil.BuildCascaderItemList(items);
        ModuleSelectedItems = AdminResourceUtil.BuildModuleSelectList((await SysResourceService.GetAllAsync())).ToList();
        await InvokeAsync(StateHasChanged);
        await base.OnInitializedAsync();
    }

    private Task OnSelectedItemChanged(CascaderItem[] items)
    {
        Model.OrgId = items.LastOrDefault()?.Parent?.Value?.ToLong() ?? 0;
        return Task.CompletedTask;
    }
    [Inject]
    ToastService ToastService { get; set; }

    #region 头像

    private List<UploadFile> PreviewFileList;

    [FileValidation(Extensions = [".png", ".jpg", ".jpeg"], FileSize = 200 * 1024)]
    public IBrowserFile? Picture { get; set; }

    private CancellationTokenSource? ReadAvatarToken { get; set; }

    public void Dispose()
    {
        ReadAvatarToken?.Cancel();
        GC.SuppressFinalize(this);
    }

    protected override void OnInitialized()
    {
        PreviewFileList = new(new[] { new UploadFile { PrevUrl = Model.Avatar } });
        base.OnInitialized();
    }

    private async Task OnAvatarUpload(UploadFile file)
    {
        if (file?.File != null)
        {
            var format = file.File.ContentType;
            ReadAvatarToken ??= new CancellationTokenSource();
            if (ReadAvatarToken.IsCancellationRequested)
            {
                ReadAvatarToken.Dispose();
                ReadAvatarToken = new CancellationTokenSource();
            }

            await file.RequestBase64ImageFileAsync(format, 640, 480, 1024 * 200, token: ReadAvatarToken.Token);

            if (file.Code != 0)
            {
                await ToastService.Error($"{file.Error} ");
            }
            else
            {
                Model.Avatar = file.PrevUrl;
            }
        }
    }

    #endregion 头像
}
