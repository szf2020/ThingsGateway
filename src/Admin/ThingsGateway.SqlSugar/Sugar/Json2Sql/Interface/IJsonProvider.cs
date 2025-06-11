namespace ThingsGateway.SqlSugar
{

    public interface IJsonProvider<T>
    {
        List<SqlObjectResult> ToSqlList();
        SqlObjectResult ToSql();
        List<string> ToSqlString();
        T ToResult();
    }
}
