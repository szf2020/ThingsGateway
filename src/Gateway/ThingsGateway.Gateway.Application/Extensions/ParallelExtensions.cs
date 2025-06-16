//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 扩展
/// </summary>
[ThingsGateway.DependencyInjection.SuppressSniffer]
public static class ParallelExtension
{

    /// <summary>
    /// 异步执行指定的操作，并指定最大并行度和取消标志
    /// </summary>
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <param name="source">要操作的集合</param>
    /// <param name="body">异步执行的操作</param>
    /// <param name="cancellationToken">取消操作的标志</param>
    /// <returns>表示异步操作的任务</returns>
    public static Task ParallelForEachStreamedAsync<T>(this IEnumerable<T> source, Func<T, CancellationToken, ValueTask> body, CancellationToken cancellationToken = default)
    {
        return ParallelForEachStreamedAsync(source, body, Environment.ProcessorCount, cancellationToken);
    }

    public static Task ParallelForEachStreamedAsync<T>(this IEnumerable<T> source, Func<T, CancellationToken, ValueTask> body, int parallelCount, CancellationToken cancellationToken = default)
    {
        // 创建并行操作的选项对象，设置最大并行度和取消标志
        var options = new ParallelOptions();
        options.CancellationToken = cancellationToken;
        options.MaxDegreeOfParallelism = parallelCount == 0 ? 1 : parallelCount;
        // 使用 Parallel.ForEachAsync 异步执行指定的操作，并返回表示异步操作的任务
        return Parallel.ForEachAsync(source, options, body);
    }
}
