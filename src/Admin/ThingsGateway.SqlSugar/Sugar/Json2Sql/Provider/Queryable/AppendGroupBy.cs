using Newtonsoft.Json.Linq;
namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// AppendGroupBy
    /// </summary>
    public partial class JsonQueryableProvider : IJsonQueryableProvider<JsonQueryResult>
    {

        private void AppendGroupBy(JToken item)
        {
            var value = item.First().ToString();
            var obj = context.Utilities.JsonToGroupByModels(value);
            sugarQueryable.GroupBy(obj);
        }
    }
}
