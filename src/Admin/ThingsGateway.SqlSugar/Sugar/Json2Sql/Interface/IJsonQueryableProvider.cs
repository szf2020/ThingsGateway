namespace ThingsGateway.SqlSugar
{
    public interface IJsonQueryableProvider<JsonQueryResult> : IJsonProvider<JsonQueryResult>
    {
        IJsonQueryableProvider<JsonQueryResult> ShowDesciption();
        IJsonQueryableProvider<JsonQueryResult> UseAuthentication(JsonTableConfig config);
        IJsonQueryableProvider<JsonQueryResult> UseAuthentication(List<JsonTableConfig> config);
    }
}
