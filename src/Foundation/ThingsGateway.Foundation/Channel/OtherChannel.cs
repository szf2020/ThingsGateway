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
/// 测试通道
/// </summary>
public class OtherChannel : SetupConfigObject, IClientChannel
{
    ~OtherChannel()
    {
        this.SafeDispose();
    }

    private SingleStreamDataHandlingAdapter m_dataHandlingAdapter;
    public DataHandlingAdapter ReadOnlyDataHandlingAdapter => m_dataHandlingAdapter;

    public OtherChannel(IChannelOptions channelOptions)
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



    private bool logSet;
    /// <inheritdoc/>
    public void SetDataHandlingAdapterLogger(ILog log)
    {
        if (!logSet && ReadOnlyDataHandlingAdapter is IDeviceDataHandleAdapter handleAdapter)
        {
            logSet = true;
            handleAdapter.Logger = log;
        }
    }
    /// <inheritdoc/>
    public void SetDataHandlingAdapter(DataHandlingAdapter adapter)
    {
        if (adapter is SingleStreamDataHandlingAdapter singleStreamDataHandlingAdapter)
            SetAdapter(singleStreamDataHandlingAdapter);

        logSet = false;
    }

    /// <summary>
    /// 设置数据处理适配器。
    /// </summary>
    /// <param name="adapter">要设置的适配器实例。</param>
    /// <exception cref="ArgumentNullException">如果提供的适配器实例为null，则抛出此异常。</exception>
    protected void SetAdapter(SingleStreamDataHandlingAdapter adapter)
    {
        // 检查当前实例是否已被释放，如果是，则抛出异常。
        ThrowIfDisposed();
        // 检查adapter参数是否为null，如果是，则抛出ArgumentNullException异常。
        if (adapter is null)
        {
            throw new ArgumentNullException(nameof(adapter));
        }

        // 如果当前实例的配置不为空，则将配置应用到适配器上。
        if (Config != null)
        {
            adapter.Config(Config);
        }

        // 设置适配器的日志记录器和加载、接收数据的回调方法。
        adapter.OnLoaded(this);
        adapter.ReceivedAsyncCallBack = PrivateHandleReceivedData;

        // 将提供的适配器实例设置为当前实例的数据处理适配器。
        m_dataHandlingAdapter = adapter;
    }

    private Task PrivateHandleReceivedData(ReadOnlyMemory<byte> byteBlock, IRequestInfo requestInfo)
    {
        LastReceivedTime = DateTime.Now;
        return this.OnChannelReceivedEvent(new ReceivedDataEventArgs(byteBlock, requestInfo), ChannelReceived);
    }

    /// <summary>
    /// 异步发送数据，保护方法。
    /// </summary>
    /// <param name="memory">待发送的字节数据内存。</param>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns>异步任务。</returns>
    protected Task ProtectedDefaultSendAsync(ReadOnlyMemory<byte> memory, CancellationToken cancellationToken)
    {
        LastSentTime = DateTime.Now;
        return Task.CompletedTask;
    }

    public Protocol Protocol => new Protocol("Other");

    public DateTimeOffset LastReceivedTime { get; private set; }

    public DateTimeOffset LastSentTime { get; private set; }

    public bool IsClient => true;

    public bool Online => online;
    public CancellationToken ClosedToken => this.m_transport == null ? new CancellationToken(true) : this.m_transport.Token;
    private CancellationTokenSource m_transport;
    public Task<Result> CloseAsync(string msg, CancellationToken token)
    {
        var cts = m_transport;
        m_transport = null;
        cts?.SafeCancel();
        cts?.SafeDispose();
        online = false;

        return Task.FromResult(Result.Success);
    }
    public volatile bool online;

    public Task ConnectAsync(CancellationToken token)
    {
        var cts = m_transport;
        m_transport = new();
        cts?.SafeCancel();
        cts?.SafeDispose();
        online = true;
        if (this.m_dataHandlingAdapter == null)
        {
            var adapter = this.Config.GetValue(SerialPortConfigExtension.SerialDataHandlingAdapterProperty)?.Invoke();
            if (adapter != null)
            {
                this.SetAdapter(adapter);
            }
        }
        return Task.CompletedTask;
    }

    public Task SendAsync(ReadOnlyMemory<byte> memory, CancellationToken cancellationToken)
    {
        if (m_dataHandlingAdapter == null)
        {
            return ProtectedDefaultSendAsync(memory, cancellationToken);
        }
        else
        {
            var byteBlock = new ByteBlock(1024);
            m_dataHandlingAdapter.SendInput(ref byteBlock, memory);

            byteBlock.SafeDispose();
            return EasyTask.CompletedTask;
        }
    }

    public Task SendAsync(IRequestInfo requestInfo, CancellationToken cancellationToken)
    {
        // 检查是否具备发送请求的条件，如果不具备则抛出异常
        ThrowIfCannotSendRequestInfo();

        var byteBlock = new ByteBlock(1024);
        m_dataHandlingAdapter.SendInput(ref byteBlock, requestInfo);

        byteBlock.SafeDispose();
        return EasyTask.CompletedTask;
    }


    private void ThrowIfCannotSendRequestInfo()
    {
        if (m_dataHandlingAdapter?.CanSendRequestInfo != true)
        {
            throw new NotSupportedException($"当前适配器为空或者不支持对象发送。");
        }
    }


}
