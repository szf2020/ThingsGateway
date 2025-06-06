namespace SqlSugar.SplitTableExtensions
{
    public static class Extensions
    {
        public static List<T> SplitTable<T>(this List<T> thisValue, Func<List<SplitTableInfo>, IEnumerable<SplitTableInfo>> getTableNamesFunc)
        {
            return thisValue;
        }
        public static List<T> SplitTable<T>(this T thisValue, Func<List<SplitTableInfo>, IEnumerable<SplitTableInfo>> getTableNamesFunc)
        {
            return new List<T>();
        }
    }
}
