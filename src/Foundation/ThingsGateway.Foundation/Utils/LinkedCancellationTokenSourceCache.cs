//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation;

public class LinkedCancellationTokenSourceCache : IDisposable
{
    private CancellationTokenSource? _cachedCts;
    private CancellationToken _token1;
    private CancellationToken _token2;
    private CancellationToken _token3;
    private readonly object _lock = new();
    ~LinkedCancellationTokenSourceCache()
    {
        Dispose();
    }

    /// <summary>
    /// 获取一个 CancellationTokenSource，它是由两个 token 链接而成的。
    /// 会尝试复用之前缓存的 CTS，前提是两个 token 仍然相同且未取消。
    /// </summary>
    public CancellationTokenSource GetLinkedTokenSource(CancellationToken token1, CancellationToken token2, CancellationToken token3 = default)
    {
        lock (_lock)
        {
            // 如果缓存的 CTS 已经取消或 Dispose，或者 token 不同，重新创建
            if (_cachedCts?.IsCancellationRequested != false ||
                !_token1.Equals(token1) || !_token2.Equals(token2) || !_token3.Equals(token3))
            {
#if NET6_0_OR_GREATER
                if (_cachedCts?.TryReset() != true)
                {
                    _cachedCts?.Dispose();
                    _cachedCts = CancellationTokenSource.CreateLinkedTokenSource(token1, token2, token3);
                }
#else
                _cachedCts?.Dispose();

                _cachedCts = CancellationTokenSource.CreateLinkedTokenSource(token1, token2, token3);
#endif


                _token1 = token1;
                _token2 = token2;
                _token3 = token3;
            }

            return _cachedCts;
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            _cachedCts?.Dispose();
            _cachedCts = null!;
        }
    }
}


