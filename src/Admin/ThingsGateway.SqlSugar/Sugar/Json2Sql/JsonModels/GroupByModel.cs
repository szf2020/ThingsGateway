namespace ThingsGateway.SqlSugar
{
    public class GroupByModel
    {
        public object FieldName { get; set; }
        public static List<GroupByModel> Create(params GroupByModel[] groupModels)
        {
            return groupModels.ToList();
        }
    }
}
