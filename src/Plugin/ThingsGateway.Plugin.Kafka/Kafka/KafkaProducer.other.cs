//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Confluent.Kafka;

using PooledAwait;

using ThingsGateway.Extension.Generic;
using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Extension.Generic;

using TouchSocket.Core;

namespace ThingsGateway.Plugin.Kafka;

/// <summary>
/// Kafka消息生产
/// </summary>
public partial class KafkaProducer : BusinessBaseWithCacheIntervalScriptAll
{
#if !Management
    private ProducerBuilder<Null, byte[]> _producerBuilder;
    private ProducerConfig _producerconfig;
    private volatile bool producerSuccess = true;




    protected override void PluginChange(PluginEventData pluginEventData)
    {
        if (!_businessPropertyWithCacheIntervalScript.PluginEventDataTopic.IsNullOrWhiteSpace())
            AddQueuePluginDataModel(new(pluginEventData));
        base.PluginChange(pluginEventData);
    }
    protected override ValueTask<OperResult> UpdatePluginEventDataModel(List<CacheDBItem<PluginEventData>> item, CancellationToken cancellationToken)
    {
        return UpdatePluginEventDataModel(item.Select(a => a.Value), cancellationToken);
    }
    private ValueTask<OperResult> UpdatePluginEventDataModel(IEnumerable<PluginEventData> item, CancellationToken cancellationToken)
    {
        var topicArrayList = GetPluginEventDataTopicArrays(item);
        return Update(topicArrayList, cancellationToken);
    }



    protected override void AlarmChange(AlarmVariable alarmVariable)
    {
        if (!_businessPropertyWithCacheIntervalScript.AlarmTopic.IsNullOrWhiteSpace())
            AddQueueAlarmModel(new(alarmVariable));
        base.AlarmChange(alarmVariable);
    }

    protected override void DeviceTimeInterval(DeviceRuntime deviceRunTime, DeviceBasicData deviceData)
    {
        if (!_businessPropertyWithCacheIntervalScript.DeviceTopic.IsNullOrWhiteSpace())
            AddQueueDevModel(new(deviceData));
        base.DeviceChange(deviceRunTime, deviceData);
    }

    protected override void DeviceChange(DeviceRuntime deviceRunTime, DeviceBasicData deviceData)
    {
        if (!_businessPropertyWithCacheIntervalScript.DeviceTopic.IsNullOrWhiteSpace())
            AddQueueDevModel(new(deviceData));
        base.DeviceChange(deviceRunTime, deviceData);
    }

    protected override ValueTask<OperResult> UpdateAlarmModel(List<CacheDBItem<AlarmVariable>> item, CancellationToken cancellationToken)
    {
        return UpdateAlarmModel(item.Select(a => a.Value).OrderBy(a => a.Id), cancellationToken);
    }

    protected override ValueTask<OperResult> UpdateDevModel(List<CacheDBItem<DeviceBasicData>> item, CancellationToken cancellationToken)
    {
        return UpdateDevModel(item.Select(a => a.Value).OrderBy(a => a.Id), cancellationToken);
    }

    protected override ValueTask<OperResult> UpdateVarModel(List<CacheDBItem<VariableBasicData>> item, CancellationToken cancellationToken)
    {
        return UpdateVarModel(item.Select(a => a.Value).OrderBy(a => a.Id), cancellationToken);
    }
    protected override ValueTask<OperResult> UpdateVarModels(List<VariableBasicData> item, CancellationToken cancellationToken)
    {
        return UpdateVarModel(item, cancellationToken);
    }

    protected override void VariableTimeInterval(IEnumerable<VariableRuntime> variableRuntimes, IEnumerable<VariableBasicData> variables)
    {
        TimeIntervalUpdateVariable(variables);
        base.VariableTimeInterval(variableRuntimes, variables);
    }
    protected override void VariableChange(VariableRuntime variableRuntime, VariableBasicData variable)
    {
        UpdateVariable(variableRuntime, variable);
        base.VariableChange(variableRuntime, variable);
    }
    private void TimeIntervalUpdateVariable(IEnumerable<VariableBasicData> variables)
    {
        if (!_businessPropertyWithCacheIntervalScript.VariableTopic.IsNullOrWhiteSpace())
        {
            if (_driverPropertys.GroupUpdate)
            {
                var data = variables is System.Collections.IList ? variables : variables.ToArray();
                var varList = data.Where(a => a.BusinessGroup.IsNullOrEmpty());
                var varGroup = data.Where(a => !a.BusinessGroup.IsNullOrEmpty()).GroupBy(a => a.BusinessGroup);

                foreach (var group in varGroup)
                {
                    AddQueueVarModel(new CacheDBItem<List<VariableBasicData>>(group.ToList()));
                }
                foreach (var variable in varList)
                {
                    AddQueueVarModel(new CacheDBItem<VariableBasicData>(variable));
                }
            }
            else
            {
                foreach (var variable in variables)
                {
                    AddQueueVarModel(new CacheDBItem<VariableBasicData>(variable));
                }
            }
        }
    }

    private void UpdateVariable(VariableRuntime variableRuntime, VariableBasicData variable)
    {
        if (!_businessPropertyWithCacheIntervalScript.VariableTopic.IsNullOrWhiteSpace())
        {
            if (_driverPropertys.GroupUpdate && variable.BusinessGroupUpdateTrigger && !variable.BusinessGroup.IsNullOrEmpty() && VariableRuntimeGroups.TryGetValue(variable.BusinessGroup, out var variableRuntimeGroup))
            {
                AddQueueVarModel(new CacheDBItem<List<VariableBasicData>>(variableRuntimeGroup.AdaptListVariableBasicData()));
            }
            else
            {
                AddQueueVarModel(new CacheDBItem<VariableBasicData>(variable));
            }
        }
    }

    #region private

    private ValueTask<OperResult> Update(IEnumerable<TopicArray> topicArrayList, CancellationToken cancellationToken)
    {
        return Update(this, topicArrayList, cancellationToken);

        static async PooledValueTask<OperResult> Update(KafkaProducer @this, IEnumerable<TopicArray> topicArrayList, CancellationToken cancellationToken)
        {
            foreach (var topicArray in topicArrayList)
            {
                var result = await @this.KafKaUpAsync(topicArray, cancellationToken).ConfigureAwait(false);
                if (@this.success != result.IsSuccess)
                {
                    if (!result.IsSuccess)
                    {
                        @this.LogMessage?.LogWarning(result.ToString());
                    }
                    @this.success = result.IsSuccess;
                }
                if (!result.IsSuccess)
                {
                    return result;
                }
            }
            return OperResult.Success;
        }
    }

    private ValueTask<OperResult> UpdateAlarmModel(IEnumerable<AlarmVariable> item, CancellationToken cancellationToken)
    {
        var topicArrayList = GetAlarmTopicArrays(item);
        return Update(topicArrayList, cancellationToken);
    }

    private ValueTask<OperResult> UpdateDevModel(IEnumerable<DeviceBasicData> item, CancellationToken cancellationToken)
    {
        var topicArrayList = GetDeviceTopicArray(item);
        return Update(topicArrayList, cancellationToken);
    }

    private ValueTask<OperResult> UpdateVarModel(IEnumerable<VariableBasicData> item, CancellationToken cancellationToken)
    {
        var topicArrayList = GetVariableBasicDataTopicArray(item.WhereIf(_driverPropertys.OnlineFilter, a => a.IsOnline == true));
        return Update(topicArrayList, cancellationToken);
    }

    #endregion private

    #region 方法


    private async Task AllPublishAsync(CancellationToken cancellationToken)
    {
        //保留消息
        //分解List，避免超出字节大小限制
        var varData = this.IdVariableRuntimes.Select(a => a.Value).AdaptIEnumerableVariableBasicData().ChunkBetter(this._driverPropertys.SplitSize);
        var devData = this.CollectDevices?.Select(a => a.Value).AdaptIEnumerableDeviceBasicData().ChunkBetter(this._driverPropertys.SplitSize);
        var alramData = GlobalData.ReadOnlyRealAlarmIdVariables.Select(a => a.Value).ChunkBetter(this._driverPropertys.SplitSize);
        foreach (var item in varData)
        {
            if (!this.success)
                break;
            await this.UpdateVarModel(item, cancellationToken).ConfigureAwait(false);
        }
        if (devData != null)
        {
            foreach (var item in devData)
            {
                if (!this.success)
                    break;
                await this.UpdateDevModel(item, cancellationToken).ConfigureAwait(false);
            }
        }
        foreach (var item in alramData)
        {
            if (!this.success)
                break;
            await this.UpdateAlarmModel(item, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// kafka上传，返回上传结果
    /// </summary>
    public ValueTask<OperResult> KafKaUpAsync(TopicArray topicArray, CancellationToken cancellationToken)
    {
        return KafKaUpAsync(this, topicArray, cancellationToken);

        static async PooledValueTask<OperResult> KafKaUpAsync(KafkaProducer @this, TopicArray topicArray, CancellationToken cancellationToken)
        {
            try
            {
                using CancellationTokenSource cancellationTokenSource = new(@this._driverPropertys.Timeout);
                using CancellationTokenSource stoppingToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, cancellationToken);
                var result = await @this._producer.ProduceAsync(topicArray.Topic, new Message<Null, byte[]> { Value = topicArray.Payload }, stoppingToken.Token).ConfigureAwait(false);
                if (result.Status != PersistenceStatus.Persisted)
                {
                    return new OperResult("Upload fail");
                }
                else
                {
                    if (@this._driverPropertys.DetailLog)
                    {
                        if (@this.LogMessage?.LogLevel <= TouchSocket.Core.LogLevel.Trace)
                            @this.LogMessage?.LogTrace(@this.GetDetailLogString(topicArray, @this._memoryVarModelQueue.Count));
                        else if (@this.LogMessage?.LogLevel <= TouchSocket.Core.LogLevel.Debug)
                            @this.LogMessage?.LogDebug(@this.GetCountLogString(topicArray, @this._memoryVarModelQueue.Count));
                    }
                    else
                    {
                        if (@this.LogMessage?.LogLevel <= TouchSocket.Core.LogLevel.Debug)
                            @this.LogMessage?.LogDebug(@this.GetCountLogString(topicArray, @this._memoryVarModelQueue.Count));
                    }
                    return OperResult.Success;
                }
            }
            catch (OperationCanceledException)
            {
                return new OperResult("Timeout");
            }
            catch (Exception ex)
            {
                return new OperResult(ex);
            }
        }
    }

    #endregion 方法


#endif
}
