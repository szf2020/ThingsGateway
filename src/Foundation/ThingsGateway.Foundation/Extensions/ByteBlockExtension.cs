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
/// 提供字节块扩展方法的静态类。
/// </summary>
public static class ByteBlockExtension
{
    public static void WriteBackAddValue<TWriter>(ref TWriter writer, byte value, int pos)
        where TWriter : IByteBlockWriter
    {
        int nowPos = writer.Position;
        writer.Position = pos;
        WriterExtension.WriteValue(ref writer, (byte)(writer.Span[pos] + value));
        writer.Position = nowPos;
    }


    public static void WriteBackValue<TWriter, T>(ref TWriter writer, T value, int pos)
        where T : unmanaged
        where TWriter : IByteBlockWriter
    {
        int nowPos = writer.Position;
        writer.Position = pos;
        WriterExtension.WriteValue(ref writer, value);
        writer.Position = nowPos;
    }

    public static void WriteBackValue<TWriter, T>(ref TWriter writer, T value, EndianType endianType, int pos)
        where T : unmanaged
        where TWriter : IByteBlockWriter
    {
        int nowPos = writer.Position;
        writer.Position = pos;
        WriterExtension.WriteValue(ref writer, value, endianType);
        writer.Position = nowPos;
    }

    public static void WriteBackNormalString<TWriter>(ref TWriter writer, string value, Encoding encoding, int pos)
    where TWriter : IByteBlockWriter
    {

        int nowPos = writer.Position;
        writer.Position = pos;
        WriterExtension.WriteNormalString(ref writer, value, encoding);
        writer.Position = nowPos;
    }


    public static string ReadNormalString<TReader>(ref TReader reader, int length)
        where TReader : IBytesReader
    {
        var span = reader.GetSpan(length).Slice(0, length);
        var str = span.ToString(Encoding.UTF8);
        reader.Advance(length);
        return str;
    }


    /// <summary>
    /// 将值类型的字节块转换为普通的字节块。
    /// </summary>
    /// <param name="valueByteBlock">要转换的值类型字节块。</param>
    /// <returns>一个新的字节块对象。</returns>
    public static ByteBlock AsByteBlock(this ValueByteBlock valueByteBlock)
    {
        ByteBlock byteBlock = new ByteBlock(valueByteBlock.TotalMemory.Slice(0, valueByteBlock.Length));
        byteBlock.Position = valueByteBlock.Position;
        byteBlock.SetLength(valueByteBlock.Length);
        return byteBlock;
    }

    #region ToArray

    /// <summary>
    /// 将指定的字节块转换为【新】字节数组。
    /// </summary>
    /// <typeparam name="TByteBlock">实现<see cref="IByteBlock"/>接口的字节块类型。</typeparam>
    /// <param name="byteBlock">字节块对象。</param>
    /// <param name="offset">起始偏移量。</param>
    /// <param name="length">要转换为数组的长度。</param>
    /// <returns>包含指定长度的【新】字节数组。</returns>
    public static byte[] ToArray<TByteBlock>(this TByteBlock byteBlock, int offset, int length) where TByteBlock : IByteBlockCore
    {
        return byteBlock.Span.Slice(offset, length).ToArray();
    }

    /// <summary>
    /// 将指定的字节块转换为【新】字节数组，从指定偏移量开始，直到字节块的末尾。
    /// </summary>
    /// <typeparam name="TByteBlock">实现<see cref="IByteBlock"/>接口的字节块类型。</typeparam>
    /// <param name="byteBlock">字节块对象。</param>
    /// <param name="offset">起始偏移量。</param>
    /// <returns>从指定偏移量到字节块末尾的【新】字节数组。</returns>
    public static byte[] ToArray<TByteBlock>(this TByteBlock byteBlock, int offset) where TByteBlock : IByteBlockCore
    {
        return ToArray(byteBlock, offset, byteBlock.Length - offset);
    }

    /// <summary>
    /// 将指定的字节块转换为【新】字节数组，从索引0开始，直到字节块的末尾。
    /// </summary>
    /// <typeparam name="TByteBlock">实现<see cref="IByteBlock"/>接口的字节块类型。</typeparam>
    /// <param name="byteBlock">字节块对象。</param>
    /// <returns>整个字节块的【新】字节数组。</returns>
    public static byte[] ToArray<TByteBlock>(this TByteBlock byteBlock) where TByteBlock : IByteBlockCore
    {
        return ToArray(byteBlock, 0, byteBlock.Length);
    }

    /// <summary>
    /// 将指定的字节块从当前位置<see cref="IByteBlockCore.Position"/>转换为【新】字节数组，直到字节块的末尾。
    /// </summary>
    /// <typeparam name="TByteBlock">实现<see cref="IByteBlock"/>接口的字节块类型。</typeparam>
    /// <param name="byteBlock">字节块对象。</param>
    /// <returns>从当前位置到字节块末尾的【新】字节数组。</returns>
    public static byte[] ToArrayTake<TByteBlock>(this TByteBlock byteBlock) where TByteBlock : IByteBlockReader
    {
        return ToArray(byteBlock, byteBlock.Position, byteBlock.CanReadLength);
    }

    /// <summary>
    /// 将指定的字节块从当前位置<see cref="IByteBlockCore.Position"/>转换为【新】字节数组，指定长度。
    /// </summary>
    /// <typeparam name="TByteBlock">实现<see cref="IByteBlock"/>接口的字节块类型。</typeparam>
    /// <param name="byteBlock">字节块对象。</param>
    /// <param name="length">要转换为数组的长度。</param>
    /// <returns>从当前位置开始，指定长度的【新】字节数组。</returns>
    public static byte[] ToArrayTake<TByteBlock>(this TByteBlock byteBlock, int length) where TByteBlock : IByteBlockReader
    {
        return ToArray(byteBlock, byteBlock.Position, length);
    }

    #endregion ToArray

    #region AsSegment

    /// <summary>
    /// 将字节块【作为】数组段。
    /// <para>
    /// 【作为】的意思是，导出的数据内存实际上依旧是<see cref="IByteBlockReader"/>生命周期内的，不能脱离生命周期使用。
    /// </para>
    /// </summary>
    /// <typeparam name="TByteBlock">实现<see cref="IByteBlockReader"/>接口的字节块类型。</typeparam>
    /// <param name="byteBlock">要转换的字节块实例。</param>
    /// <param name="offset">数组段的起始偏移量。</param>
    /// <param name="length">数组段的长度。</param>
    /// <returns>一个包含指定偏移量和长度的数组段。</returns>
    public static ArraySegment<byte> AsSegment<TByteBlock>(this TByteBlock byteBlock, int offset, int length) where TByteBlock : IByteBlockReader
    {
        return byteBlock.Memory.Slice(offset, length).GetArray();
    }

    /// <summary>
    /// 将字节块【作为】数组段，从指定偏移量开始，长度为可读长度。
    /// <para>
    /// 【作为】的意思是，导出的数据内存实际上依旧是<see cref="IByteBlockReader"/>生命周期内的，不能脱离生命周期使用。
    /// </para>
    /// </summary>
    /// <typeparam name="TByteBlock">实现<see cref="IByteBlockReader"/>接口的字节块类型。</typeparam>
    /// <param name="byteBlock">要转换的字节块实例。</param>
    /// <param name="offset">数组段的起始偏移量。</param>
    /// <returns>一个从指定偏移量开始，长度为可读长度的数组段。</returns>
    public static ArraySegment<byte> AsSegment<TByteBlock>(this TByteBlock byteBlock, int offset) where TByteBlock : IByteBlockReader
    {
        return AsSegment(byteBlock, offset, byteBlock.Length - offset);
    }

    /// <summary>
    /// 将字节块【作为】数组段，从头开始，长度为指定长度。
    /// <para>
    /// 【作为】的意思是，导出的数据内存实际上依旧是<see cref="IByteBlockReader"/>生命周期内的，不能脱离生命周期使用。
    /// </para>
    /// </summary>
    /// <typeparam name="TByteBlock">实现<see cref="IByteBlockReader"/>接口的字节块类型。</typeparam>
    /// <param name="byteBlock">要转换的字节块实例。</param>
    /// <returns>一个从头开始，长度为字节块长度的数组段。</returns>
    public static ArraySegment<byte> AsSegment<TByteBlock>(this TByteBlock byteBlock) where TByteBlock : IByteBlockReader
    {
        return AsSegment(byteBlock, 0, byteBlock.Length);
    }

    /// <summary>
    /// 将字节块【作为】数组段，从当前位置开始，指定长度。
    /// <para>
    /// 【作为】的意思是，导出的数据内存实际上依旧是<see cref="IByteBlockReader"/>生命周期内的，不能脱离生命周期使用。
    /// </para>
    /// </summary>
    /// <typeparam name="TByteBlock">实现<see cref="IByteBlockReader"/>接口的字节块类型。</typeparam>
    /// <param name="byteBlock">要转换的字节块实例。</param>
    /// <param name="length">数组段的长度。</param>
    /// <returns>一个从当前位置开始，指定长度的数组段。</returns>
    public static ArraySegment<byte> AsSegmentTake<TByteBlock>(this TByteBlock byteBlock, int length) where TByteBlock : IByteBlockReader
    {
        return AsSegment(byteBlock, byteBlock.Position, length);
    }

    /// <summary>
    /// 将字节块【作为】数组段，从当前位置开始，长度为可读长度。
    /// <para>
    /// 【作为】的意思是，导出的数据内存实际上依旧是<see cref="IByteBlockReader"/>生命周期内的，不能脱离生命周期使用。
    /// </para>
    /// </summary>
    /// <typeparam name="TByteBlock">实现<see cref="IByteBlockReader"/>接口的字节块类型。</typeparam>
    /// <param name="byteBlock">要转换的字节块实例。</param>
    /// <returns>一个从当前位置开始，长度为可读长度的数组段。</returns>
    public static ArraySegment<byte> AsSegmentTake<TByteBlock>(this TByteBlock byteBlock) where TByteBlock : IByteBlockReader
    {
        return AsSegment(byteBlock, byteBlock.Position, byteBlock.CanReadLength);
    }
    /// <inheritdoc/>
    public static string ToString<TByteBlock>(this TByteBlock byteBlock, int offset, int length) where TByteBlock : IByteBlockReader
    {
        return byteBlock.Span.Slice(offset, length).ToString(Encoding.UTF8);
    }

    /// <inheritdoc/>
    public static string ToString<TByteBlock>(this TByteBlock byteBlock, int offset) where TByteBlock : IByteBlockReader
    {
        return byteBlock.Span.Slice(offset, byteBlock.Length - offset).ToString(Encoding.UTF8);

    }
    #endregion AsSegment


}