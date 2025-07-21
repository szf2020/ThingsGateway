namespace ThingsGateway.SqlSugar
{
    public class UpdateableFilter<T> where T : class, new()
    {
        public IReadOnlyList<T> DataList { get; set; }
        public SqlSugarProvider Context { get; set; }
        public int PageSize { get; internal set; }
        public string TableName { get; internal set; }
        public bool IsEnableDiffLogEvent { get; internal set; }
        public DiffLogModel DiffModel { get; internal set; }
        public List<string> UpdateColumns { get; internal set; }
        public int ExecuteCommand()
        {
            if (DataList.Count == 1 && DataList[0] == null)
            {
                return 0;
            }
            PageSize = 1;
            var result = 0;
            var isNoTran = this.Context.Ado.IsNoTran();
            try
            {
                if (isNoTran)
                {
                    this.Context.Ado.BeginTran();
                }
                this.Context.Utilities.PageEach(DataList, PageSize, pageItem => result += SetFilterSql(this.Context.UpdateableT(pageItem[0]).AS(TableName).EnableDiffLogEventIF(IsEnableDiffLogEvent, DiffModel).UpdateColumns(UpdateColumns)).ExecuteCommand());
                if (isNoTran)
                {
                    this.Context.Ado.CommitTran();
                }
            }
            catch (Exception)
            {
                if (isNoTran)
                {
                    this.Context.Ado.RollbackTran();
                }
                throw;
            }
            return result;
        }

        public async Task<int> ExecuteCommandAsync()
        {
            if (DataList.Count == 1 && DataList[0] == null)
            {
                return 0;
            }
            PageSize = 1;
            var result = 0;
            var isNoTran = this.Context.Ado.IsNoTran();
            try
            {
                if (isNoTran)
                {
                    await Context.Ado.BeginTranAsync().ConfigureAwait(false);
                }
                await Context.Utilities.PageEachAsync(DataList, PageSize, async pageItem => result += await SetFilterSql(Context.UpdateableT(pageItem[0]).AS(TableName).EnableDiffLogEventIF(IsEnableDiffLogEvent, DiffModel).UpdateColumns(UpdateColumns)).ExecuteCommandAsync().ConfigureAwait(false)).ConfigureAwait(false);
                if (isNoTran)
                {
                    await Context.Ado.CommitTranAsync().ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                if (isNoTran)
                {
                    await Context.Ado.RollbackTranAsync().ConfigureAwait(false);
                }
                throw;
            }
            return result;
        }

        private IUpdateable<T> SetFilterSql(IUpdateable<T> updateable)
        {
            var queryable = this.Context.Queryable<T>();
            queryable.QueryBuilder.LambdaExpressions.ParameterIndex = 10000;
            var sqlobj = queryable.ToSql();
            var sql = UtilMethods.RemoveBeforeFirstWhere(sqlobj.Key);
            if (sql != sqlobj.Key)
            {
                updateable.UpdateBuilder.AppendWhere = sql;
            }
            if (sqlobj.Value != null)
            {
                updateable.UpdateBuilder.Parameters.AddRange(sqlobj.Value);
            }
            return updateable;
        }
    }
}
