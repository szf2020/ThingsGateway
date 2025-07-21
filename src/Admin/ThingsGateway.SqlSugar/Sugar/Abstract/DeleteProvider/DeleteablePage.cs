namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 可分页删除操作类
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public class DeleteablePage<T> where T : class, new()
    {
        /// <summary>
        /// 要删除的数据列表
        /// </summary>
        public IReadOnlyList<T> DataList { get; set; }
        /// <summary>
        /// SqlSugar客户端实例
        /// </summary>
        public ISqlSugarClient Context { get; set; }
        /// <summary>
        /// 每页大小
        /// </summary>
        public int PageSize { get; internal set; }
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; internal set; }
        /// <summary>
        /// 是否启用差异日志事件
        /// </summary>
        public bool IsEnableDiffLogEvent { get; internal set; }
        /// <summary>
        /// 差异日志模型
        /// </summary>
        public DiffLogModel DiffModel { get; internal set; }
        /// <summary>
        /// 更新列集合
        /// </summary>
        public List<string> UpdateColumns { get; internal set; }

        /// <summary>
        /// 执行删除命令
        /// </summary>
        /// <returns>受影响的行数</returns>
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
                this.Context.Utilities.PageEach(DataList, PageSize, pageItem => result += this.Context.Deleteable(pageItem).AS(TableName).EnableDiffLogEventIF(IsEnableDiffLogEvent, DiffModel).ExecuteCommand());
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

        /// <summary>
        /// 异步执行删除命令
        /// </summary>
        /// <returns>受影响的行数</returns>
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
                await Context.Utilities.PageEachAsync(DataList, PageSize, async pageItem => result += await Context.Deleteable(pageItem).AS(TableName).EnableDiffLogEventIF(IsEnableDiffLogEvent, DiffModel).ExecuteCommandAsync().ConfigureAwait(false)).ConfigureAwait(false);
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