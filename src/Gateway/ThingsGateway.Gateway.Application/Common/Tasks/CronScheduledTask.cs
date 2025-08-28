using ThingsGateway.NewLife;
using ThingsGateway.NewLife.Threading;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

public class CronScheduledTask : DisposeBase, IScheduledTask
{
    private int next = 10;
    private string _interval;
    private readonly Func<object?, CancellationToken, Task> _taskFunc;
    private readonly Func<object?, CancellationToken, ValueTask> _valueTaskFunc;
    private readonly Action<object?, CancellationToken> _taskAction;
    private readonly CancellationToken _token;
    private TimerX? _timer;
    private object? _state;
    private ILog LogMessage;
    private volatile int _isRunning = 0;
    private volatile int _pendingTriggers = 0;
    public Int32 Period => _timer?.Period ?? 0;
    public bool Enable => _timer?.Disposed != false ? false : true;

    public CronScheduledTask(string interval, Func<object?, CancellationToken, Task> taskFunc, object? state, ILog log, CancellationToken token)
    {
        _interval = interval;
        LogMessage = log;
        _state = state;
        _taskFunc = taskFunc;
        _token = token;
    }
    public CronScheduledTask(string interval, Func<object?, CancellationToken, ValueTask> taskFunc, object? state, ILog log, CancellationToken token)
    {
        _interval = interval;
        LogMessage = log;
        _state = state;
        _valueTaskFunc = taskFunc;
        _token = token;
    }



    public CronScheduledTask(string interval, Action<object?, CancellationToken> taskAction, object? state, ILog log, CancellationToken token)
    {
        _interval = interval;
        LogMessage = log;
        _state = state;
        _taskAction = taskAction;
        _token = token;
    }
    private bool Check()
    {
        if (_token.IsCancellationRequested)
        {
            Dispose();
            return true;
        }
        return false;
    }
    public void Start()
    {
        _timer?.Dispose();
        if (Check()) return;
        if (_taskAction != null)
            _timer = new TimerX(TimerCallback, _state, _interval, nameof(CronScheduledTask)) { Async = true, Reentrant = false };
        else if (_taskFunc != null || _valueTaskFunc != null)
            _timer = new TimerX(TimerCallbackAsync, _state, _interval, nameof(CronScheduledTask)) { Async = true, Reentrant = false };
    }

    private async ValueTask TimerCallbackAsync(object? state)
    {
        if (Check()) return;
        if (_taskFunc == null && _valueTaskFunc == null)
        {
            Dispose();
            return;
        }

        Interlocked.Increment(ref _pendingTriggers);

        if (Interlocked.Exchange(ref _isRunning, 1) == 1)
            return;

        // 减少一个触发次数
        Interlocked.Decrement(ref _pendingTriggers);

        try
        {
            if (_taskFunc != null)
                await _taskFunc(state, _token).ConfigureAwait(false);
            else if (_valueTaskFunc != null)
                await _valueTaskFunc(state, _token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }
        finally
        {
            Interlocked.Exchange(ref _isRunning, 0);
        }

        if (Interlocked.Exchange(ref _pendingTriggers, 0) >= 1)
        {
            if (!Check())
            {
                SetNext(next);
            }
        }
    }

    private void TimerCallback(object? state)
    {
        if (Check()) return;
        if (_taskAction == null)
        {
            Dispose();
            return;
        }

        Interlocked.Increment(ref _pendingTriggers);

        if (Interlocked.Exchange(ref _isRunning, 1) == 1)
            return;

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
            LogMessage?.LogWarning(ex);
        }
        finally
        {
            Interlocked.Exchange(ref _isRunning, 0);
        }

        if (Interlocked.Exchange(ref _pendingTriggers, 0) >= 1)
        {
            if (!Check())
            {
                SetNext(next);
            }
        }
    }

    public void SetNext(int interval)
    {
        // 延迟触发下一次
        if (!Check())
            _timer?.SetNext(interval);
    }
    public bool Change(int dueTime, int period)
    {
        // 延迟触发下一次
        if (!Check())
            return _timer?.Change(dueTime, period) == true;

        return false;
    }
    public void Stop()
    {
        _timer?.SafeDispose();
        _timer = null;
    }

    protected override void Dispose(bool disposing)
    {
        Stop();
        base.Dispose(disposing);
    }
}
