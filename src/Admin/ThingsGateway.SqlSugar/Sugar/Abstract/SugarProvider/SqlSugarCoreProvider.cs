using System.Diagnostics;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// Partial SqlSugarScope
    /// </summary>
    public partial class SqlSugarScope : ISqlSugarClient, ITenant
    {
        protected List<ConnectionConfig> _configs;
        protected Action<SqlSugarClient> _configAction;

        protected virtual SqlSugarClient GetContext()
        {
            SqlSugarClient result = null;
            var key = _configs.GetHashCode().ToString();
            StackTrace st = new StackTrace(false);
            var methods = st.GetFrames();
            var isAsync = UtilMethods.IsAnyAsyncMethod(methods);

            if (isAsync)
            {
                result = GetAsyncContext(key);
            }
            else
            {
                result = GetThreadContext(key);
            }
            return result;
        }
        private SqlSugarClient GetAsyncContext(string key)
        {
            SqlSugarClient result = CallContextAsync<SqlSugarClient>.GetData(key);
            if (result == null)
            {
                List<ConnectionConfig> configList = GetCopyConfigs();
                CallContextAsync<SqlSugarClient>.SetData(key, new SqlSugarClient(configList));
                result = CallContextAsync<SqlSugarClient>.GetData(key);
                if (this._configAction != null)
                {
                    this._configAction(result);
                }
            }

            return result;
        }
        private SqlSugarClient GetThreadContext(string key)
        {
            SqlSugarClient result = CallContextThread<SqlSugarClient>.GetData(key);
            if (result == null)
            {
                List<ConnectionConfig> configList = GetCopyConfigs();
                CallContextThread<SqlSugarClient>.SetData(key, new SqlSugarClient(configList));
                result = CallContextThread<SqlSugarClient>.GetData(key);
                if (this._configAction != null)
                {
                    this._configAction(result);
                }
            }
            return result;
        }
        private List<ConnectionConfig> GetCopyConfigs()
        {
            return _configs.Select(it => UtilMethods.CopyConfig(it)).ToList();
        }
    }
}
