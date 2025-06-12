
using ThingsGateway.Blazor.Diagrams.Core.Geometry;
using ThingsGateway.Blazor.Diagrams.Core.Models;
namespace ThingsGateway.Gateway.Application;

public abstract class TextNode : PlaceholderNode
{

    public TextNode(string id, Point? position = null) : base(id, position)
    {
    }

    [ModelValue]
    public virtual string Text { get; set; }
}
