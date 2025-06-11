namespace ThingsGateway.SqlSugar
{
    internal class NavContext
    {
        public List<NavContextItem> Items { get; set; }
    }
    internal class NavContextItem
    {
        public int Level { get; set; }
        public string RootName { get; set; }
    }
}
