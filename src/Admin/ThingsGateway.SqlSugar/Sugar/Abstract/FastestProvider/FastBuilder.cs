using System.Data;

namespace ThingsGateway.SqlSugar
{
    public class FastBuilder
    {
        public EntityInfo FastEntityInfo { get; set; }
        public virtual bool IsActionUpdateColumns { get; set; }
        public virtual DbFastestProperties DbFastestProperties { get; set; }
        public SqlSugarProvider Context { get; set; }
        public virtual string CharacterSet { get; set; }
        public virtual string UpdateSql { get; set; } = @"UPDATE TM
                                                    SET  {0}
                                                    FROM {1} TM
                                                    INNER JOIN {2} TE ON {3} ";


        public virtual void CloseDb()
        {
            if (this.Context.CurrentConnectionConfig.IsAutoCloseConnection && this.Context.Ado.Transaction == null)
            {
                this.Context.Ado.Connection.Close();
            }
        }

        public virtual async Task<int> UpdateByTempAsync(string tableName, string tempName, string[] updateColumns, string[] whereColumns)
        {
            var sqlbuilder = this.Context.Queryable<object>().SqlBuilder;
            Check.ArgumentNullException(updateColumns.Length == 0, "update columns count is 0");
            Check.ArgumentNullException(whereColumns.Length == 0, "where columns count is 0");
            var sets = string.Join(",", updateColumns.Select(it => $"TM.{sqlbuilder.GetTranslationColumnName(it)}=TE.{sqlbuilder.GetTranslationColumnName(it)}"));
            var wheres = string.Join(" AND ", whereColumns.Select(it => $"TM.{sqlbuilder.GetTranslationColumnName(it)}=TE.{sqlbuilder.GetTranslationColumnName(it)}"));
            string sql = string.Format(UpdateSql, sets, tableName, tempName, wheres);
            return await Context.Ado.ExecuteCommandAsync(sql).ConfigureAwait(false);
        }

        public virtual async Task CreateTempAsync<T>(DataTable dt) where T : class, new()
        {
            var sqlbuilder = this.Context.Queryable<object>().SqlBuilder;
            await Context.UnionAll(
                Context.Queryable<T>().Filter(null, true).Select(string.Join(",", dt.Columns.Cast<DataColumn>().Select(it => sqlbuilder.GetTranslationColumnName(it.ColumnName)))).Where(it => false).AS(dt.TableName),
                Context.Queryable<T>().Filter(null, true).Select(string.Join(",", dt.Columns.Cast<DataColumn>().Select(it => sqlbuilder.GetTranslationColumnName(it.ColumnName)))).Where(it => false).AS(dt.TableName)).Select("top 1 * into #temp").ToListAsync().ConfigureAwait(false);
            dt.TableName = "#temp";
        }

        public async virtual Task<int> Merge<T>(string tableName, DataTable dt, EntityInfo entityInfo, string[] whereColumns, string[] updateColumns, List<T> datas) where T : class, new()
        {
            var result = 0;
            var pageSize = 2000;
            if (dt.Columns.Count > 100)
            {
                pageSize = 100;
            }
            else if (dt.Columns.Count > 50)
            {
                pageSize = 300;
            }
            else if (dt.Columns.Count > 30)
            {
                pageSize = 500;
            }
            await Context.Utilities.PageEachAsync(datas, pageSize, async pageItems =>
            {
                var x = await Context.Storageable(pageItems).As(tableName).WhereColumns(whereColumns).ToStorageAsync().ConfigureAwait(false);
                result += await x.BulkCopyAsync().ConfigureAwait(false);
                result += await x.BulkUpdateAsync(updateColumns).ConfigureAwait(false);
                return result;
            }).ConfigureAwait(false);
            return result;
        }
    }
}
