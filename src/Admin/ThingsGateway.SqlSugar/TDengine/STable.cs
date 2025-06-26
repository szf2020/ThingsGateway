namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// STable 基础类
    /// </summary>
    public class STable
    {
        /// <summary>
        /// 标签类型ID(标记为不映射到数据库)
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string TagsTypeId { get; set; }

        /// <summary>
        /// 列标签信息列表
        /// </summary>
        public static List<ColumnTagInfo> Tags = null;
    }

    /// <summary>
    /// 列标签信息类
    /// </summary>
    public class ColumnTagInfo
    {
        /// <summary>
        /// 标签名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 标签值
        /// </summary>
        public string Value { get; set; }
    }
}