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

using System.Reflection;

using ThingsGateway.Common.Extension;

namespace ThingsGateway.Management.Application;

public static class ManagementConfigServiceHelpers
{
    public static USheetDatas ExportManagementConfig(IEnumerable<ManagementConfig> managementConfigs)
    {
        var rows = ExportRows(managementConfigs); // IEnumerable 延迟执行
        var sheets = WrapAsSheet(ManagementExportString.ManagementConfigName, rows);
        return USheetDataHelpers.GetUSheetDatas(sheets);
    }

    internal static IEnumerable<Dictionary<string, object>> ExportRows(IEnumerable<ManagementConfig>? data)
    {
        if (data == null)
            yield break;

        #region 列名称

        var type = typeof(ManagementConfig);
        var propertyInfos = type.GetRuntimeProperties().Where(a => a.GetCustomAttribute<IgnoreExcelAttribute>(false) == null)
             .OrderBy(
            a =>
            {
                var order = a.GetCustomAttribute<AutoGenerateColumnAttribute>()?.Order ?? int.MaxValue;
                if (order < 0)
                {
                    order = order + 10000000;
                }
                else if (order == 0)
                {
                    order = 10000000;
                }
                return order;
            }
            )
            ;

        #endregion 列名称

        foreach (var device in data)
        {
            Dictionary<string, object> row = new();
            foreach (var prop in propertyInfos)
            {
                var desc = type.GetPropertyDisplayName(prop.Name);
                row.Add(desc ?? prop.Name, prop.GetValue(device)?.ToString());
            }
            yield return row;
        }
    }

    internal static async IAsyncEnumerable<Dictionary<string, object>> ExportRows(IAsyncEnumerable<ManagementConfig>? data)
    {
        if (data == null)
            yield break;

        #region 列名称

        var type = typeof(ManagementConfig);
        var propertyInfos = type.GetRuntimeProperties().Where(a => a.GetCustomAttribute<IgnoreExcelAttribute>(false) == null)
             .OrderBy(
            a =>
            {
                var order = a.GetCustomAttribute<AutoGenerateColumnAttribute>()?.Order ?? int.MaxValue;
                if (order < 0)
                {
                    order = order + 10000000;
                }
                else if (order == 0)
                {
                    order = 10000000;
                }
                return order;
            }
            )
            ;

        #endregion 列名称
        var enumerator = data.GetAsyncEnumerator();
        while (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            var device = enumerator.Current;
            {
                Dictionary<string, object> row = new();
                foreach (var prop in propertyInfos)
                {
                    var desc = type.GetPropertyDisplayName(prop.Name);
                    row.Add(desc ?? prop.Name, prop.GetValue(device)?.ToString());
                }
                yield return row;
            }
        }
    }

    internal static Dictionary<string, object> WrapAsSheet(string sheetName, IEnumerable<IDictionary<string, object>> rows)
    {
        return new Dictionary<string, object>
        {
            [sheetName] = rows
        };
    }

    internal static Dictionary<string, object> WrapAsSheet(string sheetName, IAsyncEnumerable<IDictionary<string, object>> rows)
    {
        return new Dictionary<string, object>
        {
            [sheetName] = rows
        };
    }

    public static async Task<Dictionary<string, ImportPreviewOutputBase>> ImportAsync(USheetDatas uSheetDatas)
    {
        var dataScope = await ManagementGlobalData.SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        var managementConfigDicts = ManagementGlobalData.ManagementConfigs;
        //导入检验结果
        Dictionary<string, ImportPreviewOutputBase> ImportPreviews = new();
        //设备页

        var sheetNames = uSheetDatas.sheets.Keys.ToList();
        foreach (var sheetName in sheetNames)
        {
            List<IDictionary<string, object>> rows = new();
            var first = uSheetDatas.sheets[sheetName].cellData[0];

            foreach (var item in uSheetDatas.sheets[sheetName].cellData)
            {
                if (item.Key == 0)
                {
                    continue;
                }
                var expando = new Dictionary<string, object>();
                foreach (var keyValue in item.Value)
                {
                    expando.Add(first[keyValue.Key].v?.ToString(), keyValue.Value.v);
                }
                rows.Add(expando);
            }

            ManagementGlobalData.ManagementConfigService.SetManagementConfigData(dataScope, managementConfigDicts, ImportPreviews, sheetName, rows);
            var data = ImportPreviews?.FirstOrDefault().Value;
            if (data?.HasError == true)
            {
                throw new(data.Results.FirstOrDefault(a => !a.Success).ErrorMessage ?? "error");
            }
        }

        return ImportPreviews;
    }
}