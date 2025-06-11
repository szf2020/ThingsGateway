using System.Reflection;

namespace ThingsGateway.SqlSugar
{
    public class InsertMethodInfo
    {
        internal SqlSugarProvider Context { get; set; }
        internal MethodInfo MethodInfo { get; set; }
        internal object objectValue { get; set; }
        internal int pageSize { get; set; }
        public int ExecuteCommand()
        {
            if (Context == null) return 0;
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            inertable = GetPageInsertable(inertable);
            var result = inertable.GetType().GetMethod("ExecuteCommand").Invoke(inertable, Array.Empty<object>());
            return (int)result;
        }

        public InsertMethodInfo PageSize(int pageSize)
        {
            this.pageSize = pageSize;
            return this;
        }
        public async Task<int> ExecuteCommandAsync()
        {
            if (Context == null) return 0;
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            inertable = GetPageInsertable(inertable);
            var result = inertable.GetType().GetMyMethod("ExecuteCommandAsync", 0).Invoke(inertable, Array.Empty<object>());
            return await ((Task<int>)result).ConfigureAwait(false);
        }
        public int ExecuteReturnIdentity()
        {
            if (Context == null) return 0;
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            var result = inertable.GetType().GetMethod("ExecuteReturnIdentity").Invoke(inertable, Array.Empty<object>());
            return (int)result;
        }
        public async Task<int> ExecuteReturnIdentityAsync()
        {
            if (Context == null) return 0;
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            var result = inertable.GetType().GetMyMethod("ExecuteReturnIdentityAsync", 0).Invoke(inertable, Array.Empty<object>());
            return await ((Task<int>)result).ConfigureAwait(false);
        }

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

        public long ExecuteReturnSnowflakeId()
        {
            if (Context == null) return 0;
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            var result = inertable.GetType().GetMethod("ExecuteReturnSnowflakeId").Invoke(inertable, Array.Empty<object>());
            return (long)result;
        }
        public async Task<long> ExecuteReturnSnowflakeIdAsync()
        {
            if (Context == null) return 0;
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            var result = inertable.GetType().GetMyMethod("ExecuteReturnSnowflakeIdAsync", 0).Invoke(inertable, Array.Empty<object>());
            return await ((Task<long>)result).ConfigureAwait(false);
        }

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
