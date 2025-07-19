using System.Data;
using System.Xml.Linq;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// IDataRecord扩展方法
    /// </summary>
    public static partial class IDataRecordExtensions
    {
        #region Common Extensions
        public static Func<object, Type, object> DeserializeObjectFunc { get; internal set; }

        public static T GetDeserializeObject<T>(this IDataReader dr, int i)
        {
            var obj = dr.GetValue(i);
            if (obj == null)
                return default(T);
            var value = obj;
            return (T)DeserializeObjectFunc(value, typeof(T));
        }

        /// <summary>
        /// 获取XElement对象
        /// </summary>
        /// <param name="dr">数据记录器</param>
        /// <param name="i">字段索引</param>
        /// <returns>XElement对象</returns>
        public static XElement GetXelement(this IDataRecord dr, int i)
        {
            var result = XElement.Parse(dr.GetString(i).ToString());
            return result;
        }

        /// <summary>
        /// 获取Guid对象(从字符串转换)
        /// </summary>
        /// <param name="dr">数据记录器</param>
        /// <param name="i">字段索引</param>
        /// <returns>Guid对象</returns>
        public static Guid GetStringGuid(this IDataRecord dr, int i)
        {
            var result = Guid.Parse(dr.GetValue(i).ToString());
            return result;
        }

        /// <summary>
        /// 获取可空Guid对象(从字符串转换)
        /// </summary>
        /// <param name="dr">数据记录器</param>
        /// <param name="i">字段索引</param>
        /// <returns>可空Guid对象</returns>
        public static Guid? GetConvertStringGuid(this IDataRecord dr, int i)
        {
            if (dr.IsDBNull(i))
            {
                return Guid.Empty;
            }
            var result = Guid.Parse(dr.GetValue(i).ToString());
            return result;
        }

        /// <summary>
        /// 获取可空布尔值
        /// </summary>
        /// <param name="dr">数据记录器</param>
        /// <param name="i">字段索引</param>
        /// <returns>可空布尔值</returns>
        public static bool? GetConvertBoolean(this IDataRecord dr, int i)
        {
            var result = dr.GetBoolean(i);
            return result;
        }

        /// <summary>
        /// 获取可空字节
        /// </summary>
        /// <param name="dr">数据记录器</param>
        /// <param name="i">字段索引</param>
        /// <returns>可空字节</returns>
        public static byte? GetConvertByte(this IDataRecord dr, int i)
        {
            if (dr.IsDBNull(i))
            {
                return null;
            }
            var result = dr.GetByte(i);
            return result;
        }

        /// <summary>
        /// 获取可空字符
        /// </summary>
        /// <param name="dr">数据记录器</param>
        /// <param name="i">字段索引</param>
        /// <returns>可空字符</returns>
        public static char? GetConvertChar(this IDataRecord dr, int i)
        {
            if (dr.IsDBNull(i))
            {
                return null;
            }
            var result = dr.GetChar(i);
            return result;
        }

        /// <summary>
        /// 获取可空日期时间
        /// </summary>
        /// <param name="dr">数据记录器</param>
        /// <param name="i">字段索引</param>
        /// <returns>可空日期时间</returns>
        public static DateTime? GetConvertDateTime(this IDataRecord dr, int i)
        {
            var result = dr.GetDateTime(i);
            if (result == DateTime.MinValue)
            {
                return null; ;
            }
            return result;
        }

        /// <summary>
        /// 获取可空时间
        /// </summary>
        /// <param name="dr">数据记录器</param>
        /// <param name="i">字段索引</param>
        /// <returns>可空时间</returns>
        public static DateTime? GetConvertTime(this IDataRecord dr, int i)
        {
            var result = dr.GetValue(i);
            if (result == DBNull.Value)
            {
                return null; ;
            }
            return Convert.ToDateTime(result.ToString());
        }

        /// <summary>
        /// 获取时间
        /// </summary>
        /// <param name="dr">数据记录器</param>
        /// <param name="i">字段索引</param>
        /// <returns>时间</returns>
        public static DateTime GetTime(this IDataRecord dr, int i)
        {
            return Convert.ToDateTime(dr.GetValue(i).ToString());
        }

        /// <summary>
        /// 获取可空十进制数
        /// </summary>
        /// <param name="dr">数据记录器</param>
        /// <param name="i">字段索引</param>
        /// <returns>可空十进制数</returns>
        public static decimal? GetConvertDecimal(this IDataRecord dr, int i)
        {
            if (dr.IsDBNull(i))
            {
                return null;
            }
            var result = dr.GetDecimal(i);
            return result;
        }

        /// <summary>
        /// 获取可空双精度浮点数
        /// </summary>
        /// <param name="dr">数据记录器</param>
        /// <param name="i">字段索引</param>
        /// <returns>可空双精度浮点数</returns>
        public static double? GetConvertDouble(this IDataRecord dr, int i)
        {
            if (dr.IsDBNull(i))
            {
                return null;
            }
            var result = dr.GetDouble(i);
            return result;
        }

        /// <summary>
        /// 获取可空单精度浮点数(从双精度转换)
        /// </summary>
        /// <param name="dr">数据记录器</param>
        /// <param name="i">字段索引</param>
        /// <returns>可空单精度浮点数</returns>
        public static float? GetConvertDoubleToFloat(this IDataRecord dr, int i)
        {
            if (dr.IsDBNull(i))
            {
                return null;
            }
            var result = dr.GetDouble(i);
            return Convert.ToSingle(result);
        }

        /// <summary>
        /// 获取可空Guid
        /// </summary>
        /// <param name="dr">数据记录器</param>
        /// <param name="i">字段索引</param>
        /// <returns>可空Guid</returns>
        public static Guid? GetConvertGuid(this IDataRecord dr, int i)
        {
            if (dr.IsDBNull(i))
            {
                return null;
            }
            var result = dr.GetGuid(i);
            return result;
        }

        /// <summary>
        /// 获取可空短整型
        /// </summary>
        /// <param name="dr">数据记录器</param>
        /// <param name="i">字段索引</param>
        /// <returns>可空短整型</returns>
        public static short? GetConvertInt16(this IDataRecord dr, int i)
        {
            if (dr.IsDBNull(i))
            {
                return null;
            }
            var result = dr.GetInt16(i);
            return result;
        }

        /// <summary>
        /// 获取可空整型(处理Oracle NUMBER类型)
        /// </summary>
        /// <param name="dr">数据记录器</param>
        /// <param name="i">字段索引</param>
        /// <returns>可空整型</returns>
        public static Int32? GetMyIntNull(this IDataRecord dr, int i)
        {
            if (dr.IsDBNull(i))
            {
                return null;
            }
            if (dr.GetDataTypeName(i) == "NUMBER")
            {
                return Convert.ToInt32(dr.GetDouble(i));
            }
            var result = dr.GetInt32(i);
            return result;
        }

        /// <summary>
        /// 获取整型(处理Oracle NUMBER类型)
        /// </summary>
        /// <param name="dr">数据记录器</param>
        /// <param name="i">字段索引</param>
        /// <returns>整型</returns>
        public static Int32 GetMyInt(this IDataRecord dr, int i)
        {
            if (dr.GetDataTypeName(i) == "NUMBER")
            {
                return Convert.ToInt32(dr.GetDouble(i));
            }
            var result = dr.GetInt32(i);
            return result;
        }

        /// <summary>
        /// 获取可空整型
        /// </summary>
        /// <param name="dr">数据记录器</param>
        /// <param name="i">字段索引</param>
        /// <returns>可空整型</returns>
        public static Int32? GetConvertInt32(this IDataRecord dr, int i)
        {
            if (dr.IsDBNull(i))
            {
                return null;
            }
            var result = dr.GetInt32(i);
            return result;
        }

        /// <summary>
        /// 获取可空长整型
        /// </summary>
        /// <param name="dr">数据记录器</param>
        /// <param name="i">字段索引</param>
        /// <returns>可空长整型</returns>
        public static long? GetConvetInt64(this IDataRecord dr, int i)
        {
            if (dr.IsDBNull(i))
            {
                return null;
            }
            var result = dr.GetInt64(i);
            return result;
        }

        /// <summary>
        /// 获取可空单精度浮点数
        /// </summary>
        /// <param name="dr">数据记录器</param>
        /// <param name="i">字段索引</param>
        /// <returns>可空单精度浮点数</returns>
        public static float? GetConvertFloat(this IDataRecord dr, int i)
        {
            if (dr.IsDBNull(i))
            {
                return null;
            }
            var result = dr.GetFloat(i);
            return result;
        }

        /// <summary>
        /// 获取日期时间(从DateTimeOffset转换)
        /// </summary>
        /// <param name="dr">数据记录器</param>
        /// <param name="i">字段索引</param>
        /// <returns>日期时间</returns>
        public static DateTime GetdatetimeoffsetDate(this IDataRecord dr, int i)
        {
            if (dr.IsDBNull(i))
            {
                return DateTime.MinValue;
            }
            var offsetValue = (DateTimeOffset)dr.GetValue(i);
            var result = offsetValue.DateTime;
            return result;
        }

        /// <summary>
        /// 获取可空日期时间(从DateTimeOffset转换)
        /// </summary>
        /// <param name="dr">数据记录器</param>
        /// <param name="i">字段索引</param>
        /// <returns>可空日期时间</returns>
        public static DateTime? GetConvertdatetimeoffsetDate(this IDataRecord dr, int i)
        {
            if (dr.IsDBNull(i))
            {
                return DateTime.MinValue;
            }
            var offsetValue = (DateTimeOffset)dr.GetValue(i);
            var result = offsetValue.DateTime;
            return result;
        }

        /// <summary>
        /// 获取DateTimeOffset
        /// </summary>
        /// <param name="dr">数据记录器</param>
        /// <param name="i">字段索引</param>
        /// <returns>DateTimeOffset</returns>
        public static DateTimeOffset Getdatetimeoffset(this IDataRecord dr, int i)
        {
            if (dr.IsDBNull(i))
            {
                return default(DateTimeOffset);
            }
            var date = dr.GetValue(i);
            if (date is DateTime)
            {
                return new DateTimeOffset((DateTime)(date));
            }
            else
            {
                var result = (DateTimeOffset)date;
                return result;
            }
        }

        /// <summary>
        /// 获取可空DateTimeOffset
        /// </summary>
        /// <param name="dr">数据记录器</param>
        /// <param name="i">字段索引</param>
        /// <returns>可空DateTimeOffset</returns>
        public static DateTimeOffset? GetConvertdatetimeoffset(this IDataRecord dr, int i)
        {
            if (dr.IsDBNull(i))
            {
                return default(DateTimeOffset);
            }
            var date = dr.GetValue(i);
            if (date is DateTime)
            {
                return new DateTimeOffset((DateTime)(date));
            }
            else
            {
                var result = (DateTimeOffset)date;
                return result;
            }
        }

        /// <summary>
        /// 获取可空字符串
        /// </summary>
        /// <param name="dr">数据记录器</param>
        /// <param name="i">字段索引</param>
        /// <returns>可空字符串</returns>
        public static string GetConvertString(this IDataRecord dr, int i)
        {
            if (dr.IsDBNull(i))
            {
                return null;
            }
            var result = Convert.ToString(dr.GetValue(i));
            return result;
        }

        /// <summary>
        /// 获取可空值类型
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="dr">数据读取器</param>
        /// <param name="i">字段索引</param>
        /// <returns>可空值类型</returns>
        public static Nullable<T> GetOtherNull<T>(this IDataReader dr, int i) where T : struct
        {
            if (dr.IsDBNull(i))
            {
                return null;
            }
            return GetOther<T>(dr, i);
        }

        /// <summary>
        /// 获取泛型值
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="dr">数据读取器</param>
        /// <param name="i">字段索引</param>
        /// <returns>泛型值</returns>
        public static T GetOther<T>(this IDataReader dr, int i)
        {
            try
            {
                if (dr.IsDBNull(i))
                {
                    return default(T);
                }
                var result = dr.GetValue(i);
                return UtilMethods.To<T>(result);
            }
            catch (Exception ex)
            {
                return OtherException<T>(dr, i, ex);
            }
        }

        /// <summary>
        /// 获取JSON反序列化对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="dr">数据读取器</param>
        /// <param name="i">字段索引</param>
        /// <returns>反序列化对象</returns>
        public static T GetJson<T>(this IDataReader dr, int i)
        {
            var obj = dr.GetValue(i);
            if (obj == null)
                return default(T);
            if (obj is byte[] bytes)
            {
                obj = dr.GetString(i);
            }
            var value = obj.ObjToString();
            return new SerializeService().DeserializeObject<T>(value);
        }

        /// <summary>
        /// 获取数组
        /// </summary>
        /// <typeparam name="T">数组类型</typeparam>
        /// <param name="dr">数据读取器</param>
        /// <param name="i">字段索引</param>
        /// <returns>数组</returns>
        public static T GetArray<T>(this IDataReader dr, int i)
        {
            //pgsql
            var obj = dr.GetValue(i);
            if (obj == null)
                return default(T);
            return (T)obj;
        }

        /// <summary>
        /// 获取可空枚举
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="dr">数据读取器</param>
        /// <param name="i">字段索引</param>
        /// <returns>可空枚举</returns>
        public static Nullable<T> GetConvertEnum_Null<T>(this IDataReader dr, int i) where T : struct
        {
            if (dr.IsDBNull(i))
            {
                return null;
            }
            object value = dr.GetValue(i);
            if (value != null)
            {
                var valueType = value.GetType();
                if (valueType.IsIn(UtilConstants.FloatType, UtilConstants.DecType, UtilConstants.DobType))
                {
                    if (Convert.ToDecimal(value) < 0)
                    {
                        value = Convert.ToInt32(value);
                    }
                    else
                    {
                        value = Convert.ToUInt32(value);
                    }
                }
                else if (valueType == UtilConstants.StringType)
                {
                    return (T)Enum.Parse(typeof(T), value.ObjToString());
                }
            }
            T t = (T)Enum.ToObject(typeof(T), value);
            return t;
        }

        /// <summary>
        /// 获取枚举
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="dr">数据读取器</param>
        /// <param name="i">字段索引</param>
        /// <returns>枚举</returns>
        public static T GetEnum<T>(this IDataReader dr, int i) where T : struct
        {
            object value = dr.GetValue(i);
            if (value != null)
            {
                var valueType = value.GetType();
                if (valueType.IsIn(UtilConstants.FloatType, UtilConstants.DecType, UtilConstants.DobType))
                {
                    if (Convert.ToDecimal(value) < 0)
                    {
                        value = Convert.ToInt32(value);
                    }
                    else
                    {
                        value = Convert.ToUInt32(value);
                    }
                }
                else if (valueType == UtilConstants.StringType)
                {
                    return (T)Enum.Parse(typeof(T), value.ObjToString());
                }
            }
            T t = (T)Enum.ToObject(typeof(T), value);
            return t;
        }

        /// <summary>
        /// 获取实体对象(未实现)
        /// </summary>
        /// <param name="dr">数据读取器</param>
        /// <param name="context">SqlSugar提供者</param>
        /// <returns>null</returns>
        public static object GetEntity(this IDataReader dr, SqlSugarProvider context)
        {
            return null;
        }

        /// <summary>
        /// 处理其他异常情况
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="dr">数据记录器</param>
        /// <param name="i">字段索引</param>
        /// <param name="ex">异常</param>
        /// <returns>转换后的值</returns>
        private static T OtherException<T>(IDataRecord dr, int i, Exception ex)
        {
            if (dr.GetFieldType(i) == UtilConstants.DateType)
            {
                return UtilMethods.To<T>(dr.GetConvertDouble(i));
            }
            if (dr.GetFieldType(i) == UtilConstants.GuidType)
            {
                var data = dr.GetString(i);
                if (string.IsNullOrEmpty(data))
                {
                    return UtilMethods.To<T>(default(T));
                }
                else
                {
                    return UtilMethods.To<T>(Guid.Parse(data));
                }
            }
            throw new Exception(ex.Message);
        }
        #endregion
    }
}