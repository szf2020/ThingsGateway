using System.Data;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 提供AOP(面向切面编程)功能的类
    /// </summary>
    public class AopProvider
    {
        private AopProvider() { }
        /// <summary>
        /// 使用SqlSugarProvider实例初始化AopProvider
        /// </summary>
        /// <param name="context">SqlSugarProvider实例</param>
        public AopProvider(SqlSugarProvider context)
        {
            this.Context = context;
            this.Context.Ado.IsEnableLogEvent = true;
        }

        /// <summary>
        /// 获取或设置SqlSugarProvider实例
        /// </summary>
        private SqlSugarProvider Context { get; set; }

        /// <summary>
        /// 设置差异日志事件处理程序
        /// </summary>
        public Action<DiffLogModel> OnDiffLogEvent { set { this.Context.CurrentConnectionConfig.AopEvents.OnDiffLogEvent = value; } }

        /// <summary>
        /// 设置错误事件处理程序
        /// </summary>
        public Action<SqlSugarException> OnError { set { this.Context.CurrentConnectionConfig.AopEvents.OnError = value; } }

        /// <summary>
        /// 设置SQL执行前日志事件处理程序
        /// </summary>
        public Action<string, SugarParameter[]> OnLogExecuting { set { this.Context.CurrentConnectionConfig.AopEvents.OnLogExecuting = value; } }

        /// <summary>
        /// 设置SQL执行后日志事件处理程序
        /// </summary>
        public Action<string, SugarParameter[]> OnLogExecuted { set { this.Context.CurrentConnectionConfig.AopEvents.OnLogExecuted = value; } }

        /// <summary>
        /// 设置执行SQL前修改SQL的事件处理程序
        /// </summary>
        public Func<string, SugarParameter[], KeyValuePair<string, SugarParameter[]>> OnExecutingChangeSql { set { this.Context.CurrentConnectionConfig.AopEvents.OnExecutingChangeSql = value; } }

        /// <summary>
        /// 设置数据执行前事件处理程序
        /// </summary>
        public virtual Action<object, DataFilterModel> DataExecuting { set { this.Context.CurrentConnectionConfig.AopEvents.DataExecuting = value; } }

        /// <summary>
        /// 设置数据变更后事件处理程序
        /// </summary>
        public Action<object, DataFilterModel> DataChangesExecuted { set { this.Context.CurrentConnectionConfig.AopEvents.DataChangesExecuted = value; } }

        /// <summary>
        /// 设置数据执行后事件处理程序
        /// </summary>
        public virtual Action<object, DataAfterModel> DataExecuted { set { this.Context.CurrentConnectionConfig.AopEvents.DataExecuted = value; } }

        /// <summary>
        /// 设置检查连接执行前事件处理程序
        /// </summary>
        public Action<IDbConnection> CheckConnectionExecuting { set { this.Context.CurrentConnectionConfig.AopEvents.CheckConnectionExecuting = value; } }

        /// <summary>
        /// 设置检查连接执行后事件处理程序
        /// </summary>
        public Action<IDbConnection, TimeSpan> CheckConnectionExecuted { set { this.Context.CurrentConnectionConfig.AopEvents.CheckConnectionExecuted = value; } }

        /// <summary>
        /// 设置获取数据读取前事件处理程序
        /// </summary>
        public Action<string, SugarParameter[]> OnGetDataReadering { set { this.Context.CurrentConnectionConfig.AopEvents.OnGetDataReadering = value; } }

        /// <summary>
        /// 设置获取数据读取后事件处理程序
        /// </summary>
        public Action<string, SugarParameter[], TimeSpan> OnGetDataReadered { set { this.Context.CurrentConnectionConfig.AopEvents.OnGetDataReadered = value; } }
    }
}
