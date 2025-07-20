using System.Text.RegularExpressions;
namespace ThingsGateway.SqlSugar
{
    public static class DbExtensions
    {
        public static string ToJoinSqlInVals<T>(this IEnumerable<T> array)
        {
            if (array?.Any() != true)
            {
                return ToSqlValue(string.Empty);
            }
            else
            {
                return string.Join(",", array.Where(c => c != null).Select(it => it.ToSqlValue()));
            }
        }
        public static string ToJoinSqlInVals<T>(this IReadOnlyList<T> array)
        {
            if (array == null || array.Count == 0)
            {
                return ToSqlValue(string.Empty);
            }
            else
            {
                return string.Join(",", array.Where(c => c != null).Select(it => it.ToSqlValue()));
            }
        }
        public static string ToJoinSqlInValsByVarchar<T>(this IEnumerable<T> array)
        {
            if (array?.Any() != true)
            {
                return ToSqlValue(string.Empty);
            }
            else
            {
                return string.Join(",", array.Where(c => c != null).Select(it => "N" + it.ToSqlValue()));
            }
        }
        public static string ToJoinSqlInValsByVarchar<T>(this IReadOnlyList<T> array)
        {
            if (array == null || array.Count == 0)
            {
                return ToSqlValue(string.Empty);
            }
            else
            {
                return string.Join(",", array.Where(c => c != null).Select(it => "N" + it.ToSqlValue()));
            }
        }
        public static string ToJoinSqlInValsN<T>(this IReadOnlyList<T> array)
        {
            if (array == null || array.Count == 0)
            {
                return ToSqlValue(string.Empty);
            }
            else
            {
                return string.Join(",", array.Where(c => c != null).Select(it => "N" + it.ToSqlValue()));
            }
        }
        public static string ToSqlValue(this object value)
        {
            if (value != null && UtilConstants.NumericalTypes.Contains(value.GetType()))
                return value.ToString();

            var str = value + "";
            return str.ToSqlValue();
        }

        public static string ToSqlValue(this string value)
        {
            return string.Format("'{0}'", value.ToSqlFilter());
        }

        /// <summary>
        ///Sql Filter
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToSqlFilter(this string value)
        {
            if (!value.IsNullOrEmpty())
            {
                var oldLength = value.Length;
                value = value.Replace("'", "''");
                if (oldLength != value.Length && value.IndexOf(')') > 0 && value.IndexOf(@"\''") > 0) value = value.Replace("\\", "\\\\");
            }
            return value;
        }

        /// <summary>
        /// Check field format
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToCheckField(this string value)
        {
            //You can override it because the default security level is very high
            if (StaticConfig.Check_FieldFunc != null)
            {
                return StaticConfig.Check_FieldFunc(value);
            }

            //Default method
            else if (value != null)
            {
                if (value.IsContainsIn(";", "--"))
                {
                    throw new Exception($"{value} format error ");
                }
                else if (value.IsContainsIn("//") && (value.Length - value.Replace("/", "").Length) >= 4)
                {
                    throw new Exception($"{value} format error ");
                }
                else if (value.IsContainsIn('\'') && (value.Length - value.Replace("'", "").Length) % 2 != 0)
                {
                    throw new Exception($"{value} format error ");
                }
                else if (IsUpdateSql(value, "/", "/"))
                {
                    Check.ExceptionEasy($"{value} format error  ", value + "不能存在  /+【update drop 等】+/ ");
                }
                else if (IsUpdateSql(value, "/", " "))
                {
                    Check.ExceptionEasy($"{value} format error  ", value + "不能存在  /+【update drop 等】+空格 ");
                }
                else if (IsUpdateSql(value, " ", "/"))
                {
                    Check.ExceptionEasy($"{value} format error  ", value + "不能存在  空格+【update drop 等】+/ ");
                }
                else if (value.Contains(" update ", StringComparison.CurrentCultureIgnoreCase)
                    || value.Contains(" delete ", StringComparison.CurrentCultureIgnoreCase)
                    || value.Contains(" drop ", StringComparison.CurrentCultureIgnoreCase)
                    || value.Contains(" alert ", StringComparison.CurrentCultureIgnoreCase)
                    || value.Contains(" create ", StringComparison.CurrentCultureIgnoreCase)
                    || value.Contains(" insert ", StringComparison.CurrentCultureIgnoreCase))
                {
                    Check.ExceptionEasy($"{value} format error  ", value + "不能存在  空格+【update drop 等】+空格 ");
                }
            }
            return value;
        }

        private static bool IsUpdateSql(string value, string left, string right)
        {
            return value.Contains(left + "update" + right, StringComparison.CurrentCultureIgnoreCase)
                             || value.Contains(left + "delete" + right, StringComparison.CurrentCultureIgnoreCase)
                             || value.Contains(left + "drop" + right, StringComparison.CurrentCultureIgnoreCase)
                             || value.Contains(left + "alert" + right, StringComparison.CurrentCultureIgnoreCase)
                             || value.Contains(left + "create" + right, StringComparison.CurrentCultureIgnoreCase)
                             || value.Contains(left + "insert" + right, StringComparison.CurrentCultureIgnoreCase);
        }
        public static bool ContainsChinese(string input)
        {
            // 正则表达式：匹配包含至少一个中文字符的字符串
            string pattern = @"[\u4e00-\u9fa5]";
            return Regex.IsMatch(input, pattern);
        }
        public static bool IsRegexWNoContainsChinese(this string value)
        {
            if (!ContainsChinese(value) && Regex.IsMatch(value, @"^\w+$"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static string ToCheckRegexW(this string value)
        {
            if (Regex.IsMatch(value, @"^\w+$"))
            {
                return value;
            }
            else
            {
                throw new Exception($"ToCheckRegexW {value} format error ");
            }
        }
        internal static string ToLower(this string value, bool isAutoToLower)
        {
            if (value == null) return null;
            if (isAutoToLower == false) return value;
            return value.ToLower();
        }
        internal static string ToUpper(this string value, bool isAutoToUpper)
        {
            if (value == null) return null;
            if (isAutoToUpper == false) return value;
            return value.ToUpper();
        }
    }
}
