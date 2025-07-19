namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// SQL 信息类，封装了查询相关的各种参数和表达式。
    /// </summary>
    internal class SqlInfo
    {
        /// <summary>
        /// 是否为导航属性查询。
        /// </summary>
        public bool IsSelectNav { get; set; }

        /// <summary>
        /// 查询返回的最大记录数（Take）。
        /// </summary>
        public int? Take { get; set; }

        /// <summary>
        /// 跳过的记录数（Skip）。
        /// </summary>
        public int? Skip { get; set; }

        /// <summary>
        /// Where 条件字符串。
        /// </summary>
        public string WhereString { get; set; }

        /// <summary>
        /// OrderBy 排序字符串。
        /// </summary>
        public string OrderByString { get; set; }

        /// <summary>
        /// Select 查询字段字符串。
        /// </summary>
        public string SelectString { get; set; }

        /// <summary>
        /// 查询参数集合。
        /// </summary>
        public List<SugarParameter> Parameters { get; set; }

        /// <summary>
        /// 映射字段表达式集合。
        /// </summary>
        public List<MappingFieldsExpression> MappingExpressions { get; set; }

        /// <summary>
        /// 表的短名称（别名）。
        /// </summary>
        public string TableShortName { get; set; }

        /// <summary>
        /// 分表处理委托。
        /// </summary>
        public Func<List<SplitTableInfo>, IEnumerable<SplitTableInfo>> SplitTable { get; set; }
    }
}
