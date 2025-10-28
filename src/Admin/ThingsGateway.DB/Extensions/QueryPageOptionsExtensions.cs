//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

namespace ThingsGateway.DB;

/// <inheritdoc/>
[ThingsGateway.DependencyInjection.SuppressSniffer]
public static class QueryPageOptionsExtensions
{
    /// <summary>
    /// 根据查询条件返回sqlsugar ISugarQueryable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static ISugarQueryable<T> GetQuery<T>(this SqlSugarClient db, QueryPageOptions option, ISugarQueryable<T>? query = null, FilterKeyValueAction where = null)
    {
        query ??= db.Queryable<T>();
        where ??= option.ToFilter();

        if (where.HasFilters())
        {
            query = query.Where(where.GetFilterLambda<T>());//name asc模式
        }

        foreach (var item in option.SortList)
        {
            query = query.OrderByIF(!string.IsNullOrEmpty(item), $"{item}");//name asc模式
        }
        foreach (var item in option.AdvancedSortList)
        {
            query = query.OrderByIF(!string.IsNullOrEmpty(item), $"{item}");//name asc模式
        }
        query = query.OrderByIF(option.SortOrder != SortOrder.Unset, $"{option.SortName} {option.SortOrder}");
        return query;
    }

}
