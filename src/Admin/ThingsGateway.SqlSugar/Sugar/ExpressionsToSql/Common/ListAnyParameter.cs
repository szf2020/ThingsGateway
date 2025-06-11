namespace ThingsGateway.SqlSugar
{
    internal class ListAnyParameter
    {
        public string Name { get; internal set; }
        public string Sql { get; internal set; }
        public List<EntityColumnInfo> Columns { get; internal set; }
        public Func<string, string> ConvetColumnFunc { get; internal set; }
    }
}
