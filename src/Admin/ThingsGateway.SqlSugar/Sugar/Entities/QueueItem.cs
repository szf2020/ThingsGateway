namespace ThingsGateway.SqlSugar
{
    public class QueueItem
    {
        public string Sql { get; set; }
        public IReadOnlyCollection<SugarParameter> Parameters { get; set; }
    }
}
