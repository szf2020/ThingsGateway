using Newtonsoft.Json.Linq;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// ResultDefault
    /// </summary>
    public partial class JsonQueryableProvider : IJsonQueryableProvider<JsonQueryResult>
    {
        private List<SqlObjectResult> ToSqlDefault()
        {
            List<SqlObjectResult> result = new List<SqlObjectResult>();
            sugarQueryable = context.Queryable<object>();
            appendTypeNames = GetTypeNames();
            JsonQueryParameter jsonQueryParameter = new JsonQueryParameter();
            RegisterAop();
            foreach (JToken item in appendTypeNames)
            {
                AppendQueryableAll(jsonQueryParameter, item);
            }
            return ToPageDefault(result, jsonQueryParameter);
        }

        private List<JToken> GetTypeNames()
        {
            var result = this.jobject.AsJEnumerable().ToList();
            result.Add(JToken.Parse("{JoinLastAfter:null}").First());
            result = result.OrderBy(it => GetSort(it.Path)).ToList();
            return result;
        }

        private JsonQueryResult ToResultDefault()
        {
            JsonQueryResult result = new JsonQueryResult();
            var toSqls = this.ToSqlList();
            var SqlCount = toSqls.FirstOrDefault(it => it.JsonSqlType == JsonProviderType.QueryableCount);
            var SqlList = toSqls.FirstOrDefault(it => it.JsonSqlType == JsonProviderType.Queryable);
            AddCount(result, SqlCount);
            AddList(result, SqlList);
            AddDescription();
            return result;
        }
    }
}
