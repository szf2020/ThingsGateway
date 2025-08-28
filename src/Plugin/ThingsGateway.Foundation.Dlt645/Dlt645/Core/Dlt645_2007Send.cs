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

using ThingsGateway.Foundation.Extension.String;

using TouchSocket.Core;

namespace ThingsGateway.Foundation.Dlt645;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class Dlt645_2007Request
{
    #region Request

    /// <summary>
    /// 数据标识
    /// </summary>
    public Memory<byte> DataId { get; set; } = Memory<byte>.Empty;
    private string data;
    public string Data
    {
        get
        {
            return data;
        }
        set
        {
            data = value;
            if (value != null)
            {
                DataId = value.HexStringToBytes();
                DataId.Span.Reverse();
            }
            else
                DataId = Memory<byte>.Empty;
        }
    }
    /// <summary>
    /// 反转解析
    /// </summary>
    public bool Reverse { get; set; } = true;

    /// <summary>
    /// 站号信息
    /// </summary>
    public Memory<byte> Station { get; set; } = Memory<byte>.Empty;
    private string stationString;
    public string StationString
    {
        get
        {
            return stationString;
        }
        set
        {
            stationString = value;
            if (value != null)
            {
                if (value.Length < 12)
                    value = value.PadLeft(12, '0');
                Station = value.HexStringToBytes();
                Station.Span.Reverse();
            }
            else
                Station = Memory<byte>.Empty;
        }
    }
    #endregion Request
}

/// <summary>
/// <inheritdoc/>
/// </summary>
public class Dlt645_2007Send : ISendMessage
{
    internal ControlCode ControlCode = default;

    /// <summary>
    /// 密码、操作码
    /// </summary>
    private ReadOnlyMemory<byte> Codes = default;

    /// <summary>
    /// 写入值
    /// </summary>
    private ReadOnlyMemory<string> Datas = default;

    private ReadOnlyMemory<byte> Fehead = default;

    public Dlt645_2007Send(Dlt645_2007Address dlt645_2007Address, ControlCode controlCode, ReadOnlyMemory<byte> fehead = default, ReadOnlyMemory<byte> codes = default, ReadOnlyMemory<string> datas = default)
    {
        Dlt645_2007Address = dlt645_2007Address;
        ControlCode = controlCode;

        Fehead = fehead;
        Codes = codes;
        Datas = datas;
    }

    public int MaxLength => 300;
    public int SendHeadCodeIndex { get; private set; }
    public int Sign { get; set; }
    internal Dlt645_2007Address Dlt645_2007Address { get; }

    public void Build<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IBytesWriter
    {
        if (Dlt645_2007Address?.DataId.Length < 4)
        {
            throw new(AppResource.DataIdError);
        }
        if (Fehead.Length > 0)
        {
            byteBlock.Write(Fehead.Span);//帧起始符
            SendHeadCodeIndex = Fehead.Length;
        }
        var span = byteBlock.GetSpan(256);

        WriterExtension.WriteValue(ref byteBlock, (byte)0x68);//帧起始符
        byteBlock.Write(Dlt645_2007Address.Station.Span);//6个字节地址域
        WriterExtension.WriteValue(ref byteBlock, (byte)0x68);//帧起始符
        WriterExtension.WriteValue(ref byteBlock, (byte)ControlCode);//控制码

        var writerLenAnchor = new WriterAnchor<TByteBlock>(ref byteBlock, 1);
        byteBlock.Write(Dlt645_2007Address.DataId.Span.BytesAdd(0x33));//数据域标识DI3、DI2、DI1、DI0

        byteBlock.Write(Codes.Span.BytesAdd(0x33));

        if (Datas.Length > 0)
        {
            var dataInfos = Dlt645Helper.GetDataInfos(Dlt645_2007Address.DataId.Span);
            if (Datas.Length != dataInfos.Count)
            {
                throw new(AppResource.CountError);
            }
            var datas = Datas.Span;
            for (int i = 0; i < Datas.Length; i++)
            {
                var dataInfo = dataInfos[i];
                Span<byte> data;
                if (dataInfo.IsSigned)//可能为负数
                {
                    var doubleValue = Convert.ToDouble(datas[i]);
                    if (dataInfo.Digtal != 0)//无小数点
                    {
                        doubleValue *= Math.Pow(10.0, dataInfo.Digtal);
                    }
                    data = doubleValue.ToString().HexStringToBytes().Span;
                    data.Reverse();
                    if (doubleValue < 0)
                    {
                        data[0] = (byte)(data[0] & 0x80);
                    }
                }
                else
                {
                    if (dataInfo.Digtal < 0)
                    {
                        data = Encoding.ASCII.GetBytes(datas[i]).AsSpan();
                        data.Reverse();
                    }
                    else if (dataInfo.Digtal == 0)//无小数点
                    {
                        data = datas[i].HexStringToBytes().Span;
                        data.Reverse();
                    }
                    else
                    {
                        data = (Convert.ToDouble(datas[i]) * Math.Pow(10.0, dataInfo.Digtal)).ToString().HexStringToBytes().Span;
                        data.Reverse();
                    }
                }

                byteBlock.Write(data.BytesAdd(0x33));
            }
        }

        var lenSpan = writerLenAnchor.Rewind(ref byteBlock, out var length);
        lenSpan.WriteValue<byte>((byte)(length - 1));//数据域长度

        int num = 0;
        for (int index = 0; index < byteBlock.WrittenCount - SendHeadCodeIndex; ++index)
            num += span[index];
        WriterExtension.WriteValue(ref byteBlock, (byte)num);//校验码,总加和
        WriterExtension.WriteValue(ref byteBlock, (byte)0x16);//结束符
    }
}
