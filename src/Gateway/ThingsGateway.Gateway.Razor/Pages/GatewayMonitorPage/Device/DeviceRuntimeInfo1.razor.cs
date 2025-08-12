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

public partial class DeviceRuntimeInfo1 : IDisposable
{
    [Inject]
    IStringLocalizer<ThingsGateway.Gateway.Razor._Imports> GatewayLocalizer { get; set; }

    [Parameter, EditorRequired]
    public DeviceRuntime DeviceRuntime { get; set; }
    private string Name => $"{DeviceRuntime.ToString()}  -  {(DeviceRuntime.Started == false ? "Task cancel" : "Task run")}";
    public ModelValueValidateForm PluginPropertyModel;

    [Inject]
    IPluginService PluginService { get; set; }
#if !Management
    private IDriver Driver => DeviceRuntime?.Driver;
    private bool IsRedundant => GlobalData.IsRedundant(DeviceRuntime.Id);
#else
    private IDriver Driver { get; set; }
    private bool IsRedundant { get; set; }
#endif
    protected override async Task OnParametersSetAsync()
    {
#if Management

        IsRedundant = await DevicePageService.IsRedundantDeviceAsync(DeviceRuntime.Id);

#endif
        await base.OnParametersSetAsync();
    }
    protected override void OnParametersSet()
    {
#if !Management
        if (PluginPropertyModel?.Value == null || PluginPropertyModel?.Value != DeviceRuntime.Driver?.DriverProperties)
        {
            PluginPropertyModel = new ModelValueValidateForm()
            {
                Value = DeviceRuntime.Driver?.DriverProperties
            };
        }

#else
        Driver = PluginService.GetDriver(DeviceRuntime.PluginName);
        var data = PluginService.GetDriverPropertyTypes(DeviceRuntime.PluginName, Driver);

        var DriverProperties = data.Model;
        PluginServiceUtil.SetModel(DriverProperties, DeviceRuntime.DevicePropertys);

        if (PluginPropertyModel?.Value == null || PluginPropertyModel?.Value != DriverProperties)
        {
            PluginPropertyModel = new ModelValueValidateForm()
            {
                Value = DriverProperties
            };
        }
#endif
        base.OnParametersSet();
    }


#if !Management
    private async Task ShowDriverUI()
    {
        var driver = DeviceRuntime.Driver?.DriverUIType;
        if (driver == null)
        {
            return;
        }
        await DialogService.Show(new DialogOption()
        {
            IsScrolling = false,
            ShowMaximizeButton = true,
            Size = Size.ExtraExtraLarge,
            Title = DeviceRuntime.Name,
            Component = BootstrapDynamicComponent.CreateComponent(driver, new Dictionary<string, object?>()
        {
            {nameof(IDriverUIBase.Driver),DeviceRuntime.Driver},
        })
        });
    }
#endif

    [Inject]
    IDevicePageService DevicePageService { get; set; }
    private async Task DeviceRedundantThreadAsync()
    {
        await DevicePageService.DeviceRedundantThreadAsync(DeviceRuntime.Id);
    }


    private async Task RestartDeviceAsync(bool deleteCache)
    {
        await DevicePageService.RestartDeviceAsync(DeviceRuntime.Id, deleteCache);
    }

    private async Task PauseThreadAsync()
    {
        await DevicePageService.PauseThreadAsync(DeviceRuntime.Id);
    }

    protected override void OnInitialized()
    {
#if !Management
        _ = RunTimerAsync();
#endif
        base.OnInitialized();
    }

    private bool Disposed;
    private async Task RunTimerAsync()
    {
        while (!Disposed)
        {
            try
            {
#pragma warning disable CA1849
                OnParametersSet();
#pragma warning restore CA1849
                await InvokeAsync(() => StateHasChanged());
            }
            catch (Exception ex)
            {
                NewLife.Log.XTrace.WriteException(ex);
            }
            finally
            {
                await Task.Delay(5000);
            }
        }
    }

    public void Dispose()
    {
        Disposed = true;
        GC.SuppressFinalize(this);
    }
}
