namespace ThingsGateway.SqlSugar
{
    public class InsertablePage<T> where T : class, new()
    {
        public int PageSize { get; set; }
        public SqlSugarProvider Context { get; set; }
        public T[] DataList { get; set; }
        public string TableName { get; internal set; }
        public List<string> InsertColumns { get; internal set; }
        public bool IsEnableDiffLogEvent { get; internal set; }
        public DiffLogModel DiffModel { get; internal set; }
        public bool IsOffIdentity { get; internal set; }
        public bool IsInsertColumnsNull { get; internal set; }
        public bool IsMySqlIgnore { get; internal set; }
        public int ExecuteCommand()
        {
            if (DataList.Length == 1 && DataList.First() == null)
            {
                return 0;
            }
            if (PageSize == 0) { PageSize = 1000; }
            var result = 0;
            var isNoTran = this.Context.Ado.IsNoTran();
            try
            {
                if (isNoTran)
                {
                    this.Context.Ado.BeginTran();
                }
                this.Context.Utilities.PageEach(DataList, PageSize, pageItem =>
                {
                    result += this.Context.Insertable(pageItem).AS(TableName).MySqlIgnore(IsMySqlIgnore).IgnoreColumnsNull(this.IsInsertColumnsNull).OffIdentity(IsOffIdentity).EnableDiffLogEventIF(IsEnableDiffLogEvent, DiffModel).InsertColumns(InsertColumns.ToArray()).ExecuteCommand();
                });
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
            if (DataList.Length == 1 && DataList.First() == null)
            {
                return 0;
            }
            if (PageSize == 0) { PageSize = 1000; }
            var result = 0;
            var isNoTran = this.Context.Ado.IsNoTran();
            try
            {
                if (isNoTran)
                {
                    await Context.Ado.BeginTranAsync().ConfigureAwait(false);
                }
                await Context.Utilities.PageEachAsync(DataList, PageSize, async pageItem =>
                {
                    result += await Context.Insertable(pageItem).AS(TableName).IgnoreColumnsNull(IsInsertColumnsNull).OffIdentity(IsOffIdentity).EnableDiffLogEventIF(IsEnableDiffLogEvent, DiffModel).InsertColumns(InsertColumns.ToArray()).ExecuteCommandAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
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

        public List<long> ExecuteReturnSnowflakeIdList()
        {
            if (DataList.Length == 1 && DataList.First() == null)
            {
                return new List<long>();
            }
            if (PageSize == 0) { PageSize = 1000; }
            var result = new List<long>();
            var isNoTran = this.Context.Ado.IsNoTran();
            try
            {
                if (isNoTran)
                {
                    this.Context.Ado.BeginTran();
                }
                this.Context.Utilities.PageEach(DataList, PageSize, pageItem =>
                {
                    result.AddRange(this.Context.Insertable(pageItem).AS(TableName).OffIdentity(IsOffIdentity).EnableDiffLogEventIF(IsEnableDiffLogEvent, DiffModel).InsertColumns(InsertColumns.ToArray()).ExecuteReturnSnowflakeIdList());
                });
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
        public async Task<List<long>> ExecuteReturnSnowflakeIdListAsync()
        {
            if (DataList.Length == 1 && DataList.First() == null)
            {
                return new List<long>();
            }
            if (PageSize == 0) { PageSize = 1000; }
            var result = new List<long>();
            var isNoTran = this.Context.Ado.IsNoTran();
            try
            {
                if (isNoTran)
                {
                    await Context.Ado.BeginTranAsync().ConfigureAwait(false);
                }
                await Context.Utilities.PageEachAsync(DataList, PageSize, async pageItem =>
                {
                    result.AddRange(await Context.Insertable(pageItem).AS(TableName).OffIdentity(IsOffIdentity).EnableDiffLogEventIF(IsEnableDiffLogEvent, DiffModel).InsertColumns(InsertColumns.ToArray()).ExecuteReturnSnowflakeIdListAsync().ConfigureAwait(false));
                }).ConfigureAwait(false);
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

        public InsertablePage<T> IgnoreColumnsNull(bool isIgnoreNull = true)
        {
            this.PageSize = 1;
            this.IsInsertColumnsNull = isIgnoreNull;
            return this;
        }
    }
}
