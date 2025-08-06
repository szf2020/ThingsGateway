namespace ThingsGateway.SqlSugar
{
    public static class QuestDbSqlSugarClientExtensions
    {
        public static QuestDbRestAPI RestApi(this ISqlSugarClient db, int httpPort = 9000)
        {
            return new QuestDbRestAPI(db, httpPort);
        }
    }
}
