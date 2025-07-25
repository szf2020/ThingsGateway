namespace ThingsGateway.SqlSugar
{
    public partial class JsonUpdateableProvider : IJsonUpdateableProvider<JsonUpdateResult>
    {
        private static bool IsColumns(string name)
        {
            return name.Equals("Columns", StringComparison.CurrentCultureIgnoreCase);
        }

        private static bool IsWhere(string name)
        {
            return name.Equals(nameof(QueryMethodInfo.Where), StringComparison.CurrentCultureIgnoreCase);
        }

        private static bool IsWhereColumns(string name)
        {
            return name.Equals("WhereColumns", StringComparison.CurrentCultureIgnoreCase);
        }

        private static bool IsTable(string name)
        {
            return name.Equals(JsonProviderConfig.KeyUpdateable.Get(), StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
