// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using BenchmarkDotNet.Attributes;

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Sources;

using TouchSocket.Core;

namespace BenchmarkConsoleApp;


[MemoryDiagnoser]
public class BenchmarkAsyncWaitData
{
    private int Count = 100000;

    [Benchmark]
    public async Task RunAsyncWaitDataPool()
    {
        var waitHandlePool = new WaitHandlePool<MyWaitData>();
        var cts = new CancellationTokenSource(1000 * 60);
        for (var i = 0; i < this.Count; i++)
        {
            var data = new MyWaitData();
            using (var waitData = waitHandlePool.GetWaitDataAsync(data))
            {
                var task = Task.Run(() =>
                {
                    waitHandlePool.Set(data);
                });


                await waitData.WaitAsync(cts.Token).ConfigureAwait(false);

                await task;
            }
        }

    }

    [Benchmark]
    public async Task RunAsyncWaitData()
    {
        var waitHandlePool = new WaitHandlePool2<MyWaitData>();
        var cts = new CancellationTokenSource(1000 * 60);
        for (var i = 0; i < this.Count; i++)
        {
            var data = new MyWaitData();
            using (var waitData = waitHandlePool.GetWaitDataAsync(data))
            {
                var task = Task.Run(() =>
                {
                    waitHandlePool.Set(data);
                });


                await waitData.WaitAsync(cts.Token).ConfigureAwait(false);

                await task;
            }
        }
    }

    [Benchmark]
    public async Task RunAsyncWaitDataDelayPool()
    {
        var waitHandlePool = new WaitHandlePool<MyWaitData>();
        var cts = new CancellationTokenSource(1000 * 60);
        for (var i = 0; i < this.Count; i++)
        {
            var data = new MyWaitData();
            using (var waitData = waitHandlePool.GetWaitDataAsync(data))
            {
                var task = waitData.WaitAsync(cts.Token).ConfigureAwait(false);

                waitData.Set(data);

                await task;
            }
        }

    }

    [Benchmark]
    public async Task RunAsyncWaitDataDelay()
    {
        var waitHandlePool = new WaitHandlePool2<MyWaitData>();
        var cts = new CancellationTokenSource(1000 * 60);
        for (var i = 0; i < this.Count; i++)
        {
            var data = new MyWaitData();
            using (var waitData = waitHandlePool.GetWaitDataAsync(data))
            {
                var task = waitData.WaitAsync(cts.Token).ConfigureAwait(false);

                waitData.Set(data);

                await task;
            }
        }

    }

    private class MyWaitData : IWaitHandle
    {
        public int Sign { get; set; }
    }

    public sealed class WaitHandlePool2<T>
    where T : class, IWaitHandle
    {
        private readonly int m_maxSign;
        private readonly int m_minSign;
        private readonly ConcurrentDictionary<int, AsyncWaitData2<T>> m_waitDic = new();
        private readonly Action<int> _remove;
        private int m_currentSign;

        /// <summary>
        /// 初始化<see cref="WaitHandlePool{T}"/>类的新实例。
        /// </summary>
        /// <param name="minSign">签名的最小值，默认为1。</param>
        /// <param name="maxSign">签名的最大值，默认为<see cref="int.MaxValue"/>。</param>
        /// <remarks>
        /// 签名范围用于控制自动生成的唯一标识符的取值范围。
        /// 当签名达到最大值时，会自动重置到最小值重新开始分配。
        /// </remarks>
        public WaitHandlePool2(int minSign = 1, int maxSign = int.MaxValue)
        {
            this.m_minSign = minSign;
            this.m_currentSign = minSign;
            this.m_maxSign = maxSign;

            this._remove = this.Remove;
        }

        /// <summary>
        /// 取消池中所有等待操作。
        /// </summary>
        /// <remarks>
        /// 此方法会遍历池中所有的等待数据，并调用其<see cref="AsyncWaitData{T}.Cancel"/>方法来取消等待。
        /// 取消后的等待数据会从池中移除。适用于应用程序关闭或需要批量取消所有等待操作的场景。
        /// </remarks>
        public void CancelAll()
        {
            var signs = this.m_waitDic.Keys.ToList();
            foreach (var sign in signs)
            {
                if (this.m_waitDic.TryRemove(sign, out var item))
                {
                    item.Cancel();
                }
            }
        }

        /// <summary>
        /// 获取与指定结果关联的异步等待数据。
        /// </summary>
        /// <param name="result">要关联的结果对象。</param>
        /// <param name="autoSign">指示是否自动为结果对象分配签名，默认为<see langword="true"/>。</param>
        /// <returns>创建的<see cref="AsyncWaitData{T}"/>实例。</returns>
        /// <exception cref="InvalidOperationException">当指定的签名已被使用时抛出。</exception>
        /// <remarks>
        /// 如果<paramref name="autoSign"/>为<see langword="true"/>，方法会自动为结果对象生成唯一签名。
        /// 创建的等待数据会被添加到池中，直到被设置结果或取消时才会移除。
        /// </remarks>
        public AsyncWaitData2<T> GetWaitDataAsync(T result, bool autoSign = true)
        {
            if (autoSign)
            {
                result.Sign = this.GetSign();
            }
            var waitDataAsyncSlim = new AsyncWaitData2<T>(result.Sign, this._remove, result);

            if (!this.m_waitDic.TryAdd(result.Sign, waitDataAsyncSlim))
            {
                //ThrowHelper.ThrowInvalidOperationException($"The sign '{result.Sign}' is already in use.");
                return default;
            }
            return waitDataAsyncSlim;
        }

        /// <summary>
        /// 获取具有自动生成签名的异步等待数据。
        /// </summary>
        /// <param name="sign">输出参数，返回自动生成的签名值。</param>
        /// <returns>创建的<see cref="AsyncWaitData{T}"/>实例。</returns>
        /// <exception cref="InvalidOperationException">当生成的签名已被使用时抛出。</exception>
        /// <remarks>
        /// 此方法会自动生成唯一签名，并创建不包含挂起数据的等待对象。
        /// 适用于只需要等待通知而不关心具体数据内容的场景。
        /// </remarks>
        public AsyncWaitData2<T> GetWaitDataAsync(out int sign)
        {
            sign = this.GetSign();
            var waitDataAsyncSlim = new AsyncWaitData2<T>(sign, this._remove, default);
            if (!this.m_waitDic.TryAdd(sign, waitDataAsyncSlim))
            {
                return default;
            }
            return waitDataAsyncSlim;
        }

        /// <summary>
        /// 使用指定结果设置对应签名的等待操作。
        /// </summary>
        /// <param name="result">包含签名和结果数据的对象。</param>
        /// <returns>如果成功设置等待操作则返回<see langword="true"/>；否则返回<see langword="false"/>。</returns>
        /// <remarks>
        /// 此方法根据结果对象的签名查找对应的等待数据，并设置其结果。
        /// 设置成功后，等待数据会从池中移除，正在等待的任务会被完成。
        /// 如果找不到对应签名的等待数据，则返回<see langword="false"/>。
        /// </remarks>
        public bool Set(T result)
        {
            if (this.m_waitDic.TryRemove(result.Sign, out var waitDataAsync))
            {
                waitDataAsync.Set(result);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 尝试获取指定签名的异步等待数据。
        /// </summary>
        /// <param name="sign">要查找的签名。</param>
        /// <param name="waitDataAsync">输出参数，如果找到则返回对应的等待数据；否则为<see langword="null"/>。</param>
        /// <returns>如果找到指定签名的等待数据则返回<see langword="true"/>；否则返回<see langword="false"/>。</returns>
        /// <remarks>
        /// 此方法允许查询池中是否存在特定签名的等待数据，而不会修改池的状态。
        /// 适用于需要检查等待状态或获取等待数据进行进一步操作的场景。
        /// </remarks>
        public bool TryGetDataAsync(int sign, out AsyncWaitData2<T> waitDataAsync)
        {
            return this.m_waitDic.TryGetValue(sign, out waitDataAsync);
        }

        /// <summary>
        /// 生成下一个可用的唯一签名。
        /// </summary>
        /// <returns>生成的唯一签名值。</returns>
        /// <remarks>
        /// 使用原子递增操作确保签名的唯一性和线程安全性。
        /// 当签名达到最大值时，会重新开始分配以避免溢出。
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetSign()
        {
            while (true)
            {
                var currentSign = this.m_currentSign;
                var nextSign = currentSign >= this.m_maxSign ? this.m_minSign : currentSign + 1;

                if (Interlocked.CompareExchange(ref this.m_currentSign, nextSign, currentSign) == currentSign)
                {
                    return nextSign;
                }
                // 如果CAS失败，继续重试
            }
        }

        /// <summary>
        /// 从池中移除指定签名的等待数据。
        /// </summary>
        /// <param name="sign">要移除的签名。</param>
        /// <remarks>
        /// 此方法由等待数据在释放时自动调用，确保池中不会保留已完成或已取消的等待对象。
        /// </remarks>
        private void Remove(int sign)
        {
            this.m_waitDic.TryRemove(sign, out _);
        }
    }

    public sealed class AsyncWaitData2<T> : DisposableObject, IValueTaskSource<WaitDataStatus>
    {
        // ManualResetValueTaskSourceCore 是一个结构体，避免了额外托管对象分配，但需要配合 token 使用。
        private ManualResetValueTaskSourceCore<T> _core; // 核心结构体，不会分配额外对象

        // 缓存的移除回调，由 WaitHandlePool 构造时传入，避免每次分配委托。
        private readonly Action<int> _remove;

        // 挂起时的临时数据
        private readonly T _pendingData;

        // 完成时的数据
        private T _completedData;

        // 当前等待状态（成功/取消/未完成等）
        private WaitDataStatus _status;
        private CancellationTokenRegistration Registration;

        /// <summary>
        /// 使用指定签名和移除回调初始化一个新的 <see cref="AsyncWaitData{T}"/> 实例。
        /// </summary>
        /// <param name="sign">此等待项对应的签名（用于在池中查找）。</param>
        /// <param name="remove">完成或释放时调用的回调，用于将此实例从等待池中移除。</param>
        /// <param name="pendingData">可选的挂起数据，当创建时可以携带一个初始占位数据。</param>
        public AsyncWaitData2(int sign, Action<int> remove, T pendingData)
        {
            this.Sign = sign;
            this._remove = remove;
            this._pendingData = pendingData;
            this._core.RunContinuationsAsynchronously = true; // 确保续体异步执行，避免潜在的栈内联执行问题
        }

        /// <summary>
        /// 获取此等待项的签名标识。
        /// </summary>
        public int Sign { get; }

        /// <summary>
        /// 获取挂起时的原始数据（如果在创建时传入）。
        /// </summary>
        public T PendingData => this._pendingData;

        /// <summary>
        /// 获取已完成时的返回数据。
        /// </summary>
        public T CompletedData => this._completedData;

        /// <summary>
        /// 获取当前等待状态（例如：Success、Canceled 等）。
        /// </summary>
        public WaitDataStatus Status => this._status;

        /// <summary>
        /// 取消当前等待，标记为已取消并触发等待任务的异常（OperationCanceledException）。
        /// </summary>
        public void Cancel()
        {
            this.Set(WaitDataStatus.Canceled, default!);
        }

        /// <summary>
        /// 将等待项设置为成功并携带结果数据。
        /// </summary>
        /// <param name="result">要设置的完成数据。</param>
        public void Set(T result)
        {
            this.Set(WaitDataStatus.Success, result);
        }

        /// <summary>
        /// 设置等待项的状态和数据，并完成对应的 ValueTask。
        /// </summary>
        /// <param name="status">要设置的状态。</param>
        /// <param name="result">要设置的完成数据。</param>
        public void Set(WaitDataStatus status, T result)
        {
            this._status = status;
            this._completedData = result;

            if (status == WaitDataStatus.Canceled)
                this._core.SetException(new OperationCanceledException());
            else
                this._core.SetResult(result);
        }

        /// <summary>
        /// 异步等待此项完成，返回一个 <see cref="ValueTask{WaitDataStatus}"/>，可传入取消令牌以取消等待。
        /// </summary>
        /// <param name="cancellationToken">可选的取消令牌。若触发则会调用 <see cref="Cancel"/>。</param>
        /// <returns>表示等待状态的 ValueTask。</returns>
        public ValueTask<WaitDataStatus> WaitAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.CanBeCanceled)
            {
                this.Registration = cancellationToken.Register(this.Cancel);
            }

            return new ValueTask<WaitDataStatus>(this, this._core.Version);
        }

        /// <summary>
        /// 从核心获取结果（显式接口实现）。
        /// 注意：此方法由 ValueTask 基础设施调用，不应直接在用户代码中调用。
        /// </summary>
        WaitDataStatus IValueTaskSource<WaitDataStatus>.GetResult(short token)
        {
            this._core.GetResult(token);
            return this._status;
        }

        /// <summary>
        /// 获取当前 ValueTask 源的状态（显式接口实现）。
        /// </summary>
        ValueTaskSourceStatus IValueTaskSource<WaitDataStatus>.GetStatus(short token)
            => this._core.GetStatus(token);

        /// <summary>
        /// 注册续体（显式接口实现）。
        /// 注意：flags 可以控制是否捕获上下文等行为。
        /// </summary>
        void IValueTaskSource<WaitDataStatus>.OnCompleted(Action<object?> continuation, object? state,
            short token, ValueTaskSourceOnCompletedFlags flags)
            => this._core.OnCompleted(continuation, state, token, flags);

        /// <summary>
        /// 释放托管资源时调用，会触发传入的移除回调，从所在的等待池中移除此等待项。
        /// </summary>
        /// <param name="disposing">是否为显式释放。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Registration.Dispose();
                this._remove(this.Sign);
            }
            base.Dispose(disposing);
        }
    }

}
