namespace ThingsGateway.Foundation.Modbus;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class ModbusTcpMessage : MessageBase, IResultMessage
{
    /// <inheritdoc/>
    public override long HeaderLength => 8;

    public ModbusResponse Response { get; set; } = new();

    public override bool CheckHead<TByteBlock>(ref TByteBlock byteBlock)
    {
        Sign = ReaderExtension.ReadValue<TByteBlock, ushort>(ref byteBlock, EndianType.Big);
        byteBlock.BytesRead += 2;
        BodyLength = ReaderExtension.ReadValue<TByteBlock, ushort>(ref byteBlock, EndianType.Big) - 2;
        Response.Station = ReaderExtension.ReadValue<TByteBlock, byte>(ref byteBlock);
        Response.FunctionCode = ReaderExtension.ReadValue<TByteBlock, byte>(ref byteBlock);
        if ((Response.FunctionCode & 0x80) == 0)
        {
            error = false;
        }
        else
        {
            Response.FunctionCode = Response.FunctionCode.SetBit(7, false);
            error = true;
        }
        return true;
    }


    public override FilterResult CheckBody<TByteBlock>(ref TByteBlock byteBlock)
    {
        try
        {

            var f = Response.FunctionCode > 0x30 ? Response.FunctionCode - 0x30 : Response.FunctionCode;
            if (error)
            {
                Response.ErrorCode = ReaderExtension.ReadValue<TByteBlock, byte>(ref byteBlock);
                OperCode = Response.ErrorCode;
                ErrorMessage = ModbusHelper.GetDescriptionByErrorCode(Response.ErrorCode.Value);
                ErrorType = ErrorTypeEnum.DeviceError;
            }
            else
            {
                Response.ErrorCode = null;
            }
            if (Response.ErrorCode != null)
            {
                return FilterResult.Success;
            }

            if (f <= 4)
            {
                OperCode = 0;
                Response.Length = ReaderExtension.ReadValue<TByteBlock, byte>(ref byteBlock);
                Content = byteBlock.ToArrayTake(BodyLength - 1);
                Response.MasterWriteDatas = Content;
                return FilterResult.Success;
            }
            else if (f == 5 || f == 6)
            {
                Response.StartAddress = ReaderExtension.ReadValue<TByteBlock, ushort>(ref byteBlock, EndianType.Big);
                OperCode = 0;
                Content = byteBlock.ToArrayTake(BodyLength - 2);
                Response.MasterWriteDatas = Content;
                return FilterResult.Success;
            }
            else if (f == 15 || f == 16)
            {
                Response.StartAddress = ReaderExtension.ReadValue<TByteBlock, ushort>(ref byteBlock, EndianType.Big);
                OperCode = 0;
                Response.Length = ReaderExtension.ReadValue<TByteBlock, ushort>(ref byteBlock, EndianType.Big);
                Content = Array.Empty<byte>();
                return FilterResult.Success;
            }
            else
            {
                OperCode = 999;
                ErrorMessage = AppResource.ModbusError1;
            }

        }
        catch (Exception ex)
        {
            throw new Exception($"Data length:{byteBlock.TotalSequence.Length} bodyLength:{BodyLength} BytesRead:{byteBlock.BytesRead} BytesRemaining:{byteBlock.BytesRemaining}", ex);
        }
        return FilterResult.GoOn;
    }
    bool error = false;
}
