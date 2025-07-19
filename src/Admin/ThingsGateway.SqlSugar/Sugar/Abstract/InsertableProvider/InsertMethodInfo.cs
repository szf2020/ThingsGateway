using System.Reflection;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 插入方法信息类
    /// </summary>
    public class InsertMethodInfo
    {
        /// <summary>
        /// SqlSugar提供者上下文
        /// </summary>
        internal SqlSugarProvider Context { get; set; }
        /// <summary>
        /// 方法信息
        /// </summary>
        internal MethodInfo MethodInfo { get; set; }
        /// <summary>
        /// 对象值
        /// </summary>
        internal object objectValue { get; set; }
        /// <summary>
        /// 分页大小
        /// </summary>
        internal int pageSize { get; set; }

        /// <summary>
        /// 执行插入命令
        /// </summary>
        public int ExecuteCommand()
        {
            if (Context == null) return 0;
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            inertable = GetPageInsertable(inertable);
            var result = inertable.GetType().GetMethod("ExecuteCommand").Invoke(inertable, Array.Empty<object>());
            return (int)result;
        }

        /// <summary>
        /// 设置分页大小
        /// </summary>
        public InsertMethodInfo PageSize(int pageSize)
        {
            this.pageSize = pageSize;
            return this;
        }

        /// <summary>
        /// 异步执行插入命令
        /// </summary>
        public async Task<int> ExecuteCommandAsync()
        {
            if (Context == null) return 0;
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            inertable = GetPageInsertable(inertable);
            var result = inertable.GetType().GetMyMethod("ExecuteCommandAsync", 0).Invoke(inertable, Array.Empty<object>());
            return await ((Task<int>)result).ConfigureAwait(false);
        }

        /// <summary>
        /// 执行插入并返回标识
        /// </summary>
        public int ExecuteReturnIdentity()
        {
            if (Context == null) return 0;
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            var result = inertable.GetType().GetMethod("ExecuteReturnIdentity").Invoke(inertable, Array.Empty<object>());
            return (int)result;
        }
        public long ExecuteReturnBigIdentity()
        {
            if (Context == null) return 0;
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            var result = inertable.GetType().GetMethod("ExecuteReturnBigIdentity").Invoke(inertable, Array.Empty<object>());
            return (long)result;
        }

        /// <summary>
        /// 异步执行插入并返回标识
        /// </summary>
        public Task<int> ExecuteReturnIdentityAsync()
        {
            if (Context == null) return Task.FromResult(0);
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            var result = inertable.GetType().GetMyMethod("ExecuteReturnIdentityAsync", 0).Invoke(inertable, Array.Empty<object>());
            return Task.FromResult((int)result);
        }
        public Task<long> ExecuteReturnBigIdentityAsync()
        {
            if (Context == null) return Task.FromResult((long)0);
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            var result = inertable.GetType().GetMyMethod("ExecuteReturnBigIdentityAsync", 0).Invoke(inertable, Array.Empty<object>());
            return Task.FromResult((long)result);
        }
        /// <summary>
        /// 指定表名
        /// </summary>
        public CommonMethodInfo AS(string tableName)
        {
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            var newMethod = inertable.GetType().GetMyMethod("AS", 1, typeof(string));
            var result = newMethod.Invoke(inertable, new object[] { tableName });
            return new CommonMethodInfo()
            {
                Context = result
            };
        }

        /// <summary>
        /// 启用差异日志事件
        /// </summary>
        public CommonMethodInfo EnableDiffLogEvent(object businessData = null)
        {
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            var newMethod = inertable.GetType().GetMyMethod("EnableDiffLogEvent", 1, typeof(object));
            var result = newMethod.Invoke(inertable, new object[] { businessData });
            return new CommonMethodInfo()
            {
                Context = result
            };
        }

        /// <summary>
        /// 忽略指定列
        /// </summary>
        public CommonMethodInfo IgnoreColumns(params string[] ignoreColumns)
        {
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            var newMethod = inertable.GetType().GetMyMethod("IgnoreColumns", 1, typeof(string[]));
            var result = newMethod.Invoke(inertable, new object[] { ignoreColumns });
            return new CommonMethodInfo()
            {
                Context = result
            };
        }

        /// <summary>
        /// 忽略空列
        /// </summary>
        public CommonMethodInfo IgnoreColumns(bool ignoreNullColumn)
        {
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            var newMethod = inertable.GetType().GetMyMethod("IgnoreColumns", 2, typeof(bool), typeof(bool));
            var result = newMethod.Invoke(inertable, new object[] { ignoreNullColumn, true });
            return new CommonMethodInfo()
            {
                Context = result
            };
        }

        /// <summary>
        /// 分表操作
        /// </summary>
        public SplitMethodInfo SplitTable()
        {
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            var newMethod = inertable.GetType().GetMyMethod("SplitTable", 0);
            var result = newMethod.Invoke(inertable, Array.Empty<object>());
            return new SplitMethodInfo()
            {
                Context = result
            };
        }

        /// <summary>
        /// 执行插入并返回雪花ID
        /// </summary>
        public long ExecuteReturnSnowflakeId()
        {
            if (Context == null) return 0;
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            var result = inertable.GetType().GetMethod("ExecuteReturnSnowflakeId").Invoke(inertable, Array.Empty<object>());
            return (long)result;
        }

        /// <summary>
        /// 异步执行插入并返回雪花ID
        /// </summary>
        public async Task<long> ExecuteReturnSnowflakeIdAsync()
        {
            if (Context == null) return 0;
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            var result = inertable.GetType().GetMyMethod("ExecuteReturnSnowflakeIdAsync", 0).Invoke(inertable, Array.Empty<object>());
            return await ((Task<long>)result).ConfigureAwait(false);
        }

        /// <summary>
        /// 获取分页可插入对象
        /// </summary>
        private object GetPageInsertable(object inertable)
        {
            if (pageSize > 0)
            {
                inertable = inertable.GetType().GetMethod("PageSize").Invoke(inertable, new object[] { pageSize });
            }
            return inertable;
        }
    }
}