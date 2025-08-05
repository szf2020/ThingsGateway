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

public class AsyncReadWriteLock
{
    private readonly int _writeReadRatio = 3; // 写3次会允许1次读，但写入也不会被阻止，具体协议取决于插件协议实现
    public AsyncReadWriteLock(int writeReadRatio)
    {
        _writeReadRatio = writeReadRatio;
    }
    private AsyncAutoResetEvent _readerLock = new AsyncAutoResetEvent(false); // 控制读计数
    private long _writerCount = 0; // 当前活跃的写线程数
    private long _readerCount = 0; // 当前被阻塞的读线程数
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    /// <summary>
    /// 获取读锁，支持多个线程并发读取，但写入时会阻止所有读取。
    /// </summary>
    public async Task<CancellationToken> ReaderLockAsync(CancellationToken cancellationToken)
    {

        if (Interlocked.Read(ref _writerCount) > 0)
        {
            Interlocked.Increment(ref _readerCount);

            // 第一个读者需要获取写入锁，防止写操作
            await _readerLock.WaitOneAsync(cancellationToken).ConfigureAwait(false);

            Interlocked.Decrement(ref _readerCount);

        }
        return _cancellationTokenSource.Token;
    }

    public bool WriteWaited => _writerCount > 0;

    /// <summary>
    /// 获取写锁，阻止所有读取。
    /// </summary>
    public async Task<IDisposable> WriterLockAsync(CancellationToken cancellationToken)
    {

        if (Interlocked.Increment(ref _writerCount) == 1)
        {
            var cancellationTokenSource = _cancellationTokenSource;
            _cancellationTokenSource = new();
            await cancellationTokenSource.SafeCancelAsync().ConfigureAwait(false); // 取消读取
            cancellationTokenSource.SafeDispose();
        }

        return new Writer(this);
    }
    private object lockObject = new();
    private void ReleaseWriter()
    {
        var writerCount = Interlocked.Decrement(ref _writerCount);
        if (writerCount == 0)
        {
            var resetEvent = _readerLock;
            _readerLock = new(false);
            Interlocked.Exchange(ref _writeSinceLastReadCount, 0);
            resetEvent.SetAll();
        }
        else
        {
            lock (lockObject)
            {

                // 读写占空比， 用于控制写操作与读操作的比率。该比率 n 次写入操作会执行一次读取操作。即使在应用程序执行大量的连续写入操作时，也必须确保足够的读取数据处理时间。相对于更加均衡的读写数据流而言，该特点使得外部写入可连续无顾忌操作
                if (_writeReadRatio > 0)
                {
                    var count = Interlocked.Increment(ref _writeSinceLastReadCount);
                    if (count >= _writeReadRatio)
                    {
                        Interlocked.Exchange(ref _writeSinceLastReadCount, 0);
                        _readerLock.Set();
                    }
                }
                else
                {
                    _readerLock.Set();
                }
            }

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
