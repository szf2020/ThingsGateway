
using ThingsGateway.Blazor.Diagrams.Core.Geometry;

namespace ThingsGateway.Gateway.Application;

[CategoryNode(Category = "Start/End", ImgUrl = "_content/ThingsGateway.Gateway.Razor/img/Start.svg", Desc = nameof(StartNode), LocalizerType = typeof(ThingsGateway.Gateway.Application.DefaultDiagram), WidgetType = "ThingsGateway.Gateway.Razor.DefaultWidget,ThingsGateway.Gateway.Razor")]
public class StartNode : BaseNode, IStartNode
{
    public StartNode(string id, Point? position = null) : base(id, position)
    { Title = "Start"; }
}
