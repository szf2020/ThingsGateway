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

using System.Runtime.CompilerServices;

using TouchSocket.Resources;

namespace ThingsGateway.Foundation;

public class DDPTcpSessionClientChannel : TcpSessionClientChannel
{
    /// <summary>
    /// 当客户端完整建立Tcp连接时触发。
    /// <para>
    /// 覆盖父类方法，将不会触发<see cref="ITcpConnectedPlugin"/>插件。
    /// </para>
    /// </summary>
    /// <param name="e">包含连接信息的事件参数</param>
    protected override Task OnTcpConnected(ConnectedEventArgs e)
    {
        // 如果当前实例的配置不为空，则将配置应用到适配器
        if (Config != null)
        {
            DDPAdapter.Config(Config);
        }

        DDPAdapter.OnLoaded(this);

        return base.OnTcpConnected(e);
    }

    #region 发送

    /// <summary>
    /// 异步发送数据，通过适配器模式灵活处理数据发送。
    /// </summary>
    /// <param name="memory">待发送的只读字节内存块。</param>
    /// <param name="token">可取消令箭</param>
    /// <returns>一个异步任务，表示发送操作。</returns>
    protected virtual async Task NewProtectedSendAsync(ReadOnlyMemory<byte> memory, CancellationToken token)
    {
        this.ThrowIfDisposed();
        this.ThrowIfClientNotConnected();

        if (!await this.OnTcpSending(memory).ConfigureAwait(false)) return;

        var transport = this.Transport;
        var adapter = this.DataHandlingAdapter;
        var locker = transport.WriteLocker;

        await locker.WaitAsync(token).ConfigureAwait(false);
        try
        {
            // 如果数据处理适配器未设置，则使用默认发送方式。
            if (adapter == null)
            {
                await transport.Writer.WriteAsync(memory, token).ConfigureAwait(false);
            }
            else
            {
                var byteBlock = new ByteBlock(1024);
                var ddpSend = new DDPSend(memory, Id, true);
                ddpSend.Build(ref byteBlock);
                var newMemory = byteBlock.Memory;
                var writer = new PipeBytesWriter(transport.Writer);
                adapter.SendInput(ref writer, in newMemory);
                await writer.FlushAsync(token).ConfigureAwait(false);
            }
        }
        finally
        {
            locker.Release();
        }
    }

    /// <summary>
    /// 异步发送请求信息的受保护方法。
    ///
    /// 此方法首先检查当前对象是否能够发送请求信息，如果不能，则抛出异常。
    /// 如果可以发送，它将使用数据处理适配器来异步发送输入请求。
    /// </summary>
    /// <param name="requestInfo">要发送的请求信息。</param>
    /// <param name="token">可取消令箭</param>
    /// <returns>返回一个任务，该任务代表异步操作的结果。</returns>
    protected virtual async Task NewProtectedSendAsync(IRequestInfo requestInfo, CancellationToken token)
    {
        // 检查是否具备发送请求的条件，如果不具备则抛出异常
        this.ThrowIfCannotSendRequestInfo();

        this.ThrowIfDisposed();
        this.ThrowIfClientNotConnected();

        var transport = this.Transport;
        var adapter = this.DataHandlingAdapter;
        var locker = transport.WriteLocker;

        await locker.WaitAsync(token).ConfigureAwait(false);
        try
        {
            var byteBlock = new ByteBlock(1024);
            if (requestInfo is not IRequestInfoBuilder requestInfoBuilder)
            {
                throw new Exception();
            }
            requestInfoBuilder.Build(ref byteBlock);
            var ddpSend = new DDPSend(byteBlock.Memory, Id, true);

            var writer = new PipeBytesWriter(transport.Writer);
            adapter.SendInput(ref writer, ddpSend);
            await writer.FlushAsync(token).ConfigureAwait(false);
        }
        finally
        {
            locker.Release();
        }
    }


    #endregion 发送
    public override Task SendAsync(IRequestInfo requestInfo, CancellationToken token = default)
    {
        return NewProtectedSendAsync(requestInfo, token);
    }

    public override Task SendAsync(ReadOnlyMemory<byte> memory, CancellationToken token = default)
    {
        return NewProtectedSendAsync(memory, token);
    }



    private DeviceSingleStreamDataHandleAdapter<DDPTcpMessage> DDPAdapter = new();

    protected override ValueTask<bool> OnTcpReceiving(IBytesReader byteBlock)
    {

        if (DDPAdapter.TryParseRequest(ref byteBlock, out var message))
        {
            return EasyValueTask.FromResult(true);
        }
        return OnTcpReceiving(this, message);

        static async PooledValueTask<bool> OnTcpReceiving(DDPTcpSessionClientChannel @this, DDPTcpMessage message)
        {
            if (message != null)
            {
                if (message.IsSuccess)
                {
                    var id = $"ID={message.Id}";
                    if (message.Type == 0x09)
                    {
                        using var reader = new PooledBytesReader();
                        reader.Reset(message.Content);

                        if (@this.DataHandlingAdapter == null)
                        {
                            await @this.OnTcpReceived(new ReceivedDataEventArgs(message.Content, default)).ConfigureAwait(false);
                        }
                        else
                        {
                            await @this.DataHandlingAdapter.ReceivedInputAsync(reader).ConfigureAwait(false);
                        }

                        return true;
                    }
                    else
                    {
                        if (message.Type == 0x01)
                        {
                            bool log = false;
                            if (id != @this.Id) log = true;

                            //注册ID
                            if (@this.Service is ITcpServiceChannel tcpService && tcpService.TryGetClient(id, out var oldClient) && oldClient != @this)
                            {
                                @this.Logger?.Debug($"Old socket connections with the same ID {id} will be closed");
                                try
                                {
                                    //await oldClient.ShutdownAsync(System.Net.Sockets.SocketShutdown.Both).ConfigureAwait(false);
                                    await oldClient.CloseAsync().ConfigureAwait(false);
                                }
                                catch
                                {
                                }
                                try
                                {
                                    oldClient.Dispose();
                                }
                                catch
                                {
                                }
                            }

                            await @this.ResetIdAsync(id, @this.ClosedToken).ConfigureAwait(false);

                            //发送成功
                            await @this.ProtectedSendAsync(new DDPSend(ReadOnlyMemory<byte>.Empty, id, true, 0x81), @this.ClosedToken).ConfigureAwait(false);
                            if (log)
                                @this.Logger?.Info(string.Format(AppResource.DtuConnected, @this.Id));
                        }
                        else if (message.Type == 0x02)
                        {
                            await @this.ProtectedSendAsync(new DDPSend(ReadOnlyMemory<byte>.Empty, @this.Id, true, 0x82), @this.ClosedToken).ConfigureAwait(false);
                            @this.Logger?.Info(string.Format(AppResource.DtuDisconnecting, @this.Id));
                            await Task.Delay(100).ConfigureAwait(false);
                            await @this.CloseAsync().ConfigureAwait(false);
                            @this.SafeDispose();
                        }
                    }
                }
            }

            return true;
        }
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

}
