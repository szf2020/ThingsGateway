//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;

using ThingsGateway.Gateway.Application;

using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace ThingsGateway.Management;

public partial class RemoteManagementTask : AsyncDisposableObject
{
    internal const string LogPath = $"Logs/{nameof(RemoteManagementTask)}";
    private ILog LogMessage;
    private ILogger _logger;
    private TextFileLogger TextLogger;

    public RemoteManagementTask(ILogger logger, RemoteManagementOptions remoteManagementOptions)
    {
        _logger = logger;
        TextLogger = TextFileLogger.GetMultipleFileLogger(LogPath);
        TextLogger.LogLevel = TouchSocket.Core.LogLevel.Trace;
        var log = new LoggerGroup() { LogLevel = TouchSocket.Core.LogLevel.Trace };
        log?.AddLogger(new EasyLogger(Log_Out) { LogLevel = TouchSocket.Core.LogLevel.Trace });
        log?.AddLogger(TextLogger);
        LogMessage = log;

        _remoteManagementOptions = remoteManagementOptions;

    }

    private void Log_Out(TouchSocket.Core.LogLevel logLevel, object source, string message, Exception exception)
    {
        _logger?.Log_Out(logLevel, source, message, exception);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_remoteManagementOptions.Enable) return;

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
                   a.AddRpcStore(store => store.RegisterServer<RemoteManagementRpcServer>());
               })
               .ConfigurePlugins(a =>
               {
                   a.UseTcpSessionCheckClear();
                   a.UseDmtpRpc();
                   a.UseDmtpHeartbeat()//使用Dmtp心跳
                   .SetTick(TimeSpan.FromMilliseconds(_remoteManagementOptions.HeartbeatInterval))
                   .SetMaxFailCount(3);

                   a.AddDmtpHandshakedPlugin(async () =>
                   {
                       try
                       {
                           await tcpDmtpClient.ResetIdAsync($"{_remoteManagementOptions.Name}:{GlobalData.HardwareJob.HardwareInfo.UUID}").ConfigureAwait(false);
                       }
                       catch (Exception)
                       {
                           await tcpDmtpClient.CloseAsync().ConfigureAwait(false);
                       }
                   });
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
                   a.AddRpcStore(store => store.RegisterServer<RemoteManagementRpcServer>());
               })
               .ConfigurePlugins(a =>
               {
                   a.UseTcpSessionCheckClear();
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


    protected override async Task DisposeAsync(bool disposing)
    {
        if (_tcpDmtpClient != null)
        {
            await _tcpDmtpClient.CloseAsync().ConfigureAwait(false);
            _tcpDmtpClient.SafeDispose();
            _tcpDmtpClient = null;
        }
        if (_tcpDmtpService != null)
        {
            await _tcpDmtpService.ClearAsync().ConfigureAwait(false);
            _tcpDmtpService.SafeDispose();
            _tcpDmtpService = null;
        }
        await base.DisposeAsync(disposing).ConfigureAwait(false);
    }
}
