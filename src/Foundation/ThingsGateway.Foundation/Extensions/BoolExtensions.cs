//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation;

/// <summary>
/// bool扩展
/// </summary>
public static class BoolExtensions
{
    /// <summary>
    /// 将布尔数组转换为压缩的字节数组（每 8 位布尔值压缩为 1 个字节，低位在前）。
    /// </summary>
    /// <param name="array">布尔数组</param>
    /// <returns>压缩后的只读字节内存</returns>
    public static byte[] BoolArrayToByte(this ReadOnlySpan<bool> array)
    {
        if (array.IsEmpty)
            return Array.Empty<byte>();

        int byteLength = (array.Length + 7) / 8;
        byte[] result = new byte[byteLength];

        for (int i = 0; i < array.Length; i++)
        {
            if (array[i])
            {
                result[i / 8] |= (byte)(1 << (i % 8));
            }
        }

        return result;
    }

    /// <summary>
    /// 将布尔数组转换为压缩的字节数组（每 8 位布尔值压缩为 1 个字节，低位在前）。
    /// </summary>
    public static byte[] BoolArrayToByte(this Span<bool> array)
        => ((ReadOnlySpan<bool>)array).BoolArrayToByte();
}
