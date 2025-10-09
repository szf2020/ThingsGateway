using Newtonsoft.Json.Linq;

namespace ThingsGateway.SqlSugar
{
    public partial class JsonUpdateableProvider : IJsonUpdateableProvider<JsonUpdateResult>
    {
        private void AppendWhere(JToken item)
        {
            if (isList) { throw new SqlSugarException("Batch updates cannot use Where, only WhereColumns can set columns"); }
            var sqlObj = jsonCommonProvider.GetWhere(item, sugarUpdateable.UpdateBuilder.Context);
            sugarUpdateable.Where(sqlObj.Key, sqlObj.Value);
        }
    }
}
