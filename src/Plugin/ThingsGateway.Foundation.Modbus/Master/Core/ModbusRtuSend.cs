//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Foundation.Extension.Generic;

namespace ThingsGateway.Foundation.Modbus;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class ModbusRtuSend : ISendMessage
{
    private bool Read;

    public ModbusRtuSend(ModbusAddress modbusAddress, bool read)
    {
        ModbusAddress = modbusAddress;
        Read = read;
    }

    public int MaxLength => 300;
    public ModbusAddress ModbusAddress { get; }
    public int Sign { get; set; }

    public void Build<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IBytesWriter
    {
        var span = byteBlock.GetSpan(512);
        var f = ModbusAddress.FunctionCode > 0x30 ? ModbusAddress.FunctionCode - 0x30 : ModbusAddress.FunctionCode;
        if (!Read)
        {
            if (ModbusAddress.WriteFunctionCode == null)
            {
                ModbusAddress.WriteFunctionCode = (byte)(f == 1 ? 5 : 6);
            }
            if (ModbusAddress.MasterWriteDatas.Length > 2 && ModbusAddress.WriteFunctionCode < 15)
            {
                ModbusAddress.WriteFunctionCode = (byte)(f == 1 ? 15 : 16);
            }
        }

        var wf = ModbusAddress.WriteFunctionCode;

        if (ModbusAddress.FunctionCode > 0x30 && ModbusAddress.WriteFunctionCode < 0x30)
        {
            ModbusAddress.WriteFunctionCode += 0x30;
        }

        if (Read)
        {
            WriterExtension.WriteValue(ref byteBlock, (byte)ModbusAddress.Station);
            WriterExtension.WriteValue(ref byteBlock, (byte)ModbusAddress.FunctionCode);
            WriterExtension.WriteValue(ref byteBlock, (ushort)ModbusAddress.StartAddress, EndianType.Big);
            WriterExtension.WriteValue(ref byteBlock, (ushort)ModbusAddress.Length, EndianType.Big);
        }
        else if (wf == 5 || wf == 6)
        {
            WriterExtension.WriteValue(ref byteBlock, (byte)ModbusAddress.Station);
            WriterExtension.WriteValue(ref byteBlock, (byte)ModbusAddress.WriteFunctionCode);
            WriterExtension.WriteValue(ref byteBlock, (ushort)ModbusAddress.StartAddress, EndianType.Big);
            byteBlock.Write(ModbusAddress.MasterWriteDatas.Span);
        }
        else if (wf == 15 || wf == 16)
        {
            var data = ModbusAddress.MasterWriteDatas.ArrayExpandToLengthEven().Span;
            WriterExtension.WriteValue(ref byteBlock, (ushort)(data.Length + 7), EndianType.Big);
            WriterExtension.WriteValue(ref byteBlock, (byte)ModbusAddress.Station);
            WriterExtension.WriteValue(ref byteBlock, (byte)ModbusAddress.WriteFunctionCode);
            WriterExtension.WriteValue(ref byteBlock, (ushort)ModbusAddress.StartAddress, EndianType.Big);
            var len = (ushort)Math.Ceiling(wf == 15 ? ModbusAddress.MasterWriteDatas.Length * 8 : ModbusAddress.MasterWriteDatas.Length / 2.0);
            WriterExtension.WriteValue(ref byteBlock, len, EndianType.Big);
            WriterExtension.WriteValue(ref byteBlock, (byte)(len * 2));
            byteBlock.Write(data);
        }
        else
        {
            throw new System.InvalidOperationException(AppResource.ModbusError1);
        }
        var crclen = byteBlock.WrittenCount;
        byteBlock.Write(CRC16Utils.Crc16Only(span.Slice(0, (int)crclen)));
    }
}
