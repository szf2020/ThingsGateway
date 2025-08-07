namespace ThingsGateway.Gateway.Application;

public static class ScheduledTaskHelper
{
    public static IScheduledTask GetTask(string interval, Func<object?, CancellationToken, Task> func, object? state, TouchSocket.Core.ILog log, CancellationToken cancellationToken)
    {
        if (int.TryParse(interval, out int intervalV))
        {
            var intervalMilliseconds = intervalV < 10 ? 10 : intervalV;
            return new ScheduledAsyncTask(intervalMilliseconds, func, state, log, cancellationToken);
        }
        else
        {
            return new CronScheduledTask(interval, func, state, log, cancellationToken);
        }
    }
    public static IScheduledTask GetTask(string interval, Func<object?, CancellationToken, ValueTask> func, object? state, TouchSocket.Core.ILog log, CancellationToken cancellationToken)
    {
        if (int.TryParse(interval, out int intervalV))
        {
            var intervalMilliseconds = intervalV < 10 ? 10 : intervalV;
            return new ScheduledAsyncTask(intervalMilliseconds, func, state, log, cancellationToken);
        }
        else
        {
            return new CronScheduledTask(interval, func, state, log, cancellationToken);
        }
    }
    public static IScheduledTask GetTask(string interval, Action<object?, CancellationToken> action, object? state, TouchSocket.Core.ILog log, CancellationToken cancellationToken)
    {
        if (int.TryParse(interval, out int intervalV))
        {
            var intervalMilliseconds = intervalV < 10 ? 10 : intervalV;
            return new ScheduledSyncTask(intervalMilliseconds, action, state, log, cancellationToken);
        }
        else
        {
            return new CronScheduledTask(interval, action, state, log, cancellationToken);
        }
    }
}