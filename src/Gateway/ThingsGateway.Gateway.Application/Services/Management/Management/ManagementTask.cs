//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ThingsGateway.NewLife.Json.Extension;

using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.FileTransfer;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace ThingsGateway.Gateway.Application;

public partial class ManagementTask : AsyncDisposableObject
{
    internal const string LogPath = $"Logs/{nameof(ManagementTask)}";
    private ILog LogMessage;
    private ILogger _logger;
    private TextFileLogger TextLogger;

    public ManagementTask(ILogger logger, ManagementOptions managementOptions)
    {
        _logger = logger;
        TextLogger = TextFileLogger.GetMultipleFileLogger(LogPath);
        TextLogger.LogLevel = TouchSocket.Core.LogLevel.Trace;
        var log = new LoggerGroup() { LogLevel = TouchSocket.Core.LogLevel.Trace };
        log?.AddLogger(new EasyLogger(Log_Out) { LogLevel = TouchSocket.Core.LogLevel.Trace });
        log?.AddLogger(TextLogger);
        LogMessage = log;

        _managementOptions = managementOptions;

    }

    private void Log_Out(TouchSocket.Core.LogLevel logLevel, object source, string message, Exception exception)
    {
        _logger?.Log_Out(logLevel, source, message, exception);
    }
    private bool success = true;

    private IScheduledTask _scheduledTask;
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_managementOptions.Enable) return;

        if (_managementOptions.IsServer)
        {
            _tcpDmtpService ??= await GetTcpDmtpService().ConfigureAwait(false);
        }
        else
        {
            _tcpDmtpClient ??= await GetTcpDmtpClient().ConfigureAwait(false);
        }
        _scheduledTask = ScheduledTaskHelper.GetTask("10000", OpenTask, null, LogMessage, cancellationToken);
        _scheduledTask.Start();
    }

    private async Task OpenTask(object? state, CancellationToken cancellationToken)
    {
        try
        {
            await EnsureChannelOpenAsync(cancellationToken).ConfigureAwait(false);
            success = true;
        }
        catch (Exception ex)
        {
            if (success)
                LogMessage?.LogWarning(ex, "Start");

            success = false;
        }
    }

    private TcpDmtpClient? _tcpDmtpClient;
    private TcpDmtpService? _tcpDmtpService;
    private ManagementOptions _managementOptions;


    private async Task<TcpDmtpClient> GetTcpDmtpClient()
    {
        var tcpDmtpClient = new TcpDmtpClient();
        var config = new TouchSocketConfig()
               .SetRemoteIPHost(_managementOptions.ServerUri)
               .SetAdapterOption(new AdapterOption() { MaxPackageSize = 1024 * 1024 * 1024 })
               .SetDmtpOption(a => a.VerifyToken = _managementOptions.VerifyToken)
               .ConfigureContainer(a =>
               {
                   a.AddDmtpRouteService();//添加路由策略
                   a.AddLogger(LogMessage);
                   a.AddRpcStore(store =>
                   {
                       store.RegisterServer<IManagementRpcServer>(new ManagementRpcServer());
                       store.RegisterServer<IUpgradeRpcServer>(new UpgradeRpcServer());

                       foreach (var type in App.EffectiveTypes.Where(p => typeof(IPluginRpcServer).IsAssignableFrom(p) && !p.IsAbstract && p.IsClass))
                       {
                           store.RegisterServer(type);
                       }
                   });

               })
               .ConfigurePlugins(a =>
               {
                   a.UseTcpSessionCheckClear();
                   a.UseDmtpRpc().ConfigureDefaultSerializationSelector(b =>
                   {
                       b.UseSystemTextJson(json =>
                       {
                           json.Converters.Add(new ByteArrayToNumberArrayConverterSystemTextJson());
                           json.Converters.Add(new JTokenSystemTextJsonConverter());
                           json.Converters.Add(new JValueSystemTextJsonConverter());
                           json.Converters.Add(new JObjectSystemTextJsonConverter());
                           json.Converters.Add(new JArraySystemTextJsonConverter());
                       });
                   });

                   a.UseDmtpFileTransfer();//必须添加文件传输插件

                   a.Add<FilePlugin>();


                   a.UseDmtpHeartbeat()//使用Dmtp心跳
                   .SetTick(TimeSpan.FromMilliseconds(_managementOptions.HeartbeatInterval))
                   .SetMaxFailCount(3);

                   a.AddDmtpCreatedChannelPlugin(async () =>
                   {
                       try
                       {
                           await tcpDmtpClient.ResetIdAsync($"{_managementOptions.Name}:{GlobalData.HardwareJob.HardwareInfo.UUID}", tcpDmtpClient.ClosedToken).ConfigureAwait(false);
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
               .SetListenIPHosts(_managementOptions.ServerUri)
                   .SetAdapterOption(new AdapterOption() { MaxPackageSize = 1024 * 1024 * 1024 })
               .SetDmtpOption(a => a.VerifyToken = _managementOptions.VerifyToken)
               .ConfigureContainer(a =>
               {
                   a.AddDmtpRouteService();//添加路由策略
                   a.AddLogger(LogMessage);
                   a.AddRpcStore(store =>
                   {
                       store.RegisterServer<IManagementRpcServer>(new ManagementRpcServer());
                       store.RegisterServer<IUpgradeRpcServer>(new UpgradeRpcServer());
                       foreach (var type in App.EffectiveTypes.Where(p => typeof(IPluginRpcServer).IsAssignableFrom(p) && !p.IsAbstract && p.IsClass))
                       {
                           store.RegisterServer(type);
                       }
                   });

               })
               .ConfigurePlugins(a =>
               {
                   a.UseTcpSessionCheckClear();
                   a.UseDmtpRpc().ConfigureDefaultSerializationSelector(b =>
                   {
                       b.UseSystemTextJson(json =>
                       {
                           json.Converters.Add(new ByteArrayToNumberArrayConverterSystemTextJson());
                           json.Converters.Add(new JTokenSystemTextJsonConverter());
                           json.Converters.Add(new JValueSystemTextJsonConverter());
                           json.Converters.Add(new JObjectSystemTextJsonConverter());
                           json.Converters.Add(new JArraySystemTextJsonConverter());
                       });
                   });

                   a.UseDmtpFileTransfer();//必须添加文件传输插件

                   a.Add<FilePlugin>();

                   a.UseDmtpHeartbeat()//使用Dmtp心跳
                   .SetTick(TimeSpan.FromMilliseconds(_managementOptions.HeartbeatInterval))
                   .SetMaxFailCount(3);
               });

        await tcpDmtpService.SetupAsync(config).ConfigureAwait(false);
        return tcpDmtpService;
    }

    private async Task EnsureChannelOpenAsync(CancellationToken cancellationToken)
    {
        if (_managementOptions.IsServer)
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
        _scheduledTask?.SafeDispose();
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
        TextLogger?.Dispose();
        await base.DisposeAsync(disposing).ConfigureAwait(false);
    }











}