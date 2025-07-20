namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 分表插入类
    /// </summary>
    public class SplitInsertable<T> where T : class, new()
    {
        /// <summary>
        /// 分表锁对象
        /// </summary>
        private static readonly object SplitLockObj = new object();
        /// <summary>
        /// SqlSugar提供者上下文
        /// </summary>
        public SqlSugarProvider Context;
        /// <summary>
        /// 分表上下文
        /// </summary>
        internal SplitTableContext Helper;
        /// <summary>
        /// 实体信息
        /// </summary>
        public EntityInfo EntityInfo;
        /// <summary>
        /// 分表类型
        /// </summary>
        public SplitType SplitType;
        /// <summary>
        /// 可插入对象
        /// </summary>
        internal IInsertable<T> Inserable { get; set; }
        /// <summary>
        /// 表名集合
        /// </summary>
        internal List<KeyValuePair<string, object>> TableNames { get; set; }
        /// <summary>
        /// MySQL忽略标识
        /// </summary>
        internal bool MySqlIgnore { get; set; }

        /// <summary>
        /// 执行插入命令
        /// </summary>
        public int ExecuteCommand()
        {
            if (this.Context.Ado.Transaction == null)
            {
                try
                {
                    this.Context.Ado.BeginTran();
                    var result = _ExecuteCommand();
                    this.Context.Ado.CommitTran();
                    return result;
                }
                catch (Exception)
                {
                    this.Context.Ado.RollbackTran();
                    throw;
                }
            }
            else
            {
                return _ExecuteCommand();
            }
        }

        /// <summary>
        /// 异步执行插入命令
        /// </summary>
        public async Task<int> ExecuteCommandAsync()
        {
            if (this.Context.Ado.Transaction == null)
            {
                try
                {
                    this.Context.Ado.BeginTran();
                    var result = await _ExecuteCommandAsync().ConfigureAwait(false);
                    this.Context.Ado.CommitTran();
                    return result;
                }
                catch (Exception)
                {
                    this.Context.Ado.RollbackTran();
                    throw;
                }
            }
            else
            {
                return await _ExecuteCommandAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 执行插入并返回雪花ID列表
        /// </summary>
        public List<long> ExecuteReturnSnowflakeIdList()
        {
            if (this.Context.Ado.Transaction == null)
            {
                try
                {
                    this.Context.Ado.BeginTran();
                    var result = _ExecuteReturnSnowflakeIdList();
                    this.Context.Ado.CommitTran();
                    return result;
                }
                catch (Exception)
                {
                    this.Context.Ado.RollbackTran();
                    throw;
                }
            }
            else
            {
                return _ExecuteReturnSnowflakeIdList();
            }
        }

        /// <summary>
        /// 异步执行插入并返回雪花ID列表
        /// </summary>
        public async Task<List<long>> ExecuteReturnSnowflakeIdListAsync()
        {
            if (this.Context.Ado.Transaction == null)
            {
                try
                {
                    this.Context.Ado.BeginTran();
                    var result = await _ExecuteReturnSnowflakeIdListAsync().ConfigureAwait(false);
                    this.Context.Ado.CommitTran();
                    return result;
                }
                catch (Exception)
                {
                    this.Context.Ado.RollbackTran();
                    throw;
                }
            }
            else
            {
                return await _ExecuteReturnSnowflakeIdListAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 执行插入并返回雪花ID
        /// </summary>
        public long ExecuteReturnSnowflakeId()
        {
            return ExecuteReturnSnowflakeIdList().FirstOrDefault();
        }

        /// <summary>
        /// 异步执行插入并返回雪花ID
        /// </summary>
        public async Task<long> ExecuteReturnSnowflakeIdAsync()
        {
            var list = await ExecuteReturnSnowflakeIdListAsync().ConfigureAwait(false);
            return list.FirstOrDefault();
        }

        /// <summary>
        /// 实际执行插入命令
        /// </summary>
        internal int _ExecuteCommand()
        {
            CreateTable();
            var result = 0;
            var groups = TableNames.GroupBy(it => it.Key).ToList();
            var parent = ((InsertableProvider<T>)Inserable);
            var names = parent.InsertBuilder.DbColumnInfoList.GroupBy(it => it.DbColumnName).Select(i => i.Key).ToList();
            foreach (var item in groups)
            {
                var list = item.Select(it => it.Value as T).ToList();
                var dataEvent = this.Context.CurrentConnectionConfig.AopEvents?.DataExecuting;
                this.Context.Aop.DataExecuting = null;
                var groupInserable = (InsertableProvider<T>)this.Context.Insertable<T>(list);
                this.Context.Aop.DataExecuting = dataEvent;
                groupInserable.InsertBuilder.TableWithString = parent.InsertBuilder.TableWithString;
                groupInserable.RemoveCacheFunc = parent.RemoveCacheFunc;
                groupInserable.DiffModel = parent.DiffModel;
                groupInserable.IsEnableDiffLogEvent = parent.IsEnableDiffLogEvent;
                groupInserable.InsertBuilder.IsNoInsertNull = parent.InsertBuilder.IsNoInsertNull;
                groupInserable.IsOffIdentity = parent.IsOffIdentity;
                groupInserable.InsertBuilder.MySqlIgnore = this.MySqlIgnore;
                result += groupInserable.AS(item.Key).InsertColumns(names).ExecuteCommand();
            }
            return result;
        }

        /// <summary>
        /// 异步实际执行插入命令
        /// </summary>
        internal async Task<int> _ExecuteCommandAsync()
        {
            CreateTable();
            var result = 0;
            var groups = TableNames.GroupBy(it => it.Key).ToList();
            var parent = ((InsertableProvider<T>)Inserable);
            var names = parent.InsertBuilder.DbColumnInfoList.GroupBy(it => it.DbColumnName).Select(i => i.Key).ToList();
            foreach (var item in groups)
            {
                var list = item.Select(it => it.Value as T).ToList();
                var dataEvent = this.Context.CurrentConnectionConfig.AopEvents?.DataExecuting;
                this.Context.Aop.DataExecuting = null;
                var groupInserable = (InsertableProvider<T>)this.Context.Insertable<T>(list);
                this.Context.Aop.DataExecuting = dataEvent;
                groupInserable.InsertBuilder.TableWithString = parent.InsertBuilder.TableWithString;
                groupInserable.RemoveCacheFunc = parent.RemoveCacheFunc;
                groupInserable.DiffModel = parent.DiffModel;
                groupInserable.IsEnableDiffLogEvent = parent.IsEnableDiffLogEvent;
                groupInserable.InsertBuilder.IsNoInsertNull = parent.InsertBuilder.IsNoInsertNull;
                groupInserable.IsOffIdentity = parent.IsOffIdentity;
                groupInserable.InsertBuilder.MySqlIgnore = this.MySqlIgnore;
                result += await groupInserable.AS(item.Key).InsertColumns(names).ExecuteCommandAsync().ConfigureAwait(false);
            }
            return result;
        }

        /// <summary>
        /// 实际执行插入并返回雪花ID列表
        /// </summary>
        internal List<long> _ExecuteReturnSnowflakeIdList()
        {
            CreateTable();
            var result = new List<long>();
            var groups = TableNames.GroupBy(it => it.Key).ToList();
            var parent = ((InsertableProvider<T>)Inserable);
            var names = parent.InsertBuilder.DbColumnInfoList.GroupBy(it => it.DbColumnName).Select(i => i.Key).ToList();
            foreach (var item in groups)
            {
                var list = item.Select(it => it.Value as T).ToList();
                var groupInserable = (InsertableProvider<T>)this.Context.Insertable<T>(list);
                groupInserable.InsertBuilder.TableWithString = parent.InsertBuilder.TableWithString;
                groupInserable.RemoveCacheFunc = parent.RemoveCacheFunc;
                groupInserable.DiffModel = parent.DiffModel;
                groupInserable.IsEnableDiffLogEvent = parent.IsEnableDiffLogEvent;
                groupInserable.InsertBuilder.IsNoInsertNull = parent.InsertBuilder.IsNoInsertNull;
                groupInserable.IsOffIdentity = parent.IsOffIdentity;
                groupInserable.InsertBuilder.MySqlIgnore = this.MySqlIgnore;
                var idList = groupInserable.AS(item.Key).InsertColumns(names).ExecuteReturnSnowflakeIdList();
                result.AddRange(idList);
            }
            return result;
        }

        /// <summary>
        /// 异步实际执行插入并返回雪花ID列表
        /// </summary>
        internal async Task<List<long>> _ExecuteReturnSnowflakeIdListAsync()
        {
            CreateTable();
            var result = new List<long>();
            var groups = TableNames.GroupBy(it => it.Key).ToList();
            var parent = ((InsertableProvider<T>)Inserable);
            var names = parent.InsertBuilder.DbColumnInfoList.GroupBy(it => it.DbColumnName).Select(i => i.Key).ToList();
            foreach (var item in groups)
            {
                var list = item.Select(it => it.Value as T).ToList();
                var groupInserable = (InsertableProvider<T>)this.Context.Insertable<T>(list);
                groupInserable.InsertBuilder.TableWithString = parent.InsertBuilder.TableWithString;
                groupInserable.RemoveCacheFunc = parent.RemoveCacheFunc;
                groupInserable.DiffModel = parent.DiffModel;
                groupInserable.IsEnableDiffLogEvent = parent.IsEnableDiffLogEvent;
                groupInserable.InsertBuilder.IsNoInsertNull = parent.InsertBuilder.IsNoInsertNull;
                groupInserable.IsOffIdentity = parent.IsOffIdentity;
                groupInserable.InsertBuilder.MySqlIgnore = this.MySqlIgnore;
                var idList = await groupInserable.AS(item.Key).InsertColumns(names).ExecuteReturnSnowflakeIdListAsync().ConfigureAwait(false);
                result.AddRange(idList);
            }
            return result;
        }

        /// <summary>
        /// 创建分表
        /// </summary>
        private void CreateTable()
        {
            var isLog = this.Context.Ado.IsEnableLogEvent;
            this.Context.Ado.IsEnableLogEvent = false;
            foreach (var item in TableNames.GroupBy(it => it.Key).Select(it => it).ToDictionary(it => it.Key, it => it.First().Value))
            {
                var newDb = this.Context.CopyNew();
                newDb.CurrentConnectionConfig.IsAutoCloseConnection = true;
                if (!newDb.DbMaintenance.IsAnyTable(item.Key, false))
                {
                    lock (SplitLockObj)
                    {
                        var newDb2 = this.Context.CopyNew();
                        newDb2.CurrentConnectionConfig.IsAutoCloseConnection = true;
                        if (!newDb2.DbMaintenance.IsAnyTable(item.Key, false))
                        {
                            if (item.Value != null)
                            {
                                this.Context.MappingTables.Add(EntityInfo.EntityName, item.Key);
                                this.Context.CodeFirst.InitTables<T>();
                            }
                        }
                    }
                }
            }
            this.Context.Ado.IsEnableLogEvent = isLog;
            this.Context.MappingTables.Add(EntityInfo.EntityName, EntityInfo.DbTableName);
        }
    }
}