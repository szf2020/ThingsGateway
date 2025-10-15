//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Collections.Concurrent;
using System.Net;
using System.Runtime.CompilerServices;

using ThingsGateway.NewLife;

using TouchSocket.Resources;

namespace ThingsGateway.Foundation;

/// <summary>
/// Udp通道
/// </summary>
public class DDPUdpSessionChannel : UdpSessionChannel, IClientChannel, IDtuUdpSessionChannel
{
    public DDPUdpSessionChannel(IChannelOptions channelOptions) : base(channelOptions)
    {
    }
    protected override void LoadConfig(TouchSocketConfig config)
    {
        base.LoadConfig(config);

        // 如果当前实例的配置不为空，则将配置应用到适配器
        if (Config != null)
        {
            DDPAdapter.Config(Config);
        }

        if (DDPAdapter.Owner != null)
        {
            DDPAdapter.OnLoaded(this);
        }

        DDPAdapter.SendCallBackAsync = base.ProtectedDefaultSendAsync;
        DataHandlingAdapter.SendCallBackAsync = DefaultSendAsync;
    }


    protected Task DefaultSendAsync(EndPoint endPoint, ReadOnlyMemory<byte> memory, CancellationToken token)
    {
        if (TryGetId(endPoint, out var id))
        {
            return DDPAdapter.SendInputAsync(endPoint, new DDPSend(memory, id, false), token);
        }
        else
        {
            throw new ClientNotFindException();
        }
    }



    private DeviceUdpDataHandleAdapter<DDPUdpMessage> DDPAdapter = new();

    public EndPoint DefaultEndpoint => RemoteIPHost?.EndPoint;

    NonBlockingDictionary<string, WaitLock> WaitLocks { get; } = new();

    public override WaitLock GetLock(string key)
    {
        if (key.IsNullOrEmpty()) return WaitLock;
        return WaitLocks.GetOrAdd(key, (a) => new WaitLock(nameof(DDPUdpSessionChannel), WaitLock.MaxCount));
    }

    public override Task<Result> StopAsync(CancellationToken token)
    {
        WaitLocks.ForEach(a => a.Value.SafeDispose());
        WaitLocks.Clear();
        return base.StopAsync(token);
    }


    protected override async ValueTask<bool> OnUdpReceiving(UdpReceiveingEventArgs e)
    {
        var byteBlock = e.Memory;
        var endPoint = e.EndPoint;

        if (!DDPAdapter.TryParseRequest(endPoint, byteBlock, out var message))
            return true;

        if (message != null)
        {
            if (message.IsSuccess)
            {
                var id = $"ID={message.Id}";
                if (message.Type == 0x09)
                {
                    if (this.DataHandlingAdapter == null)
                    {
                        await this.OnUdpReceived(new UdpReceivedDataEventArgs(endPoint, message.Content, default)).ConfigureAwait(false);
                    }
                    else
                    {
                        await this.DataHandlingAdapter.ReceivedInputAsync(endPoint, message.Content).ConfigureAwait(false);
                    }

                    return true;
                }
                else
                {
                    if (message.Type == 0x01)
                    {
                        bool log = false;

                        //注册ID
                        if (!IdDict.TryAdd(endPoint, id))
                        {
                            IdDict[endPoint] = id;
                        }
                        else
                        {
                            log = true;
                        }
                        if (!EndPointDcit.TryAdd(id, endPoint))
                        {
                            EndPointDcit[id] = endPoint;
                        }
                        else
                        {
                            log = true;
                        }

                        //发送成功
                        await DDPAdapter.SendInputAsync(endPoint, new DDPSend(ReadOnlyMemory<byte>.Empty, id, false, 0x81), ClosedToken).ConfigureAwait(false);
                        if (log)
                            Logger?.Info(string.Format(AppResource.DtuConnected, id));
                    }
                    else if (message.Type == 0x02)
                    {
                        await DDPAdapter.SendInputAsync(endPoint, new DDPSend(ReadOnlyMemory<byte>.Empty, id, false, 0x82), ClosedToken).ConfigureAwait(false);
                        Logger?.Info(string.Format(AppResource.DtuDisconnecting, id));
                        await Task.Delay(100).ConfigureAwait(false);
                        IdDict.TryRemove(endPoint, out _);
                        EndPointDcit.TryRemove(id, out _);
                    }
                }
            }
        }
        return true;
    }

    #region Throw

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfCannotSendRequestInfo()
    {
        if (DataHandlingAdapter?.CanSendRequestInfo != true)
        {
            throw new NotSupportedException(TouchSocketResource.CannotSendRequestInfo);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfClientNotConnected()
    {
        if (Online)
        {
            return;
        }
        throw new ClientNotConnectedException();
    }

    #endregion Throw

    InternalConcurrentDictionary<EndPoint, string> IdDict { get; set; } = new();
    InternalConcurrentDictionary<string, EndPoint> EndPointDcit { get; set; } = new();
    public bool TryGetId(EndPoint endPoint, out string id)
    {
        return IdDict.TryGetValue(endPoint, out id);
    }

    public bool TryGetEndPoint(string id, out EndPoint endPoint)
    {
        return EndPointDcit.TryGetValue(id, out endPoint);
    }
}
