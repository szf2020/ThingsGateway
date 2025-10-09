using Newtonsoft.Json.Linq;

namespace ThingsGateway.SqlSugar
{
    public partial class JsonUpdateableProvider : IJsonUpdateableProvider<JsonUpdateResult>
    {
        private void AppendWhereColumns(JToken item)
        {
            var columns = item.First().ToObject<string[]>();
            if (columns.IsNullOrEmpty()) { throw new SqlSugarLangException("need WhereColumns", "WhereColumns 需要设置列名"); }
            this.sugarUpdateable.WhereColumns(columns);
        }
    }
}
