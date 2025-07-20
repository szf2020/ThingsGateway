using Newtonsoft.Json.Linq;

namespace ThingsGateway.SqlSugar
{
    public class JsonDeleteableProvider : IJsonDeleteableProvider<JsonDeleteResult>
    {
        private ISqlSugarClient context;
        private JObject jObject;
        private JsonCommonProvider jsonCommonProvider;
        private IDeleteable<object> sugarDeleteable = null;
        public JsonDeleteableProvider(ISqlSugarClient context, JObject jObject)
        {
            this.jObject = jObject;
            this.context = context;
            this.jsonCommonProvider = new JsonCommonProvider(context);
        }
        public SqlObjectResult ToSql()
        {
            return this.ToSqlList().First();
        }
        public List<SqlObjectResult> ToSqlList()
        {
            List<SqlObjectResult> result = new List<SqlObjectResult>();
            JsonQueryParameter jsonQueryParameter = new JsonQueryParameter();
            var appendTypeNames = this.jObject.AsJEnumerable();
            this.sugarDeleteable = this.context.Deleteable<object>();
            foreach (JToken item in appendTypeNames)
            {
                AppendAll(jsonQueryParameter, item);
            }
            result.Add(new SqlObjectResult(this.sugarDeleteable.ToSql(), JsonProviderType.Deleteable));
            return result;
        }
        private void AppendAll(JsonQueryParameter jsonQueryParameter, JToken item)
        {
            var name = item.Path;
            if (IsWhere(name))
            {
                AppendWhere(item);
            }
            else if (IsTable(name))
            {
                AppendTable(item);
            }
        }
        private void AppendTable(JToken item)
        {
            var tableInfo = jsonCommonProvider.GetTableName(item);
            var tableName = tableInfo.TableName.ToCheckField();
            if (tableInfo.ShortName.HasValue())
            {
                tableName = tableInfo.ShortName + "." + tableInfo.TableName;
            }
            this.sugarDeleteable.AS(tableName);
        }
        private void AppendWhere(JToken item)
        {
            var sqlObj = jsonCommonProvider.GetWhere(item, sugarDeleteable.DeleteBuilder.Context);
            sugarDeleteable.Where(sqlObj.Key, sqlObj.Value);
        }
        private static bool IsTable(string name)
        {
            return name.Equals(JsonProviderConfig.KeyDeleteable.Get(), StringComparison.CurrentCultureIgnoreCase);
        }
        private static bool IsWhere(string name)
        {
            return name.Equals("Where", StringComparison.CurrentCultureIgnoreCase);
        }
        public string ToSqlString()
        {
            throw new NotImplementedException();
        }

        public JsonDeleteResult ToResult()
        {
            var result = new JsonDeleteResult();
            var sqlInfo = this.ToSqlList();
            var sqlInfoResult = sqlInfo.First();
            result.UpdateRows = this.context.Ado.ExecuteCommand(sqlInfoResult.Sql, sqlInfoResult.Parameters);
            return result;
        }

        List<string> IJsonProvider<JsonDeleteResult>.ToSqlString()
        {
            throw new NotImplementedException();
        }
    }
}
