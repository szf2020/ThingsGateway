using System.Data;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 快速构建器类
    /// </summary>
    public class FastBuilder
    {
        /// <summary>
        /// 快速实体信息
        /// </summary>
        public EntityInfo FastEntityInfo { get; set; }

        /// <summary>
        /// 是否操作更新列
        /// </summary>
        public virtual bool IsActionUpdateColumns { get; set; }

        /// <summary>
        /// 快速操作属性配置
        /// </summary>
        public virtual DbFastestProperties DbFastestProperties { get; set; }

        /// <summary>
        /// SqlSugar上下文
        /// </summary>
        public SqlSugarProvider Context { get; set; }

        /// <summary>
        /// 字符集
        /// </summary>
        public virtual string CharacterSet { get; set; }

        /// <summary>
        /// 更新SQL模板
        /// </summary>
        public virtual string UpdateSql { get; set; } = @"UPDATE TM
                                                    SET  {0}
                                                    FROM {1} TM
                                                    INNER JOIN {2} TE ON {3} ";

        /// <summary>
        /// 关闭数据库连接
        /// </summary>
        public virtual void CloseDb()
        {
            if (this.Context.CurrentConnectionConfig.IsAutoCloseConnection && this.Context.Ado.Transaction == null)
            {
                this.Context.Ado.Connection.Close();
            }
        }

        /// <summary>
        /// 通过临时表异步更新数据
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="tempName">临时表名</param>
        /// <param name="updateColumns">更新列</param>
        /// <param name="whereColumns">条件列</param>
        /// <returns>影响行数</returns>
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

        /// <summary>
        /// 创建临时表(异步)
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="dt">数据表</param>
        public virtual async Task CreateTempAsync<T>(DataTable dt) where T : class, new()
        {
            var sqlbuilder = this.Context.Queryable<object>().SqlBuilder;
            await Context.UnionAll(
                Context.Queryable<T>().Filter(null, true).Select(string.Join(",", dt.Columns.Cast<DataColumn>().Select(it => sqlbuilder.GetTranslationColumnName(it.ColumnName)))).Where(it => false).AS(dt.TableName),
                Context.Queryable<T>().Filter(null, true).Select(string.Join(",", dt.Columns.Cast<DataColumn>().Select(it => sqlbuilder.GetTranslationColumnName(it.ColumnName)))).Where(it => false).AS(dt.TableName)).Select("top 1 * into #temp").ToListAsync().ConfigureAwait(false);
            dt.TableName = "#temp";
        }

        /// <summary>
        /// 合并数据(异步)
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="tableName">表名</param>
        /// <param name="dt">数据表</param>
        /// <param name="entityInfo">实体信息</param>
        /// <param name="whereColumns">条件列</param>
        /// <param name="updateColumns">更新列</param>
        /// <param name="datas">数据列表</param>
        /// <returns>影响行数</returns>
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