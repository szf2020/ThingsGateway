namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// Json Model to sql
    /// </summary>
    public partial interface ISqlBuilder
    {
        KeyValuePair<string, IReadOnlyCollection<SugarParameter>> OrderByModelToSql(List<OrderByModel> models);
        KeyValuePair<string, IReadOnlyCollection<SugarParameter>> GroupByModelToSql(List<GroupByModel> models);
        KeyValuePair<string, IReadOnlyCollection<SugarParameter>> SelectModelToSql(List<SelectModel> models);
        KeyValuePair<string, IReadOnlyCollection<SugarParameter>> FuncModelToSql(IFuncModel model);
    }
}
