//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Newtonsoft.Json.Linq;

using System.Text;

using ThingsGateway.Foundation.Extension.String;

namespace ThingsGateway.Foundation;

/// <summary>
/// ThingsGatewayBitConverterExtensions
/// </summary>
public static class ThingsGatewayBitConverterExtension
{

    //private static MemoryCache MemoryCache = new() { Capacity = 10000000 };
    /// <summary>
    /// 从设备地址中解析附加信息
    /// 这个方法获取<see cref="IThingsGatewayBitConverter"/>
    /// 解析步骤将被缓存。
    /// </summary>
    /// <param name="registerAddress">设备地址</param>
    /// <param name="defaultBitConverter">默认的数据转换器</param>
    /// <returns><see cref="IThingsGatewayBitConverter"/> 实例</returns>
    public static IThingsGatewayBitConverter GetTransByAddress(this IThingsGatewayBitConverter defaultBitConverter, string? registerAddress)
    {
        if (registerAddress.IsNullOrEmpty()) return defaultBitConverter;

        var type = defaultBitConverter.GetType();
        // 尝试从缓存中获取解析结果
        //var cacheKey = $"{nameof(ThingsGatewayBitConverterExtension)}_{nameof(GetTransByAddress)}_{type.FullName}_{type.TypeHandle.Value}_{defaultBitConverter.ToJsonString()}_{registerAddress}_{defaultBitConverter.GetHashCode()}";
        //if (MemoryCache.TryGetValue(cacheKey, out IThingsGatewayBitConverter cachedConverter))
        //{
        //    if (cachedConverter.Equals(defaultBitConverter))
        //    {
        //        return defaultBitConverter;
        //    }
        //    else
        //    {
        //        return (IThingsGatewayBitConverter)cachedConverter.Map(type);
        //    }
        //}

        // 去除设备地址两端的空格
        registerAddress = registerAddress.Trim();

        // 根据分号拆分附加信息
        var strs = registerAddress.SplitStringBySemicolon();

        DataFormatEnum? dataFormat = null;
        Encoding? encoding = null;
        bool? wstring = null;
        int? stringlength = null;
        BcdFormatEnum? bcdFormat = null;
        StringBuilder sb = new();
        foreach (var str in strs)
        {
            // 解析 dataFormat
            if (str.StartsWith("data=", StringComparison.OrdinalIgnoreCase))
            {
                var dataFormatName = str.Substring(5);
                try { if (Enum.TryParse<DataFormatEnum>(dataFormatName, true, out var dataFormat1)) dataFormat = dataFormat1; } catch { }
            }
            else if (str.StartsWith("vsl=", StringComparison.OrdinalIgnoreCase))
            {
                var wstringName = str.Substring(4);
                try { if (bool.TryParse(wstringName, out var wstring1)) wstring = wstring1; } catch { }
            }
            // 解析 encoding
            else if (str.StartsWith("encoding=", StringComparison.OrdinalIgnoreCase))
            {
                var encodingName = str.Substring(9);
                try { encoding = Encoding.GetEncoding(encodingName); } catch { }
            }
            // 解析 length
            else if (str.StartsWith("len=", StringComparison.OrdinalIgnoreCase))
            {
                var lenStr = str.Substring(4);
                stringlength = lenStr.IsNullOrEmpty() ? null : Convert.ToUInt16(lenStr);
            }
            // 解析 bcdFormat
            else if (str.StartsWith("bcd=", StringComparison.OrdinalIgnoreCase))
            {
                var bcdName = str.Substring(4);
                try { if (Enum.TryParse<BcdFormatEnum>(bcdName, true, out var bcdFormat1)) bcdFormat = bcdFormat1; } catch { }
            }

            // 处理其他情况，将未识别的部分拼接回去
            else
            {
                if (sb.Length > 0)
                    sb.Append($";{str}");
                else
                    sb.Append($"{str}");
            }
        }

        // 更新设备地址为去除附加信息后的地址
        registerAddress = sb.ToString();

        // 如果没有解析出任何附加信息，则直接返回默认的数据转换器
        if (bcdFormat == null && stringlength == null && encoding == null && dataFormat == null && wstring == null)
        {
            //MemoryCache.Set(cacheKey, defaultBitConverter!, 3600);
            return defaultBitConverter;
        }

        // 根据默认的数据转换器创建新的数据转换器实例
        var converter = (IThingsGatewayBitConverter)defaultBitConverter!.Map(type);

        // 更新新的数据转换器实例的属性值
        if (encoding != null)
        {
            converter.Encoding = encoding;
        }
        if (bcdFormat != null)
        {
            converter.BcdFormat = bcdFormat.Value;
        }
        if (wstring != null)
        {
            converter.IsVariableStringLength = wstring.Value;
        }
        if (stringlength != null)
        {
            converter.StringLength = stringlength.Value;
        }
        if (dataFormat != null)
        {
            converter.DataFormat = dataFormat.Value;
        }

        // 将解析结果添加到缓存中，缓存有效期为3600秒
        //MemoryCache.Set(cacheKey, converter!, 3600);
        return converter;
    }

    #region 获取对应数据类型的数据

    /// <summary>
    /// 根据数据类型获取字节数组
    /// </summary>
    public static byte[] GetBytesFormData(this IThingsGatewayBitConverter byteConverter, JToken value, DataTypeEnum dataType, int arrayLength = 1)
    {
        if (arrayLength > 1)
        {
            switch (dataType)
            {
                case DataTypeEnum.Boolean:
                    return byteConverter.GetBytes(value.ToObject<Boolean[]>());

                case DataTypeEnum.Byte:
                    return value.ToObject<Byte[]>();

                case DataTypeEnum.Int16:
                    return byteConverter.GetBytes(value.ToObject<Int16[]>());

                case DataTypeEnum.UInt16:
                    return byteConverter.GetBytes(value.ToObject<UInt16[]>());

                case DataTypeEnum.Int32:
                    return byteConverter.GetBytes(value.ToObject<Int32[]>());

                case DataTypeEnum.UInt32:
                    return byteConverter.GetBytes(value.ToObject<UInt32[]>());

                case DataTypeEnum.Int64:
                    return byteConverter.GetBytes(value.ToObject<Int64[]>());

                case DataTypeEnum.UInt64:
                    return byteConverter.GetBytes(value.ToObject<UInt64[]>());

                case DataTypeEnum.Single:
                    return byteConverter.GetBytes(value.ToObject<Single[]>());

                case DataTypeEnum.Double:
                    return byteConverter.GetBytes(value.ToObject<Double[]>());

                case DataTypeEnum.String:
                default:
                    List<byte> bytes = new();
                    String[] strings = value.ToObject<String[]>();
                    for (int i = 0; i < arrayLength; i++)
                    {
                        var data = byteConverter.GetBytes(strings[i]);
                        bytes.AddRange(data);
                    }
                    return bytes.ToArray();
            }
        }
        else
        {
            switch (dataType)
            {
                case DataTypeEnum.Boolean:
                    return byteConverter.GetBytes(value.ToObject<Boolean>());

                case DataTypeEnum.Byte:
                    return byteConverter.GetBytes(value.ToObject<Byte>());

                case DataTypeEnum.Int16:
                    return byteConverter.GetBytes(value.ToObject<Int16>());

                case DataTypeEnum.UInt16:
                    return byteConverter.GetBytes(value.ToObject<UInt16>());

                case DataTypeEnum.Int32:
                    return byteConverter.GetBytes(value.ToObject<Int32>());

                case DataTypeEnum.UInt32:
                    return byteConverter.GetBytes(value.ToObject<UInt32>());

                case DataTypeEnum.Int64:
                    return byteConverter.GetBytes(value.ToObject<Int64>());

                case DataTypeEnum.UInt64:
                    return byteConverter.GetBytes(value.ToObject<UInt64>());

                case DataTypeEnum.Single:
                    return byteConverter.GetBytes(value.ToObject<Single>());

                case DataTypeEnum.Double:
                    return byteConverter.GetBytes(value.ToObject<Double>());

                case DataTypeEnum.String:
                default:
                    return byteConverter.GetBytes(value.ToObject<String>());
            }
        }
    }

    /// <summary>
    /// 根据数据类型获取实际值
    /// </summary>
    public static bool GetChangedDataFormBytes(
        this IThingsGatewayBitConverter byteConverter,
        IDevice device,
        string address,
        byte[] buffer,
        int index,
        DataTypeEnum dataType,
        int arrayLength,
        object? oldValue,
        out object? result)
    {
        switch (dataType)
        {
            case DataTypeEnum.Boolean:
                if (arrayLength > 1)
                {
                    var newVal = byteConverter.ToBoolean(buffer, index, arrayLength, device.BitReverse(address));
                    if (oldValue is bool[] oldArr && newVal.SequenceEqual(oldArr))
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }
                else
                {
                    var newVal = byteConverter.ToBoolean(buffer, index, device.BitReverse(address));
                    if (oldValue is bool oldVal && oldVal == newVal)
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }

            case DataTypeEnum.Byte:
                if (arrayLength > 1)
                {
                    var newVal = byteConverter.ToByte(buffer, index, arrayLength);
                    if (oldValue is byte[] oldArr && newVal.SequenceEqual(oldArr))
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }
                else
                {
                    var newVal = byteConverter.ToByte(buffer, index);
                    if (oldValue is byte oldVal && oldVal == newVal)
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }

            case DataTypeEnum.Int16:
                if (arrayLength > 1)
                {
                    var newVal = byteConverter.ToInt16(buffer, index, arrayLength);
                    if (oldValue is short[] oldArr && newVal.SequenceEqual(oldArr))
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }
                else
                {
                    var newVal = byteConverter.ToInt16(buffer, index);
                    if (oldValue is short oldVal && oldVal == newVal)
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }

            case DataTypeEnum.UInt16:
                if (arrayLength > 1)
                {
                    var newVal = byteConverter.ToUInt16(buffer, index, arrayLength);
                    if (oldValue is ushort[] oldArr && newVal.SequenceEqual(oldArr))
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }
                else
                {
                    var newVal = byteConverter.ToUInt16(buffer, index);
                    if (oldValue is ushort oldVal && oldVal == newVal)
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }

            case DataTypeEnum.Int32:
                if (arrayLength > 1)
                {
                    var newVal = byteConverter.ToInt32(buffer, index, arrayLength);
                    if (oldValue is int[] oldArr && newVal.SequenceEqual(oldArr))
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }
                else
                {
                    var newVal = byteConverter.ToInt32(buffer, index);
                    if (oldValue is int oldVal && oldVal == newVal)
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }

            case DataTypeEnum.UInt32:
                if (arrayLength > 1)
                {
                    var newVal = byteConverter.ToUInt32(buffer, index, arrayLength);
                    if (oldValue is uint[] oldArr && newVal.SequenceEqual(oldArr))
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }
                else
                {
                    var newVal = byteConverter.ToUInt32(buffer, index);
                    if (oldValue is uint oldVal && oldVal == newVal)
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }

            case DataTypeEnum.Int64:
                if (arrayLength > 1)
                {
                    var newVal = byteConverter.ToInt64(buffer, index, arrayLength);
                    if (oldValue is long[] oldArr && newVal.SequenceEqual(oldArr))
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }
                else
                {
                    var newVal = byteConverter.ToInt64(buffer, index);
                    if (oldValue is long oldVal && oldVal == newVal)
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }

            case DataTypeEnum.UInt64:
                if (arrayLength > 1)
                {
                    var newVal = byteConverter.ToUInt64(buffer, index, arrayLength);
                    if (oldValue is ulong[] oldArr && newVal.SequenceEqual(oldArr))
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }
                else
                {
                    var newVal = byteConverter.ToUInt64(buffer, index);
                    if (oldValue is ulong oldVal && oldVal == newVal)
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }

            case DataTypeEnum.Single:
                if (arrayLength > 1)
                {
                    var newVal = byteConverter.ToSingle(buffer, index, arrayLength);
                    if (oldValue is float[] oldArr && newVal.SequenceEqual(oldArr))
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }
                else
                {
                    var newVal = byteConverter.ToSingle(buffer, index);
                    if (oldValue is float oldVal && oldVal == newVal)
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }

            case DataTypeEnum.Double:
                if (arrayLength > 1)
                {
                    var newVal = byteConverter.ToDouble(buffer, index, arrayLength);
                    if (oldValue is double[] oldArr && newVal.SequenceEqual(oldArr))
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }
                else
                {
                    var newVal = byteConverter.ToDouble(buffer, index);
                    if (oldValue is double oldVal && oldVal == newVal)
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }

            case DataTypeEnum.String:
            default:
                if (arrayLength > 1)
                {
                    var newArr = new string[arrayLength];
                    for (int i = 0; i < arrayLength; i++)
                    {
                        newArr[i] = byteConverter.ToString(buffer, index + i * (byteConverter.StringLength ?? 1), byteConverter.StringLength ?? 1);
                    }

                    if (oldValue is string[] oldArr && newArr.SequenceEqual(oldArr))
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newArr;
                    return true;
                }
                else
                {
                    var str = byteConverter.ToString(buffer, index, byteConverter.StringLength ?? 1);
                    if (oldValue is string oldStr && oldStr == str)
                    {
                        result = oldStr;
                        return false;
                    }
                    result = str;
                    return true;
                }
        }
    }



    #endregion 获取对应数据类型的数据
}
