using Newtonsoft.Json.Linq;
namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// AppendHaving
    /// </summary>
    public partial class JsonQueryableProvider : IJsonQueryableProvider<JsonQueryResult>
    {
        private void AppendHaving(JToken item)
        {
            var value = item.First().ToString();
            var obj = context.Utilities.JsonToSqlFuncModels(value);
            sugarQueryable.Having(obj);
        }

    }
}
