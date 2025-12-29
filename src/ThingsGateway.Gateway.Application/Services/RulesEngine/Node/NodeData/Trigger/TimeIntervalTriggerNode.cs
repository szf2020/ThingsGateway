using ThingsGateway.Blazor.Diagrams.Core.Geometry;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

[CategoryNode(Category = "Trigger", ImgUrl = $"{INode.ModuleBasePath}img/TimeInterval.svg", Desc = nameof(TimeIntervalTriggerNode), LocalizerType = typeof(ThingsGateway.Gateway.Application.INode), WidgetType = $"ThingsGateway.Gateway.Razor.TextWidget,{INode.FileModulePath}")]
public class TimeIntervalTriggerNode : TextNode, ITriggerNode, IDisposable
{
    ~TimeIntervalTriggerNode()
    {
        this.SafeDispose();
    }
    public TimeIntervalTriggerNode(string id, Point? position = null) : base(id, position) { Title = "TimeIntervalTriggerNode"; Placeholder = "TimeIntervalTriggerNode.Placeholder"; }

    private IScheduledTask _task;
    private Func<NodeOutput, CancellationToken, Task> Func { get; set; }
    Task ITriggerNode.StartAsync(Func<NodeOutput, CancellationToken, Task> func, CancellationToken cancellationToken)
    {
        Func = func;
        if (int.TryParse(Text, out int delay))
        {
            if (delay <= 500)
                Text = "500";
        }
        _task = ScheduledTaskHelper.GetTask(Text, Timer, null, Logger, cancellationToken);
        _task.Start();
        return Task.CompletedTask;
    }

    private async Task Timer(object? state, CancellationToken cancellationToken)
    {
        try
        {
            if (Func != null)
            {
                Logger?.Trace($"Timer: {Text}");
                await Func.Invoke(new NodeOutput(), cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex);
        }
    }

    public void Dispose()
    {
        _task?.Stop();
        _task?.TryDispose();

        GC.SuppressFinalize(this);
    }




}
