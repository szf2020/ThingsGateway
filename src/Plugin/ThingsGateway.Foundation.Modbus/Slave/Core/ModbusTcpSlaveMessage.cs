//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.Modbus;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class ModbusTcpSlaveMessage : MessageBase, IResultMessage
{
    /// <summary>
    /// 当前关联的字节数组
    /// </summary>
    public ReadOnlyMemory<byte> Bytes { get; set; }

    /// <inheritdoc/>
    public override int HeaderLength => 12;

    public ModbusRequest Request { get; set; } = new();

    public override bool CheckHead<TByteBlock>(ref TByteBlock byteBlock)
    {
        Sign = ReaderExtension.ReadValue<TByteBlock, ushort>(ref byteBlock, EndianType.Big);
        byteBlock.Position += 2;
        BodyLength = ReaderExtension.ReadValue<TByteBlock, ushort>(ref byteBlock, EndianType.Big) - 6;
        Request.Station = ReaderExtension.ReadValue<TByteBlock, byte>(ref byteBlock);
        Request.FunctionCode = ReaderExtension.ReadValue<TByteBlock, byte>(ref byteBlock);
        var f = Request.FunctionCode > 0x30 ? Request.FunctionCode - 0x30 : Request.FunctionCode;

        Request.StartAddress = ReaderExtension.ReadValue<TByteBlock, ushort>(ref byteBlock, EndianType.Big);

        if (f == 3 || f == 4)
        {
            Request.Length = (ushort)(ReaderExtension.ReadValue<TByteBlock, ushort>(ref byteBlock, EndianType.Big) * 2);
            return true;
        }
        else if (f == 1 || f == 2)
        {
            Request.Length = ReaderExtension.ReadValue<TByteBlock, ushort>(ref byteBlock, EndianType.Big);
            return true;
        }
        else if (f == 5)
        {
            Request.Data = byteBlock.Memory.Slice(byteBlock.Position, 1);
            return true;
        }
        else if (f == 6)
        {
            Request.Data = byteBlock.Memory.Slice(byteBlock.Position, 2);
            return true;
        }
        else if (f == 15)
        {
            Request.Length = ReaderExtension.ReadValue<TByteBlock, ushort>(ref byteBlock, EndianType.Big);
            return true;
        }
        else if (f == 16)
        {
            Request.Length = (ushort)(ReaderExtension.ReadValue<TByteBlock, ushort>(ref byteBlock, EndianType.Big) * 2);
            return true;
        }
        return false;
    }

    public override FilterResult CheckBody<TByteBlock>(ref TByteBlock byteBlock)
    {
        var pos = byteBlock.Position - HeaderLength;
        Bytes = byteBlock.Memory.Slice(pos, HeaderLength + BodyLength);

        var f = Request.FunctionCode > 0x30 ? Request.FunctionCode - 0x30 : Request.FunctionCode;

        if (f == 15)
        {
            byteBlock.Position += 1;
            Request.Data = byteBlock.Memory.Slice(byteBlock.Position, Request.Length).Span.ByteToBoolArray(Request.Length).BoolToByte();
        }
        else if (f == 16)
        {
            byteBlock.Position += 1;
            Request.Data = byteBlock.Memory.Slice(byteBlock.Position, Request.Length);
        }

        OperCode = 0;
        return FilterResult.Success;
    }
}
