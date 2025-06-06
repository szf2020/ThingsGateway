namespace SqlSugar
{
    public interface ICustomConditionalFunc
    {
        KeyValuePair<string, SugarParameter[]> GetConditionalSql(ConditionalModel json, int index);
    }
}
