//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Buffers;

namespace ThingsGateway.Foundation;

/// <summary>
/// CRC16验证
/// </summary>
public static class CRC16Utils
{
    /// <summary>
    /// 通过指定多项式码来获取对应的数据的CRC校验码
    /// </summary>
    /// <param name="bytes">需要校验的数据，不包含CRC字节</param>
    /// <returns>返回带CRC校验码的字节数组，可用于串口发送</returns>
    public static byte[] Crc16Only(ReadOnlySpan<byte> bytes)
    {
        return Crc16Only(bytes, 0xA001);
    }

    /// <summary>
    /// 通过指定多项式码来获取对应的数据的CRC校验码
    /// </summary>
    /// <param name="bytes">需要校验的数据，不包含CRC字节</param>
    /// <param name="xdapoly">多项式</param>
    /// <param name="crc16">crc16</param>
    /// <returns>返回带CRC校验码的字节数组，可用于串口发送</returns>
    public static byte[] Crc16Only(ReadOnlySpan<byte> bytes, int xdapoly, bool crc16 = true)
    {
        int length = bytes.Length;
        int num = 0xFFFF;

        for (int i = 0; i < length; i++)
        {
            num = (num >> (crc16 ? 0 : 8)) ^ bytes[i];

            for (int j = 0; j < 8; j++)
            {
                int num2 = num & 1;
                num >>= 1;
                if (num2 == 1)
                {
                    num ^= xdapoly;
                }
            }
        }

        if (crc16)
        {
            return new byte[]
            {
            (byte)(num & 0xFFu),
            (byte)(num >> 8)
            };
        }
        else
        {
            return new byte[]
            {
            (byte)(num >> 8),
            (byte)(num & 0xFFu)
            };
        }
    }



    /// <summary>
    /// 通过指定多项式码来获取对应的数据的CRC校验码
    /// </summary>
    /// <param name="sequence">需要校验的数据，不包含CRC字节</param>
    /// <returns>返回带CRC校验码的字节数组，可用于串口发送</returns>
    public static byte[] Crc16Only(ReadOnlySequence<byte> sequence)
    {
        return Crc16Only(sequence, 0xA001);
    }

    /// <summary>
    /// 通过指定多项式码来获取对应的数据的CRC校验码
    /// </summary>
    /// <param name="sequence">需要校验的数据，不包含CRC字节</param>
    /// <param name="xdapoly">多项式</param>
    /// <param name="crc16">是否低位在前（true=低字节优先，false=高字节优先）</param>
    /// <returns>返回带CRC校验码的字节数组，可用于串口发送</returns>
    public static byte[] Crc16Only(ReadOnlySequence<byte> sequence, int xdapoly, bool crc16 = true)
    {
        int num = 0xFFFF;

        foreach (var segment in sequence)
        {
            var span = segment.Span;
            for (int i = 0; i < span.Length; i++)
            {
                num = (num >> (crc16 ? 0 : 8)) ^ span[i];

                for (int j = 0; j < 8; j++)
                {
                    int num2 = num & 1;
                    num >>= 1;
                    if (num2 == 1)
                    {
                        num ^= xdapoly;
                    }
                }
            }
        }

        if (crc16)
        {
            return new byte[]
            {
                (byte)(num & 0xFFu),
                (byte)(num >> 8)
            };
        }
        else
        {
            return new byte[]
            {
                (byte)(num >> 8),
                (byte)(num & 0xFFu)
            };
        }
    }

}
