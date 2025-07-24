using System.Linq.Expressions;

namespace ThingsGateway.SqlSugar
{
    public interface IDeleteable<T> where T : class, new()
    {
        DeleteBuilder DeleteBuilder { get; set; }
        int ExecuteCommand();
        bool ExecuteCommandHasChange();
        Task<int> ExecuteCommandAsync();
        Task<int> ExecuteCommandAsync(CancellationToken token);
        Task<bool> ExecuteCommandHasChangeAsync();
        IDeleteable<T> AS(string tableName);
        IDeleteable<T> AsType(Type tableNameType);
        IDeleteable<T> With(string lockString);
        IDeleteable<T> WhereT(T deleteObj);
        IDeleteable<T> WhereIF(bool isWhere, Expression<Func<T, bool>> expression);
        IDeleteable<T> Where(Expression<Func<T, bool>> expression);
        IDeleteable<T> Where(IReadOnlyCollection<T> deleteObjs);
        DeleteablePage<T> PageSize(int pageSize);
        IDeleteable<T> InT<PkType>(PkType primaryKeyValue);
        IDeleteable<T> In<PkType>(IReadOnlyCollection<PkType> primaryKeyValues);
        IDeleteable<T> InT<PkType>(Expression<Func<T, object>> inField, PkType primaryKeyValue);
        IDeleteable<T> In<PkType>(Expression<Func<T, object>> inField, IReadOnlyCollection<PkType> primaryKeyValues);
        IDeleteable<T> In<PkType>(Expression<Func<T, object>> inField, ISugarQueryable<PkType> childQueryExpression);

        IDeleteable<T> In<PkType>(string inField, IReadOnlyCollection<PkType> primaryKeyValues);
        IDeleteable<T> Where(string whereString, object parameters = null);
        IDeleteable<T> Where(string whereString, SugarParameter parameter);
        IDeleteable<T> Where(string whereString, IReadOnlyCollection<SugarParameter> parameters);
        IDeleteable<T> Where(string whereString, List<SugarParameter> parameters);
        IDeleteable<T> WhereColumns(T data, Expression<Func<T, object>> columns);
        IDeleteable<T> WhereColumns(List<T> list, Expression<Func<T, object>> columns);
        IDeleteable<T> WhereColumns(List<Dictionary<string, object>> columns);
        IDeleteable<T> Where(List<IConditionalModel> conditionalModels);
        IDeleteable<T> Where(List<IConditionalModel> conditionalModels, bool isWrap);
        IDeleteable<T> EnableDiffLogEventIF(bool isEnableDiffLogEvent, object businessData = null);
        IDeleteable<T> EnableDiffLogEvent(object businessData = null);
        IDeleteable<T> RemoveDataCache();
        IDeleteable<T> RemoveDataCache(string likeString);
        KeyValuePair<string, IReadOnlyCollection<SugarParameter>> ToSql();
        string ToSqlString();
        IDeleteable<T> EnableQueryFilter();
        IDeleteable<T> EnableQueryFilter(Type type);
        SplitTableDeleteProvider<T> SplitTable(Func<List<SplitTableInfo>, IEnumerable<SplitTableInfo>> getTableNamesFunc);
        SplitTableDeleteByObjectProvider<T> SplitTable();
        LogicDeleteProvider<T> IsLogic();
        void AddQueue();
    }
}
