using System.Collections.Concurrent;

using ThingsGateway.NewLife.Log;
using ThingsGateway.NewLife.Reflection;
using ThingsGateway.NewLife.Threading;

namespace ThingsGateway.NewLife.Collections;

/// <summary>资源池。支持空闲释放，主要用于数据库连接池和网络连接池</summary>
/// <remarks>
/// 文档 https://newlifex.com/core/object_pool
/// </remarks>
/// <typeparam name="T"></typeparam>
public class ObjectPool<T> : DisposeBase, IPool<T> where T : notnull
{
    #region 属性
    /// <summary>名称</summary>
    public String Name { get; set; }

    private Int32 _FreeCount;
    /// <summary>空闲个数</summary>
    public Int32 FreeCount => _FreeCount;

    private Int32 _BusyCount;
    /// <summary>繁忙个数</summary>
    public Int32 BusyCount => _BusyCount;

    /// <summary>最大个数。默认0，0表示无上限</summary>
    public Int32 Max { get; set; } = 0;

    /// <summary>最小个数。默认1</summary>
    public Int32 Min { get; set; } = 1;

    /// <summary>空闲清理时间。最小个数之上的资源超过空闲时间时被清理，默认60s</summary>
    public Int32 IdleTime { get; set; } = 60;

    /// <summary>完全空闲清理时间。最小个数之下的资源超过空闲时间时被清理，默认0s永不清理</summary>
    public Int32 AllIdleTime { get; set; } = 0;

    /// <summary>基础空闲集合。只保存最小个数，最热部分</summary>
    private readonly ConcurrentStack<Item> _free = new();

    /// <summary>扩展空闲集合。保存最小个数以外部分</summary>
    private readonly ConcurrentQueue<Item> _free2 = new();

    /// <summary>借出去的放在这</summary>
    private readonly NonBlockingDictionary<T, Item> _busy = new();

    //private readonly Object SyncRoot = new();
    #endregion

    #region 构造
    /// <summary>实例化一个资源池</summary>
    public ObjectPool()
    {
        var str = GetType().Name;
        if (str.Contains('`')) str = str.Substring(null, "`");
        if (str != "Pool")
            Name = str;
        else
            Name = $"Pool<{typeof(T).Name}>";

        // 启动定期清理的定时器
        StartTimer();
    }
    ~ObjectPool()
    {
        this.TryDispose();
    }
    /// <summary>销毁</summary>
    /// <param name="disposing"></param>
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        _timer.TryDispose();

        WriteLog($"Dispose {typeof(T).FullName} FreeCount={FreeCount:n0} BusyCount={BusyCount:n0}");

        Clear();
    }

    private volatile Boolean _inited;
    private void Init()
    {
        if (_inited) return;

        lock (lockThis)
        {
            if (_inited) return;
            _inited = true;

            WriteLog($"Init {typeof(T).FullName} Min={Min} Max={Max} IdleTime={IdleTime}s AllIdleTime={AllIdleTime}s");
        }
    }
    #endregion

    #region 内嵌
    private sealed class Item
    {
        /// <summary>数值</summary>
        public T? Value { get; set; }

        /// <summary>过期时间</summary>
        public DateTime LastTime { get; set; }
    }
    #endregion

    #region 主方法
    /// <summary>借出</summary>
    /// <returns></returns>
    public virtual T Get()
    {
        Item? pi = null;
        do
        {
            // 从空闲集合借一个
            if (_free.TryPop(out pi) || _free2.TryDequeue(out pi))
            {
                Interlocked.Decrement(ref _FreeCount);
            }
            else
            {
                // 超出最大值后，抛出异常
                var count = BusyCount;
                if (Max > 0 && count >= Max)
                {
                    var msg = $"申请失败，已有 {count:n0} 达到或超过最大值 {Max:n0}";
                    WriteLog("Acquire Max " + msg);
                    throw new Exception(Name + " " + msg);
                }

                // 借不到，增加
                pi = new Item
                {
                    Value = OnCreate(),
                };

                if (count == 0) Init();
#if DEBUG
                WriteLog("Acquire Create Free={0} Busy={1}", FreeCount, count + 1);
#endif

            }

            // 借出时如果不可用，再次借取
        } while (pi.Value == null || !OnGet(pi.Value));

        // 最后时间
        pi.LastTime = TimerX.Now;

        // 加入繁忙集合
        _busy.TryAdd(pi.Value, pi);

        Interlocked.Increment(ref _BusyCount);

        return pi.Value;
    }

    /// <summary>借出时是否可用</summary>
    /// <param name="value"></param>
    /// <returns></returns>
    protected virtual Boolean OnGet(T value) => true;

    /// <summary>申请资源包装项，Dispose时自动归还到池中</summary>
    /// <returns></returns>
    public PoolItem<T> GetItem() => new(this, Get());

    /// <summary>归还</summary>
    /// <param name="value"></param>
    public virtual Boolean Return(T value)
    {
        if (value == null) return false;

        // 从繁忙队列找到并移除缓存项
        if (!_busy.TryRemove(value, out var pi))
        {
#if DEBUG
            WriteLog("Return Error");
#endif

            return false;
        }

        Interlocked.Decrement(ref _BusyCount);

        // 是否可用
        if (!OnReturn(value))
        {
            return false;
        }

        if (value is DisposeBase db && db.Disposed)
        {
            return false;
        }

        var min = Min;

        // 如果空闲数不足最小值，则返回到基础空闲集合
        if (_FreeCount < min /*|| _free.Count < min*/)
            _free.Push(pi);
        else
            _free2.Enqueue(pi);

        // 最后时间
        pi.LastTime = TimerX.Now;

        Interlocked.Increment(ref _FreeCount);



        return true;
    }

    /// <summary>归还时是否可用</summary>
    /// <param name="value"></param>
    /// <returns></returns>
    protected virtual Boolean OnReturn(T value) => true;

    /// <summary>清空已有对象</summary>
    public virtual Int32 Clear()
    {
        var count = _FreeCount + _BusyCount;

        //_busy.Clear();
        //_BusyCount = 0;

        //_free.Clear();
        //while (_free2.TryDequeue(out var rs)) ;
        //_FreeCount = 0;

        while (_free.TryPop(out var pi)) OnDispose(pi.Value);
        while (_free2.TryDequeue(out var pi)) OnDispose(pi.Value);
        _FreeCount = 0;

        foreach (var item in _busy)
        {
            OnDispose(item.Key);
        }
        _busy.Clear();
        _BusyCount = 0;

        return count;
    }

    /// <summary>销毁</summary>
    /// <param name="value"></param>
    protected virtual void OnDispose(T? value) => value.TryDispose();
    #endregion

    #region 重载
    /// <summary>创建实例</summary>
    /// <returns></returns>
    protected virtual T? OnCreate() => (T?)typeof(T).CreateInstance();
    #endregion
    protected object lockThis = new();

    #region 定期清理
    private TimerX? _timer;

    private void StartTimer()
    {
        if (_timer != null) return;
        lock (lockThis)
        {
            if (_timer != null) return;

            _timer = new TimerX(Work, null, 60000, 60000) { Async = true };
        }
    }

    private void Work(Object? state)
    {
        //// 总数小于等于最小个数时不处理
        //if (FreeCount + BusyCount <= Min) return;

        // 遍历并干掉过期项
        var count = 0;

        // 清理过期不还。避免有借没还
        if (AllIdleTime > 0 && !_busy.IsEmpty)
        {
            var exp = TimerX.Now.AddSeconds(-AllIdleTime);
            foreach (var item in _busy)
            {
                if (item.Value.LastTime < exp)
                {
                    if (_busy.TryRemove(item.Key, out _))
                    {
                        // 业务层可能故意有借没还
                        //v.TryDispose();

                        Interlocked.Decrement(ref _BusyCount);
                    }
                }
            }
        }

        // 总数小于等于最小个数时不处理
        if (IdleTime > 0 && !_free2.IsEmpty && FreeCount + BusyCount > Min)
        {
            var exp = TimerX.Now.AddSeconds(-IdleTime);
            // 移除扩展空闲集合里面的超时项
            while (_free2.TryPeek(out var pi) && pi.LastTime < exp)
            {
                // 取出来销毁。在并行操作中，此时返回可能是另一个对象
                if (_free2.TryDequeue(out var pi2))
                {
                    if (pi2.LastTime < exp)
                    {
                        pi2.Value.TryDispose();

                        count++;
                        Interlocked.Decrement(ref _FreeCount);
                    }
                    else
                    {
                        // 可能是另一个对象，放回去
                        _free2.Enqueue(pi2);
                    }
                }
            }
        }

        if (AllIdleTime > 0 && !_free.IsEmpty)
        {
            var exp = TimerX.Now.AddSeconds(-AllIdleTime);
            // 移除基础空闲集合里面的超时项
            while (_free.TryPeek(out var pi) && pi.LastTime < exp)
            {
                // 取出来销毁
                if (_free.TryPop(out var pi2))
                {
                    if (pi2.LastTime < exp)
                    {
                        pi2.Value.TryDispose();

                        count++;
                        Interlocked.Decrement(ref _FreeCount);
                    }
                    else
                    {
                        // 可能是另一个对象，放回去
                        _free.Push(pi2);
                    }
                }
            }
        }

        if (count > 0)
        {

            WriteLog("Release New={6:n0} Release={7:n0} Free={0} Busy={1} 清除过期资源 {2:n0} 项。", FreeCount, BusyCount, count);
        }
    }
    #endregion

    #region 日志
    /// <summary>日志</summary>
    public ILog Log { get; set; } = Logger.Null;

    /// <summary>写日志</summary>
    /// <param name="format"></param>
    /// <param name="args"></param>
    public void WriteLog(String format, params Object?[] args)
    {
        if (Log?.Enable != true) return;

        Log.Info(Name + "." + format, args);
    }
    #endregion
}

/// <summary>资源池包装项，自动归还资源到池中</summary>
/// <typeparam name="T"></typeparam>
public class PoolItem<T> : DisposeBase
{
    #region 属性
    /// <summary>数值</summary>
    public T Value { get; }

    /// <summary>池</summary>
    public IPool<T> Pool { get; }
    #endregion

    #region 构造
    /// <summary>包装项</summary>
    /// <param name="pool"></param>
    /// <param name="value"></param>
    public PoolItem(IPool<T> pool, T value)
    {
        Pool = pool;
        Value = value;
    }

    /// <summary>销毁</summary>
    /// <param name="disposing"></param>
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        Pool.Return(Value);
    }
    #endregion
}