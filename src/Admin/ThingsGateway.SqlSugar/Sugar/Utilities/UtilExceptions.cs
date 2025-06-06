using System.Reflection;
namespace SqlSugar
{
    public class SqlSugarException : Exception
    {
        public string Sql { get; set; }
        public object Parametres { get; set; }
        public new Exception InnerException;
        public new string StackTrace;
        public new MethodBase TargetSite;
        public new string Source;

        public SqlSugarException(string message)
            : base(message) { }

        public SqlSugarException(SqlSugarProvider context, string message, string sql)
            : base(message)
        {
            this.Sql = sql;
        }

        public SqlSugarException(SqlSugarProvider context, string message, string sql, object pars)
            : base(message)
        {
            this.Sql = sql;
            this.Parametres = pars;
        }

        public SqlSugarException(SqlSugarProvider context, Exception ex, string sql, object pars)
            : base(ex.Message)
        {
            this.Sql = sql;
            this.Parametres = pars;
            this.InnerException = ex.InnerException;
            this.StackTrace = ex.StackTrace;
            this.TargetSite = ex.TargetSite;
            this.Source = ex.Source;
        }

        public SqlSugarException(SqlSugarProvider context, string message, object pars)
            : base(message)
        {
            this.Parametres = pars;
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

        public VersionExceptions(SqlSugarProvider context, string message, string sql, object pars) : base(context, message, sql, pars)
        {
        }

        public VersionExceptions(SqlSugarProvider context, Exception ex, string sql, object pars) : base(context, ex, sql, pars)
        {
        }

        public VersionExceptions(SqlSugarProvider context, string message, object pars) : base(context, message, pars)
        {
        }

        public VersionExceptions() : base()
        {
        }

        public VersionExceptions(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
