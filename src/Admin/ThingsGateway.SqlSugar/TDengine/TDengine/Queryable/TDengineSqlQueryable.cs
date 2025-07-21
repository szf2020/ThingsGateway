namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// TDengine 单表查询提供类
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public class TDengineQueryable<T> : QueryableProvider<T>
    {
        /// <summary>
        /// 设置 WITH 子句 (TDengine 不支持，直接返回当前对象)
        /// </summary>
        /// <param name="withString">WITH 子句内容</param>
        /// <returns>当前查询对象</returns>
        public override ISugarQueryable<T> With(string withString)
        {
            return this;
        }

        /// <summary>
        /// 设置 PARTITION BY 子句 (转换为 GROUP BY)
        /// </summary>
        /// <param name="groupFields">分组字段</param>
        /// <returns>当前查询对象</returns>
        public override ISugarQueryable<T> PartitionBy(string groupFields)
        {
            this.GroupBy(groupFields);
            return this;
        }
    }

    public class TDengineQueryable<T, T2> : QueryableProvider<T, T2>
    {
        public new ISugarQueryable<T, T2> With(string withString)
        {
            return this;
        }
    }
    public class TDengineQueryable<T, T2, T3> : QueryableProvider<T, T2, T3>
    {
    }
    public class TDengineQueryable<T, T2, T3, T4> : QueryableProvider<T, T2, T3, T4>
    {
    }
    public class TDengineQueryable<T, T2, T3, T4, T5> : QueryableProvider<T, T2, T3, T4, T5>
    {
    }
    public class TDengineQueryable<T, T2, T3, T4, T5, T6> : QueryableProvider<T, T2, T3, T4, T5, T6>
    {
    }
    public class TDengineQueryable<T, T2, T3, T4, T5, T6, T7> : QueryableProvider<T, T2, T3, T4, T5, T6, T7>
    {
    }
    public class TDengineQueryable<T, T2, T3, T4, T5, T6, T7, T8> : QueryableProvider<T, T2, T3, T4, T5, T6, T7, T8>
    {
    }
    public class TDengineQueryable<T, T2, T3, T4, T5, T6, T7, T8, T9> : QueryableProvider<T, T2, T3, T4, T5, T6, T7, T8, T9>
    {
    }
    public class TDengineQueryable<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> : QueryableProvider<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>
    {
    }
    public class TDengineQueryable<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : QueryableProvider<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
    {
    }
    public class TDengineQueryable<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : QueryableProvider<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
    {
    }
}
