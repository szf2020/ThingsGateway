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

namespace ThingsGateway.Gateway.Application;

public static partial class ChannelServiceHelpers
{
    public static USheetDatas ExportChannel(IEnumerable<Channel> channels)
    {
        var rows = ExportRows(channels); // IEnumerable 延迟执行
        var sheets = WrapAsSheet(GatewayExportString.ChannelName, rows);
        return USheetDataHelpers.GetUSheetDatas(sheets);
    }
    internal static Dictionary<string, object> WrapAsSheet(string sheetName, IEnumerable<IDictionary<string, object>> rows)
    {
        return new Dictionary<string, object>
        {
            [sheetName] = rows
        };
    }

    internal static IEnumerable<Dictionary<string, object>> ExportRows(IEnumerable<Channel>? data)
    {
        if (data == null)
            yield break;

        #region 列名称

        var type = typeof(Channel);
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

}