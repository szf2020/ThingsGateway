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

namespace ThingsGateway.Foundation;

/// <summary>
/// Tcp客户端通道
/// </summary>
public class TcpClientChannel : TcpClient, IClientChannel
{
    /// <inheritdoc/>
    public TcpClientChannel(IChannelOptions channelOptions)
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
    private IDeviceDataHandleAdapter _deviceDataHandleAdapter;
    public void SetDataHandlingAdapterLogger(ILog log)
    {
        if (_deviceDataHandleAdapter == null && DataHandlingAdapter is IDeviceDataHandleAdapter handleAdapter)
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
    public ChannelReceivedEventHandler ChannelReceived { get; } = new();

    /// <inheritdoc/>
    public IChannelOptions ChannelOptions { get; }

    /// <inheritdoc/>
    public ChannelTypeEnum ChannelType => ChannelOptions.ChannelType;

    /// <inheritdoc/>
    public ConcurrentList<IDevice> Collects { get; } = new();

    /// <inheritdoc/>
    public DataHandlingAdapter ReadOnlyDataHandlingAdapter => DataHandlingAdapter;

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
    public virtual WaitLock GetLock(string key) => WaitLock;

    /// <inheritdoc/>
    public WaitLock WaitLock => ChannelOptions.WaitLock;



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


    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{IP}:{Port}";
    }

    protected override Task OnTcpClosed(ClosedEventArgs e)
    {
        Logger?.Info($"{ToString()}  Closed{(e.Message.IsNullOrEmpty() ? string.Empty : $" -{e.Message}")}");

        return base.OnTcpClosed(e);
    }
    /// <inheritdoc/>
    protected override Task OnTcpClosing(ClosingEventArgs e)
    {
        Logger?.Trace($"{ToString()}  Closing{(e.Message.IsNullOrEmpty() ? string.Empty : $" -{e.Message}")}");

        return base.OnTcpClosing(e);
    }

    /// <inheritdoc/>
    protected override Task OnTcpConnecting(ConnectingEventArgs e)
    {
        Logger?.Trace($"{ToString()}  Connecting{(e.Message.IsNullOrEmpty() ? string.Empty : $"-{e.Message}")}");
        return base.OnTcpConnecting(e);
    }

    protected override Task OnTcpConnected(ConnectedEventArgs e)
    {
        Logger?.Info($"{ToString()}  Connected");

        return base.OnTcpConnected(e);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
    {
        await base.OnTcpReceived(e).ConfigureAwait(false);

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
