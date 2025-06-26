namespace ThingsGateway.SqlSugar.TDengine
{
    /// <summary>
    /// 提供给外部用户的通用扩展方法
    /// </summary>
    public static class UtilExtensions
    {
        /// <summary>
        /// 设置TDengine子表名称
        /// </summary>
        public static TagInserttable<T> SetTDengineChildTableName<T>(this IInsertable<T> thisValue, Func<string, T, string> getChildTableNamefunc) where T : class, new()
        {
            TagInserttable<T> result = new TagInserttable<T>();
            result.thisValue = thisValue;
            result.Context = ((InsertableProvider<T>)thisValue).Context;
            result.getChildTableNamefunc = getChildTableNamefunc;
            return result;
        }

        /// <summary>
        /// 将对象转换为字符串(不去除空格)
        /// </summary>
        public static string ObjToStringNoTrim(this object thisValue)
        {
            if (thisValue != null) return thisValue.ToString();
            return string.Empty;
        }

        /// <summary>
        /// 将字符串转换为小写(根据isLower参数决定)
        /// </summary>
        public static string ToLower(this string value, bool isLower)
        {
            if (isLower)
            {
                return value.ObjToString().ToLower();
            }
            return value.ObjToString();
        }

        /// <summary>
        /// 将对象转换为整型(转换失败返回0)
        /// </summary>
        public static int ObjToInt(this object thisValue)
        {
            int reval = 0;
            if (thisValue == null) return 0;
            if (thisValue is Enum)
            {
                return (int)thisValue;
            }
            if (thisValue != null && thisValue != DBNull.Value && int.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return reval;
        }

        /// <summary>
        /// 将对象转换为整型(转换失败返回指定错误值)
        /// </summary>
        public static int ObjToInt(this object thisValue, int errorValue)
        {
            int reval = 0;
            if (thisValue is Enum)
            {
                return (int)thisValue;
            }
            if (thisValue != null && thisValue != DBNull.Value && int.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return errorValue;
        }

        /// <summary>
        /// 将对象转换为金额(double类型,转换失败返回0)
        /// </summary>
        public static double ObjToMoney(this object thisValue)
        {
            double reval = 0;
            if (thisValue != null && thisValue != DBNull.Value && double.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return 0;
        }

        /// <summary>
        /// 将对象转换为金额(double类型,转换失败返回指定错误值)
        /// </summary>
        public static double ObjToMoney(this object thisValue, double errorValue)
        {
            double reval = 0;
            if (thisValue != null && thisValue != DBNull.Value && double.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return errorValue;
        }

        /// <summary>
        /// 将对象转换为字符串(去除空格,转换失败返回空字符串)
        /// </summary>
        public static string ObjToString(this object thisValue)
        {
            if (thisValue != null) return thisValue.ToString().Trim();
            return string.Empty;
        }

        /// <summary>
        /// 将对象转换为字符串(去除空格,转换失败返回指定错误值)
        /// </summary>
        public static string ObjToString(this object thisValue, string errorValue)
        {
            if (thisValue != null) return thisValue.ToString().Trim();
            return errorValue;
        }

        /// <summary>
        /// 将对象转换为Decimal(转换失败返回0)
        /// </summary>
        public static Decimal ObjToDecimal(this object thisValue)
        {
            Decimal reval = 0;
            if (thisValue != null && thisValue != DBNull.Value && decimal.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return 0;
        }

        /// <summary>
        /// 将对象转换为Decimal(转换失败返回指定错误值)
        /// </summary>
        public static Decimal ObjToDecimal(this object thisValue, decimal errorValue)
        {
            Decimal reval = 0;
            if (thisValue != null && thisValue != DBNull.Value && decimal.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return errorValue;
        }

        /// <summary>
        /// 将对象转换为DateTime(转换失败返回DateTime.MinValue)
        /// </summary>
        public static DateTime ObjToDate(this object thisValue)
        {
            DateTime reval = DateTime.MinValue;
            if (thisValue != null && thisValue != DBNull.Value && DateTime.TryParse(thisValue.ToString(), out reval))
            {
                reval = Convert.ToDateTime(thisValue);
            }
            return reval;
        }

        /// <summary>
        /// 将对象转换为DateTime(转换失败返回指定错误值)
        /// </summary>
        public static DateTime ObjToDate(this object thisValue, DateTime errorValue)
        {
            DateTime reval = DateTime.MinValue;
            if (thisValue != null && thisValue != DBNull.Value && DateTime.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return errorValue;
        }

        /// <summary>
        /// 将对象转换为bool(转换失败返回false)
        /// </summary>
        public static bool ObjToBool(this object thisValue)
        {
            bool reval = false;
            if (thisValue != null && thisValue != DBNull.Value && bool.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }
            return reval;
        }
    }
}