namespace ThingsGateway.SqlSugar
{
    public sealed class SnowFlakeSingle
    {
        private static readonly object LockObject = new object();
        private static DistributedSystem.Snowflake.IdWorker worker;
        public static int WorkId = 1;
        public static int DatacenterId = 1;
        private SnowFlakeSingle()
        {
        }
        static SnowFlakeSingle() { }
        public static DistributedSystem.Snowflake.IdWorker Instance
        {
            get
            {
                if (worker == null)
                {
                    lock (LockObject)
                    {
                        if (worker == null)
                        {
                            worker = new DistributedSystem.Snowflake.IdWorker(WorkId, DatacenterId);
                        }
                    }
                }
                return worker;
            }
        }
        public static DistributedSystem.Snowflake.IdWorker instance
        {
            get
            {
                return Instance;
            }
        }
    }
}
