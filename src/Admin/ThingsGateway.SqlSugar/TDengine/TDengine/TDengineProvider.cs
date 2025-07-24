using System.Data;
using System.Data.Common;

using TDengineAdo;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// TDengine 数据提供程序
    /// </summary>
    public partial class TDengineProvider : AdoProvider
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public TDengineProvider()
        {
        }

        /// <summary>
        /// 获取或设置数据库连接
        /// </summary>
        public override IDbConnection Connection
        {
            get
            {
                if (base._DbConnection == null)
                {
                    try
                    {
                        var TDengineConnectionString = base.Context.CurrentConnectionConfig.ConnectionString;
                        base._DbConnection = new TDengineConnection(TDengineConnectionString);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                return base._DbConnection;
            }
            set
            {
                base._DbConnection = value;
            }
        }

        /// <summary>
        /// 开始事务
        /// </summary>
        public override void BeginTran()
        {
        }

        /// <summary>
        /// 使用指定事务名称开始事务
        /// </summary>
        /// <param name="transactionName">事务名称</param>
        public override void BeginTran(string transactionName)
        {
        }

        /// <summary>
        /// 使用指定隔离级别和事务名称开始事务
        /// </summary>
        /// <param name="iso">隔离级别</param>
        /// <param name="transactionName">事务名称</param>
        public override void BeginTran(IsolationLevel iso, string transactionName)
        {
        }

        /// <summary>
        /// 获取数据适配器
        /// </summary>
        /// <returns>数据适配器</returns>
        public override IDataAdapter GetAdapter()
        {
            return new TDengineDataAdapter();
        }

        /// <summary>
        /// 获取数据库命令
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数数组</param>
        /// <returns>数据库命令</returns>
        public override DbCommand GetCommand(string sql, IReadOnlyCollection<SugarParameter> parameters)
        {
            TDengineCommand sqlCommand = new TDengineCommand(sql, (TDengineConnection)this.Connection);
            sqlCommand.CommandType = this.CommandType;
            sqlCommand.CommandTimeout = this.CommandTimeOut;
            if (parameters.HasValue())
            {
                IDataParameter[] ipars = ToIDbDataParameter(parameters);
                sqlCommand.Parameters.AddRange((TDengineParameter[])ipars);
            }
            CheckConnection();
            return sqlCommand;
        }

        /// <summary>
        /// 设置命令到适配器
        /// </summary>
        /// <param name="dataAdapter">数据适配器</param>
        /// <param name="command">数据库命令</param>
        public override void SetCommandToAdapter(IDataAdapter dataAdapter, DbCommand command)
        {
            ((TDengineDataAdapter)dataAdapter).SelectCommand = (TDengineCommand)command;
        }

        /// <summary>
        /// 是否使用纳秒精度
        /// </summary>
        public static bool _IsIsNanosecond { get; set; }

        /// <summary>
        /// 是否使用微秒精度
        /// </summary>
        public static bool _IsMicrosecond { get; set; }

        /// <summary>
        /// 将SugarParameter数组转换为IDataParameter数组
        /// </summary>
        /// <param name="parameters">SugarParameter参数数组</param>
        /// <returns>IDataParameter参数数组</returns>
        public override IDataParameter[] ToIDbDataParameter(params IReadOnlyCollection<SugarParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0) return null;
            TDengineParameter[] result = new TDengineParameter[parameters.Count];
            int i = 0;
            foreach (var parameter in parameters)
            {
                if (parameter.Value == null) parameter.Value = DBNull.Value;
                if (parameter.Value is bool)
                {
                    parameter.Value = parameter.Value?.ToString()?.ToLower();
                }
                var sqlParameter = new TDengineParameter(parameter.ParameterName, parameter.Value, parameter.DbType, 0);
                if (parameter.CustomDbType?.Equals(System.Data.DbType.DateTime2) == true || (parameter.Value is DateTime && _IsMicrosecond))
                {
                    sqlParameter.IsMicrosecond = true;
                }
                else if (parameter.CustomDbType?.Equals(typeof(Date19)) == true || (parameter.Value is DateTime && _IsIsNanosecond))
                {
                    sqlParameter.IsNanosecond = true;
                }
                else if (parameter.Value is DateTime && this.Context.CurrentConnectionConfig.ConnectionString.Contains("config_"))
                {
                    _IsIsNanosecond = sqlParameter.IsNanosecond = this.Context.CurrentConnectionConfig.ConnectionString.Contains("config_ns");
                    _IsMicrosecond = sqlParameter.IsMicrosecond = this.Context.CurrentConnectionConfig.ConnectionString.Contains("config_us");
                }
                result[i] = sqlParameter;
                i++;
            }
            return result;
        }
    }
}