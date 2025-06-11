namespace ThingsGateway.SqlSugar.TDengine
{
    public class STable
    {
        [SugarColumn(IsIgnore = true)]
        public string TagsTypeId { get; set; }
        public static List<ColumnTagInfo> Tags = null;
    }
    public class ColumnTagInfo
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
