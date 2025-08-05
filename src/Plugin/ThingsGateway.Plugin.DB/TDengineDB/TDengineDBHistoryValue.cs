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

using ThingsGateway.Common;
using ThingsGateway.SqlSugar;

namespace ThingsGateway.Plugin.TDengineDB;

[SugarTable("historyValue")]
[STableAttribute(STableName = "historyValue", Tag1 = nameof(DeviceName), Tag2 = nameof(Name))]
public class TDengineDBNumberHistoryValue : STable, IPrimaryIdEntity, IDBHistoryValue
{
    [System.ComponentModel.DataAnnotations.Key]
    public long Id { get; set; }

    [SugarColumn(InsertServerTime = true)]
    [AutoGenerateColumn(Order = 1, Visible = true, Sortable = true, Filterable = false)]
    public DateTime CreateTime { get; set; }

    [AutoGenerateColumn(Order = 1, Visible = true, Sortable = true, Filterable = false)]
    public DateTime CollectTime { get; set; }

    [AutoGenerateColumn(Order = 1, Visible = true, Sortable = true, Filterable = false)]
    public bool IsOnline { get; set; }

    [AutoGenerateColumn(Order = 1, Visible = true, Sortable = true, Filterable = false)]
    public decimal Value { get; set; }

    [AutoGenerateColumn(Order = 1, Visible = true, Sortable = true, Filterable = false)]
    public string DeviceName { get; set; }

    [AutoGenerateColumn(Order = 1, Visible = true, Sortable = true, Filterable = false)]
    public string Name { get; set; }
}
