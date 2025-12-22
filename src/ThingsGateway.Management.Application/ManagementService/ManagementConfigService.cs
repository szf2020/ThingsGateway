//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;

using MiniExcelLibs;

using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using ThingsGateway.Common.Extension;

namespace ThingsGateway.Management.Application;

public partial class ManagementConfigService(IDispatchService<ManagementConfig> dispatchService) : BaseService<ManagementConfig>
{

    public override async Task<bool> DeleteAsync(IEnumerable<long> models)
    {
        var result = await base.DeleteAsync(models).ConfigureAwait(false);
        if (result)
        {
            foreach (var id in models)
            {

                if (ManagementGlobalData.IdManagementConfigs.TryGetValue(id, out var oldModel))
                {
                    try
                    {
                        await oldModel.DisposeAsync().ConfigureAwait(false);
                        DeleteManagementConfigFromCache();
                    }
                    catch (Exception ex)
                    {
                        ManagementGlobalData.LogMessage.LogWarning(ex);
                    }
                }
            }
        }
        return result;
    }

    public override async Task<bool> SaveAsync(ManagementConfig model, BootstrapBlazor.Components.ItemChangedType changedType)
    {
        var result = await base.SaveAsync(model, changedType).ConfigureAwait(false);
        if (result)
        {
            try
            {
                if (ManagementGlobalData.IdManagementConfigs.TryGetValue(model.Id, out var oldModel))
                {
                    await oldModel.DisposeAsync().ConfigureAwait(false);
                }

                await model.InitAsync().ConfigureAwait(false);
                DeleteManagementConfigFromCache();
            }
            catch (Exception ex)
            {
                ManagementGlobalData.LogMessage.LogWarning(ex);
            }
        }
        return result;

    }

    public override async Task<bool> SaveAsync(List<ManagementConfig> models, BootstrapBlazor.Components.ItemChangedType changedType)
    {
        var result = await base.SaveAsync(models, changedType).ConfigureAwait(false);
        if (result)
        {
            foreach (var model in models)
            {
                try
                {

                    var id = model.Id;
                    if (ManagementGlobalData.IdManagementConfigs.TryGetValue(id, out var oldModel))
                    {

                        await oldModel.DisposeAsync().ConfigureAwait(false);

                    }
                    await model.InitAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    ManagementGlobalData.LogMessage.LogWarning(ex);
                }
            }

            DeleteManagementConfigFromCache();
        }
        return result;
    }

    public void DeleteManagementConfigFromCache()
    {
        dispatchService.Dispatch(null);
    }

    /// <summary>
    /// 从缓存/数据库获取全部信息
    /// </summary>
    /// <returns>列表</returns>
    public async Task<List<ManagementConfig>> GetFromDBAsync(Expression<Func<ManagementConfig, bool>> expression = null, ISqlOrmClient db = null)
    {

        db ??= GetDB();
        var channels = await db.Queryable<ManagementConfig>().WhereIF(expression != null, expression).OrderBy(a => a.Id).ToListAsync().ConfigureAwait(false);
        return channels;
    }

    #region 导出
    private async Task<Func<ISqlQueryable<ManagementConfig>, ISqlQueryable<ManagementConfig>>> GetWhereQueryFunc(ManagementExportFilter exportFilter)
    {
        var dataScope = await ManagementGlobalData.SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        var whereQuery = (ISqlQueryable<ManagementConfig> a) => a
        .WhereIF(!exportFilter.QueryPageOptions.SearchText.IsNullOrWhiteSpace(), a => a.Name.Contains(exportFilter.QueryPageOptions.SearchText!))

                          .WhereIF(dataScope != null && dataScope?.Count > 0, u => dataScope.Contains(u.CreateOrgId))//在指定机构列表查询
         .WhereIF(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId);
        return whereQuery;
    }
    /// <inheritdoc/>
    [OperDesc("ExportManagementConfig", isRecordPar: false, localizerType: typeof(ManagementConfig))]
    public async Task<Dictionary<string, object>> ExportManagementConfigAsync(ManagementExportFilter exportFilter)
    {
        var managementConfigs = await GetEnumerableData(exportFilter).ConfigureAwait(false);
        var rows = ManagementConfigServiceHelpers.ExportRows(managementConfigs); // IEnumerable 延迟执行
        var sheets = ManagementConfigServiceHelpers.WrapAsSheet(ManagementExportString.ManagementConfigName, rows);
        return sheets;
    }

    private async Task<IAsyncEnumerable<ManagementConfig>> GetEnumerableData(ManagementExportFilter exportFilter)
    {
        var db = GetDB();
        var whereQuery = await GetWhereQueryFunc(exportFilter).ConfigureAwait(false);

        var query = GetQuery(db, exportFilter.QueryPageOptions, whereQuery, exportFilter.FilterKeyValueAction);

        return query.ToAsyncEnumerable();
    }

    #endregion 导出

    #region 导入

    /// <inheritdoc/>
    [OperDesc("ImportManagementConfig", isRecordPar: false, localizerType: typeof(ManagementConfig))]
    public async Task<Dictionary<string, ImportPreviewOutputBase>> ImportManagementConfigAsync(IBrowserFile browserFile)
    {
        var data = await PreviewAsync(browserFile).ConfigureAwait(false);
        await ImportManagementConfigAsync(data).ConfigureAwait(false);
        return data;
    }


    /// <inheritdoc/>
    [OperDesc("ImportManagementConfig", isRecordPar: false, localizerType: typeof(ManagementConfig))]
    public async Task<HashSet<long>> ImportManagementConfigAsync(Dictionary<string, ImportPreviewOutputBase> input)
    {
        List<ManagementConfig>? managementConfigs = new List<ManagementConfig>();
        foreach (var item in input)
        {
            if (item.Key == ManagementExportString.ManagementConfigName)
            {
                var managementConfigImports = ((ImportPreviewListOutput<ManagementConfig>)item.Value).Data;
                managementConfigs = managementConfigImports;
                break;
            }
        }
        var upData = managementConfigs.Where(a => a.IsUp).ToList();
        var insertData = managementConfigs.Where(a => !a.IsUp).ToList();


        using var db = GetDB();
        if (ManagementGlobalData.HardwareJob.HardwareInfo.AvailableMemory > 2048)
        {
            await db.BulkCopyAsync(insertData, 200000).ConfigureAwait(false);
            await db.BulkUpdateAsync(upData, 200000).ConfigureAwait(false);
        }
        else
        {
            await db.BulkCopyAsync(insertData, 10000).ConfigureAwait(false);
            await db.BulkUpdateAsync(upData, 10000).ConfigureAwait(false);
        }
        DeleteManagementConfigFromCache();
        var data = managementConfigs.Select(a => a.Id).ToHashSet();


        var newChannelRuntimes = (await GetFromDBAsync((a => data.Contains(a.Id))).ConfigureAwait(false));
        foreach (var channelRuntime in newChannelRuntimes)
        {
            try
            {
                if (ManagementGlobalData.IdManagementConfigs.TryGetValue(channelRuntime.Id, out var oldModel))
                {
                    await oldModel.DisposeAsync().ConfigureAwait(false);
                }

                await channelRuntime.InitAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ManagementGlobalData.LogMessage.LogWarning(ex);
            }
        }
        return data;
    }


    /// <inheritdoc/>
    public async Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile browserFile)
    {
        var path = await browserFile.StorageLocal().ConfigureAwait(false);
        try
        {
            var dataScope = await ManagementGlobalData.SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
            var sheetNames = MiniExcel.GetSheetNames(path);
            var managementConfigDicts = ManagementGlobalData.ManagementConfigs;
            //导入检验结果
            Dictionary<string, ImportPreviewOutputBase> ImportPreviews = new();
            //设备页
            foreach (var sheetName in sheetNames)
            {
#pragma warning disable CA1849
                var rows = MiniExcel.Query(path, useHeaderRow: true, sheetName: sheetName).Cast<IDictionary<string, object>>();
#pragma warning restore CA1849
                SetManagementConfigData(dataScope, managementConfigDicts, ImportPreviews, sheetName, rows);
            }

            return ImportPreviews;
        }
        finally
        {
            FileHelper.DeleteFile(path);
        }
    }

    public void SetManagementConfigData(HashSet<long>? dataScope, IReadOnlyDictionary<string, ManagementConfig> managementConfigDicts, Dictionary<string, ImportPreviewOutputBase> ImportPreviews, string sheetName, IEnumerable<IDictionary<string, object>> rows)
    {
        #region sheet

        if (sheetName == ManagementExportString.ManagementConfigName)
        {
            int row = 1;
            ImportPreviewListOutput<ManagementConfig> importPreviewOutput = new();
            ImportPreviews.Add(sheetName, importPreviewOutput);
            List<ManagementConfig> managementConfigs = new();
            var type = typeof(ManagementConfig);
            // 获取目标类型的所有属性，并根据是否需要过滤 IgnoreExcelAttribute 进行筛选
            var managementConfigProperties = type.GetRuntimeProperties().Where(a => (a.GetCustomAttribute<IgnoreExcelAttribute>() == null) && a.CanWrite)
                                        .ToDictionary(a => type.GetPropertyDisplayName(a.Name), a => (a, ReflectHelper.IsNullableType(a)));

            rows.ForEach(item =>
            {
                try
                {
                    var managementConfig = item.ConvertToEntity<ManagementConfig>(managementConfigProperties);
                    if (managementConfig == null)
                    {
                        importPreviewOutput.HasError = true;
                        importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, Localizer["ImportNullError"]));
                        return;
                    }

                    // 进行对象属性的验证
                    var validationContext = new ValidationContext(managementConfig);
                    var validationResults = new List<ValidationResult>();
                    validationContext.ValidateProperty(validationResults);

                    // 构建验证结果的错误信息
                    using ValueStringBuilder stringBuilder = new();
                    foreach (var validationResult in validationResults.Where(v => !string.IsNullOrEmpty(v.ErrorMessage)))
                    {
                        foreach (var memberName in validationResult.MemberNames)
                        {
                            stringBuilder.Append(validationResult.ErrorMessage!);
                        }
                    }

                    // 如果有验证错误，则添加错误信息到导入预览结果并返回
                    if (stringBuilder.Length > 0)
                    {
                        importPreviewOutput.HasError = true;
                        importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, stringBuilder.ToString()));
                        return;
                    }

                    if (managementConfigDicts.TryGetValue(managementConfig.Name, out var collectManagementConfig))
                    {
                        managementConfig.Id = collectManagementConfig.Id;
                        managementConfig.CreateOrgId = collectManagementConfig.CreateOrgId;
                        managementConfig.CreateUserId = collectManagementConfig.CreateUserId;
                        managementConfig.IsUp = true;
                    }
                    else
                    {
                        managementConfig.Id = CommonUtils.GetSingleId();
                        managementConfig.IsUp = false;
                        managementConfig.CreateOrgId = UserManager.OrgId;
                        managementConfig.CreateUserId = UserManager.UserId;
                    }

                    if (managementConfig.IsUp && ((dataScope != null && dataScope?.Count > 0 && !dataScope.Contains(managementConfig.CreateOrgId)) || dataScope?.Count == 0 && managementConfig.CreateUserId != UserManager.UserId))
                    {
                        importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, "Operation not permitted"));
                    }
                    else
                    {
                        managementConfigs.Add(managementConfig);
                        importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), true, null));
                    }
                    return;
                }
                catch (Exception ex)
                {
                    importPreviewOutput.HasError = true;
                    importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, ex.Message));
                    return;
                }
            });
            importPreviewOutput.Data = managementConfigs.DistinctBy(a => a.Name).ToList();
        }

        #endregion sheet
    }

    #endregion 导入
}
