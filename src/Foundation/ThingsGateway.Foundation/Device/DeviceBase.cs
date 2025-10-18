//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Newtonsoft.Json.Linq;

using PooledAwait;

using System.Net;

using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Foundation.Extension.String;
using ThingsGateway.NewLife;
using ThingsGateway.NewLife.Collections;
using ThingsGateway.NewLife.Extension;

namespace ThingsGateway.Foundation;

/// <summary>
/// 协议基类
/// </summary>
public abstract class DeviceBase : AsyncAndSyncDisposableObject, IDevice
{
    /// <inheritdoc/>
    public IChannel Channel { get; private set; }

    public virtual bool SupportMultipleDevice()
    {
        return true;
    }

    /// <inheritdoc/>
    public virtual void InitChannel(IChannel channel, ILog? deviceLog = default)
    {
        if (channel == null) throw new ArgumentNullException(nameof(channel));
        if (channel.Collects.Contains(this))
            return;
        Channel = channel;
        _deviceLogger = deviceLog;
        lock (channel)
        {
            if (channel.Collects.Contains(this))
                return;
            if (channel.Collects.Count > 0)
            {
                //var device = channel.Collects.First();
                //if (device.GetType() != GetType())
                //    throw new InvalidOperationException("The channel already exists in the device of another type");

                if (!SupportMultipleDevice())
                    throw new InvalidOperationException("The proactive response device does not support multiple devices");
            }

            if (channel.Collects.Count == 0)
            {
                channel.Config.ConfigurePlugins(ConfigurePlugins(channel.Config));

                if (Channel is IClientChannel clientChannel)
                {
                    if (clientChannel.ChannelType == ChannelTypeEnum.UdpSession)
                    {
                        channel.Config.SetUdpDataHandlingAdapter(() =>
                        {
                            var adapter = GetDataAdapter() as UdpDataHandlingAdapter;
                            return adapter;
                        });
                    }
                    else
                    {
                        channel.Config.SetSerialDataHandlingAdapter(() =>
                        {
                            var adapter = GetDataAdapter() as SingleStreamDataHandlingAdapter;
                            return adapter;
                        });
                        channel.Config.SetTcpDataHandlingAdapter(() =>
                        {
                            var adapter = GetDataAdapter() as SingleStreamDataHandlingAdapter;
                            return adapter;
                        });
                    }
                }
                else if (Channel is ITcpServiceChannel serviceChannel)
                {
                    channel.Config.SetTcpDataHandlingAdapter(() =>
                    {
                        var adapter = GetDataAdapter() as SingleStreamDataHandlingAdapter;
                        return adapter;
                    });
                }
            }

            channel.Collects.Add(this);
            Channel.Starting.Add(ChannelStarting);
            Channel.Stoped.Add(ChannelStoped);
            Channel.Stoping.Add(ChannelStoping);
            Channel.Started.Add(ChannelStarted);
            Channel.ChannelReceived.Add(ChannelReceived);

            SetChannel();
        }
    }

    protected virtual void SetChannel()
    {
        Channel.ChannelOptions.MaxConcurrentCount = 1;
    }

    /// <inheritdoc/>
    ~DeviceBase()
    {
        this.SafeDispose();
    }

    #region

    private ILog? _deviceLogger;

    /// <inheritdoc/>
    public virtual ILog? Logger
    {
        get
        {
            return _deviceLogger ?? Channel?.Logger;
        }
    }

    /// <inheritdoc/>
    public virtual int RegisterByteLength { get; protected set; } = 1;

    /// <inheritdoc/>
    public virtual IThingsGatewayBitConverter ThingsGatewayBitConverter { get; } = new ThingsGatewayBitConverter();

    /// <inheritdoc/>
    public bool OnLine => Channel.Online;

    #endregion

    #region 属性

    /// <inheritdoc/>
    public virtual int SendDelayTime { get; set; }

    /// <inheritdoc/>
    public virtual int Timeout { get; set; } = 3000;

    /// <summary>
    /// <inheritdoc cref="IThingsGatewayBitConverter.IsStringReverseByteWord"/>
    /// </summary>
    public bool IsStringReverseByteWord
    {
        get
        {
            return ThingsGatewayBitConverter.IsStringReverseByteWord;
        }
        set
        {
            ThingsGatewayBitConverter.IsStringReverseByteWord = value;
        }
    }

    /// <inheritdoc/>
    public virtual DataFormatEnum DataFormat
    {
        get => ThingsGatewayBitConverter.DataFormat;
        set => ThingsGatewayBitConverter.DataFormat = value;
    }

    #endregion 属性

    #region 适配器

    /// <inheritdoc/>
    public abstract DataHandlingAdapter GetDataAdapter();

    /// <summary>
    /// 通道连接成功时，如果通道存在其他设备并且不希望其他设备处理时，返回true
    /// </summary>
    protected virtual ValueTask<bool> ChannelStarted(IClientChannel channel, bool last)
    {
        return EasyValueTask.FromResult(true);
    }

    /// <summary>
    /// 通道断开连接前，如果通道存在其他设备并且不希望其他设备处理时，返回true
    /// </summary>
    protected virtual ValueTask<bool> ChannelStoping(IClientChannel channel, bool last)
    {
        return EasyValueTask.FromResult(true);
    }

    /// <summary>
    /// 通道断开连接后，如果通道存在其他设备并且不希望其他设备处理时，返回true
    /// </summary>
    protected ValueTask<bool> ChannelStoped(IClientChannel channel, bool last)
    {
        try
        {
            channel.WaitHandlePool.CancelAll();
        }
        catch
        {
        }

        return EasyValueTask.FromResult(true);
    }

    /// <summary>
    /// 通道即将连接成功时，会设置适配器，如果通道存在其他设备并且不希望其他设备处理时，返回true
    /// </summary>
    protected virtual ValueTask<bool> ChannelStarting(IClientChannel channel, bool last)
    {
        channel.SetDataHandlingAdapterLogger(Logger);
        return EasyValueTask.FromResult(true);
    }

    /// <summary>
    /// 设置适配器
    /// </summary>
    protected virtual void SetDataAdapter(IClientChannel clientChannel)
    {
        var adapter = clientChannel.ReadOnlyDataHandlingAdapter;
        if (adapter == null)
        {
            var dataHandlingAdapter = GetDataAdapter();
            clientChannel.SetDataHandlingAdapter(dataHandlingAdapter);
        }
        else
        {
            if (Channel?.Collects?.Count > 1)
            {
                var dataHandlingAdapter = GetDataAdapter();
                if (adapter.GetType() != dataHandlingAdapter.GetType())
                {
                    clientChannel.SetDataHandlingAdapter(dataHandlingAdapter);
                }
            }
        }
    }

    #endregion 适配器

    #region 变量地址解析

    /// <inheritdoc/>
    public abstract List<T> LoadSourceRead<T>(IEnumerable<IVariable> deviceVariables, int maxPack, string defaultIntervalTime) where T : IVariableSource, new();

    /// <inheritdoc/>
    public virtual string GetAddressDescription()
    {
        return AppResource.DefaultAddressDes;
    }
    /// <summary>
    /// 获取bit偏移量
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public int GetBitOffsetDefault(string address)
    {
        return GetBitOffset(address) ?? 0;
    }
    /// <summary>
    /// 获取bit偏移量
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public virtual int? GetBitOffset(string address)
    {
        int? bitIndex = null;
        if (address?.IndexOf('.') > 0)
            bitIndex = address.SplitStringByDelimiter().Last().ToInt();
        return bitIndex;
    }

    /// <inheritdoc/>
    public virtual bool BitReverse(string address)
    {
        return address?.IndexOf('.') > 0;
    }

    /// <inheritdoc/>
    public virtual int GetLength(string address, int length, int typeLength, bool isBool = false)
    {
        var result = Math.Ceiling((double)length * typeLength / RegisterByteLength);
        if (isBool && GetBitOffset(address) != null)
        {
            var data = Math.Ceiling((double)length / RegisterByteLength / 8);
            return (int)data;
        }
        else
        {
            return (int)result;
        }
    }

    #endregion 变量地址解析

    #region 设备异步返回

    /// <summary>
    /// 日志输出16进制
    /// </summary>
    public virtual bool IsHexLog { get; init; } = true;

    /// <summary>
    /// 接收,非主动发送的情况，重写实现非主从并发通讯协议，如果通道存在其他设备并且不希望其他设备处理时，设置<see cref="TouchSocketEventArgs.Handled"/> 为true
    /// </summary>
    protected virtual ValueTask ChannelReceived(IClientChannel client, ReceivedDataEventArgs e, bool last)
    {
        if (e.RequestInfo is MessageBase response)
        {
            try
            {
                if (client.WaitHandlePool.Set(response))
                {
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Logger?.LogWarning(ex, $"Response {response.Sign}");
            }
        }

        return EasyValueTask.CompletedTask;
    }
    public bool AutoConnect { get; protected set; } = true;
    /// <inheritdoc/>
    private Task SendAsync(ISendMessage sendMessage, IClientChannel channel, CancellationToken token = default)
    {
        return SendAsync(this, sendMessage, channel, token);

        static async PooledTask SendAsync(DeviceBase @this, ISendMessage sendMessage, IClientChannel channel, CancellationToken token)
        {
            if (@this.SendDelayTime != 0)
                await Task.Delay(@this.SendDelayTime, token).ConfigureAwait(false);

            if (channel is IDtuUdpSessionChannel udpSession)
            {
                EndPoint? endPoint = @this.GetUdpEndpoint();
                await udpSession.SendAsync(endPoint, sendMessage, token).ConfigureAwait(false);

            }
            else
            {
                await channel.SendAsync(sendMessage, token).ConfigureAwait(false);
            }
        }

    }

    private ValueTask BeforeSendAsync(IClientChannel channel, CancellationToken token)
    {
        SetDataAdapter(channel);
        if (AutoConnect && Channel != null && Channel?.Online != true)
        {
            return ConnectAsync(token);
        }
        else
        {
            return EasyValueTask.CompletedTask;
        }
    }

    private WaitLock connectWaitLock = new(nameof(DeviceBase));

    public ValueTask ConnectAsync(CancellationToken token)
    {
        return ConnectAsync(this, token);

        static async PooledValueTask ConnectAsync(DeviceBase @this, CancellationToken token)
        {
            if (@this.AutoConnect && @this.Channel != null && @this.Channel?.Online != true)
            {
                try
                {
                    await @this.connectWaitLock.WaitAsync(token).ConfigureAwait(false);
                    if (@this.AutoConnect && @this.Channel != null && @this.Channel?.Online != true)
                    {
                        if (@this.Channel.PluginManager == null)
                            await @this.Channel.SetupAsync(@this.Channel.Config.Clone()).ConfigureAwait(false);
                        await @this.Channel.CloseAsync().ConfigureAwait(false);
                        using var ctsTime = new CancellationTokenSource(@this.Channel.ChannelOptions.ConnectTimeout);
                        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ctsTime.Token, token);
                        await @this.Channel.ConnectAsync(cts.Token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    @this.connectWaitLock.Release();
                }
            }
        }
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> SendAsync(ISendMessage sendMessage, CancellationToken cancellationToken)
    {
        return SendAsync(this, sendMessage, cancellationToken);

        static async PooledValueTask<OperResult> SendAsync(DeviceBase @this, ISendMessage sendMessage, CancellationToken cancellationToken)
        {
            try
            {
                var channelResult = @this.GetChannel();
                if (!channelResult.IsSuccess) return new OperResult<byte[]>(channelResult);
                WaitLock? waitLock = @this.GetWaitLock(channelResult.Content);

                try
                {
                    await @this.BeforeSendAsync(channelResult.Content, cancellationToken).ConfigureAwait(false);

                    await waitLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                    channelResult.Content.SetDataHandlingAdapterLogger(@this.Logger);


                    await @this.SendAsync(sendMessage, channelResult.Content, cancellationToken).ConfigureAwait(false);
                    return OperResult.Success;
                }
                finally
                {
                    waitLock.Release();
                }
            }
            catch (Exception ex)
            {
                return new(ex);
            }
        }
    }

    /// <inheritdoc/>
    public virtual OperResult<IClientChannel> GetChannel()
    {
        if (Channel is IClientChannel clientChannel1)
            return new OperResult<IClientChannel>() { Content = clientChannel1 };

        var socketId = this is IDtu dtu1 ? dtu1.DtuId : null;

        if (string.IsNullOrWhiteSpace(socketId))
        {
            if (Channel is IClientChannel clientChannel)
                return new OperResult<IClientChannel>() { Content = clientChannel };
            else
                return new OperResult<IClientChannel>("The communication link cannot be obtained, DtuId must be set!");
        }


        if (Channel is ITcpServiceChannel serviceChannel)
        {
            if (serviceChannel.TryGetClient($"ID={socketId}", out var client))
            {
                return new OperResult<IClientChannel>() { Content = client };
            }
            else
            {
                if (serviceChannel.TryGetClient($"ID={socketId}", out var client1))
                {
                    return new OperResult<IClientChannel>() { Content = client1 };
                }
                return (new OperResult<IClientChannel>(string.Format(AppResource.DtuNoConnectedWaining, socketId)));
            }
        }
        else
        {
            if (Channel is IClientChannel clientChannel)
                return new OperResult<IClientChannel>() { Content = clientChannel };
            else
                return new OperResult<IClientChannel>("The communication link cannot be obtained!");
        }
    }

    /// <inheritdoc/>
    public virtual EndPoint GetUdpEndpoint()
    {
        if (Channel is IDtuUdpSessionChannel udpSessionChannel)
        {
            var socketId = this is IDtu dtu1 ? dtu1.DtuId : null;
            if (string.IsNullOrWhiteSpace(socketId))
                return udpSessionChannel.DefaultEndpoint;

            {
                if (udpSessionChannel.TryGetEndPoint($"ID={socketId}", out var endPoint))
                {
                    return endPoint;
                }
                else
                {
                    if (udpSessionChannel.TryGetEndPoint($"ID={socketId}", out var endPoint1))
                    {
                        return endPoint1;
                    }
                    throw new Exception(string.Format(AppResource.DtuNoConnectedWaining, socketId));
                }
            }
        }

        return null;
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult<ReadOnlyMemory<byte>>> SendThenReturnAsync(ISendMessage sendMessage, CancellationToken cancellationToken = default)
    {
        var channelResult = GetChannel();
        if (!channelResult.IsSuccess) return EasyValueTask.FromResult(new OperResult<ReadOnlyMemory<byte>>(channelResult));
        return SendThenReturnAsync(sendMessage, channelResult.Content, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult<ReadOnlyMemory<byte>>> SendThenReturnAsync(ISendMessage sendMessage, IClientChannel channel, CancellationToken cancellationToken = default)
    {
        return SendThenReturn(this, sendMessage, channel, cancellationToken);

        static async PooledValueTask<OperResult<ReadOnlyMemory<byte>>> SendThenReturn(DeviceBase @this, ISendMessage sendMessage, IClientChannel channel, CancellationToken cancellationToken)
        {
            try
            {
                var result = await @this.SendThenReturnMessageAsync(sendMessage, channel, cancellationToken).ConfigureAwait(false);
                return new OperResult<ReadOnlyMemory<byte>>(result) { Content = result.Content };
            }
            catch (Exception ex)
            {
                return new(ex);
            }
        }
    }

    /// <inheritdoc/>
    protected virtual ValueTask<MessageBase> SendThenReturnMessageAsync(ISendMessage sendMessage, CancellationToken cancellationToken = default)
    {
        var channelResult = GetChannel();
        if (!channelResult.IsSuccess) return EasyValueTask.FromResult(new MessageBase(channelResult));
        return SendThenReturnMessageAsync(sendMessage, channelResult.Content, cancellationToken);
    }

    /// <inheritdoc/>
    protected virtual ValueTask<MessageBase> SendThenReturnMessageAsync(ISendMessage command, IClientChannel clientChannel, CancellationToken cancellationToken = default)
    {
        return GetResponsedDataAsync(command, clientChannel, Timeout, cancellationToken);
    }

    private ObjectPoolLock<ReusableCancellationTokenSource> _reusableTimeouts = new();

    /// <summary>
    /// 发送并等待数据
    /// </summary>
    protected ValueTask<MessageBase> GetResponsedDataAsync(
        ISendMessage command,
        IClientChannel clientChannel,
        int timeout = 3000,
        CancellationToken cancellationToken = default)
    {
        return GetResponsedDataAsync(this, command, clientChannel, timeout, cancellationToken);

        static async PooledValueTask<MessageBase> GetResponsedDataAsync(DeviceBase @this, ISendMessage command, IClientChannel clientChannel, int timeout, CancellationToken cancellationToken)
        {
            var waitData = clientChannel.WaitHandlePool.GetWaitDataAsync(out var sign);
            command.Sign = sign;
            WaitLock? waitLock = null;

            try
            {
                await @this.BeforeSendAsync(clientChannel, cancellationToken).ConfigureAwait(false);

                waitLock = @this.GetWaitLock(clientChannel);

                await waitLock.WaitAsync(cancellationToken).ConfigureAwait(false);

                clientChannel.SetDataHandlingAdapterLogger(@this.Logger);

                await @this.SendAsync(command, clientChannel, cancellationToken).ConfigureAwait(false);

                if (waitData.Status == WaitDataStatus.Success)
                    return waitData.CompletedData;

                var reusableTimeout = @this._reusableTimeouts.Get();
                try
                {

                    var ctsToken = reusableTimeout.GetTokenSource(timeout, cancellationToken, @this.Channel.ClosedToken);
                    await waitData.WaitAsync(ctsToken).ConfigureAwait(false);

                }
                catch (OperationCanceledException)
                {
                    return reusableTimeout.TimeoutStatus
                        ? new MessageBase(new TimeoutException()) { ErrorMessage = $"Timeout, sign: {sign}" }
                        : new MessageBase(new OperationCanceledException());
                }
                catch (Exception ex)
                {
                    return new MessageBase(ex);
                }
                finally
                {
                    reusableTimeout.Set();
                    @this._reusableTimeouts.Return(reusableTimeout);
                }

                if (waitData.Status == WaitDataStatus.Success)
                {
                    return waitData.CompletedData;
                }
                else
                {
                    var operResult = waitData.Check(reusableTimeout.TimeoutStatus);
                    if (waitData.CompletedData != null)
                    {
                        waitData.CompletedData.ErrorMessage = $"{operResult.ErrorMessage}, sign: {sign}";
                        return waitData.CompletedData;
                    }
                    else
                    {
                        return new MessageBase(new OperationCanceledException());
                    }

                    //return new MessageBase(operResult) { ErrorMessage = $"{operResult.ErrorMessage}, sign: {sign}" };
                }
            }
            catch (Exception ex)
            {
                return new MessageBase(ex);
            }
            finally
            {
                waitLock?.Release();
                waitData?.SafeDispose();

            }
        }
    }


    private WaitLock GetWaitLock(IClientChannel clientChannel)
    {
        WaitLock? waitLock = null;
        if (clientChannel is IDtuUdpSessionChannel udpSessionChannel)
        {
            waitLock = udpSessionChannel.GetLock(this is IDtu dtu1 ? dtu1.DtuId : null);
        }
        waitLock ??= clientChannel.GetLock(null);
        return waitLock;
    }

    #endregion 设备异步返回

    #region 动态类型读写

    /// <inheritdoc/>
    public virtual async ValueTask<IOperResult<Array>> ReadArrayAsync(string address, int length, DataTypeEnum dataType, CancellationToken cancellationToken = default)
    {
        return dataType switch
        {
            DataTypeEnum.String => await ReadStringAsync(address, length, cancellationToken: cancellationToken).ConfigureAwait(false),
            DataTypeEnum.Boolean => await ReadBooleanAsync(address, length, cancellationToken: cancellationToken).ConfigureAwait(false),
            DataTypeEnum.Byte => (await ReadAsync(address, length, cancellationToken).ConfigureAwait(false)).OperResultFrom(a => a.ToArray()),
            DataTypeEnum.Int16 => await ReadInt16Async(address, length, cancellationToken: cancellationToken).ConfigureAwait(false),
            DataTypeEnum.UInt16 => await ReadUInt16Async(address, length, cancellationToken: cancellationToken).ConfigureAwait(false),
            DataTypeEnum.Int32 => await ReadInt32Async(address, length, cancellationToken: cancellationToken).ConfigureAwait(false),
            DataTypeEnum.UInt32 => await ReadUInt32Async(address, length, cancellationToken: cancellationToken).ConfigureAwait(false),
            DataTypeEnum.Int64 => await ReadInt64Async(address, length, cancellationToken: cancellationToken).ConfigureAwait(false),
            DataTypeEnum.UInt64 => await ReadUInt64Async(address, length, cancellationToken: cancellationToken).ConfigureAwait(false),
            DataTypeEnum.Float => await ReadSingleAsync(address, length, cancellationToken: cancellationToken).ConfigureAwait(false),
            DataTypeEnum.Double => await ReadDoubleAsync(address, length, cancellationToken: cancellationToken).ConfigureAwait(false),
            DataTypeEnum.Decimal => await ReadDecimalAsync(address, length, cancellationToken: cancellationToken).ConfigureAwait(false),
            _ => new OperResult<Array>(string.Format(AppResource.DataTypeNotSupported, dataType)),
        };
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteJTokenAsync(string address, JToken value, DataTypeEnum dataType, CancellationToken cancellationToken = default)
    {
        return WriteJTokenAsync(this, address, value, dataType, cancellationToken);

        static async PooledValueTask<OperResult> WriteJTokenAsync(DeviceBase @this, string address, JToken value, DataTypeEnum dataType, CancellationToken cancellationToken)
        {
            try
            {
                var bitConverter = @this.ThingsGatewayBitConverter.GetTransByAddress(address);
                if (value is JArray jArray)
                {
                    return dataType switch
                    {
                        DataTypeEnum.String => await @this.WriteAsync(address, jArray.ToObject<String[]>().AsMemory(), cancellationToken: cancellationToken).ConfigureAwait(false),
                        DataTypeEnum.Boolean => await @this.WriteAsync(address, jArray.ToObject<Boolean[]>().AsMemory(), cancellationToken).ConfigureAwait(false),
                        DataTypeEnum.Byte => await @this.WriteAsync(address, jArray.ToObject<Byte[]>().AsMemory(), dataType, cancellationToken).ConfigureAwait(false),
                        DataTypeEnum.Int16 => await @this.WriteAsync(address, jArray.ToObject<Int16[]>().AsMemory(), cancellationToken: cancellationToken).ConfigureAwait(false),
                        DataTypeEnum.UInt16 => await @this.WriteAsync(address, jArray.ToObject<UInt16[]>().AsMemory(), cancellationToken: cancellationToken).ConfigureAwait(false),
                        DataTypeEnum.Int32 => await @this.WriteAsync(address, jArray.ToObject<Int32[]>().AsMemory(), cancellationToken: cancellationToken).ConfigureAwait(false),
                        DataTypeEnum.UInt32 => await @this.WriteAsync(address, jArray.ToObject<UInt32[]>().AsMemory(), cancellationToken: cancellationToken).ConfigureAwait(false),
                        DataTypeEnum.Int64 => await @this.WriteAsync(address, jArray.ToObject<Int64[]>().AsMemory(), cancellationToken: cancellationToken).ConfigureAwait(false),
                        DataTypeEnum.UInt64 => await @this.WriteAsync(address, jArray.ToObject<UInt64[]>().AsMemory(), cancellationToken: cancellationToken).ConfigureAwait(false),
                        DataTypeEnum.Float => await @this.WriteAsync(address, jArray.ToObject<Single[]>().AsMemory(), cancellationToken: cancellationToken).ConfigureAwait(false),
                        DataTypeEnum.Double => await @this.WriteAsync(address, jArray.ToObject<Double[]>().AsMemory(), cancellationToken: cancellationToken).ConfigureAwait(false),
                        DataTypeEnum.Decimal => await @this.WriteAsync(address, jArray.ToObject<Decimal[]>().AsMemory(), cancellationToken: cancellationToken).ConfigureAwait(false),
                        _ => new OperResult(string.Format(AppResource.DataTypeNotSupported, dataType)),
                    };
                }
                else
                {
                    return dataType switch
                    {
                        DataTypeEnum.String => await @this.WriteAsync(address, value.ToObject<String>(), bitConverter, cancellationToken).ConfigureAwait(false),
                        DataTypeEnum.Boolean => await @this.WriteAsync(address, value.ToObject<Boolean>(), bitConverter, cancellationToken).ConfigureAwait(false),
                        DataTypeEnum.Byte => await @this.WriteAsync(address, value.ToObject<Byte>(), bitConverter, cancellationToken).ConfigureAwait(false),
                        DataTypeEnum.Int16 => await @this.WriteAsync(address, value.ToObject<Int16>(), bitConverter, cancellationToken).ConfigureAwait(false),
                        DataTypeEnum.UInt16 => await @this.WriteAsync(address, value.ToObject<UInt16>(), bitConverter, cancellationToken).ConfigureAwait(false),
                        DataTypeEnum.Int32 => await @this.WriteAsync(address, value.ToObject<Int32>(), bitConverter, cancellationToken).ConfigureAwait(false),
                        DataTypeEnum.UInt32 => await @this.WriteAsync(address, value.ToObject<UInt32>(), bitConverter, cancellationToken).ConfigureAwait(false),
                        DataTypeEnum.Int64 => await @this.WriteAsync(address, value.ToObject<Int64>(), bitConverter, cancellationToken).ConfigureAwait(false),
                        DataTypeEnum.UInt64 => await @this.WriteAsync(address, value.ToObject<UInt64>(), bitConverter, cancellationToken).ConfigureAwait(false),
                        DataTypeEnum.Float => await @this.WriteAsync(address, value.ToObject<Single>(), bitConverter, cancellationToken).ConfigureAwait(false),
                        DataTypeEnum.Double => await @this.WriteAsync(address, value.ToObject<Double>(), bitConverter, cancellationToken).ConfigureAwait(false),
                        DataTypeEnum.Decimal => await @this.WriteAsync(address, value.ToObject<Decimal>(), bitConverter, cancellationToken).ConfigureAwait(false),
                        _ => new OperResult(string.Format(AppResource.DataTypeNotSupported, dataType)),
                    };
                }
            }
            catch (Exception ex)
            {
                return new OperResult(ex);
            }
        }
    }

    #endregion 动态类型读写

    #region 读取

    /// <inheritdoc/>
    public abstract ValueTask<OperResult<ReadOnlyMemory<byte>>> ReadAsync(string address, int length, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public virtual async ValueTask<OperResult<bool[]>> ReadBooleanAsync(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);

        var result = await ReadAsync(address, GetLength(address, length, RegisterByteLength, true), cancellationToken).ConfigureAwait(false);

        return result.OperResultFrom(() => bitConverter.ToBoolean(result.Content.Span, GetBitOffsetDefault(address), length, BitReverse(address)));
    }

    /// <inheritdoc/>
    public virtual async ValueTask<OperResult<Int16[]>> ReadInt16Async(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);
        var result = await ReadAsync(address, GetLength(address, length, 2), cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => bitConverter.ToInt16(result.Content.Span, 0, length));
    }

    /// <inheritdoc/>
    public virtual async ValueTask<OperResult<UInt16[]>> ReadUInt16Async(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);
        var result = await ReadAsync(address, GetLength(address, length, 2), cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => bitConverter.ToUInt16(result.Content.Span, 0, length));
    }

    /// <inheritdoc/>
    public virtual async ValueTask<OperResult<Int32[]>> ReadInt32Async(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);
        var result = await ReadAsync(address, GetLength(address, length, 4), cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => bitConverter.ToInt32(result.Content.Span, 0, length));
    }

    /// <inheritdoc/>
    public virtual async ValueTask<OperResult<UInt32[]>> ReadUInt32Async(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);
        var result = await ReadAsync(address, GetLength(address, length, 4), cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => bitConverter.ToUInt32(result.Content.Span, 0, length));
    }

    /// <inheritdoc/>
    public virtual async ValueTask<OperResult<Int64[]>> ReadInt64Async(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);
        var result = await ReadAsync(address, GetLength(address, length, 8), cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => bitConverter.ToInt64(result.Content.Span, 0, length));
    }

    /// <inheritdoc/>
    public virtual async ValueTask<OperResult<UInt64[]>> ReadUInt64Async(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);
        var result = await ReadAsync(address, GetLength(address, length, 8), cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => bitConverter.ToUInt64(result.Content.Span, 0, length));
    }

    /// <inheritdoc/>
    public virtual async ValueTask<OperResult<Single[]>> ReadSingleAsync(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);
        var result = await ReadAsync(address, GetLength(address, length, 4), cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => bitConverter.ToSingle(result.Content.Span, 0, length));
    }

    /// <inheritdoc/>
    public virtual async ValueTask<OperResult<Double[]>> ReadDoubleAsync(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);
        var result = await ReadAsync(address, GetLength(address, length, 8), cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => bitConverter.ToDouble(result.Content.Span, 0, length));
    }
    /// <inheritdoc/>
    public virtual async ValueTask<OperResult<Decimal[]>> ReadDecimalAsync(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);
        var result = await ReadAsync(address, GetLength(address, length, 8), cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => bitConverter.ToDecimal(result.Content.Span, 0, length));
    }
    /// <inheritdoc/>
    public virtual async ValueTask<OperResult<String[]>> ReadStringAsync(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);
        if (bitConverter.StringLength == null) return new OperResult<String[]>(AppResource.StringAddressError);
        var len = bitConverter.StringLength * length;

        var result = await ReadAsync(address, GetLength(address, len.Value, 1), cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() =>
        {
            String[] strings = new String[length];
            for (int i = 0; i < length; i++)
            {
                var data = bitConverter.ToString(result.Content.Span, i * bitConverter.StringLength.Value, bitConverter.StringLength.Value);
                strings[i] = data;
            }
            return strings;
        }
        );
    }

    #endregion 读取

    #region 写入

    /// <inheritdoc/>
    public abstract ValueTask<OperResult> WriteAsync(string address, ReadOnlyMemory<byte> value, DataTypeEnum dataType, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract ValueTask<OperResult> WriteAsync(string address, ReadOnlyMemory<bool> value, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, bool value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        return WriteAsync(address, new bool[1] { value }, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, byte value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);
        return WriteAsync(address, new byte[] { value }.AsMemory(), DataTypeEnum.Byte, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, short value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);
        return WriteAsync(address, bitConverter.GetBytes(value), DataTypeEnum.Int16, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, ushort value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);
        return WriteAsync(address, bitConverter.GetBytes(value), DataTypeEnum.UInt16, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, int value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);
        return WriteAsync(address, bitConverter.GetBytes(value), DataTypeEnum.Int32, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, uint value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);
        return WriteAsync(address, bitConverter.GetBytes(value), DataTypeEnum.UInt32, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, long value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);
        return WriteAsync(address, bitConverter.GetBytes(value), DataTypeEnum.Int64, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, ulong value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);
        return WriteAsync(address, bitConverter.GetBytes(value), DataTypeEnum.UInt64, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, float value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);
        return WriteAsync(address, bitConverter.GetBytes(value), DataTypeEnum.Float, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, double value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);
        return WriteAsync(address, bitConverter.GetBytes(value), DataTypeEnum.Double, cancellationToken);
    }
    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, decimal value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);
        return WriteAsync(address, bitConverter.GetBytes(value), DataTypeEnum.Decimal, cancellationToken);
    }
    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, string value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);
        var data = bitConverter.GetBytes(value);
        return WriteAsync(address, data.ArrayExpandToLength(bitConverter.StringLength ?? data.Length), DataTypeEnum.String, cancellationToken);
    }

    #endregion 写入

    #region 写入数组

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, ReadOnlyMemory<short> value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);
        return WriteAsync(address, bitConverter.GetBytes(value.Span), DataTypeEnum.Int16, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, ReadOnlyMemory<ushort> value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);
        return WriteAsync(address, bitConverter.GetBytes(value.Span), DataTypeEnum.UInt16, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, ReadOnlyMemory<int> value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);
        return WriteAsync(address, bitConverter.GetBytes(value.Span), DataTypeEnum.Int32, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, ReadOnlyMemory<uint> value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);
        return WriteAsync(address, bitConverter.GetBytes(value.Span), DataTypeEnum.UInt32, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, ReadOnlyMemory<long> value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);
        return WriteAsync(address, bitConverter.GetBytes(value.Span), DataTypeEnum.Int64, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, ReadOnlyMemory<ulong> value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);
        return WriteAsync(address, bitConverter.GetBytes(value.Span), DataTypeEnum.UInt64, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, ReadOnlyMemory<float> value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);
        return WriteAsync(address, bitConverter.GetBytes(value.Span), DataTypeEnum.Float, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, ReadOnlyMemory<double> value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);
        return WriteAsync(address, bitConverter.GetBytes(value.Span), DataTypeEnum.Double, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, ReadOnlyMemory<decimal> value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);
        return WriteAsync(address, bitConverter.GetBytes(value.Span), DataTypeEnum.Decimal, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual ValueTask<OperResult> WriteAsync(string address, ReadOnlyMemory<string> value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);
        if (bitConverter.StringLength == null) return EasyValueTask.FromResult(new OperResult(AppResource.StringAddressError));
        List<ReadOnlyMemory<byte>> bytes = new();
        foreach (var a in value.Span)
        {
            var data = bitConverter.GetBytes(a);
            bytes.Add(data.ArrayExpandToLength(bitConverter.StringLength ?? data.Length));
        }

        return WriteAsync(address, bytes.CombineMemoryBlocks(), DataTypeEnum.String, cancellationToken);
    }

    #endregion 写入数组

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (Channel != null)
        {
            lock (Channel)
            {

                Channel.Starting.Remove(ChannelStarting);
                Channel.Stoped.Remove(ChannelStoped);
                Channel.Started.Remove(ChannelStarted);
                Channel.Stoping.Remove(ChannelStoping);
                Channel.ChannelReceived.Remove(ChannelReceived);

                if (Channel.Collects.Count == 1)
                {
                    if (Channel is ITcpServiceChannel tcpServiceChannel)
                    {
                        tcpServiceChannel.Clients.ForEach(a => a.WaitHandlePool?.CancelAll());
                    }

                    try
                    {
                        //只关闭，不释放
                        _ = Channel.CloseAsync();
                        if (Channel is IClientChannel client)
                        {
                            client.WaitHandlePool?.CancelAll();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger?.LogWarning(ex);
                    }
                }
                else
                {
                    if (Channel is ITcpServiceChannel tcpServiceChannel && this is IDtu dtu)
                    {
                        if (tcpServiceChannel.TryGetClient($"ID={dtu.DtuId}", out var client))
                        {
                            client.WaitHandlePool?.CancelAll();
                            _ = client.CloseAsync();
                        }
                    }
                }

                Channel.Collects.Remove(this);
                if (Channel is IClientChannel clientChannel)
                {
                    clientChannel.LogSeted(false);
                }

            }
        }
        _reusableTimeouts?.SafeDispose();
        _deviceLogger?.TryDispose();
        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    protected override async Task DisposeAsync(bool disposing)
    {
        if (Channel != null)
        {
            Channel.Starting.Remove(ChannelStarting);
            Channel.Stoped.Remove(ChannelStoped);
            Channel.Started.Remove(ChannelStarted);
            Channel.Stoping.Remove(ChannelStoping);
            Channel.ChannelReceived.Remove(ChannelReceived);

            if (Channel.Collects.Count == 1)
            {
                if (Channel is ITcpServiceChannel tcpServiceChannel)
                {
                    tcpServiceChannel.Clients.ForEach(a => a.WaitHandlePool?.CancelAll());
                }

                try
                {
                    //只关闭，不释放
                    await Channel.CloseAsync().ConfigureAwait(false);
                    if (Channel is IClientChannel client)
                    {
                        client.WaitHandlePool?.CancelAll();
                    }
                }
                catch (Exception ex)
                {
                    Logger?.LogWarning(ex);
                }
            }
            else
            {
                if (Channel is ITcpServiceChannel tcpServiceChannel && this is IDtu dtu)
                {
                    if (tcpServiceChannel.TryGetClient($"ID={dtu.DtuId}", out var client))
                    {
                        client.WaitHandlePool?.CancelAll();
                        await client.CloseAsync().ConfigureAwait(false);
                    }
                }
            }

            Channel.Collects.Remove(this);

            if (Channel is IClientChannel clientChannel)
            {
                clientChannel.LogSeted(false);
            }

        }

        _reusableTimeouts?.SafeDispose();
        _deviceLogger?.TryDispose();
        base.Dispose(disposing);
    }
    /// <inheritdoc/>
    public virtual Action<IPluginManager> ConfigurePlugins(TouchSocketConfig config)
    {
        switch (Channel.ChannelType)
        {
            case ChannelTypeEnum.TcpService:
                {
                    if (Channel.ChannelOptions.DtuSeviceType == DtuSeviceType.Default)
                        return PluginUtil.GetDtuPlugin(Channel.ChannelOptions);
                    else
                        return PluginUtil.GetTcpServicePlugin(Channel.ChannelOptions);
                }

        }
        return a => { };
    }
    public abstract ValueTask<OperResult<ReadOnlyMemory<byte>>> ReadAsync(object state, CancellationToken cancellationToken = default);

}
