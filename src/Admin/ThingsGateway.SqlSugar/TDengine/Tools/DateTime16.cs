using System.Data;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// DateTime16 类型转换器
    /// 实现 ISugarDataConverter 接口用于处理 DateTime 类型的数据转换
    /// </summary>
    public class DateTime16 : ISugarDataConverter
    {
        /// <summary>
        /// 将列值转换为 SugarParameter
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="columnValue">列值</param>
        /// <param name="columnIndex">列索引</param>
        /// <returns>转换后的 SugarParameter</returns>
        public SugarParameter ParameterConverter<T>(object columnValue, int columnIndex)
        {
            var name = "@Common" + columnIndex;
            Type undertype = SqlSugar.UtilMethods.GetUnderType(typeof(T)); // 获取没有 nullable 的枚举类型
            return new SugarParameter(name, columnValue, undertype) { CustomDbType = System.Data.DbType.DateTime2 };
        }

        /// <summary>
        /// 将数据记录中的值转换为指定类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="dr">数据记录</param>
        /// <param name="i">列索引</param>
        /// <returns>转换后的值</returns>
        public T QueryConverter<T>(IDataRecord dr, int i)
        {
            var value = dr.GetValue(i);
            return (T)TaosUtilMethods.ChangeType2(value, typeof(T));
        }
    }
}