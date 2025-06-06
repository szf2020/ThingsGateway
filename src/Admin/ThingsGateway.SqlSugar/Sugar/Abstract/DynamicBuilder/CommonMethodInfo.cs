namespace SqlSugar
{
    public class CommonMethodInfo
    {
        internal object Context { get; set; }

        public int ExecuteReturnIdentity()
        {
            if (Context == null) return 0;
            var result = Context.GetType().GetMyMethod("ExecuteReturnIdentity", 0).Invoke(Context, Array.Empty<object>());
            return (int)result;
        }
        public async Task<int> ExecuteReturnIdentityAsync()
        {
            if (Context == null) return 0;
            var result = Context.GetType().GetMyMethod("ExecuteReturnIdentityAsync", 0).Invoke(Context, Array.Empty<object>());
            return await ((Task<int>)result).ConfigureAwait(false);
        }
        public int ExecuteCommand()
        {
            if (Context == null) return 0;
            var result = Context.GetType().GetMyMethod("ExecuteCommand", 0).Invoke(Context, Array.Empty<object>());
            return (int)result;
        }
        public async Task<int> ExecuteCommandAsync()
        {
            if (Context == null) return 0;
            var result = Context.GetType().GetMyMethod("ExecuteCommandAsync", 0).Invoke(Context, Array.Empty<object>());
            return await ((Task<int>)result).ConfigureAwait(false);
        }
    }
    public class SplitMethodInfo
    {
        internal object Context { get; set; }

        public int ExecuteCommand()
        {
            if (Context == null) return 0;
            var result = Context.GetType().GetMyMethod("ExecuteCommand", 0).Invoke(Context, Array.Empty<object>());
            return (int)result;
        }
        public async Task<int> ExecuteCommandAsync()
        {
            if (Context == null) return 0;
            var result = Context.GetType().GetMyMethod("ExecuteCommandAsync", 0).Invoke(Context, Array.Empty<object>());
            return await ((Task<int>)result).ConfigureAwait(false);
        }
    }
    public class UpdateCommonMethodInfo
    {
        internal object Context { get; set; }

        public int ExecuteCommand()
        {
            if (Context == null) return 0;
            var result = Context.GetType().GetMyMethod("ExecuteCommand", 0).Invoke(Context, Array.Empty<object>());
            return (int)result;
        }
        public UpdateCommonMethodInfo WhereColumns(params string[] columns)
        {
            var result = Context.GetType().GetMyMethod("WhereColumns", 1, typeof(string[])).Invoke(Context, new object[] { columns });
            UpdateCommonMethodInfo updateCommonMethod = new UpdateCommonMethodInfo();
            updateCommonMethod.Context = result;
            return updateCommonMethod;
        }
        public UpdateCommonMethodInfo UpdateColumns(params string[] columns)
        {
            var result = Context.GetType().GetMyMethod("UpdateColumns", 1, typeof(string[])).Invoke(Context, new object[] { columns });
            UpdateCommonMethodInfo updateCommonMethod = new UpdateCommonMethodInfo();
            updateCommonMethod.Context = result;
            return updateCommonMethod;
        }
        public async Task<int> ExecuteCommandAsync()
        {
            if (Context == null) return 0;
            var result = Context.GetType().GetMyMethod("ExecuteCommandAsync", 0).Invoke(Context, Array.Empty<object>());
            return await ((Task<int>)result).ConfigureAwait(false);
        }
        public UpdateCommonMethodInfo SplitTable()
        {
            var newMethod = this.Context.GetType().GetMyMethod("SplitTable", 0);
            var result = newMethod.Invoke(Context, Array.Empty<object>());
            return new UpdateCommonMethodInfo()
            {
                Context = result
            };
        }
    }

}
