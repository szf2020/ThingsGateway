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

namespace ThingsGateway.Gateway.Application;


public partial class AlarmRuntimePropertys
{

    /// <summary>
    /// 事件类型
    /// </summary>
    [AutoGenerateColumn(Visible = false)]
    public EventTypeEnum? EventType { get; set; }

    /// <summary>
    /// 报警类型
    /// </summary>
    [AutoGenerateColumn(Visible = false)]
    public AlarmTypeEnum? AlarmType { get; set; }

    /// <summary>
    /// 报警值
    /// </summary>
    [AutoGenerateColumn(Visible = false)]
    public string AlarmCode { get; set; }

    /// <summary>
    /// 恢复值
    /// </summary>
    [AutoGenerateColumn(Visible = false)]
    public string RecoveryCode { get; set; }


    /// <summary>
    /// 事件时间
    /// </summary>
    [AutoGenerateColumn(Ignore = true)]
    internal DateTime? PrepareAlarmEventTime { get; set; }
    /// <summary>
    /// 事件时间
    /// </summary>
    [AutoGenerateColumn(Ignore = true)]
    internal DateTime? PrepareFinishEventTime { get; set; }


    /// <summary>
    /// 报警时间
    /// </summary>
    [AutoGenerateColumn(Visible = false)]
    public DateTime AlarmTime { get; set; } = DateTime.UnixEpoch.ToLocalTime();

    [AutoGenerateColumn(Visible = false)]
    public DateTime FinishTime { get; set; } = DateTime.UnixEpoch.ToLocalTime();

    [AutoGenerateColumn(Visible = false)]
    public DateTime ConfirmTime { get; set; } = DateTime.UnixEpoch.ToLocalTime();


    /// <summary>
    /// 报警限值
    /// </summary>
    [AutoGenerateColumn(Visible = false)]
    public string AlarmLimit { get; set; }

    /// <summary>
    /// 报警文本
    /// </summary>
    [AutoGenerateColumn(Visible = false)]
    public string AlarmText { get; set; }


    [AutoGenerateColumn(Visible = false)]
    public DateTime EventTime { get; set; } = DateTime.UnixEpoch.ToLocalTime();

    internal object AlarmLockObject = new();

    internal bool AlarmConfirm;
}
