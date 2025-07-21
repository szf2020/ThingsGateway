using Newtonsoft.Json.Linq;
namespace ThingsGateway.SqlSugar
{
    public partial class JsonUpdateableProvider : IJsonUpdateableProvider<JsonUpdateResult>
    {
        private void AppendRow(JToken item)
        {
            var itemFirst = item.First();
            var isObject = itemFirst.Type == JTokenType.Object;
            var value = itemFirst.ToString();
            var dics = context.Utilities.JsonToColumnsModels(value);
            if (isObject)
                sugarUpdateable = this.context.UpdateableT(dics[0]).AS(this.TableName);
            else
            {
                sugarUpdateable = this.context.Updateable<Dictionary<string, object>>(dics).AS(this.TableName);
                isList = dics.Count > 1;
            }
        }
    }
}
