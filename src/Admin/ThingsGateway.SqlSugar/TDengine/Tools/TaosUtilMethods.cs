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
        /// 获取基础类型(如果是Nullable类型则返回其基础类型)
        /// </summary>
        internal static Type GetUnderType(Type oldType)
        {
            Type type = Nullable.GetUnderlyingType(oldType);
            return type == null ? oldType : type;
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


    }
}