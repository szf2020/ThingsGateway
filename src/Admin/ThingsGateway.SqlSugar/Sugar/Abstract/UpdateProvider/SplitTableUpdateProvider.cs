using System.Text.RegularExpressions;

namespace ThingsGateway.SqlSugar
{
    public class SplitTableUpdateProvider<T> where T : class, new()
    {
        public SqlSugarProvider Context;
        public UpdateableProvider<T> updateobj;

        public IEnumerable<SplitTableInfo> Tables { get; set; }

        public int ExecuteCommandWithOptLock(bool isThrowError = false)
        {
            var updates = updateobj.UpdateObjs;
            var tableName = this.Context.SplitHelper(updates[0]).GetTableName();
            var names = updateobj.UpdateBuilder.DbColumnInfoList.Select(it => it.DbColumnName).Distinct().ToArray();
            return this.Context.Updateable(updates).AS(tableName)
                .UpdateColumns(names).ExecuteCommandWithOptLock(isThrowError);
        }
        public int ExecuteCommand()
        {
            if (this.Context.Ado.Transaction == null)
            {
                try
                {
                    this.Context.Ado.BeginTran();
                    var result = _ExecuteCommand();
                    this.Context.Ado.CommitTran();
                    return result;
                }
                catch (Exception)
                {
                    this.Context.Ado.RollbackTran();
                    throw;
                }
            }
            else
            {
                return _ExecuteCommand();
            }
        }
        public async Task<int> ExecuteCommandWithOptLockAsync(bool isThrowError = false)
        {
            var updates = updateobj.UpdateObjs;
            var tableName = this.Context.SplitHelper(updates[0]).GetTableName();
            var names = updateobj.UpdateBuilder.DbColumnInfoList.Select(it => it.DbColumnName).Distinct().ToArray();
            return await Context.Updateable(updates).AS(tableName)
                .UpdateColumns(names).ExecuteCommandWithOptLockAsync(isThrowError).ConfigureAwait(false);
        }
        public async Task<int> ExecuteCommandAsync()
        {
            if (this.Context.Ado.Transaction == null)
            {
                try
                {
                    this.Context.Ado.BeginTran();
                    var result = await _ExecuteCommandAsync().ConfigureAwait(false);
                    this.Context.Ado.CommitTran();
                    return result;
                }
                catch (Exception)
                {
                    this.Context.Ado.RollbackTran();
                    throw;
                }
            }
            else
            {
                return await _ExecuteCommandAsync().ConfigureAwait(false);
            }
        }
        private int _ExecuteCommand()
        {
            var result = 0;
            var sqlobj = updateobj.ToSql();

            foreach (var item in Tables)
            {
                var newsqlobj = GetSqlObj(sqlobj, item.TableName);
                result += this.Context.Ado.ExecuteCommand(newsqlobj.Key, newsqlobj.Value);
            }
            return result;
        }

        private async Task<int> _ExecuteCommandAsync()
        {
            var result = 0;
            var sqlobj = updateobj.ToSql();
            foreach (var item in Tables)
            {
                var newsqlobj = GetSqlObj(sqlobj, item.TableName);
                result += await Context.Ado.ExecuteCommandAsync(newsqlobj.Key, newsqlobj.Value).ConfigureAwait(false);
            }
            return result;
        }

        private KeyValuePair<string, List<SugarParameter>> GetSqlObj(KeyValuePair<string, IReadOnlyList<SugarParameter>> keyValuePair, string asName)
        {
            List<SugarParameter> pars = new List<SugarParameter>();
            string sql = keyValuePair.Key;
            if (keyValuePair.Value != null)
            {
                pars = keyValuePair.Value.Select(it => new SugarParameter(it.ParameterName, it.Value)).ToList();
            }
            sql = Regex.Replace(sql, updateobj.EntityInfo.DbTableName, asName, RegexOptions.IgnoreCase);
            return new KeyValuePair<string, List<SugarParameter>>(sql, pars);
        }
    }
}
