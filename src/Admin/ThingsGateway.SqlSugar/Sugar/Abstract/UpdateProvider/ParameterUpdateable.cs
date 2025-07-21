using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace ThingsGateway.SqlSugar
{
    public class ParameterUpdateable<T> where T : class, new()
    {
        internal UpdateableProvider<T> Updateable { get; set; }
        internal SqlSugarProvider Context { get; set; }
        public int ExecuteCommand()
        {
            var result = 0;
            var list = Updateable.UpdateObjs;
            var count = list.Count;
            var size = GetPageSize(20, count);
            Context.Utilities.PageEach(list, size, item =>
            {
                Before(item);
                List<SugarParameter> allParamter = new List<SugarParameter>();
                var sql = GetSql(item);
                result += Context.Ado.ExecuteCommand(sql.Key, sql.Value);
                After(item);
            });
            return result < 0 ? count : result;
        }
        public async Task<int> ExecuteCommandAsync()
        {
            var result = 0;
            var list = Updateable.UpdateObjs;
            var count = list.Count;
            var size = GetPageSize(20, count);
            await Context.Utilities.PageEachAsync(list, size, async item =>
            {
                Before(item);
                List<SugarParameter> allParamter = new List<SugarParameter>();
                var sql = GetSql(item);
                result += await Context.Ado.ExecuteCommandAsync(sql.Key, sql.Value).ConfigureAwait(false);
                After(item);
            }).ConfigureAwait(false);
            return result < 0 ? count : result;
        }

        private void Before(List<T> updateObjects)
        {
            if (this.Updateable.IsEnableDiffLogEvent && updateObjects.Count > 0)
            {
                var isDisableMasterSlaveSeparation = this.Updateable.Ado.IsDisableMasterSlaveSeparation;
                this.Updateable.Ado.IsDisableMasterSlaveSeparation = true;
                var parameters = Updateable.UpdateBuilder.Parameters;
                if (parameters == null)
                    parameters = new List<SugarParameter>();
                Updateable.DiffModel.BeforeData = GetDiffTable(updateObjects);
                Updateable.DiffModel.Sql = this.Updateable.UpdateBuilder.ToSqlString();
                Updateable.DiffModel.Parameters = parameters;
                this.Updateable.Ado.IsDisableMasterSlaveSeparation = isDisableMasterSlaveSeparation;
            }
        }

        protected void After(List<T> updateObjects)
        {
            if (this.Updateable.IsEnableDiffLogEvent && updateObjects.Count > 0)
            {
                var isDisableMasterSlaveSeparation = this.Updateable.Ado.IsDisableMasterSlaveSeparation;
                this.Updateable.Ado.IsDisableMasterSlaveSeparation = true;
                Updateable.DiffModel.AfterData = GetDiffTable(updateObjects);
                Updateable.DiffModel.Time = this.Context.Ado.SqlExecutionTime;
                if (this.Context.CurrentConnectionConfig.AopEvents.OnDiffLogEvent != null)
                    this.Context.CurrentConnectionConfig.AopEvents.OnDiffLogEvent(Updateable.DiffModel);
                this.Updateable.Ado.IsDisableMasterSlaveSeparation = isDisableMasterSlaveSeparation;
            }
            if (this.Updateable.RemoveCacheFunc != null)
            {
                this.Updateable.RemoveCacheFunc();
            }
        }
        private List<DiffLogTableInfo> GetDiffTable(List<T> updateObjects)
        {
            var builder = Updateable.UpdateBuilder.Builder;
            var tableWithString = builder.GetTranslationColumnName(Updateable.UpdateBuilder.TableName);
            var wheres = Updateable.WhereColumnList ?? Updateable.UpdateBuilder.PrimaryKeys;
            if (wheres == null)
            {
                wheres = Updateable.UpdateBuilder.DbColumnInfoList
                    .Where(it => it.IsPrimarykey).Select(it => it.DbColumnName).Distinct().ToList();
            }
            var sqlDb = this.Context.CopyNew();
            sqlDb.Aop.DataExecuting = null;
            var dataColumns = sqlDb.Updateable(updateObjects).UpdateBuilder.DbColumnInfoList;
            List<SugarParameter> parameters = new List<SugarParameter>();
            StringBuilder allWhereString = new StringBuilder();
            string columnStr = string.Join(",", dataColumns.Select(x => x.DbColumnName).Distinct());
            foreach (var item in dataColumns.GroupBy(it => it.TableId))
            {
                StringBuilder whereString = new StringBuilder();
                foreach (var whereItem in wheres)
                {
                    var pk = item.FirstOrDefault(it => it.DbColumnName.EqualCase(whereItem));
                    var paraterName = FormatValue(pk.PropertyType, pk.DbColumnName, pk.Value, parameters);
                    whereString.Append($" {pk.DbColumnName} = {paraterName} AND");
                }
                allWhereString.Append($" {Regex.Replace(whereString.ToString(), "AND$", "")} OR");
            }
            string key = $"SELECT {columnStr} FROM {tableWithString} WHERE {Regex.Replace(allWhereString.ToString(), "OR$", "")}";

            var dt = sqlDb.Ado.GetDataTable(key, parameters);
            return Updateable.GetTableDiff(dt);
        }

        #region Values Helper

        public KeyValuePair<string, IReadOnlyList<SugarParameter>> GetSql(List<T> updateObjects)
        {
            var inserable = Updateable as UpdateableProvider<T>;
            var builder = inserable.UpdateBuilder.Builder;
            var tableWithString = builder.GetTranslationColumnName(inserable.UpdateBuilder.TableName);
            var wheres = inserable.WhereColumnList ?? inserable.UpdateBuilder.PrimaryKeys;
            if (wheres == null)
            {
                wheres = inserable.UpdateBuilder.DbColumnInfoList
                    .Where(it => it.IsPrimarykey).Select(it => it.DbColumnName).Distinct().ToList();
            }
            StringBuilder sbAllSql = new StringBuilder();
            var sqlTemp = ($" UPDATE {tableWithString} SET {{0}}  WHERE {{1}};\r\n");
            List<SugarParameter> parameters = new List<SugarParameter>();
            Check.ExceptionEasy(wheres?.Count == 0, "Updates cannot be without a primary key or condition", "更新不能没有主键或者条件");
            var sqlDb = this.Context.CopyNew();
            sqlDb.Aop.DataExecuting = null;
            foreach (var list in sqlDb.Updateable(updateObjects).UpdateBuilder.DbColumnInfoList.GroupBy(it => it.TableId))
            {
                Check.ExceptionEasy(list?.Any() != true, "Set has no columns", "更新Set没有列");
                StringBuilder setString = new StringBuilder();
                foreach (var setItem in list)
                {
                    if (setItem.IsPrimarykey) { continue; }
                    if (Updateable.UpdateBuilder.UpdateColumns?.Count > 0)
                    {
                        if (!Updateable.UpdateBuilder.UpdateColumns.Any(it => it.EqualCase(setItem.DbColumnName)))
                        {
                            continue;
                        }
                    }
                    if (Updateable.UpdateBuilder.IgnoreColumns?.Count > 0)
                    {
                        if (Updateable.UpdateBuilder.IgnoreColumns.Any(it => it.EqualCase(setItem.DbColumnName)))
                        {
                            continue;
                        }
                    }
                    var paraterName = FormatValue(setItem.PropertyType, setItem.DbColumnName, setItem.Value, parameters);
                    setString.Append($" {builder.GetTranslationColumnName(setItem.DbColumnName)} = {paraterName} ,");
                }
                StringBuilder whereString = new StringBuilder();
                foreach (var whereItem in wheres)
                {
                    var pk = list.FirstOrDefault(it => it.DbColumnName.EqualCase(whereItem));
                    var paraterName = FormatValue(pk.PropertyType, pk.DbColumnName, pk.Value, parameters);
                    whereString.Append($" {pk.DbColumnName} = {paraterName} AND");
                }
                var builderItem = string.Format(sqlTemp, setString.ToString().TrimEnd(','), whereString.ToString().TrimEnd('D').TrimEnd('N').TrimEnd('A'));
                sbAllSql.Append(builderItem);
            }
            builder.FormatSaveQueueSql(sbAllSql);
            return new KeyValuePair<string, IReadOnlyList<SugarParameter>>(sbAllSql.ToString(), parameters);
        }

        private int GetPageSize(int pageSize, int count)
        {
            if (pageSize * count > 2100)
            {
                pageSize = 50;
            }
            if (pageSize * count > 2100)
            {
                pageSize = 20;
            }
            if (pageSize * count > 2100)
            {
                pageSize = 10;
            }

            return pageSize;
        }
        private string FormatValue(Type type, string name, object value, List<SugarParameter> allParamter)
        {
            var keyword = this.Updateable.UpdateBuilder.Builder.SqlParameterKeyWord;
            var result = keyword + name + allParamter.Count;
            var addParameter = new SugarParameter(result, value, type);
            allParamter.Add(addParameter);
            return result;
        }
        #endregion
    }
}
