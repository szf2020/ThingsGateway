namespace SqlSugar
{
    public class OrderByModel
    {
        public object FieldName { get; set; }
        public OrderByType OrderByType { get; set; }
        public static List<OrderByModel> Create(params OrderByModel[] orderByModel)
        {
            return orderByModel.ToList();
        }
    }
}
