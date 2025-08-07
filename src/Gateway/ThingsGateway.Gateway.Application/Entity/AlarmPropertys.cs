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

using System.ComponentModel.DataAnnotations;

namespace ThingsGateway.Gateway.Application;

public class AlarmPropertys : IValidatableObject
{
    private int alarmDelay;
    private int alarmLevel;

    private decimal hAlarmCode = 50;
    private decimal lAlarmCode = 10;
    private decimal hHAlarmCode = 90;
    private decimal lLAlarmCode = 0;

    private bool boolOpenAlarmEnable;
    private bool boolCloseAlarmEnable;
    private bool hAlarmEnable;
    private bool hHAlarmEnable;
    private bool lLAlarmEnable;
    private bool lAlarmEnable;
    private bool customAlarmEnable;

    private string boolOpenRestrainExpressions;
    private string boolOpenAlarmText;
    private string boolCloseRestrainExpressions;
    private string boolCloseAlarmText;
    private string hRestrainExpressions;
    private string hAlarmText;
    private string hHRestrainExpressions;
    private string hHAlarmText;
    private string lRestrainExpressions;
    private string lAlarmText;

    private string lLRestrainExpressions;
    private string lLAlarmText;
    private string customRestrainExpressions;
    private string customAlarmText;
    private string customAlarmCode;

    #region 报警


    /// <summary>
    /// 报警等级
    /// </summary>
    [SugarColumn(ColumnDescription = "报警等级")]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public int AlarmLevel { get => alarmLevel; set => alarmLevel = value; }

    /// <summary>
    /// 报警延时
    /// </summary>
    [SugarColumn(ColumnDescription = "报警延时")]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public int AlarmDelay { get => alarmDelay; set => alarmDelay = value; }

    /// <summary>
    /// 布尔开报警使能
    /// </summary>
    [SugarColumn(ColumnDescription = "布尔开报警使能")]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public bool BoolOpenAlarmEnable { get => boolOpenAlarmEnable; set => boolOpenAlarmEnable = value; }

    /// <summary>
    /// 布尔开报警约束
    /// </summary>
    [SugarColumn(ColumnDescription = "布尔开报警约束", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string BoolOpenRestrainExpressions { get => boolOpenRestrainExpressions; set => boolOpenRestrainExpressions = value; }

    /// <summary>
    /// 布尔开报警文本
    /// </summary>
    [SugarColumn(ColumnDescription = "布尔开报警文本", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string BoolOpenAlarmText { get => boolOpenAlarmText; set => boolOpenAlarmText = value; }

    /// <summary>
    /// 布尔关报警使能
    /// </summary>
    [SugarColumn(ColumnDescription = "布尔关报警使能")]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public bool BoolCloseAlarmEnable { get => boolCloseAlarmEnable; set => boolCloseAlarmEnable = value; }

    /// <summary>
    /// 布尔关报警约束
    /// </summary>
    [SugarColumn(ColumnDescription = "布尔关报警约束", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string BoolCloseRestrainExpressions { get => boolCloseRestrainExpressions; set => boolCloseRestrainExpressions = value; }

    /// <summary>
    /// 布尔关报警文本
    /// </summary>
    [SugarColumn(ColumnDescription = "布尔关报警文本", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string BoolCloseAlarmText { get => boolCloseAlarmText; set => boolCloseAlarmText = value; }

    /// <summary>
    /// 高报使能
    /// </summary>
    [SugarColumn(ColumnDescription = "高报使能")]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public bool HAlarmEnable { get => hAlarmEnable; set => hAlarmEnable = value; }

    /// <summary>
    /// 高报约束
    /// </summary>
    [SugarColumn(ColumnDescription = "高报约束", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string HRestrainExpressions { get => hRestrainExpressions; set => hRestrainExpressions = value; }

    /// <summary>
    /// 高报文本
    /// </summary>
    [SugarColumn(ColumnDescription = "高报文本", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string HAlarmText { get => hAlarmText; set => hAlarmText = value; }

    /// <summary>
    /// 高限值
    /// </summary>
    [SugarColumn(ColumnDescription = "高限值", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public decimal HAlarmCode { get => hAlarmCode; set => hAlarmCode = value; }

    /// <summary>
    /// 高高报使能
    /// </summary>
    [SugarColumn(ColumnDescription = "高高报使能")]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public bool HHAlarmEnable { get => hHAlarmEnable; set => hHAlarmEnable = value; }

    /// <summary>
    /// 高高报约束
    /// </summary>
    [SugarColumn(ColumnDescription = "高高报约束", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string HHRestrainExpressions { get => hHRestrainExpressions; set => hHRestrainExpressions = value; }

    /// <summary>
    /// 高高报文本
    /// </summary>
    [SugarColumn(ColumnDescription = "高高报文本", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string HHAlarmText { get => hHAlarmText; set => hHAlarmText = value; }

    /// <summary>
    /// 高高限值
    /// </summary>
    [SugarColumn(ColumnDescription = "高高限值", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public decimal HHAlarmCode { get => hHAlarmCode; set => hHAlarmCode = value; }

    /// <summary>
    /// 低报使能
    /// </summary>
    [SugarColumn(ColumnDescription = "低报使能")]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public bool LAlarmEnable { get => lAlarmEnable; set => lAlarmEnable = value; }

    /// <summary>
    /// 低报约束
    /// </summary>
    [SugarColumn(ColumnDescription = "低报约束", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string LRestrainExpressions { get => lRestrainExpressions; set => lRestrainExpressions = value; }

    /// <summary>
    /// 低报文本
    /// </summary>
    [SugarColumn(ColumnDescription = "低报文本", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string LAlarmText { get => lAlarmText; set => lAlarmText = value; }

    /// <summary>
    /// 低限值
    /// </summary>
    [SugarColumn(ColumnDescription = "低限值", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public decimal LAlarmCode { get => lAlarmCode; set => lAlarmCode = value; }

    /// <summary>
    /// 低低报使能
    /// </summary>
    [SugarColumn(ColumnDescription = "低低报使能")]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public bool LLAlarmEnable { get => lLAlarmEnable; set => lLAlarmEnable = value; }

    /// <summary>
    /// 低低报约束
    /// </summary>
    [SugarColumn(ColumnDescription = "低低报约束", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string LLRestrainExpressions { get => lLRestrainExpressions; set => lLRestrainExpressions = value; }

    /// <summary>
    /// 低低报文本
    /// </summary>
    [SugarColumn(ColumnDescription = "低低报文本", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string LLAlarmText { get => lLAlarmText; set => lLAlarmText = value; }

    /// <summary>
    /// 低低限值
    /// </summary>
    [SugarColumn(ColumnDescription = "低低限值", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public decimal LLAlarmCode { get => lLAlarmCode; set => lLAlarmCode = value; }

    /// <summary>
    /// 自定义报警使能
    /// </summary>
    [SugarColumn(ColumnDescription = "自定义报警使能")]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public bool CustomAlarmEnable { get => customAlarmEnable; set => customAlarmEnable = value; }

    /// <summary>
    /// 自定义报警条件约束
    /// </summary>
    [SugarColumn(ColumnDescription = "自定义报警条件约束", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string CustomRestrainExpressions { get => customRestrainExpressions; set => customRestrainExpressions = value; }

    /// <summary>
    /// 自定义文本
    /// </summary>
    [SugarColumn(ColumnDescription = "自定义文本", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string CustomAlarmText { get => customAlarmText; set => customAlarmText = value; }

    /// <summary>
    /// 自定义报警条件
    /// </summary>
    [SugarColumn(ColumnDescription = "自定义报警条件", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string CustomAlarmCode { get => customAlarmCode; set => customAlarmCode = value; }

    #endregion 报警



    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {

        if (HHAlarmEnable && HAlarmEnable && HHAlarmCode <= HAlarmCode)
        {
            yield return new ValidationResult("HHAlarmCode must be greater than HAlarmCode", new[] { nameof(HHAlarmCode), nameof(HAlarmCode) });
        }
        if (HAlarmEnable && LAlarmEnable && HAlarmCode <= LAlarmCode)
        {
            yield return new ValidationResult("HAlarmCode must be greater than LAlarmCode", new[] { nameof(HAlarmCode), nameof(LAlarmCode) });
        }
        if (LAlarmEnable && LLAlarmEnable && LAlarmCode <= LLAlarmCode)
        {
            yield return new ValidationResult("LAlarmCode must be greater than LLAlarmCode", new[] { nameof(LAlarmCode), nameof(LLAlarmCode) });
        }

        if (HHAlarmEnable && LAlarmEnable && HHAlarmCode <= LAlarmCode)
        {
            yield return new ValidationResult("HHAlarmCode should be greater than or less than LAlarmCode", new[] { nameof(HHAlarmCode), nameof(LAlarmCode) });
        }
        if (HHAlarmEnable && LLAlarmEnable && HHAlarmCode <= LLAlarmCode)
        {
            yield return new ValidationResult("HHAlarmCode should be greater than or less than LLAlarmCode", new[] { nameof(HHAlarmCode), nameof(LLAlarmCode) });
        }
        if (HAlarmEnable && LLAlarmEnable && HAlarmCode <= LLAlarmCode)
        {
            yield return new ValidationResult("HAlarmCode should be greater than or less than LLAlarmCode", new[] { nameof(HAlarmCode), nameof(LLAlarmCode) });
        }
    }
}
