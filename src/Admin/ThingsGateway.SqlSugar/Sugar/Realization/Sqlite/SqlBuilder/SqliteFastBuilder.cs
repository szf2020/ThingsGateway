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
        private Dictionary<string, (Type, List<DataInfos>)> UpdateDataInfos { get; set; }
        public bool IsActionUpdateColumns { get; set; }
        public DbFastestProperties DbFastestProperties { get; set; } = new DbFastestProperties() { IsNoCopyDataTable = true, IsDataTable = false };
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
        Task<string> CreateTempAsync<T>(Dictionary<string, (Type, List<DataInfos>)> list) where T : class, new()
        {
            IsUpdate = true;
            return Task.FromResult(string.Empty);
        }
        public Task CreateTempAsync<T>(DataTable dt) where T : class, new()
        {

            IsUpdate = true;
            return Task.CompletedTask;
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

        public async Task<int> ExecuteBulkCopyAsync(string tableName, Dictionary<string, (Type, List<DataInfos>)> list)
        {
            if (list.Count == 0 || IsUpdate)
            {
                this.UpdateDataInfos = list;
                return 0;
            }
            foreach (var item in this.entityInfo.Columns)
            {
                if (item.IsIdentity)
                {
                    list.Remove(item.DbColumnName);
                }
            }
            int result = 0;
            var cn = this.Context.Ado.Connection as SqliteConnection;
            Open(cn);
            if (this.Context.Ado.Transaction == null)
            {
#pragma warning disable CA2007 // 考虑对等待的任务调用 ConfigureAwait
                await using (var transaction = await cn.BeginTransactionAsync().ConfigureAwait(false))
                {
                    result = await _BulkCopy(tableName, list, result, cn).ConfigureAwait(false);
                    await transaction.CommitAsync().ConfigureAwait(false);
                }
#pragma warning restore CA2007 // 考虑对等待的任务调用 ConfigureAwait
            }
            else
            {
                result = await _BulkCopy(tableName, list, result, cn).ConfigureAwait(false);
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

        private async Task<int> _BulkCopy(string tableName, Dictionary<string, (Type, List<DataInfos>)> list, int i, SqliteConnection cn)
        {
            using (var cmd = cn.CreateCommand())
            {
                if (this.Context?.CurrentConnectionConfig?.MoreSettings?.IsCorrectErrorSqlParameterName == true)
                {
                    var count = list.FirstOrDefault().Value.Item2?.Count;
                    if (count > 0)
                    {
                        for (int index = 0; index < count; index++)
                        {
                            var row = list.GetRows(index).ToDictionary(a => a.ColumnName, a => a.Value);
                            cmd.CommandText = this.Context.InsertableT(row).AS(tableName).ToSqlString().Replace(";SELECT LAST_INSERT_ROWID();", "");
                            TransformInsertCommand(cmd);
                            i += await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    var count = list.FirstOrDefault().Value.Item2?.Count;
                    if(count==1)
                    {
                        var row = list.GetRows(0).ToDictionary(a => a.ColumnName, a => a.Value);
                        cmd.CommandText = this.Context.InsertableT(row).AS(tableName).ToSqlString().Replace(";SELECT LAST_INSERT_ROWID();", "");
                        TransformInsertCommand(cmd);
                        i += await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                        return i;
                    }

                    if (count > 0)
                    {
                        var row = list.GetRows(0).ToDictionary(a => a.ColumnName, a => a.Value);
                        cmd.CommandText = this.Context.InsertableT(row).AS(tableName).ToSql().Key.Replace(";SELECT LAST_INSERT_ROWID();", "");
                    }

                    TransformInsertCommand(cmd);
                    if (count > 0)
                    {
                        for (int index = 0; index < count; index++)
                        {
                            var row = list.GetRows(index);
                            foreach (var item in row)
                            {

                                if (IsBoolTrue(item))
                                {
                                    cmd.Parameters.AddWithValue("@" + item.ColumnName, true);
                                }
                                else if (IsBoolFalse(item))
                                {
                                    cmd.Parameters.AddWithValue("@" + item.ColumnName, false);
                                }
                                else
                                {
                                    cmd.Parameters.AddWithValue("@" + item.ColumnName, item.Value);
                                }
                            }


                            i += await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                            cmd.Parameters.Clear();
                        }
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
        private async Task<int> _BulkUpdate(string tableName, Dictionary<string, (Type, List<DataInfos>)> list, int i, string[] whereColumns, string[] updateColumns, SqliteConnection cn)
        {
            using (var cmd = cn.CreateCommand())
            {
                if (this.Context?.CurrentConnectionConfig?.MoreSettings?.IsCorrectErrorSqlParameterName == true)
                {
                    var count = list.FirstOrDefault().Value.Item2?.Count;
                    if (count > 0)
                    {
                        for (int index = 0; index < count; index++)
                        {
                            var row = list.GetRows(index).ToDictionary(a => a.ColumnName, a => a.Value);
                            cmd.CommandText = this.Context.UpdateableT(row)
                             .WhereColumns(whereColumns)
                             .UpdateColumns(updateColumns)
                             .AS(tableName).ToSqlString();
                            i += await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                    }

                }
                else
                {
                    var count = list.FirstOrDefault().Value.Item2?.Count;

                    if (count == 1)
                    {
                        var row = list.GetRows(0).ToDictionary(a => a.ColumnName, a => a.Value);
                        cmd.CommandText = this.Context.UpdateableT(row)
                         .WhereColumns(whereColumns)
                         .UpdateColumns(updateColumns)
                         .AS(tableName).ToSqlString();
                        i += await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                        return i;
                    }

                    if (count > 0)
                    {
                        var row = list.GetRows(0).ToDictionary(a => a.ColumnName, a => a.Value);
                        cmd.CommandText = this.Context.UpdateableT(row)
                        .WhereColumns(whereColumns)
                        .UpdateColumns(updateColumns)
                        .AS(tableName).ToSql().Key;
                    }

                    if (count > 0)
                    {
                        for (int index = 0; index < count; index++)
                        {
                            var row = list.GetRows(index);
                            foreach (var item in row)
                            {

                                if (IsBoolTrue(item))
                                {
                                    cmd.Parameters.AddWithValue("@" + item.ColumnName, true);
                                }
                                else if (IsBoolFalse(item))
                                {
                                    cmd.Parameters.AddWithValue("@" + item.ColumnName, false);
                                }
                                else
                                {
                                    cmd.Parameters.AddWithValue("@" + item.ColumnName, item.Value);
                                }
                            }


                            i += await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                            cmd.Parameters.Clear();
                        }
                    }
                }
            }
            return i;
        }

        private static bool IsBoolFalse(DataRow dataRow, DataColumn item)
        {
            return dataRow[item.ColumnName] != null && dataRow[item.ColumnName] is string && dataRow[item.ColumnName].ToString() == ("isSqliteCore_False");
        }
        private static bool IsBoolFalse(DataInfos dataRow)
        {
            return dataRow.Value != null && dataRow.Value is string str && str == ("isSqliteCore_False");
        }
        private static bool IsBoolTrue(DataRow dataRow, DataColumn item)
        {
            return dataRow[item.ColumnName] != null && dataRow[item.ColumnName] is string && dataRow[item.ColumnName].ToString() == ("isSqliteCore_True");
        }
        private static bool IsBoolTrue(DataInfos dataRow)
        {
            return dataRow.Value != null && dataRow.Value is string str && str == ("isSqliteCore_True");
        }
        private static void Open(SqliteConnection cn)
        {
            if (cn.State != ConnectionState.Open)
                cn.Open();
        }

        public async Task<int> UpdateByTempAsync(string tableName, string tempName, string[] updateColumns, string[] whereColumns)
        {
            var dt = UpdateDataTable;
            if (dt != null)
            {

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

            else
            {

                if (UpdateDataInfos.FirstOrDefault().Value.Item2 == null || UpdateDataInfos.FirstOrDefault().Value.Item2?.Count == 0)
                {
                    return 0;
                }
                int result = 0;
                var cn = this.Context.Ado.Connection as SqliteConnection;
                Open(cn);
                if (this.Context.Ado.Transaction == null)
                {
#pragma warning disable CA2007 // 考虑对等待的任务调用 ConfigureAwait
                    await using (var transaction = await cn.BeginTransactionAsync().ConfigureAwait(false))
                    {
                        result = await _BulkUpdate(tableName, UpdateDataInfos, result, whereColumns, updateColumns, cn).ConfigureAwait(false);
                        await transaction.CommitAsync().ConfigureAwait(false);
                    }
#pragma warning restore CA2007 // 考虑对等待的任务调用 ConfigureAwait
                }
                else
                {
                    result = await _BulkUpdate(tableName, UpdateDataInfos, result, whereColumns, updateColumns, cn).ConfigureAwait(false);
                }
                return result;
            }
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

        public async Task<int> Merge<T>(string tableName, IEnumerable<DataInfos> list, EntityInfo entityInfo, string[] whereColumns, string[] updateColumns, IEnumerable<T> datas) where T : class, new()
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
