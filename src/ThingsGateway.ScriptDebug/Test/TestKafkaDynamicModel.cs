using System.Collections.Concurrent;

using ThingsGateway.Foundation;
using ThingsGateway.Gateway.Application;
using ThingsGateway.NewLife.Extension;
public class TestKafkaDynamicModel1 : DynamicModelBase
{
    private Dictionary<string, VariableRuntime> variableRuntimes = new();

    private long id = 0;

    public TestKafkaDynamicModel1()
    {
        var name = "测试MqttServer";
        if (GlobalData.ReadOnlyDevices.TryGetValue(name, out var kafka1))
        {
            id = kafka1.Id;

            foreach (var item in kafka1.Driver?.IdVariableRuntimes)
            {
                //变量备注1作为Key(AE报警SourceId)
                var data1 = item.Value.GetPropertyValue(id, nameof(BusinessVariableProperty.Data1));
                if (!data1.IsNullOrEmpty())
                {
                    variableRuntimes.Add(data1, item.Value);
                }
            }

        }
        else
        {
            throw new Exception($"找不到设备 {name}");
        }

    }

    private ConcurrentDictionary<Tuple<string, string, int>, DateTime> EventKeyTimes = new();

    public override IEnumerable<dynamic> GetList(IEnumerable<object> datas)
    {
        if (datas == null) return null;
        var pluginEventDatas = datas.Cast<PluginEventData>();
        var opcDatas = pluginEventDatas.Select(
            a =>
            {
                if (a.ObjectValue == null)
                {
                    a.ObjectValue = a.Value.ToObject(Type.GetType(a.ValueType));
                }
                return a.ObjectValue is ThingsGateway.Plugin.OpcAe.OpcAeEventData opcData ? opcData : null;
            }
            ).Where(a => a != null).ToList();

        List<KafkaAlarmEntity> alarmEntities = new List<KafkaAlarmEntity>();
        if (opcDatas.Count == 0)
        {
            Logger?.LogInformation("没有OPCAE数据");
            return alarmEntities;
        }


        foreach (var opcAeEventData in opcDatas)
        {
            //一般只需要条件报警
            //if (opcAeEventData.EventType != Opc.Ae.EventType.Condition)
            //    continue;
            //重连时触发的事件，可以跳过不处理
            //if(opcAeEventData.Refresh)
            //    continue;
            var sourceName = opcAeEventData.SourceID;
            if (variableRuntimes.TryGetValue(sourceName, out var variableRuntime))
            {
                
                var ack = opcAeEventData.EventType != Opc.Ae.EventType.Condition ? false : ((Opc.Ae.ConditionState)opcAeEventData.NewState).HasFlag(Opc.Ae.ConditionState.Acknowledged);

                bool isRecover = opcAeEventData.EventType != Opc.Ae.EventType.Condition ? false : !((Opc.Ae.ConditionState)opcAeEventData.NewState).HasFlag(Opc.Ae.ConditionState.Active);
                if (opcAeEventData.EventType != Opc.Ae.EventType.Condition)
                {
                    bool alarm = (opcAeEventData.Message).Contains("raised");
                    if (alarm)
                    {
                        opcAeEventData.AlarmTime = opcAeEventData.Time;
                        EventKeyTimes.AddOrUpdate(Tuple.Create(opcAeEventData.SourceID, opcAeEventData.ConditionName, opcAeEventData.Cookie), opcAeEventData.Time, (k, v) => opcAeEventData.Time);
                    }
                    else
                    {
                        if (EventKeyTimes.TryGetValue(Tuple.Create(opcAeEventData.SourceID, opcAeEventData.ConditionName, opcAeEventData.Cookie), out var time))
                            opcAeEventData.AlarmTime = time;
                        else
                            opcAeEventData.AlarmTime = opcAeEventData.Time;
                    }
                    isRecover = !alarm;
                }

                //构建告警实体
                KafkaAlarmEntity alarmEntity = new KafkaAlarmEntity
                {
                    alarmCode = variableRuntime.GetPropertyValue(id, nameof(BusinessVariableProperty.Data2)), //唯一编码
                    resourceCode = variableRuntime.GetPropertyValue(id, nameof(BusinessVariableProperty.Data3)), //资源编码
                    resourceName = variableRuntime.GetPropertyValue(id, nameof(BusinessVariableProperty.Data4)), //资源名称  
                    metricCode = variableRuntime.GetPropertyValue(id, nameof(BusinessVariableProperty.Data5)), //指标编码
                    metricName = variableRuntime.GetPropertyValue(id, nameof(BusinessVariableProperty.Data6)), //指标名称
                    content = $"{variableRuntime.GetPropertyValue(id, nameof(BusinessVariableProperty.Data4))}{variableRuntime.GetPropertyValue(id, nameof(BusinessVariableProperty.Data6))}{opcAeEventData.Message}", //告警内容，设备名称+告警内容（包含阈值信息），可能opcae里没有带阈值信息，那么就需要录入固定值，可选Data10
                    alarmType = variableRuntime.GetPropertyValue(id, nameof(BusinessVariableProperty.Data7)), // opcAeEventData.Severity 告警类型，子系统产生告警的类型，可能需要固定备注值

                    confirmedTime = ack ? opcAeEventData.Time.DateTimeToUnixTimestamp() : null, //告警确认时间
                    fixTime = isRecover ? opcAeEventData.Time : null, //解除告警时间
                    lastTime = opcAeEventData.AlarmTime, //产生告警时间
                    status = isRecover ? "FIXED" : "UNFIXED", //告警状态
                    alarmLevel = variableRuntime.GetPropertyValue(id, nameof(BusinessVariableProperty.Data8)), //opcAeEventData.Severity.ToString(), //告警等级,可能需要固定备注值
                    subSystemCode = variableRuntime.GetPropertyValue(id, nameof(BusinessVariableProperty.Data9)), //子系统编码
                    type = "SUB_SYSTEM_ALARM", //默认填写字段
                    confirmAccount = opcAeEventData.ActorID, //告警确认人
                    clearAccount = opcAeEventData.ActorID, //告警清除人
                    processInstruction = null //告警处理说明，OPCAE不带有
                };
                alarmEntities.Add(alarmEntity);
            }
            else
            {
                Logger?.LogInformation($"找不到相关变量{sourceName}");
            }
        }

        return alarmEntities;
    }
}


/// <summary>
/// 告警实体
/// </summary>
public class KafkaAlarmEntity
{
    /// <summary>
    /// 告警编码唯一 (非空)
    /// 示例："8e8a118ac452fd04da8c26fa588a7cab"
    /// </summary>
    public string alarmCode { get; set; }

    /// <summary>
    /// 资源编码，唯一编码，需要按照映射表上传 (非空)
    /// 示例："RS_A6K9MUSG19V"
    /// </summary>
    public string resourceCode { get; set; }

    /// <summary>
    /// 资源名称，需要按照映射表上传 (非空)
    /// 示例："MB-A7"
    /// </summary>
    public string resourceName { get; set; }

    /// <summary>
    /// 指标编码唯一，需要按照映射表上传 (非空)
    /// 示例："ActivePowerPa"
    /// </summary>
    public string metricCode { get; set; }

    /// <summary>
    /// 指标名称，需要按照映射表上传 (非空)
    /// 示例："有功功率Pa"
    /// </summary>
    public string metricName { get; set; }

    /// <summary>
    /// 告警内容：设备名称+告警内容（包含阈值信息） (非空)
    /// 示例："MB-A7，有功功率Pa > 30"
    /// </summary>
    public string content { get; set; }

    /// <summary>
    /// 告警类型，子系统产生告警的类型 (非空)
    /// 示例："0101" 表示高限报警
    /// </summary>
    public string alarmType { get; set; }

    /// <summary>
    /// 告警确认时间 (可空，时间戳)
    /// 示例：1586152800000
    /// </summary>
    public long? confirmedTime { get; set; }

    /// <summary>
    /// 解除告警时间 (可空，时间戳)
    /// 示例：1586152800000
    /// </summary>
    public DateTime? fixTime { get; set; }

    /// <summary>
    /// 产生告警时间 (非空，时间戳)
    /// 示例：1586152800000
    /// </summary>
    public DateTime lastTime { get; set; }

    /// <summary>
    /// 告警状态 (非空)
    /// 可选值：UNFIXED（新增告警）、FIXED（解除告警）
    /// </summary>
    public string status { get; set; }

    /// <summary>
    /// 告警等级，需要按照映射表上传 (非空)
    /// 示例："1"
    /// </summary>
    public string alarmLevel { get; set; }

    /// <summary>
    /// 子系统编码 (非空)
    /// 示例："MS_NEW_PD_DCIM_001"
    /// </summary>
    public string subSystemCode { get; set; }

    /// <summary>
    /// 默认填写字段 (非空)
    /// 固定值："SUB_SYSTEM_ALARM"
    /// </summary>
    public string type { get; set; }

    /// <summary>
    /// 告警确认人 (可空)
    /// 示例："admin3"
    /// </summary>
    public string confirmAccount { get; set; }

    /// <summary>
    /// 告警清除人 (可空)
    /// 示例："admin"
    /// </summary>
    public string clearAccount { get; set; }

    /// <summary>
    /// 告警处理说明 (可空)
    /// 示例："admin"
    /// </summary>
    public string processInstruction { get; set; }
}