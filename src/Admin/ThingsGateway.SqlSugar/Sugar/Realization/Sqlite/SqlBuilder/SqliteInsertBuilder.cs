using System.Text;

namespace ThingsGateway.SqlSugar
{
    public class SqliteInsertBuilder : InsertBuilder
    {
        private const string InsertPrefix = "INSERT INTO ";
        private const string InsertIgnorePrefix = "INSERT OR IGNORE  INTO  ";

        public override string SqlTemplate =>
            IsReturnIdentity
                ? @"INSERT INTO {0} ({1}) VALUES ({2});SELECT LAST_INSERT_ROWID();"
                : @"INSERT INTO {0} ({1}) VALUES ({2});";

        public override string SqlTemplateBatch => "INSERT INTO {0} ({1})";

        public override string ToSqlString()
        {
            // 过滤掉 null 值
            if (IsNoInsertNull)
                DbColumnInfoList = DbColumnInfoList.Where(it => it.Value != null).ToList();

            // 按 TableId 分组
            var groupList = DbColumnInfoList.GroupBy(it => it.TableId).ToList();
            var firstGroup = groupList[0];

            // 列名缓存
            var columnNames = string.Join(",", firstGroup.Select(it => Builder.GetTranslationColumnName(it.DbColumnName)));

            if (groupList.Count == 1)
            {
                // 单条插入
                var columnValues = string.Join(",", DbColumnInfoList.Select(it =>
                    base.GetDbColumn(it, Builder.SqlParameterKeyWord + it.DbColumnName)));

                ActionMinDate();
                return string.Format(SqlTemplate, GetTableNameString, columnNames, columnValues);
            }

            // 批量插入
            var sb = new StringBuilder(256 + groupList.Count * 64);
            sb.Append(InsertPrefix).Append(GetTableNameString).Append(" (").Append(columnNames).Append(") VALUES");

            var groupCount = groupList.Count;
            int i = 0;

            foreach (var group in groupList)
            {
                sb.Append('(');

                int itemCount = 0;
                foreach (var col in group)
                {
                    if (itemCount++ > 0) sb.Append(',');
                    sb.Append(base.GetDbColumn(col, FormatValue(i, col.DbColumnName, col.Value)));
                }

                sb.Append(i == groupCount - 1 ? ") " : "), ");
                i++;
            }

            sb.AppendLine(";SELECT LAST_INSERT_ROWID();");

            if (MySqlIgnore)
            {
                sb.Remove(0, InsertPrefix.Length);
                sb.Insert(0, InsertIgnorePrefix);
            }

            return sb.ToString();
        }

        public object FormatValue(int index, string name, object value)
        {
            if (value == null)
                return "NULL";

            var type = UtilMethods.GetUnderType(value.GetType());

            if (type == UtilConstants.DateType)
            {
                var date = value.ObjToDate();
                var minDate = UtilMethods.GetMinDate(Context.CurrentConnectionConfig);
                if (date < minDate) date = minDate;

                var format = Context.CurrentConnectionConfig?.MoreSettings?.DisableMillisecond == true
                    ? "yyyy-MM-dd HH:mm:ss"
                    : "yyyy-MM-dd HH:mm:ss.fff";
                return $"'{date.ToString(format)}'";
            }

            if (type.IsEnum())
            {
                if (Context.CurrentConnectionConfig.MoreSettings?.TableEnumIsString == true)
                    return value.ToSqlValue();
                else
                    return Convert.ToInt64(value);
            }

            if (type == UtilConstants.DateTimeOffsetType)
                return GetDateTimeOffsetString(value);

            if (type == UtilConstants.ByteArrayType)
            {
                var parameterName = $"{Builder.SqlParameterKeyWord}{name}{index}";
                Parameters.Add(new SugarParameter(parameterName, value));
                return parameterName;
            }

            if (type == UtilConstants.BoolType)
                return ((bool)value) ? "1" : "0";

            if (type == UtilConstants.StringType || type == UtilConstants.ObjType)
                return $"'{value.ToString().ToSqlFilter()}'";

            return $"'{value}'";
        }

        private string GetDateTimeOffsetString(object value)
        {
            var date = UtilMethods.ConvertFromDateTimeOffset((DateTimeOffset)value);
            var minDate = UtilMethods.GetMinDate(Context.CurrentConnectionConfig);
            if (date < minDate) date = minDate;
            return $"'{date:yyyy-MM-dd HH:mm:ss.fffffff}'";
        }
    }
}
