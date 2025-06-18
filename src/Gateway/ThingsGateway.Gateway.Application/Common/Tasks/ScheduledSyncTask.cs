using ThingsGateway.NewLife;
using ThingsGateway.NewLife.Threading;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

public class ScheduledSyncTask : DisposeBase, IScheduledTask, IScheduledIntIntervalTask
{
    private int _interval10MS = 10;
    public int IntervalMS { get; }
    private readonly Action<object?, CancellationToken> _taskAction;
    private readonly CancellationToken _token;
    private TimerX? _timer;
    private object? _state;
    private ILog LogMessage;
    private volatile int _isRunning = 0;
    private volatile int _pendingTriggers = 0;

    public ScheduledSyncTask(int interval, Action<object?, CancellationToken> taskFunc, object? state, ILog log, CancellationToken token)
    {
        IntervalMS = interval;
        LogMessage = log;
        _state = state;
        _taskAction = taskFunc;
        _token = token;
    }

    public void Start()
    {
        _timer?.Dispose();
        if (!_token.IsCancellationRequested)
            _timer = new TimerX(TimerCallback, _state, IntervalMS, IntervalMS, nameof(IScheduledTask)) { Async = true };
    }

    private void TimerCallback(object? state)
    {
        if (_token.IsCancellationRequested)
            return;

        Interlocked.Increment(ref _pendingTriggers);

        if (Interlocked.Exchange(ref _isRunning, 1) == 1)
            return;
        Do(state);
    }

    private void Do(object? state)
    {
        // 减少一个触发次数
        Interlocked.Decrement(ref _pendingTriggers);

        try
        {
            _taskAction(state, _token);
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

        if (Interlocked.Exchange(ref _pendingTriggers, 0) >= 1)
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
        if (!_token.IsCancellationRequested)
            _timer?.SetNext(_interval10MS);
    }

    public void Stop()
    {
        _timer?.Dispose();
        _timer = null;
    }

    protected override void Dispose(bool disposing)
    {
        Stop();
        base.Dispose(disposing);
    }
}