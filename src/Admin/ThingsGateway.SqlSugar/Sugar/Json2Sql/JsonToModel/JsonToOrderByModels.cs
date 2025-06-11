using Newtonsoft.Json.Linq;

namespace ThingsGateway.SqlSugar
{
    public partial class ContextMethods : IContextMethods
    {
        public List<OrderByModel> JsonToOrderByModels(string json)
        {
            List<OrderByModel> conditionalModels = new List<OrderByModel>();
            var jarray = this.Context.Utilities.DeserializeObject<JArray>(json);
            foreach (var item in jarray)
            {
                if (IsFieldName(item))
                {
                    var model = item.ToObject<OrderByModel>();
                    conditionalModels.Add(model);
                }
                else if (item.Type == JTokenType.String)
                {
                    conditionalModels.Add(new OrderByModel() { FieldName = item.ObjToString().ToCheckField() });
                }
                else if (item.Type == JTokenType.Array)
                {
                    conditionalModels.Add(new OrderByModel()
                    {
                        FieldName = item[0].ObjToString(),
                        OrderByType = item[1].ObjToString().Equals("desc", StringComparison.CurrentCultureIgnoreCase) ? OrderByType.Desc : OrderByType.Asc
                    });
                }
            }
            return conditionalModels;
        }
    }
}
