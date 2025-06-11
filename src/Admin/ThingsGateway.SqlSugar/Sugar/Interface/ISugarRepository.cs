namespace ThingsGateway.SqlSugar
{
    public interface ISugarRepository
    {
        ISqlSugarClient Context { get; set; }
    }
}
