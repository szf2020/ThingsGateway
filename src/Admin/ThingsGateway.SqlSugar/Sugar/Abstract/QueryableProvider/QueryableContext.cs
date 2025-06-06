namespace SqlSugar
{
    public class MapperContext<T>
    {
        public ISqlSugarClient context { get; set; }

        public List<T> list { get; set; }
        public Dictionary<string, object> TempChildLists { get; set; }
    }
}
