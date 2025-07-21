namespace ThingsGateway.SqlSugar
{
    public class SqlObjectResult
    {
        public SqlObjectResult(KeyValuePair<string, IReadOnlyList<SugarParameter>> keyValuePair, JsonProviderType jsonSqlType)
        {
            this.Sql = keyValuePair.Key;
            this.Parameters = keyValuePair.Value;
            this.JsonSqlType = jsonSqlType;
        }

        public JsonProviderType JsonSqlType { get; set; }
        public string Sql { get; set; }
        public IReadOnlyList<SugarParameter> Parameters { get; set; }
    }
}
