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

using Mapster;

using ThingsGateway.SqlSugar;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 变量运行态
/// </summary>
public partial class VariableRuntime : Variable, IVariable, IDisposable
{




    #region 属性
    /// <summary>
    /// 事件类型
    /// </summary>
    [AutoGenerateColumn(Visible = false)]
    public EventTypeEnum? EventType { get => eventType; set => eventType = value; }

    /// <summary>
    /// 报警类型
    /// </summary>
    [AutoGenerateColumn(Visible = false)]
    public AlarmTypeEnum? AlarmType { get => alarmType; set => alarmType = value; }


    /// <summary>
    /// 报警值
    /// </summary>
    [AutoGenerateColumn(Visible = false)]
    public string AlarmCode { get => alarmCode; set => alarmCode = value; }


    /// <summary>
    /// 恢复值
    /// </summary>
    [AutoGenerateColumn(Visible = false)]
    public string RecoveryCode { get => recoveryCode; set => recoveryCode = value; }

    /// <summary>
    /// 这个参数值由自动打包方法写入<see cref="IDevice.LoadSourceRead{T}(IEnumerable{IVariable}, int, string)"/>
    /// </summary>
    [AutoGenerateColumn(Visible = false)]
    public int Index { get => index; set => index = value; }
    /// <summary>
    /// 事件时间
    /// </summary>
    [AutoGenerateColumn(Ignore = true)]
    internal DateTime? PrepareEventTime { get => prepareEventTime; set => prepareEventTime = value; }

    /// <summary>
    /// 变化时间
    /// </summary>
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 5)]
    public DateTime ChangeTime { get => changeTime; set => changeTime = value; }

    /// <summary>
    /// 报警时间
    /// </summary>
    [AutoGenerateColumn(Visible = false)]
    public DateTime AlarmTime { get => alarmTime; set => alarmTime = value; }


    /// <summary>
    /// 事件时间
    /// </summary>
    [AutoGenerateColumn(Visible = false)]
    public DateTime EventTime { get => eventTime; set => eventTime = value; }


    /// <summary>
    /// 采集时间
    /// </summary>
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 5)]
    public DateTime CollectTime { get => collectTime; set => collectTime = value; }


    [SugarColumn(ColumnDescription = "排序码", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, DefaultSort = false, Sortable = true)]
    [IgnoreExcel]
    public override int SortCode { get => sortCode; set => sortCode = value; }


    /// <summary>
    /// 上次值
    /// </summary>
    [AutoGenerateColumn(Visible = false, Order = 6)]
    public object LastSetValue { get => lastSetValue; set => lastSetValue = value; }

    /// <summary>
    /// 原始值
    /// </summary>
    [AutoGenerateColumn(Visible = false, Order = 6)]
    public object RawValue { get => rawValue; set => rawValue = value; }



    /// <summary>
    /// 所在采集设备
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [AutoGenerateColumn(Ignore = true)]
    public DeviceRuntime DeviceRuntime { get => deviceRuntime; set => deviceRuntime = value; }

    /// <summary>
    /// VariableSource
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [AdaptIgnore]
    [AutoGenerateColumn(Ignore = true)]
    public IVariableSource VariableSource { get => variableSource; set => variableSource = value; }

    /// <summary>
    /// VariableMethod
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [AdaptIgnore]
    [AutoGenerateColumn(Ignore = true)]
    public VariableMethod VariableMethod { get => variableMethod; set => variableMethod = value; }



    /// <summary>
    /// 这个参数值由自动打包方法写入<see cref="IDevice.LoadSourceRead{T}(IEnumerable{IVariable}, int, string)"/>
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [AutoGenerateColumn(Ignore = true)]
    public IThingsGatewayBitConverter ThingsGatewayBitConverter { get => thingsGatewayBitConverter; set => thingsGatewayBitConverter = value; }




    /// <summary>
    /// 设备名称
    /// </summary>
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 4)]
    public string DeviceName => DeviceRuntime?.Name;

    /// <summary>
    /// 是否在线
    /// </summary>
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 5)]
    public bool IsOnline
    {
        get
        {
            return _isOnline;
        }
        private set
        {
            if (IsOnline != value)
            {
                _isOnlineChanged = true;
            }
            else
            {
                _isOnlineChanged = false;
            }
            _isOnline = value;
        }
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 5)]
    public string LastErrorMessage
    {
        get
        {
            if (_isOnline == false)
                return _lastErrorMessage ?? VariableSource?.LastErrorMessage ?? VariableMethod?.LastErrorMessage;
            else
                return null;
        }
    }


    /// <summary>
    /// 实时值
    /// </summary>
    [AutoGenerateColumn(Visible = true, Order = 6)]
    public string RuntimeType => Value?.GetType()?.ToString();

    /// <summary>
    /// 实时值
    /// </summary>
    [AutoGenerateColumn(Visible = true, Order = 6)]
    public object Value { get => _value; set => _value = value; }






    /// <summary>
    /// 报警使能
    /// </summary>
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public bool AlarmEnable
    {
        get
        {
            return LAlarmEnable || LLAlarmEnable || HAlarmEnable || HHAlarmEnable || BoolOpenAlarmEnable || BoolCloseAlarmEnable || CustomAlarmEnable;
        }
    }

    /// <summary>
    /// 报警限值
    /// </summary>
    [AutoGenerateColumn(Visible = false)]
    public string AlarmLimit { get => alarmLimit; set => alarmLimit = value; }

    /// <summary>
    /// 报警文本
    /// </summary>
    [AutoGenerateColumn(Visible = false)]
    public string AlarmText { get => alarmText; set => alarmText = value; }

    #endregion

}

