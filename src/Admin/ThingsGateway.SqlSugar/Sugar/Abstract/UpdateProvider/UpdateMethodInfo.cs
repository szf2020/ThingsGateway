using System.Reflection;

namespace ThingsGateway.SqlSugar
{
    public class UpdateMethodInfo
    {
        internal SqlSugarProvider Context { get; set; }
        internal MethodInfo MethodInfo { get; set; }
        internal object objectValue { get; set; }

        public int ExecuteCommandWithOptLock(bool isThrowError = false)
        {
            if (Context == null) return 0;
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            var result = inertable.GetType().GetMyMethod(nameof(ExecuteCommandWithOptLock), 1, typeof(bool)).Invoke(inertable, new object[] { isThrowError });
            return (int)result;
        }

        public async Task<int> ExecuteCommandWithOptLockAsync(bool isThrowError = false)
        {
            if (Context == null) return 0;
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            var result = inertable.GetType().GetMyMethod(nameof(ExecuteCommandWithOptLockAsync), 1, typeof(bool)).Invoke(inertable, new object[] { isThrowError });
            return await ((Task<int>)result).ConfigureAwait(false);
        }
        public int ExecuteCommand()
        {
            if (Context == null) return 0;
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            var result = inertable.GetType().GetMethod(nameof(ExecuteCommand)).Invoke(inertable, Array.Empty<object>());
            return (int)result;
        }

        public async Task<int> ExecuteCommandAsync()
        {
            if (Context == null) return 0;
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            var result = inertable.GetType().GetMyMethod(nameof(ExecuteCommandAsync), 0).Invoke(inertable, Array.Empty<object>());
            return await ((Task<int>)result).ConfigureAwait(false);
        }
        public UpdateCommonMethodInfo EnableDiffLogEvent(object businessData = null)
        {
            if (Context == null)
            {
                return new UpdateCommonMethodInfo();
            }
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            var newMethod = inertable.GetType().GetMyMethod(nameof(EnableDiffLogEvent), 1, typeof(object));
            var result = newMethod.Invoke(inertable, new object[] { businessData });
            return new UpdateCommonMethodInfo()
            {
                Context = result
            };
        }
        public UpdateCommonMethodInfo IgnoreColumns(params string[] ignoreColumns)
        {
            if (Context == null)
            {
                return new UpdateCommonMethodInfo();
            }
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            var newMethod = inertable.GetType().GetMyMethod(nameof(IgnoreColumns), 1, typeof(string[]));
            var result = newMethod.Invoke(inertable, new object[] { ignoreColumns });
            return new UpdateCommonMethodInfo()
            {
                Context = result
            };
        }
        public UpdateCommonMethodInfo IgnoreNullColumns()
        {
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            var newMethod = inertable.GetType().GetMyMethod(nameof(IgnoreNullColumns), 0);
            var result = newMethod.Invoke(inertable, Array.Empty<object>());
            return new UpdateCommonMethodInfo()
            {
                Context = result
            };
        }
        public UpdateCommonMethodInfo UpdateColumns(params string[] updateColumns)
        {
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            var newMethod = inertable.GetType().GetMyMethod(nameof(UpdateColumns), 1, typeof(string[]));
            var result = newMethod.Invoke(inertable, new object[] { updateColumns });
            return new UpdateCommonMethodInfo()
            {
                Context = result
            };
        }

        public UpdateCommonMethodInfo WhereColumns(params string[] whereColumns)
        {
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            var newMethod = inertable.GetType().GetMyMethod("WhereColumns", 1, typeof(string[]));
            var result = newMethod.Invoke(inertable, new object[] { whereColumns });
            return new UpdateCommonMethodInfo()
            {
                Context = result,
            };
        }

        public UpdateCommonMethodInfo AS(string tableName)
        {
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            var newMethod = inertable.GetType().GetMyMethod(nameof(QueryMethodInfo.AS), 1, typeof(string));
            var result = newMethod.Invoke(inertable, new object[] { tableName });
            return new UpdateCommonMethodInfo()
            {
                Context = result
            };
        }
        public UpdateCommonMethodInfo SplitTable()
        {
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            var newMethod = inertable.GetType().GetMyMethod(nameof(SplitTable), 0);
            var result = newMethod.Invoke(inertable, Array.Empty<object>());
            return new UpdateCommonMethodInfo()
            {
                Context = result
            };
        }
    }
}
