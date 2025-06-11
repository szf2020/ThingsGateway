namespace ThingsGateway.SqlSugar
{
    public static class QuestDbSqlSugarClientExtensions
    {
        public static QuestDbRestAPI RestApi(this ISqlSugarClient db)
        {
            return new QuestDbRestAPI(db);
        }
    }
}
