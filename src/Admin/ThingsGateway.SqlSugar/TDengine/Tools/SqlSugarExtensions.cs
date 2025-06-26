using System.Reflection;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// SqlSugar 扩展方法
    /// </summary>
    public static class SqlSugarExtensions
    {
        /// <summary>
        /// 将查询转换为 TDengine 超级表查询
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="queryable">可查询对象</param>
        /// <returns>转换后的可查询对象</returns>
        public static ISugarQueryable<T> AsTDengineSTable<T>(this ISugarQueryable<T> queryable) where T : class, new()
        {
            var attr = TaosUtilMethods.GetCommonSTableAttribute(queryable.Context, typeof(T).GetCustomAttribute<STableAttribute>());
            queryable.AS(attr.STableName);
            return queryable;
        }

        /// <summary>
        /// 将删除操作转换为 TDengine 超级表删除
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="queryable">可删除对象</param>
        /// <returns>转换后的可删除对象</returns>
        public static IDeleteable<T> AsTDengineSTable<T>(this IDeleteable<T> queryable) where T : class, new()
        {
            var attr = TaosUtilMethods.GetCommonSTableAttribute(((DeleteableProvider<T>)queryable).Context, typeof(T).GetCustomAttribute<STableAttribute>());
            queryable.AS(attr.STableName);
            return queryable;
        }

        /// <summary>
        /// 映射超级表名称
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="db">数据库客户端</param>
        /// <param name="newSTableName">新的超级表名称</param>
        public static void MappingSTableName<T>(this ISqlSugarClient db, string newSTableName)
        {
            STableAttribute sTableAttribute = typeof(T).GetCustomAttribute<STableAttribute>();
            if (db.TempItems == null)
            {
                db.TempItems = new Dictionary<string, object>();
            }
            if (sTableAttribute != null)
            {
                var key = "GetCommonSTableAttribute_" + sTableAttribute.STableName;
                db.TempItems.Remove(key);
                db.TempItems.Add(key, newSTableName);
            }
        }
    }
}