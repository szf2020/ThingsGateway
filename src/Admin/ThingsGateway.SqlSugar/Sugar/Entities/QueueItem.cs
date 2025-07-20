namespace ThingsGateway.SqlSugar
{
    public class QueueItem
    {
        public string Sql { get; set; }
        public IReadOnlyList<SugarParameter> Parameters { get; set; }
    }
}
