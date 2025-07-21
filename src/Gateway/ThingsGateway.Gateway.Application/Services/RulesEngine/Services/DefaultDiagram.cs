using ThingsGateway.Blazor.Diagrams.Core;
using ThingsGateway.Blazor.Diagrams.Core.Options;

namespace ThingsGateway.Gateway.Application;

internal class DefaultDiagram : Diagram
{
    public override DiagramOptions Options { get; }

    public DefaultDiagram()
    {
        Options = new();
    }
}