namespace ThingsGateway.SqlSugar
{
    public class DbFastestProperties
    {
        public bool HasOffsetTime { get; set; }
        public string[] WhereColumns { get; set; }
        public bool IsOffIdentity { get; set; }
        public bool IsMerge { get; set; }
        public bool IsNoCopyDataTable { get; set; }
        public bool IsConvertDateTimeOffsetToDateTime { get; set; }
        public bool NoPage { get; set; }
    }
}
