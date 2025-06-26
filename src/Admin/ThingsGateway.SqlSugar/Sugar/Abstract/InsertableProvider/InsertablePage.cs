namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 可分页插入数据类
    /// </summary>
    public class InsertablePage<T> where T : class, new()
    {
        /// <summary>
        /// 每页大小
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// SqlSugar上下文
        /// </summary>
        public SqlSugarProvider Context { get; set; }
        /// <summary>
        /// 数据列表
        /// </summary>
        public T[] DataList { get; set; }
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; internal set; }
        /// <summary>
        /// 插入列集合
        /// </summary>
        public List<string> InsertColumns { get; internal set; }
        /// <summary>
        /// 是否启用差异日志事件
        /// </summary>
        public bool IsEnableDiffLogEvent { get; internal set; }
        /// <summary>
        /// 差异日志模型
        /// </summary>
        public DiffLogModel DiffModel { get; internal set; }
        /// <summary>
        /// 是否关闭标识列
        /// </summary>
        public bool IsOffIdentity { get; internal set; }
        /// <summary>
        /// 插入列是否为空
        /// </summary>
        public bool IsInsertColumnsNull { get; internal set; }
        /// <summary>
        /// 是否MySQL忽略
        /// </summary>
        public bool IsMySqlIgnore { get; internal set; }

        /// <summary>
        /// 执行插入命令
        /// </summary>
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

        /// <summary>
        /// 异步执行插入命令
        /// </summary>
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

        /// <summary>
        /// 执行并返回雪花ID列表
        /// </summary>
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

        /// <summary>
        /// 异步执行并返回雪花ID列表
        /// </summary>
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

        /// <summary>
        /// 设置忽略空列
        /// </summary>
        public InsertablePage<T> IgnoreColumnsNull(bool isIgnoreNull = true)
        {
            this.PageSize = 1;
            this.IsInsertColumnsNull = isIgnoreNull;
            return this;
        }
    }
}