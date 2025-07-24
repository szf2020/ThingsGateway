namespace ThingsGateway.SqlSugar
{
    public class SqlObjectResult
    {
        public SqlObjectResult(KeyValuePair<string, IReadOnlyCollection<SugarParameter>> keyValuePair, JsonProviderType jsonSqlType)
        {
            this.Sql = keyValuePair.Key;
            this.Parameters = keyValuePair.Value;
            this.JsonSqlType = jsonSqlType;
        }

        public JsonProviderType JsonSqlType { get; set; }
        public string Sql { get; set; }
        public IReadOnlyCollection<SugarParameter> Parameters { get; set; }
    }
}
