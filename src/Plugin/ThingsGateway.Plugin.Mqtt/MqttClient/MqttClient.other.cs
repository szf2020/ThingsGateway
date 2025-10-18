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

using CSScripting;

using MQTTnet;

#if NET6_0
using MQTTnet.Client;
#endif

using Newtonsoft.Json.Linq;

using PooledAwait;

using System.Collections.Concurrent;
using System.Text;

using ThingsGateway.Extension.Generic;
using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.NewLife;
using ThingsGateway.NewLife.Extension;
using ThingsGateway.NewLife.Json.Extension;


namespace ThingsGateway.Plugin.Mqtt;

/// <summary>
/// MqttClient
/// </summary>
public partial class MqttClient : BusinessBaseWithCacheIntervalScriptAll
{
    private static readonly CompositeFormat RpcTopic = CompositeFormat.Parse("{0}/+");
    public const string ThingsBoardRpcTopic = "v1/gateway/rpc";
    private IMqttClient _mqttClient;

    private MqttClientOptions _mqttClientOptions;

    private MqttClientSubscribeOptions _mqttSubscribeOptions;

    private WaitLock ConnectLock = new(nameof(MqttClient));

#if !Management




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

    private ConcurrentQueue<DeviceBasicData> ThingsBoardDeviceConnectQueue { get; set; } = new();

    private async ValueTask<OperResult> UpdateThingsBoardDeviceConnect(DeviceBasicData deviceData)
    {
        var topicJsonTBList = new List<TopicArray>();

        {
            if (deviceData.DeviceStatus == DeviceStatusEnum.OnLine)
            {
                var json = new
                {
                    device = deviceData.Name,
                };
                var topicArray = new TopicArray()
                {
                    Topic = "v1/gateway/connect",
                    Payload = json.ToSystemTextJsonUtf8Bytes(_driverPropertys.JsonFormattingIndented)
                };

                topicJsonTBList.Add(topicArray);
            }
            else
            {
                var json = new
                {
                    device = deviceData.Name,
                };
                var topicArray = new TopicArray()
                {
                    Topic = "v1/gateway/disconnect",
                    Payload = json.ToSystemTextJsonUtf8Bytes(_driverPropertys.JsonFormattingIndented)
                };

                topicJsonTBList.Add(topicArray);
            }
        }
        var result = await Update(topicJsonTBList, default).ConfigureAwait(false);
        if (success != result.IsSuccess)
        {
            if (!result.IsSuccess)
            {
                LogMessage?.LogWarning(result.ToString());
            }
            success = result.IsSuccess;
        }
        return result;
    }

    protected override void DeviceTimeInterval(DeviceRuntime deviceRunTime, DeviceBasicData deviceData)
    {
        if (!_businessPropertyWithCacheIntervalScript.DeviceTopic.IsNullOrWhiteSpace())
            AddQueueDevModel(new(deviceData));

        base.DeviceChange(deviceRunTime, deviceData);
    }
    protected override void DeviceChange(DeviceRuntime deviceRunTime, DeviceBasicData deviceData)
    {
        if (_driverPropertys.RpcWriteTopic == ThingsBoardRpcTopic)
        {
            ThingsBoardDeviceConnectQueue.Enqueue(deviceData);
        }

        if (!_businessPropertyWithCacheIntervalScript.DeviceTopic.IsNullOrWhiteSpace())
            AddQueueDevModel(new(deviceData));

        base.DeviceChange(deviceRunTime, deviceData);
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
                //获取组内全部变量
                AddQueueVarModel(new CacheDBItem<List<VariableBasicData>>(variableRuntimeGroup.AdaptListVariableBasicData()));
            }
            else
            {
                AddQueueVarModel(new CacheDBItem<VariableBasicData>(variable));
            }
        }
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

    #region mqtt方法

    #region private

    private ValueTask<OperResult> Update(IEnumerable<TopicArray> topicArrayList, CancellationToken cancellationToken)
    {
        return Update(this, topicArrayList, cancellationToken);

        static async PooledValueTask<OperResult> Update(MqttClient @this, IEnumerable<TopicArray> topicArrayList, CancellationToken cancellationToken)
        {
            foreach (TopicArray topicArray in topicArrayList)
            {
                var result = await @this.MqttUpAsync(topicArray, cancellationToken).ConfigureAwait(false);
                if (cancellationToken.IsCancellationRequested)
                    return result;
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

    private async Task AllPublishAsync(CancellationToken cancellationToken)
    {

        //保留消息
        //分解List，避免超出mqtt字节大小限制
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

    private ValueTask<Dictionary<string, Dictionary<string, IOperResult>>> GetRpcResult(string clientId, Dictionary<string, Dictionary<string, JToken>> rpcDatas)
    {
        return GetRpcResult(this, clientId, rpcDatas);

        static async PooledValueTask<Dictionary<string, Dictionary<string, IOperResult>>> GetRpcResult(MqttClient @this, string clientId, Dictionary<string, Dictionary<string, JToken>> rpcDatas)
        {
            var mqttRpcResult = new Dictionary<string, Dictionary<string, IOperResult>>();
            rpcDatas.ForEach(a => mqttRpcResult.Add(a.Key, new()));
            try
            {
                foreach (var rpcData in rpcDatas)
                {
                    if (GlobalData.ReadOnlyDevices.TryGetValue(rpcData.Key, out var device))
                    {
                        foreach (var item in rpcData.Value)
                        {
                            if (device.ReadOnlyVariableRuntimes.TryGetValue(item.Key, out var variable) && @this.IdVariableRuntimes.TryGetValue(variable.Id, out var tag))
                            {
                                var rpcEnable = tag.GetPropertyValue(@this.DeviceId, nameof(_variablePropertys.VariableRpcEnable))?.ToBoolean();
                                if (rpcEnable == false)
                                {
                                    mqttRpcResult[rpcData.Key].Add(item.Key, new OperResult("RPCEnable is False"));
                                }
                            }
                            else
                            {
                                mqttRpcResult[rpcData.Key].Add(item.Key, new OperResult("The variable does not exist"));
                            }
                        }
                    }
                }

                Dictionary<string, Dictionary<string, string>> writeData = new();
                foreach (var item in rpcDatas)
                {
                    writeData.Add(item.Key, new());

                    foreach (var kv in item.Value)
                    {
                        if (!mqttRpcResult[item.Key].ContainsKey(kv.Key))
                        {
                            writeData[item.Key].Add(kv.Key, kv.Value?.ToString());
                        }
                    }
                }

                var result = await GlobalData.RpcService.InvokeDeviceMethodAsync(@this.ToString() + "-" + clientId,
                    writeData).ConfigureAwait(false);

                foreach (var dictKv in result)
                {
                    foreach (var item in dictKv.Value)
                    {
                        mqttRpcResult[dictKv.Key].TryAdd(item.Key, item.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                @this.LogMessage?.LogWarning(ex);
            }

            return mqttRpcResult;
        }
    }

    private async Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        try
        {
#if NET8_0_OR_GREATER

            var payload = args.ApplicationMessage.Payload;
            var payloadCount = payload.Length;
#else

        var payload = args.ApplicationMessage.PayloadSegment;
        var payloadCount = payload.Count;

#endif

            if (args.ApplicationMessage.Topic == _driverPropertys.RpcQuestTopic && payloadCount > 0)
            {
                await AllPublishAsync(CancellationToken.None).ConfigureAwait(false);
                return;
            }

            if (!_driverPropertys.DeviceRpcEnable)
                return;

            if (!_driverPropertys.BigTextScriptRpc.IsNullOrEmpty())
            {
                var rpcBase = CSharpScriptEngineExtension.Do<DynamicMqttClientRpcBase>(_driverPropertys.BigTextScriptRpc);

                await rpcBase.RPCInvokeAsync(LogMessage, args, _driverPropertys, _mqttClient, GetRpcResult, TryMqttClientAsync, CancellationToken.None).ConfigureAwait(false);
            }
            else
            {
                Dictionary<string, Dictionary<string, JToken>> rpcDatas = new();

                //适配 ThingsBoardRp
                if (args.ApplicationMessage.Topic == ThingsBoardRpcTopic)
                {
                    var thingsBoardRpcData = Encoding.UTF8.GetString(payload).FromJsonNetString<ThingsBoardRpcData>();
                    if (thingsBoardRpcData == null)
                        return;
                    rpcDatas.Add(thingsBoardRpcData.device, thingsBoardRpcData.data.@params.ToDictionary(a => a.Key, a => JToken.Parse(a.Value)));

                    if (rpcDatas == null)
                        return;

                    var mqttRpcResult = await GetRpcResult(args.ClientId, rpcDatas).ConfigureAwait(false);
                    try
                    {
                        var isConnect = await TryMqttClientAsync(CancellationToken.None).ConfigureAwait(false);
                        if (isConnect.IsSuccess)
                        {
                            ThingsBoardRpcResponseData thingsBoardRpcResponseData = new();
                            thingsBoardRpcResponseData.device = thingsBoardRpcData.device;
                            thingsBoardRpcResponseData.id = thingsBoardRpcData.data.id;
                            thingsBoardRpcResponseData.data.success = mqttRpcResult[thingsBoardRpcResponseData.device].All(b => b.Value.IsSuccess);
                            thingsBoardRpcResponseData.data.message = mqttRpcResult[thingsBoardRpcResponseData.device].Select(a => a.Value.ErrorMessage).ToSystemTextJsonString(_driverPropertys.JsonFormattingIndented);

                            var variableMessage = new MqttApplicationMessageBuilder()
        .WithTopic($"{args.ApplicationMessage.Topic}")
        .WithPayload(thingsBoardRpcResponseData.ToSystemTextJsonString(_driverPropertys.JsonFormattingIndented)).Build();
                            await _mqttClient.PublishAsync(variableMessage).ConfigureAwait(false);
                        }
                    }
                    catch
                    {
                    }
                }
                else
                {
                    var t = string.Format(null, RpcTopic, _driverPropertys.RpcWriteTopic);
                    if (MqttTopicFilterComparer.Compare(args.ApplicationMessage.Topic, t) != MqttTopicFilterCompareResult.IsMatch)
                        return;
                    rpcDatas = Encoding.UTF8.GetString(payload).FromJsonNetString<Dictionary<string, Dictionary<string, JToken>>>();
                    if (rpcDatas == null)
                        return;

                    var mqttRpcResult = await GetRpcResult(args.ClientId, rpcDatas).ConfigureAwait(false);
                    try
                    {
                        var isConnect = await TryMqttClientAsync(CancellationToken.None).ConfigureAwait(false);
                        if (isConnect.IsSuccess)
                        {
                            var variableMessage = new MqttApplicationMessageBuilder()
        .WithTopic($"{args.ApplicationMessage.Topic}/Response")
        .WithPayload(mqttRpcResult.ToSystemTextJsonString(_driverPropertys.JsonFormattingIndented)).Build();
                            await _mqttClient.PublishAsync(variableMessage).ConfigureAwait(false);
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex, $"MqttClient_ApplicationMessageReceivedAsync error");
        }
    }

    private async Task MqttClient_ConnectedAsync(MqttClientConnectedEventArgs args)
    {
        //连接成功后订阅相关主题
        if (_mqttSubscribeOptions != null)
        {
            var subResult = await _mqttClient.SubscribeAsync(_mqttSubscribeOptions).ConfigureAwait(false);
            if (subResult.Items.Any(a => a.ResultCode > (MqttClientSubscribeResultCode)10))
            {
                LogMessage?.LogWarning($"Subscribe fail  {subResult.Items
                    .Where(a => a.ResultCode > (MqttClientSubscribeResultCode)10)
                    .Select(a =>
                    new
                    {
                        Topic = a.TopicFilter.Topic,
                        ResultCode = a.ResultCode.ToString()
                    }
                    )
                    .ToSystemTextJsonString(_driverPropertys.JsonFormattingIndented)}");
            }
        }
    }

    /// <summary>
    /// 上传mqtt，返回上传结果
    /// </summary>
    public ValueTask<OperResult> MqttUpAsync(TopicArray topicArray, CancellationToken cancellationToken = default)
    {
        return MqttUpAsync(this, topicArray, cancellationToken);

        static async PooledValueTask<OperResult> MqttUpAsync(MqttClient @this, TopicArray topicArray, CancellationToken cancellationToken)
        {
            try
            {
                var isConnect = await @this.TryMqttClientAsync(cancellationToken).ConfigureAwait(false);
                if (isConnect.IsSuccess)
                {
                    var variableMessage = new MqttApplicationMessageBuilder()
        .WithTopic(topicArray.Topic).WithQualityOfServiceLevel(@this._driverPropertys.MqttQualityOfServiceLevel).WithRetainFlag()
        .WithPayload(topicArray.Payload).Build();
                    var result = await @this._mqttClient.PublishAsync(variableMessage, cancellationToken).ConfigureAwait(false);
                    if (result.IsSuccess)
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
                    else
                    {
                        return new OperResult($"Upload fail{result.ReasonString}");
                    }
                }
                else
                {
                    return isConnect;
                }
            }
            catch (Exception ex)
            {
                return new OperResult($"Upload fail", ex);
            }
        }
    }

    private ValueTask<OperResult> TryMqttClientAsync(CancellationToken cancellationToken)
    {
        if (DisposedValue || _mqttClient == null) return TouchSocket.Core.EasyValueTask.FromResult(new OperResult("MqttClient is disposed"));

        if (_mqttClient?.IsConnected == true)
            return TouchSocket.Core.EasyValueTask.FromResult(OperResult.Success);
        return Client(this, cancellationToken);

        static async PooledValueTask<OperResult> Client(MqttClient @this, CancellationToken cancellationToken)
        {
            if (@this._mqttClient?.IsConnected == true)
                return OperResult.Success;
            try
            {
                await @this.ConnectLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                await Task.Delay(100, cancellationToken).ConfigureAwait(false);
                if (@this._mqttClient?.IsConnected == true)
                    return OperResult.Success;
                using var timeoutToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(@this._driverPropertys.ConnectTimeout));
                using CancellationTokenSource stoppingToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken.Token);
                if (@this._mqttClient?.IsConnected == true)
                    return OperResult.Success;
                if (@this._mqttClient == null)
                {
                    return new OperResult("mqttClient is null");
                }
                var result = await @this._mqttClient.ConnectAsync(@this._mqttClientOptions, stoppingToken.Token).ConfigureAwait(false);
                if (@this._mqttClient.IsConnected)
                {
                    return OperResult.Success;
                }
                else
                {
                    if (timeoutToken.IsCancellationRequested)
                        return new OperResult($"Connect timeout");
                    else
                        return new OperResult($"Connect fail {result.ReasonString}");
                }
            }
            catch (Exception ex)
            {
                return new OperResult(ex);
            }
            finally
            {
                @this.ConnectLock.Release();
            }
        }
    }

    #endregion mqtt方法

#endif
}
