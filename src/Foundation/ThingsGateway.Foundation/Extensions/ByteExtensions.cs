//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Text;

namespace ThingsGateway.Foundation;

/// <inheritdoc/>
public static class ByteExtensions
{
    public static T[] SpliceArray<T>(this T[] array, params T[] values)
    {
        return DataTransUtil.SpliceArray<T>(array, values);
    }


    /// <summary>
    /// 获取byte数据类型的第offset位，是否为True<br />
    /// </summary>
    /// <param name="value">byte数值</param>
    /// <param name="offset">索引位置</param>
    /// <returns>结果</returns>
    public static bool BoolOnByteIndex(this byte value, int offset)
    {
        if (offset < 0 || offset > 7)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset value must be between 0 and 7.");
        }

        byte mask = (byte)(1 << offset);
        return (value & mask) == mask;
    }

    public static byte[] BoolToByte(this ReadOnlySpan<bool> value, byte trueData = 0xff)
    {
        byte[] bytes = new byte[value.Length];
        for (int i = 0; i < value.Length; i++)
        {
            bytes[i] = value[i] ? (byte)trueData : (byte)0;
        }
        return bytes;
    }
    public static bool[] ByteToBool(this ReadOnlySpan<byte> value)
    {
        bool[] bytes = new bool[value.Length];
        for (int i = 0; i < value.Length; i++)
        {
            bytes[i] = value[i] > 0 ? true : false;
        }
        return bytes;
    }
    /// <summary>
    /// 数组内容分别相加某个数字
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static byte[] BytesAdd(this byte[] bytes, int value)
    {
        return BytesAdd((ReadOnlySpan<byte>)bytes, value);
    }
    /// <summary>
    /// 数组内容分别相加某个数字
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static byte[] BytesAdd(this Span<byte> bytes, int value)
    {
        return BytesAdd((ReadOnlySpan<byte>)bytes, value);
    }
    /// <summary>
    /// 数组内容分别相加某个数字
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static byte[] BytesAdd(this ReadOnlySpan<byte> bytes, int value)
    {
        if (bytes.IsEmpty) return Array.Empty<byte>();
        byte[] result = new byte[bytes.Length];
        for (int index = 0; index < bytes.Length; index++)
        {
            result[index] = (byte)(bytes[index] + value);
        }

        return result;
    }

    /// <summary>
    /// 将byte数组按照双字节进行反转，如果为单数的情况，则自动补齐<br />
    /// </summary>
    /// <param name="inBytes">输入的字节信息</param>
    /// <returns>反转后的数据</returns>
    /// <summary>
    /// 将字节数组按“字（2字节）”为单位反转高低位，奇数长度自动补齐 0。
    /// </summary>
    public static ReadOnlySpan<byte> BytesReverseByWord(this ReadOnlySpan<byte> inBytes)
    {
        int len = inBytes.Length;
        if (len == 0)
            return ReadOnlySpan<byte>.Empty;

        // 如果是奇数，自动补齐 0
        int evenLen = (len % 2 == 0) ? len : len + 1;
        if (evenLen == len) return inBytes;

        Span<byte> result = new byte[evenLen];
        inBytes.CopyTo(result);

        // 逐字（2 字节）交换
        for (int i = 0; i < evenLen; i += 2)
        {
            byte temp = result[i];
            result[i] = result[i + 1];
            result[i + 1] = temp;
        }

        return result;
    }

    /// <summary>
    /// 将byte数组按照双字节进行反转，如果为单数的情况，则自动补齐<br />
    /// </summary>
    /// <param name="inBytes">输入的字节信息</param>
    /// <returns>反转后的数据</returns>
    /// <summary>
    /// 将字节数组按“字（2字节）”为单位反转高低位，奇数长度自动补齐 0。
    /// </summary>
    public static Memory<byte> BytesReverseByWord(this Memory<byte> inBytes)
    {
        int len = inBytes.Length;
        if (len == 0)
            return Memory<byte>.Empty;

        // 如果是奇数，自动补齐 0
        int evenLen = (len % 2 == 0) ? len : len + 1;
        if (evenLen == len) return inBytes;

        byte[] result = new byte[evenLen];
        inBytes.CopyTo(result);

        // 逐字（2 字节）交换
        for (int i = 0; i < evenLen; i += 2)
        {
            byte temp = result[i];
            result[i] = result[i + 1];
            result[i + 1] = temp;
        }

        return result;
    }

    /// <summary>
    /// 从字节数组中提取位数组，length 代表位数
    /// </summary>
    /// <param name="inBytes">原始的字节数组</param>
    /// <param name="length">想要转换的位数，如果超出字节数组长度 * 8，则自动缩小为数组最大长度</param>
    /// <returns>转换后的布尔数组</returns>
    public static ReadOnlySpan<bool> ByteToBoolArray(this ReadOnlySpan<byte> inBytes, int length)
    {
        // 计算字节数组能够提供的最大位数
        int maxBitLength = inBytes.Length * 8;

        // 如果指定长度超出最大位数，则将长度缩小为最大位数
        if (length > maxBitLength)
        {
            length = maxBitLength;
        }

        // 创建对应长度的布尔数组
        bool[] boolArray = new bool[length];

        // 从字节数组中提取位信息并转换为布尔值存储到布尔数组中
        for (int index = 0; index < length; ++index)
        {
            boolArray[index] = inBytes[index / 8].BoolOnByteIndex(index % 8);
        }

        return boolArray;
    }


    public static ReadOnlyMemory<byte> CombineMemoryBlocks(this List<ReadOnlyMemory<byte>> blocks)
    {
        if (blocks == null || blocks.Count == 0)
            return ReadOnlyMemory<byte>.Empty;

        // 计算总长度
        int totalLength = 0;
        foreach (var block in blocks)
        {
            totalLength += block.Length;
        }

        if (totalLength == 0)
            return ReadOnlyMemory<byte>.Empty;

        // 分配目标数组
        byte[] result = new byte[totalLength];
        int offset = 0;

        // 拷贝每一段内存
        foreach (var block in blocks)
        {
            block.Span.CopyTo(result.AsSpan(offset));
            offset += block.Length;
        }

        return result;
    }

    /// <summary>
    /// 获取异或校验,返回ASCII十六进制字符串的字节数组<br />
    /// </summary>
    public static byte[] GetAsciiXOR(this ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
            return Array.Empty<byte>();

        byte xor = data[0];
        for (int i = 1; i < data.Length; i++)
        {
            xor ^= data[i];
        }

        // 将结果转换为 2 位 ASCII 十六进制字符串，如 "3F" -> [0x33, 0x46]
        byte[] result = Encoding.ASCII.GetBytes(xor.ToString("X2"));
        return result;
    }


    /// <summary>
    /// 获取Byte数组的第 boolIndex 偏移的bool值，这个偏移值可以为 10，就是第 1 个字节的 第3位 <br />
    /// </summary>
    /// <param name="bytes">字节数组信息</param>
    /// <param name="boolIndex">指定字节的位偏移</param>
    /// <returns>bool值</returns>
    public static bool GetBoolByIndex(this ReadOnlySpan<byte> bytes, int boolIndex)
    {
        return bytes[boolIndex / 8].BoolOnByteIndex(boolIndex % 8);
    }

    /// <summary>
    /// 字节数组默认转16进制字符
    /// </summary>
    /// <returns></returns>
    public static string ToHexString(this ArraySegment<byte> buffer, char splite = default, int newLineCount = 0)
    {
        return DataTransUtil.ByteToHexString(buffer, splite, newLineCount);
    }

    /// <summary>
    /// 字节数组默认转16进制字符
    /// </summary>
    /// <returns></returns>
    public static string ToHexString(this byte[] buffer, char splite = default, int newLineCount = 0)
    {
        return DataTransUtil.ByteToHexString(buffer, splite, newLineCount);
    }
    /// <summary>
    /// 字节数组默认转16进制字符
    /// </summary>
    /// <returns></returns>
    public static string ToHexString(this Span<byte> buffer, char splite = default, int newLineCount = 0)
    {
        return DataTransUtil.ByteToHexString(buffer, splite, newLineCount);
    }
    /// <summary>
    /// 字节数组默认转16进制字符
    /// </summary>
    /// <returns></returns>
    public static string ToHexString(this ReadOnlySpan<byte> buffer, char splite = default, int newLineCount = 0)
    {
        return DataTransUtil.ByteToHexString(buffer, splite, newLineCount);
    }
}
