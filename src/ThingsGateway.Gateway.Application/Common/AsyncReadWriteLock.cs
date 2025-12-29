//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using TouchSocket.Core;


namespace ThingsGateway.Gateway.Application;

public class AsyncReadWriteLock : IDisposable
{
    private readonly int _writeReadRatio = 3; // 写3次会允许1次读，但写入也不会被阻止，具体协议取决于插件协议实现
    public AsyncReadWriteLock(int writeReadRatio, bool writePriority)
    {
        _writeReadRatio = writeReadRatio;
        _writePriority = writePriority;
    }
    private bool _writePriority;
    private ThingsGateway.Gateway.Application.AsyncAutoResetEvent _readerGate = new ThingsGateway.Gateway.Application.AsyncAutoResetEvent(false); // 控制读计数
    private long _writerCount = 0; // 当前活跃的写线程数
    private long _readerCount = 0; // 当前被阻塞的读线程数
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    /// <summary>
    /// 获取读锁，支持多个线程并发读取，但写入时会阻止所有读取。
    /// </summary>
    public ValueTask<CancellationToken> ReaderLockAsync(CancellationToken cancellationToken)
    {
        return ReaderLockAsync(this, cancellationToken);
        static async PooledValueTask<CancellationToken> ReaderLockAsync(
        AsyncReadWriteLock @this,
        CancellationToken cancellationToken)
        {
            if (Interlocked.Read(ref @this._writerCount) > 0)
            {

                Task task = null;
                lock (@this.lockObject)
                {
                    //AsyncAutoResetEvent加入队列是同步的，所以不会担心并发task未等待导致的问题
                    if (Interlocked.Read(ref @this._writerCount) > 0)
                        task = @this._readerGate.WaitOneAsync(cancellationToken);
                }

                if (task != null)
                {
                    try
                    {
                        Interlocked.Increment(ref @this._readerCount);
                        await task.ConfigureAwait(false);
                        Interlocked.Decrement(ref @this._readerCount);
                    }
                    finally
                    {
                    }
                }

            }

            return @this._cancellationTokenSource.Token;
        }
    }

    public bool WriteWaited => _writerCount > 0;
    public bool ReadWaited => _readerCount > 0;

    /// <summary>
    /// 获取写锁，阻止所有读取。
    /// </summary>
    public ValueTask<IDisposable> WriterLockAsync()
    {
        return WriterLockAsync(this);

        static async PooledValueTask<IDisposable> WriterLockAsync(AsyncReadWriteLock @this)
        {
            if (Interlocked.Increment(ref @this._writerCount) == 1)
            {
                if (@this._writePriority)
                {
                    var cancellationTokenSource = @this._cancellationTokenSource;
                    @this._cancellationTokenSource = new();
                    await cancellationTokenSource.SafeCancelAsync().ConfigureAwait(false); // 取消读取
                    cancellationTokenSource.SafeDispose();
                }
            }

            return new Writer(@this);
        }
    }


    private object lockObject = new();
    private void ReleaseWriter()
    {
        var writerCount = Interlocked.Decrement(ref _writerCount);

        lock (lockObject)
        {
            if (writerCount == 0)
            {
                _writeSinceLastReadCount = 0;

                _readerGate.SetAll();
            }
            else
            {


                // 读写占空比， 用于控制写操作与读操作的比率。该比率 n 次写入操作会执行一次读取操作。即使在应用程序执行大量的连续写入操作时，也必须确保足够的读取数据处理时间。相对于更加均衡的读写数据流而言，该特点使得外部写入可连续无顾忌操作
                if (_writeReadRatio > 0)
                {

                    var count = _writeSinceLastReadCount++;
                    if (count >= _writeReadRatio)
                    {
                        _writeSinceLastReadCount = 0;
                        _readerGate.Set();
                    }
                }
                else
                {
                    _readerGate.Set();
                }
            }

        }
    }
    public void Dispose()
    {
        lock (lockObject)
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.SafeCancel();
                _cancellationTokenSource.SafeDispose();
            }
            _readerGate.SetAll();
        }
    }

    private int _writeSinceLastReadCount = 0;
    private struct Writer : IDisposable
    {
        private readonly AsyncReadWriteLock _lock;

        public Writer(AsyncReadWriteLock lockObj)
        {
            _lock = lockObj;
        }

        public void Dispose()
        {
            _lock.ReleaseWriter();
        }
    }
}
