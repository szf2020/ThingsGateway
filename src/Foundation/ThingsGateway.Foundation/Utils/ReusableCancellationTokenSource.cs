//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation;

using System;
using System.Threading;

public sealed class ReusableCancellationTokenSource : IDisposable
{
    private readonly Timer _timer;
    private readonly object _lock = new();
    private CancellationTokenSource? _cts;

    public ReusableCancellationTokenSource()
    {
        _timer = new Timer(OnTimeout, null, Timeout.Infinite, Timeout.Infinite);
    }

    public bool TimeoutStatus = false;

    private void OnTimeout(object? state)
    {
        lock (_lock)
        {
            TimeoutStatus = true;

            if (_cts?.IsCancellationRequested == false)
                _cts?.Cancel();

        }
    }
    /// <summary>
    /// 获取一个 CTS，并启动超时
    /// </summary>
    public CancellationTokenSource GetTokenSource(long timeout, CancellationToken external1 = default)
    {
        lock (_lock)
        {
            TimeoutStatus = false;

            // 如果已有 CTS，先 Dispose
            _cts?.SafeCancel();
            _cts?.SafeDispose();

            // 创建新的 CTS
            _cts = CancellationTokenSource.CreateLinkedTokenSource(external1);

            // 启动 Timer
            _timer.Change(timeout, Timeout.Infinite);

            return _cts;
        }
    }

    /// <summary>
    /// 获取一个 CTS，并启动超时
    /// </summary>
    public CancellationTokenSource GetTokenSource(long timeout, CancellationToken external1 = default, CancellationToken external2 = default, CancellationToken external3 = default)
    {
        lock (_lock)
        {
            TimeoutStatus = false;

            // 如果已有 CTS，先 Dispose
            _cts?.SafeCancel();
            _cts?.SafeDispose();

            // 创建新的 CTS
            _cts = CancellationTokenSource.CreateLinkedTokenSource(external1, external2, external3);

            // 启动 Timer
            _timer.Change(timeout, Timeout.Infinite);

            return _cts;
        }
    }


    public void Set()
    {
        _timer?.Change(Timeout.Infinite, Timeout.Infinite);
    }

    /// <summary>
    /// 手动取消
    /// </summary>
    public void Cancel()
    {
        lock (_lock)
        {
            _cts?.SafeCancel();
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            _cts?.SafeCancel();
            _cts?.SafeDispose();
            _timer.SafeDispose();
        }
    }
}



