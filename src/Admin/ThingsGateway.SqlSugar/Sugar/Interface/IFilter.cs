namespace ThingsGateway.SqlSugar
{
    public interface IFilter
    {
        IFilter Add(SqlFilterItem filter);
        void Remove(string filterName);
        List<SqlFilterItem> GetFilterList { get; }
    }
}
