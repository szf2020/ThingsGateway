
using ThingsGateway.Blazor.Diagrams.Core.Geometry;
using ThingsGateway.Gateway.Application.Extensions;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

[CategoryNode(Category = "Expression", ImgUrl = $"{INode.ModuleBasePath}img/CSharpScript.svg", Desc = nameof(ConditionNode), LocalizerType = typeof(ThingsGateway.Gateway.Application.INode), WidgetType = $"ThingsGateway.Gateway.Razor.CSharpScriptWidget,{INode.FileModulePath}")]
public class ConditionNode : TextNode, IConditionNode
{
    public ConditionNode(string id, Point? position = null) : base(id, position)
    {
        Title = "ConditionNode"; Placeholder = "ConditionNode.Placeholder";
        Text = "return true;";
    }

    Task<bool> IConditionNode.ExecuteAsync(NodeInput input, CancellationToken cancellationToken)
    {
        var value = ExpressionEvaluatorExtension.GetExpressionsResult(Text, input.Value, Logger);
        var next = value.ToBoolean(false);
        Logger?.Trace($"Condition result: {next}");
        return Task.FromResult(next);
    }

}
