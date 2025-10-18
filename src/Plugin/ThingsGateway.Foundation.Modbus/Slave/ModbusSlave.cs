//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using PooledAwait;

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
    private ConcurrentDictionary<int, ByteBlock> ModbusServer01ByteBlocks = new();

    /// <summary>
    /// 开关输入
    /// </summary>
    private ConcurrentDictionary<int, ByteBlock> ModbusServer02ByteBlocks = new();

    /// <summary>
    /// 输入寄存器
    /// </summary>
    private ConcurrentDictionary<int, ByteBlock> ModbusServer03ByteBlocks = new();

    /// <summary>
    /// 保持寄存器
    /// </summary>
    private ConcurrentDictionary<int, ByteBlock> ModbusServer04ByteBlocks = new();

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
    protected override void SetChannel()
    {
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
        if (ModbusServer01ByteBlocks.ContainsKey(mAddress.Station))
            return;
        else
            ModbusServer01ByteBlocks.GetOrAdd(mAddress.Station, a =>
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

        if (ModbusServer02ByteBlocks.ContainsKey(mAddress.Station))
            return;
        else
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

        if (ModbusServer03ByteBlocks.ContainsKey(mAddress.Station))
            return;
        else
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

        if (ModbusServer04ByteBlocks.ContainsKey(mAddress.Station))
            return;
        else
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

            ModbusServer01ByteBlocks.TryGetValue(mAddress.Station, out var ModbusServer01ByteBlock);
            ModbusServer02ByteBlocks.TryGetValue(mAddress.Station, out var ModbusServer02ByteBlock);
            ModbusServer03ByteBlocks.TryGetValue(mAddress.Station, out var ModbusServer03ByteBlock);
            ModbusServer04ByteBlocks.TryGetValue(mAddress.Station, out var ModbusServer04ByteBlock);
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
    public override ValueTask<OperResult> WriteAsync(string address, ReadOnlyMemory<byte> value, DataTypeEnum dataType, CancellationToken cancellationToken = default)
    {
        try
        {
            var mAddress = GetModbusAddress(address, Station);
            mAddress.SlaveWriteDatas = new(value);
            return EasyValueTask.FromResult<OperResult>(ModbusRequest(mAddress, false, cancellationToken));
        }
        catch (Exception ex)
        {
            return EasyValueTask.FromResult(new OperResult(ex));
        }
    }

    /// <inheritdoc/>
    public override ValueTask<OperResult> WriteAsync(string address, ReadOnlyMemory<bool> value, CancellationToken cancellationToken = default)
    {
        try
        {
            var mAddress = GetModbusAddress(address, Station);
            if (mAddress.IsBitFunction)
            {
                mAddress.SlaveWriteDatas = new(value.Span.BoolToByte());
                ModbusRequest(mAddress, false, cancellationToken);
                return EasyValueTask.FromResult(OperResult.Success);
            }
            else
            {
                if (mAddress.BitIndex < 16)
                {
                    mAddress.Length = 2;
                    var readData = ModbusRequest(mAddress, true, cancellationToken);
                    if (!readData.IsSuccess) return EasyValueTask.FromResult<OperResult>(readData);
                    var writeData = TouchSocketBitConverter.BigEndian.To<ushort>(readData.Content.Span);
                    var span = value.Span;
                    for (int i = 0; i < value.Length; i++)
                    {
                        writeData = writeData.SetBit(mAddress.BitIndex.Value + i, span[i]);
                    }
                    mAddress.SlaveWriteDatas = new(ThingsGatewayBitConverter.GetBytes(writeData));
                    ModbusRequest(mAddress, false, cancellationToken);
                    return EasyValueTask.FromResult<OperResult>(OperResult.Success);
                }
                else
                {
                    return EasyValueTask.FromResult<OperResult>(new OperResult(string.Format(AppResource.ValueOverlimit, nameof(mAddress.BitIndex), 16)));
                }
            }
        }
        catch (Exception ex)
        {
            return EasyValueTask.FromResult<OperResult>(new OperResult(ex));
        }
    }
    protected override ValueTask ChannelReceived(IClientChannel client, ReceivedDataEventArgs e, bool last)
    {
        return HandleChannelReceivedAsync(client, e, last);
    }

    private ValueTask HandleChannelReceivedAsync(IClientChannel client, ReceivedDataEventArgs e, bool last)
    {
        return HandleChannelReceivedAsync(this, client, e);

        static async PooledValueTask HandleChannelReceivedAsync(ModbusSlave @this, IClientChannel client, ReceivedDataEventArgs e)
        {
            if (!TryParseRequest(e.RequestInfo, out var modbusRequest, out var sequences, out var modbusRtu))
                return;

            if (!@this.MulStation && modbusRequest.Station != @this.Station)
                return;

            var function = NormalizeFunctionCode(modbusRequest.FunctionCode);

            if (function <= 4)
                await @this.HandleReadRequestAsync(client, e, modbusRequest, sequences, modbusRtu).ConfigureAwait(false);
            else
                await @this.HandleWriteRequestAsync(client, e, modbusRequest, sequences, modbusRtu, function).ConfigureAwait(false);
            return;
        }
    }

    private static bool TryParseRequest(object requestInfo, out ModbusRequest modbusRequest, out ReadOnlySequence<byte> sequences, out bool modbusRtu)
    {
        modbusRequest = default;
        sequences = default;
        modbusRtu = false;

        switch (requestInfo)
        {
            case ModbusRtuSlaveMessage rtuMsg when rtuMsg.IsSuccess:
                modbusRequest = rtuMsg.Request;
                sequences = rtuMsg.Sequences;
                modbusRtu = true;
                return true;

            case ModbusTcpSlaveMessage tcpMsg when tcpMsg.IsSuccess:
                modbusRequest = tcpMsg.Request;
                sequences = tcpMsg.Sequences;
                modbusRtu = false;
                return true;

            default:
                return false;
        }
    }

    private static byte NormalizeFunctionCode(byte funcCode)
        => funcCode > 0x30 ? (byte)(funcCode - 0x30) : funcCode;

    private Task HandleReadRequestAsync(
        IClientChannel client,
        ReceivedDataEventArgs e,
        ModbusRequest modbusRequest,
        ReadOnlySequence<byte> sequences,
        bool modbusRtu)
    {
        var data = ModbusRequest(modbusRequest, true);
        if (!data.IsSuccess)
        {
            return WriteError(modbusRtu, client, sequences, e);
        }

        return Write(this, client, e, modbusRequest, sequences, modbusRtu, data);

        static async PooledTask Write(ModbusSlave @this, IClientChannel client, ReceivedDataEventArgs e, ModbusRequest modbusRequest, ReadOnlySequence<byte> sequences, bool modbusRtu, OperResult<ReadOnlyMemory<byte>> data)
        {
            ValueByteBlock byteBlock = new(1024);
            try
            {
                WriteReadResponse(modbusRequest, sequences, data.Content, ref byteBlock, modbusRtu);
                await @this.ReturnData(client, byteBlock.Memory, e).ConfigureAwait(false);
            }
            catch
            {
                await @this.WriteError(modbusRtu, client, sequences, e).ConfigureAwait(false);
            }
            finally
            {
                byteBlock.SafeDispose();
            }
        }
    }

    private Task HandleWriteRequestAsync(
        IClientChannel client,
        ReceivedDataEventArgs e,
        ModbusRequest modbusRequest,
        ReadOnlySequence<byte> sequences,
        bool modbusRtu,
        byte f)
    {
        return HandleWriteRequestAsync(this, client, e, modbusRequest, sequences, modbusRtu, f);


        static async PooledTask HandleWriteRequestAsync(ModbusSlave @this, IClientChannel client, ReceivedDataEventArgs e, ModbusRequest modbusRequest, ReadOnlySequence<byte> sequences, bool modbusRtu, byte f)
        {
            var modbusAddress = new ModbusAddress(modbusRequest);
            bool isSuccess;

            switch (f)
            {
                case 5:
                case 15:
                    modbusAddress.WriteFunctionCode = modbusRequest.FunctionCode;
                    modbusAddress.FunctionCode = 1;
                    isSuccess = await @this.HandleWriteCoreAsync(modbusAddress, client, modbusRequest).ConfigureAwait(false);
                    break;

                case 6:
                case 16:
                    modbusAddress.WriteFunctionCode = modbusRequest.FunctionCode;
                    modbusAddress.FunctionCode = 3;
                    isSuccess = await @this.HandleWriteCoreAsync(modbusAddress, client, modbusRequest).ConfigureAwait(false);
                    break;

                default:
                    return;
            }

            if (isSuccess)
                await @this.WriteSuccess(modbusRtu, client, sequences, e).ConfigureAwait(false);
            else
                await @this.WriteError(modbusRtu, client, sequences, e).ConfigureAwait(false);
            return;
        }
    }

    private Task<bool> HandleWriteCoreAsync(ModbusAddress address, IClientChannel client, ModbusRequest modbusRequest)
    {
        return HandleWriteCoreAsync(this, address, client, modbusRequest);

        static async PooledTask<bool> HandleWriteCoreAsync(ModbusSlave @this, ModbusAddress address, IClientChannel client, ModbusRequest modbusRequest)
        {
            if (@this.WriteData != null)
            {
                var result = await @this.WriteData(address, @this.ThingsGatewayBitConverter, client).ConfigureAwait(false);
                if (!result.IsSuccess) return false;
            }

            if (@this.IsWriteMemory)
            {
                var memResult = @this.ModbusRequest(modbusRequest, false);
                return memResult.IsSuccess;
            }

            return true;
        }
    }

    private static void WriteReadResponse(
        ModbusRequest modbusRequest,
        ReadOnlySequence<byte> sequences,
        ReadOnlyMemory<byte> content,
        ref ValueByteBlock byteBlock,
        bool modbusRtu)
    {
        if (modbusRtu)
            ByteBlockExtension.Write(ref byteBlock, sequences.Slice(0, 2));
        else
            ByteBlockExtension.Write(ref byteBlock, sequences.Slice(0, 8));

        if (modbusRequest.IsBitFunction)
        {
            var bitdata = content.Span.ByteToBool().AsSpan().BoolArrayToByte();
            var len = (int)Math.Ceiling(modbusRequest.Length / 8.0);
            var bitWriteData = bitdata.AsMemory().Slice(0, len);
            WriterExtension.WriteValue(ref byteBlock, (byte)bitWriteData.Length);
            byteBlock.Write(bitWriteData.Span);
        }
        else
        {
            WriterExtension.WriteValue(ref byteBlock, (byte)content.Length);
            byteBlock.Write(content.Span);
        }

        if (modbusRtu)
            byteBlock.Write(CRC16Utils.Crc16Only(byteBlock.Memory.Span));
        else
            ByteBlockExtension.WriteBackValue(ref byteBlock, (byte)(byteBlock.Length - 6), EndianType.Big, 5);
    }

    private Task ReturnData(IClientChannel client, ReadOnlyMemory<byte> sendData, ReceivedDataEventArgs e)
    {
        return ReturnData(SendDelayTime, client, sendData, e);

        static async PooledTask ReturnData(int deley, IClientChannel client, ReadOnlyMemory<byte> sendData, ReceivedDataEventArgs e)
        {
            if (deley > 0)
                await Task.Delay(deley).ConfigureAwait(false);
            if (client is IUdpClientSender udpClientSender)
                await udpClientSender.SendAsync(((UdpReceivedDataEventArgs)e).EndPoint, sendData).ConfigureAwait(false);
            else
                await client.SendAsync(sendData, client.ClosedToken).ConfigureAwait(false);
        }
    }

    private Task WriteError(bool modbusRtu, IClientChannel client, ReadOnlySequence<byte> bytes, ReceivedDataEventArgs e)
    {
        return WriteError(this, modbusRtu, client, bytes, e);

        static async PooledTask WriteError(ModbusSlave @this, bool modbusRtu, IClientChannel client, ReadOnlySequence<byte> bytes, ReceivedDataEventArgs e)
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
                await @this.ReturnData(client, byteBlock.Memory, e).ConfigureAwait(false);
            }
            finally
            {
                byteBlock.SafeDispose();
            }
        }
    }

    private Task WriteSuccess(bool modbusRtu, IClientChannel client, ReadOnlySequence<byte> bytes, ReceivedDataEventArgs e)
    {
        return WriteSuccess(this, modbusRtu, client, bytes, e);

        static async PooledTask WriteSuccess(ModbusSlave @this, bool modbusRtu, IClientChannel client, ReadOnlySequence<byte> bytes, ReceivedDataEventArgs e)
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
                await @this.ReturnData(client, byteBlock.Memory, e).ConfigureAwait(false);
            }
            finally
            {
                byteBlock.SafeDispose();
            }
        }
    }

    #endregion 核心
}
