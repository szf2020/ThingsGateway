namespace SqlSugar
{
    public class DeleteablePage<T> where T : class, new()
    {
        public T[] DataList { get; set; }
        public ISqlSugarClient Context { get; set; }
        public int PageSize { get; internal set; }
        public string TableName { get; internal set; }
        public bool IsEnableDiffLogEvent { get; internal set; }
        public DiffLogModel DiffModel { get; internal set; }
        public List<string> UpdateColumns { get; internal set; }
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
                    result += this.Context.Deleteable(pageItem).AS(TableName).EnableDiffLogEventIF(IsEnableDiffLogEvent, DiffModel).ExecuteCommand();
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
                    result += await Context.Deleteable(pageItem).AS(TableName).EnableDiffLogEventIF(IsEnableDiffLogEvent, DiffModel).ExecuteCommandAsync().ConfigureAwait(false);
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
    }
}
