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
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

using TouchSocket.Sockets;

namespace ThingsGateway.Foundation.Modbus;

/// <summary>
/// ChannelEventHandler
/// </summary>
public delegate ValueTask<IOperResult> ModbusServerWriteEventHandler(ModbusAddress modbusAddress, IThingsGatewayBitConverter bitConverter, IClientChannel channel);

/// <inheritdoc/>
public class ModbusSlave : DeviceBase, IModbusAddress
{
    /// <summary>
    /// 继电器
    /// </summary>
    private ConcurrentDictionary<byte, ByteBlock> ModbusServer01ByteBlocks = new();

    /// <summary>
    /// 开关输入
    /// </summary>
    private ConcurrentDictionary<byte, ByteBlock> ModbusServer02ByteBlocks = new();

    /// <summary>
    /// 输入寄存器
    /// </summary>
    private ConcurrentDictionary<byte, ByteBlock> ModbusServer03ByteBlocks = new();

    /// <summary>
    /// 保持寄存器
    /// </summary>
    private ConcurrentDictionary<byte, ByteBlock> ModbusServer04ByteBlocks = new();

    /// <inheritdoc/>
    public override void InitChannel(IChannel channel, ILog? deviceLog = null)
    {
        base.InitChannel(channel, deviceLog);
        RegisterByteLength = 2;
    }
    public override IThingsGatewayBitConverter ThingsGatewayBitConverter { get; } = new ThingsGatewayBitConverter(EndianType.Big);

    public override bool SupportMultipleDevice()
    {
        return Channel.ChannelType == ChannelTypeEnum.TcpService;
    }

    #region 属性

    private ModbusTypeEnum modbusType;

    /// <summary>
    /// 写入内存
    /// </summary>
    public bool IsWriteMemory { get; set; }

    /// <summary>
    /// Modbus类型
    /// </summary>
    public ModbusTypeEnum ModbusType
    {
        get { return modbusType; }
        set { modbusType = value; }
    }

    /// <summary>
    /// 多站点
    /// </summary>
    public bool MulStation { get; set; } = true;

    /// <summary>
    /// 默认站点
    /// </summary>
    public byte Station { get; set; } = 1;

    #endregion 属性

    /// <summary>
    /// 接收外部写入时，传出变量地址/写入字节组/转换规则/客户端
    /// </summary>
    public ModbusServerWriteEventHandler WriteData { get; set; }

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
                        return new DeviceSingleStreamDataHandleAdapter<ModbusTcpSlaveMessage>()
                        {
                            CacheTimeout = TimeSpan.FromMilliseconds(Channel.ChannelOptions.CacheTimeout),
                            IsSingleThread = false,
                        };

                    case ChannelTypeEnum.UdpSession:
                        return new DeviceUdpDataHandleAdapter<ModbusTcpSlaveMessage>()
                        {
                            IsSingleThread = false,
                        };
                }
                break;

            case ModbusTypeEnum.ModbusRtu:
                switch (Channel.ChannelType)
                {
                    case ChannelTypeEnum.TcpClient:
                    case ChannelTypeEnum.TcpService:
                    case ChannelTypeEnum.SerialPort:
                        return new DeviceSingleStreamDataHandleAdapter<ModbusRtuSlaveMessage>()
                        {
                            CacheTimeout = TimeSpan.FromMilliseconds(Channel.ChannelOptions.CacheTimeout),
                            IsSingleThread = false,
                        };

                    case ChannelTypeEnum.UdpSession:
                        return new DeviceUdpDataHandleAdapter<ModbusRtuSlaveMessage>()
                        {
                            IsSingleThread = false,
                        };
                }
                break;
        }
        return new DeviceSingleStreamDataHandleAdapter<ModbusTcpSlaveMessage>()
        {
            CacheTimeout = TimeSpan.FromMilliseconds(Channel.ChannelOptions.CacheTimeout),
            IsSingleThread = false,
        };
    }

    /// <inheritdoc/>
    public override List<T> LoadSourceRead<T>(IEnumerable<IVariable> deviceVariables, int maxPack, string defaultIntervalTime)
    {
        return PackHelper.LoadSourceRead<T>(this, deviceVariables, maxPack, defaultIntervalTime);
    }

    /// <inheritdoc/>
    protected override Task DisposeAsync(bool disposing)
    {
        foreach (var item in ModbusServer01ByteBlocks)
        {
            item.Value.SafeDispose();
        }
        foreach (var item in ModbusServer02ByteBlocks)
        {
            item.Value.SafeDispose();
        }
        foreach (var item in ModbusServer03ByteBlocks)
        {
            item.Value.SafeDispose();
        }
        foreach (var item in ModbusServer04ByteBlocks)
        {
            item.Value.SafeDispose();
        }
        ModbusServer01ByteBlocks.Clear();
        ModbusServer02ByteBlocks.Clear();
        ModbusServer03ByteBlocks.Clear();
        ModbusServer04ByteBlocks.Clear();
        return base.DisposeAsync(disposing);
    }

    /// <inheritdoc/>
    private void Init(ModbusRequest mAddress)
    {
        //自动扩容
        ModbusServer01ByteBlocks.GetOrAdd(mAddress.Station, a =>
        {
            var bytes = new ByteBlock(256,
            (c) =>
            {
                var data= ArrayPool<byte>.Shared.Rent(c);
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = 0;
                }
                return data;
            },
            (m) =>
            {
                if (MemoryMarshal.TryGetArray((ReadOnlyMemory<byte>)m, out var result))
                {
                    ArrayPool<byte>.Shared.Return(result.Array);
                }
            }
            );
            bytes.SetLength(256);
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes.WriteByte(0);
            }
            bytes.Position = 0;
            return bytes;
        });
        ModbusServer02ByteBlocks.GetOrAdd(mAddress.Station, a =>
        {
            var bytes = new ByteBlock(256,
            (c) =>
            {
                var data = ArrayPool<byte>.Shared.Rent(c);
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = 0;
                }
                return data;
            },
            (m) =>
            {
                if (MemoryMarshal.TryGetArray((ReadOnlyMemory<byte>)m, out var result))
                {
                    ArrayPool<byte>.Shared.Return(result.Array);
                }
            }
            );
            bytes.SetLength(256);
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes.WriteByte(0);
            }
            bytes.Position = 0;
            return bytes;
        });
        ModbusServer03ByteBlocks.GetOrAdd(mAddress.Station, a =>
        {
            var bytes = new ByteBlock(256,
            (c) =>
            {
                var data = ArrayPool<byte>.Shared.Rent(c);
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = 0;
                }
                return data;
            },
            (m) =>
            {
                if (MemoryMarshal.TryGetArray((ReadOnlyMemory<byte>)m, out var result))
                {
                    ArrayPool<byte>.Shared.Return(result.Array);
                }
            }
            );
            bytes.SetLength(256);
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes.WriteByte(0);
            }
            bytes.Position = 0;
            return bytes;
        });
        ModbusServer04ByteBlocks.GetOrAdd(mAddress.Station, a =>
        {
            var bytes = new ByteBlock(256,
            (c) =>
            {
                var data = ArrayPool<byte>.Shared.Rent(c);
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = 0;
                }
                return data;
            },
            (m) =>
            {
                if (MemoryMarshal.TryGetArray((ReadOnlyMemory<byte>)m, out var result))
                {
                    ArrayPool<byte>.Shared.Return(result.Array);
                }
            }
            );
            bytes.SetLength(256);
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes.WriteByte(0);
            }
            bytes.Position = 0;
            return bytes;
        });
    }

    public override Action<IPluginManager> ConfigurePlugins(TouchSocketConfig config)
    {
        switch (Channel.ChannelType)
        {
            case ChannelTypeEnum.TcpClient:
                return PluginUtil.GetDtuClientPlugin(Channel.ChannelOptions);
            case ChannelTypeEnum.TcpService:
                return PluginUtil.GetTcpServicePlugin(Channel.ChannelOptions);
        }
        return a => { };
    }

    #region 核心

    private readonly ReaderWriterLockSlim _lockSlim = new();

    /// <inheritdoc/>
    public OperResult<ReadOnlyMemory<byte>> ModbusRequest(ModbusRequest mAddress, bool read, CancellationToken cancellationToken = default)
    {
        try
        {
            var f = mAddress.FunctionCode > 0x30 ? mAddress.FunctionCode - 0x30 : mAddress.FunctionCode;

            if (MulStation)
            {
                Init(mAddress);
            }
            else
            {
                if (Station != mAddress.Station)
                {
                    return new(string.Format(AppResource.StationNotSame, mAddress.Station, Station));
                }
                Init(mAddress);
            }
            var ModbusServer01ByteBlock = ModbusServer01ByteBlocks[mAddress.Station];
            var ModbusServer02ByteBlock = ModbusServer02ByteBlocks[mAddress.Station];
            var ModbusServer03ByteBlock = ModbusServer03ByteBlocks[mAddress.Station];
            var ModbusServer04ByteBlock = ModbusServer04ByteBlocks[mAddress.Station];
            if (read)
            {
                using (new ReadLock(_lockSlim))
                {
                    int len = mAddress.Length;

                    switch (f)
                    {
                        case 1:
                            ModbusServer01ByteBlock.Position = mAddress.StartAddress;
                            return OperResult.CreateSuccessResult((ReadOnlyMemory<byte>)ModbusServer01ByteBlock.GetMemory(len).Slice(0, len));

                        case 2:
                            ModbusServer02ByteBlock.Position = mAddress.StartAddress;
                            return OperResult.CreateSuccessResult((ReadOnlyMemory<byte>)ModbusServer02ByteBlock.GetMemory(len).Slice(0, len));

                        case 3:
                            ModbusServer03ByteBlock.Position = mAddress.StartAddress * RegisterByteLength;
                            return OperResult.CreateSuccessResult((ReadOnlyMemory<byte>)ModbusServer03ByteBlock.GetMemory(len).Slice(0, len));

                        case 4:
                            ModbusServer04ByteBlock.Position = mAddress.StartAddress * RegisterByteLength;
                            return OperResult.CreateSuccessResult((ReadOnlyMemory<byte>)ModbusServer04ByteBlock.GetMemory(len).Slice(0, len));
                    }
                }
            }
            else
            {
                using (new WriteLock(_lockSlim))
                {
                    switch (f)
                    {
                        case 2:
                            ModbusServer02ByteBlock.Position = mAddress.StartAddress;
                            ByteBlockExtension.Write(ref ModbusServer02ByteBlock, mAddress.SlaveWriteDatas);
                            return new();

                        case 1:
                        case 5:
                        case 15:
                            ModbusServer01ByteBlock.Position = mAddress.StartAddress;
                            ByteBlockExtension.Write(ref ModbusServer01ByteBlock, mAddress.SlaveWriteDatas);

                            return new();

                        case 4:
                            ModbusServer04ByteBlock.Position = mAddress.StartAddress * RegisterByteLength;
                            ByteBlockExtension.Write(ref ModbusServer04ByteBlock, mAddress.SlaveWriteDatas);

                            return new();

                        case 3:
                        case 6:
                        case 16:
                            ModbusServer03ByteBlock.Position = mAddress.StartAddress * RegisterByteLength;
                            ByteBlockExtension.Write(ref ModbusServer03ByteBlock, mAddress.SlaveWriteDatas);

                            return new();
                    }
                }
            }

            return new(AppResource.FunctionError);
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }

    /// <inheritdoc/>
    public override ValueTask<OperResult<ReadOnlyMemory<byte>>> ReadAsync(string address, int length, CancellationToken cancellationToken = default)
    {
        var mAddress = GetModbusAddress(address, Station);
        mAddress.Length = (ushort)(length * RegisterByteLength);
        var result = ModbusRequest(mAddress, true, cancellationToken);
        if (result.IsSuccess)
        {
            return EasyValueTask.FromResult(new OperResult<ReadOnlyMemory<byte>>() { Content = result.Content });
        }
        else
        {
            return EasyValueTask.FromResult(new OperResult<ReadOnlyMemory<byte>>(result));
        }
    }

    public override ValueTask<OperResult<ReadOnlyMemory<byte>>> ReadAsync(object state, CancellationToken cancellationToken = default)
    {
        try
        {
            if (state is ModbusAddress mAddress)
            {
                var result = ModbusRequest(mAddress, true, cancellationToken);
                if (result.IsSuccess)
                {
                    return EasyValueTask.FromResult(new OperResult<ReadOnlyMemory<byte>>() { Content = result.Content });
                }
                else
                {
                    return EasyValueTask.FromResult(new OperResult<ReadOnlyMemory<byte>>(result));
                }
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

    public virtual ModbusAddress GetModbusAddress(string address, byte? station, bool isCache = true)
    {
        var mAddress = ModbusAddress.ParseFrom(address, station, isCache);
        return mAddress;
    }
    /// <inheritdoc/>
    public override async ValueTask<OperResult> WriteAsync(string address, ReadOnlyMemory<byte> value, DataTypeEnum dataType, CancellationToken cancellationToken = default)
    {
        try
        {
            await EasyValueTask.CompletedTask.ConfigureAwait(false);
            var mAddress = GetModbusAddress(address, Station);
            mAddress.SlaveWriteDatas = new(value);
            return ModbusRequest(mAddress, false, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    /// <inheritdoc/>
    public override async ValueTask<OperResult> WriteAsync(string address, ReadOnlyMemory<bool> value, CancellationToken cancellationToken = default)
    {
        try
        {
            await EasyValueTask.CompletedTask.ConfigureAwait(false);
            var mAddress = GetModbusAddress(address, Station);
            if (mAddress.IsBitFunction)
            {
                mAddress.SlaveWriteDatas = new(value.Span.BoolToByte());
                ModbusRequest(mAddress, false, cancellationToken);
                return OperResult.Success;
            }
            else
            {
                if (mAddress.BitIndex < 16)
                {
                    mAddress.Length = 2;
                    var readData = ModbusRequest(mAddress, true, cancellationToken);
                    if (!readData.IsSuccess) return readData;
                    var writeData = TouchSocketBitConverter.BigEndian.To<ushort>(readData.Content.Span);
                    var span = value.Span;
                    for (int i = 0; i < value.Length; i++)
                    {
                        writeData = writeData.SetBit(mAddress.BitIndex.Value + i, span[i]);
                    }
                    mAddress.SlaveWriteDatas = new(ThingsGatewayBitConverter.GetBytes(writeData));
                    ModbusRequest(mAddress, false, cancellationToken);
                    return OperResult.Success;
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

    /// <inheritdoc/>
    protected override async Task ChannelReceived(IClientChannel client, ReceivedDataEventArgs e, bool last)
    {
        var requestInfo = e.RequestInfo;
        bool modbusRtu = false;
        ModbusRequest modbusRequest = default;
        ReadOnlySequence<byte> readOnlySequences = default;
        //接收外部报文
        if (requestInfo is ModbusRtuSlaveMessage modbusRtuSlaveMessage)
        {
            if (!modbusRtuSlaveMessage.IsSuccess)
            {
                return;
            }
            modbusRequest = modbusRtuSlaveMessage.Request;
            readOnlySequences = modbusRtuSlaveMessage.Sequences;
            modbusRtu = true;
        }
        else if (requestInfo is ModbusTcpSlaveMessage modbusTcpSlaveMessage)
        {
            if (!modbusTcpSlaveMessage.IsSuccess)
            {
                return;
            }
            modbusRequest = modbusTcpSlaveMessage.Request;
            readOnlySequences = modbusTcpSlaveMessage.Sequences;
            modbusRtu = false;
        }
        else
        {
            return;
        }
        //忽略不同设备地址的报文
        if (!MulStation && modbusRequest.Station != Station)
        {
            return;
        }
        var f = modbusRequest.FunctionCode > 0x30 ? modbusRequest.FunctionCode - 0x30 : modbusRequest.FunctionCode;

        if (f <= 4)
        {
            var data = ModbusRequest(modbusRequest, true);
            if (data.IsSuccess)
            {
                ValueByteBlock byteBlock = new(1024);
                try
                {
                    if (modbusRtu)
                    {
                        ByteBlockExtension.Write(ref byteBlock, readOnlySequences.Slice(0, 2));
                        if (modbusRequest.IsBitFunction)
                        {
                            var bitdata = data.Content.Span.ByteToBool().AsSpan().BoolArrayToByte();
                            ReadOnlyMemory<byte> bitwritedata = bitdata.Length == (int)Math.Ceiling(modbusRequest.Length / 8.0) ? bitdata.AsMemory() : bitdata.AsMemory().Slice(0, (int)Math.Ceiling(modbusRequest.Length / 8.0));
                            WriterExtension.WriteValue(ref byteBlock, (byte)bitwritedata.Length);
                            byteBlock.Write(bitwritedata.Span);
                        }
                        else
                        {
                            WriterExtension.WriteValue(ref byteBlock, (byte)data.Content.Length);
                            byteBlock.Write(data.Content.Span);
                        }
                        byteBlock.Write(CRC16Utils.Crc16Only(byteBlock.Memory.Span));
                        await ReturnData(client, byteBlock.Memory, e).ConfigureAwait(false);
                    }
                    else
                    {
                        ByteBlockExtension.Write(ref byteBlock, readOnlySequences.Slice(0, 8));
                        if (modbusRequest.IsBitFunction)
                        {
                            var bitdata = data.Content.Span.ByteToBool().AsSpan().BoolArrayToByte();
                            ReadOnlyMemory<byte> bitwritedata = bitdata.Length == (int)Math.Ceiling(modbusRequest.Length / 8.0) ? bitdata.AsMemory() : bitdata.AsMemory().Slice(0, (int)Math.Ceiling(modbusRequest.Length / 8.0));
                            WriterExtension.WriteValue(ref byteBlock, (byte)bitwritedata.Length);
                            byteBlock.Write(bitwritedata.Span);
                        }
                        else
                        {
                            WriterExtension.WriteValue(ref byteBlock, (byte)data.Content.Length);
                            byteBlock.Write(data.Content.Span);
                        }
                        ByteBlockExtension.WriteBackValue(ref byteBlock, (byte)(byteBlock.Length - 6), EndianType.Big, 5);
                        await ReturnData(client, byteBlock.Memory, e).ConfigureAwait(false);
                    }
                }
                catch
                {
                    await WriteError(modbusRtu, client, readOnlySequences, e).ConfigureAwait(false);
                }
                finally
                {
                    byteBlock.SafeDispose();
                }
            }
            else
            {
                await WriteError(modbusRtu, client, readOnlySequences, e).ConfigureAwait(false);//返回错误码
            }
        }
        else//写入
        {
            if (f == 5 || f == 15)
            {
                //写入继电器
                if (WriteData != null)
                {
                    var modbusAddress = new ModbusAddress(modbusRequest) { WriteFunctionCode = modbusRequest.FunctionCode, FunctionCode = 1 };
                    // 接收外部写入时，传出变量地址/写入字节组/转换规则/客户端
                    if ((await WriteData(modbusAddress, ThingsGatewayBitConverter, client).ConfigureAwait(false)).IsSuccess)
                    {
                        await WriteSuccess(modbusRtu, client, readOnlySequences, e).ConfigureAwait(false);
                        if (IsWriteMemory)
                        {
                            var result = ModbusRequest(modbusRequest, false);
                            if (result.IsSuccess)
                                await WriteSuccess(modbusRtu, client, readOnlySequences, e).ConfigureAwait(false);
                            else
                                await WriteError(modbusRtu, client, readOnlySequences, e).ConfigureAwait(false);
                        }
                        else
                            await WriteSuccess(modbusRtu, client, readOnlySequences, e).ConfigureAwait(false);
                    }
                    else
                    {
                        await WriteError(modbusRtu, client, readOnlySequences, e).ConfigureAwait(false);
                    }
                }
                else
                {
                    //写入内存区
                    var result = ModbusRequest(modbusRequest, false);
                    if (result.IsSuccess)
                    {
                        await WriteSuccess(modbusRtu, client, readOnlySequences, e).ConfigureAwait(false);
                    }
                    else
                    {
                        await WriteError(modbusRtu, client, readOnlySequences, e).ConfigureAwait(false);
                    }
                }
            }
            else if (f == 6 || f == 16)
            {
                //写入寄存器
                if (WriteData != null)
                {
                    var modbusAddress = new ModbusAddress(modbusRequest) { WriteFunctionCode = modbusRequest.FunctionCode, FunctionCode = 3 };
                    if ((await WriteData(modbusAddress, ThingsGatewayBitConverter, client).ConfigureAwait(false)).IsSuccess)
                    {
                        if (IsWriteMemory)
                        {
                            var result = ModbusRequest(modbusRequest, false);
                            if (result.IsSuccess)
                                await WriteSuccess(modbusRtu, client, readOnlySequences, e).ConfigureAwait(false);
                            else
                                await WriteError(modbusRtu, client, readOnlySequences, e).ConfigureAwait(false);
                        }
                        else
                            await WriteSuccess(modbusRtu, client, readOnlySequences, e).ConfigureAwait(false);
                    }
                    else
                    {
                        await WriteError(modbusRtu, client, readOnlySequences, e).ConfigureAwait(false);
                    }
                }
                else
                {
                    var result = ModbusRequest(modbusRequest, false);
                    if (result.IsSuccess)
                    {
                        await WriteSuccess(modbusRtu, client, readOnlySequences, e).ConfigureAwait(false);
                    }
                    else
                    {
                        await WriteError(modbusRtu, client, readOnlySequences, e).ConfigureAwait(false);
                    }
                }
            }
        }
    }

    private async Task ReturnData(IClientChannel client, ReadOnlyMemory<byte> sendData, ReceivedDataEventArgs e)
    {
        if (SendDelayTime > 0)
            await Task.Delay(SendDelayTime).ConfigureAwait(false);
        if (client is IUdpClientSender udpClientSender)
            await udpClientSender.SendAsync(((UdpReceivedDataEventArgs)e).EndPoint, sendData).ConfigureAwait(false);
        else
            await client.SendAsync(sendData, client.ClosedToken).ConfigureAwait(false);
    }

    private async Task WriteError(bool modbusRtu, IClientChannel client, ReadOnlySequence<byte> bytes, ReceivedDataEventArgs e)
    {
        ValueByteBlock byteBlock = new(20);
        try
        {
            if (modbusRtu)
            {
                ByteBlockExtension.Write(ref byteBlock, bytes.Slice(0, 2));
                WriterExtension.WriteValue(ref byteBlock, (byte)1);
                byteBlock.Write(CRC16Utils.Crc16Only(byteBlock.Span));
                ByteBlockExtension.WriteBackValue(ref byteBlock, (byte)(byteBlock.Span[1] + 128), EndianType.Big, 1);
            }
            else
            {
                ByteBlockExtension.Write(ref byteBlock, bytes.Slice(0, 8));
                WriterExtension.WriteValue(ref byteBlock, (byte)1);
                ByteBlockExtension.WriteBackValue(ref byteBlock, (byte)(byteBlock.Length - 6), EndianType.Big, 5);
                ByteBlockExtension.WriteBackValue(ref byteBlock, (byte)(byteBlock.Span[7] + 128), EndianType.Big, 7);
            }
            await ReturnData(client, byteBlock.Memory, e).ConfigureAwait(false);
        }
        finally
        {
            byteBlock.SafeDispose();
        }
    }

    private async Task WriteSuccess(bool modbusRtu, IClientChannel client, ReadOnlySequence<byte> bytes, ReceivedDataEventArgs e)
    {
        ValueByteBlock byteBlock = new(20);
        try
        {
            if (modbusRtu)
            {
                ByteBlockExtension.Write(ref byteBlock, bytes.Slice(0, 6));
                byteBlock.Write(CRC16Utils.Crc16Only(byteBlock.Span));
            }
            else
            {
                ByteBlockExtension.Write(ref byteBlock, bytes.Slice(0, 12));
                ByteBlockExtension.WriteBackValue(ref byteBlock, (byte)(byteBlock.Length - 6), EndianType.Big, 5);
            }
            await ReturnData(client, byteBlock.Memory, e).ConfigureAwait(false);
        }
        finally
        {
            byteBlock.SafeDispose();
        }
    }

    #endregion 核心
}
