
using ThingsGateway.Blazor.Diagrams.Core.Geometry;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

[CategoryNode(Category = "Actuator", ImgUrl = "_content/ThingsGateway.Gateway.Razor/img/Rpc.svg", Desc = nameof(VariableRpcNode), LocalizerType = typeof(ThingsGateway.Gateway.Application.DefaultDiagram), WidgetType = "ThingsGateway.Gateway.Razor.VariableWidget,ThingsGateway.Gateway.Razor")]
public class VariableRpcNode : VariableNode, IActuatorNode
{

    public VariableRpcNode(string id, Point? position = null) : base(id, position)
    { Title = "VariableRpcNode"; }

    async Task<NodeOutput> IActuatorNode.ExecuteAsync(NodeInput input, CancellationToken cancellationToken)
    {
        if ((!DeviceText.IsNullOrWhiteSpace()) && GlobalData.ReadOnlyDevices.TryGetValue(DeviceText, out var device))
        {
            if (device.ReadOnlyVariableRuntimes.TryGetValue(Text, out var value))
            {
                var data = await value.RpcAsync(input.JToken.ToString(), $"RulesEngine: {RulesEngineName}", cancellationToken).ConfigureAwait(false);
                if (data.IsSuccess)
                    Logger?.Trace($" VariableRpcNode - VariableName {Text} : execute success");
                else
                    Logger?.Warning($" VariableRpcNode - VariableName {Text} : {data.ErrorMessage}");
                return new NodeOutput() { Value = data };
            }
        }
        Logger?.Warning($" VariableRpcNode - VariableName {Text} : not found");
        return new NodeOutput() { };
    }


}
