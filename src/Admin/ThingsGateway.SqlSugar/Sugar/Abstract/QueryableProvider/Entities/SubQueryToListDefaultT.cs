namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 子查询默认返回对象，包含主键和索引信息。
    /// </summary>
    internal class SubQueryToListDefaultT
    {
        /// <summary>
        /// 主键值。
        /// </summary>
        public object Id { get; set; }

        /// <summary>
        /// 索引值。
        /// </summary>
        public object SugarIndex { get; set; }
    }
}
