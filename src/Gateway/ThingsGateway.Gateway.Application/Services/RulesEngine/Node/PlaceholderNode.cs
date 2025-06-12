using ThingsGateway.Blazor.Diagrams.Core.Geometry;

namespace ThingsGateway.Gateway.Application;

public abstract class PlaceholderNode : BaseNode
{

    protected PlaceholderNode(string id, Point? position = null) : base(id, position)
    {
    }

    public string Placeholder { get; set; }
}