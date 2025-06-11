using Newtonsoft.Json.Linq;
namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// ApendJoinLastAfter
    /// </summary>
    public partial class JsonQueryableProvider : IJsonQueryableProvider<JsonQueryResult>
    {
        private void ApendJoinLastAfter(JToken item)
        {
            if (IsAppendSelect())
            {
                JArray jArray = new JArray();
                var tableConfigs = this.jsonTableConfigs.GroupBy(it => it.TableName).Select(it => it.First()).ToList();
                var isJoinTable = IsAnyJoin(appendTypeNames);
                foreach (var config in tableConfigs)
                {

                    if (isJoinTable)
                    {

                    }
                    else
                    {
                        if (config.Columns.Count != 0)
                        {
                            foreach (var column in config.Columns.Select(it => it.Name).Distinct())
                            {
                                jArray.Add(column);
                            }
                        }
                    }
                }
                this.AppendSelect(jArray);
            }
        }

        private bool IsAppendSelect()
        {
            return !IsAnySelect(appendTypeNames) && this.jsonTableConfigs.Count != 0;
        }
    }
}
