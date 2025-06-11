namespace ThingsGateway.SqlSugar
{
    public interface IRazorService
    {
        List<KeyValuePair<string, string>> GetClassStringList(string razorTemplate, List<RazorTableInfo> model);
    }
}
