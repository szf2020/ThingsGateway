using Microsoft.Data.Sqlite;

using System.Data;

namespace ThingsGateway.SqlSugar
{
    public class SqliteFastBuilder : IFastBuilder
    {
        public EntityInfo FastEntityInfo { get; set; }
        private EntityInfo entityInfo;
        private bool IsUpdate = false;
        public string CharacterSet { get; set; }
        private DataTable UpdateDataTable { get; set; }
        public bool IsActionUpdateColumns { get; set; }
        public DbFastestProperties DbFastestProperties { get; set; } = new DbFastestProperties() { IsNoCopyDataTable = true };
        public SqliteFastBuilder(EntityInfo entityInfo)
        {
            this.entityInfo = entityInfo;
        }

        public SqlSugarProvider Context { get; set; }

        public void CloseDb()
        {
            if (this.Context.CurrentConnectionConfig.IsAutoCloseConnection && this.Context.Ado.Transaction == null)
            {
                this.Context.Close();
            }
        }

        public async Task CreateTempAsync<T>(DataTable dt) where T : class, new()
        {
            await Task.Delay(0).ConfigureAwait(false);
            IsUpdate = true;
        }

        public async Task<int> ExecuteBulkCopyAsync(DataTable dt)
        {
            if (dt.Rows.Count == 0 || IsUpdate)
            {
                this.UpdateDataTable = dt;
                return 0;
            }
            foreach (var item in this.entityInfo.Columns)
            {
                if (item.IsIdentity && dt.Columns.Contains(item.DbColumnName))
                {
                    dt.Columns.Remove(item.DbColumnName);
                }
            }
            var dictionary = this.Context.Utilities.DataTableToDictionaryList(dt.Rows.Cast<DataRow>().Take(1).CopyToDataTable());
            int result = 0;
            var cn = this.Context.Ado.Connection as SqliteConnection;
            Open(cn);
            if (this.Context.Ado.Transaction == null)
            {
#pragma warning disable CA2007 // 考虑对等待的任务调用 ConfigureAwait
                await using (var transaction = await cn.BeginTransactionAsync().ConfigureAwait(false))
                {
                    result = await _BulkCopy(dt, dictionary, result, cn).ConfigureAwait(false);
                    await transaction.CommitAsync().ConfigureAwait(false);
                }
#pragma warning restore CA2007 // 考虑对等待的任务调用 ConfigureAwait
            }
            else
            {
                result = await _BulkCopy(dt, dictionary, result, cn).ConfigureAwait(false);
            }
            return result;
        }

        private async Task<int> _BulkCopy(DataTable dt, List<Dictionary<string, object>> dictionary, int i, SqliteConnection cn)
        {
            using (var cmd = cn.CreateCommand())
            {
                if (this.Context?.CurrentConnectionConfig?.MoreSettings?.IsCorrectErrorSqlParameterName == true)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        cmd.CommandText = this.Context.InsertableT(UtilMethods.DataRowToDictionary(item)).AS(dt.TableName).ToSqlString().Replace(";SELECT LAST_INSERT_ROWID();", "");
                        TransformInsertCommand(cmd);
                        i += await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }
                }
                else
                {
                    cmd.CommandText = this.Context.InsertableT(dictionary[0]).AS(dt.TableName).ToSql().Key.Replace(";SELECT LAST_INSERT_ROWID();", "");
                    TransformInsertCommand(cmd);
                    foreach (DataRow dataRow in dt.Rows)
                    {
                        foreach (DataColumn item in dt.Columns)
                        {
                            if (IsBoolTrue(dataRow, item))
                            {
                                cmd.Parameters.AddWithValue("@" + item.ColumnName, true);
                            }
                            else if (IsBoolFalse(dataRow, item))
                            {
                                cmd.Parameters.AddWithValue("@" + item.ColumnName, false);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@" + item.ColumnName, dataRow[item.ColumnName]);
                            }
                        }
                        i += await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                        cmd.Parameters.Clear();
                    }
                }
            }
            return i;
        }
        private void TransformInsertCommand(SqliteCommand cmd)
        {
            if (this.DbFastestProperties?.IsIgnoreInsertError == true)
            {
                const string insertPrefix = "INSERT INTO ";
                if (cmd.CommandText.StartsWith(insertPrefix))
                {
                    cmd.CommandText = string.Concat("INSERT OR IGNORE  INTO  ", cmd.CommandText.AsSpan(insertPrefix.Length));
                }
            }
        }

        private async Task<int> _BulkUpdate(DataTable dt, List<Dictionary<string, object>> dictionary, int i, string[] whereColumns, string[] updateColumns, SqliteConnection cn)
        {
            using (var cmd = cn.CreateCommand())
            {
                if (this.Context?.CurrentConnectionConfig?.MoreSettings?.IsCorrectErrorSqlParameterName == true)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        cmd.CommandText = this.Context.UpdateableT(UtilMethods.DataRowToDictionary(item))
                         .WhereColumns(whereColumns)
                         .UpdateColumns(updateColumns)
                         .AS(dt.TableName).ToSqlString();
                        i += await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }
                }
                else
                {
                    cmd.CommandText = this.Context.UpdateableT(dictionary[0])
                        .WhereColumns(whereColumns)
                        .UpdateColumns(updateColumns)
                        .AS(dt.TableName).ToSql().Key;

                    foreach (DataRow dataRow in dt.Rows)
                    {
                        foreach (DataColumn item in dt.Columns)
                        {
                            if (IsBoolTrue(dataRow, item))
                            {
                                cmd.Parameters.AddWithValue("@" + item.ColumnName, true);
                            }
                            else if (IsBoolFalse(dataRow, item))
                            {
                                cmd.Parameters.AddWithValue("@" + item.ColumnName, false);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@" + item.ColumnName, dataRow[item.ColumnName]);
                            }
                        }
                        i += await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                        cmd.Parameters.Clear();
                    }
                }
            }
            return i;
        }

        private static bool IsBoolFalse(DataRow dataRow, DataColumn item)
        {
            return dataRow[item.ColumnName] != null && dataRow[item.ColumnName] is string && dataRow[item.ColumnName].ToString() == ("isSqliteCore_False");
        }

        private static bool IsBoolTrue(DataRow dataRow, DataColumn item)
        {
            return dataRow[item.ColumnName] != null && dataRow[item.ColumnName] is string && dataRow[item.ColumnName].ToString() == ("isSqliteCore_True");
        }

        private static void Open(SqliteConnection cn)
        {
            if (cn.State != ConnectionState.Open)
                cn.Open();
        }

        public async Task<int> UpdateByTempAsync(string tableName, string tempName, string[] updateColumns, string[] whereColumns)
        {
            var dt = UpdateDataTable;
            if (dt.Rows.Count == 0)
            {
                return 0;
            }
            var dictionary = this.Context.Utilities.DataTableToDictionaryList(dt.Rows.Cast<DataRow>().Take(1).CopyToDataTable());
            int result = 0;
            var cn = this.Context.Ado.Connection as SqliteConnection;
            Open(cn);
            if (this.Context.Ado.Transaction == null)
            {
#pragma warning disable CA2007 // 考虑对等待的任务调用 ConfigureAwait
                await using (var transaction = await cn.BeginTransactionAsync().ConfigureAwait(false))
                {
                    result = await _BulkUpdate(dt, dictionary, result, whereColumns, updateColumns, cn).ConfigureAwait(false);
                    await transaction.CommitAsync().ConfigureAwait(false);
                }
#pragma warning restore CA2007 // 考虑对等待的任务调用 ConfigureAwait
            }
            else
            {
                result = await _BulkUpdate(dt, dictionary, result, whereColumns, updateColumns, cn).ConfigureAwait(false);
            }
            return result;
        }

        public async Task<int> Merge<T>(string tableName, DataTable dt, EntityInfo entityInfo, string[] whereColumns, string[] updateColumns, IEnumerable<T> datas) where T : class, new()
        {
            var result = 0;
            await Context.Utilities.PageEachAsync(datas, 2000, async pageItems =>
            {
                var x = await Context.Storageable(pageItems).AS(tableName).WhereColumns(whereColumns).ToStorageAsync().ConfigureAwait(false);
                result += await x.BulkCopyAsync().ConfigureAwait(false);
                result += await x.BulkUpdateAsync(updateColumns).ConfigureAwait(false);
                return result;
            }).ConfigureAwait(false);
            return result;
        }
    }
}
