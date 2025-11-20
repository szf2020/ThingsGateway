
using ThingsGateway.Blazor.Diagrams.Core.Geometry;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

[CategoryNode(Category = "Expression", ImgUrl = $"{INode.ModuleBasePath}img/Delay.svg", Desc = nameof(DelayNode), LocalizerType = typeof(ThingsGateway.Gateway.Application.INode), WidgetType = $"ThingsGateway.Gateway.Razor.NumberWidget,{INode.FileModulePath}")]
public class DelayNode : NumberNode, IExpressionNode
{
    public DelayNode(string id, Point? position = null) : base(id, position) { Title = "DelayNode"; Placeholder = "DelayNode.Placeholder"; }

    async Task<OperResult<NodeOutput>> IExpressionNode.ExecuteAsync(NodeInput input, CancellationToken cancellationToken)
    {
        try
        {
            Logger?.Trace($"Delay {Number} ms");
            await Task.Delay(Number ?? 0, cancellationToken).ConfigureAwait(false);
            return new OperResult<NodeOutput>() { Content = new NodeOutput() };
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex);
            return new OperResult<NodeOutput>(ex);
        }
    }
}
