using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

public class ScheduledTask
{
    private TimeSpan _interval { get; }
    private TimeSpan _interval10 = TimeSpan.FromMilliseconds(10);
    private double _intervalMS { get; }
    private readonly Func<object?, CancellationToken, Task> _taskFunc;
    private readonly CancellationToken _token;
    private Timer? _timer;
    private object? _state;
    private ILog LogMessage;
    private volatile int _isRunning = 0;
    private volatile int _pendingTriggers = 0;

    public ScheduledTask(TimeSpan interval, Func<object?, CancellationToken, Task> taskFunc, object? state, ILog log, CancellationToken token)
    {
        LogMessage = log;
        _state = state;
        _interval = interval;
        _intervalMS = interval.TotalMilliseconds;
        _taskFunc = taskFunc;
        _token = token;
    }

    public void Start()
    {
        _timer?.Dispose();
        _timer = new Timer(TimerCallback, _state, TimeSpan.Zero, _interval);
    }

    private void TimerCallback(object? state)
    {
        _ = Do(state);
    }

    private async Task Do(object? state)
    {
        if (_token.IsCancellationRequested)
            return;

        Interlocked.Exchange(ref _pendingTriggers, 1);

        if (Interlocked.Exchange(ref _isRunning, 1) == 1)
            return;

        try
        {
            await _taskFunc(state, _token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            LogMessage.LogWarning(ex);
        }
        finally
        {
            Interlocked.Exchange(ref _isRunning, 0);
        }

        if (Interlocked.Exchange(ref _pendingTriggers, 0) == 1)
        {
            if (!_token.IsCancellationRequested)
            {
                DelayDo();
            }
        }
    }

    private void DelayDo()
    {
        // 延迟触发下一次
        _timer?.Change(_interval10, _interval);
    }

    public void Change(int dueTime, int period)
    {
        _timer?.Change(dueTime, period);
    }

    public void Stop()
    {
        _timer?.Dispose();
        _timer = null;
    }

}