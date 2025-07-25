using Dm;

using System.Data;

namespace ThingsGateway.SqlSugar
{
    public class DmFastBuilder : FastBuilder, IFastBuilder
    {
        public override bool IsActionUpdateColumns { get; set; } = true;
        public override DbFastestProperties DbFastestProperties { get; set; } = new DbFastestProperties()
        {
            HasOffsetTime = true,
            IsConvertDateTimeOffsetToDateTime = true
        };
        public async Task<int> ExecuteBulkCopyAsync(DataTable dt)
        {
            dt = UtilMethods.ConvertDateTimeOffsetToDateTime(dt);
            if (DbFastestProperties?.IsOffIdentity == true)
            {
                var isNoTran = this.Context.Ado.IsNoTran() && this.Context.CurrentConnectionConfig.IsAutoCloseConnection;
                try
                {
                    if (isNoTran)
                        await this.Context.Ado.BeginTranAsync().ConfigureAwait(false);

                    await this.Context.Ado.ExecuteCommandAsync($"SET IDENTITY_INSERT {dt.TableName} ON").ConfigureAwait(false);
                    var result = await _Execute(dt).ConfigureAwait(false);
                    await this.Context.Ado.ExecuteCommandAsync($"SET IDENTITY_INSERT {dt.TableName} OFF").ConfigureAwait(false);

                    if (isNoTran)
                        await this.Context.Ado.CommitTranAsync().ConfigureAwait(false);

                    return result;
                }
                catch (Exception)
                {
                    if (isNoTran)
                        await this.Context.Ado.CommitTranAsync().ConfigureAwait(false);
                    throw;
                }
            }
            else
            {
                return await _Execute(dt).ConfigureAwait(false);
            }
        }
        public override async Task CreateTempAsync<T>(DataTable dt)
        {
            var queryable = this.Context.Queryable<T>();
            var tableName = queryable.SqlBuilder.GetTranslationTableName(dt.TableName);
            var sqlBuilder = this.Context.Queryable<object>().SqlBuilder;
            var dts = dt.Columns.Cast<DataColumn>().Select(it => sqlBuilder.GetTranslationColumnName(it.ColumnName));
            dt.TableName = "temp" + SnowFlakeSingle.instance.getID();
            var sql = queryable.AS(tableName).Where(it => false).Select(string.Join(",", dts)).ToSql().Key;
            await Context.Ado.ExecuteCommandAsync($"CREATE  TABLE {dt.TableName}    as ( {sql} ) ").ConfigureAwait(false);
        }
        public override string UpdateSql { get; set; } = @"UPDATE  {1} TM    INNER JOIN {2} TE  ON {3} SET {0} ";

        private async Task<int> _Execute(DataTable dt)
        {
            DmBulkCopy bulkCopy = GetBulkCopyInstance();
            bulkCopy.DestinationTableName = dt.TableName;
            try
            {
                bulkCopy.WriteToServer(dt);
                await Task.Delay(0).ConfigureAwait(false);//No Support Async
            }
            catch (Exception)
            {
                CloseDb();
                throw;
            }
            CloseDb();
            return dt.Rows.Count;
        }

        public DmBulkCopy GetBulkCopyInstance()
        {
            DmBulkCopy copy;
            if (this.Context.Ado.Transaction == null)
            {
                copy = new DmBulkCopy((DmConnection)this.Context.Ado.Connection);
            }
            else
            {
                copy = new DmBulkCopy((DmConnection)this.Context.Ado.Connection, DmBulkCopyOptions.Default, (DmTransaction)this.Context.Ado.Transaction);
            }
            if (this.Context.Ado.Connection.State == ConnectionState.Closed)
            {
                this.Context.Ado.Connection.Open();
            }
            copy.BulkCopyTimeout = this.Context.Ado.CommandTimeOut;
            return copy;
        }
    }
}
