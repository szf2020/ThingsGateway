using System.Data;
using System.Data.Common;
using System.Reflection;

namespace ThingsGateway.SqlSugar
{
    public partial interface IAdo
    {
        string SqlParameterKeyWord { get; }
        IDbConnection Connection { get; set; }
        IDbTransaction Transaction { get; set; }
        IDataParameter[] ToIDbDataParameter(params IReadOnlyCollection<SugarParameter> pars);
        IReadOnlyCollection<SugarParameter> GetParameters(object obj, PropertyInfo[] propertyInfo = null);
        SqlSugarProvider Context { get; set; }
        void CheckConnectionAfter(IDbConnection Connection);
        void CheckConnectionBefore(IDbConnection Connection);
        void ExecuteBefore(string sql, IReadOnlyCollection<SugarParameter> pars);
        void ExecuteAfter(string sql, IReadOnlyCollection<SugarParameter> pars);
        void GetDataBefore(string sql, IReadOnlyCollection<SugarParameter> parameters);
        void GetDataAfter(string sql, IReadOnlyCollection<SugarParameter> parameters);
        bool IsAnyTran();
        bool IsNoTran();
        bool IsEnableLogEvent { get; set; }
        StackTraceInfo SqlStackTrace { get; }
        IDataParameterCollection DataReaderParameters { get; set; }
        CommandType CommandType { get; set; }
        CancellationToken? CancellationToken { get; set; }
        bool IsDisableMasterSlaveSeparation { get; set; }
        bool IsClearParameters { get; set; }
        int CommandTimeOut { get; set; }
        TimeSpan SqlExecutionTime { get; }
        TimeSpan ConnectionExecutionTime { get; }
        int SqlExecuteCount { get; }
        SugarActionType SqlExecuteType { get; }
        IDbBind DbBind { get; }
        void SetCommandToAdapter(IDataAdapter adapter, DbCommand command);
        IDataAdapter GetAdapter();
        DbCommand GetCommand(string sql, IReadOnlyCollection<SugarParameter> parameters);

        DataTable GetDataTable(string sql, object parameters);
        DataTable GetDataTable(string sql, params IReadOnlyCollection<SugarParameter> parameters);

        Task<DataTable> GetDataTableAsync(string sql, object parameters);
        Task<DataTable> GetDataTableAsync(string sql, params IReadOnlyCollection<SugarParameter> parameters);

        DataSet GetDataSetAll(string sql, object parameters);
        DataSet GetDataSetAll(string sql, params IReadOnlyCollection<SugarParameter> parameters);

        Task<DataSet> GetDataSetAllAsync(string sql, object parameters);
        Task<DataSet> GetDataSetAllAsync(string sql, params IReadOnlyCollection<SugarParameter> parameters);

        IDataReader GetDataReader(string sql, object parameters);
        IDataReader GetDataReader(string sql, params IReadOnlyCollection<SugarParameter> parameters);

        Task<IDataReader> GetDataReaderAsync(string sql, object parameters);
        Task<IDataReader> GetDataReaderAsync(string sql, params IReadOnlyCollection<SugarParameter> parameters);

        object GetScalar(string sql, object parameters);
        object GetScalar(string sql, params IReadOnlyCollection<SugarParameter> parameters);

        Task<object> GetScalarAsync(string sql, object parameters);
        Task<object> GetScalarAsync(string sql, object parameters, CancellationToken cancellationToken);
        Task<object> GetScalarAsync(string sql, params IReadOnlyCollection<SugarParameter> parameters);

        int ExecuteCommandWithGo(string sql, params IReadOnlyCollection<SugarParameter> parameters);
        int ExecuteCommand(string sql, object parameters);
        int ExecuteCommand(string sql, params IReadOnlyCollection<SugarParameter> parameters);

        Task<int> ExecuteCommandAsync(string sql, params IReadOnlyCollection<SugarParameter> parameters);
        Task<int> ExecuteCommandAsync(string sql, object parameters);
        Task<int> ExecuteCommandAsync(string sql, object parameters, CancellationToken cancellationToken);

        string GetString(string sql, object parameters);
        string GetString(string sql, params IReadOnlyCollection<SugarParameter> parameters);
        Task<string> GetStringAsync(string sql, object parameters);
        Task<string> GetStringAsync(string sql, object parameters, CancellationToken cancellationToken);
        Task<string> GetStringAsync(string sql, params IReadOnlyCollection<SugarParameter> parameters);

        int GetInt(string sql, object pars);
        int GetInt(string sql, params IReadOnlyCollection<SugarParameter> parameters);

        Task<int> GetIntAsync(string sql, object pars);
        Task<int> GetIntAsync(string sql, object pars, CancellationToken cancellationToken);
        Task<int> GetIntAsync(string sql, params IReadOnlyCollection<SugarParameter> parameters);

        long GetLong(string sql, object pars = null);

        Task<long> GetLongAsync(string sql, object pars = null);

        Double GetDouble(string sql, object parameters);
        Double GetDouble(string sql, params IReadOnlyCollection<SugarParameter> parameters);

        Task<Double> GetDoubleAsync(string sql, object parameters);
        Task<Double> GetDoubleAsync(string sql, object parameters, CancellationToken cancellationToken);
        Task<Double> GetDoubleAsync(string sql, params IReadOnlyCollection<SugarParameter> parameters);

        decimal GetDecimal(string sql, object parameters);
        decimal GetDecimal(string sql, params IReadOnlyCollection<SugarParameter> parameters);

        Task<decimal> GetDecimalAsync(string sql, object parameters);
        Task<decimal> GetDecimalAsync(string sql, params IReadOnlyCollection<SugarParameter> parameters);

        DateTime GetDateTime(string sql, object parameters);
        DateTime GetDateTime(string sql, params IReadOnlyCollection<SugarParameter> parameters);

        Task<DateTime> GetDateTimeAsync(string sql, object parameters);
        Task<DateTime> GetDateTimeAsync(string sql, params IReadOnlyCollection<SugarParameter> parameters);

        Tuple<List<T>, List<T2>> SqlQuery<T, T2>(string sql, object parameters = null);
        Tuple<List<T>, List<T2>, List<T3>> SqlQuery<T, T2, T3>(string sql, object parameters = null);
        Tuple<List<T>, List<T2>, List<T3>, List<T4>> SqlQuery<T, T2, T3, T4>(string sql, object parameters = null);
        Tuple<List<T>, List<T2>, List<T3>, List<T4>, List<T5>> SqlQuery<T, T2, T3, T4, T5>(string sql, object parameters = null);
        Tuple<List<T>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>> SqlQuery<T, T2, T3, T4, T5, T6>(string sql, object parameters = null);
        Tuple<List<T>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>, List<T7>> SqlQuery<T, T2, T3, T4, T5, T6, T7>(string sql, object parameters = null);

        Task<Tuple<List<T>, List<T2>>> SqlQueryAsync<T, T2>(string sql, object parameters = null);
        Task<Tuple<List<T>, List<T2>, List<T3>>> SqlQueryAsync<T, T2, T3>(string sql, object parameters = null);
        Task<Tuple<List<T>, List<T2>, List<T3>, List<T4>>> SqlQueryAsync<T, T2, T3, T4>(string sql, object parameters = null);
        Task<Tuple<List<T>, List<T2>, List<T3>, List<T4>, List<T5>>> SqlQueryAsync<T, T2, T3, T4, T5>(string sql, object parameters = null);
        Task<Tuple<List<T>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>>> SqlQueryAsync<T, T2, T3, T4, T5, T6>(string sql, object parameters = null);
        Task<Tuple<List<T>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>, List<T7>>> SqlQueryAsync<T, T2, T3, T4, T5, T6, T7>(string sql, object parameters = null);

        List<T> SqlQuery<T>(string sql, object parameters = null);
        List<T> SqlQuery<T>(string sql, params IReadOnlyCollection<SugarParameter> parameters);
        Task<List<T>> MasterSqlQueryAasync<T>(string sql, object parameters = null);
        List<T> MasterSqlQuery<T>(string sql, object parameters = null);

        Task<List<T>> SqlQueryAsync<T>(string sql, object parameters = null);
        Task<List<T>> SqlQueryAsync<T>(string sql, object parameters, CancellationToken token);
        Task<List<T>> SqlQueryAsync<T>(string sql, params IReadOnlyCollection<SugarParameter> parameters);

        T SqlQuerySingle<T>(string sql, object whereObj = null);
        T SqlQuerySingle<T>(string sql, params IReadOnlyCollection<SugarParameter> parameters);

        Task<T> SqlQuerySingleAsync<T>(string sql, object whereObj = null);
        Task<T> SqlQuerySingleAsync<T>(string sql, params IReadOnlyCollection<SugarParameter> parameters);

        void RemoveCancellationToken();

        void Dispose();
        void Close();
        Task CloseAsync();
        void Open();
        Task OpenAsync();
        SugarConnection OpenAlways();
        bool IsValidConnection();
        bool IsValidConnectionNoClose();
        void CheckConnection();

        void BeginTran();
        Task BeginTranAsync();
        Task BeginTranAsync(IsolationLevel iso);
        void BeginTran(IsolationLevel iso);
        void BeginTran(string transactionName);
        void BeginTran(IsolationLevel iso, string transactionName);
        void RollbackTran();
        Task RollbackTranAsync();
        void CommitTran();
        Task CommitTranAsync();
        SqlSugarTransactionAdo UseTran();
        DbResult<bool> UseTran(Action action, Action<Exception> errorCallBack = null);
        DbResult<T> UseTran<T>(Func<T> action, Action<Exception> errorCallBack = null);
        Task<DbResult<bool>> UseTranAsync(Func<Task> action, Action<Exception> errorCallBack = null);
        Task<DbResult<T>> UseTranAsync<T>(Func<Task<T>> action, Action<Exception> errorCallBack = null);
        IAdo UseStoredProcedure();
    }
}
