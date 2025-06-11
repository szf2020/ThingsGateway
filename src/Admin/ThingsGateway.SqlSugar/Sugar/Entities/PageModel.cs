namespace ThingsGateway.SqlSugar
{
    public class PageModel
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        /// <summary>
        /// output
        /// </summary>
        public int TotalCount { get; set; }
    }
}
