using ThingsGateway.NewLife.Log;
using ThingsGateway.NewLife.Reflection;

namespace ThingsGateway.NewLife.Collections;

/// <summary>资源池。支持空闲释放，主要用于数据库连接池和网络连接池</summary>
/// <remarks>
/// 文档 https://newlifex.com/core/object_pool
/// </remarks>
/// <typeparam name="T"></typeparam>
public class ObjectPoolLock<T> : DisposeBase, IPool<T> where T : class
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

    private readonly object _syncRoot = new();

    /// <summary>基础空闲集合。只保存最小个数，最热部分</summary>
    private readonly Stack<T> _free = new();

    /// <summary>扩展空闲集合。保存最小个数以外部分</summary>
    private readonly Queue<T> _free2 = new();

    /// <summary>借出去的放在这</summary>
    private readonly HashSet<T> _busy = new();

    //private readonly Object SyncRoot = new();
    #endregion

    #region 构造
    /// <summary>实例化一个资源池</summary>
    public ObjectPoolLock()
    {
        var str = GetType().Name;
        if (str.Contains('`')) str = str.Substring(null, "`");
        if (str != "Pool")
            Name = str;
        else
            Name = $"Pool<{typeof(T).Name}>";

    }
    ~ObjectPoolLock()
    {
        this.TryDispose();
    }
    /// <summary>销毁</summary>
    /// <param name="disposing"></param>
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

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

            WriteLog($"Init {typeof(T).FullName} Min={Min} Max={Max}");
        }
    }
    #endregion

    #region 主方法
    /// <summary>借出</summary>
    /// <returns></returns>
    public virtual T Get()
    {
        T? pi = null;
        do
        {
            lock (_syncRoot)
            {
                if (_free.Count > 0)
                {
                    pi = _free.Pop();
                    _FreeCount--;
                }
                else if (_free2.Count > 0)
                {
                    pi = _free2.Dequeue();
                    _FreeCount--;
                }
                else
                {
                    var count = BusyCount;
                    if (Max > 0 && count >= Max)
                    {
                        var msg = $"申请失败，已有 {count:n0} 达到或超过最大值 {Max:n0}";
                        WriteLog("Acquire Max " + msg);
                        throw new Exception(Name + " " + msg);
                    }

                    pi = OnCreate();
                    if (count == 0) Init();

#if DEBUG
                    WriteLog("Acquire Create Free={0} Busy={1}", FreeCount, count + 1);
#endif
                }
            }

            // 如果拿到的对象不可用，则重新借
        } while (pi == null || !OnGet(pi));

        lock (_syncRoot)
        {
            // 加入繁忙集合
            _busy.Add(pi);

            _BusyCount++;
        }
        return pi;
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
        lock (_syncRoot)
        {
            // 从繁忙队列找到并移除缓存项
            if (!_busy.Remove(value))
            {
#if DEBUG
                WriteLog("Return Error");
#endif

                return false;
            }

            _BusyCount--;
        }

        // 是否可用
        if (!OnReturn(value))
        {
            return false;
        }

        if (value is DisposeBase db && db.Disposed)
        {
            return false;
        }
        lock (_syncRoot)
        {
            if (_FreeCount < Min)
                _free.Push(value);
            else
                _free2.Enqueue(value);
            _FreeCount++;
        }

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

        lock (_syncRoot)
        {
            count = _FreeCount + _BusyCount;

            while (_free.Count > 0)
            {
                var pi = _free.Pop();
                OnDispose(pi);
            }

            while (_free2.Count > 0)
            {
                var pi = _free2.Dequeue();
                OnDispose(pi);
            }

            _FreeCount = 0;

            foreach (var item in _busy)
            {
                OnDispose(item);
            }
            _busy.Clear();
            _BusyCount = 0;
        }

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
