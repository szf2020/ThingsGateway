using ThingsGateway.Blazor.Diagrams.Core;
using ThingsGateway.Blazor.Diagrams.Core.Controls.Default;
using ThingsGateway.Blazor.Diagrams.Core.Models.Base;
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