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

using ThingsGateway.Foundation.Extension.Generic;

namespace ThingsGateway.Foundation;

/// <summary>
/// ThingsGatewayBitConverterExtensions
/// </summary>
public static class ThingsGatewayBitConverterExtension
{
    //private static MemoryCache MemoryCache = new() { Capacity = 10000000 };

    #region 获取对应数据类型的数据

    /// <summary>
    /// 根据数据类型获取字节数组
    /// </summary>
    public static ReadOnlyMemory<byte> GetBytesFormData(this IThingsGatewayBitConverter byteConverter, JToken value, DataTypeEnum dataType, bool array)
    {
        if (array)
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

                case DataTypeEnum.Decimal:
                    return byteConverter.GetBytes(value.ToObject<Decimal[]>());

                case DataTypeEnum.String:
                    List<ReadOnlyMemory<byte>> bytes = new();

                    String[] strings = value.ToObject<String[]>();
                    for (int i = 0; i < strings.Length; i++)
                    {
                        var data = byteConverter.GetBytes(strings[i]);
                        bytes.Add(data.ArrayExpandToLength(byteConverter.StringLength ?? data.Length));
                    }
                    return bytes.CombineMemoryBlocks();
                default:
                    throw new(string.Format(ThingsGateway.Foundation.AppResource.DataTypeNotSupported, dataType));
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

                case DataTypeEnum.Decimal:
                    return byteConverter.GetBytes(value.ToObject<Decimal>());

                case DataTypeEnum.String:
                    return byteConverter.GetBytes(value.ToObject<String>());

                default:
                    throw new(string.Format(ThingsGateway.Foundation.AppResource.DataTypeNotSupported, dataType));
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
        ReadOnlySpan<byte> buffer,
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
            case DataTypeEnum.Decimal:
                if (arrayLength > 1)
                {
                    var newVal = byteConverter.ToDecimal(buffer, index, arrayLength);
                    if (oldValue is decimal[] oldArr && newVal.SequenceEqual(oldArr))
                    {
                        result = oldValue;
                        return false;
                    }
                    result = newVal;
                    return true;
                }
                else
                {
                    var newVal = byteConverter.ToDecimal(buffer, index);
                    if (oldValue is decimal oldVal && oldVal == newVal)
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
