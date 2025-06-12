
using ThingsGateway.Blazor.Diagrams.Core.Geometry;
using ThingsGateway.Blazor.Diagrams.Core.Models;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

[CategoryNode(Category = "Expression", ImgUrl = "_content/ThingsGateway.Gateway.Razor/img/Delay.svg", Desc = nameof(DelayNode), LocalizerType = typeof(ThingsGateway.Gateway.Application.DefaultDiagram), WidgetType = "ThingsGateway.Gateway.Razor.NumberWidget,ThingsGateway.Gateway.Razor")]
public class DelayNode : NumberNode, IExpressionNode
{
    public DelayNode(string id, Point? position = null) : base(id, position) { Title = "DelayNode"; Placeholder = "DelayNode.Placeholder"; }

    async Task<NodeOutput> IExpressionNode.ExecuteAsync(NodeInput input, CancellationToken cancellationToken)
    {
        Logger?.Trace($"Delay {Number} ms");
        await Task.Delay(Number ?? 0, cancellationToken).ConfigureAwait(false);
        return new NodeOutput();
    }

}


