namespace ThingsGateway.SqlSugar
{
    public class StackTraceInfo
    {
        public string FirstFileName { get { return this.MyStackTraceList[0].FileName; } }
        public string FirstMethodName { get { return this.MyStackTraceList[0].MethodName; } }
        public int FirstLine { get { return this.MyStackTraceList[0].Line; } }

        public List<StackTraceInfoItem> MyStackTraceList { get; set; }
        public List<StackTraceInfoItem> SugarStackTraceList { get; set; }
    }
    public class StackTraceInfoItem
    {
        public string FileName { get; set; }
        public string MethodName { get; set; }
        public int Line { get; set; }
    }
}
