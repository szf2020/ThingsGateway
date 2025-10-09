namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// TDengine SQL 构建器
    /// </summary>
    public class TDengineBuilder : SqlBuilderProvider
    {
        /// <summary>
        /// 获取 SQL 左引号
        /// </summary>
        public override string SqlTranslationLeft
        {
            get
            {
                return "`";
            }
        }

        /// <summary>
        /// 获取 SQL 右引号
        /// </summary>
        public override string SqlTranslationRight
        {
            get
            {
                return "`";
            }
        }

        /// <summary>
        /// 获取当前日期 SQL 表达式
        /// </summary>
        public override string SqlDateNow
        {
            get
            {
                return "current_date";
            }
        }

        /// <summary>
        /// 获取当前日期时间 SQL 表达式
        /// </summary>
        public override string FullSqlDateNow
        {
            get
            {
                return " now() ";
            }
        }

        /// <summary>
        /// 是否自动转换为小写
        /// </summary>
        public bool isAutoToLower
        {
            get
            {
                if (this.Context.CurrentConnectionConfig.MoreSettings == null) return true;
                else if (
                    this.Context.CurrentConnectionConfig.MoreSettings.PgSqlIsAutoToLower == false &&
                    this.Context.CurrentConnectionConfig.MoreSettings?.PgSqlIsAutoToLowerCodeFirst == false)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// 获取转换后的列名
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <returns>转换后的列名</returns>
        public override string GetTranslationColumnName(string propertyName)
        {
            if (propertyName.Contains('.') && !propertyName.Contains(SqlTranslationLeft))
            {
                return string.Join(".", propertyName.Split('.').Select(it => $"{SqlTranslationLeft}{it.ToLower(isAutoToLower)}{SqlTranslationRight}"));
            }

            if (propertyName.Contains(SqlTranslationLeft)) return propertyName;
            else
                return SqlTranslationLeft + propertyName.ToLower(isAutoToLower) + SqlTranslationRight;
        }

        /// <summary>
        /// 获取转换后的列名（带实体名）
        /// </summary>
        /// <param name="entityName">实体名</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>转换后的列名</returns>
        public override string GetTranslationColumnName(string entityName, string propertyName)
        {
            if (entityName == null) { throw new SqlSugarException(string.Format(null, ErrorMessage.ObjNotExistCompositeFormat, "Table Name")); }
            if (propertyName == null) { throw new SqlSugarException(string.Format(null, ErrorMessage.ObjNotExistCompositeFormat, "Column Name")); }
            var context = this.Context;
            var mappingInfo = context
                 .MappingColumns
                 .FirstOrDefault(it =>
                 it.EntityName.Equals(entityName, StringComparison.CurrentCultureIgnoreCase) &&
                 it.PropertyName.Equals(propertyName, StringComparison.CurrentCultureIgnoreCase));
            return (mappingInfo == null ? SqlTranslationLeft + propertyName.ToLower(isAutoToLower) + SqlTranslationRight : SqlTranslationLeft + mappingInfo.DbColumnName.ToLower(isAutoToLower) + SqlTranslationRight);
        }

        /// <summary>
        /// 获取转换后的表名
        /// </summary>
        /// <param name="name">表名</param>
        /// <returns>转换后的表名</returns>
        public override string GetTranslationTableName(string name)
        {
            if (name == null) { throw new SqlSugarException(string.Format(null, ErrorMessage.ObjNotExistCompositeFormat, "Table Name")); }
            var context = this.Context;

            var mappingInfo = context
                .MappingTables
                .FirstOrDefault(it => it.EntityName.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            if (mappingInfo == null && name.Contains('.') && name.Contains('`'))
            {
                return name;
            }
            name = (mappingInfo == null ? name : mappingInfo.DbTableName);
            if (name.Contains('.') && !name.Contains('(') && !name.Contains("\".\""))
            {
                return string.Join(".", name.ToLower(isAutoToLower).Split('.').Select(it => SqlTranslationLeft + it + SqlTranslationRight));
            }
            else if (name.Contains('('))
            {
                return name;
            }
            else if (name.Contains(SqlTranslationLeft) && name.Contains(SqlTranslationRight))
            {
                return name;
            }
            else
            {
                return SqlTranslationLeft + name.ToLower(isAutoToLower).TrimEnd('"').TrimStart('"') + SqlTranslationRight;
            }
        }

        /// <summary>
        /// 获取 UNION 格式化 SQL
        /// </summary>
        /// <param name="sql">SQL 语句</param>
        /// <returns>格式化后的 SQL</returns>
        public override string GetUnionFomatSql(string sql)
        {
            return " ( " + sql + " )  ";
        }

        /// <summary>
        /// 获取可为空的类型
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="columnName">列名</param>
        /// <returns>可为空的类型</returns>
        public override Type GetNullType(string tableName, string columnName)
        {
            if (tableName != null)
                tableName = tableName.Trim();
            var columnInfo = this.Context.DbMaintenance.GetColumnInfosByTableName(tableName).FirstOrDefault(z => z.DbColumnName?.EqualCase(columnName) == true);
            if (columnInfo != null)
            {
                var cTypeName = this.Context.Ado.DbBind.GetCsharpTypeNameByDbTypeName(columnInfo.DataType);
                var value = SqlSugar.UtilMethods.GetTypeByTypeName(cTypeName);
                if (value != null)
                {
                    var key = "GetNullType_" + tableName + columnName;
                    return ReflectionInoCacheService.Instance.GetOrCreate(key, () => value);
                }
            }
            return null;
        }
    }
}