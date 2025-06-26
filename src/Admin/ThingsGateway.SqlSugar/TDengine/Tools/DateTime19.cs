using System.Data;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 日期19位精度枚举
    /// </summary>
    internal enum Date19
    {
        /// <summary>
        /// 19位时间精度
        /// </summary>
        time = 19
    }

    /// <summary>
    /// DateTime19 类型转换器
    /// 实现 ISugarDataConverter 接口用于处理高精度 DateTime 类型的数据转换
    /// </summary>
    public class DateTime19 : ISugarDataConverter
    {
        /// <summary>
        /// 将列值转换为 SugarParameter
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="columnValue">列值</param>
        /// <param name="columnIndex">列索引</param>
        /// <returns>转换后的 SugarParameter 对象</returns>
        /// <remarks>
        /// 使用 Date19 作为自定义数据库类型，支持19位时间精度
        /// </remarks>
        public SugarParameter ParameterConverter<T>(object columnValue, int columnIndex)
        {
            var name = "@Common" + columnIndex;
            Type undertype = SqlSugar.UtilMethods.GetUnderType(typeof(T)); // 获取没有 nullable 的枚举类型
            return new SugarParameter(name, columnValue, undertype) { CustomDbType = typeof(Date19) };
        }

        /// <summary>
        /// 将数据记录中的值转换为指定类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="dr">数据记录接口</param>
        /// <param name="i">列索引</param>
        /// <returns>转换后的类型值</returns>
        /// <remarks>
        /// 使用 UtilMethods.ChangeType2 方法进行类型转换
        /// </remarks>
        public T QueryConverter<T>(IDataRecord dr, int i)
        {
            var value = dr.GetValue(i);
            return (T)TaosUtilMethods.ChangeType2(value, typeof(T));
        }
    }
}