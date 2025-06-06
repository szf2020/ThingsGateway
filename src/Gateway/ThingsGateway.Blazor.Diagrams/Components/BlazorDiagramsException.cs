namespace ThingsGateway.Blazor.Diagrams;

public class BlazorDiagramsException : Exception
{
    public BlazorDiagramsException(string? message) : base(message)
    {
    }

    public BlazorDiagramsException() : base()
    {
    }

    public BlazorDiagramsException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}