namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// TDengine 数据库类型绑定提供程序
    /// </summary>
    public class TDengineDbBind : DbBindProvider
    {
        /// <summary>
        /// 根据C#类型名称获取数据库类型名称
        /// </summary>
        /// <param name="csharpTypeName">C#类型名称</param>
        /// <returns>数据库类型名称</returns>
        public override string GetDbTypeName(string csharpTypeName)
        {
            if (csharpTypeName == UtilConstants.ByteArrayType.Name)
                return "bytea";
            if (string.Equals(csharpTypeName, "int32", StringComparison.OrdinalIgnoreCase))
                csharpTypeName = "int";
            if (string.Equals(csharpTypeName, "int16", StringComparison.OrdinalIgnoreCase))
                csharpTypeName = "short";
            if (string.Equals(csharpTypeName, "int64", StringComparison.OrdinalIgnoreCase))
                csharpTypeName = "long";
            if (csharpTypeName.IsInCase("boolean", "bool"))
                csharpTypeName = "bool";
            if (csharpTypeName == "DateTimeOffset")
                csharpTypeName = "DateTime";
            var mappings = this.MappingTypes.Where(it => it.Value.ToString().Equals(csharpTypeName, StringComparison.CurrentCultureIgnoreCase));
            if (mappings?.Any() == true)
                return mappings.First().Key;
            else
                return "varchar";
        }

        /// <summary>
        /// 根据数据库类型名称获取C#属性类型名称
        /// </summary>
        /// <param name="dbTypeName">数据库类型名称</param>
        /// <returns>C#属性类型名称</returns>
        public override string GetPropertyTypeName(string dbTypeName)
        {
            dbTypeName = dbTypeName.ToLower();
            if (dbTypeName == "int32")
            {
                dbTypeName = "int";
            }
            else if (dbTypeName == "int16")
            {
                dbTypeName = "short";
            }
            else if (dbTypeName == "int64")
            {
                dbTypeName = "long";
            }
            else if (dbTypeName == "string")
            {
                dbTypeName = "string";
            }
            else if (dbTypeName == "boolean")
            {
                dbTypeName = "bool";
            }
            else if (dbTypeName == "bool")
            {
                dbTypeName = "bool";
            }
            else if (dbTypeName == "sbyte")
            {
                dbTypeName = "sbyte";
            }
            else if (dbTypeName == "double")
            {
                dbTypeName = "double";
            }
            else if (dbTypeName == "binary")
            {
                dbTypeName = "string";
            }
            else if (dbTypeName == "timestamp")
            {
                dbTypeName = "DateTime";
            }
            else if (dbTypeName == "bigint")
            {
                dbTypeName = "long";
            }
            else if (dbTypeName == "char")
            {
                dbTypeName = "string";
            }
            else if (dbTypeName == "smallint")
            {
                dbTypeName = "short";
            }
            else if (dbTypeName == "int unsigned")
            {
                dbTypeName = "int";
            }
            else if (dbTypeName == "bigint unsigned")
            {
                dbTypeName = "long";
            }
            else if (dbTypeName == "tinyint unsigned")
            {
                dbTypeName = "byte";
            }
            else if (TDengineDbBind.MappingTypesConst.FirstOrDefault(it => (it.Key).Equals(dbTypeName, StringComparison.CurrentCultureIgnoreCase)) is { } data)
            {
                dbTypeName = data.Value.ToString();
            }
            return dbTypeName;
        }

        /// <summary>
        /// 获取数据库类型与C#类型的映射关系
        /// </summary>
        public override List<KeyValuePair<string, CSharpDataType>> MappingTypes
        {
            get
            {
                var extService = this.Context.CurrentConnectionConfig.ConfigureExternalServices;
                if (extService?.AppendDataReaderTypeMappings.HasValue() == true)
                {
                    return extService.AppendDataReaderTypeMappings.Union(MappingTypesConst).ToList();
                }
                else
                {
                    return MappingTypesConst;
                }
            }
        }

        /// <summary>
        /// TDengine 数据库类型与C#类型的常量映射关系
        /// </summary>
        public static List<KeyValuePair<string, CSharpDataType>> MappingTypesConst = new List<KeyValuePair<string, CSharpDataType>>(){
                    new KeyValuePair<string, CSharpDataType>("BOOL",CSharpDataType.@bool),
                    new KeyValuePair<string, CSharpDataType>("TINYINT",CSharpDataType.@byte),
                     new KeyValuePair<string, CSharpDataType>("TINYINT",CSharpDataType.@int),
                    new KeyValuePair<string, CSharpDataType>("SMALLINT",CSharpDataType.@short),
                    new KeyValuePair<string, CSharpDataType>("INT",CSharpDataType.@int),
                    new KeyValuePair<string, CSharpDataType>("BIGINT",CSharpDataType.@long),
                    new KeyValuePair<string, CSharpDataType>("TINYINT UNSIGNED",CSharpDataType.@byte),
                    new KeyValuePair<string, CSharpDataType>("TINYINT UNSIGNED",CSharpDataType.@int),
                    new KeyValuePair<string, CSharpDataType>("SMALLINT UNSIGNED",CSharpDataType.@short),
                    new KeyValuePair<string, CSharpDataType>("INT UNSIGNED",CSharpDataType.@int),
                    new KeyValuePair<string, CSharpDataType>("BIGINT UNSIGNED",CSharpDataType.@long),
                    new KeyValuePair<string, CSharpDataType>("FLOAT",CSharpDataType.Single),
                    new KeyValuePair<string, CSharpDataType>("DOUBLE",CSharpDataType.@double),
                    new KeyValuePair<string, CSharpDataType>("float8",CSharpDataType.@double),
                    new KeyValuePair<string, CSharpDataType>("BINARY",CSharpDataType.@string),
                    new KeyValuePair<string, CSharpDataType>("TIMESTAMP",CSharpDataType.DateTime),
                    new KeyValuePair<string, CSharpDataType>("NCHAR",CSharpDataType.@string),
                    new KeyValuePair<string, CSharpDataType>("JSON",CSharpDataType.@string)
                };

        /// <summary>
        /// 获取需要特殊处理的字符串类型列表
        /// </summary>
        public override List<string> StringThrow
        {
            get
            {
                return new List<string>() { "int32", "datetime", "decimal", "double", "byte" };
            }
        }
    }
}