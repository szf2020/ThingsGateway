namespace ThingsGateway.SqlSugar
{
    public interface ICustomConditionalFunc
    {
        KeyValuePair<string, IReadOnlyCollection<SugarParameter>> GetConditionalSql(ConditionalModel json, int index);
    }
}
