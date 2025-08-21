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

namespace ThingsGateway.Foundation.Dlt645;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class Dlt645_2007Response : Dlt645_2007Request
{
    /// <summary>
    /// 错误码
    /// </summary>
    public byte? ErrorCode { get; set; }
}

/// <summary>
/// <inheritdoc/>
/// </summary>
public class Dlt645_2007Message : MessageBase, IResultMessage
{
    private static readonly byte[] ReadStation = [0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA];

    private long HeadCodeIndex;

    public Dlt645_2007Send? Dlt645_2007Send { get; set; }

    /// <inheritdoc/>
    public override long HeaderLength { get; set; } = 10;

    public Dlt645_2007Address? Request { get; set; }

    /// <inheritdoc/>
    public override bool CheckHead<TByteBlock>(ref TByteBlock byteBlock)
    {
        //因为设备可能带有FE前导符开头，这里找到0x68的位置
        if (byteBlock != null)
        {
            var index = byteBlock.TotalSequence.IndexOf(0x68);
            if (index > -1)
            {
                HeadCodeIndex = index;
            }
        }

        //帧起始符 地址域  帧起始符 控制码 数据域长度共10个字节
        HeaderLength = HeadCodeIndex - byteBlock.BytesRead + 10;
        if (byteBlock.BytesRead + byteBlock.BytesRemaining > HeadCodeIndex + 9)
        {
            BodyLength = byteBlock.TotalSequence.GetByte(HeadCodeIndex + 9) + 2;
        }
        return true;

    }

    public override FilterResult CheckBody<TByteBlock>(ref TByteBlock byteBlock)
    {
        var sequence = byteBlock.TotalSequence;
        var pos = byteBlock.BytesRead - HeaderLength;
        var endIndex = HeaderLength + BodyLength + pos;

        if (sequence.GetByte(endIndex - 1) == 0x16)
        {
            //检查校验码
            var sumCheck = sequence.SumRange(HeadCodeIndex, endIndex - HeadCodeIndex - 2);
            if ((byte)sumCheck != sequence.GetByte(endIndex - 2))
            {
                //校验错误
                ErrorMessage = AppResource.SumError;
                OperCode = 999;
                return FilterResult.Success;
            }
            byte? ErrorCode;
            var controlCode = sequence.GetByte(HeadCodeIndex + 8);
            if ((controlCode & 0x40) == 0x40)//控制码bit6为1时，返回错误
            {
                ErrorCode = (byte)(sequence.GetByte(HeadCodeIndex + 10) - 0x33);
                var error = Dlt645Helper.Get2007ErrorMessage(ErrorCode.Value);
                ErrorMessage = string.Format(AppResource.FunctionError, $"0x{controlCode:X2}", error);
                OperCode = 999;
                return FilterResult.Success;
            }

            if (Dlt645_2007Send != null)
            {
                var Station = sequence.Slice(HeadCodeIndex + 1, 6);
                if (!Station.SequenceEqual(Request.Station.Span))//设备地址不符合时，返回错误
                {
                    if (!Request.Station.Span.SequenceEqual(ReadStation))//读写通讯地址例外
                    {
                        ErrorMessage = AppResource.StationNotSame;
                        OperCode = 999;
                        return FilterResult.Success;
                    }
                }
                if (controlCode != ((byte)Dlt645_2007Send.ControlCode) + 0x80)//控制码不符合时，返回错误
                {
                    ErrorMessage =
                         string.Format(AppResource.FunctionNotSame, $"0x{controlCode:X2}", $"0x{(byte)Dlt645_2007Send.ControlCode:X2}");
                    OperCode = 999;
                    return FilterResult.Success;
                }
                if (Dlt645_2007Send.ControlCode == ControlCode.Read || Dlt645_2007Send.ControlCode == ControlCode.Write)
                {
                    //数据标识不符合时，返回错误
                    var DataId = sequence.Slice(HeadCodeIndex + 10, 4).BytesAdd(-0x33).AsSpan();
                    if (!DataId.SequenceEqual(Request.DataId.Span))
                    {
                        ErrorMessage = AppResource.DataIdNotSame;
                        OperCode = 999;
                        return FilterResult.Success;
                    }
                }
            }

            OperCode = 0;
            Content = byteBlock.TotalSequence.Slice(HeadCodeIndex + 10, BodyLength - 2).ToArray();
            return FilterResult.Success;
        }

        return FilterResult.GoOn;
    }


    public override void SendInfo(ISendMessage sendMessage)
    {
        Dlt645_2007Send = ((Dlt645_2007Send)sendMessage);
        Request = Dlt645_2007Send.Dlt645_2007Address;
    }
}
