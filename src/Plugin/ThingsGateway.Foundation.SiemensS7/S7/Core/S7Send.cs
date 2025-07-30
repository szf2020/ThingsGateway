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

using TouchSocket.Core;

namespace ThingsGateway.Foundation.SiemensS7;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class S7Request
{
    #region Request

    public int AddressStart { get; set; }

    /// <summary>
    /// bit位偏移
    /// </summary>
    public byte BitCode { get; set; }

    /// <summary>
    /// 写入数据
    /// </summary>
    public ReadOnlyMemory<byte> Data { get; set; }

    /// <summary>
    /// 数据块代码
    /// </summary>
    public S7Area DataCode { get; set; }

    /// <summary>
    /// DB块数据信息
    /// </summary>
    public ushort DbBlock { get; set; }

    /// <summary>
    /// Length
    /// </summary>
    public int Length { get; set; }

    public int BitLength { get; set; }
    public bool IsBit { get; set; }
    #endregion Request
}

/// <summary>
/// <inheritdoc/>
/// </summary>
public class S7Send : ISendMessage
{
    internal bool Handshake;
    internal byte[] HandshakeBytes;
    internal bool Read;
    internal SiemensS7Address[] SiemensS7Address;

    public S7Send(byte[] handshakeBytes)
    {
        HandshakeBytes = handshakeBytes;
        Handshake = true;
    }

    public S7Send(SiemensS7Address[] siemensS7Address, bool read)
    {
        SiemensS7Address = siemensS7Address;
        Read = read;
    }

    public int MaxLength => 2048;
    public int Sign { get; set; }

    public void Build<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlockWriter
    {
        if (Handshake == true)
        {
            byteBlock.Write(HandshakeBytes);
            return;
        }
        if (Read == true)
        {
            GetReadCommand(ref byteBlock, SiemensS7Address);
        }
        else
        {
            GetWriteByteCommand(ref byteBlock, SiemensS7Address);
        }
    }

    internal void GetReadCommand<TByteBlock>(ref TByteBlock byteBlock, SiemensS7Address[] siemensS7Address) where TByteBlock : IByteBlockWriter
    {
        byte len = (byte)siemensS7Address.Length;
        ushort telegramLen = (ushort)(len * 12 + 19);
        ushort parameterLen = (ushort)(len * 12 + 2);
        //TPKT
        WriterExtension.WriteValue(ref byteBlock, (byte)3);//版本
        WriterExtension.WriteValue(ref byteBlock, (byte)0);//保留
        WriterExtension.WriteValue(ref byteBlock, (ushort)telegramLen, EndianType.Big);//长度，item.len*12+19
        //COTP信息
        WriterExtension.WriteValue(ref byteBlock, (byte)2);//长度
        WriterExtension.WriteValue(ref byteBlock, (byte)0xf0);//pdu类型
        WriterExtension.WriteValue(ref byteBlock, (byte)0x80);//目标引用
        //header
        WriterExtension.WriteValue(ref byteBlock, (byte)0x32);//协议id
        WriterExtension.WriteValue(ref byteBlock, (byte)0x01);//请求
        WriterExtension.WriteValue(ref byteBlock, (ushort)0x00, EndianType.Big);//冗余识别
        WriterExtension.WriteValue(ref byteBlock, (ushort)Sign, EndianType.Big);//数据ID标识
        WriterExtension.WriteValue(ref byteBlock, (ushort)parameterLen, EndianType.Big);//参数长度，item.len*12+2
        WriterExtension.WriteValue(ref byteBlock, (ushort)0x00, EndianType.Big);//数据长度，data.len+4 ,写入时填写，读取时为0
        //par
        WriterExtension.WriteValue(ref byteBlock, (byte)0x04);//功能码，4 Read Var, 5 Write Var
        WriterExtension.WriteValue(ref byteBlock, (byte)len);//Item数量
        //通信项构建
        for (int index = 0; index < len; index++)
        {
            WriterExtension.WriteValue(ref byteBlock, (byte)0x12);//Var 规范
            WriterExtension.WriteValue(ref byteBlock, (byte)0x0a);//剩余的字节长度
            WriterExtension.WriteValue(ref byteBlock, (byte)0x10);//Syntax ID

            if (siemensS7Address[index].DataCode == S7Area.CT || siemensS7Address[index].DataCode == S7Area.TM)
            {
                WriterExtension.WriteValue(ref byteBlock, (byte)siemensS7Address[index].DataCode);//数据类型
            }
            else
            {
                WriterExtension.WriteValue(ref byteBlock, (byte)S7WordLength.Byte);//数据类型
            }
            WriterExtension.WriteValue(ref byteBlock, (ushort)siemensS7Address[index].Length, EndianType.Big);//读取长度
            WriterExtension.WriteValue(ref byteBlock, (ushort)siemensS7Address[index].DbBlock, EndianType.Big);//DB编号
            WriterExtension.WriteValue(ref byteBlock, (byte)siemensS7Address[index].DataCode);//数据块类型
            WriterExtension.WriteValue(ref byteBlock, (byte)(siemensS7Address[index].AddressStart / 256 / 256 % 256));//数据块偏移量
            WriterExtension.WriteValue(ref byteBlock, (byte)(siemensS7Address[index].AddressStart / 256 % 256));//数据块偏移量
            WriterExtension.WriteValue(ref byteBlock, (byte)(siemensS7Address[index].AddressStart % 256));//数据块偏移量
        }
    }

    internal void GetWriteByteCommand<TByteBlock>(ref TByteBlock byteBlock, SiemensS7Address[] addresss) where TByteBlock : IByteBlockWriter
    {
        byte itemLen = (byte)addresss.Length;
        ushort parameterLen = (ushort)(itemLen * 12 + 2);
        //TPKT
        WriterExtension.WriteValue(ref byteBlock, (byte)3);//版本
        WriterExtension.WriteValue(ref byteBlock, (byte)0);
        WriterExtension.WriteValue(ref byteBlock, (ushort)0, EndianType.Big);//长度，item.len*12+19
        //COTP信息
        WriterExtension.WriteValue(ref byteBlock, (byte)2);//长度
        WriterExtension.WriteValue(ref byteBlock, (byte)0xf0);//pdu类型
        WriterExtension.WriteValue(ref byteBlock, (byte)0x80);//目标引用
        //header
        WriterExtension.WriteValue(ref byteBlock, (byte)0x32);//协议id
        WriterExtension.WriteValue(ref byteBlock, (byte)0x01);//请求
        WriterExtension.WriteValue(ref byteBlock, (ushort)0x00, EndianType.Big);//冗余识别
        WriterExtension.WriteValue(ref byteBlock, (ushort)Sign, EndianType.Big);//数据ID标识
        WriterExtension.WriteValue(ref byteBlock, (ushort)parameterLen, EndianType.Big);//参数长度，item.len*12+2
        WriterExtension.WriteValue(ref byteBlock, (ushort)0, EndianType.Big);//数据长度，data.len+4 ,写入时填写，读取时为0

        //par
        WriterExtension.WriteValue(ref byteBlock, (byte)0x05);//功能码，4 Read Var, 5 Write Var
        WriterExtension.WriteValue(ref byteBlock, (byte)itemLen);//Item数量
        //写入Item与读取大致相同

        foreach (var address in addresss)
        {
            var data = address.Data;
            byte len = (byte)address.Length;
            bool isBit = (address.IsBit && len == 1);
            WriterExtension.WriteValue(ref byteBlock, (byte)0x12);//Var 规范
            WriterExtension.WriteValue(ref byteBlock, (byte)0x0a);//剩余的字节长度
            WriterExtension.WriteValue(ref byteBlock, (byte)0x10);//Syntax ID
            WriterExtension.WriteValue(ref byteBlock, (byte)(isBit ? (byte)S7WordLength.Bit : (byte)S7WordLength.Byte));//数据类型
            WriterExtension.WriteValue(ref byteBlock, (ushort)len, EndianType.Big);//长度
            WriterExtension.WriteValue(ref byteBlock, (ushort)address.DbBlock, EndianType.Big);//DB编号
            WriterExtension.WriteValue(ref byteBlock, (byte)address.DataCode);//数据块类型
            WriterExtension.WriteValue(ref byteBlock, (byte)((address.AddressStart + address.BitCode) / 256 / 256));//数据块偏移量
            WriterExtension.WriteValue(ref byteBlock, (byte)((address.AddressStart + address.BitCode) / 256));//数据块偏移量
            WriterExtension.WriteValue(ref byteBlock, (byte)((address.AddressStart + address.BitCode) % 256));//数据块偏移量
        }
        ushort dataLen = 0;
        //data
        foreach (var address in addresss)
        {
            var data = address.Data;
            byte len = (byte)address.Length;
            bool isBit = (address.IsBit && len == 1);
            data = data.ArrayExpandToLengthEven();
            //后面跟的是写入的数据信息
            WriterExtension.WriteValue(ref byteBlock, (byte)0);
            WriterExtension.WriteValue(ref byteBlock, (byte)(isBit ? address.DataCode == S7Area.CT ? 9 : 3 : 4));//Bit:3;Byte:4;Counter或者Timer:9
            WriterExtension.WriteValue(ref byteBlock, (ushort)(isBit ? (byte)address.BitLength : len * 8), EndianType.Big);
            byteBlock.Write(data.Span);

            dataLen = (ushort)(dataLen + data.Length + 4);
        }
        ushort telegramLen = (ushort)(itemLen * 12 + 19 + dataLen);
        byteBlock.Position = 2;
        WriterExtension.WriteValue(ref byteBlock, (ushort)telegramLen, EndianType.Big);//长度
        byteBlock.Position = 15;
        WriterExtension.WriteValue(ref byteBlock, (ushort)dataLen, EndianType.Big);//长度
        byteBlock.Position = byteBlock.Length;
    }
}
