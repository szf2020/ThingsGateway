// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License
// See the LICENSE file in the project root for more information.
// Maintainer: Argo Zhang(argo@live.ca) Website: https://www.blazor.zone

using Microsoft.Extensions.Caching.Memory;

using System.Reflection;
namespace ThingsGateway;

internal static class CacheManagerHelpers
{
    /// <summary>
    /// Sets default sliding expiration if no expiration is configured
    /// </summary>
    internal static void SetDefaultSlidingExpiration(this ICacheEntry entry, TimeSpan offset)
    {
        if (entry.SlidingExpiration == null && entry.AbsoluteExpiration == null
            && entry.AbsoluteExpirationRelativeToNow == null
            && entry.Priority != CacheItemPriority.NeverRemove)
        {
            entry.SetSlidingExpiration(offset);
        }
    }

    /// <summary>
    /// 获得唯一类型名称方法
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static string GetUniqueName(this Assembly assembly) => assembly.IsCollectible
        ? $"{assembly.GetName().Name}-{assembly.GetHashCode()}"
        : $"{assembly.GetName().Name}";

    /// <summary>
    /// 获得唯一类型名称方法
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetUniqueTypeName(this Type type) => type.IsCollectible
        ? $"{type.FullName}-{type.TypeHandle.Value}"
        : $"{type.FullName}";
}