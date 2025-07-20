namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// Json Model to sql
    /// </summary>
    public partial interface ISqlBuilder
    {
        KeyValuePair<string, IReadOnlyList<SugarParameter>> OrderByModelToSql(List<OrderByModel> models);
        KeyValuePair<string, IReadOnlyList<SugarParameter>> GroupByModelToSql(List<GroupByModel> models);
        KeyValuePair<string, IReadOnlyList<SugarParameter>> SelectModelToSql(List<SelectModel> models);
        KeyValuePair<string, IReadOnlyList<SugarParameter>> FuncModelToSql(IFuncModel model);
    }

}
