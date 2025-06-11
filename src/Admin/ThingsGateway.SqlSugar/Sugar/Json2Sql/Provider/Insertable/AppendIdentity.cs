using Newtonsoft.Json.Linq;
namespace ThingsGateway.SqlSugar
{
    public partial class JsonInsertableProvider : IJsonInsertableProvider<JsonInsertResult>
    {
        private void AppendIdentity(JToken item)
        {
            var tableInfo = jsonCommonProvider.GetTableName(item);
            this.IdentityId = tableInfo.TableName;
        }
    }
}
