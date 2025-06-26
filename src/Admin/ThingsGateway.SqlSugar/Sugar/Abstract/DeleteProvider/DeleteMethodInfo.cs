using System.Reflection;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 删除方法信息类
    /// </summary>
    public class DeleteMethodInfo
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
        /// 执行删除命令
        /// </summary>
        /// <returns>影响的行数</returns>
        public int ExecuteCommand()
        {
            if (Context == null) return 0;
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            var result = inertable.GetType().GetMethod("ExecuteCommand").Invoke(inertable, Array.Empty<object>());
            return (int)result;
        }
        /// <summary>
        /// 异步执行删除命令
        /// </summary>
        /// <returns>影响的行数任务</returns>
        public async Task<int> ExecuteCommandAsync()
        {
            if (Context == null) return 0;
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            var result = inertable.GetType().GetMyMethod("ExecuteCommandAsync", 0).Invoke(inertable, Array.Empty<object>());
            return await ((Task<int>)result).ConfigureAwait(false);
        }

        /// <summary>
        /// 指定表名
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns>通用方法信息</returns>
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
        /// <param name="businessData">业务数据</param>
        /// <returns>通用方法信息</returns>
        public CommonMethodInfo EnableDiffLogEvent(object businessData = null)
        {
            if (Context == null)
            {
                return new CommonMethodInfo();
            }
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            var newMethod = inertable.GetType().GetMyMethod("EnableDiffLogEvent", 1, typeof(object));
            var result = newMethod.Invoke(inertable, new object[] { businessData });
            return new CommonMethodInfo()
            {
                Context = result
            };
        }
        /// <summary>
        /// 分表操作
        /// </summary>
        /// <returns>通用方法信息</returns>
        public CommonMethodInfo SplitTable()
        {
            var inertable = MethodInfo.Invoke(Context, new object[] { objectValue });
            var newMethod = inertable.GetType().GetMyMethod("SplitTable", 0);
            var result = newMethod.Invoke(inertable, Array.Empty<object>());
            return new CommonMethodInfo()
            {
                Context = result
            };
        }
    }
}