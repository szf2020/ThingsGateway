using System.Collections;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text.RegularExpressions;
namespace ThingsGateway.SqlSugar
{
    public abstract partial class AdoProvider : AdoAccessory, IAdo
    {
        #region Constructor
        /// <summary>
        /// 初始化 ADO.NET 数据访问提供程序
        /// </summary>
        public AdoProvider()
        {
            this.IsEnableLogEvent = false; // 默认禁用日志事件
            this.CommandType = CommandType.Text; // 默认命令类型为文本
            this.IsClearParameters = true; // 默认清除参数
            this.CommandTimeOut = 300; // 默认命令超时时间为300秒
        }
        #endregion

        #region Properties
        /// <summary>
        /// 是否是非关系型数据库
        /// </summary>
        public virtual bool IsNoSql { get; set; }

        /// <summary>
        /// 是否异步打开连接
        /// </summary>
        internal bool IsOpenAsync { get; set; }

        /// <summary>
        /// 输出参数集合
        /// </summary>
        protected List<IDataParameter> OutputParameters { get; set; }

        /// <summary>
        /// SQL参数关键字(默认为@)
        /// </summary>
        public virtual string SqlParameterKeyWord { get { return "@"; } }

        /// <summary>
        /// 当前数据库事务
        /// </summary>
        public IDbTransaction Transaction { get; set; }

        /// <summary>
        /// SqlSugar上下文
        /// </summary>
        public virtual SqlSugarProvider Context { get; set; }

        /// <summary>
        /// 旧的命令类型
        /// </summary>
        internal CommandType OldCommandType { get; set; }

        /// <summary>
        /// 旧的清除参数标志
        /// </summary>
        internal bool OldClearParameters { get; set; }

        /// <summary>
        /// 数据读取器参数集合
        /// </summary>
        public IDataParameterCollection DataReaderParameters { get; set; }

        /// <summary>
        /// SQL执行时间
        /// </summary>
        public TimeSpan SqlExecutionTime { get { return AfterTime - BeforeTime; } }

        /// <summary>
        /// 连接执行时间
        /// </summary>
        public TimeSpan ConnectionExecutionTime { get { return CheckConnectionAfterTime - CheckConnectionBeforeTime; } }

        /// <summary>
        /// 获取数据执行时间
        /// </summary>
        public TimeSpan GetDataExecutionTime { get { return GetDataAfterTime - GetDataBeforeTime; } }

        /// <summary>
        /// 增删改操作影响的行数
        /// </summary>
        public int SqlExecuteCount { get; protected set; } = 0;

        /// <summary>
        /// SQL执行类型
        /// </summary>
        public SugarActionType SqlExecuteType { get => this.Context.SugarActionType; }

        /// <summary>
        /// SQL堆栈跟踪信息
        /// </summary>
        public StackTraceInfo SqlStackTrace { get { return UtilMethods.GetStackTrace(); } }

        /// <summary>
        /// 是否禁用主从分离
        /// </summary>
        public bool IsDisableMasterSlaveSeparation { get; set; }

        // 时间记录字段
        internal DateTime BeforeTime = DateTime.MinValue;
        internal DateTime AfterTime = DateTime.MinValue;
        internal DateTime GetDataBeforeTime = DateTime.MinValue;
        internal DateTime GetDataAfterTime = DateTime.MinValue;
        internal DateTime CheckConnectionBeforeTime = DateTime.MinValue;
        internal DateTime CheckConnectionAfterTime = DateTime.MinValue;

        /// <summary>
        /// 数据库绑定对象
        /// </summary>
        public virtual IDbBind DbBind
        {
            get
            {
                if (base._DbBind == null)
                {
                    IDbBind bind = InstanceFactory.GetDbBind(this.Context.CurrentConnectionConfig);
                    base._DbBind = bind;
                    bind.Context = this.Context;
                }
                return base._DbBind;
            }
        }

        /// <summary>
        /// 命令超时时间(秒)
        /// </summary>
        public virtual int CommandTimeOut { get; set; }

        /// <summary>
        /// 命令类型
        /// </summary>
        public virtual CommandType CommandType { get; set; }

        /// <summary>
        /// 是否启用日志事件
        /// </summary>
        public virtual bool IsEnableLogEvent { get; set; }

        /// <summary>
        /// 是否清除参数
        /// </summary>
        public virtual bool IsClearParameters { get; set; }

        /// <summary>
        /// 日志开始事件
        /// </summary>
        public virtual Action<string, IReadOnlyCollection<SugarParameter>> LogEventStarting => this.Context.CurrentConnectionConfig.AopEvents?.OnLogExecuting;

        /// <summary>
        /// 日志完成事件
        /// </summary>
        public virtual Action<string, IReadOnlyCollection<SugarParameter>> LogEventCompleted => this.Context.CurrentConnectionConfig.AopEvents?.OnLogExecuted;

        /// <summary>
        /// 检查连接执行前事件
        /// </summary>
        public virtual Action<IDbConnection> CheckConnectionExecuting => this.Context.CurrentConnectionConfig.AopEvents?.CheckConnectionExecuting;

        /// <summary>
        /// 检查连接执行后事件
        /// </summary>
        public virtual Action<IDbConnection, TimeSpan> CheckConnectionExecuted => this.Context.CurrentConnectionConfig.AopEvents?.CheckConnectionExecuted;

        /// <summary>
        /// 获取数据读取前事件
        /// </summary>
        public virtual Action<string, IReadOnlyCollection<SugarParameter>> OnGetDataReadering => this.Context.CurrentConnectionConfig.AopEvents?.OnGetDataReadering;

        /// <summary>
        /// 获取数据读取后事件
        /// </summary>
        public virtual Action<string, IReadOnlyCollection<SugarParameter>, TimeSpan> OnGetDataReadered => this.Context.CurrentConnectionConfig.AopEvents?.OnGetDataReadered;

        /// <summary>
        /// 处理SQL执行前事件
        /// </summary>
        public virtual Func<string, IReadOnlyCollection<SugarParameter>, KeyValuePair<string, IReadOnlyCollection<SugarParameter>>> ProcessingEventStartingSQL => this.Context.CurrentConnectionConfig.AopEvents?.OnExecutingChangeSql;

        /// <summary>
        /// SQL格式化函数
        /// </summary>
        protected virtual Func<string, string> FormatSql { get; set; }

        /// <summary>
        /// 错误事件
        /// </summary>
        public virtual Action<SqlSugarException> ErrorEvent => this.Context.CurrentConnectionConfig.AopEvents?.OnError;

        /// <summary>
        /// 差异日志事件
        /// </summary>
        public virtual Action<DiffLogModel> DiffLogEvent => this.Context.CurrentConnectionConfig.AopEvents?.OnDiffLogEvent;

        /// <summary>
        /// 从库连接集合
        /// </summary>
        public virtual List<IDbConnection> SlaveConnections { get; set; }

        /// <summary>
        /// 主库连接
        /// </summary>
        public virtual IDbConnection MasterConnection { get; set; }

        /// <summary>
        /// 主库连接字符串
        /// </summary>
        public virtual string MasterConnectionString { get; set; }

        /// <summary>
        /// 取消令牌
        /// </summary>
        public virtual CancellationToken? CancellationToken { get; set; }
        #endregion

        #region Connection
        /// <summary>
        /// 检查连接是否有效(会自动关闭连接)
        /// </summary>
        public virtual bool IsValidConnection()
        {
            try
            {
                if (this.IsAnyTran())
                {
                    return true;
                }
                using (OpenAlways())
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 检查连接是否有效(不会自动关闭连接)
        /// </summary>
        public virtual bool IsValidConnectionNoClose()
        {
            try
            {
                this.Open();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 打开连接
        /// </summary>
        public virtual void Open()
        {
            CheckConnection();
        }

        /// <summary>
        /// 异步打开连接
        /// </summary>
        public virtual async Task OpenAsync()
        {
            await CheckConnectionAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// 获取一个始终打开的连接对象
        /// </summary>
        public SugarConnection OpenAlways()
        {
            SugarConnection result = new SugarConnection();
            result.IsAutoClose = this.Context.CurrentConnectionConfig.IsAutoCloseConnection;
            result.conn = this.Connection;
            result.Context = this.Context;
            this.Context.CurrentConnectionConfig.IsAutoCloseConnection = false;
            this.Open();
            return result;
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public virtual void Close()
        {
            if (this.Transaction != null)
            {
                this.Transaction = null;
            }
            if (this.Connection != null && this.Connection.State == ConnectionState.Open)
            {
                this.Connection.Close();
            }
            if (this.IsMasterSlaveSeparation && this.SlaveConnections.HasValue())
            {
                foreach (var slaveConnection in this.SlaveConnections)
                {
                    if (slaveConnection != null && slaveConnection.State == ConnectionState.Open)
                    {
                        slaveConnection.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {
            if (this.Transaction != null)
            {
                this.Transaction.Rollback();
                this.Transaction = null;
            }

            this.Connection?.Dispose();
            this.Connection = null;

            if (this.IsMasterSlaveSeparation)
            {
                if (this.SlaveConnections != null)
                {
                    foreach (var slaveConnection in this.SlaveConnections)
                    {
                        if (slaveConnection != null && slaveConnection.State == ConnectionState.Open)
                        {
                            slaveConnection.Dispose();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 检查连接状态
        /// </summary>
        public virtual void CheckConnection()
        {
            this.CheckConnectionBefore(this.Connection);
            if (this.Connection.State != ConnectionState.Open)
            {
                try
                {
                    this.Connection.Open();
                }
                catch (Exception ex)
                {
                    if (this.Context.CurrentConnectionConfig?.DbType == DbType.SqlServer && ex.Message?.Contains("provider: SSL") == true)
                    {
                        Check.ExceptionEasy(true, ex.Message, "SSL出错，因为升级了驱动,字符串增加Encrypt=True;TrustServerCertificate=True;即可。详细错误：" + ex.Message);
                    }
                    Check.Exception(true, ErrorMessage.ConnnectionOpen, ex.Message + $"DbType=\"{this.Context.CurrentConnectionConfig.DbType}\";ConfigId=\"{this.Context.CurrentConnectionConfig.ConfigId}\"");
                }
            }
            this.CheckConnectionAfter(this.Connection);
        }

        /// <summary>
        /// 异步检查连接状态
        /// </summary>
        public virtual async Task CheckConnectionAsync()
        {
            this.CheckConnectionBefore(this.Connection);
            if (this.Connection.State != ConnectionState.Open)
            {
                try
                {
                    if (IsOpenAsync)
                        await (Connection as DbConnection).OpenAsync().ConfigureAwait(false);
                    else
#pragma warning disable CA1849
                        (Connection as DbConnection).Open();
#pragma warning restore CA1849
                }
                catch (Exception ex)
                {
                    Check.Exception(true, ErrorMessage.ConnnectionOpen, ex.Message + $"DbType=\"{this.Context.CurrentConnectionConfig.DbType}\";ConfigId=\"{this.Context.CurrentConnectionConfig.ConfigId}\"");
                }
            }
            this.CheckConnectionAfter(this.Connection);
        }

        /// <summary>
        /// 连接检查前的操作
        /// </summary>
        public virtual void CheckConnectionBefore(IDbConnection Connection)
        {
            this.CheckConnectionBeforeTime = DateTime.Now;
            if (this.IsEnableLogEvent)
            {
                Action<IDbConnection> action = CheckConnectionExecuting;
                if (action != null)
                {
                    action(Connection);
                }
            }
        }

        /// <summary>
        /// 连接检查后的操作
        /// </summary>
        public virtual void CheckConnectionAfter(IDbConnection Connection)
        {
            this.CheckConnectionAfterTime = DateTime.Now;
            if (this.IsEnableLogEvent)
            {
                Action<IDbConnection, TimeSpan> action = CheckConnectionExecuted;
                if (action != null)
                {
                    action(Connection, this.ConnectionExecutionTime);
                }
            }
        }
        #endregion

        #region Transaction
        /// <summary>
        /// 检查是否存在事务
        /// </summary>
        public virtual bool IsAnyTran()
        {
            return this.Transaction != null;
        }

        /// <summary>
        /// 检查是否没有事务
        /// </summary>
        public virtual bool IsNoTran()
        {
            return this.Transaction == null;
        }

        /// <summary>
        /// 开始事务
        /// </summary>
        public virtual void BeginTran()
        {
            CheckConnection();
            if (this.Transaction == null)
                this.Transaction = this.Connection.BeginTransaction();
        }

        /// <summary>
        /// 异步开始事务
        /// </summary>
        public virtual async Task BeginTranAsync()
        {
            await CheckConnectionAsync().ConfigureAwait(false);
            if (this.Transaction == null)
                this.Transaction = await (Connection as DbConnection).BeginTransactionAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// 使用指定隔离级别开始事务
        /// </summary>
        public virtual void BeginTran(IsolationLevel iso)
        {
            CheckConnection();
            if (this.Transaction == null)
                this.Transaction = this.Connection.BeginTransaction(iso);
        }

        /// <summary>
        /// 异步使用指定隔离级别开始事务
        /// </summary>
        public virtual async Task BeginTranAsync(IsolationLevel iso)
        {
            await CheckConnectionAsync().ConfigureAwait(false);
            if (this.Transaction == null)
                this.Transaction = await (Connection as DbConnection).BeginTransactionAsync(iso).ConfigureAwait(false);
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public virtual void RollbackTran()
        {
            if (this.Transaction != null)
            {
                this.Transaction.Rollback();
                this.Transaction = null;
                if (this.Context.CurrentConnectionConfig.IsAutoCloseConnection) this.Close();
            }
        }

        /// <summary>
        /// 异步回滚事务
        /// </summary>
        public virtual async Task RollbackTranAsync()
        {
            if (this.Transaction != null)
            {
                await (Transaction as DbTransaction).RollbackAsync().ConfigureAwait(false);
                this.Transaction = null;
                if (this.Context.CurrentConnectionConfig.IsAutoCloseConnection) await CloseAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public virtual void CommitTran()
        {
            if (this.Transaction != null)
            {
                this.Transaction.Commit();
                this.Transaction = null;
                if (this.Context.CurrentConnectionConfig.IsAutoCloseConnection) this.Close();
            }
        }

        /// <summary>
        /// 异步提交事务
        /// </summary>
        public virtual async Task CommitTranAsync()
        {
            if (this.Transaction != null)
            {
                await (Transaction as DbTransaction).CommitAsync().ConfigureAwait(false);
                this.Transaction = null;
                if (this.Context.CurrentConnectionConfig.IsAutoCloseConnection) await CloseAsync().ConfigureAwait(false);
            }
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// 将SugarParameter数组转换为IDataParameter数组
        /// </summary>
        public abstract IDataParameter[] ToIDbDataParameter(params IReadOnlyCollection<SugarParameter> pars);

        /// <summary>
        /// 设置命令到数据适配器
        /// </summary>
        public abstract void SetCommandToAdapter(IDataAdapter adapter, DbCommand command);

        /// <summary>
        /// 获取数据适配器
        /// </summary>
        public abstract IDataAdapter GetAdapter();

        /// <summary>
        /// 获取数据库命令对象
        /// </summary>
        public abstract DbCommand GetCommand(string sql, IReadOnlyCollection<SugarParameter> pars);

        /// <summary>
        /// 数据库连接对象
        /// </summary>
        public abstract IDbConnection Connection { get; set; }

        /// <summary>
        /// 使用指定事务名称开始事务(仅SQL Server支持)
        /// </summary>
        public abstract void BeginTran(string transactionName);

        /// <summary>
        /// 使用指定隔离级别和事务名称开始事务(仅SQL Server支持)
        /// </summary>
        public abstract void BeginTran(IsolationLevel iso, string transactionName);
        #endregion

        #region Transaction Usage
        /// <summary>
        /// 获取事务对象
        /// </summary>
        public SqlSugarTransactionAdo UseTran()
        {
            return new SqlSugarTransactionAdo(this.Context);
        }

        /// <summary>
        /// 使用事务执行操作
        /// </summary>
        public DbResult<bool> UseTran(Action action, Action<Exception> errorCallBack = null)
        {
            var result = new DbResult<bool>();
            try
            {
                this.BeginTran();
                if (action != null)
                    action();
                this.CommitTran();
                result.Data = result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.ErrorException = ex;
                result.ErrorMessage = ex.Message;
                result.IsSuccess = false;
                this.RollbackTran();
                if (errorCallBack != null)
                {
                    errorCallBack(ex);
                }
            }
            return result;
        }

        /// <summary>
        /// 异步使用事务执行操作
        /// </summary>
        public async Task<DbResult<bool>> UseTranAsync(Func<Task> action, Action<Exception> errorCallBack = null)
        {
            var result = new DbResult<bool>();
            try
            {
                if (IsOpenAsync)
                    await BeginTranAsync().ConfigureAwait(false);
                else
#pragma warning disable CA1849
                    BeginTran();
                if (action != null)
                    await action().ConfigureAwait(false);
                if (IsOpenAsync)
                    await CommitTranAsync().ConfigureAwait(false);
                else
                    CommitTran();
                result.Data = result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.ErrorException = ex;
                result.ErrorMessage = ex.Message;
                result.IsSuccess = false;
                if (IsOpenAsync)
                    await RollbackTranAsync().ConfigureAwait(false);
                else
                    RollbackTran();
#pragma warning restore CA1849
                if (errorCallBack != null)
                {
                    errorCallBack(ex);
                }
            }
            return result;
        }

        /// <summary>
        /// 使用事务执行操作并返回结果
        /// </summary>
        public DbResult<T> UseTran<T>(Func<T> action, Action<Exception> errorCallBack = null)
        {
            var result = new DbResult<T>();
            try
            {
                this.BeginTran();
                if (action != null)
                    result.Data = action();
                this.CommitTran();
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.ErrorException = ex;
                result.ErrorMessage = ex.Message;
                result.IsSuccess = false;
                this.RollbackTran();
                if (errorCallBack != null)
                {
                    errorCallBack(ex);
                }
            }
            return result;
        }

        /// <summary>
        /// 异步使用事务执行操作并返回结果
        /// </summary>
        public async Task<DbResult<T>> UseTranAsync<T>(Func<Task<T>> action, Action<Exception> errorCallBack = null)
        {
            var result = new DbResult<T>();
            try
            {
                if (IsOpenAsync)
                    await BeginTranAsync().ConfigureAwait(false);
                else
#pragma warning disable CA1849
                    BeginTran();
                if (action != null)
                    result.Data = await action().ConfigureAwait(false);
                if (IsOpenAsync)
                    await CommitTranAsync().ConfigureAwait(false);
                else
                    CommitTran();
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.ErrorException = ex;
                result.ErrorMessage = ex.Message;
                result.IsSuccess = false;
                if (IsOpenAsync)
                    await RollbackTranAsync().ConfigureAwait(false);
                else
                    RollbackTran();
#pragma warning restore CA1849
                if (errorCallBack != null)
                {
                    errorCallBack(ex);
                }
            }
            return result;
        }

        /// <summary>
        /// 使用存储过程
        /// </summary>
        public IAdo UseStoredProcedure()
        {
            this.OldCommandType = this.CommandType;
            this.OldClearParameters = this.IsClearParameters;
            this.CommandType = CommandType.StoredProcedure;
            this.IsClearParameters = false;
            return this;
        }
        #endregion

        #region Core Methods
        /// <summary>
        /// 执行包含GO语句的SQL命令
        /// </summary>
        public virtual int ExecuteCommandWithGo(string sql, params IReadOnlyCollection<SugarParameter> parameters)
        {
            if (string.IsNullOrEmpty(sql))
                return 0;
            if (!sql.Contains("go", StringComparison.CurrentCultureIgnoreCase))
            {
                return ExecuteCommand(sql);
            }

            // 使用正则表达式分割包含GO的SQL语句
            System.Collections.ArrayList al = new System.Collections.ArrayList();
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(
                @"^(\s*)go(\s*)$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase |
                System.Text.RegularExpressions.RegexOptions.Multiline |
                System.Text.RegularExpressions.RegexOptions.Compiled |
                System.Text.RegularExpressions.RegexOptions.ExplicitCapture);

            al.AddRange(reg.Split(sql));
            int count = 0;
            foreach (string item in al)
            {
                if (item.HasValue())
                {
                    count += ExecuteCommand(item, parameters);
                }
            }
            return count;
        }

        /// <summary>
        /// 执行SQL命令
        /// </summary>
        public virtual int ExecuteCommand(string sql, params IReadOnlyCollection<SugarParameter> parameters)
        {
            try
            {
                InitParameters(ref sql, parameters);
                if (IsFormat(parameters))
                    sql = FormatSql(sql);

                // 检查是否使用了SQL中间件
                if (this.Context.CurrentConnectionConfig?.SqlMiddle?.IsSqlMiddle == true)
                    return this.Context.CurrentConnectionConfig.SqlMiddle.ExecuteCommand(sql, parameters);

                SetConnectionStart(sql);
                if (this.ProcessingEventStartingSQL != null)
                    ExecuteProcessingSQL(ref sql, ref parameters);

                ExecuteBefore(sql, parameters);
                IDbCommand sqlCommand = GetCommand(sql, parameters);
                int count = sqlCommand.ExecuteNonQuery();

                if (this.IsClearParameters)
                    sqlCommand.Parameters.Clear();

                // 记录影响条数
                this.SqlExecuteCount = count;
                ExecuteAfter(sql, parameters);
                sqlCommand.Dispose();
                return count;
            }
            catch (Exception ex)
            {
                SugarCatch(ex, sql, parameters);
                CommandType = CommandType.Text;
                if (ErrorEvent != null)
                    ExecuteErrorEvent(sql, parameters, ex);
                throw;
            }
            finally
            {
                if (this.IsAutoClose()) this.Close();
                SetConnectionEnd(sql);
            }
        }

        /// <summary>
        /// 获取数据读取器
        /// </summary>
        public virtual IDataReader GetDataReader(string sql, params IReadOnlyCollection<SugarParameter> parameters)
        {
            try
            {
                InitParameters(ref sql, parameters);
                if (IsFormat(parameters))
                    sql = FormatSql(sql);

                if (this.Context.CurrentConnectionConfig?.SqlMiddle?.IsSqlMiddle == true)
                    return this.Context.CurrentConnectionConfig.SqlMiddle.GetDataReader(sql, parameters);

                SetConnectionStart(sql);
                var isSp = this.CommandType == CommandType.StoredProcedure;

                if (this.ProcessingEventStartingSQL != null)
                    ExecuteProcessingSQL(ref sql, ref parameters);

                ExecuteBefore(sql, parameters);
                IDbCommand sqlCommand = GetCommand(sql, parameters);
                IDataReader sqlDataReader = sqlCommand.ExecuteReader(this.IsAutoClose() ? CommandBehavior.CloseConnection : CommandBehavior.Default);

                if (isSp)
                    DataReaderParameters = sqlCommand.Parameters;

                if (this.IsClearParameters)
                    sqlCommand.Parameters.Clear();

                ExecuteAfter(sql, parameters);
                SetConnectionEnd(sql);

                if (SugarCompatible.IsFramework || this.Context.CurrentConnectionConfig.DbType != DbType.Sqlite)
                    sqlCommand.Dispose();

                return sqlDataReader;
            }
            catch (Exception ex)
            {
                SugarCatch(ex, sql, parameters);
                CommandType = CommandType.Text;
                if (ErrorEvent != null)
                    ExecuteErrorEvent(sql, parameters, ex);
                throw;
            }
        }

        /// <summary>
        /// 获取数据集
        /// </summary>
        public virtual DataSet GetDataSetAll(string sql, params IReadOnlyCollection<SugarParameter> parameters)
        {
            try
            {
                InitParameters(ref sql, parameters);
                if (IsFormat(parameters))
                    sql = FormatSql(sql);

                if (this.Context.CurrentConnectionConfig?.SqlMiddle?.IsSqlMiddle == true)
                    return this.Context.CurrentConnectionConfig.SqlMiddle.GetDataSetAll(sql, parameters);

                SetConnectionStart(sql);
                if (this.ProcessingEventStartingSQL != null)
                    ExecuteProcessingSQL(ref sql, ref parameters);

                ExecuteBefore(sql, parameters);
                IDataAdapter dataAdapter = this.GetAdapter();
                DbCommand sqlCommand = GetCommand(sql, parameters);
                this.SetCommandToAdapter(dataAdapter, sqlCommand);

                DataSet ds = new DataSet();
                dataAdapter.Fill(ds);

                if (this.IsClearParameters)
                    sqlCommand.Parameters.Clear();

                ExecuteAfter(sql, parameters);
                sqlCommand.Dispose();
                return ds;
            }
            catch (Exception ex)
            {
                SugarCatch(ex, sql, parameters);
                CommandType = CommandType.Text;
                if (ErrorEvent != null)
                    ExecuteErrorEvent(sql, parameters, ex);
                throw;
            }
            finally
            {
                if (this.IsAutoClose()) this.Close();
                SetConnectionEnd(sql);
            }
        }

        /// <summary>
        /// 获取单个值
        /// </summary>
        public virtual object GetScalar(string sql, params IReadOnlyCollection<SugarParameter> parameters)
        {
            try
            {
                InitParameters(ref sql, parameters);
                if (IsFormat(parameters))
                    sql = FormatSql(sql);

                if (this.Context.CurrentConnectionConfig?.SqlMiddle?.IsSqlMiddle == true)
                    return this.Context.CurrentConnectionConfig.SqlMiddle.GetScalar(sql, parameters);

                SetConnectionStart(sql);
                if (this.ProcessingEventStartingSQL != null)
                    ExecuteProcessingSQL(ref sql, ref parameters);

                ExecuteBefore(sql, parameters);
                IDbCommand sqlCommand = GetCommand(sql, parameters);
                object scalar = sqlCommand.ExecuteScalar();

                if (this.IsClearParameters)
                    sqlCommand.Parameters.Clear();

                ExecuteAfter(sql, parameters);
                sqlCommand.Dispose();
                return scalar;
            }
            catch (Exception ex)
            {
                SugarCatch(ex, sql, parameters);
                CommandType = CommandType.Text;
                if (ErrorEvent != null)
                    ExecuteErrorEvent(sql, parameters, ex);
                throw;
            }
            finally
            {
                if (this.IsAutoClose()) this.Close();
                SetConnectionEnd(sql);
            }
        }

        /// <summary>
        /// 异步执行SQL命令
        /// </summary>
        public virtual async Task<int> ExecuteCommandAsync(string sql, params IReadOnlyCollection<SugarParameter> parameters)
        {
            try
            {
                Async();
                InitParameters(ref sql, parameters);
                if (IsFormat(parameters))
                    sql = FormatSql(sql);

                if (this.Context.CurrentConnectionConfig?.SqlMiddle?.IsSqlMiddle == true)
                    return await Context.CurrentConnectionConfig.SqlMiddle.ExecuteCommandAsync(sql, parameters).ConfigureAwait(false);

                SetConnectionStart(sql);
                if (this.ProcessingEventStartingSQL != null)
                    ExecuteProcessingSQL(ref sql, ref parameters);

                ExecuteBefore(sql, parameters);
#pragma warning disable CA1849
                var sqlCommand = IsOpenAsync ? await GetCommandAsync(sql, parameters).ConfigureAwait(false) : GetCommand(sql, parameters);
#pragma warning restore CA1849

                int count;
                if (this.CancellationToken == null)
                    count = await sqlCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                else
                    count = await sqlCommand.ExecuteNonQueryAsync(CancellationToken.Value).ConfigureAwait(false);

                if (this.IsClearParameters)
                    sqlCommand.Parameters.Clear();

                this.SqlExecuteCount = count;
                ExecuteAfter(sql, parameters);
                sqlCommand.Dispose();
                return count;
            }
            catch (Exception ex)
            {
                SugarCatch(ex, sql, parameters);
                CommandType = CommandType.Text;
                if (ErrorEvent != null)
                    ExecuteErrorEvent(sql, parameters, ex);
                throw;
            }
            finally
            {
                if (this.IsAutoClose())
                    await this.CloseAsync().ConfigureAwait(false);
                SetConnectionEnd(sql);
            }
        }

        /// <summary>
        /// 异步获取数据读取器
        /// </summary>
        public virtual async Task<IDataReader> GetDataReaderAsync(string sql, params IReadOnlyCollection<SugarParameter> parameters)
        {
            try
            {
                Async();
                InitParameters(ref sql, parameters);
                if (IsFormat(parameters))
                    sql = FormatSql(sql);

                if (this.Context.CurrentConnectionConfig?.SqlMiddle?.IsSqlMiddle == true)
                    return await Context.CurrentConnectionConfig.SqlMiddle.GetDataReaderAsync(sql, parameters).ConfigureAwait(false);

                SetConnectionStart(sql);
                var isSp = this.CommandType == CommandType.StoredProcedure;

                if (this.ProcessingEventStartingSQL != null)
                    ExecuteProcessingSQL(ref sql, ref parameters);

                ExecuteBefore(sql, parameters);
#pragma warning disable CA1849
                var sqlCommand = IsOpenAsync ? await GetCommandAsync(sql, parameters).ConfigureAwait(false) : GetCommand(sql, parameters);
#pragma warning restore CA1849

                DbDataReader sqlDataReader;
                if (this.CancellationToken == null)
                    sqlDataReader = await sqlCommand.ExecuteReaderAsync(IsAutoClose() ? CommandBehavior.CloseConnection : CommandBehavior.Default).ConfigureAwait(false);
                else
                    sqlDataReader = await sqlCommand.ExecuteReaderAsync(IsAutoClose() ? CommandBehavior.CloseConnection : CommandBehavior.Default, CancellationToken.Value).ConfigureAwait(false);

                if (isSp)
                    DataReaderParameters = sqlCommand.Parameters;

                if (this.IsClearParameters)
                    sqlCommand.Parameters.Clear();

                ExecuteAfter(sql, parameters);
                SetConnectionEnd(sql);

                if (SugarCompatible.IsFramework || this.Context.CurrentConnectionConfig.DbType != DbType.Sqlite)
                    sqlCommand.Dispose();

                return sqlDataReader;
            }
            catch (Exception ex)
            {
                SugarCatch(ex, sql, parameters);
                CommandType = CommandType.Text;
                if (ErrorEvent != null)
                    ExecuteErrorEvent(sql, parameters, ex);
                throw;
            }
        }

        /// <summary>
        /// 异步获取单个值
        /// </summary>
        public virtual async Task<object> GetScalarAsync(string sql, params IReadOnlyCollection<SugarParameter> parameters)
        {
            try
            {
                Async();
                InitParameters(ref sql, parameters);
                if (IsFormat(parameters))
                    sql = FormatSql(sql);

                if (this.Context.CurrentConnectionConfig?.SqlMiddle?.IsSqlMiddle == true)
                    return await Context.CurrentConnectionConfig.SqlMiddle.GetScalarAsync(sql, parameters).ConfigureAwait(false);

                SetConnectionStart(sql);
                if (this.ProcessingEventStartingSQL != null)
                    ExecuteProcessingSQL(ref sql, ref parameters);

                ExecuteBefore(sql, parameters);
#pragma warning disable CA1849
                var sqlCommand = IsOpenAsync ? await GetCommandAsync(sql, parameters).ConfigureAwait(false) : GetCommand(sql, parameters);
#pragma warning restore CA1849

                object scalar;
                if (CancellationToken == null)
                    scalar = await sqlCommand.ExecuteScalarAsync().ConfigureAwait(false);
                else
                    scalar = await sqlCommand.ExecuteScalarAsync(CancellationToken.Value).ConfigureAwait(false);

                if (this.IsClearParameters)
                    sqlCommand.Parameters.Clear();

                ExecuteAfter(sql, parameters);
                sqlCommand.Dispose();
                return scalar;
            }
            catch (Exception ex)
            {
                SugarCatch(ex, sql, parameters);
                CommandType = CommandType.Text;
                if (ErrorEvent != null)
                    ExecuteErrorEvent(sql, parameters, ex);
                throw;
            }
            finally
            {
                if (this.IsAutoClose())
                    await this.CloseAsync().ConfigureAwait(false);
                SetConnectionEnd(sql);
            }
        }

        /// <summary>
        /// 异步获取数据集(伪异步实现)
        /// </summary>
        public virtual Task<DataSet> GetDataSetAllAsync(string sql, params IReadOnlyCollection<SugarParameter> parameters)
        {
            Async();

            // 由于DataSet不支持真正的异步操作，这里使用Task.Run模拟异步
            if (CancellationToken == null)
            {
                return Task.Run(() => GetDataSetAll(sql, parameters));
            }
            else
            {
                return Task.Run(() => GetDataSetAll(sql, parameters), this.CancellationToken.Value);
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// 获取字符串结果(使用对象参数)
        /// </summary>
        public virtual string GetString(string sql, object parameters)
        {
            return GetString(sql, this.GetParameters(parameters));
        }

        /// <summary>
        /// 获取字符串结果(使用参数数组)
        /// </summary>
        public virtual string GetString(string sql, params IReadOnlyCollection<SugarParameter> parameters)
        {
            return Convert.ToString(GetScalar(sql, parameters));
        }
        public virtual Task<string> GetStringAsync(string sql, object parameters, CancellationToken cancellationToken)
        {
            this.CancellationToken = cancellationToken;
            return GetStringAsync(sql, this.GetParameters(parameters));
        }
        /// <summary>
        /// 异步获取字符串结果(使用对象参数)
        /// </summary>
        public virtual Task<string> GetStringAsync(string sql, object parameters)
        {
            return GetStringAsync(sql, this.GetParameters(parameters));
        }

        /// <summary>
        /// 异步获取字符串结果(使用参数数组)
        /// </summary>
        public virtual async Task<string> GetStringAsync(string sql, params IReadOnlyCollection<SugarParameter> parameters)
        {
            return Convert.ToString(await GetScalarAsync(sql, parameters).ConfigureAwait(false));
        }

        /// <summary>
        /// 获取长整型结果(使用对象参数)
        /// </summary>
        public virtual long GetLong(string sql, object parameters = null)
        {
            return Convert.ToInt64(GetScalar(sql, GetParameters(parameters)));
        }

        /// <summary>
        /// 异步获取长整型结果(使用对象参数)
        /// </summary>
        public virtual async Task<long> GetLongAsync(string sql, object parameters = null)
        {
            return Convert.ToInt64(await GetScalarAsync(sql, GetParameters(parameters)).ConfigureAwait(false));
        }

        /// <summary>
        /// 获取整型结果(使用对象参数)
        /// </summary>
        public virtual int GetInt(string sql, object parameters)
        {
            return GetInt(sql, this.GetParameters(parameters));
        }

        /// <summary>
        /// 获取整型结果(使用参数数组)
        /// </summary>
        public virtual int GetInt(string sql, params IReadOnlyCollection<SugarParameter> parameters)
        {
            return GetScalar(sql, parameters).ObjToInt();
        }
        public virtual Task<int> GetIntAsync(string sql, object parameters, CancellationToken cancellationToken)
        {
            this.CancellationToken = cancellationToken;
            return GetIntAsync(sql, this.GetParameters(parameters));
        }
        /// <summary>
        /// 异步获取整型结果(使用对象参数)
        /// </summary>
        public virtual Task<int> GetIntAsync(string sql, object parameters)
        {
            return GetIntAsync(sql, this.GetParameters(parameters));
        }

        /// <summary>
        /// 异步获取整型结果(使用参数数组)
        /// </summary>
        public virtual async Task<int> GetIntAsync(string sql, params IReadOnlyCollection<SugarParameter> parameters)
        {
            var list = await GetScalarAsync(sql, parameters).ConfigureAwait(false);
            return list.ObjToInt();
        }

        /// <summary>
        /// 获取双精度浮点数结果(使用对象参数)
        /// </summary>
        public virtual Double GetDouble(string sql, object parameters)
        {
            return GetDouble(sql, this.GetParameters(parameters));
        }

        /// <summary>
        /// 获取双精度浮点数结果(使用参数数组)
        /// </summary>
        public virtual Double GetDouble(string sql, params IReadOnlyCollection<SugarParameter> parameters)
        {
            return GetScalar(sql, parameters).ObjToMoney();
        }
        public virtual Task<Double> GetDoubleAsync(string sql, object parameters, CancellationToken cancellationToken)
        {
            this.CancellationToken = cancellationToken;
            return GetDoubleAsync(sql, this.GetParameters(parameters));
        }
        /// <summary>
        /// 异步获取双精度浮点数结果(使用对象参数)
        /// </summary>
        public virtual Task<Double> GetDoubleAsync(string sql, object parameters)
        {
            return GetDoubleAsync(sql, this.GetParameters(parameters));
        }

        /// <summary>
        /// 异步获取双精度浮点数结果(使用参数数组)
        /// </summary>
        public virtual async Task<Double> GetDoubleAsync(string sql, params IReadOnlyCollection<SugarParameter> parameters)
        {
            var result = await GetScalarAsync(sql, parameters).ConfigureAwait(false);
            return result.ObjToMoney();
        }

        /// <summary>
        /// 获取十进制数结果(使用对象参数)
        /// </summary>
        public virtual decimal GetDecimal(string sql, object parameters)
        {
            return GetDecimal(sql, this.GetParameters(parameters));
        }

        /// <summary>
        /// 获取十进制数结果(使用参数数组)
        /// </summary>
        public virtual decimal GetDecimal(string sql, params IReadOnlyCollection<SugarParameter> parameters)
        {
            return GetScalar(sql, parameters).ObjToDecimal();
        }

        /// <summary>
        /// 异步获取十进制数结果(使用对象参数)
        /// </summary>
        public virtual Task<decimal> GetDecimalAsync(string sql, object parameters)
        {
            return GetDecimalAsync(sql, this.GetParameters(parameters));
        }

        /// <summary>
        /// 异步获取十进制数结果(使用参数数组)
        /// </summary>
        public virtual async Task<decimal> GetDecimalAsync(string sql, params IReadOnlyCollection<SugarParameter> parameters)
        {
            var result = await GetScalarAsync(sql, parameters).ConfigureAwait(false);
            return result.ObjToDecimal();
        }

        /// <summary>
        /// 获取日期时间结果(使用对象参数)
        /// </summary>
        public virtual DateTime GetDateTime(string sql, object parameters)
        {
            return GetDateTime(sql, this.GetParameters(parameters));
        }

        /// <summary>
        /// 获取日期时间结果(使用参数数组)
        /// </summary>
        public virtual DateTime GetDateTime(string sql, params IReadOnlyCollection<SugarParameter> parameters)
        {
            return GetScalar(sql, parameters).ObjToDate();
        }

        /// <summary>
        /// 异步获取日期时间结果(使用对象参数)
        /// </summary>
        public virtual Task<DateTime> GetDateTimeAsync(string sql, object parameters)
        {
            return GetDateTimeAsync(sql, this.GetParameters(parameters));
        }

        /// <summary>
        /// 异步获取日期时间结果(使用参数数组)
        /// </summary>
        public virtual async Task<DateTime> GetDateTimeAsync(string sql, params IReadOnlyCollection<SugarParameter> parameters)
        {
            var list = await GetScalarAsync(sql, parameters).ConfigureAwait(false);
            return list.ObjToDate();
        }

        #endregion

        #region Query Methods

        /// <summary>
        /// 执行SQL查询并返回对象列表(使用对象参数)
        /// </summary>
        public virtual List<T> SqlQuery<T>(string sql, object parameters = null)
        {
            var sugarParameters = this.GetParameters(parameters);
            return SqlQuery<T>(sql, sugarParameters);
        }

        /// <summary>
        /// 执行SQL查询并返回对象列表(使用参数数组)
        /// </summary>
        public virtual List<T> SqlQuery<T>(string sql, params IReadOnlyCollection<SugarParameter> parameters)
        {
            var result = SqlQuery<T, object, object, object, object, object, object>(sql, parameters);
            return result.Item1;
        }

        /// <summary>
        /// 强制使用主库执行SQL查询
        /// </summary>
        public List<T> MasterSqlQuery<T>(string sql, object parameters = null)
        {
            var oldValue = this.Context.Ado.IsDisableMasterSlaveSeparation;
            this.Context.Ado.IsDisableMasterSlaveSeparation = true;
            var result = this.Context.Ado.SqlQuery<T>(sql, parameters);
            this.Context.Ado.IsDisableMasterSlaveSeparation = oldValue;
            return result;
        }

        /// <summary>
        /// 异步强制使用主库执行SQL查询
        /// </summary>
        public async Task<List<T>> MasterSqlQueryAasync<T>(string sql, object parameters = null)
        {
            var oldValue = this.Context.Ado.IsDisableMasterSlaveSeparation;
            this.Context.Ado.IsDisableMasterSlaveSeparation = true;
            var result = await Context.Ado.SqlQueryAsync<T>(sql, parameters).ConfigureAwait(false);
            this.Context.Ado.IsDisableMasterSlaveSeparation = oldValue;
            return result;
        }

        /// <summary>
        /// 执行SQL查询并返回两个结果集
        /// </summary>
        public Tuple<List<T>, List<T2>> SqlQuery<T, T2>(string sql, object parameters = null)
        {
            var result = SqlQuery<T, T2, object, object, object, object, object>(sql, parameters);
            return new Tuple<List<T>, List<T2>>(result.Item1, result.Item2);
        }

        /// <summary>
        /// 执行SQL查询并返回三个结果集
        /// </summary>
        public Tuple<List<T>, List<T2>, List<T3>> SqlQuery<T, T2, T3>(string sql, object parameters = null)
        {
            var result = SqlQuery<T, T2, T3, object, object, object, object>(sql, parameters);
            return new Tuple<List<T>, List<T2>, List<T3>>(result.Item1, result.Item2, result.Item3);
        }

        /// <summary>
        /// 执行SQL查询并返回四个结果集
        /// </summary>
        public Tuple<List<T>, List<T2>, List<T3>, List<T4>> SqlQuery<T, T2, T3, T4>(string sql, object parameters = null)
        {
            var result = SqlQuery<T, T2, T3, T4, object, object, object>(sql, parameters);
            return new Tuple<List<T>, List<T2>, List<T3>, List<T4>>(result.Item1, result.Item2, result.Item3, result.Item4);
        }

        /// <summary>
        /// 执行SQL查询并返回五个结果集
        /// </summary>
        public Tuple<List<T>, List<T2>, List<T3>, List<T4>, List<T5>> SqlQuery<T, T2, T3, T4, T5>(string sql, object parameters = null)
        {
            var result = SqlQuery<T, T2, T3, T4, T5, object, object>(sql, parameters);
            return new Tuple<List<T>, List<T2>, List<T3>, List<T4>, List<T5>>(result.Item1, result.Item2, result.Item3, result.Item4, result.Item5);
        }

        /// <summary>
        /// 执行SQL查询并返回六个结果集
        /// </summary>
        public Tuple<List<T>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>> SqlQuery<T, T2, T3, T4, T5, T6>(string sql, object parameters = null)
        {
            var result = SqlQuery<T, T2, T3, T4, T5, T6, object>(sql, parameters);
            return new Tuple<List<T>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>>(result.Item1, result.Item2, result.Item3, result.Item4, result.Item5, result.Item6);
        }

        /// <summary>
        /// 执行SQL查询并返回七个结果集
        /// </summary>
        public virtual Tuple<List<T>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>, List<T7>> SqlQuery<T, T2, T3, T4, T5, T6, T7>(string sql, object parameters = null)
        {
            var parsmeterArray = this.GetParameters(parameters);
            this.Context.InitMappingInfo<T>();
            var builder = InstanceFactory.GetSqlbuilder(this.Context.CurrentConnectionConfig);
            builder.SqlQueryBuilder.sql.Append(sql);
            if (parsmeterArray != null && parsmeterArray.Count != 0)
                builder.SqlQueryBuilder.Parameters.AddRange(parsmeterArray);
            string sqlString = builder.SqlQueryBuilder.ToSqlString();
            IReadOnlyCollection<SugarParameter> Parameters = builder.SqlQueryBuilder.Parameters;
            this.GetDataBefore(sqlString, Parameters);
            using (var dataReader = this.GetDataReader(sqlString, Parameters))
            {
                DbDataReader DbReader = (DbDataReader)dataReader;
                List<T> result = new List<T>();
                if (DbReader.HasRows)
                {
                    result = GetData<T>(typeof(T), dataReader);
                }
                else
                {
                    dataReader.Read();
                }
                List<T2> result2 = null;
                if (NextResult(dataReader))
                {
                    this.Context.InitMappingInfo<T2>();
                    result2 = GetData<T2>(typeof(T2), dataReader);
                }
                List<T3> result3 = null;
                if (NextResult(dataReader))
                {
                    this.Context.InitMappingInfo<T3>();
                    result3 = GetData<T3>(typeof(T3), dataReader);
                }
                List<T4> result4 = null;
                if (NextResult(dataReader))
                {
                    this.Context.InitMappingInfo<T4>();
                    result4 = GetData<T4>(typeof(T4), dataReader);
                }
                List<T5> result5 = null;
                if (NextResult(dataReader))
                {
                    this.Context.InitMappingInfo<T5>();
                    result5 = GetData<T5>(typeof(T5), dataReader);
                }
                List<T6> result6 = null;
                if (NextResult(dataReader))
                {
                    this.Context.InitMappingInfo<T6>();
                    result6 = GetData<T6>(typeof(T6), dataReader);
                }
                List<T7> result7 = null;
                if (NextResult(dataReader))
                {
                    this.Context.InitMappingInfo<T7>();
                    result7 = GetData<T7>(typeof(T7), dataReader);
                }
                builder.SqlQueryBuilder.Clear();
                if (this.Context.Ado.DataReaderParameters != null)
                {
                    foreach (IDataParameter item in this.Context.Ado.DataReaderParameters)
                    {
                        var parameter = parsmeterArray.FirstOrDefault(it => item.ParameterName.Substring(1) == it.ParameterName.Substring(1));
                        if (parameter != null)
                        {
                            parameter.Value = item.Value;
                        }
                    }
                    this.Context.Ado.DataReaderParameters = null;
                }
                this.GetDataAfter(sqlString, Parameters);
                return Tuple.Create<List<T>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>, List<T7>>(result, result2, result3, result4, result5, result6, result7);
            }
        }
        #region Async Query Methods

        /// <summary>
        /// 异步执行SQL查询并返回对象列表(带取消令牌)
        /// </summary>
        public Task<List<T>> SqlQueryAsync<T>(string sql, object parameters, CancellationToken token)
        {
            this.CancellationToken = token;
            return SqlQueryAsync<T>(sql, parameters);
        }

        /// <summary>
        /// 异步执行SQL查询并返回对象列表(使用对象参数)
        /// </summary>
        public virtual Task<List<T>> SqlQueryAsync<T>(string sql, object parameters = null)
        {
            var sugarParameters = this.GetParameters(parameters);
            return SqlQueryAsync<T>(sql, sugarParameters);
        }

        /// <summary>
        /// 异步执行SQL查询并返回对象列表(使用参数数组)
        /// </summary>
        public virtual async Task<List<T>> SqlQueryAsync<T>(string sql, params IReadOnlyCollection<SugarParameter> parameters)
        {
            var result = await SqlQueryAsync<T, object, object, object, object, object, object>(sql, parameters).ConfigureAwait(false);
            return result.Item1;
        }

        /// <summary>
        /// 异步执行SQL查询并返回两个结果集
        /// </summary>
        public async Task<Tuple<List<T>, List<T2>>> SqlQueryAsync<T, T2>(string sql, object parameters = null)
        {
            var result = await SqlQueryAsync<T, T2, object, object, object, object, object>(sql, parameters).ConfigureAwait(false);
            return new Tuple<List<T>, List<T2>>(result.Item1, result.Item2);
        }

        /// <summary>
        /// 异步执行SQL查询并返回三个结果集
        /// </summary>
        public async Task<Tuple<List<T>, List<T2>, List<T3>>> SqlQueryAsync<T, T2, T3>(string sql, object parameters = null)
        {
            var result = await SqlQueryAsync<T, T2, T3, object, object, object, object>(sql, parameters).ConfigureAwait(false);
            return new Tuple<List<T>, List<T2>, List<T3>>(result.Item1, result.Item2, result.Item3);
        }

        /// <summary>
        /// 异步执行SQL查询并返回四个结果集
        /// </summary>
        public async Task<Tuple<List<T>, List<T2>, List<T3>, List<T4>>> SqlQueryAsync<T, T2, T3, T4>(string sql, object parameters = null)
        {
            var result = await SqlQueryAsync<T, T2, T3, T4, object, object, object>(sql, parameters).ConfigureAwait(false);
            return new Tuple<List<T>, List<T2>, List<T3>, List<T4>>(result.Item1, result.Item2, result.Item3, result.Item4);
        }

        /// <summary>
        /// 异步执行SQL查询并返回五个结果集
        /// </summary>
        public async Task<Tuple<List<T>, List<T2>, List<T3>, List<T4>, List<T5>>> SqlQueryAsync<T, T2, T3, T4, T5>(string sql, object parameters = null)
        {
            var result = await SqlQueryAsync<T, T2, T3, T4, T5, object, object>(sql, parameters).ConfigureAwait(false);
            return new Tuple<List<T>, List<T2>, List<T3>, List<T4>, List<T5>>(result.Item1, result.Item2, result.Item3, result.Item4, result.Item5);
        }

        /// <summary>
        /// 异步执行SQL查询并返回六个结果集
        /// </summary>
        public async Task<Tuple<List<T>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>>> SqlQueryAsync<T, T2, T3, T4, T5, T6>(string sql, object parameters = null)
        {
            var result = await SqlQueryAsync<T, T2, T3, T4, T5, T6, object>(sql, parameters).ConfigureAwait(false);
            return new Tuple<List<T>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>>(result.Item1, result.Item2, result.Item3, result.Item4, result.Item5, result.Item6);
        }

        /// <summary>
        /// 异步执行SQL查询并返回七个结果集
        /// </summary>
        public virtual async Task<Tuple<List<T>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>, List<T7>>> SqlQueryAsync<T, T2, T3, T4, T5, T6, T7>(string sql, object parameters = null)
        {
            var parsmeterArray = this.GetParameters(parameters);
            this.Context.InitMappingInfo<T>();
            var builder = InstanceFactory.GetSqlbuilder(this.Context.CurrentConnectionConfig);
            builder.SqlQueryBuilder.sql.Append(sql);
            if (parsmeterArray != null && parsmeterArray.Count != 0)
                builder.SqlQueryBuilder.Parameters.AddRange(parsmeterArray);
            string sqlString = builder.SqlQueryBuilder.ToSqlString();
            IReadOnlyCollection<SugarParameter> Parameters = builder.SqlQueryBuilder.Parameters;
            this.GetDataBefore(sqlString, Parameters);
            using (var dataReader = await GetDataReaderAsync(sqlString, Parameters).ConfigureAwait(false))
            {
                DbDataReader DbReader = (DbDataReader)dataReader;
                List<T> result = new List<T>();
                if (DbReader.HasRows)
                {
                    result = await GetDataAsync<T>(typeof(T), dataReader).ConfigureAwait(false);
                }
                List<T2> result2 = null;
                if (NextResult(dataReader))
                {
                    this.Context.InitMappingInfo<T2>();
                    result2 = await GetDataAsync<T2>(typeof(T2), dataReader).ConfigureAwait(false);
                }
                List<T3> result3 = null;
                if (NextResult(dataReader))
                {
                    this.Context.InitMappingInfo<T3>();
                    result3 = await GetDataAsync<T3>(typeof(T3), dataReader).ConfigureAwait(false);
                }
                List<T4> result4 = null;
                if (NextResult(dataReader))
                {
                    this.Context.InitMappingInfo<T4>();
                    result4 = await GetDataAsync<T4>(typeof(T4), dataReader).ConfigureAwait(false);
                }
                List<T5> result5 = null;
                if (NextResult(dataReader))
                {
                    this.Context.InitMappingInfo<T5>();
                    result5 = await GetDataAsync<T5>(typeof(T5), dataReader).ConfigureAwait(false);
                }
                List<T6> result6 = null;
                if (NextResult(dataReader))
                {
                    this.Context.InitMappingInfo<T6>();
                    result6 = await GetDataAsync<T6>(typeof(T6), dataReader).ConfigureAwait(false);
                }
                List<T7> result7 = null;
                if (NextResult(dataReader))
                {
                    this.Context.InitMappingInfo<T7>();
                    result7 = await GetDataAsync<T7>(typeof(T7), dataReader).ConfigureAwait(false);
                }
                builder.SqlQueryBuilder.Clear();
                if (this.Context.Ado.DataReaderParameters != null)
                {
                    foreach (IDataParameter item in this.Context.Ado.DataReaderParameters)
                    {
                        var parameter = parsmeterArray.FirstOrDefault(it => item.ParameterName.Substring(1) == it.ParameterName.Substring(1));
                        if (parameter != null)
                        {
                            parameter.Value = item.Value;
                        }
                    }
                    this.Context.Ado.DataReaderParameters = null;
                }
                this.GetDataAfter(sqlString, Parameters);
                return Tuple.Create<List<T>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>, List<T7>>(result, result2, result3, result4, result5, result6, result7);
            }
        }

        #endregion

        /// <summary>
        /// 执行SQL查询并返回单个对象(使用对象参数)
        /// </summary>
        public virtual T SqlQuerySingle<T>(string sql, object parameters = null)
        {
            var result = SqlQuery<T>(sql, parameters);
            return result == null ? default(T) : result.FirstOrDefault();
        }

        /// <summary>
        /// 执行SQL查询并返回单个对象(使用参数数组)
        /// </summary>
        public virtual T SqlQuerySingle<T>(string sql, params IReadOnlyCollection<SugarParameter> parameters)
        {
            var result = SqlQuery<T>(sql, parameters);
            return result == null ? default(T) : result.FirstOrDefault();
        }

        /// <summary>
        /// 异步执行SQL查询并返回单个对象(使用对象参数)
        /// </summary>
        public virtual async Task<T> SqlQuerySingleAsync<T>(string sql, object parameters = null)
        {
            var result = await SqlQueryAsync<T>(sql, parameters).ConfigureAwait(false);
            return result == null ? default(T) : result.FirstOrDefault();
        }

        /// <summary>
        /// 异步执行SQL查询并返回单个对象(使用参数数组)
        /// </summary>
        public virtual async Task<T> SqlQuerySingleAsync<T>(string sql, params IReadOnlyCollection<SugarParameter> parameters)
        {
            var result = await SqlQueryAsync<T>(sql, parameters).ConfigureAwait(false);
            return result == null ? default(T) : result.FirstOrDefault();
        }

        #endregion

        #region DataTable Methods

        /// <summary>
        /// 获取DataTable(使用参数数组)
        /// </summary>
        public virtual DataTable GetDataTable(string sql, params IReadOnlyCollection<SugarParameter> parameters)
        {
            var ds = GetDataSetAll(sql, parameters);
            if (ds.Tables.Count != 0 && ds.Tables.Count > 0) return ds.Tables[0];
            return new DataTable();
        }

        /// <summary>
        /// 获取DataTable(使用对象参数)
        /// </summary>
        public virtual DataTable GetDataTable(string sql, object parameters)
        {
            return GetDataTable(sql, this.GetParameters(parameters));
        }

        /// <summary>
        /// 异步获取DataTable(使用参数数组)
        /// </summary>
        public virtual async Task<DataTable> GetDataTableAsync(string sql, params IReadOnlyCollection<SugarParameter> parameters)
        {
            var ds = await GetDataSetAllAsync(sql, parameters).ConfigureAwait(false);
            if (ds.Tables.Count != 0 && ds.Tables.Count > 0) return ds.Tables[0];
            return new DataTable();
        }

        /// <summary>
        /// 异步获取DataTable(使用对象参数)
        /// </summary>
        public virtual Task<DataTable> GetDataTableAsync(string sql, object parameters)
        {
            return GetDataTableAsync(sql, this.GetParameters(parameters));
        }

        #endregion

        #region DataSet Methods

        /// <summary>
        /// 获取DataSet(使用对象参数)
        /// </summary>
        public virtual DataSet GetDataSetAll(string sql, object parameters)
        {
            return GetDataSetAll(sql, this.GetParameters(parameters));
        }

        /// <summary>
        /// 异步获取DataSet(使用对象参数)
        /// </summary>
        public virtual Task<DataSet> GetDataSetAllAsync(string sql, object parameters)
        {
            return GetDataSetAllAsync(sql, this.GetParameters(parameters));
        }

        #endregion

        #region DataReader Methods

        /// <summary>
        /// 获取DataReader(使用对象参数)
        /// </summary>
        public virtual IDataReader GetDataReader(string sql, object parameters)
        {
            return GetDataReader(sql, this.GetParameters(parameters));
        }

        /// <summary>
        /// 异步获取DataReader(使用对象参数)
        /// </summary>
        public virtual Task<IDataReader> GetDataReaderAsync(string sql, object parameters)
        {
            return GetDataReaderAsync(sql, this.GetParameters(parameters));
        }

        #endregion

        #region Scalar Methods

        /// <summary>
        /// 获取标量值(使用对象参数)
        /// </summary>
        public virtual object GetScalar(string sql, object parameters)
        {
            return GetScalar(sql, this.GetParameters(parameters));
        }
        public virtual Task<object> GetScalarAsync(string sql, object parameters, CancellationToken cancellationToken)
        {
            this.CancellationToken = cancellationToken;
            return GetScalarAsync(sql, this.GetParameters(parameters));
        }
        /// <summary>
        /// 异步获取标量值(使用对象参数)
        /// </summary>
        public virtual Task<object> GetScalarAsync(string sql, object parameters)
        {
            return GetScalarAsync(sql, this.GetParameters(parameters));
        }

        #endregion

        #region Execute Command Methods

        /// <summary>
        /// 执行命令(使用对象参数)
        /// </summary>
        public virtual int ExecuteCommand(string sql, object parameters)
        {
            return ExecuteCommand(sql, GetParameters(parameters));
        }

        /// <summary>
        /// 异步执行命令(带取消令牌)
        /// </summary>
        public Task<int> ExecuteCommandAsync(string sql, object parameters, CancellationToken cancellationToken)
        {
            this.CancellationToken = CancellationToken;
            return ExecuteCommandAsync(sql, parameters);
        }

        /// <summary>
        /// 异步执行命令(使用对象参数)
        /// </summary>
        public virtual Task<int> ExecuteCommandAsync(string sql, object parameters)
        {
            return ExecuteCommandAsync(sql, GetParameters(parameters));
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 异步获取命令对象(需要子类实现)
        /// </summary>
        public virtual async Task<DbCommand> GetCommandAsync(string sql, IReadOnlyCollection<SugarParameter> parameters)
        {
            await Task.FromResult(0).ConfigureAwait(false);
            throw new NotImplementedException();
        }

        /// <summary>
        /// 异步关闭连接
        /// </summary>
        public async Task CloseAsync()
        {
            if (this.Transaction != null)
            {
                this.Transaction = null;
            }
            if (this.Connection != null && this.Connection.State == ConnectionState.Open)
            {
                if (IsOpenAsync)
                    await (Connection as DbConnection).CloseAsync().ConfigureAwait(false);
                else
#pragma warning disable CA1849
                    (Connection as DbConnection).Close();
#pragma warning restore CA1849
            }
            if (this.IsMasterSlaveSeparation && this.SlaveConnections.HasValue())
            {
                foreach (var slaveConnection in this.SlaveConnections)
                {
                    if (slaveConnection != null && slaveConnection.State == ConnectionState.Open)
                    {
                        if (IsOpenAsync)
                            await (Connection as DbConnection).CloseAsync().ConfigureAwait(false);
                        else
#pragma warning disable CA1849
                            (Connection as DbConnection).Close();
#pragma warning restore CA1849
                    }
                }
            }
        }

        /// <summary>
        /// 异常处理
        /// </summary>
        protected virtual void SugarCatch(Exception ex, string sql, IReadOnlyCollection<SugarParameter> parameters)
        {
            if (sql?.Contains("{year}{month}{day}") == true)
            {
                Check.ExceptionEasy("need .SplitTable() message:" + ex.Message, "当前代码是否缺少 .SplitTable() ,可以看文档 [分表]  , 详细错误:" + ex.Message);
            }
        }

        /// <summary>
        /// 移除取消令牌
        /// </summary>
        public virtual void RemoveCancellationToken()
        {
            this.CancellationToken = null;
        }

        /// <summary>
        /// 设置异步上下文ID
        /// </summary>
        protected void Async()
        {
            if (this.Context.Root != null && this.Context.Root.AsyncId == null)
            {
                this.Context.Root.AsyncId = Guid.NewGuid();
            }
        }

        /// <summary>
        /// 安全地移动到下一个结果集
        /// </summary>
        protected bool NextResult(IDataReader dataReader)
        {
            try
            {
                return dataReader.NextResult();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 处理执行前的SQL
        /// </summary>
        protected void ExecuteProcessingSQL(ref string sql, ref IReadOnlyCollection<SugarParameter> parameters)
        {
            var result = this.ProcessingEventStartingSQL(sql, parameters);
            sql = result.Key;
            parameters = result.Value;
        }

        /// <summary>
        /// 执行前的操作
        /// </summary>
        public virtual void ExecuteBefore(string sql, IReadOnlyCollection<SugarParameter> parameters)
        {
            this.BeforeTime = DateTime.Now;
            if (this.IsEnableLogEvent)
            {
                Action<string, IReadOnlyCollection<SugarParameter>> action = LogEventStarting;
                if (action != null)
                {
                    if (parameters == null || parameters.Count == 0)
                    {
                        action(sql, Array.Empty<SugarParameter>());
                    }
                    else
                    {
                        action(sql, parameters);
                    }
                }
            }
        }
        private static readonly HashSet<ParameterDirection> Keys = new HashSet<ParameterDirection>()
        {
            ParameterDirection.Output, ParameterDirection.InputOutput, ParameterDirection.ReturnValue
        };
        /// <summary>
        /// 执行后的操作
        /// </summary>
        public virtual void ExecuteAfter(string sql, IReadOnlyCollection<SugarParameter> parameters)
        {
            this.AfterTime = DateTime.Now;
            var hasParameter = parameters.HasValue();
            if (hasParameter)
            {
                foreach (var outputParameter in parameters.Where(it => Keys.Contains(it.Direction)))
                {
                    var gobalOutputParamter = this.OutputParameters.FirstOrDefault(it => it.ParameterName == outputParameter.ParameterName);
                    if (gobalOutputParamter == null)
                    {//Oracle bug
                        gobalOutputParamter = this.OutputParameters.FirstOrDefault(it => it.ParameterName == outputParameter.ParameterName.TrimStart(outputParameter.ParameterName[0]));
                    }
                    outputParameter.Value = gobalOutputParamter.Value;
                    this.OutputParameters.Remove(gobalOutputParamter);
                }
            }
            if (this.IsEnableLogEvent)
            {
                Action<string, IReadOnlyCollection<SugarParameter>> action = LogEventCompleted;
                if (action != null)
                {
                    if (parameters == null || parameters.Count == 0)
                    {
                        action(sql, Array.Empty<SugarParameter>());
                    }
                    else
                    {
                        action(sql, parameters);
                    }
                }
            }
            if (this.OldCommandType != 0)
            {
                this.CommandType = this.OldCommandType;
                this.IsClearParameters = this.OldClearParameters;
                this.OldCommandType = 0;
                this.OldClearParameters = false;
            }
        }
        #region Data Access Event Methods

        /// <summary>
        /// 数据获取前事件处理
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数数组</param>
        public virtual void GetDataBefore(string sql, IReadOnlyCollection<SugarParameter> parameters)
        {
            this.GetDataBeforeTime = DateTime.Now;
            if (this.IsEnableLogEvent)
            {
                Action<string, IReadOnlyCollection<SugarParameter>> action = OnGetDataReadering;
                if (action != null)
                {
                    if (parameters == null || parameters.Count == 0)
                    {
                        action(sql, Array.Empty<SugarParameter>());
                    }
                    else
                    {
                        action(sql, parameters);
                    }
                }
            }
        }

        /// <summary>
        /// 数据获取后事件处理
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数数组</param>
        public virtual void GetDataAfter(string sql, IReadOnlyCollection<SugarParameter> parameters)
        {
            this.GetDataAfterTime = DateTime.Now;
            if (this.IsEnableLogEvent)
            {
                Action<string, IReadOnlyCollection<SugarParameter>, TimeSpan> action = OnGetDataReadered;
                if (action != null)
                {
                    if (parameters == null || parameters.Count == 0)
                    {
                        action(sql, Array.Empty<SugarParameter>(), GetDataExecutionTime);
                    }
                    else
                    {
                        action(sql, parameters, GetDataExecutionTime);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// 获取参数数组
        /// </summary>
        /// <param name="parameters">参数对象</param>
        /// <param name="propertyInfo">属性信息数组</param>
        /// <returns>SugarParameter数组</returns>
        public virtual IReadOnlyCollection<SugarParameter> GetParameters(object parameters, PropertyInfo[] propertyInfo = null)
        {
            if (parameters == null) return Array.Empty<SugarParameter>();
            return base.GetParameters(parameters, propertyInfo, this.SqlParameterKeyWord);
        }

        #endregion

        #region Connection Management

        /// <summary>
        /// 检查是否自动关闭连接
        /// </summary>
        /// <returns>是否自动关闭</returns>
        protected bool IsAutoClose()
        {
            return this.Context.CurrentConnectionConfig.IsAutoCloseConnection && this.Transaction == null;
        }

        /// <summary>
        /// 检查是否启用主从分离
        /// </summary>
        protected bool IsMasterSlaveSeparation
        {
            get
            {
                return this.Context.CurrentConnectionConfig.SlaveConnectionConfigs.HasValue() &&
                       this.IsDisableMasterSlaveSeparation == false;
            }
        }

        /// <summary>
        /// 设置连接开始(主从分离处理)
        /// </summary>
        /// <param name="sql">SQL语句</param>
        protected void SetConnectionStart(string sql)
        {
            if (this.Transaction == null && this.IsMasterSlaveSeparation && IsRead(sql))
            {
                if (this.MasterConnection == null)
                {
                    this.MasterConnection = this.Connection;
                    this.MasterConnectionString = this.MasterConnection.ConnectionString;
                }
                var saves = this.Context.CurrentConnectionConfig.SlaveConnectionConfigs
                    .Where(it => it.HitRate > 0)
                    .ToArray();

                var currentIndex = UtilRandom.GetRandomIndex(
                    saves.Select((it, idx) => new { idx, it.HitRate })
                         .ToDictionary(x => x.idx, x => x.HitRate));

                var currentSaveConnection = saves[currentIndex];
                this.Connection = null;
                this.Context.CurrentConnectionConfig.ConnectionString = currentSaveConnection.ConnectionString;
                this.Connection = this.Connection;
                if (this.SlaveConnections.IsNullOrEmpty() ||
                    !this.SlaveConnections.Any(it => EqualsConnectionString(it.ConnectionString, this.Connection.ConnectionString)))
                {
                    if (this.SlaveConnections == null) this.SlaveConnections = new List<IDbConnection>();
                    this.SlaveConnections.Add(this.Connection);
                }
            }
            else if (this.Transaction == null && this.IsMasterSlaveSeparation && this.MasterConnection == null)
            {
                this.MasterConnection = this.Connection;
                this.MasterConnectionString = this.MasterConnection.ConnectionString;
            }
        }

        /// <summary>
        /// 比较两个连接字符串是否相等
        /// </summary>
        private bool EqualsConnectionString(string connectionString1, string connectionString2)
        {
            var connectionString1Array = connectionString1.Split(';');
            var connectionString2Array = connectionString2.Split(';');
            var result = connectionString1Array.Except(connectionString2Array);
            return !result.Any();
        }

        /// <summary>
        /// 检查是否需要格式化SQL
        /// </summary>
        private bool IsFormat(IReadOnlyCollection<SugarParameter> parameters)
        {
            return FormatSql != null && parameters?.Count > 0;
        }

        /// <summary>
        /// 设置连接结束(主从分离处理)
        /// </summary>
        /// <param name="sql">SQL语句</param>
        protected void SetConnectionEnd(string sql)
        {
            if (this.IsMasterSlaveSeparation && IsRead(sql) && this.Transaction == null)
            {
                this.Connection = this.MasterConnection;
                this.Context.CurrentConnectionConfig.ConnectionString = this.MasterConnectionString;
            }
            this.Context.SugarActionType = SugarActionType.UnKnown;
        }

        /// <summary>
        /// 检查是否为只读SQL
        /// </summary>
        private bool IsRead(string sql)
        {
            var sqlLower = sql.ToLower();
            var result = Regex.IsMatch(sqlLower, "[ ]*select[ ]") &&
                        !Regex.IsMatch(sqlLower, "[ ]*insert[ ]|[ ]*update[ ]|[ ]*delete[ ]");
            return result;
        }

        #endregion

        #region Error Handling

        /// <summary>
        /// 执行错误事件
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数数组</param>
        /// <param name="ex">异常对象</param>
        protected void ExecuteErrorEvent(string sql, IReadOnlyCollection<SugarParameter> parameters, Exception ex)
        {
            this.AfterTime = DateTime.Now;
            ErrorEvent(new SqlSugarException(this.Context, ex, sql, parameters));
        }

        #endregion

        #region Parameter Initialization

        /// <summary>
        /// 初始化参数
        /// </summary>
        /// <param name="sql">SQL语句引用</param>
        /// <param name="parameters">参数数组</param>
        protected void InitParameters(ref string sql, IReadOnlyCollection<SugarParameter> parameters)
        {
            this.SqlExecuteCount = 0;
            this.BeforeTime = DateTime.MinValue;
            this.AfterTime = DateTime.MinValue;

            if (parameters.HasValue())
            {
                foreach (var item in parameters)
                {
                    if (item.Value != null)
                    {
                        var type = item.Value.GetType();
                        // 处理数组参数
                        if ((type != UtilConstants.ByteArrayType && type.IsArray && item.IsArray == false) ||
                            type.FullName.IsCollectionsList() || type.IsIterator())
                        {
                            var newValues = new List<string>();
                            foreach (var inValute in item.Value as IEnumerable)
                            {
                                newValues.Add(inValute.ObjToString());
                            }
                            if (newValues.IsNullOrEmpty())
                            {
                                newValues.Add("-1");
                            }

                            // 处理不同格式的参数名
                            if (item.ParameterName.Substring(0, 1) == ":")
                            {
                                sql = sql.Replace(string.Concat("@", item.ParameterName.AsSpan(1)),
                                    newValues.ToJoinSqlInVals());
                            }
                            if (item.ParameterName.Substring(0, 1) != this.SqlParameterKeyWord &&
                                sql.ObjToString().Contains(this.SqlParameterKeyWord + item.ParameterName))
                            {
                                sql = sql.Replace(this.SqlParameterKeyWord + item.ParameterName,
                                    newValues.ToJoinSqlInVals());
                            }
                            else if (item.Value != null && UtilMethods.IsNumberArray(item.Value.GetType()))
                            {
                                if (newValues.Any(it => string.IsNullOrEmpty(it)))
                                {
                                    newValues.RemoveAll(r => string.IsNullOrEmpty(r));
                                    newValues.Add("null");
                                }
                                sql = sql.Replace(item.ParameterName, string.Join(",", newValues));
                            }
                            else
                            {
                                sql = sql.Replace(item.ParameterName, newValues.ToJoinSqlInVals());
                            }
                            item.Value = DBNull.Value;
                        }
                    }

                    // 处理参数名中的空格
                    if (item.ParameterName?.Contains(' ') == true)
                    {
                        var oldName = item.ParameterName;
                        item.ParameterName = item.ParameterName.Replace(" ", "");
                        sql = sql.Replace(oldName, item.ParameterName);
                    }
                }
            }
        }

        #endregion

        #region Data Conversion

        /// <summary>
        /// 将DataReader转换为指定类型列表
        /// </summary>
        /// <typeparam name="TResult">结果类型</typeparam>
        /// <param name="entityType">实体类型</param>
        /// <param name="dataReader">数据读取器</param>
        /// <returns>结果列表</returns>
        protected List<TResult> GetData<TResult>(Type entityType, IDataReader dataReader)
        {
            List<TResult> result;
            if (entityType == UtilConstants.DynamicType)
            {
                result = this.Context.Utilities.DataReaderToExpandoObjectListNoUsing(dataReader) as List<TResult>;
            }
            else if (entityType == UtilConstants.ObjType)
            {
                result = this.Context.Utilities.DataReaderToExpandoObjectListNoUsing(dataReader)
                    .Select(it => ((TResult)(object)it)).ToList();
            }
            else if (entityType.IsAnonymousType() || StaticConfig.EnableAot)
            {
                if (StaticConfig.EnableAot && entityType == UtilConstants.StringType)
                {
                    result = this.Context.Ado.DbBind.DataReaderToListNoUsing<TResult>(entityType, dataReader);
                }
                else
                {
                    result = this.Context.Utilities.DataReaderToListNoUsing<TResult>(dataReader);
                }
            }
            else
            {
                result = this.Context.Ado.DbBind.DataReaderToListNoUsing<TResult>(entityType, dataReader);
            }
            return result;
        }

        /// <summary>
        /// 异步将DataReader转换为指定类型列表
        /// </summary>
        /// <typeparam name="TResult">结果类型</typeparam>
        /// <param name="entityType">实体类型</param>
        /// <param name="dataReader">数据读取器</param>
        /// <returns>结果列表任务</returns>
        protected async Task<List<TResult>> GetDataAsync<TResult>(Type entityType, IDataReader dataReader)
        {
            List<TResult> result;
            if (entityType == UtilConstants.DynamicType)
            {
                result = await Context.Utilities.DataReaderToExpandoObjectListAsyncNoUsing(dataReader)
                    .ConfigureAwait(false) as List<TResult>;
            }
            else if (entityType == UtilConstants.ObjType)
            {
                var list = await Context.Utilities.DataReaderToExpandoObjectListAsyncNoUsing(dataReader)
                    .ConfigureAwait(false);
                result = list.Select(it => ((TResult)(object)it)).ToList();
            }
            else if (entityType.IsAnonymousType() || StaticConfig.EnableAot)
            {
                if (StaticConfig.EnableAot && entityType == UtilConstants.StringType)
                {
                    result = await Context.Ado.DbBind.DataReaderToListNoUsingAsync<TResult>(entityType, dataReader)
                        .ConfigureAwait(false);
                }
                else
                {
                    result = await Context.Utilities.DataReaderToListAsyncNoUsing<TResult>(dataReader)
                        .ConfigureAwait(false);
                }
            }
            else
            {
                result = await Context.Ado.DbBind.DataReaderToListNoUsingAsync<TResult>(entityType, dataReader)
                    .ConfigureAwait(false);
            }
            return result;
        }

        #endregion
    }
}
