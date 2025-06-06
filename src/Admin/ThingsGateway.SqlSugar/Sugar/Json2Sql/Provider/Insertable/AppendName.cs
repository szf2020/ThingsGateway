using Newtonsoft.Json.Linq;
namespace SqlSugar
{
    public partial class JsonInsertableProvider : IJsonInsertableProvider<JsonInsertResult>
    {
        private void AppendName(JToken item)
        {
            var tableInfo = jsonCommonProvider.GetTableName(item);
            this.TableName = tableInfo.TableName.ToCheckField();
            if (tableInfo.ShortName.HasValue())
            {
                this.TableName = tableInfo.ShortName + "." + tableInfo.TableName;
            }
        }
    }
}
