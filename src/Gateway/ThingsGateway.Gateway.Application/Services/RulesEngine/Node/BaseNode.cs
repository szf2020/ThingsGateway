
using ThingsGateway.Blazor.Diagrams.Core.Geometry;
using ThingsGateway.Blazor.Diagrams.Core.Models;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

public abstract class BaseNode : NodeModel, INode
{
    public BaseNode(string id, Point? position = null) : base(id, position)
    {

    }

    public string RulesEngineName { get; set; }
    public ILog Logger { get; set; }
}
