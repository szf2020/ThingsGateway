
using ThingsGateway.Blazor.Diagrams.Core.Geometry;

namespace ThingsGateway.Gateway.Application;

[CategoryNode(Category = "Actuator", ImgUrl = $"{INode.ModuleBasePath}img/CSharpScript.svg", Desc = nameof(ExecuteScriptNode), LocalizerType = typeof(ThingsGateway.Gateway.Application.INode), WidgetType = $"ThingsGateway.Gateway.Razor.CSharpScriptWidget,{INode.FileModulePath}")]
public class ExecuteScriptNode : TextNode, IActuatorNode, IExexcuteExpressionsBase, IDisposable
{
    public ExecuteScriptNode(string id, Point? position = null) : base(id, position)
    {
        Title = "ExecuteScriptNode"; Placeholder = "ExecuteScriptNode.Placeholder";
        Text =
            """
            using ThingsGateway.Foundation;
            using TouchSocket.Core;
            
            using System.Text;
            using ThingsGateway.Gateway.Application;
            using ThingsGateway.Gateway.Razor;


            public class TestExexcuteExpressions : IExexcuteExpressions
            {

                public TouchSocket.Core.ILog Logger { get; set; }

                public async System.Threading.Tasks.Task<NodeOutput> ExecuteAsync(NodeInput input, System.Threading.CancellationToken cancellationToken)
                {
                    //想上传mqtt，可以自己写mqtt上传代码，或者通过mqtt插件的公开方法上传

                    //直接获取mqttclient插件类型的第一个设备
                    var mqttClient = GlobalData.ReadOnlyChannels.FirstOrDefault(a => a.Value.PluginName == "ThingsGateway.Plugin.Mqtt.MqttClient").Value?.ReadDeviceRuntimes?.FirstOrDefault().Value?.Driver as ThingsGateway.Plugin.Mqtt.MqttClient;
                    if (mqttClient == null)
                        throw new("mqttClient NOT FOUND");

                    TopicArray topicArray = new()
                    {
                        Topic = "test",
                        Payload = Encoding.UTF8.GetBytes("test")
                    };
                    var result = await mqttClient.MqttUpAsync(topicArray, default).ConfigureAwait(false);// 主题 和 负载
                    if (!result.IsSuccess)
                        throw new(result.ErrorMessage);
                    return new NodeOutput() { Value = result };

                    //通过设备名称找出mqttClient插件
                    //var mqttClient = GlobalData.ReadOnlyDevices.FirstOrDefault(a => a.Value.Name == "mqttDevice1").Value?.Driver as ThingsGateway.Plugin.Mqtt.MqttClient;

                    //if (mqttClient == null)
                    //    throw new("mqttClient NOT FOUND");


                    //TopicArray topicArray = new()
                    //{
                    //    Topic = "test",
                    //    Payload = Encoding.UTF8.GetBytes("test")
                    //};
                    //var result = await mqttClient.MqttUpAsync(topicArray, default).ConfigureAwait(false);// 主题 和 负载
                    //if (!result.IsSuccess)
                    //    throw new(result.ErrorMessage);
                    //return new NodeOutput() { Value = result };
                }
            }



            
            

            """;
    }

    private string text;

    [ModelValue]
    public override string Text
    {
        get
        {
            return text;
        }
        set
        {
#if !Management
            if (text != value)
            {
                try
                {
                    var exexcuteExpressions = CSharpScriptEngineExtension.Do<IExexcuteExpressions>(text);
                    exexcuteExpressions?.TryDispose();
                    CSharpScriptEngineExtension.Remove(text);
                }
                catch
                {
                }
            }
#endif
            text = value;
        }
    }
#if !Management
    async Task<OperResult<NodeOutput>> IActuatorNode.ExecuteAsync(NodeInput input, CancellationToken cancellationToken)
    {
        try
        {
            Logger?.LogTrace($"Execute script");
            var exexcuteExpressions = CSharpScriptEngineExtension.Do<IExexcuteExpressions>(Text);
            if (exexcuteExpressions == null)
            {
                return new OperResult<NodeOutput>("exexcuteExpressions is null");
            }
            exexcuteExpressions.Logger = Logger;
            var data = await exexcuteExpressions.ExecuteAsync(input, cancellationToken).ConfigureAwait(false);
            return new OperResult<NodeOutput>() { Content = data };
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex);
            return new OperResult<NodeOutput>(ex);
        }
    }
#else
    Task<OperResult<NodeOutput>> IActuatorNode.ExecuteAsync(NodeInput input, CancellationToken cancellationToken)
    {
        return Task.FromResult(new OperResult<NodeOutput>());
    }
#endif
    public void Dispose()
    {
#if !Management
        if (!text.IsNullOrWhiteSpace())
        {
            try
            {
                var exexcuteExpressions = CSharpScriptEngineExtension.Do<IExexcuteExpressions>(text);
                exexcuteExpressions.TryDispose();
                CSharpScriptEngineExtension.Remove(text);
            }
            catch
            {
            }
        }
#endif
        GC.SuppressFinalize(this);
    }
}
