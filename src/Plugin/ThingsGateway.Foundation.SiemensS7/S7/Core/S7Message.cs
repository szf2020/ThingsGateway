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

using TouchSocket.Core;

namespace ThingsGateway.Foundation.SiemensS7;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class S7Message : MessageBase, IResultMessage
{
    /// <summary>
    /// 错误码
    /// </summary>
    public byte? Error { get; set; }

    /// <inheritdoc/>
    public override long HeaderLength => 4;

    public override bool CheckHead<TByteBlock>(ref TByteBlock byteBlock)
    {
        byteBlock.BytesRead += 2;
        BodyLength = ReaderExtension.ReadValue<TByteBlock, ushort>(ref byteBlock, EndianType.Big) - 4;
        return true;
    }
    public override FilterResult CheckBody<TByteBlock>(ref TByteBlock byteBlock)
    {
        var pos = byteBlock.BytesRead;
        var sequence = byteBlock.TotalSequence;
        if (sequence.GetByte(pos + 1) == 0xD0) // 首次握手0XD0连接确认
        {
            OperCode = 0;
            return FilterResult.Success;
        }
        else if (sequence.GetByte(pos + 15) == 0xF0) // PDU
        {
            // 其余情况判断错误代码
            if (sequence.GetByte(pos + 13) + sequence.GetByte(pos + 14) > 0) // 如果错误代码不为0
            {
                OperCode = 999;
                ErrorMessage = string.Format(AppResource.ReturnError, sequence.GetByte(pos + 13).ToString("X2"), sequence.GetByte(pos + 14).ToString("X2"));
                return FilterResult.Success;
            }
            else
            {
                Content = byteBlock.TotalSequence.Slice(byteBlock.BytesRead + byteBlock.BytesRemaining - 2, 2).ToArray();
                OperCode = 0;
                return FilterResult.Success;
            }
        }

        //分bit/byte解析
        else if (sequence.GetByte(pos + 15) == 0x04) // Read
        {
            byteBlock.BytesRead = pos + 7;
            var sign = ReaderExtension.ReadValue<TByteBlock, ushort>(ref byteBlock, EndianType.Big);//数据ID标识
            Sign = sign;
            byteBlock.BytesRead = pos;

            int length = sequence.GetByte(pos + 17);
            int itemLen = sequence.GetByte(pos + 16);

            //添加错误代码校验
            // 其余情况判断错误代码
            if (sequence.GetByte(pos + 13) + sequence.GetByte(pos + 14) > 0) // 如果错误代码不为0
            {
                OperCode = 999;
                ErrorMessage = string.Format(AppResource.ReturnError, sequence.GetByte(pos + 13).ToString("X2"), sequence.GetByte(pos + 14).ToString("X2"));
                return FilterResult.Success;
            }
            else
            {
                if (byteBlock.BytesRead + byteBlock.BytesRemaining < pos + 18)
                {
                    OperCode = 999;
                    ErrorMessage = AppResource.DataLengthError;
                    return FilterResult.Success;
                }
                if (sequence.GetByte(pos + 17) != byte.MaxValue)
                {
                    OperCode = 999;
                    ErrorMessage = string.Format(AppResource.ValidateDataError, sequence.GetByte(pos + 17), SiemensHelper.GetCpuError(sequence.GetByte(pos + 17)));
                    return FilterResult.Success;
                }

                ValueByteBlock data = new(length);
                var dataIndex = pos + 17;
                for (int index = 0; index < itemLen; index++)
                {
                    if (sequence.GetByte(dataIndex) != byte.MaxValue)
                    {
                        OperCode = 999;
                        ErrorMessage = string.Format(AppResource.ValidateDataError, sequence.GetByte(dataIndex), SiemensHelper.GetCpuError(sequence.GetByte(dataIndex)));
                        return FilterResult.Success;
                    }

                    if (sequence.GetByte(dataIndex + 1) == 4)//Bit:3;Byte:4;Counter或者Timer:9
                    {
                        byteBlock.BytesRead = dataIndex + 2;
                        var byteLength = ReaderExtension.ReadValue<TByteBlock, ushort>(ref byteBlock, EndianType.Big) / 8;
                        ByteBlockExtension.Write(ref data, sequence.Slice(dataIndex + 4, byteLength));
                        dataIndex += byteLength + 4;
                    }
                    else if (sequence.GetByte(dataIndex + 1) == 9)//Counter或者Timer:9
                    {
                        byteBlock.BytesRead = dataIndex + 2;
                        var byteLength = ReaderExtension.ReadValue<TByteBlock, ushort>(ref byteBlock, EndianType.Big);
                        if (byteLength % 3 == 0)
                        {
                            for (int indexCT = 0; indexCT < byteLength / 3; indexCT++)
                            {
                                var readOnlyMemories = byteBlock.TotalSequence.Slice(dataIndex + 5 + (3 * indexCT), 2);
                                foreach (var item in readOnlyMemories)
                                {
                                    data.Write(item.Span);
                                }
                            }
                        }
                        else
                        {
                            for (int indexCT = 0; indexCT < byteLength / 5; indexCT++)
                            {
                                var readOnlyMemories = byteBlock.TotalSequence.Slice(dataIndex + 7 + (5 * indexCT), 2);
                                foreach (var item in readOnlyMemories)
                                {
                                    data.Write(item.Span);
                                }
                            }
                        }
                        dataIndex += byteLength + 4;
                    }
                }

                OperCode = 0;
                Content = data.ToArray();
                data.SafeDispose();
                return FilterResult.Success;
            }
        }
        else if (sequence.GetByte(pos + 15) == 0x05) // Write
        {
            byteBlock.BytesRead = pos + 7;
            var sign = ReaderExtension.ReadValue<TByteBlock, ushort>(ref byteBlock, EndianType.Big);//数据ID标识
            Sign = sign;
            byteBlock.BytesRead = pos;
            int itemLen = sequence.GetByte(pos + 16);
            if (sequence.GetByte(pos + 13) + sequence.GetByte(pos + 14) > 0) // 如果错误代码不为0
            {
                OperCode = 999;
                ErrorMessage = string.Format(AppResource.ReturnError, sequence.GetByte(pos + 13).ToString("X2"), sequence.GetByte(pos + 14).ToString("X2"));
                return FilterResult.Success;
            }
            if (byteBlock.BytesRead + byteBlock.BytesRemaining < pos + 18)
            {
                OperCode = 999;
                ErrorMessage = AppResource.DataLengthError;
                return FilterResult.Success;
            }
            for (int i = 0; i < itemLen; i++)
            {
                if (sequence.GetByte(pos + 17 + i) != byte.MaxValue)
                {
                    OperCode = 999;
                    ErrorMessage = string.Format(AppResource.ValidateDataError, sequence.GetByte(pos + 17 + i), SiemensHelper.GetCpuError(sequence.GetByte(pos + 17 + i)));
                    return FilterResult.Success;
                }
            }

            {
                OperCode = 0;
                return FilterResult.Success;
            }
        }

        OperCode = 999;
        ErrorMessage = "Unsupport function code";
        return FilterResult.Success;
    }
}
