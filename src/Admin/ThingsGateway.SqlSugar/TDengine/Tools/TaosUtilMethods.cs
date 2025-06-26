using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 实用工具方法类
    /// </summary>
    public static class TaosUtilMethods
    {
        /// <summary>
        /// 获取通用STable特性
        /// </summary>
        public static STableAttribute GetCommonSTableAttribute(ISqlSugarClient db, STableAttribute sTableAttribute)
        {
            var key = "GetCommonSTableAttribute_" + sTableAttribute?.STableName;
            if (db.TempItems?.ContainsKey(key) == true)
            {
                sTableAttribute.STableName = db.TempItems[key] + "";
            }
            return sTableAttribute!;
        }

        /// <summary>
        /// 将DateTime转换为Unix时间戳
        /// </summary>
        public static long ToUnixTimestamp(DateTime dateTime)
        {
            // 如果DateTime是UTC时间，直接使用ToUnixTimeMilliseconds
            if (dateTime.Kind == DateTimeKind.Utc)
            {
                return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
            }
            else
            {
                // 将本地DateTime转换为UTC时间后再转换为Unix时间戳
                return new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeMilliseconds();
            }
        }

        /// <summary>
        /// 获取最小日期
        /// </summary>
        internal static DateTime GetMinDate(ConnectionConfig currentConnectionConfig)
        {
            if (currentConnectionConfig.MoreSettings == null)
            {
                return Convert.ToDateTime("1900-01-01");
            }
            else if (currentConnectionConfig.MoreSettings.DbMinDate == null)
            {
                return Convert.ToDateTime("1900-01-01");
            }
            else
            {
                return currentConnectionConfig.MoreSettings.DbMinDate.Value;
            }
        }

        /// <summary>
        /// 从DateTimeOffset转换为DateTime
        /// </summary>
        internal static DateTime ConvertFromDateTimeOffset(DateTimeOffset dateTime)
        {
            if (dateTime.Offset.Equals(TimeSpan.Zero))
                return dateTime.UtcDateTime;
            else if (dateTime.Offset.Equals(TimeZoneInfo.Local.GetUtcOffset(dateTime.DateTime)))
                return DateTime.SpecifyKind(dateTime.DateTime, DateTimeKind.Local);
            else
                return dateTime.DateTime;
        }

        /// <summary>
        /// 将对象转换为指定类型
        /// </summary>
        internal static object To(object value, Type destinationType)
        {
            return To(value, destinationType, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 将对象转换为指定类型(使用特定文化信息)
        /// </summary>
        internal static object To(object value, Type destinationType, CultureInfo culture)
        {
            if (value != null)
            {
                destinationType = GetUnderType(destinationType);
                var sourceType = value.GetType();

                var destinationConverter = TypeDescriptor.GetConverter(destinationType);
                if (destinationConverter?.CanConvertFrom(value.GetType()) == true)
                    return destinationConverter.ConvertFrom(null, culture, value);

                var sourceConverter = TypeDescriptor.GetConverter(sourceType);
                if (sourceConverter?.CanConvertTo(destinationType) == true)
                    return sourceConverter.ConvertTo(null, culture, value, destinationType);

                if (destinationType.IsEnum && value is int)
                    return Enum.ToObject(destinationType, (int)value);

                if (!destinationType.IsInstanceOfType(value))
                    return Convert.ChangeType(value, destinationType, culture);
            }
            return value;
        }

        /// <summary>
        /// 检查堆栈帧中是否有异步方法
        /// </summary>
        public static bool IsAnyAsyncMethod(StackFrame[] methods)
        {
            bool isAsync = false;
            foreach (var item in methods)
            {
                if (IsAsyncMethod(item.GetMethod()))
                {
                    isAsync = true;
                    break;
                }
            }
            return isAsync;
        }

        /// <summary>
        /// 检查方法是否是异步方法
        /// </summary>
        public static bool IsAsyncMethod(MethodBase method)
        {
            if (method == null)
            {
                return false;
            }
            if (method.DeclaringType != null)
            {
                if (method.DeclaringType.GetInterfaces().Contains(typeof(IAsyncStateMachine)))
                {
                    return true;
                }
            }
            var name = method.Name;
            if (name.Contains("OutputAsyncCausalityEvents"))
            {
                return true;
            }
            if (name.Contains("OutputWaitEtwEvents"))
            {
                return true;
            }
            if (name.Contains("ExecuteAsync"))
            {
                return true;
            }
            Type attType = typeof(AsyncStateMachineAttribute);
            var attrib = (AsyncStateMachineAttribute)method.GetCustomAttribute(attType);
            return (attrib != null);
        }

        /// <summary>
        /// 获取堆栈跟踪信息
        /// </summary>
        public static StackTraceInfo GetStackTrace()
        {
            StackTrace st = new StackTrace(true);
            StackTraceInfo info = new StackTraceInfo();
            info.MyStackTraceList = new List<StackTraceInfoItem>();
            info.SugarStackTraceList = new List<StackTraceInfoItem>();
            for (int i = 0; i < st.FrameCount; i++)
            {
                var frame = st.GetFrame(i);
                if (!string.Equals(frame.GetMethod().Module.Name, "thingsgateway.sqlsugar.dll", StringComparison.OrdinalIgnoreCase) && frame.GetMethod().Name.First() != '<')
                {
                    info.MyStackTraceList.Add(new StackTraceInfoItem()
                    {
                        FileName = frame.GetFileName(),
                        MethodName = frame.GetMethod().Name,
                        Line = frame.GetFileLineNumber()
                    });
                }
                else
                {
                    info.SugarStackTraceList.Add(new StackTraceInfoItem()
                    {
                        FileName = frame.GetFileName(),
                        MethodName = frame.GetMethod().Name,
                        Line = frame.GetFileLineNumber()
                    });
                }
            }
            return info;
        }

        /// <summary>
        /// 将对象转换为泛型类型
        /// </summary>
        internal static T To<T>(object value)
        {
            return (T)To(value, typeof(T));
        }

        /// <summary>
        /// 获取基础类型(如果是Nullable类型则返回其基础类型)
        /// </summary>
        internal static Type GetUnderType(Type oldType)
        {
            Type type = Nullable.GetUnderlyingType(oldType);
            return type == null ? oldType : type;
        }

        /// <summary>
        /// 替换SQL参数名
        /// </summary>
        public static string ReplaceSqlParameter(string itemSql, SugarParameter itemParameter, string newName)
        {
            itemSql = Regex.Replace(itemSql, string.Format(@"{0} ", "\\" + itemParameter.ParameterName), newName + " ", RegexOptions.IgnoreCase);
            itemSql = Regex.Replace(itemSql, string.Format(@"{0}\)", "\\" + itemParameter.ParameterName), newName + ")", RegexOptions.IgnoreCase);
            itemSql = Regex.Replace(itemSql, string.Format(@"{0}\,", "\\" + itemParameter.ParameterName), newName + ",", RegexOptions.IgnoreCase);
            itemSql = Regex.Replace(itemSql, string.Format(@"{0}$", "\\" + itemParameter.ParameterName), newName, RegexOptions.IgnoreCase);
            itemSql = Regex.Replace(itemSql, string.Format(@"\+{0}\+", "\\" + itemParameter.ParameterName), "+" + newName + "+", RegexOptions.IgnoreCase);
            itemSql = Regex.Replace(itemSql, string.Format(@"\+{0} ", "\\" + itemParameter.ParameterName), "+" + newName + " ", RegexOptions.IgnoreCase);
            itemSql = Regex.Replace(itemSql, string.Format(@" {0}\+", "\\" + itemParameter.ParameterName), " " + newName + "+", RegexOptions.IgnoreCase);
            itemSql = Regex.Replace(itemSql, string.Format(@"\|\|{0}\|\|", "\\" + itemParameter.ParameterName), "||" + newName + "||", RegexOptions.IgnoreCase);
            itemSql = Regex.Replace(itemSql, string.Format(@"\={0}\+", "\\" + itemParameter.ParameterName), "=" + newName + "+", RegexOptions.IgnoreCase);
            itemSql = Regex.Replace(itemSql, string.Format(@"{0}\|\|", "\\" + itemParameter.ParameterName), newName + "||", RegexOptions.IgnoreCase);
            return itemSql;
        }

        /// <summary>
        /// 获取根基础类型
        /// </summary>
        internal static Type GetRootBaseType(Type entityType)
        {
            var baseType = entityType.BaseType;
            while (baseType != null && baseType.BaseType != UtilConstants.ObjType)
            {
                baseType = baseType.BaseType;
            }
            return baseType;
        }

        /// <summary>
        /// 获取属性基础类型(并返回是否可空)
        /// </summary>
        internal static Type GetUnderType(PropertyInfo propertyInfo, ref bool isNullable)
        {
            Type unType = Nullable.GetUnderlyingType(propertyInfo.PropertyType);
            isNullable = unType != null;
            unType = unType ?? propertyInfo.PropertyType;
            return unType;
        }

        /// <summary>
        /// 获取属性基础类型
        /// </summary>
        internal static Type GetUnderType(PropertyInfo propertyInfo)
        {
            Type unType = Nullable.GetUnderlyingType(propertyInfo.PropertyType);
            unType = unType ?? propertyInfo.PropertyType;
            return unType;
        }

        /// <summary>
        /// 检查属性是否可空
        /// </summary>
        internal static bool IsNullable(PropertyInfo propertyInfo)
        {
            Type unType = Nullable.GetUnderlyingType(propertyInfo.PropertyType);
            return unType != null;
        }

        /// <summary>
        /// 检查类型是否可空
        /// </summary>
        internal static bool IsNullable(Type type)
        {
            Type unType = Nullable.GetUnderlyingType(type);
            return unType != null;
        }

        /// <summary>
        /// 类型转换方法
        /// </summary>
        public static object ChangeType2(object value, Type type)
        {
            if (value == null && type.IsGenericType) return Activator.CreateInstance(type);
            if (value == null) return null;
            if (type == value.GetType()) return value;
            if (type.IsEnum)
            {
                if (value is string)
                    return Enum.Parse(type, value as string);
                else
                    return Enum.ToObject(type, value);
            }
            if (!type.IsInterface && type.IsGenericType)
            {
                Type innerType = type.GetGenericArguments()[0];
                object innerValue = ChangeType(value, innerType);
                return Activator.CreateInstance(type, new object[] { innerValue });
            }
            if (value is string && type == typeof(Guid)) return new Guid(value as string);
            if (value is string && type == typeof(Version)) return new Version(value as string);
            if (!(value is IConvertible)) return value;
            return Convert.ChangeType(value, type);
        }

        /// <summary>
        /// 将对象转换为指定类型
        /// </summary>
        internal static T ChangeType<T>(T obj, Type type)
        {
            return (T)Convert.ChangeType(obj, type);
        }

        /// <summary>
        /// 将对象转换为泛型类型
        /// </summary>
        internal static T ChangeType<T>(T obj)
        {
            return (T)Convert.ChangeType(obj, typeof(T));
        }

        /// <summary>
        /// 将DateTime转换为DateTimeOffset
        /// </summary>
        internal static DateTimeOffset GetDateTimeOffsetByDateTime(DateTime date)
        {
            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            DateTimeOffset utcTime2 = date;
            return utcTime2;
        }

        /// <summary>
        /// 获取包装表SQL
        /// </summary>
        internal static string GetPackTable(string sql, string shortName)
        {
            return string.Format(" ({0}) {1} ", sql, shortName);
        }

        /// <summary>
        /// 根据值类型获取类型转换函数
        /// </summary>
        public static Func<string, object> GetTypeConvert(object value)
        {
            if (value is int || value is uint || value is int? || value is uint?)
            {
                return x => Convert.ToInt32(x);
            }
            else if (value is short || value is ushort || value is short? || value is ushort?)
            {
                return x => Convert.ToInt16(x);
            }
            else if (value is long || value is long? || value is ulong? || value is long?)
            {
                return x => Convert.ToInt64(x);
            }
            else if (value is DateTime || value is DateTime?)
            {
                return x => Convert.ToDateTime(x);
            }
            else if (value is bool || value is bool?)
            {
                return x => Convert.ToBoolean(x);
            }
            return null;
        }

        /// <summary>
        /// 获取值的类型名称
        /// </summary>
        internal static string GetTypeName(object value)
        {
            if (value == null)
            {
                return null;
            }
            else
            {
                return value.GetType().Name;
            }
        }

        /// <summary>
        /// 获取括号内的值
        /// </summary>
        internal static string GetParenthesesValue(string dbTypeName)
        {
            if (Regex.IsMatch(dbTypeName, @"\(.+\)"))
            {
                dbTypeName = Regex.Replace(dbTypeName, @"\(.+\)", "");
            }
            dbTypeName = dbTypeName.Trim();
            return dbTypeName;
        }

        /// <summary>
        /// 获取操作前的旧值
        /// </summary>
        internal static T GetOldValue<T>(T value, Action action)
        {
            action();
            return value;
        }

        /// <summary>
        /// 获取类型的默认值
        /// </summary>
        internal static object DefaultForType(Type targetType)
        {
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }

        /// <summary>
        /// 将字节数组转换为长整型
        /// </summary>
        internal static Int64 GetLong(byte[] bytes)
        {
            return Convert.ToInt64(string.Join("", bytes).PadRight(20, '0'));
        }

        /// <summary>
        /// 获取对象的属性值
        /// </summary>
        public static object GetPropertyValue<T>(T t, string PropertyName)
        {
            return t.GetType().GetProperty(PropertyName).GetValue(t, null);
        }

        private static readonly char[] separator = new char[] { '9' };

        /// <summary>
        /// 将数字字符串转换为实际字符串
        /// </summary>
        public static string ConvertNumbersToString(string value)
        {
            string[] splitInt = value.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            var splitChars = splitInt.Select(s => Convert.ToChar(
                                              Convert.ToInt32(s, 8)
                                            ).ToString());

            return string.Join("", splitChars);
        }

        /// <summary>
        /// 将字符串转换为数字字符串
        /// </summary>
        public static string ConvertStringToNumbers(string value)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in value)
            {
                int cAscil = (int)c;
                sb.Append(Convert.ToString(c, 8) + "9");
            }

            return sb.ToString();
        }

        /// <summary>
        /// 通过表达式调用方法操作数据
        /// </summary>
        public static void DataInoveByExpresson<Type>(Type[] datas, MethodCallExpression callExpresion)
        {
            var methodInfo = callExpresion.Method;
            foreach (var item in datas)
            {
                if (callExpresion.Arguments.Count == 0)
                {
                    methodInfo.Invoke(item, null);
                }
                else
                {
                    List<object> methodParameters = new List<object>();
                    foreach (var callItem in callExpresion.Arguments)
                    {
                        var parameter = callItem.GetType().GetProperties().FirstOrDefault(it => it.Name == "Value");
                        if (parameter == null)
                        {
                            var value = LambdaExpression.Lambda(callItem).Compile().DynamicInvoke();
                            methodParameters.Add(value);
                        }
                        else
                        {
                            var value = parameter.GetValue(callItem, null);
                            methodParameters.Add(value);
                        }
                    }
                    methodInfo.Invoke(item, methodParameters.ToArray());
                }
            }
        }

        /// <summary>
        /// 将枚举转换为字典
        /// </summary>
        public static Dictionary<string, T> EnumToDictionary<T>()
        {
            Dictionary<string, T> dic = new Dictionary<string, T>();
            if (!typeof(T).IsEnum)
            {
                return dic;
            }
            string desc = string.Empty;
            foreach (var item in Enum.GetValues(typeof(T)))
            {
                var key = item.ToString().ToLower();
                if (!dic.ContainsKey(key))
                    dic.Add(key, (T)item);
            }
            return dic;
        }
    }
}