using System.Reflection;
namespace ThingsGateway.SqlSugar
{
    public class SqlSugarException : Exception
    {
        public string Sql { get; set; }
        public IReadOnlyCollection<SugarParameter> Parameters { get; set; }
        public new Exception InnerException;
        public new string StackTrace;
        public new MethodBase TargetSite;
        public new string Source;

        public SqlSugarException(string message)
            : base(message) { }
        public SqlSugarException(string message, params object[] strings)
    : base(string.Format(message, strings)) { }
        public SqlSugarException(SqlSugarProvider context, string message, string sql)
            : base(message)
        {
            this.Sql = sql;
        }

        public SqlSugarException(SqlSugarProvider context, string message, string sql, IReadOnlyCollection<SugarParameter> pars)
            : base(message)
        {
            this.Sql = sql;
            this.Parameters = pars;
        }

        public SqlSugarException(SqlSugarProvider context, Exception ex, string sql, IReadOnlyCollection<SugarParameter> pars)
            : base(ex.Message)
        {
            this.Sql = sql;
            this.Parameters = pars;
            this.InnerException = ex.InnerException;
            this.StackTrace = ex.StackTrace;
            this.TargetSite = ex.TargetSite;
            this.Source = ex.Source;
        }

        public SqlSugarException(SqlSugarProvider context, string message, IReadOnlyCollection<SugarParameter> pars)
            : base(message)
        {
            this.Parameters = pars;
        }

        public SqlSugarException() : base()
        {
        }

        public SqlSugarException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
    public class VersionExceptions : SqlSugarException
    {
        public VersionExceptions(string message)
            : base(message) { }

        public VersionExceptions(SqlSugarProvider context, string message, string sql) : base(context, message, sql)
        {
        }

        public VersionExceptions(SqlSugarProvider context, string message, string sql, IReadOnlyCollection<SugarParameter> pars) : base(context, message, sql, pars)
        {
        }

        public VersionExceptions(SqlSugarProvider context, Exception ex, string sql, IReadOnlyCollection<SugarParameter> pars) : base(context, ex, sql, pars)
        {
        }

        public VersionExceptions(SqlSugarProvider context, string message, IReadOnlyCollection<SugarParameter> pars) : base(context, message, pars)
        {
        }

        public VersionExceptions() : base()
        {
        }

        public VersionExceptions(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }

    public class SqlSugarLangException : SqlSugarException
    {
        public SqlSugarLangException(string enmessage, string cnMessage)
            : base(ErrorMessage.GetThrowMessage(enmessage, cnMessage)) { }

        public SqlSugarLangException(SqlSugarProvider context, string message, string sql) : base(context, message, sql)
        {
        }

        public SqlSugarLangException(SqlSugarProvider context, string message, string sql, IReadOnlyCollection<SugarParameter> pars) : base(context, message, sql, pars)
        {
        }

        public SqlSugarLangException(SqlSugarProvider context, Exception ex, string sql, IReadOnlyCollection<SugarParameter> pars) : base(context, ex, sql, pars)
        {
        }

        public SqlSugarLangException(SqlSugarProvider context, string message, IReadOnlyCollection<SugarParameter> pars) : base(context, message, pars)
        {
        }

        public SqlSugarLangException() : base()
        {
        }

        public SqlSugarLangException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
