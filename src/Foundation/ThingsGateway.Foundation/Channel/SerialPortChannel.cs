//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.NewLife;

using TouchSocket.SerialPorts;

namespace ThingsGateway.Foundation;

/// <summary>
/// 串口通道
/// </summary>
public class SerialPortChannel : SerialPortClient, IClientChannel
{
    public SerialPortChannel(IChannelOptions channelOptions)
    {
        ChannelOptions = channelOptions;
        ResetSign();
    }

    public override TouchSocketConfig Config => base.Config ?? ChannelOptions.Config;

    public void ResetSign(int minSign = 0, int maxSign = ushort.MaxValue)
    {
        var pool = WaitHandlePool;
        WaitHandlePool = new WaitHandlePool<MessageBase>(minSign, maxSign);
        pool?.CancelAll();
    }
    /// <inheritdoc/>
    public ChannelReceivedEventHandler ChannelReceived { get; } = new();

    /// <inheritdoc/>
    public IChannelOptions ChannelOptions { get; }

    /// <inheritdoc/>
    public ChannelTypeEnum ChannelType => ChannelOptions.ChannelType;

    /// <inheritdoc/>
    public ConcurrentList<IDevice> Collects { get; } = new();

    /// <inheritdoc/>
    public DataHandlingAdapter ReadOnlyDataHandlingAdapter => ProtectedDataHandlingAdapter;
    private IDeviceDataHandleAdapter _deviceDataHandleAdapter;
    public void SetDataHandlingAdapterLogger(ILog log)
    {
        if (_deviceDataHandleAdapter != ProtectedDataHandlingAdapter && ProtectedDataHandlingAdapter is IDeviceDataHandleAdapter handleAdapter)
        {
            _deviceDataHandleAdapter = handleAdapter;
        }
        if (_deviceDataHandleAdapter != null)
        {
            _deviceDataHandleAdapter.Logger = log;
        }
    }
    /// <inheritdoc/>
    public void SetDataHandlingAdapter(DataHandlingAdapter adapter)
    {
        if (adapter is SingleStreamDataHandlingAdapter singleStreamDataHandlingAdapter)
            SetAdapter(singleStreamDataHandlingAdapter);
        if (adapter is IDeviceDataHandleAdapter deviceDataHandleAdapter)
            _deviceDataHandleAdapter = deviceDataHandleAdapter;
    }

    /// <inheritdoc/>
    public ChannelEventHandler Started { get; } = new();

    /// <inheritdoc/>
    public ChannelEventHandler Starting { get; } = new();

    /// <inheritdoc/>
    public ChannelEventHandler Stoped { get; } = new();

    /// <inheritdoc/>
    public ChannelEventHandler Stoping { get; } = new();
    /// <summary>
    /// 等待池
    /// </summary>
    public WaitHandlePool<MessageBase> WaitHandlePool { get; internal set; } = new(0, ushort.MaxValue);

    /// <inheritdoc/>
    public WaitLock WaitLock => ChannelOptions.WaitLock;
    public virtual WaitLock GetLock(string key) => WaitLock;



    //private readonly WaitLock _connectLock = new WaitLock();
    /// <inheritdoc/>
    public override async Task<Result> CloseAsync(string msg, CancellationToken token)
    {
        if (Online)
        {
            try
            {
                //await _connectLock.WaitAsync().ConfigureAwait(false);
                if (Online)
                {
                    PortName = null;
                    await this.OnChannelEvent(Stoping).ConfigureAwait(false);
                    var result = await base.CloseAsync(msg, token).ConfigureAwait(false);
                    if (!Online)
                    {
                        await this.OnChannelEvent(Stoped).ConfigureAwait(false);
                    }
                    return result;
                }
            }
            finally
            {
                //_connectLock.Release();
            }
        }
        return Result.Success;
    }

    /// <inheritdoc/>
    public override async Task ConnectAsync(CancellationToken token)
    {
        if (!Online)
        {
            try
            {
                //await _connectLock.WaitAsync(token).ConfigureAwait(false);
                if (!Online)
                {
                    if (token.IsCancellationRequested) return;


                    var port = Config?.GetValue(SerialPortConfigExtension.SerialPortOptionProperty);
                    if (port != null)
                        PortName = $"{port.PortName}";

                    await this.OnChannelEvent(Starting).ConfigureAwait(false);
                    await base.ConnectAsync(token).ConfigureAwait(false);
                    if (Online)
                    {
                        if (token.IsCancellationRequested) return;
                        await this.OnChannelEvent(Started).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                //_connectLock.Release();
            }
        }
    }


    private string PortName { get; set; }
    /// <inheritdoc/>
    public override string? ToString()
    {
        if (!PortName.IsNullOrEmpty())
            return PortName;

        var port = Config?.GetValue(SerialPortConfigExtension.SerialPortOptionProperty);
        if (port != null)
            return $"{port.PortName}";
        return base.ToString();
    }

    protected override Task OnSerialClosed(ClosedEventArgs e)
    {
        Logger?.Info($"{ToString()} Closed{(e.Message.IsNullOrEmpty() ? string.Empty : $" -{e.Message}")}");
        return base.OnSerialClosed(e);
    }
    /// <inheritdoc/>
    protected override Task OnSerialClosing(ClosingEventArgs e)
    {
        Logger?.Trace($"{ToString()} Closing{(e.Message.IsNullOrEmpty() ? string.Empty : $" -{e.Message}")}");
        return base.OnSerialClosing(e);
    }

    /// <inheritdoc/>
    protected override Task OnSerialConnecting(ConnectingEventArgs e)
    {
        Logger?.Trace($"{ToString()}  Connecting{(e.Message.IsNullOrEmpty() ? string.Empty : $" -{e.Message}")}");
        return base.OnSerialConnecting(e);
    }
    protected override Task OnSerialConnected(ConnectedEventArgs e)
    {
        Logger?.Debug($"{ToString()} Connected");
        return base.OnSerialConnected(e);
    }
    /// <inheritdoc/>
    protected override async Task OnSerialReceived(ReceivedDataEventArgs e)
    {
        await base.OnSerialReceived(e).ConfigureAwait(false);

        if (e.Handled)
            return;

        await this.OnChannelReceivedEvent(e, ChannelReceived).ConfigureAwait(false);

    }
    /// <inheritdoc/>
    protected override void SafetyDispose(bool disposing)
    {
        WaitHandlePool?.CancelAll();
        base.SafetyDispose(disposing);
    }
}
