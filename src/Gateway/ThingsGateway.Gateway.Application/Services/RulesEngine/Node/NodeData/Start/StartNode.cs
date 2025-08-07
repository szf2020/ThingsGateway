
using ThingsGateway.Blazor.Diagrams.Core.Geometry;

namespace ThingsGateway.Gateway.Application;

[CategoryNode(Category = "Start/End", ImgUrl = $"{INode.ModuleBasePath}img/Start.svg", Desc = nameof(StartNode), LocalizerType = typeof(ThingsGateway.Gateway.Application.INode), WidgetType = $"ThingsGateway.Gateway.Razor.DefaultWidget,{INode.FileModulePath}")]
public class StartNode : BaseNode, IStartNode
{
    public StartNode(string id, Point? position = null) : base(id, position)
    { Title = "Start"; }
}
