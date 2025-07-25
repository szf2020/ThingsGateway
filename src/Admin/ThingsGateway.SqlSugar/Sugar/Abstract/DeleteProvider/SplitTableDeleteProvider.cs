using System.Text.RegularExpressions;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 分表删除提供者
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public class SplitTableDeleteProvider<T> where T : class, new()
    {
        /// <summary>
        /// SqlSugar上下文
        /// </summary>
        public ISqlSugarClient Context;
        /// <summary>
        /// 可删除提供者
        /// </summary>
        public DeleteableProvider<T> deleteobj;

        /// <summary>
        /// 分表信息集合
        /// </summary>
        public IEnumerable<SplitTableInfo> Tables { get; set; }

        /// <summary>
        /// 执行删除命令
        /// </summary>
        /// <returns>影响的行数</returns>
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

        /// <summary>
        /// 异步执行删除命令
        /// </summary>
        /// <returns>影响的行数任务</returns>
        public async Task<int> ExecuteCommandAsync()
        {
            if (this.Context.Ado.Transaction == null)
            {
                try
                {
                    await this.Context.Ado.BeginTranAsync().ConfigureAwait(false);
                    var result = await _ExecuteCommandAsync().ConfigureAwait(false);
                    await this.Context.Ado.CommitTranAsync().ConfigureAwait(false);
                    return result;
                }
                catch (Exception)
                {
                    await this.Context.Ado.RollbackTranAsync().ConfigureAwait(false);
                    throw;
                }
            }
            else
            {
                return await _ExecuteCommandAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 执行删除命令(内部方法)
        /// </summary>
        /// <returns>影响的行数</returns>
        internal int _ExecuteCommand()
        {
            var result = 0;
            var sqlobj = deleteobj.ToSql();

            foreach (var item in Tables)
            {
                var newsqlobj = GetSqlObj(sqlobj, item.TableName);
                result += this.Context.Ado.ExecuteCommand(newsqlobj.Key, newsqlobj.Value);
            }
            return result;
        }

        /// <summary>
        /// 异步执行删除命令(内部方法)
        /// </summary>
        /// <returns>影响的行数任务</returns>
        internal async Task<int> _ExecuteCommandAsync()
        {
            var result = 0;
            var sqlobj = deleteobj.ToSql();
            foreach (var item in Tables)
            {
                var newsqlobj = GetSqlObj(sqlobj, item.TableName);
                result += await Context.Ado.ExecuteCommandAsync(newsqlobj.Key, newsqlobj.Value).ConfigureAwait(false);
            }
            return result;
        }

        /// <summary>
        /// 获取SQL对象
        /// </summary>
        /// <param name="keyValuePair">原始SQL键值对</param>
        /// <param name="asName">表别名</param>
        /// <returns>处理后的SQL键值对</returns>
        private KeyValuePair<string, List<SugarParameter>> GetSqlObj(KeyValuePair<string, IReadOnlyCollection<SugarParameter>> keyValuePair, string asName)
        {
            List<SugarParameter> pars = new List<SugarParameter>();
            string sql = keyValuePair.Key;
            if (keyValuePair.Value != null)
            {
                pars = keyValuePair.Value.Select(it => new SugarParameter(it.ParameterName, it.Value)).ToList();
            }
            sql = Regex.Replace(sql, deleteobj.EntityInfo.DbTableName, asName, RegexOptions.IgnoreCase);
            return new KeyValuePair<string, List<SugarParameter>>(sql, pars);
        }
    }
}