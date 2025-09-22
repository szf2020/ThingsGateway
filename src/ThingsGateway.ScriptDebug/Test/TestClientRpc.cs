//------------------------------------------------------------------------------
//此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using MQTTnet;

using Newtonsoft.Json.Linq;

using System.Text;

using ThingsGateway.Foundation;
using ThingsGateway.NewLife.Json.Extension;
using ThingsGateway.Plugin.Mqtt;

using TouchSocket.Core;

public class TestClientRpc : DynamicMqttClientRpcBase
{
    public override async Task RPCInvokeAsync(ILog logMessage, MqttApplicationMessageReceivedEventArgs args, MqttClientProperty driverPropertys, IMqttClient mqttClient, Func<string, Dictionary<string, Dictionary<string, JToken>>, ValueTask<Dictionary<string, Dictionary<string, IOperResult>>>> getRpcResult, Func<CancellationToken, ValueTask<OperResult>> tryMqttClientAsync, CancellationToken cancellationToken)
    {
        if (driverPropertys.RpcWriteTopic.IsNullOrWhiteSpace()) return;
        var t = string.Format(null, "{0}/+", driverPropertys.RpcWriteTopic);
        if (MqttTopicFilterComparer.Compare(args.ApplicationMessage.Topic, t) != MqttTopicFilterCompareResult.IsMatch)
            return;

        var rpcDatas = Encoding.UTF8.GetString(args.ApplicationMessage.Payload).FromJsonNetString<Dictionary<string, Dictionary<string, JToken>>>();
        if (rpcDatas == null)
            return;

        var mqttRpcResult = await getRpcResult(args.ClientId, rpcDatas).ConfigureAwait(false);
        try
        {
            var isConnect = await tryMqttClientAsync(CancellationToken.None).ConfigureAwait(false);
            if (isConnect.IsSuccess)
            {
                var variableMessage = new MqttApplicationMessageBuilder()
.WithTopic($"{args.ApplicationMessage.Topic}/Response")
.WithPayload(mqttRpcResult.ToSystemTextJsonString(driverPropertys.JsonFormattingIndented)).Build();
                await mqttClient.PublishAsync(variableMessage, cancellationToken).ConfigureAwait(false);
            }
        }
        catch
        {
        }
    }
}
