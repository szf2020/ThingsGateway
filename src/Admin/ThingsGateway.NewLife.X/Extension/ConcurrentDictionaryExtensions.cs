using System.Collections.Concurrent;

namespace ThingsGateway.NewLife.Extension;

/// <summary>并发字典扩展</summary>
public static class ConcurrentDictionaryExtensions
{
    /// <summary>从并发字典中删除</summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="dict"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static Boolean Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key) where TKey : notnull => dict.TryRemove(key, out _);

    /// <inheritdoc/>
    public static int RemoveWhere<TKey, TValue>(this IDictionary<TKey, TValue> pairs, Func<KeyValuePair<TKey, TValue>, bool> func)
    {
        // 存储需要移除的键的列表，以便之后统一移除
        var list = new List<TKey>();
        foreach (var item in pairs)
        {
            // 使用提供的函数判断当前项目是否应该被移除
            if (func?.Invoke(item) == true)
            {
                list.Add(item.Key);
            }
        }

        // 记录成功移除的项目数量
        var count = 0;
        foreach (var item in list)
        {
            // 尝试移除项目，如果成功则增加计数
            if (pairs.Remove(item))
            {
                count++;
            }
        }
        // 返回成功移除的项目数量
        return count;
    }
    /// <summary>
    /// 批量出队
    /// </summary>
    public static List<T> ToListWithDequeue<TKEY, T>(this ConcurrentDictionary<TKEY, T> values, int maxCount = 0)
    {
        if (maxCount <= 0)
        {
            maxCount = values.Count;
        }
        else
        {
            maxCount = Math.Min(maxCount, values.Count);
        }

        var list = new List<T>(maxCount);
        if (maxCount == 0) return list;
        var keys = values.Keys;
        foreach (var key in keys)
        {
            if (maxCount-- <= 0) break;
            if (values.TryRemove(key, out var result))
            {
                list.Add(result);
            }
        }
        return list;
    }
    /// <summary>
    /// 批量出队
    /// </summary>
    public static Dictionary<TKEY, T> ToDictWithDequeue<TKEY, T>(this ConcurrentDictionary<TKEY, T> values, int maxCount = 0)
    {
        if (maxCount <= 0)
        {
            maxCount = values.Count;
        }
        else
        {
            maxCount = Math.Min(maxCount, values.Count);
        }

        var dict = new Dictionary<TKEY, T>(maxCount);

        if (maxCount == 0) return dict;

        var keys = values.Keys;
        foreach (var key in keys)
        {
            if (maxCount-- <= 0) break;
            if (values.TryRemove(key, out var result))
            {
                dict.Add(key, result);
            }
        }
        return dict;
    }

}
