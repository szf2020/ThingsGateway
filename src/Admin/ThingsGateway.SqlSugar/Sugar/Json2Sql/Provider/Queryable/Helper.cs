using Newtonsoft.Json.Linq;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// Helper
    /// </summary>
    public partial class JsonQueryableProvider : IJsonQueryableProvider<JsonQueryResult>
    {
        private static bool IsJoin(string name)
        {
            return name.StartsWith("LeftJoin", StringComparison.CurrentCultureIgnoreCase) || name.StartsWith("RightJoin", StringComparison.CurrentCultureIgnoreCase) || name.StartsWith("InnerJoin", StringComparison.CurrentCultureIgnoreCase);
        }
        private static bool IsJoinLastAfter(string name)
        {
            return name.Equals("joinlastafter", StringComparison.CurrentCultureIgnoreCase);
        }

        private static bool IsPageSize(string name)
        {
            return name.Equals("PageSize", StringComparison.CurrentCultureIgnoreCase);
        }

        private static bool IsPageNumber(string name)
        {
            return name.Equals("PageNumber", StringComparison.CurrentCultureIgnoreCase);
        }

        private static bool IsSelect(string name)
        {
            return name.Equals("Select", StringComparison.CurrentCultureIgnoreCase);
        }

        private static bool IsHaving(string name)
        {
            return name.Equals("Having", StringComparison.CurrentCultureIgnoreCase);
        }

        private static bool IsGroupBy(string name)
        {
            return name.Equals("GroupBy", StringComparison.CurrentCultureIgnoreCase);
        }

        private static bool IsOrderBy(string name)
        {
            return name.Equals("OrderBy", StringComparison.CurrentCultureIgnoreCase);
        }

        private static bool IsWhere(string name)
        {
            return name.Equals("Where", StringComparison.CurrentCultureIgnoreCase);
        }

        private static bool IsForm(string name)
        {
            return name.Equals(JsonProviderConfig.KeyQueryable.Get(), StringComparison.CurrentCultureIgnoreCase);
        }

        private static bool IsAnySelect(List<JToken> appendTypeNames)
        {
            return appendTypeNames.Any(it => IsSelect(it.Path));
        }
        private static bool IsAnyJoin(List<JToken> appendTypeNames)
        {
            return appendTypeNames.Any(it => IsJoin(it.Path));
        }
        private int GetSort(string name)
        {
            if (IsForm(name))
            {
                return 0;
            }
            else if (IsJoin(name))
            {
                return 1;
            }
            else if (IsJoinLastAfter(name))
            {
                return 2;
            }
            else
            {
                return 100;
            }
        }
        private void AddMasterTableInfos(JsonTableNameInfo tableNameInfo)
        {
            AddTableInfos(tableNameInfo.TableName, tableNameInfo.ShortName, true);
        }
        private void AddTableInfos(string tableName, string shortName, bool isMaster = false)
        {
            UtilMethods.IsNullReturnNew(TableInfos);
            TableInfos.Add(new JsonQueryableProvider_TableInfo() { Table = tableName, ShortName = shortName, IsMaster = true });
        }
        private JsonQueryableProvider_TableInfo GetMasterTable()
        {
            return this.TableInfos.First(it => it.IsMaster);
        }
    }
}
