//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace ThingsGateway.Management;

public partial class RemoteManagementTask
{
    private ILog LogMessage;

    public RemoteManagementTask(ILog log)
    {
        LogMessage = log;
        _remoteManagementOptions = App.GetOptions<RemoteManagementOptions>();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_remoteManagementOptions.IsServer)
        {
            _tcpDmtpService ??= await GetTcpDmtpService().ConfigureAwait(false);
        }
        else
        {
            _tcpDmtpClient ??= await GetTcpDmtpClient().ConfigureAwait(false);
        }
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await EnsureChannelOpenAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogMessage?.LogWarning(ex, "Start");
            }
            finally
            {
                await Task.Delay(10000, cancellationToken).ConfigureAwait(false);
            }

        }
    }

    private TcpDmtpClient? _tcpDmtpClient;
    private TcpDmtpService? _tcpDmtpService;
    private RemoteManagementOptions _remoteManagementOptions;

    private async Task<TcpDmtpClient> GetTcpDmtpClient()
    {
        var tcpDmtpClient = new TcpDmtpClient();
        var config = new TouchSocketConfig()
               .SetRemoteIPHost(_remoteManagementOptions.ServerUri)
               .SetAdapterOption(new AdapterOption() { MaxPackageSize = 1024 * 1024 * 1024 })
               .SetDmtpOption(new DmtpOption() { VerifyToken = _remoteManagementOptions.VerifyToken })
               .ConfigureContainer(a =>
               {
                   a.AddLogger(LogMessage);
                   a.AddRpcStore(store => store.RegisterServer(new RemoteManagementRpcServer()));
               })
               .ConfigurePlugins(a =>
               {
                   a.UseDmtpRpc();
                   a.UseDmtpHeartbeat()//使用Dmtp心跳
                   .SetTick(TimeSpan.FromMilliseconds(_remoteManagementOptions.HeartbeatInterval))
                   .SetMaxFailCount(3);
               });

        await tcpDmtpClient.SetupAsync(config).ConfigureAwait(false);
        return tcpDmtpClient;
    }

    private async Task<TcpDmtpService> GetTcpDmtpService()
    {
        var tcpDmtpService = new TcpDmtpService();
        var config = new TouchSocketConfig()
               .SetListenIPHosts(_remoteManagementOptions.ServerUri)
                   .SetAdapterOption(new AdapterOption() { MaxPackageSize = 1024 * 1024 * 1024 })
               .SetDmtpOption(new DmtpOption() { VerifyToken = _remoteManagementOptions.VerifyToken })
               .ConfigureContainer(a =>
               {
                   a.AddLogger(LogMessage);
                   a.AddRpcStore(store => store.RegisterServer(new RemoteManagementRpcServer()));
               })
               .ConfigurePlugins(a =>
               {
                   a.UseDmtpRpc();
                   a.UseDmtpHeartbeat()//使用Dmtp心跳
                   .SetTick(TimeSpan.FromMilliseconds(_remoteManagementOptions.HeartbeatInterval))
                   .SetMaxFailCount(3);
               });

        await tcpDmtpService.SetupAsync(config).ConfigureAwait(false);
        return tcpDmtpService;
    }

    private async Task EnsureChannelOpenAsync(CancellationToken cancellationToken)
    {
        if (_remoteManagementOptions.IsServer)
        {
            if (_tcpDmtpService.ServerState != ServerState.Running)
            {
                if (_tcpDmtpService.ServerState != ServerState.Stopped)
                    await _tcpDmtpService.StopAsync(cancellationToken).ConfigureAwait(false);

                await _tcpDmtpService.StartAsync().ConfigureAwait(false);
            }
        }
        else
        {
            if (!_tcpDmtpClient.Online)
                await _tcpDmtpClient.ConnectAsync().ConfigureAwait(false);
        }
    }
}
