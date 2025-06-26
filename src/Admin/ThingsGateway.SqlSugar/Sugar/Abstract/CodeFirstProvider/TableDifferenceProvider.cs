using System.Text;
namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 表差异提供者
    /// </summary>
    public class TableDifferenceProvider
    {
        /// <summary>
        /// 表差异信息列表
        /// </summary>
        internal List<DiffTableInfo> tableInfos = new List<DiffTableInfo>();
        /// <summary>
        /// 获取差异字符串
        /// </summary>
        /// <returns>差异字符串</returns>
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

        /// <summary>
        /// 获取差异列表
        /// </summary>
        /// <returns>表差异信息列表</returns>
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

        /// <summary>
        /// 获取删除的列
        /// </summary>
        /// <param name="tableInfo">表差异信息</param>
        /// <returns>差异列信息列表</returns>
        private static List<DiffColumnsInfo> GetDeleteColumn(DiffTableInfo tableInfo)
        {
            List<DiffColumnsInfo> result = new List<DiffColumnsInfo>();
            var columns = tableInfo.OldColumnInfos.Where(z => !tableInfo.NewColumnInfos.Any(y => y.DbColumnName.EqualCase(z.DbColumnName)));
            return columns.Select(it => new DiffColumnsInfo() { Message = GetColumnString(it) }).ToList();
        }

        /// <summary>
        /// 获取更新的列
        /// </summary>
        /// <param name="tableInfo">表差异信息</param>
        /// <returns>差异列信息列表</returns>
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

        /// <summary>
        /// 获取新增的列
        /// </summary>
        /// <param name="tableInfo">表差异信息</param>
        /// <returns>差异列信息列表</returns>
        private static List<DiffColumnsInfo> GetAddColumn(DiffTableInfo tableInfo)
        {
            List<DiffColumnsInfo> result = new List<DiffColumnsInfo>();
            var columns = tableInfo.NewColumnInfos.Where(z => !tableInfo.OldColumnInfos.Any(y => y.DbColumnName.EqualCase(z.DbColumnName)));
            return columns.Select(it => new DiffColumnsInfo() { Message = GetColumnString(it) }).ToList();
        }

        /// <summary>
        /// 获取列信息字符串
        /// </summary>
        /// <param name="it">数据库列信息</param>
        /// <returns>列信息字符串</returns>
        private static string GetColumnString(DbColumnInfo it)
        {
            return $"{it.DbColumnName}  {it.DataType}  {it.Length} {it.Scale}   default:{it.DefaultValue} description:{it.ColumnDescription} pk:{it.IsPrimarykey} nullable:{it.IsNullable} identity:{it.IsIdentity} ";
        }

        /// <summary>
        /// 获取更新列信息字符串
        /// </summary>
        /// <param name="it">新列信息</param>
        /// <param name="old">旧列信息</param>
        /// <returns>更新信息字符串</returns>
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
    /// <summary>
    /// 表差异信息
    /// </summary>
    public class TableDifferenceInfo
    {
        /// <summary>
        /// 删除的列
        /// </summary>
        public List<DiffColumnsInfo> DeleteColumns { get; set; } = new List<DiffColumnsInfo>();
        /// <summary>
        /// 更新的列
        /// </summary>
        public List<DiffColumnsInfo> UpdateColumns { get; set; } = new List<DiffColumnsInfo>();
        /// <summary>
        /// 新增的列
        /// </summary>
        public List<DiffColumnsInfo> AddColumns { get; set; } = new List<DiffColumnsInfo>();
        /// <summary>
        /// 更新的备注
        /// </summary>
        public List<DiffColumnsInfo> UpdateRemark { get; set; } = new List<DiffColumnsInfo>();
        /// <summary>
        /// 是否有差异
        /// </summary>
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

        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }
    }

    /// <summary>
    /// 差异列信息
    /// </summary>
    public class DiffColumnsInfo
    {
        /// <summary>
        /// SQL模板
        /// </summary>
        public string SqlTemplate { get; set; }
        /// <summary>
        /// 差异信息
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// 表差异信息
    /// </summary>
    public class DiffTableInfo
    {
        /// <summary>
        /// 旧表信息
        /// </summary>
        public DbTableInfo OldTableInfo { get; set; }
        /// <summary>
        /// 新表信息
        /// </summary>
        public DbTableInfo NewTableInfo { get; set; }
        /// <summary>
        /// 旧列信息列表
        /// </summary>
        public List<DbColumnInfo> OldColumnInfos { get; set; }
        /// <summary>
        /// 新列信息列表
        /// </summary>
        public List<DbColumnInfo> NewColumnInfos { get; set; }
    }
}