using BootstrapBlazor.Components;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

using ThingsGateway.Blazor.Diagrams.Core.Geometry;
using ThingsGateway.Blazor.Diagrams.Core.Models;

namespace ThingsGateway.Gateway.Application;

[OrmTable("rules", TableDescription = "规则引擎")]
[OrmIndex("unique_rules_name", nameof(Rules.Name), OrderByType.Asc, true)]
[Tenant(SqlOrmConst.DB_Custom)]
public class Rules : BaseDataEntity
{
    [OrmColumn(ColumnDescription = "名称", Length = 200)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// 状态
    ///</summary>
    [OrmColumn(ColumnDescription = "状态", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Sortable = true, Filterable = true)]
    public bool Status { get; set; } = true;

    [OrmColumn(IsJson = true, ColumnDataType = StaticConfig.CodeFirst_BigString, ColumnDescription = "RulesJson", IsNullable = true)]
    [IgnoreExcel]
    [AutoGenerateColumn(Ignore = true)]
    public RulesJson RulesJson { get; set; }
}
public class RulesJson
{
    public List<NodeJson> NodeJsons { get; set; } = new();
    public List<LinkJson> LinkJsons { get; set; } = new();
}
public class LinkJson
{
    public Anchor SourcePortAnchor { get; set; } = new();
    public Anchor TargetPortAnchor { get; set; } = new();
}

public class Anchor
{
    public string NodelId { get; set; }
    public PortAlignment PortAlignment { get; set; }
}

public class NodeJson
{
    public string Id { get; set; }
    public string DraggedType { get; set; }
    public JsonObject CValues { get; set; } = new();
    public Point Point { get; set; }
}