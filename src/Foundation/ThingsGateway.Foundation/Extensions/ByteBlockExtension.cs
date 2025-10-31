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
using System.Runtime.CompilerServices;
using System.Text;

namespace ThingsGateway.Foundation;



/// <summary>
/// 提供字节块扩展方法的静态类。
/// </summary>
public static class ByteBlockExtension
{


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

    #region ToString


    public static string ToString<TByteBlock>(this TByteBlock byteBlock, long offset, long length) where TByteBlock : IBytesReader
    {
        return byteBlock.TotalSequence.Slice(offset, length).ToString(Encoding.UTF8);
    }
    public static string ToString(this ReadOnlySequence<byte> byteBlock, Encoding encoding)
    {
# if NET6_0_OR_GREATER
        return encoding.GetString(byteBlock);
#else
        using ContiguousMemoryBuffer contiguousMemoryBuffer = new(byteBlock);
        return contiguousMemoryBuffer.Memory.Span.ToString(encoding);
#endif
    }

    /// <inheritdoc/>
    public static string ToString<TByteBlock>(this TByteBlock byteBlock, long offset) where TByteBlock : IBytesReader
    {
        return ToString(byteBlock, offset, byteBlock.BytesRead + byteBlock.BytesRemaining - offset);

    }
    /// <inheritdoc/>
    public static string ToString<TByteBlock>(this TByteBlock byteBlock) where TByteBlock : IBytesReader
    {
        return byteBlock.TotalSequence.ToString(Encoding.UTF8);
    }
    #endregion ToString

    #region ToHexString


    public static string ToHexString<TByteBlock>(this TByteBlock byteBlock, long offset, long length, char splite = default) where TByteBlock : IBytesReader
    {
        return byteBlock.TotalSequence.Slice(offset, length).ToHexString(Encoding.UTF8, splite);
    }
    public static string ToHexString(this ReadOnlySequence<byte> byteBlock, Encoding encoding, char splite = default)
    {
        return byteBlock.ToHexString(splite);
    }

    /// <inheritdoc/>
    public static string ToHexString<TByteBlock>(this TByteBlock byteBlock, long offset, char splite = default) where TByteBlock : IBytesReader
    {
        return ToHexString(byteBlock, offset, byteBlock.BytesRead + byteBlock.BytesRemaining - offset, splite);

    }
    /// <inheritdoc/>
    public static string ToHexString<TByteBlock>(this TByteBlock byteBlock, char splite = default) where TByteBlock : IBytesReader
    {
        return byteBlock.TotalSequence.ToHexString(Encoding.UTF8, splite);

    }

    #endregion ToHexString


    /// <summary>
    /// 在 <see cref="ReadOnlySequence{T}"/> 中查找第一个与指定 <see cref="byte"/> 匹配的子序列的起始索引。
    /// <para>如果未找到则返回 -1。</para>
    /// </summary>
    /// <param name="sequence">要搜索的字节序列。</param>
    /// <param name="firstByte">要查找的字节。</param>
    /// <returns>匹配子序列的起始索引，未找到则返回 -1。</returns>
    public static long IndexOf(this ReadOnlySequence<byte> sequence, byte firstByte)
    {

        if (sequence.Length < 1)
        {
            return -1;
        }

        long globalPosition = 0;
        var enumerator = sequence.GetEnumerator();

        // 遍历每个内存段
        while (enumerator.MoveNext())
        {
            var currentSpan = enumerator.Current.Span;
            var localIndex = 0;

            // 在当前段中搜索首字节
            while (localIndex < currentSpan.Length)
            {
                // 查找首字节匹配位置
                var matchIndex = currentSpan.Slice(localIndex).IndexOf(firstByte);
                if (matchIndex == -1)
                {
                    break;
                }

                localIndex += matchIndex;
                var globalIndex = globalPosition + localIndex;

                // 检查剩余长度是否足够
                if (sequence.Length - globalIndex < 1)
                {
                    return -1;
                }

                // 检查完整匹配
                if (IsMatch(sequence, globalIndex, firstByte))
                {
                    return globalIndex;
                }

                localIndex++; // 继续搜索下一个位置
            }
            globalPosition += currentSpan.Length;
        }
        return -1;
    }

    private static bool IsMatch(ReadOnlySequence<byte> sequence, long start, byte value)
    {
        return sequence.Slice(start, 1).First.Span[0] == value;
    }


    public static byte GetByte(this ReadOnlySequence<byte> sequence, long index)
    {
        return sequence.Slice(index).First.Span[0];
    }

    /// <summary>
    /// 计算指定区间字节的和
    /// </summary>
    public static long SumRange(this ReadOnlySequence<byte> sequence, long start, long count)
    {
        if (start < 0 || count < 0 || start + count > sequence.Length)
            throw new ArgumentOutOfRangeException();

        long sum = 0;
        long remaining = count;

        foreach (var segment in sequence)
        {
            if (start >= segment.Length)
            {
                // 起点不在当前段
                start -= segment.Length;
                continue;
            }

            var span = segment.Span;
            int take = (int)Math.Min(segment.Length - start, remaining);

            for (int i = 0; i < take; i++)
                sum += span[(int)start + i];

            remaining -= take;
            start = 0; // 后续段从头开始

            if (remaining == 0)
                break;
        }

        return sum;
    }

    /// <summary>
    /// 比较 ReadOnlySequence 与 ReadOnlySpan 的内容是否一致。
    /// </summary>
    public static bool SequenceEqual<T>(this ReadOnlySequence<T> sequence, ReadOnlySpan<T> other)
        where T : IEquatable<T>
    {
        if (sequence.Length != other.Length)
            return false;

        // 单段，直接比较
        if (sequence.IsSingleSegment)
        {
            return sequence.First.Span.SequenceEqual(other);
        }

        // 多段，逐段比较
        int offset = 0;
        foreach (var segment in sequence)
        {
            var span = segment.Span;
            if (!other.Slice(offset, span.Length).SequenceEqual(span))
                return false;
            offset += span.Length;
        }

        return true;
    }


    /// <summary>
    /// 将指定的字节块从当前位置<see cref="IByteBlockCore.Position"/>转换为【新】字节数组，指定长度。
    /// </summary>
    /// <typeparam name="TByteBlock">实现<see cref="IByteBlock"/>接口的字节块类型。</typeparam>
    /// <param name="byteBlock">字节块对象。</param>
    /// <param name="length">要转换为数组的长度。</param>
    /// <returns>从当前位置开始，指定长度的【新】字节数组。</returns>
    public static byte[] ToArrayTake<TByteBlock>(this TByteBlock byteBlock, long length) where TByteBlock : IBytesReader
    {
        return byteBlock.Sequence.Slice(0, length).ToArray();
    }
    public static void Write<TByteBlock>(ref TByteBlock byteBlock, ReadOnlySequence<byte> bytes) where TByteBlock : IBytesWriter
    {
        foreach (var item in bytes)
        {
            byteBlock.Write(item.Span);
        }
    }
    public static void WriteBackValue<TWriter, T>(ref TWriter writer, T value, EndianType endianType, int pos)
    where T : unmanaged
    where TWriter : IByteBlockWriter
    {
        var nowPos = (int)writer.WrittenCount - pos;
        writer.Advance(-nowPos);
        var size = Unsafe.SizeOf<T>();
        var span = writer.GetSpan(size);
        TouchSocketBitConverter.GetBitConverter(endianType).WriteBytes(span, value);
        writer.Advance(nowPos);

    }
    public static void WriteBackValue<TWriter, T>(ref TWriter writer, T value, EndianType endianType, long pos)
where T : unmanaged
where TWriter : IByteBlockWriter
    {
        var nowPos = (int)(writer.WrittenCount - pos);
        writer.Advance(-nowPos);
        var size = Unsafe.SizeOf<T>();
        var span = writer.GetSpan(size);
        TouchSocketBitConverter.GetBitConverter(endianType).WriteBytes(span, value);
        writer.Advance(nowPos);

    }

    public static string ReadNormalString<TReader>(ref TReader reader, int length)
    where TReader : IBytesReader
    {
        var span = reader.GetSpan(length).Slice(0, length);
        var str = span.ToString(Encoding.UTF8);
        reader.Advance(length);
        return str;
    }

    public static void WriteBackNormalString<TWriter>(ref TWriter writer, string value, Encoding encoding, int pos)
where TWriter : IByteBlockWriter
    {
        var nowPos = (int)(writer.WrittenCount - pos);
        writer.Advance(-nowPos);
        WriterExtension.WriteNormalString(ref writer, value, encoding);
        writer.Advance(nowPos);

    }


    public static int WriteNormalString(this Span<byte> span, string value, Encoding encoding=null)
    {
        encoding ??= Encoding.UTF8;
        var maxSize = encoding.GetMaxByteCount(value.Length);
        var chars = value.AsSpan();

        unsafe
        {
            fixed (char* p = &chars[0])
            {
                fixed (byte* p1 = &span[0])
                {
                    var len = encoding.GetBytes(p, chars.Length, p1, maxSize);
                    return len;
                }
            }
        }

    }
}