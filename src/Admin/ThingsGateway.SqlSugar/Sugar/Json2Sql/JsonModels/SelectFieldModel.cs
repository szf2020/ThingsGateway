namespace ThingsGateway.SqlSugar
{
    public class SelectModel
    {
        public object FieldName { get; set; }

        public string AsName { get; set; }

        public static List<SelectModel> Create(params SelectModel[] SelectModels)
        {
            return SelectModels.ToList();
        }
    }
}
