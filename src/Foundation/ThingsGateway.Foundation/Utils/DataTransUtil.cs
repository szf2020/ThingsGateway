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

/// <summary>
/// 常用转换
/// </summary>
public static class DataTransUtil
{
    /// <summary>
    /// 字节数据转化成16进制表示的字符串
    /// </summary>
    /// <param name="InBytes">字节数组</param>
    /// <param name="segment">分割符</param>
    /// <param name="newLineCount">指定在何处换行，设为0则不换行</param>
    /// <returns>返回的字符串</returns>
    public static string ByteToHexString(ReadOnlySpan<byte> InBytes, char segment = default, int newLineCount = 0) => ByteToHexString(InBytes, 0, InBytes.Length, segment, newLineCount);

    /// <summary>
    /// 将字节数组转换为十六进制表示的字符串
    /// </summary>
    /// <param name="inBytes">输入的字节数组</param>
    /// <param name="offset">offset</param>
    /// <param name="length">length</param>
    /// <param name="segment">用于分隔每个字节的字符</param>
    /// <param name="newLineCount">指定在何处换行，设为0则不换行</param>
    /// <returns>转换后的十六进制字符串</returns>
    public static string ByteToHexString(ReadOnlySpan<byte> inBytes, int offset, int length, char segment = default, int newLineCount = 0)
    {
        // 合法性检查：避免越界和空输入
        if (inBytes.IsEmpty || offset < 0 || length <= 0 || offset + length > inBytes.Length)
            return string.Empty;
        // 每个字节占 2 个字符，如果有分隔符 +1，再加换行（估算上限）
        int estimatedSize = length * (segment != default ? 3 : 2) + (length / Math.Max(newLineCount, int.MaxValue));
        StringBuilder sb = new StringBuilder(estimatedSize);

        int end = offset + length;
        for (int i = offset; i < end; i++)
        {
            sb.Append(inBytes[i].ToString("X2")); // 转大写16进制

            if (segment != default && i < end - 1)
                sb.Append(segment);

            if (newLineCount > 0 && (i - offset + 1) % newLineCount == 0)
                sb.AppendLine();
        }

        return sb.ToString();
    }


    /// <summary>
    /// 获取Bcd值
    /// </summary>
    public static byte GetBcdCodeFromChar(char value, BcdFormatEnum format)
    {
        return format switch
        {
            BcdFormatEnum.C8421 => value switch
            {
                '0' => 0,
                '1' => 1,
                '2' => 2,
                '3' => 3,
                '4' => 4,
                '5' => 5,
                '6' => 6,
                '7' => 7,
                '8' => 8,
                '9' => 9,
                _ => byte.MaxValue,
            },
            BcdFormatEnum.C5421 => value switch
            {
                '0' => 0,
                '1' => 1,
                '2' => 2,
                '3' => 3,
                '4' => 4,
                '5' => 8,
                '6' => 9,
                '7' => 10,
                '8' => 11,
                '9' => 12,
                _ => byte.MaxValue,
            },
            BcdFormatEnum.C2421 => value switch
            {
                '0' => 0,
                '1' => 1,
                '2' => 2,
                '3' => 3,
                '4' => 4,
                '5' => 11,
                '6' => 12,
                '7' => 13,
                '8' => 14,
                '9' => 15,
                _ => byte.MaxValue,
            },
            BcdFormatEnum.C3 => value switch
            {
                '0' => 3,
                '1' => 4,
                '2' => 5,
                '3' => 6,
                '4' => 7,
                '5' => 8,
                '6' => 9,
                '7' => 10,
                '8' => 11,
                '9' => 12,
                _ => byte.MaxValue,
            },
            BcdFormatEnum.Gray => value switch
            {
                '0' => 0,
                '1' => 1,
                '2' => 3,
                '3' => 2,
                '4' => 6,
                '5' => 7,
                '6' => 5,
                '7' => 4,
                '8' => 12,
                '9' => 8,
                _ => byte.MaxValue,
            },
            _ => byte.MaxValue,
        };
    }

    /// <summary>
    /// 获取Bcd值
    /// </summary>
    public static string GetBcdFromByte(int value, BcdFormatEnum format)
    {
        return format switch
        {
            BcdFormatEnum.C8421 => value switch
            {
                0 => "0",
                1 => "1",
                2 => "2",
                3 => "3",
                4 => "4",
                5 => "5",
                6 => "6",
                7 => "7",
                8 => "8",
                9 => "9",
                _ => "*",
            },
            BcdFormatEnum.C5421 => value switch
            {
                0 => "0",
                1 => "1",
                2 => "2",
                3 => "3",
                4 => "4",
                8 => "5",
                9 => "6",
                10 => "7",
                11 => "8",
                12 => "9",
                _ => "*",
            },
            BcdFormatEnum.C2421 => value switch
            {
                0 => "0",
                1 => "1",
                2 => "2",
                3 => "3",
                4 => "4",
                11 => "5",
                12 => "6",
                13 => "7",
                14 => "8",
                15 => "9",
                _ => "*",
            },
            BcdFormatEnum.C3 => value switch
            {
                3 => "0",
                4 => "1",
                5 => "2",
                6 => "3",
                7 => "4",
                8 => "5",
                9 => "6",
                10 => "7",
                11 => "8",
                12 => "9",
                _ => "*",
            },
            BcdFormatEnum.Gray => value switch
            {
                0 => "0",
                1 => "1",
                2 => "3",
                3 => "2",
                4 => "7",
                5 => "6",
                6 => "4",
                7 => "5",
                8 => "9",
                12 => "8",
                _ => "*",
            },
            _ => "*",
        };
    }

    /// <summary>
    /// 根据指定的字节数组和Bcd格式返回对应的Bcd值
    /// </summary>
    /// <param name="buffer">输入的字节数组</param>
    /// <param name="format">Bcd格式枚举</param>
    /// <returns>转换后的Bcd值字符串</returns>
    public static string GetBcdValue(ReadOnlySpan<byte> buffer, BcdFormatEnum format)
    {
        // 用于存储最终的Bcd值的字符串构建器
        StringBuilder stringBuilder = new();

        // 遍历字节数组进行Bcd值计算
        for (int index = 0; index < buffer.Length; ++index)
        {
            // 获取当前字节的低四位和高四位
            int num1 = buffer[index] & 15;
            int num2 = buffer[index] >> 4;

            // 根据指定的Bcd格式将每个字节转换为Bcd并追加到字符串构建器中
            stringBuilder.Append(GetBcdFromByte(num2, format));
            stringBuilder.Append(GetBcdFromByte(num1, format));
        }

        // 返回最终的Bcd值字符串
        return stringBuilder.ToString();
    }

    /// <summary>
    /// 根据给定的Bcd值字符串和Bcd格式返回对应的字节数组
    /// </summary>
    /// <param name="value">Bcd值字符串</param>
    /// <param name="format">Bcd格式枚举</param>
    /// <returns>转换后的字节数组</returns>
    public static byte[] GetBytesFromBCD(string value, BcdFormatEnum format)
    {
        if (string.IsNullOrEmpty(value))
        {
            return Array.Empty<byte>();
        }

        int length = (value.Length + 1) / 2;
        byte[] bytesFromBcd = new byte[length];

        for (int index = 0; index < length; ++index)
        {
            byte highNibble = (byte)(GetBcdCodeFromChar(value[2 * index], format) << 4);
            byte lowNibble = 0;

            if ((2 * index) + 1 < value.Length)
            {
                lowNibble = GetBcdCodeFromChar(value[(2 * index) + 1], format);
            }

            bytesFromBcd[index] = (byte)(highNibble | lowNibble);
        }

        return bytesFromBcd;
    }


    /// <summary>
    /// 返回bit代表的数据
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static byte GetDataByBitIndex(int offset)
    {
        return offset switch
        {
            0 => 1,
            1 => 2,
            2 => 4,
            3 => 8,
            4 => 16,
            5 => 32,
            6 => 64,
            7 => 128,
            _ => 0,
        };
    }

    /// <summary>
    /// 16进制Char转int
    /// </summary>
    /// <param name="ch"></param>
    /// <returns></returns>
    public static int GetIntByHexChar(char ch)
    {
        return ch switch
        {
            '0' => 0,
            '1' => 1,
            '2' => 2,
            '3' => 3,
            '4' => 4,
            '5' => 5,
            '6' => 6,
            '7' => 7,
            '8' => 8,
            '9' => 9,
            'A' or 'a' => 10,
            'B' or 'b' => 11,
            'C' or 'c' => 12,
            'D' or 'd' => 13,
            'E' or 'e' => 14,
            'F' or 'f' => 15,
            _ => -1,
        };
    }

    /// <summary>
    /// 将十六进制字符串转换为对应的字节数组
    /// </summary>
    /// <param name="hex">输入的十六进制字符串</param>
    /// <returns>转换后的字节数组</returns>
    public static Memory<byte> HexStringToBytes(string hex)
    {
        if (string.IsNullOrEmpty(hex))
            return Memory<byte>.Empty;

        int len = hex.Length / 2;
        var result = new byte[len];
        int byteIndex = 0;

        for (int i = 0; i < hex.Length - 1; i += 2)
        {
            int hi = GetHexCharIndex(hex[i]);
            int lo = GetHexCharIndex(hex[i + 1]);

            if (hi >= 0 && lo >= 0 && hi < 0x10 && lo < 0x10)
            {
                result[byteIndex++] = (byte)((hi << 4) | lo);
            }
            else
            {
                i--;
                continue;
            }
        }

        return new Memory<byte>(result, 0, byteIndex);
    }


    /// <summary>
    /// 拼接任意个泛型数组为一个总的泛型数组对象。
    /// </summary>
    /// <typeparam name="T">数组的类型信息</typeparam>
    /// <param name="arrays">任意个长度的数组</param>
    /// <returns>拼接之后的最终的结果对象</returns>
    public static T[] SpliceArray<T>(params T[][] arrays)
    {
        if (arrays == null || arrays.Length == 0)
            return Array.Empty<T>();

        // 预先计算所有数组的总长度，避免多次扩容
        int totalLength = 0;
        foreach (var array in arrays)
        {
            if (array != null)
                totalLength += array.Length;
        }

        if (totalLength == 0)
            return Array.Empty<T>();

        // 分配目标数组
        T[] result = new T[totalLength];
        int offset = 0;

        // 拷贝所有数组到目标数组中
        foreach (var array in arrays)
        {
            if (array == null || array.Length == 0)
                continue;

            Array.Copy(array, 0, result, offset, array.Length);
            offset += array.Length;
        }

        return result;
    }


    /// <summary>
    /// 拼接任意个泛型数组为一个总的泛型数组对象。
    /// </summary>
    /// <typeparam name="T">数组的类型信息</typeparam>
    /// <param name="arrays">任意个长度的数组</param>
    /// <returns>拼接之后的最终的结果对象</returns>
    public static Memory<T> SpliceArray<T>(params Memory<T>[] arrays)
    {
        if (arrays == null || arrays.Length == 0)
            return Array.Empty<T>();

        // 预先计算所有数组的总长度，避免多次扩容
        int totalLength = 0;
        foreach (var array in arrays)
        {
            if (!array.IsEmpty)
                totalLength += array.Length;
        }

        if (totalLength == 0)
            return Array.Empty<T>();

        // 分配目标数组
        Memory<T> result = new T[totalLength];
        int offset = 0;

        // 拷贝所有数组到目标数组中
        foreach (var array in arrays)
        {
            if (array.IsEmpty)
                continue;

            array.CopyTo(result.Slice(offset, array.Length));
            offset += array.Length;
        }

        return result;
    }

    /// <summary>
    /// 将整数进行有效的拆分成数组，指定每个元素的最大值
    /// </summary>
    /// <param name="integer">整数信息</param>
    /// <param name="everyLength">单个的数组长度</param>
    /// <returns>拆分后的数组长度</returns>
    public static int[] SplitIntegerToArray(int integer, int everyLength)
    {
        int[] array = new int[(integer / everyLength) + (integer % everyLength == 0 ? 0 : 1)];
        for (int index = 0; index < array.Length; ++index)
            array[index] = index != array.Length - 1 ? everyLength : (integer % everyLength == 0 ? everyLength : integer % everyLength);
        return array;
    }

    private static byte GetHexCharIndex(char ch)
    {
        return ch switch
        {
            '0' => 0,
            '1' => 1,
            '2' => 2,
            '3' => 3,
            '4' => 4,
            '5' => 5,
            '6' => 6,
            '7' => 7,
            '8' => 8,
            '9' => 9,
            'A' or 'a' => 0x0a,
            'B' or 'b' => 0x0b,
            'C' or 'c' => 0x0c,
            'D' or 'd' => 0x0d,
            'E' or 'e' => 0x0e,
            'F' or 'f' => 0x0f,
            _ => 0x10,
        };
    }
}
