//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using MQTTnet;
using MQTTnet.Server;

using Newtonsoft.Json.Linq;

using System.Text;

using ThingsGateway.Foundation;
using ThingsGateway.NewLife.Extension;
using ThingsGateway.NewLife.Json.Extension;

namespace ThingsGateway.Plugin.Mqtt;

public abstract class DynamicMqttServerRpcBase
{
    /// <summary>
    ///触发rpc脚本调用 
    /// </summary>
    /// <param name="logMessage">日志对象</param>
    /// <param name="args">InterceptingPublishEventArgs</param>
    /// <param name="driverPropertys">插件属性</param>
    /// <param name="mqttServer">mqttServer</param>
    /// <param name="getRpcResult">传入clientId和rpc数据(设备，变量名称+值字典)，返回rpc结果</param>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns></returns>
    public virtual async Task RPCInvokeAsync(TouchSocket.Core.ILog logMessage, InterceptingPublishEventArgs args, MqttServerProperty driverPropertys, MQTTnet.Server.MqttServer mqttServer, Func<string, Dictionary<string, Dictionary<string, JToken>>, ValueTask<Dictionary<string, Dictionary<string, IOperResult>>>> getRpcResult, CancellationToken cancellationToken)
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
            var variableMessage = new MqttApplicationMessageBuilder()
.WithTopic($"{args.ApplicationMessage.Topic}/Response")
.WithPayload(mqttRpcResult.ToSystemTextJsonString(driverPropertys.JsonFormattingIndented)).Build();
            await mqttServer.InjectApplicationMessage(new InjectedMqttApplicationMessage(variableMessage), cancellationToken).ConfigureAwait(false);
        }
        catch
        {
        }
    }
}
