//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Foundation.Extension.String;

using TouchSocket.SerialPorts;

namespace ThingsGateway.Foundation;

/// <summary>
/// 通道扩展
/// </summary>
public static class ChannelOptionsExtensions
{
    /// <summary>
    /// 触发通道接收事件
    /// </summary>
    /// <param name="clientChannel">通道</param>
    /// <param name="e">接收数据</param>
    /// <param name="funcs">事件</param>
    /// <returns></returns>
    internal static async Task OnChannelReceivedEvent(this IClientChannel clientChannel, ReceivedDataEventArgs e, ChannelReceivedEventHandler funcs)
    {
        clientChannel.ThrowIfNull(nameof(IClientChannel));
        e.ThrowIfNull(nameof(ReceivedDataEventArgs));
        funcs.ThrowIfNull(nameof(ChannelReceivedEventHandler));

        if (funcs.Count > 0)
        {
            for (int i = 0; i < funcs.Count; i++)
            {
                var func = funcs[i];
                if (func == null) continue;
                await func.Invoke(clientChannel, e, i == funcs.Count - 1).ConfigureAwait(false);
                if (e.Handled)
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 触发通道事件
    /// </summary>
    /// <param name="clientChannel">通道</param>
    /// <param name="funcs">事件</param>
    /// <returns></returns>
    internal static async Task OnChannelEvent(this IClientChannel clientChannel, ChannelEventHandler funcs)
    {
        try
        {

            clientChannel.ThrowIfNull(nameof(IClientChannel));
            funcs.ThrowIfNull(nameof(ChannelEventHandler));

            if (funcs.Count > 0)
            {
                for (int i = 0; i < funcs.Count; i++)
                {
                    var func = funcs[i];
                    if (func == null) continue;
                    var handled = await func.Invoke(clientChannel, i == funcs.Count - 1).ConfigureAwait(false);
                    if (handled)
                    {
                        break;
                    }
                }
            }

        }
        catch (Exception ex)
        {
            clientChannel.Logger?.LogWarning(ex, "fail ChannelEvent");
        }
    }

    /// <summary>
    /// 获取一个新的通道。传入通道类型，远程服务端地址，绑定地址，串口配置信息
    /// </summary>
    /// <param name="config">配置</param>
    /// <param name="channelOptions">通道配置</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IChannel? GetChannel(this TouchSocketConfig config, IChannelOptions channelOptions)
    {
        config.ThrowIfNull(nameof(TouchSocketConfig));
        channelOptions.ThrowIfNull(nameof(IChannelOptions));
        var channelType = channelOptions.ChannelType;
        channelType.ThrowIfNull(nameof(ChannelTypeEnum));

        if (channelOptions.MaxClientCount > 0)
            config.SetMaxCount(channelOptions.MaxClientCount);

        config.SetTransportOption(a =>
        {
            a.SendPipeOptions = new System.IO.Pipelines.PipeOptions(
             minimumSegmentSize: 1024,
             useSynchronizationContext: false);
            a.ReceivePipeOptions = new System.IO.Pipelines.PipeOptions(
                pauseWriterThreshold: 1024 * 1024,
                resumeWriterThreshold: 1024 * 512,
             minimumSegmentSize: 1024,
                useSynchronizationContext: false);
        });

        switch (channelType)
        {
            case ChannelTypeEnum.TcpClient:
                return config.GetTcpClient(channelOptions);

            case ChannelTypeEnum.TcpService:
                return config.GetTcpService(channelOptions);

            case ChannelTypeEnum.SerialPort:
                return config.GetSerialPort(channelOptions);

            case ChannelTypeEnum.UdpSession:
                return config.GetUdpSession(channelOptions);
            case ChannelTypeEnum.Other:
                channelOptions.Config = config;
                OtherChannel otherChannel = new OtherChannel(channelOptions);
                return otherChannel;
        }
        return default;
    }

    /// <summary>
    /// 获取一个新的串口通道。传入串口配置信息
    /// </summary>
    /// <param name="config">配置</param>
    /// <param name="channelOptions">串口配置</param>
    /// <returns></returns>
    private static SerialPortChannel GetSerialPort(this TouchSocketConfig config, IChannelOptions channelOptions)
    {
        channelOptions.ThrowIfNull(nameof(SerialPortOption));
        channelOptions.Config = config;
        config.SetSerialPortOption(options =>
        {
            options.PortName = channelOptions.PortName;
            options.BaudRate = channelOptions.BaudRate;
            options.DataBits = channelOptions.DataBits;
            options.Parity = channelOptions.Parity;
            options.StopBits = channelOptions.StopBits;
            options.DtrEnable = channelOptions.DtrEnable;
            options.RtsEnable = channelOptions.RtsEnable;
            options.Handshake = channelOptions.Handshake;
        });
        //载入配置
        SerialPortChannel serialPortChannel = new SerialPortChannel(channelOptions);
        return serialPortChannel;
    }

    /// <summary>
    /// 获取一个新的Tcp客户端通道。传入远程服务端地址和绑定地址
    /// </summary>
    /// <param name="config">配置</param>
    /// <param name="channelOptions">通道配置</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    private static TcpClientChannel GetTcpClient(this TouchSocketConfig config, IChannelOptions channelOptions)
    {
        var remoteUrl = channelOptions.RemoteUrl;
        var bindUrl = channelOptions.BindUrl;
        remoteUrl.ThrowIfNull(nameof(remoteUrl));
        channelOptions.Config = config;
        config.SetRemoteIPHost(remoteUrl);
        if (!string.IsNullOrWhiteSpace(bindUrl))
            config.SetBindIPHost(bindUrl);

        //载入配置
        TcpClientChannel tcpClientChannel = new TcpClientChannel(channelOptions);
        return tcpClientChannel;
    }

    /// <summary>
    /// 获取一个新的Tcp服务会话通道。传入远程服务端地址和绑定地址
    /// </summary>
    /// <param name="config">配置</param>
    /// <param name="channelOptions">通道配置</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    private static IChannel GetTcpService(this TouchSocketConfig config, IChannelOptions channelOptions)
    {
        var bindUrl = channelOptions.BindUrl;
        bindUrl.ThrowIfNull(nameof(bindUrl));
        channelOptions.Config = config;

        var urls = bindUrl.SplitStringBySemicolon();
        config.SetListenIPHosts(IPHost.ParseIPHosts(urls));

        switch (channelOptions.DtuSeviceType)
        {

            case DtuSeviceType.DDP:
                return new TcpServiceChannel<DDPTcpSessionClientChannel>(channelOptions);

            case DtuSeviceType.Default:
            default:
                return new TcpServiceChannel<TcpSessionClientChannel>(channelOptions);
        }
    }

    /// <summary>
    /// 获取一个新的Udp会话通道。传入远程服务端地址和绑定地址
    /// </summary>
    /// <param name="config">配置</param>
    /// <param name="channelOptions">通道配置</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    private static UdpSessionChannel GetUdpSession(this TouchSocketConfig config, IChannelOptions channelOptions)
    {
        var remoteUrl = channelOptions.RemoteUrl;
        var bindUrl = channelOptions.BindUrl;
        if (string.IsNullOrEmpty(remoteUrl) && string.IsNullOrEmpty(bindUrl))
            throw new ArgumentNullException(nameof(IPHost));
        channelOptions.Config = config;

        if (!string.IsNullOrEmpty(remoteUrl))
            config.SetRemoteIPHost(remoteUrl);

        if (!string.IsNullOrEmpty(bindUrl))
            config.SetBindIPHost(bindUrl);
        else
            config.SetBindIPHost(new IPHost(0));

        switch (channelOptions.DtuSeviceType)
        {

            case DtuSeviceType.DDP:
                //载入配置
                var ddpUdp = new DDPUdpSessionChannel(channelOptions);
#if NETSTANDARD || NET6_0_OR_GREATER
                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                {
                    config.SetUdpConnReset(true);
                }
#endif
                return ddpUdp;

            case DtuSeviceType.Default:
            default:
                //载入配置
                var udpSessionChannel = new UdpSessionChannel(channelOptions);
#if NETSTANDARD || NET6_0_OR_GREATER
                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                {
                    config.SetUdpConnReset(true);
                }
#endif
                return udpSessionChannel;
        }
    }
}
