namespace ThingsGateway.SqlSugar
{
    public interface IReportable<T>
    {
        //IReportable<T> MakeUp(Func<T,object> auto);
        ISugarQueryable<T> ToQueryable();
        ISugarQueryable<SingleColumnEntity<Y>> ToQueryable<Y>();
        ISugarQueryable<SingleColumnEntity<Y>> ToQueryable<Y>(bool onlySelectEntity);
    }
}
