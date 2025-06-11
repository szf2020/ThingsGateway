namespace ThingsGateway.SqlSugar
{
    public static class SplitTableInfoExtensions
    {
        public static IEnumerable<SplitTableInfo> InTableNames(this List<SplitTableInfo> tables, params string[] tableNames)
        {
            return tables.Where(it => tableNames.Any(y => y.Equals(it.TableName, StringComparison.OrdinalIgnoreCase)));
        }
        public static IEnumerable<SplitTableInfo> ContainsTableNames(this List<SplitTableInfo> tables, params string[] tableNames)
        {
            List<SplitTableInfo> result = new List<SplitTableInfo>();
            foreach (var item in tables)
            {
                if (tableNames.Any(it => item.TableName.ObjToString().Contains(it.ObjToString(), StringComparison.CurrentCultureIgnoreCase)))
                {
                    result.Add(item);
                }
            }
            return result;
        }
        public static IEnumerable<SplitTableInfo> ContainsTableNamesIfNullDefaultFirst(this List<SplitTableInfo> tables, params string[] tableNames)
        {
            List<SplitTableInfo> result = new List<SplitTableInfo>();
            foreach (var item in tables)
            {
                if (tableNames.Any(it => item.TableName.ObjToString().Contains(it.ObjToString(), StringComparison.CurrentCultureIgnoreCase)))
                {
                    result.Add(item);
                }
            }
            if (result.Count == 0 && tables.Count != 0)
            {
                result.Add(tables.First());
            }
            return result;
        }
    }
}
