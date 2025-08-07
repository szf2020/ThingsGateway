
using ThingsGateway.Blazor.Diagrams.Core.Geometry;
#if !Management
using ThingsGateway.Gateway.Application.Extensions;
#endif
using ThingsGateway.NewLife.Extension;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

[CategoryNode(Category = "Expression", ImgUrl = "_content/ThingsGateway.Gateway.Razor/img/CSharpScript.svg", Desc = nameof(ConditionNode), LocalizerType = typeof(ThingsGateway.Gateway.Application.INode), WidgetType = "ThingsGateway.Gateway.Razor.CSharpScriptWidget,ThingsGateway.Gateway.Razor")]
public class ConditionNode : TextNode, IConditionNode
{
    public ConditionNode(string id, Point? position = null) : base(id, position)
    {
        Title = "ConditionNode"; Placeholder = "ConditionNode.Placeholder";
        Text = "return true;";
    }
#if !Management

    Task<bool> IConditionNode.ExecuteAsync(NodeInput input, CancellationToken cancellationToken)
    {
        var value = Text.GetExpressionsResult(input.Value, Logger);
        var next = value.ToBoolean(false);
        Logger?.Trace($"Condition result: {next}");
        return Task.FromResult(next);
    }
#else
    Task<bool> IConditionNode.ExecuteAsync(NodeInput input, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
#endif
}
