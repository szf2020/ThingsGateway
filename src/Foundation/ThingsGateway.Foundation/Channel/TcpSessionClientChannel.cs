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
/// Tcp终端通道
/// </summary>
public class TcpSessionClientChannel : TcpSessionClient, IClientChannel
{
    ~TcpSessionClientChannel()
    {
        this.SafeDispose();
    }
    /// <inheritdoc/>
    public TcpSessionClientChannel()
    {
    }
    /// <inheritdoc/>
    public void SetDataHandlingAdapterLogger(ILog log)
    {
        if (DataHandlingAdapter is IDeviceDataHandleAdapter handleAdapter)
        {
            handleAdapter.Logger = log;
        }
    }
    /// <inheritdoc/>
    public void SetDataHandlingAdapter(DataHandlingAdapter adapter)
    {
        if (adapter is SingleStreamDataHandlingAdapter singleStreamDataHandlingAdapter)
            SetAdapter(singleStreamDataHandlingAdapter);

    }

    public void ResetSign(int minSign = 1, int maxSign = ushort.MaxValue - 1)
    {
        var pool = WaitHandlePool;
        WaitHandlePool = new WaitHandlePool<MessageBase>(minSign, maxSign);
        pool?.CancelAll();
    }
    /// <inheritdoc/>
    public ChannelReceivedEventHandler ChannelReceived { get; } = new();

    /// <inheritdoc/>
    public IChannelOptions ChannelOptions { get; internal set; }

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
    public WaitHandlePool<MessageBase> WaitHandlePool { get; private set; } = new(1, ushort.MaxValue - 1);

    /// <inheritdoc/>
    public WaitLock WaitLock { get; internal set; } = new(nameof(TcpSessionClientChannel));
    public virtual WaitLock GetLock(string key) => WaitLock;

    /// <inheritdoc/>
    public override Task<Result> CloseAsync(string msg, CancellationToken token)
    {
        WaitHandlePool?.CancelAll();
        return base.CloseAsync(msg, token);
    }

    /// <inheritdoc/>
    public Task ConnectAsync(CancellationToken token) => Task.CompletedTask;



    /// <inheritdoc/>
    public Task SetupAsync(TouchSocketConfig config) => Task.CompletedTask;



    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{IP}:{Port}:{Id}";
    }

    /// <inheritdoc/>
    protected override void SafetyDispose(bool disposing)
    {
        WaitHandlePool?.CancelAll();
        base.SafetyDispose(disposing);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpClosed(ClosedEventArgs e)
    {
        //Logger?.Debug($"{ToString()} Closed{(e.Message.IsNullOrEmpty() ? string.Empty : $"-{e.Message}")}");
        await this.OnChannelEvent(Stoped).ConfigureAwait(false);
        await base.OnTcpClosed(e).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpClosing(ClosingEventArgs e)
    {
        //Logger?.Debug($"{ToString()} Closing{(e.Message.IsNullOrEmpty() ? string.Empty : $"-{e.Message}")}");
        await this.OnChannelEvent(Stoping).ConfigureAwait(false);
        await base.OnTcpClosing(e).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpConnected(ConnectedEventArgs e)
    {
        await this.OnChannelEvent(Started).ConfigureAwait(false);
        await base.OnTcpConnected(e).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpConnecting(ConnectingEventArgs e)
    {
        await this.OnChannelEvent(Starting).ConfigureAwait(false);
        await base.OnTcpConnecting(e).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
    {
        await base.OnTcpReceived(e).ConfigureAwait(false);

        if (e.Handled)
            return;

        await this.OnChannelReceivedEvent(e, ChannelReceived).ConfigureAwait(false);
    }
}
