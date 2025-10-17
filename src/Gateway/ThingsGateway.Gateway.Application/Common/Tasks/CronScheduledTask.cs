using PooledAwait;

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

    private ValueTask TimerCallbackAsync(object? state)
    {
        return TimerCallbackAsync(this, state);
        static async PooledValueTask TimerCallbackAsync(CronScheduledTask @this, object? state)
        {
            if (@this.Check()) return;
            if (@this._taskFunc == null && @this._valueTaskFunc == null)
            {
                @this.Dispose();
                return;
            }

            Interlocked.Increment(ref @this._pendingTriggers);

            if (Interlocked.Exchange(ref @this._isRunning, 1) == 1)
                return;

            // 减少一个触发次数
            Interlocked.Decrement(ref @this._pendingTriggers);

            try
            {
                if (@this._taskFunc != null)
                    await @this._taskFunc(state, @this._token).ConfigureAwait(false);
                else if (@this._valueTaskFunc != null)
                    await @this._valueTaskFunc(state, @this._token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                @this.LogMessage?.LogWarning(ex);
            }
            finally
            {
                Interlocked.Exchange(ref @this._isRunning, 0);
            }

            if (Interlocked.Exchange(ref @this._pendingTriggers, 0) >= 1)
            {
                if (!@this.Check())
                {
                    int nextValue = @this.next;
                    @this.SetNext(nextValue);
                }
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
                int nextValue = next;
                SetNext(nextValue);
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
