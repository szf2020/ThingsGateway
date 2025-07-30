//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using TouchSocket.Core;

namespace ThingsGateway.Foundation.SiemensS7;

/// <summary>
/// S7BitConverter
/// </summary>
public class S7BitConverter : ThingsGatewayBitConverter
{
    public S7BitConverter()
    {
    }

    public S7BitConverter(EndianType endianType) : base(endianType)
    {
    }

    public bool SMART200 { get; set; } = false;
    public bool WStringEnable { get; set; } = false;
    public override int? StringLength { get; set; } = 100;


    /// <inheritdoc/>
    public override string ToString(ReadOnlySpan<byte> buffer, int offset, int length)
    {
        if (WStringEnable)
        {
            if (!SMART200)
                return base.ToString(buffer, offset, this.ToUInt16(buffer, offset - 2) * 2);
            else
                return base.ToString(buffer, offset, buffer[offset - 1] * 2);
        }
        else if (IsVariableStringLength)
        {
            if (!SMART200)
                return base.ToString(buffer, offset, buffer[offset - 1]);
            else
                return base.ToString(buffer, offset, buffer[offset - 1]);
        }
        else
        {
            return base.ToString(buffer, offset, length);
        }
    }

    public override void OtherPropertySet(IThingsGatewayBitConverter thingsGatewayBitConverter, string registerAddress)
    {
        if (thingsGatewayBitConverter is not S7BitConverter s7BitConverter)
        {
            return;
        }
        var sAddress = SiemensS7Address.ParseFrom(registerAddress);
        s7BitConverter.WStringEnable = sAddress.WStringEnable;

        base.OtherPropertySet(thingsGatewayBitConverter, registerAddress);
    }
}
