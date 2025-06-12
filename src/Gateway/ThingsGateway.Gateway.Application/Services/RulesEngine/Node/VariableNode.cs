
using ThingsGateway.Blazor.Diagrams.Core.Geometry;
using ThingsGateway.Blazor.Diagrams.Core.Models;
namespace ThingsGateway.Gateway.Application;

public abstract class VariableNode : TextNode
{

    public VariableNode(string id, Point? position = null) : base(id, position)
    {
        Placeholder = "Variable.Placeholder";
        DevicePlaceholder = "Device.Placeholder";
    }
    public string DevicePlaceholder { get; set; }

    [ModelValue]
    public string DeviceText { get; set; }
}
