using System.Data;

namespace ThingsGateway.SqlSugar
{
    public partial class FastestProvider<T> : IFastest<T> where T : class, new()
    {
        /// <summary>获取对应数据库类型的快速构建器</summary>
        private IFastBuilder GetBuider()
        {
            var className = string.Empty;
            switch (this.context.CurrentConnectionConfig.DbType)
            {
                case DbType.MySql:
                    var result1 = new MySqlFastBuilder();
                    result1.CharacterSet = this.CharacterSet;
                    return result1;
                case DbType.SqlServer:
                    var result2 = new SqlServerFastBuilder();
                    result2.DbFastestProperties.IsOffIdentity = this.IsOffIdentity;
                    return result2;
                case DbType.Sqlite:
                    var resultSqlite = new SqliteFastBuilder(this.entityInfo);
                    if (resultSqlite.DbFastestProperties != null)
                        resultSqlite.DbFastestProperties.IsIgnoreInsertError = this.IsIgnoreInsertError;
                    return resultSqlite;
                case DbType.Oracle:
                    return new OracleFastBuilder(this.entityInfo);
                case DbType.PostgreSQL:
                    return new PostgreSQLFastBuilder(this.entityInfo);
                case DbType.MySqlConnector:
                    var resultConnector = InstanceFactory.CreateInstance<IFastBuilder>($"{SugarConst.StartName}SqlSugar.MySqlConnector.MySqlFastBuilder");
                    resultConnector.CharacterSet = this.CharacterSet;
                    return resultConnector;
                case DbType.Dm:
                    var result3 = new DmFastBuilder();
                    result3.DbFastestProperties.IsOffIdentity = this.IsOffIdentity;
                    return result3;
                case DbType.QuestDB:
                    return new QuestDBFastBuilder(this.entityInfo);
                case DbType.Custom:
                    className = InstanceFactory.CustomNamespace + "." + InstanceFactory.CustomDbName + "FastBuilder";
                    break;
                default:
                    className = $"{SugarConst.StartName}SqlSugar.{this.context.CurrentConnectionConfig.DbType.ToString().Replace("Native", "")}FastBuilder";
                    break;
            }
            var result = InstanceFactory.CreateInstance<IFastBuilder>(className);
            result.CharacterSet = this.CharacterSet;
            result.FastEntityInfo = this.entityInfo;
            return result;
        }

        /// <summary>将实体列表转换为DataTable</summary>
        private DataTable ToDdateTable(IEnumerable<T> datas)
        {
            var builder = GetBuider();
            DataTable tempDataTable = ReflectionInoCore<DataTable>.GetInstance().GetOrCreate("BulkCopyAsync" + typeof(T).GetHashCode(),
            () =>
            {
                if (AsName == null)
                {
                    return queryable.Where(it => false).Select("*").ToDataTable();
                }
                else
                {
                    return queryable.AS(AsName).Where(it => false).Select("*").ToDataTable();
                }
            }
            );
            var dt = new DataTable();
            List<string> uInt64TypeName = new List<string>();
            foreach (DataColumn item in tempDataTable.Columns)
            {
                if (item.DataType == typeof(UInt64))
                {
                    uInt64TypeName.Add(item.ColumnName);
                }
                if (item.DataType.Name == "ClickHouseDecimal")
                {
                    dt.Columns.Add(item.ColumnName, typeof(decimal));
                }
                else
                {
                    dt.Columns.Add(item.ColumnName, item.DataType);
                }
            }
            dt.TableName = GetTableName();
            var columns = entityInfo.Columns;
            if (columns.Where(it => !it.IsIgnore).Count() > tempDataTable.Columns.Count)
            {
                var tempColumns = tempDataTable.Columns.Cast<DataColumn>().Select(it => it.ColumnName);
                columns = columns.Where(it => tempColumns.Contains(it.DbColumnName)).ToList();
            }
            var isMySql = this.context.CurrentConnectionConfig.DbType.IsIn(DbType.MySql, DbType.MySqlConnector);
            var isSqliteCore = SugarCompatible.IsFramework == false && this.context.CurrentConnectionConfig.DbType.IsIn(DbType.Sqlite);
            foreach (var item in datas)
            {
                var dr = dt.NewRow();
                foreach (var column in columns)
                {
                    if (column.IsIgnore)
                    {
                        continue;
                    }
                    var name = column.DbColumnName;
                    if (name == null)
                    {
                        name = column.PropertyName;
                    }
                    var value = ValueConverter(column, GetValue(item, column));
                    if (column.SqlParameterDbType != null && column.SqlParameterDbType is Type && UtilMethods.HasInterface((Type)column.SqlParameterDbType, typeof(ISugarDataConverter)))
                    {
                        var columnInfo = column;
                        var p = UtilMethods.GetParameterConverter(0, value, columnInfo);
                        value = p.Value;
                    }
                    else if (isMySql && column.UnderType == UtilConstants.BoolType)
                    {
                        if (value.ObjToBool() == false && uInt64TypeName.Any(z => z.EqualCase(column.DbColumnName)))
                        {
                            value = DBNull.Value;
                        }
                    }
                    else if (isSqliteCore && column.UnderType == UtilConstants.StringType && value is bool)
                    {
                        value = "isSqliteCore_" + value.ObjToString();
                    }
                    else if (isSqliteCore && column.UnderType == UtilConstants.BoolType && value is bool)
                    {
                        value = Convert.ToBoolean(value) ? 1 : 0;
                    }
                    else if (column.UnderType == UtilConstants.DateTimeOffsetType && value != null && value != DBNull.Value)
                    {
                        if (builder.DbFastestProperties?.HasOffsetTime == true)
                        {
                            //Don't need to deal with
                        }
                        else
                        {
                            value = UtilMethods.ConvertFromDateTimeOffset((DateTimeOffset)value);
                        }
                    }
                    else if (value != DBNull.Value && value != null && column.UnderType?.FullName == "System.TimeOnly")
                    {
                        value = UtilMethods.TimeOnlyToTimeSpan(value);
                    }
                    else if (value != DBNull.Value && value != null && column.UnderType?.FullName == "System.DateOnly")
                    {
                        value = UtilMethods.DateOnlyToDateTime(value);
                    }
                    dr[name] = value;
                }
                dt.Rows.Add(dr);
            }

            return dt;
        }
        /// <summary>将实体列表转换为DataTable</summary>
        private Dictionary<string, ValueTuple<Type, List<DataInfos>>> ToDdict(IEnumerable<T> datas)
        {
            var builder = GetBuider();
            DataTable tempDataTable = ReflectionInoCore<DataTable>.GetInstance().GetOrCreate("BulkCopyAsync" + typeof(T).GetHashCode(),
            () =>
            {
                if (AsName == null)
                {
                    return queryable.Where(it => false).Select("*").ToDataTable();
                }
                else
                {
                    return queryable.AS(AsName).Where(it => false).Select("*").ToDataTable();
                }
            }
            );
            HashSet<string> uInt64TypeName = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, ValueTuple<Type, List<DataInfos>>> results = new(StringComparer.OrdinalIgnoreCase);
            foreach (DataColumn item in tempDataTable.Columns)
            {
                if (item.DataType == typeof(UInt64))
                {
                    uInt64TypeName.Add(item.ColumnName);
                }
                if (item.DataType.Name == "ClickHouseDecimal")
                {
                    results.Add(item.ColumnName, (typeof(decimal), new()));
                }
                else
                {
                    results.Add(item.ColumnName, (item.DataType, new()));
                }
            }
            var columns = entityInfo.Columns;
            if (columns.Where(it => !it.IsIgnore).Count() > tempDataTable.Columns.Count)
            {
                var tempColumns = tempDataTable.Columns.Cast<DataColumn>().Select(it => it.ColumnName);
                columns = columns.Where(it => tempColumns.Contains(it.DbColumnName)).ToList();
            }
            var isMySql = this.context.CurrentConnectionConfig.DbType.IsIn(DbType.MySql, DbType.MySqlConnector);
            var isSqliteCore = SugarCompatible.IsFramework == false && this.context.CurrentConnectionConfig.DbType.IsIn(DbType.Sqlite);
            foreach (var item in datas)
            {
                foreach (var column in columns)
                {
                    if (column.IsIgnore)
                    {
                        continue;
                    }
                    var name = column.DbColumnName;
                    if (name == null)
                    {
                        name = column.PropertyName;
                    }
                    var value = ValueConverter(column, GetValue(item, column));
                    if (column.SqlParameterDbType != null && column.SqlParameterDbType is Type && UtilMethods.HasInterface((Type)column.SqlParameterDbType, typeof(ISugarDataConverter)))
                    {
                        var columnInfo = column;
                        var p = UtilMethods.GetParameterConverter(0, value, columnInfo);
                        value = p.Value;
                    }
                    else if (isMySql && column.UnderType == UtilConstants.BoolType)
                    {
                        if (value.ObjToBool() == false && uInt64TypeName.Any(z => z.EqualCase(column.DbColumnName)))
                        {
                            value = DBNull.Value;
                        }
                    }
                    else if (isSqliteCore && column.UnderType == UtilConstants.StringType && value is bool)
                    {
                        value = "isSqliteCore_" + value.ObjToString();
                    }
                    else if (isSqliteCore && column.UnderType == UtilConstants.BoolType && value is bool)
                    {
                        value = Convert.ToBoolean(value) ? 1 : 0;
                    }
                    else if (column.UnderType == UtilConstants.DateTimeOffsetType && value != null && value != DBNull.Value)
                    {
                        if (builder.DbFastestProperties?.HasOffsetTime == true)
                        {
                            //Don't need to deal with
                        }
                        else
                        {
                            value = UtilMethods.ConvertFromDateTimeOffset((DateTimeOffset)value);
                        }
                    }
                    else if (value != DBNull.Value && value != null && column.UnderType?.FullName == "System.TimeOnly")
                    {
                        value = UtilMethods.TimeOnlyToTimeSpan(value);
                    }
                    else if (value != DBNull.Value && value != null && column.UnderType?.FullName == "System.DateOnly")
                    {
                        value = UtilMethods.DateOnlyToDateTime(value);
                    }
                    var dr = new DataInfos();
                    dr.ColumnName = column.DbColumnName;
                    dr.Value = value;
                    results[column.DbColumnName].Item2.Add(dr);
                }
            }
            return results;
        }

        /// <summary>获取实体属性值</summary>
        private static object GetValue(T item, EntityColumnInfo column)
        {
            if (StaticConfig.EnableAot)
            {
                return column.PropertyInfo.GetValue(item);
            }
            else
            {
                return PropertyCallAdapterProvider<T>.GetInstance(column.PropertyName).InvokeGet(item);
            }
        }

        /// <summary>获取表名</summary>
        private string GetTableName()
        {
            if (this.AsName.HasValue())
            {
                return queryable.SqlBuilder.GetTranslationTableName(AsName);
            }
            else
            {
                return queryable.SqlBuilder.GetTranslationTableName(this.context.EntityMaintenance.GetTableName<T>());
            }
        }

        /// <summary>值转换器</summary>
        private object ValueConverter(EntityColumnInfo columnInfo, object value)
        {
            if (value == null)
                return DBNull.Value;
            if (value is DateTime && (DateTime)value == DateTime.MinValue)
            {
                return UtilMethods.MinDate;
            }
            else if (columnInfo.UnderType.IsEnum())
            {
                value = Convert.ToInt64(value);
            }
            else if (columnInfo.IsJson && value != null)
            {
                value = this.context.Utilities.SerializeObject(value);
            }
            else if (columnInfo.IsTranscoding && value.HasValue())
            {
                value = UtilMethods.EncodeBase64(value.ToString());
            }
            return value;
        }

        /// <summary>获取可写入的DataTable</summary>
        private DataTable GetCopyWriteDataTable(DataTable dt)
        {
            var builder = GetBuider();
            if (builder.DbFastestProperties?.IsConvertDateTimeOffsetToDateTime == true)
            {
                dt = UtilMethods.ConvertDateTimeOffsetToDateTime(dt);
            }
            if (builder.DbFastestProperties?.IsNoCopyDataTable == true)
            {
                return dt;
            }
            DataTable tempDataTable = null;
            if (AsName == null)
            {
                tempDataTable = queryable.Clone().Where(it => false).Select("*").ToDataTable();
            }
            else
            {
                tempDataTable = queryable.Clone().AS(AsName).Where(it => false).Select("*").ToDataTable();
            }

            List<string> uInt64TypeName = new List<string>();
            foreach (DataColumn item in tempDataTable.Columns)
            {
                if (item.DataType == typeof(UInt64))
                {
                    uInt64TypeName.Add(item.ColumnName);
                }
            }
            var temColumnsList = tempDataTable.Columns.Cast<DataColumn>().Select(it => it.ColumnName).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var columns = dt.Columns.Cast<DataColumn>().Where(it => temColumnsList.Contains(it.ColumnName)).ToList();
            foreach (DataRow item in dt.Rows)
            {
                DataRow dr = tempDataTable.NewRow();
                foreach (DataColumn column in columns)
                {
                    dr[column.ColumnName] = item[column.ColumnName];
                    if (dr[column.ColumnName] == null || dr[column.ColumnName] == DBNull.Value)
                    {
                        dr[column.ColumnName] = DBNull.Value;
                    }
                    else if (column.DataType == UtilConstants.BoolType && this.context.CurrentConnectionConfig.DbType.IsIn(DbType.MySql, DbType.MySqlConnector))
                    {
                        if (Convert.ToBoolean(dr[column.ColumnName]) == false && uInt64TypeName.Any(z => z.EqualCase(column.ColumnName)))
                        {
                            dr[column.ColumnName] = DBNull.Value;
                        }
                    }
                }
                tempDataTable.Rows.Add(dr);
            }
            tempDataTable.TableName = dt.TableName;
            return tempDataTable;
        }

        /// <summary>获取可更新的DataTable</summary>
        private DataTable GetCopyWriteDataTableUpdate(DataTable dt)
        {
            var sqlBuilder = this.context.Queryable<object>().SqlBuilder;
            var dts = dt.Columns.Cast<DataColumn>().Select(it => sqlBuilder.GetTranslationColumnName(it.ColumnName));
            DataTable tempDataTable = null;
            if (AsName == null)
            {
                tempDataTable = queryable.Clone().Where(it => false).Select(string.Join(",", dts)).ToDataTable();
            }
            else
            {
                tempDataTable = queryable.Clone().AS(AsName).Where(it => false).Select(string.Join(",", dts)).ToDataTable();
            }

            List<string> uInt64TypeName = new List<string>();
            foreach (DataColumn item in tempDataTable.Columns)
            {
                if (item.DataType == typeof(UInt64))
                {
                    uInt64TypeName.Add(item.ColumnName);
                }
            }
            var temColumnsList = tempDataTable.Columns.Cast<DataColumn>().Select(it => it.ColumnName).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var columns = dt.Columns.Cast<DataColumn>().Where(it => temColumnsList.Contains(it.ColumnName, StringComparer.OrdinalIgnoreCase)).ToList();
            foreach (DataRow item in dt.Rows)
            {
                DataRow dr = tempDataTable.NewRow();
                foreach (DataColumn column in columns)
                {
                    dr[column.ColumnName] = item[column.ColumnName];
                    if (dr[column.ColumnName] == null || dr[column.ColumnName] == DBNull.Value)
                    {
                        dr[column.ColumnName] = DBNull.Value;
                    }
                    else if (column.DataType == UtilConstants.BoolType && this.context.CurrentConnectionConfig.DbType.IsIn(DbType.MySql, DbType.MySqlConnector))
                    {
                        if (Convert.ToBoolean(dr[column.ColumnName]) == false && uInt64TypeName.Any(z => z.EqualCase(column.ColumnName)))
                        {
                            dr[column.ColumnName] = DBNull.Value;
                        }
                    }
                }
                tempDataTable.Rows.Add(dr);
            }
            tempDataTable.TableName = dt.TableName;
            return tempDataTable;
        }

        /// <summary>移除缓存</summary>
        private void RemoveCache()
        {
            if (!string.IsNullOrEmpty(CacheKey) || !string.IsNullOrEmpty(CacheKeyLike))
            {
                if (this.context.CurrentConnectionConfig.ConfigureExternalServices?.DataInfoCacheService == null) { throw new SqlSugarException("ConnectionConfig.ConfigureExternalServices.DataInfoCacheService is null"); }
                var service = this.context.CurrentConnectionConfig.ConfigureExternalServices?.DataInfoCacheService;
                if (!string.IsNullOrEmpty(CacheKey))
                {
                    CacheSchemeMain.RemoveCache(service, CacheKey);
                }
                if (!string.IsNullOrEmpty(CacheKeyLike))
                {
                    CacheSchemeMain.RemoveCacheByLike(service, CacheKeyLike);
                }
            }
            if (this.context.CurrentConnectionConfig?.MoreSettings?.IsAutoRemoveDataCache == true)
            {
                var cacheService = this.context.CurrentConnectionConfig?.ConfigureExternalServices?.DataInfoCacheService;
                if (cacheService != null)
                {
                    CacheSchemeMain.RemoveCache(cacheService, this.context.EntityMaintenance.GetTableName<T>());
                }
            }
        }
    }
}