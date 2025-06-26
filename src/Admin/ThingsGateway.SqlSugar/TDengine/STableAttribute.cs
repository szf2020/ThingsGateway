namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// STable特性标记，用于标识TDengine的超级表结构
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class STableAttribute : Attribute
    {
        /// <summary>
        /// 标签集合(json)
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        /// 标签1
        /// </summary>
        public string Tag1 { get; set; }

        /// <summary>
        /// 标签2
        /// </summary>
        public string Tag2 { get; set; }

        /// <summary>
        /// 标签3
        /// </summary>
        public string Tag3 { get; set; }

        /// <summary>
        /// 标签4
        /// </summary>
        public string Tag4 { get; set; }

        /// <summary>
        /// 超级表名称
        /// </summary>
        public string STableName { get; set; }
    }
}