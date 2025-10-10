using System.Data;
using System.Reflection;

namespace ThingsGateway.SqlSugar
{
    internal static class IDataReaderEntityBuilderHelpers
    {

        /// <summary>
        /// IsDBNull方法
        /// </summary>
        internal static readonly MethodInfo isDBNullMethod = typeof(IDataRecord).GetMethod(nameof(IDataRecord.IsDBNull), new Type[] { typeof(int) });
        /// <summary>
        /// GetBoolean方法
        /// </summary>
        internal static readonly MethodInfo getBoolean = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetBoolean), new Type[] { typeof(int) });
        /// <summary>
        /// GetByte方法
        /// </summary>
        internal static readonly MethodInfo getByte = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetByte), new Type[] { typeof(int) });
        /// <summary>
        /// GetDateTime方法
        /// </summary>
        internal static readonly MethodInfo getDateTime = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetDateTime), new Type[] { typeof(int) });
        internal static readonly MethodInfo getDecimal = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetDecimal), new[] { typeof(int) });
        internal static readonly MethodInfo getDouble = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetDouble), new[] { typeof(int) });
        internal static readonly MethodInfo getFloat = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetFloat), new[] { typeof(int) });
        internal static readonly MethodInfo getGuid = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetGuid), new[] { typeof(int) });
        internal static readonly MethodInfo getInt16 = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetInt16), new[] { typeof(int) });
        internal static readonly MethodInfo getInt32 = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetInt32), new[] { typeof(int) });
        internal static readonly MethodInfo getInt64 = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetInt64), new[] { typeof(int) });
        internal static readonly MethodInfo getString = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetString), new[] { typeof(int) });
        internal static readonly MethodInfo getdatetimeoffset = typeof(IDataRecordExtensions).GetMethod(nameof(IDataRecordExtensions.GetDateTimeOffset));
        internal static readonly MethodInfo getdatetimeoffsetDate = typeof(IDataRecordExtensions).GetMethod(nameof(IDataRecordExtensions.GetDateTimeOffsetDate));
        internal static readonly MethodInfo getStringGuid = typeof(IDataRecordExtensions).GetMethod(nameof(IDataRecordExtensions.GetStringGuid));
        internal static readonly MethodInfo getXelement = typeof(IDataRecordExtensions).GetMethod(nameof(IDataRecordExtensions.GetXelement));
        internal static readonly MethodInfo getConvertStringGuid = typeof(IDataRecordExtensions).GetMethod(nameof(IDataRecordExtensions.GetConvertStringGuid));
        internal static readonly MethodInfo getEnum = typeof(IDataRecordExtensions).GetMethod(nameof(IDataRecordExtensions.GetEnum));
        internal static readonly MethodInfo getConvertString = typeof(IDataRecordExtensions).GetMethod(nameof(IDataRecordExtensions.GetConvertString));
        internal static readonly MethodInfo getConvertFloat = typeof(IDataRecordExtensions).GetMethod(nameof(IDataRecordExtensions.GetConvertFloat));
        internal static readonly MethodInfo getConvertBoolean = typeof(IDataRecordExtensions).GetMethod(nameof(IDataRecordExtensions.GetConvertBoolean));
        internal static readonly MethodInfo getConvertByte = typeof(IDataRecordExtensions).GetMethod(nameof(IDataRecordExtensions.GetConvertByte));
        internal static readonly MethodInfo getConvertChar = typeof(IDataRecordExtensions).GetMethod(nameof(IDataRecordExtensions.GetConvertChar));
        internal static readonly MethodInfo getConvertDateTime = typeof(IDataRecordExtensions).GetMethod(nameof(IDataRecordExtensions.GetConvertDateTime));
        internal static readonly MethodInfo getConvertTime = typeof(IDataRecordExtensions).GetMethod(nameof(IDataRecordExtensions.GetConvertTime));
        internal static readonly MethodInfo getTime = typeof(IDataRecordExtensions).GetMethod(nameof(IDataRecordExtensions.GetTime));
        internal static readonly MethodInfo getConvertDecimal = typeof(IDataRecordExtensions).GetMethod(nameof(IDataRecordExtensions.GetConvertDecimal));
        internal static readonly MethodInfo getConvertDouble = typeof(IDataRecordExtensions).GetMethod(nameof(IDataRecordExtensions.GetConvertDouble));
        internal static readonly MethodInfo getConvertDoubleToFloat = typeof(IDataRecordExtensions).GetMethod(nameof(IDataRecordExtensions.GetConvertDoubleToFloat));
        internal static readonly MethodInfo getConvertGuid = typeof(IDataRecordExtensions).GetMethod(nameof(IDataRecordExtensions.GetConvertGuid));
        internal static readonly MethodInfo getConvertInt16 = typeof(IDataRecordExtensions).GetMethod(nameof(IDataRecordExtensions.GetConvertInt16));
        internal static readonly MethodInfo getConvertInt32 = typeof(IDataRecordExtensions).GetMethod(nameof(IDataRecordExtensions.GetConvertInt32));
        internal static readonly MethodInfo getConvertInt64 = typeof(IDataRecordExtensions).GetMethod(nameof(IDataRecordExtensions.GetConvertInt64));
        internal static readonly MethodInfo getConvertEnum_Null = typeof(IDataRecordExtensions).GetMethod(nameof(IDataRecordExtensions.GetConvertEnum_Null));
        internal static readonly MethodInfo getConvertdatetimeoffset = typeof(IDataRecordExtensions).GetMethod(nameof(IDataRecordExtensions.GetConvertdatetimeoffset));
        internal static readonly MethodInfo getConvertdatetimeoffsetDate = typeof(IDataRecordExtensions).GetMethod(nameof(IDataRecordExtensions.GetConvertdatetimeoffsetDate));
        internal static readonly MethodInfo getOtherNull = typeof(IDataRecordExtensions).GetMethod(nameof(IDataRecordExtensions.GetOtherNull));
        internal static readonly MethodInfo getOther = typeof(IDataRecordExtensions).GetMethod(nameof(IDataRecordExtensions.GetOther));
        internal static readonly MethodInfo getJson = typeof(IDataRecordExtensions).GetMethod(nameof(IDataRecordExtensions.GetJson));
        internal static readonly MethodInfo getArray = typeof(IDataRecordExtensions).GetMethod(nameof(IDataRecordExtensions.GetArray));
        internal static readonly MethodInfo getEntity = typeof(IDataRecordExtensions).GetMethod(nameof(IDataRecordExtensions.GetEntity), new[] { typeof(SqlSugarProvider) });
        internal static readonly MethodInfo getMyIntNull = typeof(IDataRecordExtensions).GetMethod(nameof(IDataRecordExtensions.GetMyIntNull));
        internal static readonly MethodInfo getMyInt = typeof(IDataRecordExtensions).GetMethod(nameof(IDataRecordExtensions.GetMyInt));
    }
}