using System.Linq.Expressions;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 导航属性追加信息，包含映射关系、追加属性和结果集合。
    /// </summary>
    internal class AppendNavInfo
    {
        /// <summary>
        /// 导航属性与列信息的映射字典。
        /// </summary>
        public Dictionary<string, MappingNavColumnInfo> MappingNavProperties { get; set; } = new Dictionary<string, MappingNavColumnInfo>();

        /// <summary>
        /// 需要追加的属性字典。
        /// </summary>
        public Dictionary<string, string> AppendProperties { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 追加导航属性的结果集合。
        /// </summary>
        public List<AppendNavResult> Result { get; set; } = new List<AppendNavResult>();
    }

    /// <summary>
    /// 追加导航属性的结果，存储结果字典。
    /// </summary>
    internal class AppendNavResult
    {
        /// <summary>
        /// 结果字典。
        /// </summary>
        public Dictionary<string, object> result = new Dictionary<string, object>();
    }

    /// <summary>
    /// 导航属性列的映射信息。
    /// </summary>
    internal class MappingNavColumnInfo
    {
        /// <summary>
        /// 表达式列表。
        /// </summary>
        public List<Expression> ExpressionList { get; set; }

        /// <summary>
        /// 当前属性名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 父级属性名称。
        /// </summary>
        public string ParentName { get; set; }
    }
}
