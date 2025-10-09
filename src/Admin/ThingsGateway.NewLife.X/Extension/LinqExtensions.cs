//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Collections.Concurrent;

namespace ThingsGateway.Extension.Generic;

/// <inheritdoc/>
public static class LinqExtensions
{
    /// <summary>
    /// 将序列分批，每批固定数量
    /// </summary>
    public static IEnumerable<List<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (batchSize <= 0) throw new ArgumentOutOfRangeException(nameof(batchSize));

        List<T> batch = new List<T>(batchSize);
        foreach (var item in source)
        {
            batch.Add(item);
            if (batch.Count >= batchSize)
            {
                yield return batch;
                batch = new List<T>(batchSize);
            }
        }

        // 剩余不足 batchSize 的最后一批
        if (batch.Count > 0)
            yield return batch;
    }

    /// <inheritdoc/>
    public static ICollection<T> AddIF<T>(this ICollection<T> thisValue, bool isOk, Func<T> predicate)
    {
        if (isOk)
        {
            thisValue.Add(predicate());
        }

        return thisValue;
    }

    /// <inheritdoc/>
    public static void RemoveWhere<T>(this ICollection<T> @this, Func<T, bool> @where)
    {
        var del = new List<T>();
        foreach (var obj in @this.Where(where))
        {
            del.Add(obj);
        }
        foreach (var obj in del)
        {
            @this.Remove(obj);
        }
    }
    /// <inheritdoc/>
    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> thisValue, bool isOk, Func<T, bool> predicate)
    {
        if (isOk)
        {
            thisValue = thisValue.Where(predicate);
        }
        return thisValue;
    }

    /// <inheritdoc/>
    public static void AddRange<TKey, TItem>(this Dictionary<TKey, TItem> @this, IEnumerable<KeyValuePair<TKey, TItem>> values)
    {
        foreach (var value in values)
        {
            @this.TryAdd(value.Key, value.Value);
        }
    }
    /// <inheritdoc/>
    public static void AddRange<T>(this ICollection<T> @this, IEnumerable<T> values)
    {
        foreach (T value in values)
        {
            @this.Add(value);
        }
    }
    /// <inheritdoc/>
    public static void AddRange<T>(this ICollection<T> @this, params T[] values)
    {
        foreach (T item in values)
        {
            @this.Add(item);
        }
    }

    /// <summary>
    /// 从并发字典中删除
    /// </summary>
    public static bool Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key) where TKey : notnull
    {
        return dict.TryRemove(key, out TValue? _);
    }
}
