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

using Riok.Mapperly.Abstractions;

using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace ThingsGateway.Gateway.Application;
#pragma warning disable CS0649

/// <summary>
/// 设备变量表
/// </summary>
#if !Management
[SugarTable("variable", TableDescription = "设备变量表")]
[Tenant(SqlSugarConst.DB_Custom)]
[SugarIndex("index_device", nameof(Variable.DeviceId), OrderByType.Asc)]
[SugarIndex("unique_deviceid_variable_name", nameof(Variable.Name), OrderByType.Asc, nameof(Variable.DeviceId), OrderByType.Asc, true)]
#endif
public class Variable : BaseDataEntity, IValidatableObject
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [SugarColumn(ColumnDescription = "Id", IsPrimaryKey = true)]
    [AutoGenerateColumn(Visible = false, IsVisibleWhenEdit = false, IsVisibleWhenAdd = false, Sortable = true, DefaultSort = true, DefaultSortOrder = SortOrder.Asc)]
    [System.ComponentModel.DataAnnotations.Key]
    public override long Id { get; set; }

    /// <summary>
    /// 导入验证专用
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    internal long Row;
    private double hAlarmCode = 50;
    private double lAlarmCode = 10;
    private double hHAlarmCode = 90;
    private double lLAlarmCode = 0;
    private long deviceId;
    private int? arrayLength;
    private int alarmDelay;
    private int alarmLevel;
    private ProtectTypeEnum protectType = ProtectTypeEnum.ReadWrite;
    private DataTypeEnum dataType = DataTypeEnum.Int16;

    /// <summary>
    /// 导入验证专用
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    internal bool IsUp;
    private bool enable = true;
    public bool DynamicVariable;
    private bool rpcWriteEnable = true;
    private bool saveValue = false;
    private bool boolOpenAlarmEnable;
    private bool boolCloseAlarmEnable;
    private bool hAlarmEnable;
    private bool hHAlarmEnable;
    private bool lLAlarmEnable;
    private bool lAlarmEnable;
    private bool customAlarmEnable;
    private bool businessGroupUpdateTrigger = true;
    private bool rpcWriteCheck;

    private object _value;
    private string name;
    private string collectGroup = string.Empty;
    private string businessGroup;
    private string description;
    private string unit;
    private string intervalTime;
    private string registerAddress;
    private string otherMethod;
    private string readExpressions;
    private string writeExpressions;
    private string boolOpenRestrainExpressions;
    private string boolOpenAlarmText;
    private string boolCloseRestrainExpressions;
    private string boolCloseAlarmText;
    private string hRestrainExpressions;
    private string hAlarmText;
    private Dictionary<long, Dictionary<string, string>>? variablePropertys;
    private string hHRestrainExpressions;
    private string hHAlarmText;
    private string lRestrainExpressions;
    private string lAlarmText;

    private string lLRestrainExpressions;
    private string lLAlarmText;
    private string customRestrainExpressions;
    private string customAlarmText;
    private string customAlarmCode;
    private string remark1;
    private string remark2;
    private string remark3;
    private string remark4;
    private string remark5;

    /// <summary>
    /// 变量额外属性Json
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [MapperIgnore]
    public ConcurrentDictionary<long, ModelValueValidateForm>? VariablePropertyModels;

    /// <summary>
    /// 设备
    /// </summary>
    [SugarColumn(ColumnDescription = "设备")]
    [AutoGenerateColumn(Visible = true, Order = 1, Filterable = false, Sortable = false)]
    [IgnoreExcel]
    [Required]
    [NotNull]
    [MinValue(1)]
    public virtual long DeviceId { get => deviceId; set => deviceId = value; }

    /// <summary>
    /// 变量名称
    /// </summary>
    [SugarColumn(ColumnDescription = "变量名称", IsNullable = false)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 1)]
    [Required]
    public virtual string Name { get => name; set => name = value; }

    /// <summary>
    /// 采集组
    /// </summary>
    [SugarColumn(ColumnDescription = "采集组", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 1)]
    public virtual string CollectGroup { get => collectGroup; set => collectGroup = value; }
    /// <summary>
    /// 分组名称
    /// </summary>
    [SugarColumn(ColumnDescription = "分组名称", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 1)]
    public virtual string BusinessGroup { get => businessGroup; set => businessGroup = value; }

    /// <summary>
    /// 分组上传触发变量
    /// </summary>
    [SugarColumn(ColumnDescription = "分组上传触发变量", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 1)]
    public virtual bool BusinessGroupUpdateTrigger { get => businessGroupUpdateTrigger; set => businessGroupUpdateTrigger = value; }

    /// <summary>
    /// 写入后再次读取检查值是否一致
    /// </summary>
    [SugarColumn(ColumnDescription = "写入后再次读取检查值是否一致", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 1)]
    public virtual bool RpcWriteCheck { get => rpcWriteCheck; set => rpcWriteCheck = value; }

    /// <summary>
    /// 描述
    /// </summary>
    [SugarColumn(ColumnDescription = "描述", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 2)]
    public string Description { get => description; set => description = value; }

    /// <summary>
    /// 单位
    /// </summary>
    [SugarColumn(ColumnDescription = "单位", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 3)]
    public virtual string Unit { get => unit; set => unit = value; }

    /// <summary>
    /// 间隔时间
    /// </summary>
    [SugarColumn(ColumnDescription = "间隔时间", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual string IntervalTime { get => intervalTime; set => intervalTime = value; }

    /// <summary>
    /// 变量地址，可能带有额外的信息，比如<see cref="DataFormatEnum"/> ，以;分割
    /// </summary>
    [SugarColumn(ColumnDescription = "变量地址", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public string RegisterAddress { get => registerAddress; set => registerAddress = value; }

    /// <summary>
    /// 数组长度
    /// </summary>
    [SugarColumn(ColumnDescription = "数组长度", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public int? ArrayLength { get => arrayLength; set => arrayLength = value; }

    /// <summary>
    /// 其他方法，若不为空，此时RegisterAddress为方法参数
    /// </summary>
    [SugarColumn(ColumnDescription = "特殊方法", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public string OtherMethod { get => otherMethod; set => otherMethod = value; }

    /// <summary>
    /// 使能
    /// </summary>
    [SugarColumn(ColumnDescription = "使能")]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual bool Enable { get => enable; set => enable = value; }
    /// <summary>
    /// 读写权限
    /// </summary>
    [SugarColumn(ColumnDescription = "读写权限", IsNullable = false)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual ProtectTypeEnum ProtectType { get => protectType; set => protectType = value; }
    /// <summary>
    /// 数据类型
    /// </summary>
    [SugarColumn(ColumnDescription = "数据类型")]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual DataTypeEnum DataType { get => dataType; set => dataType = value; }
    /// <summary>
    /// 读取表达式
    /// </summary>
    [SugarColumn(ColumnDescription = "读取表达式", Length = 1000, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual string ReadExpressions { get => readExpressions; set => readExpressions = value; }

    /// <summary>
    /// 写入表达式
    /// </summary>
    [SugarColumn(ColumnDescription = "写入表达式", Length = 1000, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual string WriteExpressions { get => writeExpressions; set => writeExpressions = value; }

    /// <summary>
    /// 是否允许远程Rpc写入
    /// </summary>
    [SugarColumn(ColumnDescription = "远程写入", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual bool RpcWriteEnable { get => rpcWriteEnable; set => rpcWriteEnable = value; }
    /// <summary>
    /// 初始值
    /// </summary>
    [SugarColumn(IsJson = true, ColumnDataType = StaticConfig.CodeFirst_BigString, ColumnDescription = "初始值", IsNullable = true)]
    [AutoGenerateColumn(Ignore = true)]
    public object InitValue
    {
        get
        {
            return _value;
        }
        set
        {
            if (value != null)
                _value = value?.ToString()?.GetJTokenFromString();
            else
                _value = null;
        }
    }

    /// <summary>
    /// 保存初始值
    /// </summary>
    [SugarColumn(ColumnDescription = "保存初始值", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual bool SaveValue { get => saveValue; set => saveValue = value; }
    /// <summary>
    /// 变量额外属性Json
    /// </summary>
    [SugarColumn(IsJson = true, ColumnDataType = StaticConfig.CodeFirst_BigString, ColumnDescription = "变量属性Json", IsNullable = true)]
    [IgnoreExcel]
    [AutoGenerateColumn(Ignore = true)]
    public Dictionary<long, Dictionary<string, string>>? VariablePropertys { get => variablePropertys; set => variablePropertys = value; }

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
    public double HAlarmCode { get => hAlarmCode; set => hAlarmCode = value; }

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
    public double HHAlarmCode { get => hHAlarmCode; set => hHAlarmCode = value; }

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
    public double LAlarmCode { get => lAlarmCode; set => lAlarmCode = value; }

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
    public double LLAlarmCode { get => lLAlarmCode; set => lLAlarmCode = value; }

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

    #region 备用字段

    /// <summary>
    /// 自定义
    /// </summary>
    [SugarColumn(ColumnDescription = "自定义1", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string Remark1 { get => remark1; set => remark1 = value; }

    /// <summary>
    /// 自定义
    /// </summary>
    [SugarColumn(ColumnDescription = "自定义2", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string Remark2 { get => remark2; set => remark2 = value; }

    /// <summary>
    /// 自定义
    /// </summary>
    [SugarColumn(ColumnDescription = "自定义3", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string Remark3 { get => remark3; set => remark3 = value; }

    /// <summary>
    /// 自定义
    /// </summary>
    [SugarColumn(ColumnDescription = "自定义4", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string Remark4 { get => remark4; set => remark4 = value; }

    /// <summary>
    /// 自定义
    /// </summary>
    [SugarColumn(ColumnDescription = "自定义5", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string Remark5 { get => remark5; set => remark5 = value; }

    #endregion 备用字段

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(RegisterAddress) && string.IsNullOrEmpty(OtherMethod))
        {
            yield return new ValidationResult("Both RegisterAddress and OtherMethod cannot be empty or null.", new[] { nameof(OtherMethod), nameof(RegisterAddress) });
        }

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
