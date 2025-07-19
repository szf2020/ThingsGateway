namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 查询追加列信息，用于描述在查询中动态追加的列。
    /// </summary>
    internal class QueryableAppendColumn
    {
        /// <summary>
        /// 列的别名。
        /// </summary>
        public string AsName { get; set; }

        /// <summary>
        /// 列的索引位置。
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 列的值。
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 列的名称。
        /// </summary>
        public string Name { get; set; }
    }
}
