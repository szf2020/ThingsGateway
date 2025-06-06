namespace SqlSugar
{
    public interface ISugarRepository
    {
        ISqlSugarClient Context { get; set; }
    }
}
