//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Newtonsoft.Json;

using System.Runtime.CompilerServices;
using System.Text;

using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Foundation.Extension.String;

using TouchSocket.Resources;

namespace ThingsGateway.Foundation;

/// <summary>
/// 将基数据类型转换为指定端的一个字节数组，
/// 或将一个字节数组转换为指定端基数据类型。
/// </summary>
public partial class ThingsGatewayBitConverter : IThingsGatewayBitConverter
{
    /// <inheritdoc/>
#if NET6_0_OR_GREATER

    [System.Text.Json.Serialization.JsonConverter(typeof(EncodingConverter))]
    [JsonConverter(typeof(NewtonsoftEncodingConverter))]
    public Encoding? Encoding { get; set; }
#else

    [JsonConverter(typeof(NewtonsoftEncodingConverter))]
    public Encoding? Encoding { get; set; }

#endif

    public Encoding EncodingValue => Encoding ?? Encoding.UTF8;

    /// <inheritdoc/>
    public DataFormatEnum DataFormat { get; set; }

    /// <inheritdoc/>
    public virtual BcdFormatEnum? BcdFormat { get; set; }

    /// <inheritdoc/>
    public virtual int? StringLength { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    public ThingsGatewayBitConverter()
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="endianType"></param>
    public ThingsGatewayBitConverter(EndianType endianType)
    {
        EndianType = endianType;
    }

    /// <inheritdoc/>
    public virtual EndianType EndianType
    {
        get => endianType; set
        {
            endianType = value;
            TouchSocketBitConverter = new TouchSocketBitConverter(endianType);
        }
    }

    /// <inheritdoc/>
    public virtual bool IsStringReverseByteWord { get; set; }

    /// <inheritdoc/>
    public virtual bool IsVariableStringLength { get; set; }

    internal protected TouchSocketBitConverter TouchSocketBitConverter { get; set; }

    static ThingsGatewayBitConverter()
    {
        BigEndian = new ThingsGatewayBitConverter(EndianType.Big);
        LittleEndian = new ThingsGatewayBitConverter(EndianType.Little);
    }

    /// <summary>
    /// 以大端
    /// </summary>
    public static readonly ThingsGatewayBitConverter BigEndian;

    /// <summary>
    /// 以小端
    /// </summary>
    public static readonly ThingsGatewayBitConverter LittleEndian;
    private EndianType endianType;

    public virtual void OtherPropertySet(IThingsGatewayBitConverter thingsGatewayBitConverter, string registerAddress)
    {
    }

    /// <summary>
    /// 从设备地址中解析附加信息
    /// 这个方法获取<see cref="IThingsGatewayBitConverter"/>
    /// 解析步骤将被缓存。
    /// </summary>
    /// <param name="registerAddress">设备地址</param>
    /// <returns><see cref="IThingsGatewayBitConverter"/> 实例</returns>
    public virtual IThingsGatewayBitConverter GetTransByAddress(string? registerAddress)
    {
        if (registerAddress.IsNullOrEmpty()) return this;

        var type = this.GetType();
        // 尝试从缓存中获取解析结果
        //var cacheKey = $"{nameof(ThingsGatewayBitConverterExtension)}_{nameof(GetTransByAddress)}_{type.FullName}_{type.TypeHandle.Value}_{this.ToJsonString()}_{registerAddress}_{this.GetHashCode()}";
        //if (MemoryCache.TryGetValue(cacheKey, out IThingsGatewayBitConverter cachedConverter))
        //{
        //    if (cachedConverter.Equals(this))
        //    {
        //        return this;
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

        var converter = (IThingsGatewayBitConverter)this!.Map(type);
        // 如果没有解析出任何附加信息，则直接返回默认的数据转换器
        if (bcdFormat == null && stringlength == null && encoding == null && dataFormat == null && wstring == null)
        {
            //MemoryCache.Set(cacheKey, this!, 3600);
            return converter;
        }

        // 根据默认的数据转换器创建新的数据转换器实例

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
        OtherPropertySet(converter, registerAddress);
        // 将解析结果添加到缓存中，缓存有效期为3600秒
        //MemoryCache.Set(cacheKey, converter!, 3600);
        return converter;
    }
    #region GetBytes

    /// <inheritdoc/>
    public virtual Memory<byte> GetBytes(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return Memory<byte>.Empty;
        }
        if (StringLength != null)
        {
            if (BcdFormat != null)
            {
                var bytes = DataTransUtil.GetBytesFromBCD(value, BcdFormat.Value).AsMemory();
                return IsStringReverseByteWord ? bytes.BytesReverseByWord().ArrayExpandToLength(StringLength.Value) : bytes.ArrayExpandToLength(StringLength.Value);
            }
            else
            {
                var bytes = Encoding.GetBytes(value).AsMemory();
                return IsStringReverseByteWord ? bytes.BytesReverseByWord().ArrayExpandToLength(StringLength.Value) : bytes.ArrayExpandToLength(StringLength.Value);
            }
        }
        else
        {
            if (BcdFormat != null)
            {
                var bytes = DataTransUtil.GetBytesFromBCD(value, BcdFormat.Value).AsMemory();
                return IsStringReverseByteWord ? bytes.BytesReverseByWord() : bytes;
            }
            else
            {
                var bytes = Encoding.GetBytes(value).AsMemory();
                return IsStringReverseByteWord ? bytes.BytesReverseByWord() : bytes;
            }
        }
    }



    /// <inheritdoc/>
    public virtual byte[] GetBytes(decimal value)
    {
        return GetTBytes(value);
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(char value)
    {
        return GetTBytes(value);
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(bool value)
    {
        return GetTBytes(value);
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(short value)
    {
        return GetTBytes(value);
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(ushort value)
    {
        return GetTBytes(value);
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(ReadOnlySpan<bool> values)
    {
        return GetTBytes(values);
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(ReadOnlySpan<short> value)
    {
        return GetTBytes(value);
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(ReadOnlySpan<ushort> value)
    {
        return GetTBytes(value);
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(ReadOnlySpan<int> value)
    {
        return GetTBytes(value);
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(ReadOnlySpan<uint> value)
    {
        return GetTBytes(value);
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(ReadOnlySpan<long> value)
    {
        return GetTBytes(value);
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(ReadOnlySpan<ulong> value)
    {
        return GetTBytes(value);
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(ReadOnlySpan<float> value)
    {
        return GetTBytes(value);
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(ReadOnlySpan<double> value)
    {
        return GetTBytes(value);
    }
    /// <inheritdoc/>
    public virtual byte[] GetBytes(ReadOnlySpan<decimal> value)
    {
        return GetTBytes(value);
    }
    #endregion GetBytes

    /// <inheritdoc/>
    public virtual string ToString(ReadOnlySpan<byte> buffer, int offset, int length)
    {
        buffer = buffer.Slice(offset, length);
        if (BcdFormat != null)
        {
            return IsStringReverseByteWord ? DataTransUtil.GetBcdValue(buffer.BytesReverseByWord(), BcdFormat.Value) : DataTransUtil.GetBcdValue(buffer, BcdFormat.Value);
        }
        else
        {
            return IsStringReverseByteWord ?
                buffer.BytesReverseByWord().ToString(Encoding).TrimEnd().Replace($"\0", "") :
                buffer.ToString(Encoding).TrimEnd().Replace($"\0", "");
        }
    }

    /// <inheritdoc/>
    public virtual bool ToBoolean(ReadOnlySpan<byte> buffer, int offset, bool isReverse)
    {
        ReadOnlySpan<byte> bytes;
        if (isReverse)
            bytes = buffer.BytesReverseByWord();
        else
            bytes = buffer;
        return bytes.GetBoolByIndex(offset);
    }

    /// <inheritdoc/>
    public virtual byte ToByte(ReadOnlySpan<byte> buffer, int offset)
    {
        return buffer[offset];
    }

    /// <inheritdoc/>
    public virtual char ToChar(ReadOnlySpan<byte> buffer, int offset)
    {
        return To<char>(buffer.Slice(offset));
    }

    /// <inheritdoc/>
    public virtual decimal ToDecimal(ReadOnlySpan<byte> buffer, int offset)
    {
        return To<decimal>(buffer.Slice(offset));
    }

    /// <inheritdoc/>
    public virtual short ToInt16(ReadOnlySpan<byte> buffer, int offset)
    {
        return To<Int16>(buffer.Slice(offset));
    }

    /// <inheritdoc/>
    public virtual int ToInt32(ReadOnlySpan<byte> buffer, int offset)
    {
        return To<Int32>(buffer.Slice(offset));
    }

    /// <inheritdoc/>
    public virtual long ToInt64(ReadOnlySpan<byte> buffer, int offset)
    {
        return To<Int64>(buffer.Slice(offset));
    }

    /// <inheritdoc/>
    public virtual ushort ToUInt16(ReadOnlySpan<byte> buffer, int offset)
    {
        return To<UInt16>(buffer.Slice(offset));
    }

    /// <inheritdoc/>
    public virtual uint ToUInt32(ReadOnlySpan<byte> buffer, int offset)
    {
        return To<UInt32>(buffer.Slice(offset));
    }

    /// <inheritdoc/>
    public virtual ulong ToUInt64(ReadOnlySpan<byte> buffer, int offset)
    {
        return To<UInt64>(buffer.Slice(offset));
    }

    /// <inheritdoc/>
    public virtual float ToSingle(ReadOnlySpan<byte> buffer, int offset)
    {
        return To<float>(buffer.Slice(offset));
    }

    /// <inheritdoc/>
    public virtual double ToDouble(ReadOnlySpan<byte> buffer, int offset)
    {
        return To<double>(buffer.Slice(offset));
    }

    /// <inheritdoc/>
    public virtual bool[] ToBoolean(ReadOnlySpan<byte> buffer, int offset, int len, bool isReverse = false)
    {
        bool[] result = new bool[len];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = ToBoolean(buffer, offset + i, isReverse);
        }
        return result;
    }

    /// <inheritdoc/>
    public virtual byte[] ToByte(ReadOnlySpan<byte> buffer, int offset, int length)
    {
        byte[] bytes = new byte[length];
        buffer.Slice(offset, length).CopyTo(bytes);
        return bytes;
    }

    /// <inheritdoc/>
    public virtual double[] ToDouble(ReadOnlySpan<byte> buffer, int offset, int len)
    {
        double[] numArray = new double[len];
        for (int index = 0; index < len; ++index)
        {
            numArray[index] = ToDouble(buffer, offset + 8 * index);
        }
        return numArray;
    }

    /// <inheritdoc/>
    public virtual short[] ToInt16(ReadOnlySpan<byte> buffer, int offset, int len)
    {
        short[] numArray = new short[len];
        for (int index = 0; index < len; ++index)
        {
            numArray[index] = ToInt16(buffer, offset + 2 * index);
        }
        return numArray;
    }

    /// <inheritdoc/>
    public virtual int[] ToInt32(ReadOnlySpan<byte> buffer, int offset, int len)
    {
        int[] numArray = new int[len];
        for (int index = 0; index < len; ++index)
        {
            numArray[index] = ToInt32(buffer, offset + 4 * index);
        }
        return numArray;
    }

    /// <inheritdoc/>
    public virtual long[] ToInt64(ReadOnlySpan<byte> buffer, int offset, int len)
    {
        long[] numArray = new long[len];
        for (int index = 0; index < len; ++index)
        {
            numArray[index] = ToInt64(buffer, offset + 8 * index);
        }
        return numArray;
    }

    /// <inheritdoc/>
    public virtual float[] ToSingle(ReadOnlySpan<byte> buffer, int offset, int len)
    {
        float[] numArray = new float[len];
        for (int index = 0; index < len; ++index)
        {
            numArray[index] = ToSingle(buffer, offset + 4 * index);
        }
        return numArray;
    }

    /// <inheritdoc/>
    public virtual ushort[] ToUInt16(ReadOnlySpan<byte> buffer, int offset, int len)
    {
        ushort[] numArray = new ushort[len];
        for (int index = 0; index < len; ++index)
        {
            numArray[index] = ToUInt16(buffer, offset + 2 * index);
        }
        return numArray;
    }

    /// <inheritdoc/>
    public virtual uint[] ToUInt32(ReadOnlySpan<byte> buffer, int offset, int len)
    {
        uint[] numArray = new uint[len];
        for (int index = 0; index < len; ++index)
        {
            numArray[index] = ToUInt32(buffer, offset + 4 * index);
        }
        return numArray;
    }

    /// <inheritdoc/>
    public virtual ulong[] ToUInt64(ReadOnlySpan<byte> buffer, int offset, int len)
    {
        ulong[] numArray = new ulong[len];
        for (int index = 0; index < len; ++index)
        {
            numArray[index] = ToUInt64(buffer, offset + 8 * index);
        }
        return numArray;
    }

    /// <inheritdoc/>
    public virtual decimal[] ToDecimal(ReadOnlySpan<byte> buffer, int offset, int len)
    {
        decimal[] numArray = new decimal[len];
        for (int index = 0; index < len; ++index)
        {
            numArray[index] = ToDecimal(buffer, offset + 8 * index);
        }
        return numArray;
    }



    /// <inheritdoc/>
    public virtual byte[] GetTBytes<T>(ReadOnlySpan<T> value) where T : unmanaged
    {
        var size = Unsafe.SizeOf<T>();
        using (ValueByteBlock byteBlock = new ValueByteBlock(value.Length * size))
        {
            for (int index = 0; index < value.Length; ++index)
            {
                var bytes = GetTBytes(value[index]);
                byteBlock.Write(bytes);
            }
            return byteBlock.ToArray();
        }
    }


    private byte[] GetTBytes<T>(T value) where T : unmanaged
    {
        var size = Unsafe.SizeOf<T>();
        var bytes = new byte[size];
        this.WriteBytes(bytes, value);
        return bytes;
    }


    /// <summary>
    /// 将指定值的字节表示形式写入到指定的字节跨度中。
    /// </summary>
    /// <typeparam name="T">要写入的值的类型，必须是非托管类型。</typeparam>
    /// <param name="span">要写入字节的目标跨度。</param>
    /// <param name="value">要写入的值。</param>
    /// <returns>写入的字节数。</returns>
    private unsafe int WriteBytes<T>(Span<byte> span, T value) where T : unmanaged
    {
        var size = sizeof(T);

        if (span.Length < size)
        {
            throw new ArgumentOutOfRangeException(nameof(span.Length), TouchSocketCoreResource.ValueLessThan.Format(nameof(span.Length), span.Length, size));
        }

        if (size == 2)
        {
            return TouchSocketBitConverter.WriteBytes(span, value);
        }

        Unsafe.As<byte, T>(ref span[0]) = value;

        if (size == 1)
        {
            // 对于单字节类型，不需要转换
            return size;
        }

        if (!TouchSocketBitConverter.IsSameOfSet())
        {

            if (size == 4)
            {
                this.ByteTransDataFormat4_Net6(ref span[0]);
            }
            else if (size == 8)
            {
                this.ByteTransDataFormat8_Net6(ref span[0]);
            }
            else if (size == 16)
            {
                this.ByteTransDataFormat16_Net6(ref span[0]);
            }
            else
            {
                throw new NotSupportedException(size.ToString());
            }
        }

        return size;
    }

    /// <summary>
    /// 将字节跨度转换为指定类型
    /// </summary>
    /// <typeparam name="T">要转换成的类型</typeparam>
    /// <param name="span">要转换的字节跨度</param>
    /// <returns>转换后的值</returns>
    /// <exception cref="ArgumentOutOfRangeException">当字节跨度长度不足以表示类型T时抛出</exception>
    /// <exception cref="NotSupportedException">当类型T不支持时抛出</exception>
    public unsafe T To<T>(ReadOnlySpan<byte> span) where T : unmanaged
    {
        var size = sizeof(T);
        if (span.Length < size)
        {
            throw new ArgumentOutOfRangeException(nameof(span.Length), TouchSocketCoreResource.ValueLessThan.Format(nameof(span.Length), span.Length, size));
        }

        if (size == 2)
        {
            return TouchSocketBitConverter.To<T>(span);
        }

        fixed (byte* p = &span[0])
        {
            if (this.TouchSocketBitConverter.IsSameOfSet())
            {
                return Unsafe.Read<T>(p);
            }
            else
            {
                if (size == 4)
                {
                    this.ByteTransDataFormat4_Net6(p);
                    var v = Unsafe.Read<T>(p);
                    this.ByteTransDataFormat4_Net6(p);
                    return v;
                }
                else if (size == 8)
                {
                    this.ByteTransDataFormat8_Net6(p);
                    var v = Unsafe.Read<T>(p);
                    this.ByteTransDataFormat8_Net6(p);
                    return v;
                }
                else if (size == 16)
                {
                    this.ByteTransDataFormat16_Net6(p);
                    var v = Unsafe.Read<T>(p);
                    this.ByteTransDataFormat16_Net6(p);
                    return v;
                }
                else
                {
                    throw new NotSupportedException(size.ToString());
                }
            }
        }
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(int value)
    {
        return GetTBytes(value);
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(uint value)
    {
        return GetTBytes(value);
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(long value)
    {
        return GetTBytes(value);
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(ulong value)
    {
        return GetTBytes(value);
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(float value)
    {
        return GetTBytes(value);
    }

    /// <inheritdoc/>
    public virtual byte[] GetBytes(double value)
    {
        return GetTBytes(value);
    }




    #region Tool

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ByteTransDataFormat4_Net6(ref byte value)
    {
        unsafe
        {
            fixed (byte* p = &value)
            {
                var a = Unsafe.ReadUnaligned<byte>(p);
                var b = Unsafe.ReadUnaligned<byte>(p + 1);
                var c = Unsafe.ReadUnaligned<byte>(p + 2);
                var d = Unsafe.ReadUnaligned<byte>(p + 3);

                switch (DataFormat)
                {
                    case DataFormatEnum.ABCD:
                        Unsafe.WriteUnaligned(p, d);
                        Unsafe.WriteUnaligned(p + 1, c);
                        Unsafe.WriteUnaligned(p + 2, b);
                        Unsafe.WriteUnaligned(p + 3, a);
                        break;

                    case DataFormatEnum.BADC:
                        Unsafe.WriteUnaligned(p, c);
                        Unsafe.WriteUnaligned(p + 1, d);
                        Unsafe.WriteUnaligned(p + 2, a);
                        Unsafe.WriteUnaligned(p + 3, b);
                        break;

                    case DataFormatEnum.CDAB:
                        Unsafe.WriteUnaligned(p, b);
                        Unsafe.WriteUnaligned(p + 1, a);
                        Unsafe.WriteUnaligned(p + 2, d);
                        Unsafe.WriteUnaligned(p + 3, c);
                        break;

                    case DataFormatEnum.DCBA:
                        return;
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ByteTransDataFormat8_Net6(ref byte value)
    {
        unsafe
        {
            fixed (byte* p = &value)
            {
                var a = Unsafe.ReadUnaligned<byte>(p);
                var b = Unsafe.ReadUnaligned<byte>(p + 1);
                var c = Unsafe.ReadUnaligned<byte>(p + 2);
                var d = Unsafe.ReadUnaligned<byte>(p + 3);
                var e = Unsafe.ReadUnaligned<byte>(p + 4);
                var f = Unsafe.ReadUnaligned<byte>(p + 5);
                var g = Unsafe.ReadUnaligned<byte>(p + 6);
                var h = Unsafe.ReadUnaligned<byte>(p + 7);

                switch (DataFormat)
                {
                    case DataFormatEnum.ABCD:
                        Unsafe.WriteUnaligned(p, h);
                        Unsafe.WriteUnaligned(p + 1, g);
                        Unsafe.WriteUnaligned(p + 2, f);
                        Unsafe.WriteUnaligned(p + 3, e);
                        Unsafe.WriteUnaligned(p + 4, d);
                        Unsafe.WriteUnaligned(p + 5, c);
                        Unsafe.WriteUnaligned(p + 6, b);
                        Unsafe.WriteUnaligned(p + 7, a);
                        break;

                    case DataFormatEnum.BADC:
                        Unsafe.WriteUnaligned(p, g);
                        Unsafe.WriteUnaligned(p + 1, h);
                        Unsafe.WriteUnaligned(p + 2, e);
                        Unsafe.WriteUnaligned(p + 3, f);
                        Unsafe.WriteUnaligned(p + 4, c);
                        Unsafe.WriteUnaligned(p + 5, d);
                        Unsafe.WriteUnaligned(p + 6, a);
                        Unsafe.WriteUnaligned(p + 7, b);
                        break;

                    case DataFormatEnum.CDAB:
                        Unsafe.WriteUnaligned(p, b);
                        Unsafe.WriteUnaligned(p + 1, a);
                        Unsafe.WriteUnaligned(p + 2, d);
                        Unsafe.WriteUnaligned(p + 3, c);
                        Unsafe.WriteUnaligned(p + 4, f);
                        Unsafe.WriteUnaligned(p + 5, e);
                        Unsafe.WriteUnaligned(p + 6, h);
                        Unsafe.WriteUnaligned(p + 7, g);
                        break;

                    case DataFormatEnum.DCBA:
                        break;
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ByteTransDataFormat16_Net6(ref byte value)
    {
        unsafe
        {
            fixed (byte* p = &value)
            {
                switch (DataFormat)
                {
                    case DataFormatEnum.ABCD:
                        var span = new Span<byte>(p, 16);
                        span.Reverse();
                        break;

                    case DataFormatEnum.DCBA:
                        return;

                    default:
                        throw new NotSupportedException();
                }
            }
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void ByteTransDataFormat4_Net6(byte* p)
    {
        var a = Unsafe.ReadUnaligned<byte>(p);
        var b = Unsafe.ReadUnaligned<byte>(p + 1);
        var c = Unsafe.ReadUnaligned<byte>(p + 2);
        var d = Unsafe.ReadUnaligned<byte>(p + 3);

        switch (this.DataFormat)
        {
            case DataFormatEnum.ABCD:
                Unsafe.WriteUnaligned(p, d);
                Unsafe.WriteUnaligned(p + 1, c);
                Unsafe.WriteUnaligned(p + 2, b);
                Unsafe.WriteUnaligned(p + 3, a);
                break;

            case DataFormatEnum.BADC:
                Unsafe.WriteUnaligned(p, c);
                Unsafe.WriteUnaligned(p + 1, d);
                Unsafe.WriteUnaligned(p + 2, a);
                Unsafe.WriteUnaligned(p + 3, b);
                break;

            case DataFormatEnum.CDAB:
                Unsafe.WriteUnaligned(p, b);
                Unsafe.WriteUnaligned(p + 1, a);
                Unsafe.WriteUnaligned(p + 2, d);
                Unsafe.WriteUnaligned(p + 3, c);
                break;

            case DataFormatEnum.DCBA:
                return;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void ByteTransDataFormat8_Net6(byte* p)
    {
        var a = Unsafe.ReadUnaligned<byte>(p);
        var b = Unsafe.ReadUnaligned<byte>(p + 1);
        var c = Unsafe.ReadUnaligned<byte>(p + 2);
        var d = Unsafe.ReadUnaligned<byte>(p + 3);
        var e = Unsafe.ReadUnaligned<byte>(p + 4);
        var f = Unsafe.ReadUnaligned<byte>(p + 5);
        var g = Unsafe.ReadUnaligned<byte>(p + 6);
        var h = Unsafe.ReadUnaligned<byte>(p + 7);

        switch (this.DataFormat)
        {
            case DataFormatEnum.ABCD:
                Unsafe.WriteUnaligned(p, h);
                Unsafe.WriteUnaligned(p + 1, g);
                Unsafe.WriteUnaligned(p + 2, f);
                Unsafe.WriteUnaligned(p + 3, e);
                Unsafe.WriteUnaligned(p + 4, d);
                Unsafe.WriteUnaligned(p + 5, c);
                Unsafe.WriteUnaligned(p + 6, b);
                Unsafe.WriteUnaligned(p + 7, a);
                break;

            case DataFormatEnum.BADC:
                Unsafe.WriteUnaligned(p, g);
                Unsafe.WriteUnaligned(p + 1, h);
                Unsafe.WriteUnaligned(p + 2, e);
                Unsafe.WriteUnaligned(p + 3, f);
                Unsafe.WriteUnaligned(p + 4, c);
                Unsafe.WriteUnaligned(p + 5, d);
                Unsafe.WriteUnaligned(p + 6, a);
                Unsafe.WriteUnaligned(p + 7, b);
                break;

            case DataFormatEnum.CDAB:
                Unsafe.WriteUnaligned(p, b);
                Unsafe.WriteUnaligned(p + 1, a);
                Unsafe.WriteUnaligned(p + 2, d);
                Unsafe.WriteUnaligned(p + 3, c);
                Unsafe.WriteUnaligned(p + 4, f);
                Unsafe.WriteUnaligned(p + 5, e);
                Unsafe.WriteUnaligned(p + 6, h);
                Unsafe.WriteUnaligned(p + 7, g);
                break;

            case DataFormatEnum.DCBA:
                break;
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void ByteTransDataFormat16_Net6(byte* p)
    {
        switch (this.DataFormat)
        {
            case DataFormatEnum.ABCD:
                var span = new Span<byte>(p, 16);
                span.Reverse();
                break;

            case DataFormatEnum.DCBA:
                return;

            default:
            case DataFormatEnum.CDAB:
            case DataFormatEnum.BADC:
                throw new NotSupportedException();
        }
    }

    #endregion Tool
}
