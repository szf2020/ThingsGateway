using System.Reflection;
using System.Text.RegularExpressions;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 数据库维护提供者基类
    /// </summary>
    public abstract partial class DbMaintenanceProvider : IDbMaintenance
    {
        #region DML
        /// <summary>
        /// 获取存储过程列表
        /// </summary>
        public List<string> GetProcList()
        {
            return GetProcList(this.Context.Ado.Connection.Database);
        }
        /// <summary>
        /// 获取指定数据库的存储过程列表
        /// </summary>
        public virtual List<string> GetProcList(string dbName)
        {
            return new List<string>();
        }
        /// <summary>
        /// 获取数据库列表
        /// </summary>
        public virtual List<string> GetDataBaseList(SqlSugarClient db)
        {
            return db.Ado.SqlQuery<string>(this.GetDataBaseSql);
        }
        /// <summary>
        /// 获取当前连接的数据库列表
        /// </summary>
        public virtual List<string> GetDataBaseList()
        {
            return this.Context.Ado.SqlQuery<string>(this.GetDataBaseSql);
        }
        /// <summary>
        /// 获取视图信息列表
        /// </summary>
        public virtual List<DbTableInfo> GetViewInfoList(bool isCache = true)
        {
            string cacheKey = "DbMaintenanceProvider.GetViewInfoList" + this.Context.CurrentConnectionConfig.ConfigId;
            cacheKey = GetCacheKey(cacheKey);
            var result = new List<DbTableInfo>();
            if (isCache)
                result = GetListOrCache<DbTableInfo>(cacheKey, this.GetViewInfoListSql);
            else
                result = this.Context.Ado.SqlQuery<DbTableInfo>(this.GetViewInfoListSql);
            foreach (var item in result)
            {
                item.DbObjectType = DbObjectType.View;
            }
            return result;
        }

        /// <summary>
        /// 获取表信息列表(可自定义SQL转换)
        /// </summary>
        public List<DbTableInfo> GetTableInfoList(Func<DbType, string, string> getChangeSqlFunc)
        {
            var db = this.Context.CopyNew();
            db.CurrentConnectionConfig.IsAutoCloseConnection = true;
            db.Aop.OnExecutingChangeSql = (sql, pars) =>
            {
                sql = getChangeSqlFunc(this.Context.CurrentConnectionConfig.DbType, sql);
                return new KeyValuePair<string, IReadOnlyList<SugarParameter>>(sql, pars);
            };
            var result = db.DbMaintenance.GetTableInfoList(false);
            return result;
        }

        /// <summary>
        /// 获取表信息列表
        /// </summary>
        public virtual List<DbTableInfo> GetTableInfoList(bool isCache = true)
        {
            string cacheKey = "DbMaintenanceProvider.GetTableInfoList" + this.Context.CurrentConnectionConfig.ConfigId;
            cacheKey = GetCacheKey(cacheKey);
            var result = new List<DbTableInfo>();
            if (isCache)
                result = GetListOrCache<DbTableInfo>(cacheKey, this.GetTableInfoListSql);
            else
                result = this.Context.Ado.SqlQuery<DbTableInfo>(this.GetTableInfoListSql);
            foreach (var item in result)
            {
                item.DbObjectType = DbObjectType.Table;
            }
            return result;
        }
        /// <summary>
        /// 根据表名获取列信息(可自定义SQL转换)
        /// </summary>
        public List<DbColumnInfo> GetColumnInfosByTableName(string tableName, Func<DbType, string, string> getChangeSqlFunc)
        {
            var db = this.Context.CopyNew();
            db.CurrentConnectionConfig.IsAutoCloseConnection = true;
            db.Aop.OnExecutingChangeSql = (sql, pars) =>
            {
                sql = getChangeSqlFunc(this.Context.CurrentConnectionConfig.DbType, sql);
                return new KeyValuePair<string, IReadOnlyList<SugarParameter>>(sql, pars);
            };
            var result = db.DbMaintenance.GetColumnInfosByTableName(tableName, false);
            return result;
        }
        /// <summary>
        /// 根据表名获取列信息
        /// </summary>
        public virtual List<DbColumnInfo> GetColumnInfosByTableName(string tableName, bool isCache = true)
        {
            if (string.IsNullOrEmpty(tableName)) return new List<DbColumnInfo>();
            string cacheKey = "DbMaintenanceProvider.GetColumnInfosByTableName." + this.SqlBuilder.GetNoTranslationColumnName(tableName).ToLower() + this.Context.CurrentConnectionConfig.ConfigId;
            cacheKey = GetCacheKey(cacheKey);
            var sql = string.Format(this.GetColumnInfosByTableNameSql, tableName);
            if (isCache)
                return GetListOrCache<DbColumnInfo>(cacheKey, sql).GroupBy(it => it.DbColumnName).Select(it => it.First()).ToList();
            else
                return this.Context.Ado.SqlQuery<DbColumnInfo>(sql).GroupBy(it => it.DbColumnName).Select(it => it.First()).ToList();

        }
        /// <summary>
        /// 获取自增列列表
        /// </summary>
        public virtual List<string> GetIsIdentities(string tableName)
        {
            string cacheKey = "DbMaintenanceProvider.GetIsIdentities" + this.SqlBuilder.GetNoTranslationColumnName(tableName).ToLower() + this.Context.CurrentConnectionConfig.ConfigId;
            cacheKey = GetCacheKey(cacheKey);
            return this.Context.Utilities.GetReflectionInoCacheInstance().GetOrCreate(cacheKey, () =>
            {
                var result = GetColumnInfosByTableName(tableName).Where(it => it.IsIdentity);
                return result.Select(it => it.DbColumnName).ToList();
            });
        }
        /// <summary>
        /// 获取主键列列表
        /// </summary>
        public virtual List<string> GetPrimaries(string tableName)
        {
            string cacheKey = "DbMaintenanceProvider.GetPrimaries" + this.SqlBuilder.GetNoTranslationColumnName(tableName).ToLower() + this.Context.CurrentConnectionConfig.ConfigId;
            cacheKey = GetCacheKey(cacheKey);
            return this.Context.Utilities.GetReflectionInoCacheInstance().GetOrCreate(cacheKey, () =>
            {
                var result = GetColumnInfosByTableName(tableName).Where(it => it.IsPrimarykey);
                return result.Select(it => it.DbColumnName).ToList();
            });
        }
        /// <summary>
        /// 获取索引列表
        /// </summary>
        public virtual List<string> GetIndexList(string tableName)
        {
            return new List<string>();
        }
        /// <summary>
        /// 获取函数列表
        /// </summary>
        public virtual List<string> GetFuncList()
        {
            return new List<string>();
        }
        /// <summary>
        /// 获取触发器名称列表
        /// </summary>
        public virtual List<string> GetTriggerNames(string tableName)
        {
            return new List<string>();
        }
        /// <summary>
        /// 获取数据库类型列表
        /// </summary>
        public virtual List<string> GetDbTypes()
        {
            return new List<string>();
        }
        #endregion

        #region Check
        /// <summary>
        /// 检查表是否存在
        /// </summary>
        public virtual bool IsAnyTable<T>()
        {
            if (typeof(T).GetCustomAttribute<SplitTableAttribute>() != null)
            {
                var tables = this.Context.SplitHelper(typeof(T)).GetTables();
                var isAny = false;
                foreach (var item in tables)
                {
                    if (this.Context.DbMaintenance.IsAnyTable(item.TableName, false))
                    {
                        isAny = true;
                        break;
                    }
                }
                return isAny;
            }
            else
            {
                this.Context.InitMappingInfo<T>();
                return this.IsAnyTable(this.Context.EntityMaintenance.GetEntityInfo<T>().DbTableName, false);
            }
        }
        /// <summary>
        /// 检查表是否存在
        /// </summary>
        public virtual bool IsAnyTable(string tableName, bool isCache = true)
        {
            Check.Exception(string.IsNullOrEmpty(tableName), "IsAnyTable tableName is not null");
            tableName = this.SqlBuilder.GetNoTranslationColumnName(tableName);
            var tables = GetTableInfoList(isCache);
            if (tables == null) return false;
            else return tables.Any(it => it.Name.Equals(tableName, StringComparison.CurrentCultureIgnoreCase));
        }
        /// <summary>
        /// 检查列是否存在
        /// </summary>
        public virtual bool IsAnyColumn(string tableName, string columnName, bool isCache = true)
        {
            columnName = this.SqlBuilder.GetNoTranslationColumnName(columnName);
            tableName = this.SqlBuilder.GetNoTranslationColumnName(tableName);
            var isAny = IsAnyTable(tableName, isCache);
            Check.Exception(!isAny, string.Format("Table {0} does not exist", tableName));
            var columns = GetColumnInfosByTableName(tableName, isCache);
            if (columns.IsNullOrEmpty()) return false;
            return columns.Any(it => it.DbColumnName.Equals(columnName, StringComparison.CurrentCultureIgnoreCase));
        }
        /// <summary>
        /// 检查是否是主键
        /// </summary>
        public virtual bool IsPrimaryKey(string tableName, string columnName)
        {
            columnName = this.SqlBuilder.GetNoTranslationColumnName(columnName);
            var isAny = IsAnyTable(tableName);
            Check.Exception(!isAny, string.Format("Table {0} does not exist", tableName));
            var columns = GetColumnInfosByTableName(tableName);
            if (columns.IsNullOrEmpty()) return false;
            var result = columns.Any(it => it.IsPrimarykey && it.DbColumnName.Equals(columnName, StringComparison.CurrentCultureIgnoreCase));
            return result;
        }
        /// <summary>
        /// 检查是否是主键(带缓存)
        /// </summary>
        public virtual bool IsPrimaryKey(string tableName, string columnName, bool isCache = true)
        {
            columnName = this.SqlBuilder.GetNoTranslationColumnName(columnName);
            var isAny = IsAnyTable(tableName, isCache);
            Check.Exception(!isAny, string.Format("Table {0} does not exist", tableName));
            var columns = GetColumnInfosByTableName(tableName, isCache);
            if (columns.IsNullOrEmpty()) return false;
            var result = columns.Any(it => it.IsPrimarykey && it.DbColumnName.Equals(columnName, StringComparison.CurrentCultureIgnoreCase));
            return result;
        }
        /// <summary>
        /// 检查是否是自增列
        /// </summary>
        public virtual bool IsIdentity(string tableName, string columnName)
        {
            columnName = this.SqlBuilder.GetNoTranslationColumnName(columnName);
            var isAny = IsAnyTable(tableName);
            Check.Exception(!isAny, string.Format("Table {0} does not exist", tableName));
            var columns = GetColumnInfosByTableName(tableName);
            if (columns.IsNullOrEmpty()) return false;
            return columns.Any(it => it.IsIdentity && it.DbColumnName.Equals(columnName, StringComparison.CurrentCultureIgnoreCase));
        }
        /// <summary>
        /// 检查约束是否存在
        /// </summary>
        public virtual bool IsAnyConstraint(string constraintName)
        {
            return this.Context.Ado.GetInt("select  object_id('" + constraintName + "')") > 0;
        }
        /// <summary>
        /// 检查是否有系统表权限
        /// </summary>
        public virtual bool IsAnySystemTablePermissions()
        {
            this.Context.Ado.CheckConnection();
            string sql = this.CheckSystemTablePermissionsSql;
            try
            {
                var oldIsEnableLog = this.Context.Ado.IsEnableLogEvent;
                this.Context.Ado.IsEnableLogEvent = false;
                this.Context.Ado.ExecuteCommand(sql);
                this.Context.Ado.IsEnableLogEvent = oldIsEnableLog;
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region DDL
        /// <summary>
        /// 设置自增初始值
        /// </summary>
        public virtual bool SetAutoIncrementInitialValue(string tableName, int initialValue)
        {
            Console.WriteLine("no support");
            return true;
        }
        /// <summary>
        /// 设置自增初始值
        /// </summary>
        public virtual bool SetAutoIncrementInitialValue(Type entityType, int initialValue)
        {
            Console.WriteLine("no support");
            return true;
        }
        /// <summary>
        /// 删除索引
        /// </summary>
        public virtual bool DropIndex(string indexName)
        {
            indexName = this.SqlBuilder.GetNoTranslationColumnName(indexName);
            this.Context.Ado.ExecuteCommand($" DROP INDEX  {indexName} ");
            return true;
        }
        /// <summary>
        /// 删除索引(指定表名)
        /// </summary>
        public virtual bool DropIndex(string indexName, string tableName)
        {
            indexName = this.SqlBuilder.GetNoTranslationColumnName(indexName);
            tableName = this.SqlBuilder.GetNoTranslationColumnName(tableName);
            this.Context.Ado.ExecuteCommand($" DROP INDEX  {indexName} ");
            return true;
        }
        /// <summary>
        /// 删除视图
        /// </summary>
        public virtual bool DropView(string viewName)
        {
            viewName = this.SqlBuilder.GetNoTranslationColumnName(viewName);
            this.Context.Ado.ExecuteCommand($" DROP VIEW {viewName} ");
            return true;
        }
        /// <summary>
        /// 删除函数
        /// </summary>
        public virtual bool DropFunction(string funcName)
        {
            funcName = this.SqlBuilder.GetNoTranslationColumnName(funcName);
            this.Context.Ado.ExecuteCommand($" DROP FUNCTION  {funcName} ");
            return true;
        }
        /// <summary>
        /// 删除存储过程
        /// </summary>
        public virtual bool DropProc(string procName)
        {
            procName = this.SqlBuilder.GetNoTranslationColumnName(procName);
            this.Context.Ado.ExecuteCommand($" DROP PROCEDURE  {procName} ");
            return true;
        }
        /// <summary>
        /// 创建数据库(当前连接字符串)
        /// </summary>
        public virtual bool CreateDatabase(string databaseDirectory = null)
        {
            var seChar = Path.DirectorySeparatorChar.ToString();
            if (databaseDirectory.HasValue())
            {
                databaseDirectory = databaseDirectory.TrimEnd('\\').TrimEnd('/');
            }
            var databaseName = this.Context.Ado.Connection.Database;
            return CreateDatabase(databaseName, databaseDirectory);
        }
        /// <summary>
        /// 创建数据库(指定数据库名)
        /// </summary>
        public virtual bool CreateDatabase(string databaseName, string databaseDirectory = null)
        {
            this.Context.Ado.ExecuteCommand(string.Format(CreateDataBaseSql, databaseName, databaseDirectory));
            return true;
        }

        /// <summary>
        /// 添加主键
        /// </summary>
        public virtual bool AddPrimaryKey(string tableName, string columnName)
        {
            tableName = this.SqlBuilder.GetTranslationTableName(tableName);
            columnName = this.SqlBuilder.GetTranslationTableName(columnName);
            var temp = "PK_{0}_{1}";
            if (tableName.IsContainsIn(" ", "-"))
            {
                temp = SqlBuilder.GetTranslationColumnName(temp);
            }
            string sql = string.Format(this.AddPrimaryKeySql, tableName, string.Format(temp, this.SqlBuilder.GetNoTranslationColumnName(tableName).Replace("-", "_"), this.SqlBuilder.GetNoTranslationColumnName(columnName)), columnName);
            if ((tableName + columnName).Length > 25 && this.Context?.CurrentConnectionConfig?.MoreSettings?.MaxParameterNameLength > 0)
            {
                sql = string.Format(this.AddPrimaryKeySql, tableName, string.Format(temp, this.SqlBuilder.GetNoTranslationColumnName(tableName).GetNonNegativeHashCodeString(), "Id"), columnName);
            }
            this.Context.Ado.ExecuteCommand(sql);
            return true;
        }

        /// <summary>
        /// 添加复合主键
        /// </summary>
        public bool AddPrimaryKeys(string tableName, IReadOnlyList<string> columnNames)
        {
            tableName = this.SqlBuilder.GetTranslationTableName(tableName);
            var columnName = string.Join(",", columnNames);
            var pkName = string.Format("PK_{0}_{1}", this.SqlBuilder.GetNoTranslationColumnName(tableName), columnName.Replace(",", "_"));
            if (pkName.Length > 25 && this.Context?.CurrentConnectionConfig?.MoreSettings?.MaxParameterNameLength > 0)
            {
                pkName = "PK_" + pkName.GetNonNegativeHashCodeString();
            }
            columnName = string.Join(",", columnNames.Select(it => SqlBuilder.GetTranslationColumnName(it)));
            string sql = string.Format(this.AddPrimaryKeySql, tableName, pkName, columnName);
            this.Context.Ado.ExecuteCommand(sql);
            return true;
        }
        /// <summary>
        /// 添加复合主键(指定主键名)
        /// </summary>
        public bool AddPrimaryKeys(string tableName, IReadOnlyList<string> columnNames, string pkName)
        {
            tableName = this.SqlBuilder.GetTranslationTableName(tableName);
            var columnName = string.Join(",", columnNames);
            string sql = string.Format(this.AddPrimaryKeySql, tableName, pkName, columnName);
            this.Context.Ado.ExecuteCommand(sql);
            return true;
        }
        /// <summary>
        /// 添加列
        /// </summary>
        public virtual bool AddColumn(string tableName, DbColumnInfo columnInfo)
        {
            tableName = this.SqlBuilder.GetTranslationTableName(tableName);
            var isAddNotNUll = columnInfo.IsNullable == false && columnInfo.DefaultValue.HasValue();
            if (isAddNotNUll)
            {
                columnInfo = this.Context.Utilities.TranslateCopy(columnInfo);
                columnInfo.IsNullable = true;
            }
            string sql = GetAddColumnSql(tableName, columnInfo);
            this.Context.Ado.ExecuteCommand(sql);
            if (isAddNotNUll)
            {
                if (columnInfo.TableName == null)
                {
                    columnInfo.TableName = tableName;
                }
                var dtColumns = this.Context.Queryable<object>().AS(columnInfo.TableName).Where("1=2")
                    .Select(this.SqlBuilder.GetTranslationColumnName(columnInfo.DbColumnName)).ToDataTable().Columns.Cast<System.Data.DataColumn>();
                var dtColumInfo = dtColumns.First(it => it.ColumnName.EqualCase(columnInfo.DbColumnName));
                var type = UtilMethods.GetUnderType(dtColumInfo.DataType);
                var value = type == UtilConstants.StringType ? (object)"" : Activator.CreateInstance(type);
                if (this.Context.CurrentConnectionConfig.DbType == DbType.Oracle)
                {
                    value = columnInfo.DefaultValue;
                    if (value.Equals(""))
                    {
                        value = "empty";
                    }
                }
                value = GetDefaultValue(columnInfo, value);
                var dt = new Dictionary<string, object>();
                dt.Add(columnInfo.DbColumnName, value);
                if (columnInfo.DataType.EqualCase("json") && columnInfo.DefaultValue?.Contains('}') == true)
                {
                    {
                        dt[columnInfo.DbColumnName] = "{}";
                        var sqlobj = this.Context.UpdateableT(dt)
                        .AS(tableName)
                        .Where($"{this.SqlBuilder.GetTranslationColumnName(columnInfo.DbColumnName)} is null ").ToSql();
                        sqlobj.Value[0].IsJson = true;
                        this.Context.Ado.ExecuteCommand(sqlobj.Key, sqlobj.Value);
                    }
                }
                else if (columnInfo.DataType.EqualCase("json") && columnInfo.DefaultValue?.Contains(']') == true)
                {
                    {
                        dt[columnInfo.DbColumnName] = "[]";
                        var sqlobj = this.Context.UpdateableT(dt)
                        .AS(tableName)
                        .Where($"{this.SqlBuilder.GetTranslationColumnName(columnInfo.DbColumnName)} is null ").ToSql();
                        sqlobj.Value[0].IsJson = true;
                        this.Context.Ado.ExecuteCommand(sqlobj.Key, sqlobj.Value);
                    }
                }
                else
                {
                    this.Context.UpdateableT(dt)
                                 .AS(tableName)
                                 .Where($"{this.SqlBuilder.GetTranslationColumnName(columnInfo.DbColumnName)} is null ").ExecuteCommand();
                }
                columnInfo.IsNullable = false;
                UpdateColumn(tableName, columnInfo);
            }
            return true;
        }
        /// <summary>
        /// 获取默认值
        /// </summary>
        public virtual object GetDefaultValue(DbColumnInfo columnInfo, object value)
        {
            if (columnInfo.DataType.ObjToString().IsInCase("varchar", "nvarchar", "varchar2", "nvarchar2") && !string.IsNullOrEmpty(columnInfo.DefaultValue) && Regex.IsMatch(columnInfo.DefaultValue, @"^\w+$"))
            {
                value = columnInfo.DefaultValue;
            }
            else if (columnInfo.DataType.ObjToString().IsInCase("float", "double", "decimal", "int", "int4", "bigint", "int8", "int2") && columnInfo.DefaultValue.IsInt())
            {
                value = Convert.ToInt32(columnInfo.DefaultValue);
            }
            return value;
        }
        /// <summary>
        /// 更新列
        /// </summary>
        public virtual bool UpdateColumn(string tableName, DbColumnInfo column)
        {
            tableName = this.SqlBuilder.GetTranslationTableName(tableName);
            string sql = GetUpdateColumnSql(tableName, column);
            this.Context.Ado.ExecuteCommand(sql);
            return true;
        }
        /// <summary>
        /// 创建表
        /// </summary>
        public abstract bool CreateTable(string tableName, List<DbColumnInfo> columns, bool isCreatePrimaryKey = true);
        /// <summary>
        /// 删除表
        /// </summary>
        public virtual bool DropTable(string tableName)
        {
            tableName = this.SqlBuilder.GetTranslationTableName(tableName);
            this.Context.Ado.ExecuteCommand(string.Format(this.DropTableSql, tableName));
            return true;
        }
        /// <summary>
        /// 批量删除表
        /// </summary>
        public virtual bool DropTable(params string[] tableName)
        {
            foreach (var item in tableName)
            {
                DropTable(item);
            }
            return true;
        }
        /// <summary>
        /// 根据实体类型删除表
        /// </summary>
        public virtual bool DropTable(params Type[] tableEnittyTypes)
        {
            foreach (var item in tableEnittyTypes)
            {
                var tableName = this.Context.EntityMaintenance.GetTableName(item);
                DropTable(tableName);
            }
            return true;
        }
        /// <summary>
        /// 删除表(泛型)
        /// </summary>
        public virtual bool DropTable<T>()
        {
            if (typeof(T).GetCustomAttribute<SplitTableAttribute>() != null)
            {
                var tables = this.Context.SplitHelper(typeof(T)).GetTables();
                foreach (var item in tables)
                {
                    this.Context.DbMaintenance.DropTable(SqlBuilder.GetTranslationColumnName(item.TableName));
                }
                return true;
            }
            else
            {
                var tableName = this.Context.EntityMaintenance.GetTableName<T>();
                return DropTable(tableName);
            }
        }
        /// <summary>
        /// 删除多个表(泛型)
        /// </summary>
        public virtual bool DropTable<T, T2>()
        {
            DropTable<T>();
            DropTable<T2>();
            return true;
        }
        /// <summary>
        /// 删除多个表(泛型)
        /// </summary>
        public virtual bool DropTable<T, T2, T3>()
        {
            DropTable<T>();
            DropTable<T2>();
            DropTable<T3>();
            return true;
        }
        /// <summary>
        /// 删除多个表(泛型)
        /// </summary>
        public virtual bool DropTable<T, T2, T3, T4>()
        {
            DropTable<T>();
            DropTable<T2>();
            DropTable<T3>();
            DropTable<T4>();
            return true;
        }
        /// <summary>
        /// 清空表(泛型)
        /// </summary>
        public virtual bool TruncateTable<T>()
        {
            if (typeof(T).GetCustomAttribute<SplitTableAttribute>() != null)
            {
                var tables = this.Context.SplitHelper(typeof(T)).GetTables();
                foreach (var item in tables)
                {
                    this.Context.DbMaintenance.TruncateTable(SqlBuilder.GetTranslationColumnName(item.TableName));
                }
                return true;
            }
            else
            {
                this.Context.InitMappingInfo<T>();
                return this.TruncateTable(this.Context.EntityMaintenance.GetEntityInfo<T>().DbTableName);
            }
        }
        /// <summary>
        /// 清空多个表(泛型)
        /// </summary>
        public virtual bool TruncateTable<T, T2>()
        {
            TruncateTable<T>();
            TruncateTable<T2>();
            return true;
        }
        /// <summary>
        /// 清空多个表(泛型)
        /// </summary>
        public virtual bool TruncateTable<T, T2, T3>()
        {
            TruncateTable<T>();
            TruncateTable<T2>();
            TruncateTable<T3>();
            return true;
        }
        /// <summary>
        /// 清空多个表(泛型)
        /// </summary>
        public virtual bool TruncateTable<T, T2, T3, T4>()
        {
            TruncateTable<T>();
            TruncateTable<T2>();
            TruncateTable<T3>();
            TruncateTable<T4>();
            return true;
        }
        /// <summary>
        /// 清空多个表(泛型)
        /// </summary>
        public virtual bool TruncateTable<T, T2, T3, T4, T5>()
        {
            TruncateTable<T>();
            TruncateTable<T2>();
            TruncateTable<T3>();
            TruncateTable<T4>();
            TruncateTable<T5>();
            return true;
        }
        /// <summary>
        /// 删除列
        /// </summary>
        public virtual bool DropColumn(string tableName, string columnName)
        {
            columnName = this.SqlBuilder.GetTranslationColumnName(columnName);
            tableName = this.SqlBuilder.GetTranslationTableName(tableName);
            this.Context.Ado.ExecuteCommand(string.Format(this.DropColumnToTableSql, tableName, columnName));
            return true;
        }
        /// <summary>
        /// 删除约束
        /// </summary>
        public virtual bool DropConstraint(string tableName, string constraintName)
        {
            tableName = this.SqlBuilder.GetTranslationTableName(tableName);
            string sql = string.Format(this.DropConstraintSql, tableName, constraintName);
            this.Context.Ado.ExecuteCommand(sql);
            return true;
        }
        /// <summary>
        /// 清空表
        /// </summary>
        public virtual bool TruncateTable(string tableName)
        {
            tableName = this.SqlBuilder.GetTranslationTableName(tableName);
            this.Context.Ado.ExecuteCommand(string.Format(this.TruncateTableSql, tableName));
            return true;
        }
        /// <summary>
        /// 批量清空表
        /// </summary>
        public bool TruncateTable(params string[] tableNames)
        {
            foreach (var item in tableNames)
            {
                TruncateTable(item);
            }
            return true;
        }
        /// <summary>
        /// 根据实体类型清空表
        /// </summary>
        public bool TruncateTable(params Type[] tableEnittyTypes)
        {
            foreach (var item in tableEnittyTypes)
            {
                var name = this.Context.EntityMaintenance.GetTableName(item);
                TruncateTable(name);
            }
            return true;
        }
        /// <summary>
        /// 备份数据库
        /// </summary>
        public virtual bool BackupDataBase(string databaseName, string fullFileName)
        {
            var directory = FileHelper.GetDirectoryFromFilePath(fullFileName);
            if (!FileHelper.IsExistDirectory(directory))
            {
                FileHelper.CreateDirectory(directory);
            }
            this.Context.Ado.ExecuteCommand(string.Format(this.BackupDataBaseSql, databaseName, fullFileName));
            return true;
        }
        /// <summary>
        /// 备份表
        /// </summary>
        public virtual bool BackupTable(string oldTableName, string newTableName, int maxBackupDataRows = int.MaxValue)
        {
            oldTableName = this.SqlBuilder.GetTranslationTableName(oldTableName);
            newTableName = this.SqlBuilder.GetTranslationTableName(newTableName);
            string sql = string.Format(this.BackupTableSql, maxBackupDataRows, newTableName, oldTableName);
            this.Context.Ado.ExecuteCommand(sql);
            return true;
        }
        /// <summary>
        /// 重命名列
        /// </summary>
        public virtual bool RenameColumn(string tableName, string oldColumnName, string newColumnName)
        {
            tableName = this.SqlBuilder.GetTranslationTableName(tableName);
            oldColumnName = this.SqlBuilder.GetTranslationColumnName(oldColumnName);
            newColumnName = this.SqlBuilder.GetTranslationColumnName(newColumnName);
            string sql = string.Format(this.RenameColumnSql, tableName, oldColumnName, newColumnName);
            this.Context.Ado.ExecuteCommand(sql);
            return true;
        }
        /// <summary>
        /// 添加列注释
        /// </summary>
        public virtual bool AddColumnRemark(string columnName, string tableName, string description)
        {
            string sql = string.Format(this.AddColumnRemarkSql, columnName, tableName, description);
            this.Context.Ado.ExecuteCommand(sql);
            return true;
        }
        /// <summary>
        /// 删除列注释
        /// </summary>
        public virtual bool DeleteColumnRemark(string columnName, string tableName)
        {
            string sql = string.Format(this.DeleteColumnRemarkSql, columnName, tableName);
            this.Context.Ado.ExecuteCommand(sql);
            return true;
        }
        /// <summary>
        /// 检查列注释是否存在
        /// </summary>
        public virtual bool IsAnyColumnRemark(string columnName, string tableName)
        {
            string sql = string.Format(this.IsAnyColumnRemarkSql, columnName, tableName);
            var dt = this.Context.Ado.GetDataTable(sql);
            return dt.Rows?.Count > 0;
        }
        /// <summary>
        /// 添加表注释
        /// </summary>
        public virtual bool AddTableRemark(string tableName, string description)
        {
            string sql = string.Format(this.AddTableRemarkSql, tableName, description);
            this.Context.Ado.ExecuteCommand(sql);
            return true;
        }
        /// <summary>
        /// 删除表注释
        /// </summary>
        public virtual bool DeleteTableRemark(string tableName)
        {
            string sql = string.Format(this.DeleteTableRemarkSql, tableName);
            this.Context.Ado.ExecuteCommand(sql);
            return true;
        }
        /// <summary>
        /// 检查表注释是否存在
        /// </summary>
        public virtual bool IsAnyTableRemark(string tableName)
        {
            string sql = string.Format(this.IsAnyTableRemarkSql, tableName);
            var dt = this.Context.Ado.GetDataTable(sql);
            return dt.Rows?.Count > 0;
        }
        /// <summary>
        /// 添加默认值
        /// </summary>
        public virtual bool AddDefaultValue(string tableName, string columnName, string defaultValue)
        {
            if (defaultValue == "''")
            {
                defaultValue = "";
            }
            if (defaultValue.IsDate() && !AddDefaultValueSql.Contains("'{2}'"))
            {
                defaultValue = "'" + defaultValue + "'";
            }
            if (defaultValue?.EqualCase("'current_timestamp'") == true)
            {
                defaultValue = defaultValue.TrimEnd('\'').TrimStart('\'');
            }
            if (defaultValue?.EqualCase("'current_date'") == true)
            {
                defaultValue = defaultValue.TrimEnd('\'').TrimStart('\'');
            }
            string sql = string.Format(AddDefaultValueSql, tableName, columnName, defaultValue);
            this.Context.Ado.ExecuteCommand(sql);
            return true;
        }
        /// <summary>
        /// 创建索引
        /// </summary>
        public virtual bool CreateIndex(string tableName, IReadOnlyList<string> columnNames, bool isUnique = false)
        {
            string sql = string.Format(CreateIndexSql, this.SqlBuilder.GetTranslationTableName(tableName), string.Join(",", columnNames.Select(it => this.SqlBuilder.GetTranslationColumnName(it))), string.Join("_", columnNames) + this.Context.CurrentConnectionConfig.IndexSuffix, isUnique ? "UNIQUE" : "");
            sql = sql.Replace("_" + this.SqlBuilder.SqlTranslationLeft, "_");
            sql = sql.Replace(this.SqlBuilder.SqlTranslationRight + "_", "_");
            sql = sql.Replace(this.SqlBuilder.SqlTranslationLeft + this.SqlBuilder.SqlTranslationLeft, this.SqlBuilder.SqlTranslationLeft);
            sql = sql.Replace(this.SqlBuilder.SqlTranslationRight + this.SqlBuilder.SqlTranslationRight, this.SqlBuilder.SqlTranslationRight);
            this.Context.Ado.ExecuteCommand(sql);
            return true;
        }
        /// <summary>
        /// 创建唯一索引
        /// </summary>
        public virtual bool CreateUniqueIndex(string tableName, IReadOnlyList<string> columnNames)
        {
            string sql = string.Format(CreateIndexSql, this.SqlBuilder.GetTranslationTableName(tableName), string.Join(",", columnNames.Select(it => this.SqlBuilder.GetTranslationColumnName(it))), string.Join("_", columnNames) + this.Context.CurrentConnectionConfig.IndexSuffix + "_Unique", "UNIQUE");
            sql = sql.Replace("_" + this.SqlBuilder.SqlTranslationLeft, "_");
            sql = sql.Replace(this.SqlBuilder.SqlTranslationRight + "_", "_");
            sql = sql.Replace(this.SqlBuilder.SqlTranslationLeft + this.SqlBuilder.SqlTranslationLeft, this.SqlBuilder.SqlTranslationLeft);
            sql = sql.Replace(this.SqlBuilder.SqlTranslationRight + this.SqlBuilder.SqlTranslationRight, this.SqlBuilder.SqlTranslationRight);
            this.Context.Ado.ExecuteCommand(sql);
            return true;
        }
        /// <summary>
        /// 创建索引(指定索引名)
        /// </summary>
        public virtual bool CreateIndex(string tableName, IReadOnlyList<string> columnNames, string IndexName, bool isUnique = false)
        {
            var include = "";
            if (IndexName.Contains("{include:", StringComparison.CurrentCultureIgnoreCase))
            {
                include = Regex.Match(IndexName, @"\{include\:.+$").Value;
                IndexName = IndexName.Replace(include, "");
                if (include == null)
                {
                    throw new Exception("include format error");
                }
                include = include.Replace("{include:", "").Replace("}", "");
                include = $"include({include})";
            }
            string sql = string.Format("CREATE {3} INDEX {2} ON {0}({1})" + include, this.SqlBuilder.GetTranslationColumnName(tableName), string.Join(",", columnNames), IndexName, isUnique ? "UNIQUE" : "");
            this.Context.Ado.ExecuteCommand(sql);
            return true;
        }
        /// <summary>
        /// 检查索引是否存在
        /// </summary>
        public virtual bool IsAnyIndex(string indexName)
        {
            string sql = string.Format(this.IsAnyIndexSql, indexName, this.Context.Ado.Connection.Database);
            return this.Context.Ado.GetInt(sql) > 0;
        }
        /// <summary>
        /// 添加注释
        /// </summary>
        public virtual bool AddRemark(EntityInfo entity)
        {
            var db = this.Context;
            var columns = entity.Columns.Where(it => it.IsIgnore == false);
            List<DbColumnInfo> dbColumn = new List<DbColumnInfo>();
            if (entity.Columns.Any(it => it.ColumnDescription.HasValue()))
            {
                dbColumn = db.DbMaintenance.GetColumnInfosByTableName(entity.DbTableName, false);
            }
            foreach (var item in columns)
            {
                if (item.ColumnDescription != null)
                {
                    //column remak
                    if (db.DbMaintenance.IsAnyColumnRemark(item.DbColumnName, item.DbTableName))
                    {
                        if (!dbColumn.Any(it => it.DbColumnName == item.DbColumnName && it.ColumnDescription == item.ColumnDescription))
                        {
                            db.DbMaintenance.DeleteColumnRemark(item.DbColumnName, item.DbTableName);
                            db.DbMaintenance.AddColumnRemark(item.DbColumnName, item.DbTableName, item.ColumnDescription);
                        }
                    }
                    else
                    {
                        db.DbMaintenance.AddColumnRemark(item.DbColumnName, item.DbTableName, item.ColumnDescription);
                    }
                }
            }

            //table remak
            if (entity.TableDescription != null)
            {
                if (db.DbMaintenance.IsAnyTableRemark(entity.DbTableName))
                {
                    db.DbMaintenance.DeleteTableRemark(entity.DbTableName);
                    db.DbMaintenance.AddTableRemark(entity.DbTableName, entity.TableDescription);
                }
                else
                {
                    db.DbMaintenance.AddTableRemark(entity.DbTableName, entity.TableDescription);
                }
            }
            return true;
        }

        /// <summary>
        /// 添加索引
        /// </summary>
        public virtual void AddIndex(EntityInfo entityInfo)
        {
            var db = this.Context;
            var columns = entityInfo.Columns.Where(it => it.IsIgnore == false);
            var indexColumns = columns.Where(it => it.IndexGroupNameList.HasValue());
            if (indexColumns.HasValue())
            {
                var groups = indexColumns.SelectMany(it => it.IndexGroupNameList).GroupBy(it => it).Select(it => it.Key);
                foreach (var item in groups)
                {
                    var columnNames = indexColumns.Where(it => it.IndexGroupNameList.Any(i => i.Equals(item, StringComparison.CurrentCultureIgnoreCase))).Select(it => it.DbColumnName).ToArray();
                    var indexName = string.Format("Index_{0}_{1}" + this.Context.CurrentConnectionConfig.IndexSuffix, entityInfo.DbTableName, string.Join("_", columnNames));
                    if (!IsAnyIndex(indexName))
                    {
                        CreateIndex(entityInfo.DbTableName, columnNames);
                    }
                }
            }


            var uIndexColumns = columns.Where(it => it.UIndexGroupNameList.HasValue());
            if (uIndexColumns.HasValue())
            {
                var groups = uIndexColumns.SelectMany(it => it.UIndexGroupNameList).GroupBy(it => it).Select(it => it.Key);
                foreach (var item in groups)
                {
                    var columnNames = uIndexColumns.Where(it => it.UIndexGroupNameList.Any(i => i.Equals(item, StringComparison.CurrentCultureIgnoreCase))).Select(it => it.DbColumnName).ToArray();
                    var indexName = string.Format("Index_{0}_{1}_Unique" + this.Context.CurrentConnectionConfig.IndexSuffix, entityInfo.DbTableName, string.Join("_", columnNames));
                    if (!IsAnyIndex(indexName))
                    {
                        CreateUniqueIndex(entityInfo.DbTableName, columnNames);
                    }
                }
            }
        }

        /// <summary>
        /// 检查是否有默认值
        /// </summary>
        protected virtual bool IsAnyDefaultValue(string tableName, string columnName, List<DbColumnInfo> columns)
        {
            var defaultValue = columns.Where(it => it.DbColumnName.Equals(columnName, StringComparison.CurrentCultureIgnoreCase)).First().DefaultValue;
            return defaultValue.HasValue();
        }

        /// <summary>
        /// 检查是否有默认值
        /// </summary>
        public virtual bool IsAnyDefaultValue(string tableName, string columnName)
        {
            return IsAnyDefaultValue(tableName, columnName, this.GetColumnInfosByTableName(tableName, false));
        }

        /// <summary>
        /// 添加默认值
        /// </summary>
        public virtual void AddDefaultValue(EntityInfo entityInfo)
        {
            var dbColumns = this.GetColumnInfosByTableName(entityInfo.DbTableName, false);
            var db = this.Context;
            var columns = entityInfo.Columns.Where(it => it.IsIgnore == false);
            foreach (var item in columns)
            {
                if (item.DefaultValue != null)
                {
                    if (!IsAnyDefaultValue(entityInfo.DbTableName, item.DbColumnName, dbColumns))
                    {
                        this.AddDefaultValue(entityInfo.DbTableName, item.DbColumnName, item.DefaultValue);
                    }
                }
            }
        }

        /// <summary>
        /// 重命名表
        /// </summary>
        public virtual bool RenameTable(string oldTableName, string newTableName)
        {
            string sql = string.Format(this.RenameTableSql, oldTableName, newTableName);
            this.Context.Ado.ExecuteCommand(sql);
            return true;
        }
        /// <summary>
        /// 检查存储过程是否存在
        /// </summary>
        public virtual bool IsAnyProcedure(string procName)
        {
            string sql = string.Format(this.IsAnyProcedureSql, procName);
            return this.Context.Ado.GetInt(sql) > 0;
        }
        #endregion

        #region Private
        /// <summary>
        /// 获取架构表信息
        /// </summary>
        public virtual List<DbTableInfo> GetSchemaTables(EntityInfo entityInfo)
        {
            return null;
        }
        /// <summary>
        /// 获取列表或缓存
        /// </summary>
        protected List<T> GetListOrCache<T>(string cacheKey, string sql)
        {
            return this.Context.Utilities.GetReflectionInoCacheInstance().GetOrCreate(cacheKey,
             () =>
             {
                 var isEnableLogEvent = this.Context.Ado.IsEnableLogEvent;
                 this.Context.Ado.IsEnableLogEvent = false;
                 var result = this.Context.Ado.SqlQuery<T>(sql);
                 this.Context.Ado.IsEnableLogEvent = isEnableLogEvent;
                 return result;
             });
        }
        /// <summary>
        /// 获取创建表SQL
        /// </summary>
        protected virtual string GetCreateTableSql(string tableName, List<DbColumnInfo> columns)
        {
            List<string> columnArray = new List<string>();
            Check.Exception(columns.IsNullOrEmpty(), "No columns found ");
            foreach (var item in columns)
            {
                string columnName = this.SqlBuilder.GetTranslationTableName(item.DbColumnName);
                string dataType = item.DataType;
                string dataSize = GetSize(item);
                string nullType = item.IsNullable ? this.CreateTableNull : CreateTableNotNull;
                string primaryKey = null;
                string identity = item.IsIdentity ? this.CreateTableIdentity : null;
                string addItem = string.Format(this.CreateTableColumn, columnName, dataType, dataSize, nullType, primaryKey, identity);
                columnArray.Add(addItem);
            }
            string tableString = string.Format(this.CreateTableSql, this.SqlBuilder.GetTranslationTableName(tableName), string.Join(",\r\n", columnArray));
            return tableString;
        }
        /// <summary>
        /// 获取添加列SQL
        /// </summary>
        protected virtual string GetAddColumnSql(string tableName, DbColumnInfo columnInfo)
        {
            string columnName = this.SqlBuilder.GetTranslationColumnName(columnInfo.DbColumnName);
            tableName = this.SqlBuilder.GetTranslationTableName(tableName);
            string dataType = columnInfo.DataType;
            if (dataType.EqualCase("varchar")
                && this.Context.CurrentConnectionConfig?.MoreSettings?.SqlServerCodeFirstNvarchar == true
                && this.Context.CurrentConnectionConfig?.DbType == DbType.SqlServer)
            {
                dataType = "nvarchar";
            }
            string dataSize = GetSize(columnInfo);
            string nullType = columnInfo.IsNullable ? this.CreateTableNull : CreateTableNotNull;
            string primaryKey = null;
            string identity = null;
            string result = string.Format(this.AddColumnToTableSql, tableName, columnName, dataType, dataSize, nullType, primaryKey, identity);
            return result;
        }
        /// <summary>
        /// 获取更新列SQL
        /// </summary>
        protected virtual string GetUpdateColumnSql(string tableName, DbColumnInfo columnInfo)
        {
            string columnName = this.SqlBuilder.GetTranslationColumnName(columnInfo.DbColumnName);
            tableName = this.SqlBuilder.GetTranslationTableName(tableName);
            string dataSize = GetSize(columnInfo);
            string dataType = columnInfo.DataType;
            string nullType = columnInfo.IsNullable ? this.CreateTableNull : CreateTableNotNull;
            string primaryKey = null;
            string identity = null;
            string result = string.Format(this.AlterColumnToTableSql, tableName, columnName, dataType, dataSize, nullType, primaryKey, identity);
            return result;
        }
        /// <summary>
        /// 获取缓存键
        /// </summary>
        protected virtual string GetCacheKey(string cacheKey)
        {
            return this.Context.CurrentConnectionConfig.DbType + "." + this.Context.Ado.Connection.Database + "." + cacheKey;
        }
        /// <summary>
        /// 获取列大小
        /// </summary>
        protected virtual string GetSize(DbColumnInfo item)
        {
            string dataSize = null;
            var isMax = item.Length > 4000 || item.Length == -1;
            if (isMax)
            {
                dataSize = item?.Length > 0 ? string.Format("({0})", "max") : null;
            }
            else if (item.Length == 0 && item.DecimalDigits > 0)
            {
                item.Length = 10;
                dataSize = string.Format("({0},{1})", item.Length, item.DecimalDigits);
            }
            else if (item?.Length > 0 && item.DecimalDigits == 0)
            {
                dataSize = item?.Length > 0 ? string.Format("({0})", item.Length) : null;
            }
            else if (item?.Length > 0 && item.DecimalDigits > 0)
            {
                dataSize = item?.Length > 0 ? string.Format("({0},{1})", item.Length, item.DecimalDigits) : null;
            }
            return dataSize;
        }
        #endregion
    }
}