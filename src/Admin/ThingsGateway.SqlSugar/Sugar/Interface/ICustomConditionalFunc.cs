namespace ThingsGateway.SqlSugar
{
    public interface ICustomConditionalFunc
    {
        KeyValuePair<string, IReadOnlyList<SugarParameter>> GetConditionalSql(ConditionalModel json, int index);
    }
}
