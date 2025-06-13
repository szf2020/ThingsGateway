using ThingsGateway.Blazor.Diagrams.Core.Geometry;
namespace ThingsGateway.Gateway.Application;

public abstract class NumberNode : PlaceholderNode
{

    public NumberNode(string id, Point? position = null) : base(id, position)
    {
    }
    [ModelValue]
    public int? Number { get; set; }


}
