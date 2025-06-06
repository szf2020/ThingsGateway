namespace SqlSugar
{
    public class ReSetValueBySqlExpListModel
    {
        public string DbColumnName { get; set; }
        public string Sql { get; set; }
        public ReSetValueBySqlExpListModelType? Type { get; set; }
    }
    public enum ReSetValueBySqlExpListModelType
    {
        Default,
        List
    }
}
