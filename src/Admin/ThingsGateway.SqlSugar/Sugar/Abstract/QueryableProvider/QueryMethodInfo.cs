using System.Data;
using System.Text.RegularExpressions;

namespace ThingsGateway.SqlSugar
{
    public class QueryMethodInfo
    {
        public object QueryableObj { get; internal set; }
        public SqlSugarProvider Context { get; internal set; }
        public Type EntityType { get; set; }

        #region Sql API

        public QueryMethodInfo MergeTable()
        {
            var method = QueryableObj.GetType().GetMethod("MergeTable");
            this.QueryableObj = method.Invoke(QueryableObj, Array.Empty<object>());
            return this;
        }
        public QueryMethodInfo AS(string tableName)
        {
            string shortName = $"{tableName}_1";
            if (!Regex.IsMatch(shortName, @"^\w+$"))
            {
                shortName = "maintable";
            }
            var method = QueryableObj.GetType().GetMyMethod(nameof(QueryMethodInfo.AS), 2, typeof(string), typeof(string));
            this.QueryableObj = method.Invoke(QueryableObj, new object[] { tableName, shortName });
            return this;
        }
        public QueryMethodInfo AS(string tableName, string shortName)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(QueryMethodInfo.AS), 2, typeof(string), typeof(string));
            this.QueryableObj = method.Invoke(QueryableObj, new object[] { tableName, shortName });
            return this;
        }
        public QueryMethodInfo OrderBy(List<OrderByModel> models)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(QueryMethodInfo.OrderBy), 1, typeof(List<OrderByModel>));
            this.QueryableObj = method.Invoke(QueryableObj, new object[] { models });
            return this;
        }
        public QueryMethodInfo OrderBy(string orderBySql)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(QueryMethodInfo.OrderBy), 1, typeof(string));
            this.QueryableObj = method.Invoke(QueryableObj, new object[] { orderBySql });
            return this;
        }
        public QueryMethodInfo AddJoinInfo(string tableName, string shortName, string onWhere, JoinType type = JoinType.Left)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(AddJoinInfo), 4, typeof(string), typeof(string), typeof(string), typeof(JoinType));
            this.QueryableObj = method.Invoke(QueryableObj, new object[] { tableName, shortName, onWhere, type });
            return this;
        }
        public QueryMethodInfo AddJoinInfo(string tableName, string shortName, IFuncModel onFunc, JoinType type = JoinType.Left)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(AddJoinInfo), 4, typeof(string), typeof(string), typeof(IFuncModel), typeof(JoinType));
            this.QueryableObj = method.Invoke(QueryableObj, new object[] { tableName, shortName, onFunc, type });
            return this;
        }
        public QueryMethodInfo AddJoinInfo(List<JoinInfoParameter> joinInfoParameters)
        {
            foreach (var item in joinInfoParameters)
            {
                AddJoinInfo(item.TableName, item.ShortName, item.Models, item.Type);
            }
            return this;
        }
        public QueryMethodInfo AddJoinInfo(Type joinEntityType, Dictionary<string, Type> keyIsShortName_ValueIsType_Dictionary, FormattableString expOnWhere, JoinType type = JoinType.Left)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(AddJoinInfo), 4, typeof(Type), typeof(Dictionary<string, Type>), typeof(FormattableString), typeof(JoinType));
            this.QueryableObj = method.Invoke(QueryableObj, new object[] { joinEntityType, keyIsShortName_ValueIsType_Dictionary, expOnWhere, type });
            return this;
        }
        public QueryMethodInfo AddJoinInfo(Type joinEntityType, string shortName, string onWhere, JoinType type = JoinType.Left)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(AddJoinInfo), 4, typeof(Type), typeof(string), typeof(string), typeof(JoinType));
            this.QueryableObj = method.Invoke(QueryableObj, new object[] { joinEntityType, shortName, onWhere, type });
            return this;
        }
        public QueryMethodInfo GroupBy(List<GroupByModel> models)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(QueryMethodInfo.GroupBy), 1, typeof(List<GroupByModel>));
            this.QueryableObj = method.Invoke(QueryableObj, new object[] { models });
            return this;
        }
        public QueryMethodInfo GroupBy(string groupBySql)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(QueryMethodInfo.GroupBy), 1, typeof(string));
            this.QueryableObj = method.Invoke(QueryableObj, new object[] { groupBySql });
            return this;
        }
        public QueryMethodInfo Where(string expShortName, FormattableString expressionString)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(QueryMethodInfo.Where), 2, typeof(string), typeof(FormattableString));
            this.QueryableObj = method.Invoke(QueryableObj, new object[] { expShortName, expressionString });
            return this;
        }
        public QueryMethodInfo Where(Dictionary<string, Type> keyIsShortName_ValueIsType_Dictionary, FormattableString expressionString)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(QueryMethodInfo.Where), 2, typeof(Dictionary<string, Type>), typeof(FormattableString));
            this.QueryableObj = method.Invoke(QueryableObj, new object[] { keyIsShortName_ValueIsType_Dictionary, expressionString });
            return this;
        }
        public QueryMethodInfo Where(List<IConditionalModel> conditionalModels)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(QueryMethodInfo.Where), 1, typeof(List<IConditionalModel>));
            this.QueryableObj = method.Invoke(QueryableObj, new object[] { conditionalModels });
            return this;
        }

        public QueryMethodInfo Where(IFuncModel model)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(QueryMethodInfo.Where), 1, typeof(IFuncModel));
            this.QueryableObj = method.Invoke(QueryableObj, new object[] { model });
            return this;
        }

        public QueryMethodInfo Where(List<IConditionalModel> conditionalModels, bool isWrap)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(QueryMethodInfo.Where), 2, typeof(List<IConditionalModel>), typeof(bool));
            this.QueryableObj = method.Invoke(QueryableObj, new object[] { conditionalModels, isWrap });
            return this;
        }
        public QueryMethodInfo Where(string sql, object parameters = null)
        {
            var method = QueryableObj.GetType().GetMyMethodNoGen(nameof(QueryMethodInfo.Where), 2, typeof(string), typeof(object));
            this.QueryableObj = method.Invoke(QueryableObj, new object[] { sql, parameters });
            return this;
        }
        public QueryMethodInfo Having(IFuncModel model)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(QueryMethodInfo.Having), 1, typeof(IFuncModel));
            this.QueryableObj = method.Invoke(QueryableObj, new object[] { model });
            return this;
        }
        public QueryMethodInfo Having(string sql, object parameters = null)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(QueryMethodInfo.Having), 2, typeof(string), typeof(object));
            this.QueryableObj = method.Invoke(QueryableObj, new object[] { sql, parameters });
            return this;
        }
        public QueryMethodInfo SplitTable(Func<List<SplitTableInfo>, IEnumerable<SplitTableInfo>> getTableNamesFunc)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(SplitTable), 1, typeof(Func<List<SplitTableInfo>, IEnumerable<SplitTableInfo>>));
            this.QueryableObj = method.Invoke(QueryableObj, new object[] { getTableNamesFunc });
            return this;
        }
        public QueryMethodInfo SplitTable(DateTime beginTime, DateTime endTime)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(SplitTable), 2, typeof(DateTime), typeof(DateTime));
            this.QueryableObj = method.Invoke(QueryableObj, new object[] { beginTime, endTime });
            return this;
        }
        public QueryMethodInfo SplitTable()
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(SplitTable), 0);
            this.QueryableObj = method.Invoke(QueryableObj, Array.Empty<object>());
            return this;
        }
        public QueryMethodInfo Select(string expShortName, List<string> columns, params object[] args)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(QueryMethodInfo.Select), 3, typeof(string), typeof(List<string>), typeof(object[]));
            this.QueryableObj = method.Invoke(QueryableObj, new object[] { expShortName, columns, args });
            return this;
        }
        public QueryMethodInfo Select(List<SelectModel> models)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(QueryMethodInfo.Select), 1, typeof(List<SelectModel>));
            this.QueryableObj = method.Invoke(QueryableObj, new object[] { models });
            return this;
        }
        public QueryMethodInfo Select(string expShortName, FormattableString expSelect, Type resultType)
        {
            var method = QueryableObj.GetType().GetMyMethodIsGenericMethod(nameof(QueryMethodInfo.Select), 3, typeof(string), typeof(FormattableString), typeof(Type));
            if (method.IsGenericMethodDefinition)
            {
                method = method.MakeGenericMethod(resultType);
            }
            this.QueryableObj = method.Invoke(QueryableObj, new object[] { expShortName, expSelect, resultType });
            return this;
        }
        public QueryMethodInfo Select(Dictionary<string, Type> keyIsShortName_ValueIsType_Dictionary, FormattableString expSelect, Type resultType)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(QueryMethodInfo.Select), 3, typeof(Dictionary<string, Type>), typeof(FormattableString), typeof(Type));
            method = method.MakeGenericMethod(resultType);
            this.QueryableObj = method.Invoke(QueryableObj, new object[] { keyIsShortName_ValueIsType_Dictionary, expSelect, resultType });
            return this;
        }
        public QueryMethodInfo Select(string selectorSql)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(QueryMethodInfo.Select), 1, typeof(string))
             .MakeGenericMethod(EntityType);
            this.QueryableObj = method.Invoke(QueryableObj, new object[] { selectorSql });
            return this;
        }

        public QueryMethodInfo Select(string selectorSql, Type selectType)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(QueryMethodInfo.Select), 1, typeof(string))
             .MakeGenericMethod(selectType);
            this.QueryableObj = method.Invoke(QueryableObj, new object[] { selectorSql });
            return this;
        }

        #endregion

        #region Nav

        public QueryMethodInfo IncludesAllFirstLayer(params string[] ignoreNavPropertyNames)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(IncludesAllFirstLayer), 1, typeof(string[]));
            this.QueryableObj = method.Invoke(QueryableObj, new object[] { ignoreNavPropertyNames });
            return this;
        }
        public QueryMethodInfo Includes(string navPropertyName)
        {
            var method = QueryableObj.GetType().GetMyMethod("IncludesByNameString", 1, typeof(string));
            this.QueryableObj = method.Invoke(QueryableObj, new object[] { navPropertyName });
            return this;
        }
        public QueryMethodInfo IgnoreColumns(params string[] ignoreColumns)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(IgnoreColumns), 1, typeof(string[]));
            this.QueryableObj = method.Invoke(QueryableObj, new object[] { ignoreColumns });
            return this;
        }
        public QueryMethodInfo Includes(string navPropertyName, string thenNavPropertyName2)
        {
            var method = QueryableObj.GetType().GetMyMethod("IncludesByNameString", 2, typeof(string), typeof(string));
            this.QueryableObj = method.Invoke(QueryableObj, new object[] { navPropertyName, thenNavPropertyName2 });
            return this;
        }
        public QueryMethodInfo Includes(string navPropertyName, string thenNavPropertyName2, string thenNavPropertyName3)
        {
            var method = QueryableObj.GetType().GetMyMethod("IncludesByNameString", 3, typeof(string), typeof(string), typeof(string));
            this.QueryableObj = method.Invoke(QueryableObj, new object[] { navPropertyName, thenNavPropertyName2, thenNavPropertyName3 });
            return this;
        }
        #endregion

        #region Result

        public void IntoTable(Type type, string tableName)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(IntoTable), 2, typeof(Type), typeof(string));
            var result = method.Invoke(QueryableObj, new object[] { type, tableName });
        }
        public object ToPageList(int pageNumber, int pageSize)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(ToPageList), 2, typeof(int), typeof(int));
            var result = method.Invoke(QueryableObj, new object[] { pageNumber, pageSize });
            return result;
        }
        public object ToPageList(int pageNumber, int pageSize, ref int count)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(ToPageList), 3, typeof(int), typeof(int), typeof(int).MakeByRefType());
            var parameters = new object[] { pageNumber, pageSize, count };
            var result = method.Invoke(QueryableObj, parameters);
            count = parameters.Last().ObjToInt();
            return result;
        }
        public object ToList()
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(QueryMethodInfo.ToList), 0);
            var result = method.Invoke(QueryableObj, Array.Empty<object>());
            return result;
        }
        public DataTable ToDataTablePage(int pageNumber, int pageSize, ref int count)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(ToDataTablePage), 3, typeof(int), typeof(int), typeof(int).MakeByRefType());
            var parameters = new object[] { pageNumber, pageSize, count };
            var result = (DataTable)method.Invoke(QueryableObj, parameters);
            count = parameters.Last().ObjToInt();
            return result;
        }
        public DataTable ToDataTablePage(int pageNumber, int pageSize)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(ToDataTablePage), 2, typeof(int), typeof(int));
            var parameters = new object[] { pageNumber, pageSize };
            var result = (DataTable)method.Invoke(QueryableObj, parameters);
            return result;
        }
        public DataTable ToDataTable()
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(ToDataTable), 0);
            var result = (DataTable)method.Invoke(QueryableObj, Array.Empty<object>());
            return result;
        }
        public string ToSqlString()
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(ToSqlString), 0);
            var result = method.Invoke(QueryableObj, Array.Empty<object>());
            return (string)result;
        }
        public KeyValuePair<string, IReadOnlyCollection<SugarParameter>> ToSql()
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(ToSql), 0);
            var result = method.Invoke(QueryableObj, Array.Empty<object>());
            return (KeyValuePair<string, IReadOnlyCollection<SugarParameter>>)result;
        }
        public object InSingle(object pkValue)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(InSingle), 1);
            var result = method.Invoke(QueryableObj, new object[] { pkValue });
            return result;
        }
        public bool CreateView(string viewNameFomat)
        {
            if (viewNameFomat?.Contains("{0}") != true)
            {
                Check.ExceptionEasy("need{0}", "需要{0}表名的占位符");
            }
            var entityInfo = this.Context.EntityMaintenance.GetEntityInfo(EntityType);
            var viewName = string.Format(viewNameFomat, entityInfo.DbTableName);
            if (!this.Context.DbMaintenance.GetViewInfoList().Any(it => it.Name.EqualCase(viewName)))
            {
                var method = QueryableObj.GetType().GetMyMethod(nameof(ToSqlString), 0);
                var result = (string)method.Invoke(QueryableObj, Array.Empty<object>());
                var sql = $"CREATE  VIEW  {viewName} AS {Environment.NewLine} {result}";
                this.Context.Ado.ExecuteCommand(sql);
                return true;
            }
            else
            {
                return false;
            }
        }
        public object First()
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(QueryMethodInfo.First), 0);
            var result = method.Invoke(QueryableObj, Array.Empty<object>());
            return result;
        }
        public bool Any()
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(QueryMethodInfo.Any), 0);
            var result = method.Invoke(QueryableObj, Array.Empty<object>());
            return Convert.ToBoolean(result);
        }
        public int Count()
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(QueryMethodInfo.Count), 0);
            var result = method.Invoke(QueryableObj, Array.Empty<object>());
            return Convert.ToInt32(result);
        }
        public object ToTree(string childPropertyName, string parentIdPropertyName, object rootValue, string primaryKeyPropertyName)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(ToTree), 4, typeof(string), typeof(string), typeof(object), typeof(string));
            var result = method.Invoke(QueryableObj, new object[] { childPropertyName, parentIdPropertyName, rootValue, primaryKeyPropertyName });
            return result;
        }

        #endregion

        #region Result Async
        public async Task<object> ToPageListAsync(int pageNumber, int pageSize)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(ToPageListAsync), 2, typeof(int), typeof(int));
            Task task = (Task)method.Invoke(QueryableObj, new object[] { pageNumber, pageSize });
            return await GetTask(task).ConfigureAwait(false);
        }
        public async Task<object> ToPageListAsync(int pageNumber, int pageSize, RefAsync<int> count)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(ToPageListAsync), 3, typeof(int), typeof(int), typeof(RefAsync<int>));
            var parameters = new object[] { pageNumber, pageSize, count };
            var task = (Task)method.Invoke(QueryableObj, parameters);
            return await GetTask(task).ConfigureAwait(false);
        }
        public async Task<object> ToListAsync()
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(ToListAsync), 0);
            var task = (Task)method.Invoke(QueryableObj, Array.Empty<object>());
            return await GetTask(task).ConfigureAwait(false);
        }
        public async Task<object> ToDataTablePageAsync(int pageNumber, int pageSize, RefAsync<int> count)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(ToDataTablePageAsync), 3, typeof(int), typeof(int), typeof(RefAsync<int>));
            var parameters = new object[] { pageNumber, pageSize, count };
            var task = (Task)method.Invoke(QueryableObj, parameters);
            count = parameters.Last().ObjToInt();
            return await GetTask(task).ConfigureAwait(false);
        }
        public async Task<object> ToDataTablePageAsync(int pageNumber, int pageSize)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(ToDataTablePageAsync), 2, typeof(int), typeof(int));
            var parameters = new object[] { pageNumber, pageSize };
            var task = (Task)method.Invoke(QueryableObj, parameters);
            return await GetTask(task).ConfigureAwait(false);
        }
        public async Task<object> ToDataTableAsync()
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(ToDataTableAsync), 0);
            var task = (Task)method.Invoke(QueryableObj, Array.Empty<object>());
            return await GetTask(task).ConfigureAwait(false);
        }
        public async Task<object> FirstAsync()
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(FirstAsync), 0);
            var task = (Task)method.Invoke(QueryableObj, Array.Empty<object>());
            return await GetTask(task).ConfigureAwait(false);
        }
        public async Task<bool> AnyAsync()
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(AnyAsync), 0);
            var result = method.Invoke(QueryableObj, Array.Empty<object>());
            return await ((Task<bool>)result).ConfigureAwait(false);
        }
        public async Task<int> CountAsync()
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(CountAsync), 0);
            var result = method.Invoke(QueryableObj, Array.Empty<object>());
            return await ((Task<int>)result).ConfigureAwait(false);
        }
        public async Task<object> InSingleAsync(object pkValue)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(InSingleAsync), 1);
            var task = (Task)method.Invoke(QueryableObj, new object[] { pkValue });
            return await GetTask(task).ConfigureAwait(false);
        }

        public async Task<object> ToTreeAsync(string childPropertyName, string parentIdPropertyName, object rootValue, string primaryKeyPropertyName)
        {
            var method = QueryableObj.GetType().GetMyMethod(nameof(ToTreeAsync), 4, typeof(string), typeof(string), typeof(object), typeof(string));
            var task = (Task)method.Invoke(QueryableObj, new object[] { childPropertyName, parentIdPropertyName, rootValue, primaryKeyPropertyName });
            return await GetTask(task).ConfigureAwait(false);
        }
        #endregion

        #region Helper
        private static async Task<object> GetTask(Task task)
        {
            await task.ConfigureAwait(false); // 等待任务完成
            var resultProperty = task.GetType().GetProperty("Result");
            var result = resultProperty.GetValue(task);
            return result;
        }
        #endregion
    }
}
