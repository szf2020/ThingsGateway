using System.Data;

namespace SqlSugar
{
    public class SugarConnection : IDisposable
    {
        public IDbConnection conn { get; set; }
        public bool IsAutoClose { get; set; }
        public ISqlSugarClient Context { get; set; }

        public void Dispose()
        {
            conn.Close();
            this.Context.CurrentConnectionConfig.IsAutoCloseConnection = IsAutoClose;
        }
    }
}
