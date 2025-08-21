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

/// <inheritdoc/>
public partial class ModbusMaster : DtuServiceDeviceBase, IModbusAddress
{
    public override void InitChannel(IChannel channel, ILog? deviceLog = null)
    {
        base.InitChannel(channel, deviceLog);

        RegisterByteLength = 2;
    }

    protected override void SetChannel()
    {
        if (ModbusType != ModbusTypeEnum.ModbusTcp)
        {
            Channel.ChannelOptions.MaxConcurrentCount = 1;
        }
    }

    public override IThingsGatewayBitConverter ThingsGatewayBitConverter { get; } = new ThingsGatewayBitConverter(EndianType.Big);

    /// <summary>
    /// Modbus类型，在initChannelAsync之前设置
    /// </summary>
    public ModbusTypeEnum ModbusType { get; set; }

    /// <summary>
    /// 站号
    /// </summary>
    public byte Station { get; set; } = 1;

    /// <inheritdoc/>
    public override string GetAddressDescription()
    {
        return $"{base.GetAddressDescription()}{Environment.NewLine}{ModbusHelper.GetAddressDescription()}";
    }

    /// <inheritdoc/>
    public override DataHandlingAdapter GetDataAdapter()
    {
        switch (ModbusType)
        {
            case ModbusTypeEnum.ModbusTcp:
                switch (Channel.ChannelType)
                {
                    case ChannelTypeEnum.TcpClient:
                    case ChannelTypeEnum.TcpService:
                    case ChannelTypeEnum.SerialPort:
                        return new DeviceSingleStreamDataHandleAdapter<ModbusTcpMessage>()
                        {
                            CacheTimeout = TimeSpan.FromMilliseconds(Channel.ChannelOptions.CacheTimeout),
                        };

                    case ChannelTypeEnum.UdpSession:
                        return new DeviceUdpDataHandleAdapter<ModbusTcpMessage>();
                }
                return new DeviceSingleStreamDataHandleAdapter<ModbusTcpMessage>()
                {
                    CacheTimeout = TimeSpan.FromMilliseconds(Channel.ChannelOptions.CacheTimeout),
                };

            case ModbusTypeEnum.ModbusRtu:
                switch (Channel.ChannelType)
                {
                    case ChannelTypeEnum.TcpClient:
                    case ChannelTypeEnum.TcpService:
                    case ChannelTypeEnum.SerialPort:
                        return new DeviceSingleStreamDataHandleAdapter<ModbusRtuMessage>()
                        {
                            CacheTimeout = TimeSpan.FromMilliseconds(Channel.ChannelOptions.CacheTimeout),
                        };

                    case ChannelTypeEnum.UdpSession:
                        return new DeviceUdpDataHandleAdapter<ModbusRtuMessage>();
                }
                return new DeviceSingleStreamDataHandleAdapter<ModbusRtuMessage>()
                {
                    CacheTimeout = TimeSpan.FromMilliseconds(Channel.ChannelOptions.CacheTimeout),
                };
        }
        return new DeviceSingleStreamDataHandleAdapter<ModbusTcpMessage>()
        {
            CacheTimeout = TimeSpan.FromMilliseconds(Channel.ChannelOptions.CacheTimeout),
        };
    }

    /// <inheritdoc/>
    public override List<T> LoadSourceRead<T>(IEnumerable<IVariable> deviceVariables, int maxPack, string defaultIntervalTime)
    {
        return PackHelper.LoadSourceRead<T>(this, deviceVariables, maxPack, defaultIntervalTime, Station);
    }

    public override ValueTask<OperResult<ReadOnlyMemory<byte>>> ReadAsync(object state, CancellationToken cancellationToken = default)
    {
        try
        {
            if (state is ModbusAddress mAddress)
            {
                return ModbusReadAsync(mAddress, cancellationToken);
            }
            else
            {
                return EasyValueTask.FromResult(new OperResult<ReadOnlyMemory<byte>>(new ArgumentException("State must be of type ModbusAddress", nameof(state))));
            }
        }
        catch (Exception ex)
        {
            return EasyValueTask.FromResult(new OperResult<ReadOnlyMemory<byte>>(ex));
        }
    }

    public ValueTask<OperResult<ReadOnlyMemory<byte>>> ModbusReadAsync(ModbusAddress mAddress, CancellationToken cancellationToken = default)
    {
        try
        {
            return SendThenReturnAsync(GetSendMessage(mAddress, true),
             cancellationToken);
        }
        catch (Exception ex)
        {
            return EasyValueTask.FromResult(new OperResult<ReadOnlyMemory<byte>>(ex));
        }
    }

    public ValueTask<OperResult<ReadOnlyMemory<byte>>> ModbusRequestAsync(ModbusAddress mAddress, bool read, CancellationToken cancellationToken = default)
    {
        try
        {
            return SendThenReturnAsync(GetSendMessage(mAddress, read),
             cancellationToken);
        }
        catch (Exception ex)
        {
            return EasyValueTask.FromResult(new OperResult<ReadOnlyMemory<byte>>(ex));
        }
    }

    /// <inheritdoc/>
    public override ValueTask<OperResult<ReadOnlyMemory<byte>>> ReadAsync(string address, int length, CancellationToken cancellationToken = default)
    {
        try
        {
            var mAddress = GetModbusAddress(address, Station);
            mAddress.Length = (ushort)length;
            return ModbusRequestAsync(mAddress, true, cancellationToken);
        }
        catch (Exception ex)
        {
            return EasyValueTask.FromResult(new OperResult<ReadOnlyMemory<byte>>(ex));
        }
    }

    /// <inheritdoc/>
    public override async ValueTask<OperResult> WriteAsync(string address, ReadOnlyMemory<byte> value, DataTypeEnum dataType, CancellationToken cancellationToken = default)
    {
        try
        {
            var mAddress = GetModbusAddress(address, Station);
            mAddress.MasterWriteDatas = value;

            if (mAddress.BitIndex == null)
            {
                return await ModbusRequestAsync(mAddress, false, cancellationToken).ConfigureAwait(false);
            }
            if (mAddress.BitIndex < 2)
            {
                mAddress.Length = 1; //请求寄存器数量
                var readData = await ModbusRequestAsync(mAddress, true, cancellationToken).ConfigureAwait(false);
                if (!readData.IsSuccess) return readData;
                var v = value.Span[0];
                var writeValye = readData.Content.ToArray();
                if (mAddress.BitIndex == 0)
                    writeValye[1] = v;
                else
                    writeValye[0] = v;

                mAddress.MasterWriteDatas = writeValye;
                return await ModbusRequestAsync(mAddress, false, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                return new OperResult(string.Format(AppResource.ValueOverlimit, nameof(mAddress.BitIndex), 2));
            }
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }
    public virtual ModbusAddress GetModbusAddress(string address, byte? station, bool isCache = true)
    {
        var mAddress = ModbusAddress.ParseFrom(address, station, isCache);
        return mAddress;
    }
    /// <inheritdoc/>
    public override async ValueTask<OperResult> WriteAsync(string address, ReadOnlyMemory<bool> value, CancellationToken cancellationToken = default)
    {
        try
        {
            var mAddress = GetModbusAddress(address, Station);
            if (value.Length > 1 && (mAddress.FunctionCode == 1 || mAddress.FunctionCode == 0x31))
            {
                mAddress.WriteFunctionCode = 15;
                mAddress.MasterWriteDatas = value.Span.BoolArrayToByte();
                return await ModbusRequestAsync(mAddress, false, cancellationToken).ConfigureAwait(false);
            }
            else if (mAddress.BitIndex == null)
            {
                var span = value.Span;
                mAddress.MasterWriteDatas = span[0] ? new byte[2] { 255, 0 } : [0, 0];
                return await ModbusRequestAsync(mAddress, false, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                if (mAddress.BitIndex < 16)
                {
                    mAddress.Length = 1; //请求寄存器数量
                    var readData = await ModbusRequestAsync(mAddress, true, cancellationToken).ConfigureAwait(false);
                    if (!readData.IsSuccess) return readData;
                    var writeData = ThingsGatewayBitConverter.ToUInt16(readData.Content.Span, 0);
                    var span = value.Span;
                    for (int i = 0; i < span.Length; i++)
                    {
                        writeData = writeData.SetBit(mAddress.BitIndex.Value + i, span[i]);
                    }
                    mAddress.MasterWriteDatas = ThingsGatewayBitConverter.GetBytes(writeData);
                    return await ModbusRequestAsync(mAddress, false, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    return new OperResult(string.Format(AppResource.ValueOverlimit, nameof(mAddress.BitIndex), 16));
                }
            }
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    protected ISendMessage GetSendMessage(ModbusAddress modbusAddress, bool read)
    {
        if (ModbusType == ModbusTypeEnum.ModbusRtu)
        {
            return new ModbusRtuSend(modbusAddress, read);
        }
        else
        {
            return new ModbusTcpSend(modbusAddress, read);
        }
    }
}
