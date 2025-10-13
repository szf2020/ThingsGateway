using System.Collections.Concurrent;

using ThingsGateway.NewLife.Threading;
namespace ThingsGateway.NewLife;

public class ExpiringDictionary<TKey, TValue> : IDisposable
{
    /// <summary>缓存项</summary>
    public class CacheItem
    {
        private TValue? _value;
        /// <summary>数值</summary>
        public TValue? Value { get => _value; }

        /// <summary>过期时间。系统启动以来的毫秒数</summary>
        public Int64 ExpiredTime { get; set; }

        /// <summary>是否过期</summary>
        public Boolean Expired => ExpiredTime <= Runtime.TickCount64;

        /// <summary>访问时间</summary>
        public Int64 VisitTime { get; private set; }

        /// <summary>构造缓存项</summary>
        /// <param name="value"></param>
        /// <param name="expire"></param>
        public CacheItem(TValue? value, Int32 expire) => Set(value, expire);

        /// <summary>设置数值和过期时间</summary>
        /// <param name="value"></param>
        /// <param name="expire">过期时间，秒</param>
        public void Set(TValue value, Int32 expire)
        {
            _value = value;

            var now = VisitTime = Runtime.TickCount64;
            if (expire <= 0)
                ExpiredTime = Int64.MaxValue;
            else
                ExpiredTime = now + expire * 1000L;
        }

        /// <summary>更新访问时间并返回数值</summary>
        /// <returns></returns>
        public TValue? Visit()
        {
            VisitTime = Runtime.TickCount64;
            var rs = _value;
            if (rs == null) return default;

            return rs;
        }
    }

    private ConcurrentDictionary<TKey, CacheItem> _dict = new();
    private readonly TimerX _cleanupTimer;
    private int defaultExpire = 60;
    public ExpiringDictionary(int expire = 60)
    {
        defaultExpire = expire;
        _cleanupTimer = new TimerX(TimerClear, null, 10000, 10000) { Async = true };
    }



    public bool TryAdd(TKey key, TValue value)
    {
        if (_dict.TryGetValue(key, out var item))
        {
            if (!item.Expired) return false;
            item.Set(value, defaultExpire);
            return true;
        }
        return _dict.TryAdd(key, new CacheItem(value, defaultExpire));
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        value = default;

        // 没有值，直接结束
        if (!_dict.TryGetValue(key, out var item) || item == null) return false;

        // 得到已有值
        value = item.Visit();

        // 是否未过期的有效值
        return !item.Expired;
    }
    public TValue GetOrAdd(TKey key, Func<TKey, TValue> func)
    {
        CacheItem? item = null;
        do
        {
            if (_dict.TryGetValue(key, out item) && item != null) return item.Visit();

            item ??= new CacheItem(func(key), defaultExpire);
        }
        while (!_dict.TryAdd(key, item));

        return item.Visit();

    }

    public bool TryRemove(TKey key) => _dict.TryRemove(key, out _);

    public void Clear() => Clear(null);

    private void Clear(object? state)
    {
        var data = _dict;
        _dict = new();
        data.Clear();
    }
    private void TimerClear(object? state)
    {

        var dic = _dict;
        if (dic.IsEmpty) return;

        // 60分钟之内过期的数据，进入LRU淘汰
        var now = Runtime.TickCount64;

        // 这里先计算，性能很重要
        var toDels = new List<TKey>();
        foreach (var item in dic)
        {
            // 已过期，准备删除
            var ci = item.Value;
            if (ci.ExpiredTime <= now)
                toDels.Add(item.Key);
        }

        // 确认删除
        foreach (var item in toDels)
        {
            _dict.Remove(item);
        }
    }
    public void Dispose()
    {
        _dict.Clear();
        _cleanupTimer.Dispose();
    }
}
