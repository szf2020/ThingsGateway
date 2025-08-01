// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

namespace ThingsGateway;


public static class DisposableExtensions
{
    #region IDisposable

    /// <summary>
    /// 安全性释放（不用判断对象是否为空）。不会抛出任何异常。
    /// </summary>
    /// <param name="dis"></param>
    /// <returns>释放状态，当对象为<see langword="null"/>，或者已被释放时，均会返回<see cref="Result.Success"/>，只有实际在释放时遇到异常时，才显示其他状态。</returns>
    public static async Task<Result> SafeDisposeAsync(this IAsyncDisposable dis)
    {
        if (dis == default)
        {
            return Result.Success;
        }
        try
        {
            await dis.DisposeAsync().ConfigureAwait(false);
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    #endregion IDisposable


#if NET8_0_OR_GREATER


    /// <summary>
    /// 安全地取消 <see cref="CancellationTokenSource"/>，并返回操作结果。
    /// </summary>
    /// <param name="tokenSource">要取消的 <see cref="CancellationTokenSource"/>。</param>
    /// <returns>一个 <see cref="Result"/> 对象，表示操作的结果。</returns>
    public static async Task<Result> SafeCancelAsync(this CancellationTokenSource tokenSource)
    {
        if (tokenSource is null)
        {
            return Result.Success;
        }
        try
        {
            await tokenSource.CancelAsync().ConfigureAwait(false);
            return Result.Success;
        }
        catch (ObjectDisposedException)
        {
            return Result.Disposed;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

#endif
}
