using System.Data;
using System.Reflection;

namespace ThingsGateway.SqlSugar
{
    public class TDengineDbMaintenance : DbMaintenanceProvider
    {
        public EntityInfo EntityInfo { get; set; }

        #region DML

        /// <summary>
        /// 获取视图信息列表的 SQL 语句。
        /// </summary>
        protected override string GetViewInfoListSql => throw new NotImplementedException();

        /// <summary>
        /// 获取所有数据库的 SQL 语句。
        /// </summary>
        protected override string GetDataBaseSql
        {
            get
            {
                return "show databases";
            }
        }

        /// <summary>
        /// 根据表名获取列信息的 SQL 语句。
        /// </summary>
        protected override string GetColumnInfosByTableNameSql
        {
            get
            {
                throw new NotSupportedException("TDengineCode暂时不支持DbFirst等方法,还在开发");
            }
        }

        /// <summary>
        /// 获取所有表信息的 SQL 语句。
        /// </summary>
        protected override string GetTableInfoListSql
        {
            get
            {
                return string.Empty;
            }
        }

        #endregion

        #region DDL

        /// <summary>
        /// 创建数据库的 SQL 语句。
        /// </summary>
        protected override string CreateDataBaseSql
        {
            get
            {
                return "CREATE DATABASE IF NOT EXISTS {0} WAL_RETENTION_PERIOD 3600";
            }
        }

        /// <summary>
        /// 添加主键的 SQL 语句。
        /// </summary>
        protected override string AddPrimaryKeySql
        {
            get
            {
                return "ALTER TABLE {0} ADD PRIMARY KEY({2}) /*{1}*/";
            }
        }

        /// <summary>
        /// 向表中添加列的 SQL 语句。
        /// </summary>
        protected override string AddColumnToTableSql
        {
            get
            {
                return "ALTER TABLE {0} ADD COLUMN {1} {2}{3} {4} {5} {6}";
            }
        }

        /// <summary>
        /// 修改表中列的 SQL 语句。
        /// </summary>
        protected override string AlterColumnToTableSql
        {
            get
            {
                return "alter table {0} MODIFY COLUMN {1} {2}{3} {4} {5} {6}";
            }
        }

        /// <summary>
        /// 备份数据库的 SQL 语句。
        /// </summary>
        protected override string BackupDataBaseSql
        {
            get
            {
                return "mysqldump.exe  {0} -uroot -p > {1}  ";
            }
        }

        /// <summary>
        /// 创建表的 SQL 语句。
        /// </summary>
        protected override string CreateTableSql
        {
            get
            {
                return "CREATE STABLE IF NOT EXISTS  {0}(\r\n{1} ) TAGS(" + SqlBuilder.GetTranslationColumnName("TagsTypeId") + " VARCHAR(100))";
            }
        }

        /// <summary>
        /// 表列定义格式的 SQL 字符串。
        /// </summary>
        protected override string CreateTableColumn
        {
            get
            {
                return "{0} {1}{2} {3} {4} {5}";
            }
        }

        /// <summary>
        /// 清空表数据的 SQL 语句。
        /// </summary>
        protected override string TruncateTableSql
        {
            get
            {
                return "TRUNCATE TABLE {0}";
            }
        }

        /// <summary>
        /// 备份表的 SQL 语句。
        /// </summary>
        protected override string BackupTableSql
        {
            get
            {
                return "create table {0} as (select * from {1} limit {2} offset 0)";
            }
        }

        /// <summary>
        /// 删除表的 SQL 语句。
        /// </summary>
        protected override string DropTableSql
        {
            get
            {
                return "DROP TABLE {0}";
            }
        }

        /// <summary>
        /// 从表中删除列的 SQL 语句。
        /// </summary>
        protected override string DropColumnToTableSql
        {
            get
            {
                return "ALTER TABLE {0} DROP COLUMN {1}";
            }
        }

        /// <summary>
        /// 删除表约束的 SQL 语句。
        /// </summary>
        protected override string DropConstraintSql
        {
            get
            {
                return "ALTER TABLE {0} DROP CONSTRAINT {1}";
            }
        }

        /// <summary>
        /// 重命名列的 SQL 语句。
        /// </summary>
        protected override string RenameColumnSql
        {
            get
            {
                return "ALTER TABLE {0} RENAME {1} TO {2}";
            }
        }

        /// <summary>
        /// 添加列注释的 SQL 语句。
        /// </summary>
        protected override string AddColumnRemarkSql => "comment on column {1}.{0} is '{2}'";

        /// <summary>
        /// 删除列注释的 SQL 语句。
        /// </summary>
        protected override string DeleteColumnRemarkSql => "comment on column {1}.{0} is ''";

        /// <summary>
        /// 判断列是否有注释的 SQL 语句。
        /// </summary>
        protected override string IsAnyColumnRemarkSql { get { throw new NotSupportedException(); } }

        /// <summary>
        /// 添加表注释的 SQL 语句。
        /// </summary>
        protected override string AddTableRemarkSql => "comment on table {0} is '{1}'";

        /// <summary>
        /// 删除表注释的 SQL 语句。
        /// </summary>
        protected override string DeleteTableRemarkSql => "comment on table {0} is ''";

        /// <summary>
        /// 判断表是否有注释的 SQL 语句。
        /// </summary>
        protected override string IsAnyTableRemarkSql { get { throw new NotSupportedException(); } }

        /// <summary>
        /// 重命名表的 SQL 语句。
        /// </summary>
        protected override string RenameTableSql => "alter table  {0} to {1}";

        /// <summary>
        /// 创建索引的 SQL 语句。
        /// </summary>
        protected override string CreateIndexSql
        {
            get
            {
                return "CREATE {3} INDEX Index_{0}_{2} ON {0} ({1})";
            }
        }

        /// <summary>
        /// 添加默认值的 SQL 语句。
        /// </summary>
        protected override string AddDefaultValueSql
        {
            get
            {
                return "ALTER TABLE {0} ALTER COLUMN {1} SET DEFAULT {2}";
            }
        }

        /// <summary>
        /// 判断索引是否存在的 SQL 语句。
        /// </summary>
        protected override string IsAnyIndexSql
        {
            get
            {
                return "  SELECT count(1) WHERE upper('{0}') IN ( SELECT upper(indexname) FROM pg_indexes )";
            }
        }

        /// <summary>
        /// 判断存储过程是否存在的 SQL 语句。
        /// </summary>
        protected override string IsAnyProcedureSql => throw new NotImplementedException();

        #endregion

        #region Check

        /// <summary>
        /// 检查系统表访问权限的 SQL 语句。
        /// </summary>
        protected override string CheckSystemTablePermissionsSql
        {
            get
            {
                return "SHOW DATABASES";
            }
        }

        #endregion

        #region Scattered

        /// <summary>
        /// 表字段可为空标识的 SQL 表达式。
        /// </summary>
        protected override string CreateTableNull
        {
            get
            {
                return " ";
            }
        }

        /// <summary>
        /// 表字段不可为空标识的 SQL 表达式。
        /// </summary>
        protected override string CreateTableNotNull
        {
            get
            {
                return " ";
            }
        }

        /// <summary>
        /// 表主键定义的 SQL 表达式。
        /// </summary>
        protected override string CreateTablePirmaryKey
        {
            get
            {
                return "PRIMARY KEY";
            }
        }

        /// <summary>
        /// 表自增字段定义的 SQL 表达式。
        /// </summary>
        protected override string CreateTableIdentity
        {
            get
            {
                return "serial";
            }
        }

        #endregion

        #region Methods  
        /// <summary>
        /// 获取表信息列表
        /// </summary>
        /// <param name="isCache">是否使用缓存</param>
        /// <returns>表信息列表</returns>
        public override List<DbTableInfo> GetTableInfoList(bool isCache = true)
        {
            var sb = new List<string>();

            // 第一个循环：获取超级表名称
            var dt = GetSTables();
            foreach (DataRow item in dt.Rows)
            {
                sb.Add(item["stable_name"].ObjToString().ToSqlFilter());
            }

            // 第二个循环：获取子表名称
            var dt2 = GetTables();
            foreach (DataRow item in dt2.Rows)
            {
                sb.Add(item["table_name"].ObjToString().ToSqlFilter());
            }
            var result = sb.Select(it => new DbTableInfo() { Name = it, DbObjectType = DbObjectType.Table }).ToList();
            return result;
        }

        /// <summary>
        /// 添加列
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="columnInfo">列信息</param>
        /// <returns>是否成功</returns>
        public override bool AddColumn(string tableName, DbColumnInfo columnInfo)
        {
            if (columnInfo.DbColumnName == "TagsTypeId")
            {
                return true;
            }
            tableName = this.SqlBuilder.GetTranslationTableName(tableName);
            var isAddNotNUll = columnInfo.IsNullable == false && columnInfo.DefaultValue.HasValue();
            if (isAddNotNUll)
            {
                columnInfo = this.Context.Utilities.TranslateCopy(columnInfo);
                columnInfo.IsNullable = true;
            }
            string sql = GetAddColumnSql(tableName, columnInfo);
            this.Context.Ado.ExecuteCommand(sql);
            return true;
        }

        /// <summary>
        /// 获取视图信息列表
        /// </summary>
        /// <param name="isCache">是否使用缓存</param>
        /// <returns>视图信息列表</returns>
        public override List<DbTableInfo> GetViewInfoList(bool isCache = true)
        {
            return new List<DbTableInfo>();
        }

        /// <summary>
        /// 创建数据库
        /// </summary>
        /// <param name="databaseName">数据库名</param>
        /// <param name="databaseDirectory">数据库目录</param>
        /// <returns>是否成功</returns>
        public override bool CreateDatabase(string databaseName, string databaseDirectory = null)
        {
            var db = this.Context.CopyNew();
            db.Ado.Connection.ChangeDatabase("");
            var sql = CreateDataBaseSql;
            if (this.Context.CurrentConnectionConfig.ConnectionString.Contains("config_us", StringComparison.OrdinalIgnoreCase))
            {
                sql += " PRECISION 'us'";
            }
            else if (this.Context.CurrentConnectionConfig.ConnectionString.Contains("config_ns", StringComparison.OrdinalIgnoreCase))
            {
                sql += " PRECISION 'ns'";
            }
            db.Ado.ExecuteCommand(string.Format(sql, databaseName));
            return true;
        }

        /// <summary>
        /// 获取索引列表
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns>索引列表</returns>
        public override List<string> GetIndexList(string tableName)
        {
            var sql = $"SELECT indexname, indexdef FROM pg_indexes WHERE upper(tablename) = upper('{tableName}')";
            return this.Context.Ado.SqlQuery<string>(sql);
        }

        /// <summary>
        /// 获取存储过程列表
        /// </summary>
        /// <param name="dbName">数据库名</param>
        /// <returns>存储过程列表</returns>
        public override List<string> GetProcList(string dbName)
        {
            var sql = $"SELECT proname FROM pg_proc p JOIN pg_namespace n ON p.pronamespace = n.oid WHERE n.nspname = '{dbName}'";
            return this.Context.Ado.SqlQuery<string>(sql);
        }

        /// <summary>
        /// 添加默认值
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="columnName">列名</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>是否成功</returns>
        public override bool AddDefaultValue(string tableName, string columnName, string defaultValue)
        {
            return base.AddDefaultValue(this.SqlBuilder.GetTranslationTableName(tableName), this.SqlBuilder.GetTranslationTableName(columnName), defaultValue);
        }

        /// <summary>
        /// 添加列备注
        /// </summary>
        /// <param name="columnName">列名</param>
        /// <param name="tableName">表名</param>
        /// <param name="description">备注内容</param>
        /// <returns>是否成功</returns>
        public override bool AddColumnRemark(string columnName, string tableName, string description)
        {
            tableName = this.SqlBuilder.GetTranslationTableName(tableName);
            string sql = string.Format(this.AddColumnRemarkSql, this.SqlBuilder.GetTranslationColumnName(columnName.ToLower(isAutoToLowerCodeFirst)), tableName, description);
            this.Context.Ado.ExecuteCommand(sql);
            return true;
        }

        /// <summary>
        /// 添加表备注
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="description">备注内容</param>
        /// <returns>是否成功</returns>
        public override bool AddTableRemark(string tableName, string description)
        {
            tableName = this.SqlBuilder.GetTranslationTableName(tableName);
            return base.AddTableRemark(tableName, description);
        }

        /// <summary>
        /// 更新列
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="columnInfo">列信息</param>
        /// <returns>是否成功</returns>
        public override bool UpdateColumn(string tableName, DbColumnInfo columnInfo)
        {
            tableName = this.SqlBuilder.GetTranslationTableName(tableName);
            var columnName = this.SqlBuilder.GetTranslationColumnName(columnInfo.DbColumnName);
            string sql = GetUpdateColumnSql(tableName, columnInfo);
            this.Context.Ado.ExecuteCommand(sql);
            var isnull = columnInfo.IsNullable ? " DROP NOT NULL " : " SET NOT NULL ";
            this.Context.Ado.ExecuteCommand(string.Format("alter table {0} alter {1} {2}", tableName, columnName, isnull));
            return true;
        }

        /// <summary>
        /// 获取更新列SQL
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="columnInfo">列信息</param>
        /// <returns>SQL语句</returns>
        protected override string GetUpdateColumnSql(string tableName, DbColumnInfo columnInfo)
        {
            string columnName = this.SqlBuilder.GetTranslationColumnName(columnInfo.DbColumnName);
            tableName = this.SqlBuilder.GetTranslationTableName(tableName);
            string dataSize = GetSize(columnInfo);
            string dataType = columnInfo.DataType;
            string nullType = "";
            string primaryKey = null;
            string identity = null;
            string result = string.Format(this.AlterColumnToTableSql, tableName, columnName, dataType, dataSize, nullType, primaryKey, identity);
            return result;
        }

        /// <summary>
        /// 添加备注
        /// </summary>
        /// <param name="entity">实体信息</param>
        /// <returns>是否成功</returns>
        public override bool AddRemark(EntityInfo entity)
        {
            return true;
        }

        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="columns">列集合</param>
        /// <param name="isCreatePrimaryKey">是否创建主键</param>
        /// <returns>是否成功</returns>
        public override bool CreateTable(string tableName, List<DbColumnInfo> columns, bool isCreatePrimaryKey = true)
        {
            if (columns.HasValue())
            {
                foreach (var item in columns)
                {
                    if (item.DbColumnName.Equals("GUID", StringComparison.CurrentCultureIgnoreCase) && item.Length == 0)
                    {
                        item.Length = 10;
                    }
                }
            }
            string sql = GetCreateTableSql(tableName, columns);
            string primaryKeyInfo = null;
            if (columns.Any(it => it.IsPrimarykey) && isCreatePrimaryKey)
            {
                primaryKeyInfo = string.Format(", Primary key({0})", string.Join(",", columns.Where(it => it.IsPrimarykey).Select(it => this.SqlBuilder.GetTranslationColumnName(it.DbColumnName.ToLower(isAutoToLowerCodeFirst)))));
            }
            sql = sql.Replace("$PrimaryKey", primaryKeyInfo);
            this.Context.Ado.ExecuteCommand(sql);
            return true;
        }

        /// <summary>
        /// 获取创建表SQL
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="columns">列集合</param>
        /// <returns>SQL语句</returns>
        protected override string GetCreateTableSql(string tableName, List<DbColumnInfo> columns)
        {
            List<string> columnArray = new List<string>();
            if (columns.IsNullOrEmpty()) { throw new SqlSugarException("No columns found "); }
            foreach (var item in columns)
            {
                string columnName = item.DbColumnName;
                string dataType = item.DataType;
                if (dataType == "varchar" && item.Length == 0)
                {
                    item.Length = 1;
                }
                string dataSize = item?.Length > 0 ? string.Format("({0})", item.Length) : null;
                if (item.Length == 0 && dataType?.IsInCase("nchar", "varchar") == true)
                {
                    dataType = "VARCHAR(200)";
                }
                if (dataType?.IsInCase("float", "double") == true)
                {
                    dataSize = null;
                }
                string primaryKey = null;
                string addItem = string.Format(this.CreateTableColumn, this.SqlBuilder.GetTranslationColumnName(columnName.ToLower(isAutoToLowerCodeFirst)), dataType, dataSize, null, primaryKey, "");
                columnArray.Add(addItem);
            }
            string tableString = string.Format(this.CreateTableSql, this.SqlBuilder.GetTranslationTableName("STable_" + tableName.ToLower(isAutoToLowerCodeFirst)), string.Join(",\r\n", columnArray));
            var childTableName = this.SqlBuilder.GetTranslationTableName(tableName.ToLower(isAutoToLowerCodeFirst));
            var stableName = this.SqlBuilder.GetTranslationTableName("STable_" + tableName.ToLower(isAutoToLowerCodeFirst));
            var isAttr = tableName.Contains("{stable}");
            var isTag1 = false;
            if (isAttr)
            {
                var attr = this.Context.Utilities.DeserializeObject<STableAttribute>(tableName.Split("{stable}").Last());
                stableName = this.SqlBuilder.GetTranslationTableName(attr.STableName.ToLower(isAutoToLowerCodeFirst));
                tableString = string.Format(this.CreateTableSql, stableName, string.Join(",\r\n", columnArray));
                tableName = childTableName = this.SqlBuilder.GetTranslationTableName(tableName.Split("{stable}").First().ToLower(isAutoToLowerCodeFirst));
                if (attr.Tags == null && attr.Tag1 != null)
                {
                    isTag1 = true;
                    STable.Tags = new List<ColumnTagInfo>() {
                      new ColumnTagInfo(){ Name=attr.Tag1 },
                      new ColumnTagInfo(){ Name=attr.Tag2 },
                      new ColumnTagInfo(){ Name=attr.Tag3 },
                      new ColumnTagInfo(){ Name=attr.Tag4 }
                    }.Where(it => it.Name.HasValue()).ToList();
                }
                else
                {
                    STable.Tags = this.Context.Utilities.DeserializeObject<List<ColumnTagInfo>>(attr.Tags);
                }
            }
            if (STable.Tags?.Count > 0)
            {
                var colums = STable.Tags.Select(it => this.SqlBuilder.GetTranslationTableName(it.Name) + "  VARCHAR(100) ");
                tableString = tableString.Replace(SqlBuilder.GetTranslationColumnName("TagsTypeId"), string.Join(",", colums));
                tableString = tableString.Replace(" VARCHAR(100)  VARCHAR(100)", " VARCHAR(100)");
                if (this.EntityInfo != null)
                {
                    foreach (var item in STable.Tags)
                    {
                        var tagColumn = this.EntityInfo.Columns.FirstOrDefault(it => it.DbColumnName == item.Name || it.PropertyName == item.Name);
                        if (tagColumn != null && tagColumn.UnderType != UtilConstants.StringType)
                        {
                            var tagType = new TDengineDbBind() { Context = this.Context }.GetDbTypeName(tagColumn.UnderType.Name);
                            tableString = tableString.Replace($"{SqlBuilder.GetTranslationColumnName(tagColumn.DbColumnName)}  VARCHAR(100)", $"{SqlBuilder.GetTranslationColumnName(tagColumn.DbColumnName)} {tagType} ");
                        }
                        else if (tagColumn != null && tagColumn.UnderType == UtilConstants.StringType && tagColumn.Length < 100 && tagColumn?.Length > 0)
                        {
                            tableString = tableString.Replace($"{SqlBuilder.GetTranslationColumnName(tagColumn.DbColumnName)}  VARCHAR(100)", $"{SqlBuilder.GetTranslationColumnName(tagColumn.DbColumnName)}  VARCHAR({tagColumn.Length}) ");
                        }
                    }
                }
            }
            this.Context.Ado.ExecuteCommand(tableString);
            var createChildSql = $"CREATE TABLE IF NOT EXISTS     {childTableName} USING {stableName} TAGS('default')";
            if (STable.Tags?.Count > 0)
            {
                var colums = STable.Tags.Select(it => it.Value.ToSqlValue());
                createChildSql = createChildSql.Replace("TAGS('default')", $"TAGS({string.Join(",", colums)})");
            }
            if (isTag1)
            {
                //No create child table
            }
            else
            {
                this.Context.Ado.ExecuteCommand(createChildSql);
            }
            return tableString;
        }

        /// <summary>
        /// 检查约束是否存在
        /// </summary>
        /// <param name="constraintName">约束名</param>
        /// <returns>是否存在</returns>
        public override bool IsAnyConstraint(string constraintName)
        {
            throw new NotSupportedException("PgSql IsAnyConstraint NotSupportedException");
        }

        /// <summary>
        /// 备份数据库
        /// </summary>
        /// <param name="databaseName">数据库名</param>
        /// <param name="fullFileName">完整文件名</param>
        /// <returns>是否成功</returns>
        public override bool BackupDataBase(string databaseName, string fullFileName)
        {
            Check.ThrowNotSupportedException("PgSql BackupDataBase NotSupported");
            return false;
        }

        /// <summary>
        /// 添加默认值
        /// </summary>
        /// <param name="entityInfo">实体信息</param>
        public override void AddDefaultValue(EntityInfo entityInfo)
        {
            var talbeName = entityInfo.DbTableName;
            var attr = GetCommonSTableAttribute(entityInfo.Type.GetCustomAttribute<STableAttribute>());
            if (attr?.Tag1 != null)
            {
                talbeName = attr.STableName;
            }
            var dbColumns = this.GetColumnInfosByTableName(talbeName, false);
            var db = this.Context;
            var columns = entityInfo.Columns.Where(it => it.IsIgnore == false);
            foreach (var item in columns)
            {
                if (item.DefaultValue.HasValue())
                {
                    if (!IsAnyDefaultValue(entityInfo.DbTableName, item.DbColumnName, dbColumns))
                    {
                        this.AddDefaultValue(entityInfo.DbTableName, item.DbColumnName, item.DefaultValue);
                    }
                }
            }
        }

        /// <summary>
        /// 获取通用STable特性
        /// </summary>
        /// <param name="sTableAttribute">STable特性</param>
        /// <returns>STable特性</returns>
        private STableAttribute GetCommonSTableAttribute(STableAttribute sTableAttribute)
        {
            return TaosUtilMethods.GetCommonSTableAttribute(this.Context, sTableAttribute);
        }

        /// <summary>
        /// 根据表名获取列信息
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="isCache">是否使用缓存</param>
        /// <returns>列信息列表</returns>
        public override List<DbColumnInfo> GetColumnInfosByTableName(string tableName, bool isCache = true)
        {

            if (string.IsNullOrEmpty(tableName)) return new List<DbColumnInfo>();
            string cacheKey = "TDengine.GetColumnInfosByTableName." + this.SqlBuilder.GetNoTranslationColumnName(tableName).ToLower() + this.Context.CurrentConnectionConfig.ConfigId;
            cacheKey = GetCacheKey(cacheKey);

            if (isCache)
            {

                return this.Context.Utilities.GetReflectionInoCacheInstance().GetOrCreate(cacheKey, () =>
                     {
                         return GetColInfo(tableName);
                     });
            }
            else
            {
                return GetColInfo(tableName);

            }

        }

        private List<DbColumnInfo> GetColInfo(string tableName)
        {
            List<DbColumnInfo> result = new List<DbColumnInfo>();

            var sql = $"select * from {this.SqlBuilder.GetTranslationColumnName(tableName)} where 1=2 ";
            DataTable dt = null;
            try
            {
                dt = this.Context.Ado.GetDataTable(sql);
            }
            catch (Exception)
            {
                sql = $"select * from `{tableName}` where 1=2 ";
                dt = this.Context.Ado.GetDataTable(sql);
            }
            foreach (DataColumn item in dt.Columns)
            {
                var addItem = new DbColumnInfo()
                {
                    DbColumnName = item.ColumnName,
                    DataType = item.DataType.Name
                };
                result.Add(addItem);
            }
            if (result.Count(it => it.DataType == "DateTime") == 1)
            {
                result.First(it => it.DataType == "DateTime").IsPrimarykey = true;
            }
            return result;
        }
        #endregion

        #region Helper
        /// <summary>
        /// 是否自动转换为小写(代码优先)
        /// </summary>
        private bool isAutoToLowerCodeFirst
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
        /// 获取Schema
        /// </summary>
        /// <returns>Schema名称</returns>
        private string GetSchema()
        {
            var schema = "public";
            if (System.Text.RegularExpressions.Regex.IsMatch(this.Context.CurrentConnectionConfig.ConnectionString.ToLower(), "searchpath="))
            {
                var regValue = System.Text.RegularExpressions.Regex.Match(this.Context.CurrentConnectionConfig.ConnectionString.ToLower(), @"searchpath\=(\w+)").Groups[1].Value;
                if (regValue.HasValue())
                {
                    schema = regValue;
                }
            }
            else if (System.Text.RegularExpressions.Regex.IsMatch(this.Context.CurrentConnectionConfig.ConnectionString.ToLower(), "search path="))
            {
                var regValue = System.Text.RegularExpressions.Regex.Match(this.Context.CurrentConnectionConfig.ConnectionString.ToLower(), @"search path\=(\w+)").Groups[1].Value;
                if (regValue.HasValue())
                {
                    schema = regValue;
                }
            }

            return schema;
        }

        /// <summary>
        /// 获取表集合
        /// </summary>
        /// <returns>数据表</returns>
        private DataTable GetTables()
        {
            return this.Context.Ado.GetDataTable("SHOW TABLES");
        }

        /// <summary>
        /// 获取超级表集合
        /// </summary>
        /// <returns>数据表</returns>
        private DataTable GetSTables()
        {
            return this.Context.Ado.GetDataTable("SHOW STABLES");
        }
        #endregion
    }
}
