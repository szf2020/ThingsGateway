namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 通用方法信息类
    /// </summary>
    public class CommonMethodInfo
    {
        /// <summary>
        /// 上下文对象
        /// </summary>
        internal object Context { get; set; }

        /// <summary>
        /// 执行并返回自增ID
        /// </summary>
        /// <returns>自增ID</returns>
        public int ExecuteReturnIdentity()
        {
            if (Context == null) return 0;
            var result = Context.GetType().GetMyMethod(nameof(ExecuteReturnIdentity), 0).Invoke(Context, Array.Empty<object>());
            return (int)result;
        }

        /// <summary>
        /// 异步执行并返回自增ID
        /// </summary>
        /// <returns>自增ID任务</returns>
        public async Task<int> ExecuteReturnIdentityAsync()
        {
            if (Context == null) return 0;
            var result = Context.GetType().GetMyMethod(nameof(ExecuteReturnIdentityAsync), 0).Invoke(Context, Array.Empty<object>());
            return await ((Task<int>)result).ConfigureAwait(false);
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <returns>影响的行数</returns>
        public int ExecuteCommand()
        {
            if (Context == null) return 0;
            var result = Context.GetType().GetMyMethod(nameof(ExecuteCommand), 0).Invoke(Context, Array.Empty<object>());
            return (int)result;
        }

        /// <summary>
        /// 异步执行命令
        /// </summary>
        /// <returns>影响的行数任务</returns>
        public async Task<int> ExecuteCommandAsync()
        {
            if (Context == null) return 0;
            var result = Context.GetType().GetMyMethod(nameof(ExecuteCommandAsync), 0).Invoke(Context, Array.Empty<object>());
            return await ((Task<int>)result).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 分表方法信息类
    /// </summary>
    public class SplitMethodInfo
    {
        /// <summary>
        /// 上下文对象
        /// </summary>
        internal object Context { get; set; }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <returns>影响的行数</returns>
        public int ExecuteCommand()
        {
            if (Context == null) return 0;
            var result = Context.GetType().GetMyMethod(nameof(ExecuteCommand), 0).Invoke(Context, Array.Empty<object>());
            return (int)result;
        }

        /// <summary>
        /// 异步执行命令
        /// </summary>
        /// <returns>影响的行数任务</returns>
        public async Task<int> ExecuteCommandAsync()
        {
            if (Context == null) return 0;
            var result = Context.GetType().GetMyMethod(nameof(ExecuteCommandAsync), 0).Invoke(Context, Array.Empty<object>());
            return await ((Task<int>)result).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 更新通用方法信息类
    /// </summary>
    public class UpdateCommonMethodInfo
    {
        /// <summary>
        /// 上下文对象
        /// </summary>
        internal object Context { get; set; }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <returns>影响的行数</returns>
        public int ExecuteCommand()
        {
            if (Context == null) return 0;
            var result = Context.GetType().GetMyMethod(nameof(ExecuteCommand), 0).Invoke(Context, Array.Empty<object>());
            return (int)result;
        }

        /// <summary>
        /// 设置WHERE条件列
        /// </summary>
        /// <param name="columns">列名数组</param>
        /// <returns>更新通用方法信息</returns>
        public UpdateCommonMethodInfo WhereColumns(params string[] columns)
        {
            var result = Context.GetType().GetMyMethod("WhereColumns", 1, typeof(string[])).Invoke(Context, new object[] { columns });
            UpdateCommonMethodInfo updateCommonMethod = new UpdateCommonMethodInfo();
            updateCommonMethod.Context = result;
            return updateCommonMethod;
        }

        /// <summary>
        /// 设置更新列
        /// </summary>
        /// <param name="columns">列名数组</param>
        /// <returns>更新通用方法信息</returns>
        public UpdateCommonMethodInfo UpdateColumns(params string[] columns)
        {
            var result = Context.GetType().GetMyMethod(nameof(UpdateColumns), 1, typeof(string[])).Invoke(Context, new object[] { columns });
            UpdateCommonMethodInfo updateCommonMethod = new UpdateCommonMethodInfo();
            updateCommonMethod.Context = result;
            return updateCommonMethod;
        }

        /// <summary>
        /// 异步执行命令
        /// </summary>
        /// <returns>影响的行数任务</returns>
        public async Task<int> ExecuteCommandAsync()
        {
            if (Context == null) return 0;
            var result = Context.GetType().GetMyMethod(nameof(ExecuteCommandAsync), 0).Invoke(Context, Array.Empty<object>());
            return await ((Task<int>)result).ConfigureAwait(false);
        }

        /// <summary>
        /// 分表操作
        /// </summary>
        /// <returns>更新通用方法信息</returns>
        public UpdateCommonMethodInfo SplitTable()
        {
            var newMethod = this.Context.GetType().GetMyMethod(nameof(SplitTable), 0);
            var result = newMethod.Invoke(Context, Array.Empty<object>());
            return new UpdateCommonMethodInfo()
            {
                Context = result
            };
        }
    }
}