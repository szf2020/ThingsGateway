using System.Reflection;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 查询格式化信息，用于描述类型、格式、属性和方法等相关信息。
    /// </summary>
    internal class QueryableFormat
    {
        /// <summary>
        /// 关联的类型。
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// 类型的字符串表示。
        /// </summary>
        public string TypeString { get; set; }

        /// <summary>
        /// 格式化字符串。
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// 属性名称。
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// 方法名称。
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// 方法信息。
        /// </summary>
        public MethodInfo MethodInfo { get; set; }
    }
}
