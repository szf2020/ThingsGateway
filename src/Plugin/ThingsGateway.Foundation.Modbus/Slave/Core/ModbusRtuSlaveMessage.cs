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

namespace ThingsGateway.Foundation.Modbus;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class ModbusRtuSlaveMessage : MessageBase, IResultMessage
{
    /// <summary>
    /// 当前关联的字节数组
    /// </summary>
    public ReadOnlySequence<byte> Sequences { get; set; }

    /// <inheritdoc/>
    public override long HeaderLength => 7;

    public ModbusRequest Request { get; set; } = new();

    /// <inheritdoc/>
    public override bool CheckHead<TByteBlock>(ref TByteBlock byteBlock)
    {
        Request.Station = ReaderExtension.ReadValue<TByteBlock, byte>(ref byteBlock);
        Request.FunctionCode = ReaderExtension.ReadValue<TByteBlock, byte>(ref byteBlock);
        var f = Request.FunctionCode > 0x30 ? Request.FunctionCode - 0x30 : Request.FunctionCode;
        Request.StartAddress = ReaderExtension.ReadValue<TByteBlock, ushort>(ref byteBlock, EndianType.Big);
        if (f == 3 || f == 4)
        {
            Request.Length = (ushort)(ReaderExtension.ReadValue<TByteBlock, ushort>(ref byteBlock, EndianType.Big) * 2);
            BodyLength = 1;
            return true;
        }
        else if (f == 1 || f == 2)
        {
            Request.Length = ReaderExtension.ReadValue<TByteBlock, ushort>(ref byteBlock, EndianType.Big);
            BodyLength = 1;
            return true;
        }
        else if (f == 5)
        {
            Request.SlaveWriteDatas = byteBlock.Sequence.Slice(0, 1);
            BodyLength = 1;
            return true;
        }
        else if (f == 6)
        {
            Request.SlaveWriteDatas = byteBlock.Sequence.Slice(0, 2);
            BodyLength = 1;
            return true;
        }
        else if (f == 15)
        {
            Request.Length = ReaderExtension.ReadValue<TByteBlock, ushort>(ref byteBlock, EndianType.Big);
            BodyLength = Request.Length + 2;
            return true;
        }
        else if (f == 16)
        {
            Request.Length = (ushort)(ReaderExtension.ReadValue<TByteBlock, ushort>(ref byteBlock, EndianType.Big) * 2);
            BodyLength = Request.Length + 2;
            return true;
        }
        return false;
    }

    public override FilterResult CheckBody<TByteBlock>(ref TByteBlock byteBlock)
    {
        var pos = byteBlock.BytesRead - HeaderLength;
        long crcLen = 0;
        Sequences = byteBlock.TotalSequence.Slice(pos, HeaderLength + BodyLength);

        var f = Request.FunctionCode > 0x30 ? Request.FunctionCode - 0x30 : Request.FunctionCode;
        if (f == 15)
        {
            Request.SlaveWriteDatas = new ReadOnlySequence<byte>(byteBlock.TotalSequence.Slice(byteBlock.BytesRead, Request.Length).ByteToBoolArray(Request.Length).BoolToByte());
        }
        else if (f == 16)
        {

            Request.SlaveWriteDatas = byteBlock.TotalSequence.Slice(byteBlock.BytesRead, Request.Length);
        }

        crcLen = HeaderLength + BodyLength - 2;

        var crc = CRC16Utils.Crc16Only(byteBlock.TotalSequence.Slice(pos, crcLen));

        //Crc
        var checkCrc = byteBlock.TotalSequence.Slice(pos + crcLen, 2);
        if (checkCrc.SequenceEqual(crc))
        {
            OperCode = 0;
            return FilterResult.Success;
        }

        return FilterResult.GoOn;
    }
}
