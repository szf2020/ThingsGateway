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
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

using ThingsGateway.Admin.Application;
using ThingsGateway.Admin.Razor;
using ThingsGateway.DB;
using ThingsGateway.Extension.Generic;
using ThingsGateway.NewLife.Json.Extension;

namespace ThingsGateway.Gateway.Razor;

public partial class VariableRuntimeInfo
{

    [Inject]
    [NotNull]
    protected BlazorAppContext? AppContext { get; set; }

    [Inject]
    [NotNull]
    private NavigationManager? NavigationManager { get; set; }

    public string RouteName => NavigationManager.ToBaseRelativePath(NavigationManager.Uri);

    protected bool AuthorizeButton(string operate)
    {
        return AppContext.IsHasButtonWithRole(RouteName, operate);
    }
    [Inject]
    [NotNull]
    public IStringLocalizer<ThingsGateway.Razor._Imports>? RazorLocalizer { get; set; }

    [Inject]
    [NotNull]
    public IStringLocalizer<ThingsGateway.Admin.Razor._Imports>? AdminLocalizer { get; set; }

    [Inject]
    [NotNull]
    public DialogService? DialogService { get; set; }

    [NotNull]
    public IStringLocalizer? Localizer { get; set; }

    [Inject]
    [NotNull]
    public IStringLocalizer<OperDescAttribute>? OperDescLocalizer { get; set; }

    [Inject]
    [NotNull]
    public ToastService? ToastService { get; set; }

    #region js
    private List<ITableColumn> ColumnsFunc()
    {
        return table.Columns;
    }
    protected override Task InvokeInitAsync() => InvokeVoidAsync("init", Id, Interop, new { Method = nameof(TriggerStateChanged) });

    private Task OnColumnVisibleChanged(string name, bool visible)
    {
        _cachedFields = table.GetVisibleColumns.ToArray();
        return Task.CompletedTask;
    }
    private Task OnColumnCreating(List<ITableColumn> columns)
    {
        foreach (var column in columns)
        {
            column.OnCellRender = a =>
            {
                a.Class = $"{column.GetFieldName()}";
            };
        }
        return Task.CompletedTask;
    }

    private ITableColumn[] _cachedFields = Array.Empty<ITableColumn>();

    [JSInvokable]
    public List<CellValue> TriggerStateChanged(int rowIndex)
    {
        try
        {

            if (table == null) return null;
            var row = table.Rows[rowIndex];
            if (_cachedFields.Length == 0) _cachedFields = table.GetVisibleColumns.ToArray();
            var list = new List<CellValue>(_cachedFields.Length);
            foreach (var col in _cachedFields)
            {
                var fieldName = col.GetFieldName();
                list.Add(new(fieldName, VariableModelUtils.GetValue(row,fieldName)));
            }

            return list;

        }
        catch (Exception)
        {
            return new();
        }
    }


    #endregion

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender || _cachedFields.Length == 0)
        {
            _cachedFields = table.GetVisibleColumns.ToArray();
        }
        base.OnAfterRender(firstRender);
    }

#if !Management
    [Parameter]
    public ChannelDeviceTreeItem SelectModel { get; set; }

    [Parameter]
    public IEnumerable<VariableRuntime>? Items { get; set; } = Enumerable.Empty<VariableRuntime>();

#endif

    private static void BeforeShowEditDialogCallback(ITableEditDialogOption<VariableRuntime> tableEditDialogOption)
    {
        tableEditDialogOption.Model = tableEditDialogOption.Model.AdaptVariableRuntime();
    }

    [Inject]
    private IOptions<WebsiteOptions>? WebsiteOption { get; set; }
    public bool Disposed { get; set; }
    protected override ValueTask DisposeAsync(bool disposing)
    {
        Disposed = true;
        VariableRuntimeDispatchService.UnSubscribe(Refresh);
        return base.DisposeAsync(disposing);
    }

    protected override void OnInitialized()
    {
        Localizer = App.CreateLocalizerByType(GetType());
        VariableRuntimeDispatchService.Subscribe(Refresh);

        scheduler = new SmartTriggerScheduler(Notify, TimeSpan.FromMilliseconds(1000));

#if !Management
        //timer = new TimerX(RunTimerAsync, null, 1000, 1000) { Async = true };
#endif
        base.OnInitialized();
    }
    //private TimerX timer;
    /// <summary>
    /// IntFormatter
    /// </summary>
    /// <param name="d"></param>
    /// <returns></returns>
    private static Task<string> JsonFormatter(object? d)
    {
        var ret = "";
        if (d is TableColumnContext<VariableRuntime, object?> data && data?.Value != null)
        {
            ret = data.Value.ToSystemTextJsonString();
        }
        return Task.FromResult(ret);
    }
    [Inject]
    private IDispatchService<VariableRuntime> VariableRuntimeDispatchService { get; set; }
    private SmartTriggerScheduler scheduler;

    private Task Refresh(DispatchEntry<VariableRuntime> entry)
    {
        scheduler.Trigger();
        return Task.CompletedTask;
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        scheduler?.Trigger();
    }

    private async Task Notify(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return;
        if (Disposed) return;
        if (table != null)
            await InvokeAsync(table.QueryAsync);
    }

    //private async Task RunTimerAsync(object? state)
    //{
    //    try
    //    {
    //        //if (table != null)
    //        //    await InvokeAsync(() => table.RowElementRefresh());

    //        await InvokeAsync(StateHasChanged);
    //    }
    //    catch (Exception ex)
    //    {
    //        NewLife.Log.XTrace.WriteException(ex);
    //    }
    //    finally
    //    {
    //    }
    //}

    #region 查询

    private QueryPageOptions _option = new();
    private Task<QueryData<VariableRuntime>> OnQueryAsync(QueryPageOptions options)
    {
#if Management
        var data = VariablePageService.OnVariableQueryAsync(options);

        _option = options;
        return data;
#else
        var data = Items
                .WhereIf(!string.IsNullOrWhiteSpace(options.SearchText), a => a.Name.Contains(options.SearchText))
                .GetQueryData(options);
        _option = options;
        return Task.FromResult(data);
#endif
    }

    #endregion 查询

    #region 写入变量

    private string WriteValue { get; set; }

    private async Task OnWriteVariable(VariableRuntime variableRuntime)
    {
        try
        {
            var data = await Task.Run(async () => await VariablePageService.OnWriteVariableAsync(variableRuntime.Id, WriteValue));
            if (!data.IsSuccess)
            {
                await ToastService.Warning(null, data.ErrorMessage);
            }
            else
            {
                await ToastService.Default();
            }
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
        }
    }

    #endregion 写入变量

    #region 编辑

    private int TestVariableCount { get; set; }
    private int TestDeviceCount { get; set; }

    private string SlaveUrl { get; set; }
    private bool BusinessEnable { get; set; }

    [Inject]
    IVariablePageService VariablePageService { get; set; }

    #region 修改
    private async Task Copy(IEnumerable<Variable> variables)
    {
        var op = new DialogOption()
        {
            IsScrolling = false,
            ShowMaximizeButton = true,
            Size = Size.ExtraLarge,
            Title = RazorLocalizer["Copy"],
            ShowFooter = false,
            ShowCloseButton = false,
        };
        if (!variables.Any())
        {
            await ToastService.Warning(null, RazorLocalizer["PleaseSelect"]);
            return;
        }
        op.Component = BootstrapDynamicComponent.CreateComponent<VariableCopyComponent>(new Dictionary<string, object?>
        {
             {nameof(VariableCopyComponent.OnSave), async  (int CopyCount,  string CopyVariableNamePrefix, int CopyVariableNameSuffixNumber) =>
            {
                await Task.Run(() =>VariablePageService.CopyVariableAsync(variables.ToList(),CopyCount,CopyVariableNamePrefix,CopyVariableNameSuffixNumber,AutoRestartThread));
                await InvokeAsync(table.QueryAsync);
            }},
            {nameof(VariableCopyComponent.Model),variables },
        });

        await DialogService.Show(op);
    }

    private async Task BatchEdit(IEnumerable<Variable> variables)
    {
        var op = new DialogOption()
        {
            IsScrolling = false,
            ShowMaximizeButton = true,
            Size = Size.ExtraLarge,
            Title = RazorLocalizer["BatchEdit"],
            ShowFooter = false,
            ShowCloseButton = false,
        };
        var oldModel = variables.Where(a => !a.DynamicVariable).FirstOrDefault();//默认值显示第一个
        if (oldModel == null)
        {
            await ToastService.Warning(null, RazorLocalizer["PleaseSelect"]);
            return;
        }
        variables = variables.Where(a => !a.DynamicVariable).AdaptListVariable();
        oldModel = oldModel.AdaptVariable();
        var model = oldModel.AdaptVariable();//默认值显示第一个
        op.Component = BootstrapDynamicComponent.CreateComponent<VariableEditComponent>(new Dictionary<string, object?>
        {
             {nameof(VariableEditComponent.OnValidSubmit), async () =>
            {
                await Task.Run(()=> VariablePageService.BatchEditVariableAsync(variables.ToList(),oldModel,model, AutoRestartThread));

                await InvokeAsync(table.QueryAsync);
            }},
            {nameof(VariableEditComponent.AutoRestartThread),AutoRestartThread },
            {nameof(VariableEditComponent.Model),model },
            {nameof(VariableEditComponent.ValidateEnable),true },
            {nameof(VariableEditComponent.BatchEditEnable),true },
        });
        await DialogService.Show(op);
    }

    private async Task<bool> Delete(IEnumerable<Variable> variables)
    {
        try
        {
            return await Task.Run(async () => await VariablePageService.DeleteVariableAsync(variables.Select(a => a.Id).ToList(), AutoRestartThread));
        }
        catch (Exception ex)
        {
            await InvokeAsync(async () => await ToastService.Warn(ex));
            return false;
        }
    }

    private async Task<bool> Save(Variable variable, ItemChangedType itemChangedType)
    {
        try
        {
            variable.VariablePropertyModels ??= new();
            foreach (var item in variable.VariablePropertyModels)
            {
                var result = (!PluginServiceUtil.HasDynamicProperty(item.Value.Value)) || (item.Value.ValidateForm?.Validate() != false);
                if (result == false)
                {
                    return false;
                }
            }

            variable.AlarmPropertys ??= new();
            if (variable.AlarmPropertysValidateForm?.Validate() == false)
            {
                return false;
            }

            variable.VariablePropertys = PluginServiceUtil.SetDict(variable.VariablePropertyModels);
            variable = variable.AdaptVariable();
            return await Task.Run(() => VariablePageService.SaveVariableAsync(variable, itemChangedType, AutoRestartThread));
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
            return false;
        }
    }

    #endregion 修改

    private Task<VariableRuntime> OnAdd()
    {
#if !Management
        return Task.FromResult(new VariableRuntime()
        {
            DeviceId = SelectModel?.TryGetDeviceRuntime(out var deviceRuntime) == true ? deviceRuntime?.IsCollect == true ? deviceRuntime?.Id ?? 0 : 0 : 0
        });
#else
        return Task.FromResult(new VariableRuntime());
#endif

    }

    #region 导出

    [Inject]
    [NotNull]
    private IGatewayExportService? GatewayExportService { get; set; }

    private async Task ExcelExportAsync(ITableExportContext<VariableRuntime> tableExportContext, bool all = false)
    {
        bool ret;
        if (all)
        {
            ret = await GatewayExportService.OnVariableExport(new() { QueryPageOptions = new() { SortName = _option.SortName, SortOrder = _option.SortOrder } });
        }
        else
        {
#if !Management
            switch (SelectModel.ChannelDevicePluginType)
            {

                case ChannelDevicePluginTypeEnum.PluginName:
                    ret = await GatewayExportService.OnVariableExport(new() { QueryPageOptions = new() { SortName = _option.SortName, SortOrder = _option.SortOrder }, PluginName = SelectModel.PluginName });
                    break;
                case ChannelDevicePluginTypeEnum.Channel:
                    ret = await GatewayExportService.OnVariableExport(new() { QueryPageOptions = new() { SortName = _option.SortName, SortOrder = _option.SortOrder }, ChannelId = SelectModel.ChannelRuntimeId });
                    break;
                case ChannelDevicePluginTypeEnum.Device:
                    ret = await GatewayExportService.OnVariableExport(new() { QueryPageOptions = new() { SortName = _option.SortName, SortOrder = _option.SortOrder }, DeviceId = SelectModel.DeviceRuntimeId, PluginType = SelectModel.TryGetDeviceRuntime(out var deviceRuntime) ? deviceRuntime?.PluginType : null });
                    break;
                default:
                    ret = await GatewayExportService.OnVariableExport(new() { QueryPageOptions = new() { SortName = _option.SortName, SortOrder = _option.SortOrder } });

                    break;
            }
#else
            ret = await GatewayExportService.OnVariableExport(new() { QueryPageOptions = _option });
#endif
        }

        // 返回 true 时自动弹出提示框
        if (ret)
            await ToastService.Default();
    }

    async Task ExcelVariableAsync(ITableExportContext<VariableRuntime> tableExportContext)
    {
        var op = new DialogOption()
        {
            IsScrolling = false,
            ShowMaximizeButton = true,
            Size = Size.ExtraExtraLarge,
            Title = GatewayLocalizer["ExcelVariable"],
            ShowFooter = false,
            ShowCloseButton = false,
        };
#if !Management

        var models = Items
                .WhereIf(!string.IsNullOrWhiteSpace(_option.SearchText), a => a.Name.Contains(_option.SearchText)).GetData(_option, out var total).Cast<Variable>().ToList();

#else

        var models = await VariablePageService.GetVariableListAsync(_option, 2000);

#endif

        if (models.Count > 2000)
        {
            await ToastService.Warning("online Excel max data count 2000");
            return;
        }
        var uSheetDatas = await VariablePageService.ExportVariableAsync(models, _option.SortName, _option.SortOrder);

        op.Component = BootstrapDynamicComponent.CreateComponent<USheet>(new Dictionary<string, object?>
        {
             {nameof(USheet.OnSave), async (USheetDatas data) =>
            {
                try
    {
                await Task.Run(async ()=>
                {
                    await    VariablePageService.ImportVariableUSheetDatasAsync(data,AutoRestartThread);
                })
                    ;
    }
finally
                {
                                 await InvokeAsync( async ()=>
            {
                    await table.QueryAsync();
             StateHasChanged();
                });
                }
            }},
            {nameof(USheet.Model),uSheetDatas },
        });

        await DialogService.Show(op);
    }

    private async Task ExcelImportAsync(ITableExportContext<VariableRuntime> tableExportContext)
    {
        var op = new DialogOption()
        {
            IsScrolling = true,
            ShowMaximizeButton = true,
            Size = Size.ExtraLarge,
            Title = GatewayLocalizer["ImportVariable"],
            ShowFooter = false,
            ShowCloseButton = false,
            OnCloseAsync = async () => await InvokeAsync(table.QueryAsync),
        };

        Func<IBrowserFile, Task<Dictionary<string, ImportPreviewOutputBase>>> import = (a =>
        {

            return VariablePageService.ImportVariableAsync(a, AutoRestartThread);

        });
        op.Component = BootstrapDynamicComponent.CreateComponent<ImportExcel>(new Dictionary<string, object?>
        {
             {nameof(ImportExcel.Import),import },
        });
        await DialogService.Show(op);
    }

    #endregion 导出

    #region 清空

    private async Task ClearAsync()
    {
        try
        {
            await Task.Run(async () =>
            {

                await VariablePageService.ClearVariableAsync(AutoRestartThread);

                await InvokeAsync(async () =>
                {
                    await ToastService.Default();
                    await InvokeAsync(table.QueryAsync);
                });
            });
        }
        catch (Exception ex)
        {
            await InvokeAsync(async () => await ToastService.Warn(ex));
        }
    }
    #endregion

    private async Task InsertTestDataAsync()
    {
        try
        {
            try
            {
                await Task.Run(() => VariablePageService.InsertTestDataAsync(TestVariableCount, TestDeviceCount, SlaveUrl, BusinessEnable, AutoRestartThread));
            }
            finally
            {
                await InvokeAsync(async () =>
                {
                    await ToastService.Default();
                    await table.QueryAsync();
                    StateHasChanged();
                });
            }
        }
        catch (Exception ex)
        {
            await InvokeAsync(async () => await ToastService.Warn(ex));
        }
    }


    private async Task InsertTestDtuDataAsync()
    {
        try
        {
            try
            {
                await Task.Run(() => VariablePageService.InsertTestDtuDataAsync(TestDeviceCount, SlaveUrl, AutoRestartThread));
            }
            finally
            {
                await InvokeAsync(async () =>
                {
                    await ToastService.Default();
                    await table.QueryAsync();
                    StateHasChanged();
                });
            }
        }
        catch (Exception ex)
        {
            await InvokeAsync(async () => await ToastService.Warn(ex));
        }
    }


    [Parameter]
    public bool AutoRestartThread { get; set; }

    [Inject]
    [NotNull]
    public IStringLocalizer<ThingsGateway.Gateway.Razor._Imports>? GatewayLocalizer { get; set; }
    #endregion
}
