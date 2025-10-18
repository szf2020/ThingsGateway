//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using CSScripting;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

using MQTTnet;
using MQTTnet.Internal;
using MQTTnet.Protocol;
using MQTTnet.Server;

using Newtonsoft.Json.Linq;

using PooledAwait;

using System.Text;

using ThingsGateway.Admin.Application;
using ThingsGateway.Extension.Generic;
using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.NewLife.Extension;
using ThingsGateway.NewLife.Json.Extension;

namespace ThingsGateway.Plugin.Mqtt;

/// <summary>
/// MqttServer
/// </summary>
public partial class MqttServer : BusinessBaseWithCacheIntervalScriptAll
{
#if !Management
    private static readonly CompositeFormat RpcTopic = CompositeFormat.Parse("{0}/+");
    private MQTTnet.Server.MqttServer _mqttServer;

#if NET10_0_OR_GREATER

    private Microsoft.Extensions.Hosting.IHost _webHost { get; set; }

#else
    private IWebHost _webHost { get; set; }

#endif




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
        if (!_businessPropertyWithCacheIntervalScript.DeviceTopic.IsNullOrEmpty())
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
                //获取组内全部变量
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

        static async PooledValueTask<OperResult> Update(MqttServer @this, IEnumerable<TopicArray> topicArrayList, CancellationToken cancellationToken)
        {
            foreach (var topicArray in topicArrayList)
            {
                var result = await @this.MqttUpAsync(topicArray, cancellationToken).ConfigureAwait(false);
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

    private ValueTask<Dictionary<string, Dictionary<string, IOperResult>>> GetRpcResult(string clientId, Dictionary<string, Dictionary<string, JToken>> rpcDatas)
    {
        return GetRpcResult(this, clientId, rpcDatas);

        static async PooledValueTask<Dictionary<string, Dictionary<string, IOperResult>>> GetRpcResult(MqttServer @this, string clientId, Dictionary<string, Dictionary<string, JToken>> rpcDatas)
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

    private List<MqttApplicationMessage> GetRetainedMessages()
    {
        //首次连接时的保留消息
        //分解List，避免超出mqtt字节大小限制
        var varData = IdVariableRuntimes.Select(a => a.Value).AdaptIEnumerableVariableBasicData().ChunkBetter(_driverPropertys.SplitSize);
        var devData = CollectDevices?.Select(a => a.Value).AdaptIEnumerableDeviceBasicData().ChunkBetter(_driverPropertys.SplitSize);
        var alramData = GlobalData.ReadOnlyRealAlarmIdVariables.Select(a => a.Value).ChunkBetter(_driverPropertys.SplitSize);
        List<MqttApplicationMessage> Messages = new();

        if (!_businessPropertyWithCacheIntervalScript.VariableTopic.IsNullOrEmpty())
        {
            foreach (var item in varData)
            {
                var topicArrayList = GetVariableBasicDataTopicArray(item);
                foreach (var topicArray in topicArrayList)
                {
                    Messages.Add(new MqttApplicationMessageBuilder()
    .WithTopic(topicArray.Topic)
    .WithPayload(topicArray.Payload).Build());
                }
            }
        }
        if (!_businessPropertyWithCacheIntervalScript.DeviceTopic.IsNullOrEmpty())
        {
            if (devData != null)
            {
                foreach (var item in devData)
                {
                    var topicArrayList = GetDeviceTopicArray(item);
                    foreach (var topicArray in topicArrayList)
                    {
                        Messages.Add(new MqttApplicationMessageBuilder()
        .WithTopic(topicArray.Topic)
        .WithPayload(topicArray.Payload).Build());
                    }
                }
            }
        }
        if (!_businessPropertyWithCacheIntervalScript.AlarmTopic.IsNullOrEmpty())
        {
            foreach (var item in alramData)
            {
                var topicArrayList = GetAlarmTopicArrays(item);
                foreach (var topicArray in topicArrayList)
                {
                    Messages.Add(new MqttApplicationMessageBuilder()
    .WithTopic(topicArray.Topic)
    .WithPayload(topicArray.Payload).Build());
                }
            }
        }
        return Messages;
    }

    private Task MqttServer_ClientDisconnectedAsync(MQTTnet.Server.ClientDisconnectedEventArgs arg)
    {
        LogMessage?.LogInformation($"{ToString()}-{arg.ClientId}-Client DisConnected-{arg.DisconnectType}");
        return Task.CompletedTask;
    }

    private Task MqttServer_InterceptingPublishAsync(InterceptingPublishEventArgs args)
    {
        return MqttServer_InterceptingPublishAsync(this, args);


        static async PooledTask MqttServer_InterceptingPublishAsync(MqttServer @this, InterceptingPublishEventArgs args)
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

                if (args.ApplicationMessage.Topic == @this._driverPropertys.RpcQuestTopic && payloadCount > 0)
                {
                    var data = @this.GetRetainedMessages();

                    foreach (var item in data)
                    {
                        await @this._mqttServer.InjectApplicationMessage(new InjectedMqttApplicationMessage(item)).ConfigureAwait(false);
                    }
                    return;
                }

                if (!@this._driverPropertys.DeviceRpcEnable || string.IsNullOrEmpty(args.ClientId))
                    return;

                if (!@this._driverPropertys.BigTextScriptRpc.IsNullOrEmpty())
                {
                    var rpcBase = CSharpScriptEngineExtension.Do<DynamicMqttServerRpcBase>(@this._driverPropertys.BigTextScriptRpc);

                    await rpcBase.RPCInvokeAsync(@this.LogMessage, args, @this._driverPropertys, @this._mqttServer, @this.GetRpcResult, CancellationToken.None).ConfigureAwait(false);
                }
                else
                {
                    if (@this._driverPropertys.RpcWriteTopic.IsNullOrWhiteSpace()) return;

                    var t = string.Format(null, RpcTopic, @this._driverPropertys.RpcWriteTopic);
                    if (MqttTopicFilterComparer.Compare(args.ApplicationMessage.Topic, t) != MqttTopicFilterCompareResult.IsMatch)
                        return;
                    var rpcDatas = Encoding.UTF8.GetString(payload).FromJsonNetString<Dictionary<string, Dictionary<string, JToken>>>();
                    if (rpcDatas == null)
                        return;
                    var mqttRpcResult = await @this.GetRpcResult(args.ClientId, rpcDatas).ConfigureAwait(false);

                    try
                    {
                        var variableMessage = new MqttApplicationMessageBuilder()
            .WithTopic($"{args.ApplicationMessage.Topic}/Response")
            .WithPayload(mqttRpcResult.ToSystemTextJsonString(@this._driverPropertys.JsonFormattingIndented)).Build();
                        await @this._mqttServer.InjectApplicationMessage(new InjectedMqttApplicationMessage(variableMessage)).ConfigureAwait(false);
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                @this.LogMessage?.LogWarning(ex, $"MqttServer_InterceptingPublishAsync error");
            }

            return;
        }
    }

    private Task MqttServer_LoadingRetainedMessageAsync(LoadingRetainedMessagesEventArgs arg)
    {
        //List<MqttApplicationMessage> Messages = GetRetainedMessages();
        //arg.LoadedRetainedMessages = Messages;
        return CompletedTask.Instance;
    }

    private async Task MqttServer_ValidatingConnectionAsync(ValidatingConnectionEventArgs arg)
    {
        if (!string.IsNullOrEmpty(_driverPropertys.StartWithId) && !arg.ClientId.StartsWith(_driverPropertys.StartWithId))
        {
            arg.ReasonCode = MqttConnectReasonCode.ClientIdentifierNotValid;
            return;
        }

        if (_driverPropertys.AnonymousEnable)
        {
            arg.ReasonCode = MqttConnectReasonCode.Success;
            return;
        }

        var _userService = App.RootServices.GetRequiredService<ISysUserService>();
        var userInfo = await _userService.GetUserByAccountAsync(arg.UserName, null).ConfigureAwait(false);//获取用户信息
        if (userInfo == null)
        {
            arg.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
            return;
        }
        if (userInfo.Password != arg.Password)
        {
            arg.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
            return;
        }
        LogMessage?.LogInformation($"{ToString()}-{arg.ClientId}-Client Connected");
    }

    /// <summary>
    /// 上传mqtt，返回上传结果
    /// </summary>
    public ValueTask<OperResult> MqttUpAsync(TopicArray topicArray, CancellationToken cancellationToken = default)
    {
        return MqttUpAsync(this, topicArray, cancellationToken);

        static async PooledValueTask<OperResult> MqttUpAsync(MqttServer @this, TopicArray topicArray, CancellationToken cancellationToken)
        {
            try
            {
                var message = new MqttApplicationMessageBuilder()
    .WithTopic(topicArray.Topic).WithQualityOfServiceLevel(@this._driverPropertys.MqttQualityOfServiceLevel).WithRetainFlag()
    .WithPayload(topicArray.Payload).Build();
                await @this._mqttServer.InjectApplicationMessage(
                        new InjectedMqttApplicationMessage(message), cancellationToken).ConfigureAwait(false);

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
            catch (Exception ex)
            {
                return new OperResult("Upload fail", ex);
            }
        }
    }

#endif
}
