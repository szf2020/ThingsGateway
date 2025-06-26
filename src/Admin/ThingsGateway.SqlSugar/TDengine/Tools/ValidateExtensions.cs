using System.Text.RegularExpressions;
namespace ThingsGateway.SqlSugar.TDengine
{
    /// <summary>
    /// 验证扩展方法类
    /// </summary>
    internal static class ValidateExtensions
    {
        /// <summary>
        /// 检查整数值是否在指定范围内
        /// </summary>
        public static bool IsInRange(this int thisValue, int begin, int end)
        {
            return thisValue >= begin && thisValue <= end;
        }

        /// <summary>
        /// 检查日期时间是否在指定范围内
        /// </summary>
        public static bool IsInRange(this DateTime thisValue, DateTime begin, DateTime end)
        {
            return thisValue >= begin && thisValue <= end;
        }

        /// <summary>
        /// 检查值是否在给定的参数列表中
        /// </summary>
        public static bool IsIn<T>(this T thisValue, params T[] values)
        {
            return values.Contains(thisValue);
        }

        /// <summary>
        /// 检查字符串是否包含任一给定的子字符串
        /// </summary>
        public static bool IsContainsIn(this string thisValue, params string[] inValues)
        {
            return inValues.Any(it => thisValue.Contains(it));
        }

        /// <summary>
        /// 检查对象是否为null或空字符串
        /// </summary>
        public static bool IsNullOrEmpty(this object thisValue)
        {
            if (thisValue == null || thisValue == DBNull.Value) return true;
            return string.IsNullOrEmpty(thisValue.ToString());
        }

        /// <summary>
        /// 检查可空Guid是否为null或空Guid
        /// </summary>
        public static bool IsNullOrEmpty(this Guid? thisValue)
        {
            if (thisValue == null) return true;
            return thisValue == Guid.Empty;
        }

        /// <summary>
        /// 检查Guid是否为空Guid
        /// </summary>
        public static bool IsNullOrEmpty(this Guid thisValue)
        {
            return thisValue == Guid.Empty;
        }

        /// <summary>
        /// 检查集合是否为null或空
        /// </summary>
        public static bool IsNullOrEmpty(this IEnumerable<object> thisValue)
        {
            if (thisValue?.Any() != true) return true;
            return false;
        }

        /// <summary>
        /// 检查对象是否有值(不为null且不为空字符串)
        /// </summary>
        public static bool HasValue(this object thisValue)
        {
            if (thisValue == null || thisValue == DBNull.Value) return false;
            return !string.IsNullOrEmpty(thisValue.ToString());
        }

        /// <summary>
        /// 检查集合是否有值(不为null且包含元素)
        /// </summary>
        public static bool HasValue(this IEnumerable<object> thisValue)
        {
            if (thisValue?.Any() != true) return false;
            return true;
        }

        /// <summary>
        /// 检查键值对集合是否有值(不为null且包含元素)
        /// </summary>
        public static bool IsValuable(this IEnumerable<KeyValuePair<string, string>> thisValue)
        {
            if (thisValue?.Any() != true) return false;
            return true;
        }

        /// <summary>
        /// 检查对象是否为null或0
        /// </summary>
        public static bool IsZero(this object thisValue)
        {
            return (thisValue == null || thisValue.ToString() == "0");
        }

        /// <summary>
        /// 检查对象是否为整数
        /// </summary>
        public static bool IsInt(this object thisValue)
        {
            if (thisValue == null) return false;
            return Regex.IsMatch(thisValue.ToString(), @"^\d+$");
        }

        /// <summary>
        /// 检查对象是否不是整数
        /// </summary>
        public static bool IsNoInt(this object thisValue)
        {
            if (thisValue == null) return true;
            return !Regex.IsMatch(thisValue.ToString(), @"^\d+$");
        }

        /// <summary>
        /// 检查对象是否为有效的金额数值
        /// </summary>
        public static bool IsMoney(this object thisValue)
        {
            if (thisValue == null) return false;
            double outValue = 0;
            return double.TryParse(thisValue.ToString(), out outValue);
        }

        /// <summary>
        /// 检查对象是否为有效的Guid
        /// </summary>
        public static bool IsGuid(this object thisValue)
        {
            if (thisValue == null) return false;
            Guid outValue = Guid.Empty;
            return Guid.TryParse(thisValue.ToString(), out outValue);
        }

        /// <summary>
        /// 检查对象是否为有效的日期
        /// </summary>
        public static bool IsDate(this object thisValue)
        {
            if (thisValue == null) return false;
            DateTime outValue = DateTime.MinValue;
            return DateTime.TryParse(thisValue.ToString(), out outValue);
        }

        /// <summary>
        /// 检查字符串是否为有效的电子邮件格式
        /// </summary>
        public static bool IsEamil(this object thisValue)
        {
            if (thisValue == null) return false;
            return Regex.IsMatch(thisValue.ToString(), @"^[\w-]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)+$");
        }

        /// <summary>
        /// 检查字符串是否为有效的手机号码格式(11位数字)
        /// </summary>
        public static bool IsMobile(this object thisValue)
        {
            if (thisValue == null) return false;
            return Regex.IsMatch(thisValue.ToString(), @"^\d{11}$");
        }

        /// <summary>
        /// 检查字符串是否为有效的电话号码格式
        /// </summary>
        public static bool IsTelephone(this object thisValue)
        {
            if (thisValue == null) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(thisValue.ToString(), @"^(\(\d{3,4}\)|\d{3,4}-|\s)?\d{8}$");
        }

        /// <summary>
        /// 检查字符串是否为有效的身份证号码格式
        /// </summary>
        public static bool IsIDcard(this object thisValue)
        {
            if (thisValue == null) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(thisValue.ToString(), @"^(\d{15}$|^\d{18}$|^\d{17}(\d|X|x))$");
        }

        /// <summary>
        /// 检查字符串是否为有效的传真号码格式
        /// </summary>
        public static bool IsFax(this object thisValue)
        {
            if (thisValue == null) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(thisValue.ToString(), @"^[+]{0,1}(\d){1,3}[ ]?([-]?((\d)|[ ]){1,12})+$");
        }

        /// <summary>
        /// 检查字符串是否匹配指定的正则表达式模式
        /// </summary>
        public static bool IsMatch(this object thisValue, string pattern)
        {
            if (thisValue == null) return false;
            Regex reg = new Regex(pattern);
            return reg.IsMatch(thisValue.ToString());
        }

        /// <summary>
        /// 检查类型是否为匿名类型
        /// </summary>
        public static bool IsAnonymousType(this Type type)
        {
            string typeName = type.Name;
            return typeName.Contains("<>") && typeName.Contains("__") && typeName.Contains("AnonymousType");
        }

        /// <summary>
        /// 检查字符串是否表示泛型列表类型
        /// </summary>
        public static bool IsCollectionsList(this string thisValue)
        {
            return (thisValue + "").StartsWith("System.Collections.Generic.List") || (thisValue + "").StartsWith("System.Collections.Generic.IEnumerable");
        }

        /// <summary>
        /// 检查字符串是否表示字符串数组类型
        /// </summary>
        public static bool IsStringArray(this string thisValue)
        {
            return (thisValue + "").IsMatch(@"System\.[a-z,A-Z,0-9]+?\[\]");
        }

        /// <summary>
        /// 检查字符串是否表示可枚举类型
        /// </summary>
        public static bool IsEnumerable(this string thisValue)
        {
            return (thisValue + "").StartsWith("System.Linq.Enumerable");
        }

        /// <summary>
        /// 字符串类型引用
        /// </summary>
        public static Type StringType = typeof(string);

        /// <summary>
        /// 检查类型是否为类类型(排除字符串和字节数组)
        /// </summary>
        public static bool IsClass(this Type thisValue)
        {
            return thisValue != StringType && thisValue.IsEntity() && thisValue != UtilConstants.ByteArrayType;
        }
    }
}