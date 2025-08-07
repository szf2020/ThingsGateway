
using ThingsGateway.Blazor.Diagrams.Core.Geometry;

#if !Management
using ThingsGateway.Gateway.Application.Extensions;

#endif
using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

[CategoryNode(Category = "Expression", ImgUrl = $"{INode.ModuleBasePath}img/CSharpScript.svg", Desc = nameof(DataNode), LocalizerType = typeof(ThingsGateway.Gateway.Application.INode), WidgetType = $"ThingsGateway.Gateway.Razor.CSharpScriptWidget,{INode.FileModulePath}")]
public class DataNode : TextNode, IExpressionNode
{
    public DataNode(string id, Point? position = null) : base(id, position)
    {
        Title = "DataNode"; Placeholder = "DataNode.Placeholder";
        Text = "return 1;";
    }
#if !Management

    Task<OperResult<NodeOutput>> IExpressionNode.ExecuteAsync(NodeInput input, CancellationToken cancellationToken)
    {
        try
        {
            var value = Text.GetExpressionsResult(input.Value, Logger);
            NodeOutput nodeOutput = new();
            nodeOutput.Value = value;
            Logger?.Trace($"Data result: {nodeOutput.JToken?.ToString(Newtonsoft.Json.Formatting.Indented)}");
            return Task.FromResult(new OperResult<NodeOutput>() { Content = nodeOutput });
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex);
            return Task.FromResult(new OperResult<NodeOutput>(ex));
        }
    }
#else
    Task<OperResult<NodeOutput>> IExpressionNode.ExecuteAsync(NodeInput input, CancellationToken cancellationToken)
    {
        return Task.FromResult(new OperResult<NodeOutput>());
    }
#endif
}
