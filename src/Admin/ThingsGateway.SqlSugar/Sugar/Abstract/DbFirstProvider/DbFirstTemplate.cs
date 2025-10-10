using System.Text;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 数据库优先模板类
    /// </summary>
    public static class DbFirstTemplate
    {
        #region Template
        /// <summary>
        /// 类模板
        /// </summary>
        public const string ClassTemplate = "{using}\r\n" +
                                               "namespace {Namespace}\r\n" +
                                               "{\r\n" +
                                               "{ClassDescription}{SugarTable}\r\n" +
                                                ClassSpace + "public partial class {ClassName}\r\n" +
                                                ClassSpace + "{\r\n" +
                                                PropertySpace + "public {ClassName}(){\r\n\r\n" +
                                                "{Constructor}\r\n" +
                                                PropertySpace + "}\r\n" +
                                                "{PropertyName}\r\n" +
                                                 ClassSpace + "}\r\n" +
                                                "}\r\n";
        /// <summary>
        /// 类描述模板
        /// </summary>
        public const string ClassDescriptionTemplate =
                                                ClassSpace + "///<summary>\r\n" +
                                                ClassSpace + "///{ClassDescription}" +
                                                ClassSpace + "///</summary>";

        /// <summary>
        /// 属性模板
        /// </summary>
        public const string PropertyTemplate = PropertySpace + "{SugarColumn}\r\n" +
                                                PropertySpace + "public {PropertyType} {PropertyName} {get;set;}\r\n";

        /// <summary>
        /// 属性描述模板
        /// </summary>
        public const string PropertyDescriptionTemplate =
                                                PropertySpace + "/// <summary>\r\n" +
                                                PropertySpace + "/// Desc:{PropertyDescription}\r\n" +
                                                PropertySpace + "/// Default:{DefaultValue}\r\n" +
                                                PropertySpace + "/// Nullable:{IsNullable}\r\n" +
                                                PropertySpace + "/// </summary>";

        /// <summary>
        /// 构造函数模板
        /// </summary>
        public const string ConstructorTemplate = PropertySpace + " this.{PropertyName} ={DefaultValue};\r\n";

        /// <summary>
        /// 命名空间模板
        /// </summary>
        public const string UsingTemplate = "using System;\r\n" +
                                               "using System.Linq;\r\n" +
                                               "using System.Text;" + "\r\n";
        #endregion

        #region Replace Key
        /// <summary>
        /// 命名空间替换键
        /// </summary>
        public const string KeyUsing = "{using}";
        /// <summary>
        /// 命名空间名称替换键
        /// </summary>
        public const string KeyNamespace = "{Namespace}";
        /// <summary>
        /// 类名替换键
        /// </summary>
        public const string KeyClassName = "{ClassName}";
        /// <summary>
        /// 是否可空替换键
        /// </summary>
        public const string KeyIsNullable = "{IsNullable}";
        /// <summary>
        /// Sugar表属性替换键
        /// </summary>
        public const string KeySugarTable = "{SugarTable}";
        /// <summary>
        /// 构造函数替换键
        /// </summary>
        public const string KeyConstructor = "{Constructor}";
        /// <summary>
        /// Sugar列属性替换键
        /// </summary>
        public const string KeySugarColumn = "{SugarColumn}";
        /// <summary>
        /// 属性类型替换键
        /// </summary>
        public const string KeyPropertyType = "{PropertyType}";
        /// <summary>
        /// 属性名替换键
        /// </summary>
        public const string KeyPropertyName = "{PropertyName}";
        /// <summary>
        /// 默认值替换键
        /// </summary>
        public const string KeyDefaultValue = "{DefaultValue}";
        /// <summary>
        /// 类描述替换键
        /// </summary>
        public const string KeyClassDescription = "{ClassDescription}";
        /// <summary>
        /// 属性描述替换键
        /// </summary>
        public const string KeyPropertyDescription = "{PropertyDescription}";
        #endregion

        #region Replace Value
        /// <summary>
        /// Sugar表属性格式化模板
        /// </summary>
        public static readonly CompositeFormat ValueSugarTable = CompositeFormat.Parse(privateValueSugarTable);

        /// <summary>
        /// Sugar表属性原始值
        /// </summary>
        private const string privateValueSugarTable = "\r\n" + ClassSpace + "[SugarTable(\"{0}\")]";

        /// <summary>
        /// Sugar列属性格式化模板
        /// </summary>
        public static readonly CompositeFormat ValueSugarCoulmn = CompositeFormat.Parse(privateValueSugarCoulmn);

        /// <summary>
        /// Sugar列属性原始值
        /// </summary>
        private const string privateValueSugarCoulmn = "\r\n" + PropertySpace + "[SugarColumn({0})]";
        #endregion

        #region Space
        /// <summary>
        /// 属性缩进空格
        /// </summary>
        public const string PropertySpace = "           ";
        /// <summary>
        /// 类缩进空格
        /// </summary>
        public const string ClassSpace = "    ";
        #endregion
    }
}