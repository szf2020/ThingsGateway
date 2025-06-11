using System.Text;
namespace ThingsGateway.SqlSugar
{
    public class TableDifferenceProvider
    {
        internal List<DiffTableInfo> tableInfos = new List<DiffTableInfo>();
        public string ToDiffString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            var diffTables = this.ToDiffList();
            if (diffTables.IsNullOrEmpty())
            {
                sb.AppendLine("No change");
            }
            else
            {
                foreach (var item in diffTables)
                {
                    sb.AppendLine($"----Table:{item.TableName}----");
                    if (item.AddColumns.HasValue())
                    {
                        sb.AppendLine($"Add column: ");
                        foreach (var addItem in item.AddColumns)
                        {
                            sb.AppendLine($"{addItem.Message} ");
                        }
                    }
                    if (item.UpdateColumns.HasValue())
                    {
                        sb.AppendLine($"Update column: ");
                        foreach (var addItem in item.UpdateColumns)
                        {
                            sb.AppendLine($"{addItem.Message} ");
                        }
                    }
                    if (item.DeleteColumns.HasValue())
                    {
                        sb.AppendLine($"Delete column: ");
                        foreach (var addItem in item.DeleteColumns)
                        {
                            sb.AppendLine($"{addItem.Message} ");
                        }
                    }
                }
            }
            sb.AppendLine();
            sb.AppendLine();
            return sb.ToString();
        }

        public List<TableDifferenceInfo> ToDiffList()
        {
            List<TableDifferenceInfo> result = new List<TableDifferenceInfo>();
            foreach (var tableInfo in tableInfos)
            {
                TableDifferenceInfo addItem = new TableDifferenceInfo();
                if (tableInfo.OldTableInfo == null)
                    tableInfo.OldTableInfo = new DbTableInfo();
                addItem.TableName = tableInfo.OldTableInfo.Name;
                addItem.AddColumns = GetAddColumn(tableInfo);
                addItem.UpdateColumns = GetUpdateColumn(tableInfo);
                addItem.DeleteColumns = GetDeleteColumn(tableInfo);
                if (addItem.IsDiff)
                    result.Add(addItem);
            }
            return result;
        }

        private static List<DiffColumnsInfo> GetDeleteColumn(DiffTableInfo tableInfo)
        {
            List<DiffColumnsInfo> result = new List<DiffColumnsInfo>();
            var columns = tableInfo.OldColumnInfos.Where(z => !tableInfo.NewColumnInfos.Any(y => y.DbColumnName.EqualCase(z.DbColumnName)));
            return columns.Select(it => new DiffColumnsInfo() { Message = GetColumnString(it) }).ToList();
        }

        private List<DiffColumnsInfo> GetUpdateColumn(DiffTableInfo tableInfo)
        {
            var oldColumnDict = tableInfo.OldColumnInfos.ToDictionary(c => c.DbColumnName, StringComparer.OrdinalIgnoreCase);
            return tableInfo.NewColumnInfos
                .Where(newCol => oldColumnDict.TryGetValue(newCol.DbColumnName, out var oldCol) && (
                    newCol.Length != oldCol.Length ||
                    newCol.ColumnDescription != oldCol.ColumnDescription ||
                    newCol.DataType != oldCol.DataType ||
                    newCol.DecimalDigits != oldCol.DecimalDigits ||
                    newCol.IsPrimarykey != oldCol.IsPrimarykey ||
                    newCol.IsIdentity != oldCol.IsIdentity ||
                    newCol.IsNullable != oldCol.IsNullable))
                .Select(newCol =>
                {
                    var oldCol = oldColumnDict[newCol.DbColumnName];
                    return new DiffColumnsInfo
                    {
                        Message = GetUpdateColumnString(newCol, oldCol)
                    };
                }).ToList();
        }

        private static List<DiffColumnsInfo> GetAddColumn(DiffTableInfo tableInfo)
        {
            List<DiffColumnsInfo> result = new List<DiffColumnsInfo>();
            var columns = tableInfo.NewColumnInfos.Where(z => !tableInfo.OldColumnInfos.Any(y => y.DbColumnName.EqualCase(z.DbColumnName)));
            return columns.Select(it => new DiffColumnsInfo() { Message = GetColumnString(it) }).ToList();
        }

        private static string GetColumnString(DbColumnInfo it)
        {
            return $"{it.DbColumnName}  {it.DataType}  {it.Length} {it.Scale}   default:{it.DefaultValue} description:{it.ColumnDescription} pk:{it.IsPrimarykey} nullable:{it.IsNullable} identity:{it.IsIdentity} ";
        }

        private static string GetUpdateColumnString(DbColumnInfo it, DbColumnInfo old)
        {
            var result = $"{it.DbColumnName}  changes: ";
            if (it.DataType != old.DataType)
            {
                result += $"  [DataType:{old.DataType}->{it.DataType}] ";
            }
            if (it.Length != old.Length)
            {
                result += $"  [Length:{old.Length}->{it.Length}] ";
            }
            if (it.Scale != old.Scale)
            {
                result += $"  [Scale:{old.Scale}->{it.Scale}] ";
            }
            if (it.ColumnDescription != old.ColumnDescription)
            {
                result += $"  [Description:{old.ColumnDescription}->{it.ColumnDescription}] ";
            }
            if (it.IsPrimarykey != old.IsPrimarykey)
            {
                result += $"  [Pk:{old.IsPrimarykey}->{it.IsPrimarykey}] ";
            }
            if (it.IsNullable != old.IsNullable)
            {
                result += $"  [Nullable:{old.IsNullable}->{it.IsNullable}] ";
            }
            if (it.IsIdentity != old.IsIdentity)
            {
                result += $"  [Identity:{old.IsIdentity}->{it.IsIdentity}] ";
            }
            return result;
        }
    }
    public class TableDifferenceInfo
    {
        public List<DiffColumnsInfo> DeleteColumns { get; set; } = new List<DiffColumnsInfo>();
        public List<DiffColumnsInfo> UpdateColumns { get; set; } = new List<DiffColumnsInfo>();
        public List<DiffColumnsInfo> AddColumns { get; set; } = new List<DiffColumnsInfo>();
        public List<DiffColumnsInfo> UpdateRemark { get; set; } = new List<DiffColumnsInfo>();
        public bool IsDiff
        {
            get
            {
                return
                    (DeleteColumns.Count > 0 ||
                     UpdateColumns.Count > 0 ||
                     AddColumns.Count > 0 ||
                     UpdateRemark.Count > 0);
            }
        }

        public string TableName { get; set; }
    }

    public class DiffColumnsInfo
    {
        public string SqlTemplate { get; set; }
        public string Message { get; set; }
    }

    public class DiffTableInfo
    {
        public DbTableInfo OldTableInfo { get; set; }
        public DbTableInfo NewTableInfo { get; set; }
        public List<DbColumnInfo> OldColumnInfos { get; set; }
        public List<DbColumnInfo> NewColumnInfos { get; set; }
    }
}
