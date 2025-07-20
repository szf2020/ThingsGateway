namespace ThingsGateway.SqlSugar
{
    public class UpdateablePage<T> where T : class, new()
    {
        public IReadOnlyList<T> DataList { get; set; }
        public SqlSugarProvider Context { get; set; }
        public int PageSize { get; internal set; }
        public string TableName { get; internal set; }
        public bool IsEnableDiffLogEvent { get; internal set; }
        public DiffLogModel DiffModel { get; internal set; }
        public List<string> UpdateColumns { get; internal set; }
        public string[] WhereColumnList { get; internal set; }
        public Dictionary<string, ReSetValueBySqlExpListModel> ReSetValueBySqlExpList { get; internal set; }

        public UpdateableFilter<T> EnableQueryFilter()
        {
            return new UpdateableFilter<T>()
            {
                Context = Context,
                DataList = DataList,
                DiffModel = DiffModel,
                IsEnableDiffLogEvent = IsEnableDiffLogEvent,
                PageSize = PageSize,
                TableName = TableName,
                UpdateColumns = UpdateColumns
            };
        }
        public int ExecuteCommand()
        {
            if (DataList.Count == 1 && DataList[0] == null)
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
                    var updateable = this.Context.Updateable(pageItem).AS(TableName);
                    updateable.UpdateBuilder.ReSetValueBySqlExpList = this.ReSetValueBySqlExpList;
                    result += updateable.WhereColumns(WhereColumnList).EnableDiffLogEventIF(IsEnableDiffLogEvent, DiffModel).UpdateColumns(UpdateColumns.ToArray()).ExecuteCommand();
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
            if (DataList.Count == 1 && DataList[0] == null)
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
                    var updateable = Context.Updateable(pageItem);
                    updateable.UpdateBuilder.ReSetValueBySqlExpList = ReSetValueBySqlExpList;
                    result += await updateable.AS(TableName).WhereColumns(WhereColumnList).EnableDiffLogEventIF(IsEnableDiffLogEvent, DiffModel).UpdateColumns(UpdateColumns.ToArray()).ExecuteCommandAsync().ConfigureAwait(false);
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
