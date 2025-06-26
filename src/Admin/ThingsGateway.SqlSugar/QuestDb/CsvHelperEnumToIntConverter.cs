using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// CSV 枚举到整数的类型转换器
    /// </summary>
    public class CsvHelperEnumToIntConverter : ITypeConverter
    {
        /// <summary>
        /// 将对象转换为字符串
        /// </summary>
        /// <param name="value">要转换的对象</param>
        /// <param name="row">写入行上下文</param>
        /// <param name="memberMapData">成员映射数据</param>
        /// <returns>转换后的字符串</returns>
        public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            if (value == null)
            {
                return "null";
            }
            else if (value is Enum enumValue)
            {
                return (Convert.ToInt32(enumValue)).ToString();
            }
            throw new NotSupportedException();
        }

        /// <summary>
        /// 将字符串转换为对象
        /// </summary>
        /// <param name="text">要转换的字符串</param>
        /// <param name="row">读取行上下文</param>
        /// <param name="memberMapData">成员映射数据</param>
        /// <returns>转换后的对象</returns>
        public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (int.TryParse(text, out int intValue))
            {
                return text;
            }
            throw new NotSupportedException();
        }
    }
}
