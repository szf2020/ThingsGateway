using Newtonsoft.Json.Linq;

namespace ThingsGateway.SqlSugar
{
    public partial class ContextMethods : IContextMethods
    {
        public List<GroupByModel> JsonToGroupByModels(string json)
        {
            List<GroupByModel> conditionalModels = new List<GroupByModel>();
            var jarray = this.Context.Utilities.DeserializeObject<JArray>(json);
            foreach (var item in jarray)
            {
                if (item.ObjToString().Contains("fieldname", StringComparison.CurrentCultureIgnoreCase))
                {
                    var model = item.ToObject<GroupByModel>();
                    conditionalModels.Add(model);
                }
                else
                {
                    conditionalModels.Add(new GroupByModel() { FieldName = item.ObjToString().ToCheckField() });
                }
            }
            return conditionalModels;
        }
    }
}
