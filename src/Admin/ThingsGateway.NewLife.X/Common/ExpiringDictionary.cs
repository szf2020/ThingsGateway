using System.Collections.Concurrent;

using ThingsGateway.NewLife.Threading;
namespace ThingsGateway.NewLife;

public class ExpiringDictionary<TKey, TValue> : IDisposable
{
    private ConcurrentDictionary<TKey, TValue> _dict = new();
    private readonly TimerX _cleanupTimer;

    public ExpiringDictionary(int cleanupInterval = 60000)
    {
        _cleanupTimer = new TimerX(Clear, null, cleanupInterval, cleanupInterval) { Async = true };
    }

    public void TryAdd(TKey key, TValue value)
    {
        if (_cleanupTimer.Disposed) throw new ObjectDisposedException(nameof(ExpiringDictionary<TKey, TValue>));
        _dict.TryAdd(key, value);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (_cleanupTimer.Disposed) throw new ObjectDisposedException(nameof(ExpiringDictionary<TKey, TValue>));
        return _dict.TryGetValue(key, out value);
    }
    public TValue GetOrAdd(TKey key, Func<TKey, TValue> func)
    {
        if (_cleanupTimer.Disposed) throw new ObjectDisposedException(nameof(ExpiringDictionary<TKey, TValue>));
        return _dict.GetOrAdd(key, func);
    }
    public TValue GetOrAdd(TKey key, TValue value)
    {
        if (_cleanupTimer.Disposed) throw new ObjectDisposedException(nameof(ExpiringDictionary<TKey, TValue>));
        return _dict.GetOrAdd(key, value);
    }

    public bool TryRemove(TKey key) => _dict.TryRemove(key, out _);

    public void Clear() => Clear(null);

    private void Clear(object? state)
    {
        var data = _dict;
        _dict = new();
        data.Clear();
    }

    public void Dispose()
    {
        _dict.Clear();
        _cleanupTimer.Dispose();
    }
}
