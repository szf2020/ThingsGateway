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

using Newtonsoft.Json.Linq;

using ThingsGateway.Extension.Generic;
using ThingsGateway.Gateway.Application;
using ThingsGateway.NewLife;

using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace ThingsGateway.Management;

internal sealed class RedundancyTask : IRpcDriver, IAsyncDisposable
{
    private readonly ILogger _logger;
    private readonly IRedundancyService _redundancyService;
    private readonly GatewayRedundantSerivce _gatewayRedundantSerivce;

    public RedundancyTask(ILogger logger)
    {
        _logger = logger;
        _gatewayRedundantSerivce = App.RootServices.GetRequiredService<GatewayRedundantSerivce>();
        _redundancyService = App.RootServices.GetRequiredService<IRedundancyService>();
        // 创建新的文件日志记录器，并设置日志级别为 Trace
        LogPath = "Logs/RedundancyLog";
        TextLogger = TextFileLogger.GetMultipleFileLogger(LogPath);
        TextLogger.LogLevel = TouchSocket.Core.LogLevel.Trace;
    }


    public ILog LogMessage { get; set; }
    public TextFileLogger TextLogger { get; }
    public string LogPath { get; }
    private TcpDmtpClient _tcpDmtpClient;
    private TcpDmtpService _tcpDmtpService;

    private void Log_Out(TouchSocket.Core.LogLevel logLevel, object source, string message, Exception exception)
    {
        _logger?.Log_Out(logLevel, source, message, exception);
    }

    private ScheduledAsyncTask scheduledTask;


    /// <summary>
    /// 主站
    /// </summary>
    private async Task DoMasterWork(object? state, CancellationToken stoppingToken)
    {
        // 延迟一段时间，避免过于频繁地执行任务
        await Task.Delay(500, stoppingToken).ConfigureAwait(false);
        try
        {
            bool online = false;

            var waitInvoke = CreateDmtpInvokeOption(stoppingToken);

            try
            {
                if (_tcpDmtpService.Clients.Count != 0)
                {
                    online = true;
                }
                // 如果 online 为 true，表示设备在线
                if (online)
                {

                    int batchSize = 10000;
                    if (GlobalData.HardwareJob.HardwareInfo.MachineInfo.AvailableMemory > 2 * 1024 * 1024)
                    {
                        batchSize = 200000;
                    }
                    var deviceRunTimes = GlobalData.ReadOnlyIdDevices.Where(a => a.Value.IsCollect == true).Select(a => a.Value).Batch(batchSize);


                    foreach (var item in _tcpDmtpService.Clients)
                    {
                        foreach (var deviceDataWithValues in deviceRunTimes)
                        {

                            // 将 GlobalData.CollectDevices 和 GlobalData.Variables 同步到从站
                            await item.GetDmtpRpcActor().InvokeAsync(
                                             nameof(ReverseCallbackServer.UpData), null, waitInvoke, deviceDataWithValues.AdaptListDeviceDataWithValue()).ConfigureAwait(false);
                        }
                        LogMessage?.LogTrace($"{item.GetIPPort()} Update StandbyStation data success");
                    }
                }
            }
            catch (Exception ex)
            {
                // 输出警告日志，指示同步数据到从站时发生错误
                LogMessage?.LogWarning(ex, "Synchronize data to standby site error");
            }
            await Task.Delay(RedundancyOptions.SyncInterval, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex, "Execute");
        }
    }

    /// <summary>
    /// 从站
    /// </summary>
    private async Task DoSlaveWork(object? state, CancellationToken stoppingToken)
    {
        // 延迟一段时间，避免过于频繁地执行任务
        await Task.Delay(5000, stoppingToken).ConfigureAwait(false);
        try
        {
            bool online = false;
            var waitInvoke = CreateDmtpInvokeOption(stoppingToken);


            try
            {
                await _tcpDmtpClient.TryConnectAsync().ConfigureAwait(false);

                {
                    // 初始化读取错误计数器
                    var readErrorCount = 0;
                    // 当读取错误次数小于最大错误计数时循环执行
                    while (readErrorCount < RedundancyOptions.MaxErrorCount)
                    {
                        try
                        {
                            // 发送 Ping 请求以检查设备是否在线，超时时间为 10000 毫秒
                            online = await _tcpDmtpClient.PingAsync(10000).ConfigureAwait(false);
                            if (online)
                                break;
                            else
                            {
                                readErrorCount++;
                                await Task.Delay(RedundancyOptions.SyncInterval, stoppingToken).ConfigureAwait(false);
                            }
                        }
                        catch
                        {
                            // 捕获异常，增加读取错误计数器
                            readErrorCount++;
                            await Task.Delay(RedundancyOptions.SyncInterval, stoppingToken).ConfigureAwait(false);
                        }
                    }
                }


                // 如果设备不在线
                if (!online)
                {
                    // 无法获取状态，启动本机
                    await ActiveAsync().ConfigureAwait(false);
                }
                else
                {
                    // 如果设备在线
                    LogMessage?.LogTrace($"Ping ActiveStation {RedundancyOptions.MasterUri} success");
                    await StandbyAsync().ConfigureAwait(false);
                }
            }
            finally
            {
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex, "Execute");
        }
    }


    private WaitLock _switchLock = new();



    private bool first;
    private async Task StandbyAsync()
    {
        try
        {
            await _switchLock.WaitAsync().ConfigureAwait(false);
            if (_gatewayRedundantSerivce.StartCollectChannelEnable)
            {
                // 输出日志，指示主站已恢复，从站将切换到备用状态
                if (first)
                    LogMessage?.Warning("Master site has recovered, local machine (standby) will switch to standby state");

                // 将 IsStart 设置为 false，表示当前设备为从站，切换到备用状态
                _gatewayRedundantSerivce.StartCollectChannelEnable = false;
                _gatewayRedundantSerivce.StartBusinessChannelEnable = RedundancyOptions?.IsStartBusinessDevice ?? false;
                await RestartAsync().ConfigureAwait(false);
            }
        }
        finally
        {
            _switchLock.Release();
            first = true;
        }
    }

    private async Task ActiveAsync()
    {
        try
        {
            await _switchLock.WaitAsync().ConfigureAwait(false);

            _gatewayRedundantSerivce.StartBusinessChannelEnable = true;
            if (!_gatewayRedundantSerivce.StartCollectChannelEnable)
            {
                // 输出日志，指示无法连接冗余站点，本机将切换到正常状态
                if (first)
                    LogMessage?.Warning("Cannot connect to redundant site, local machine will switch to normal state");
                _gatewayRedundantSerivce.StartCollectChannelEnable = true;
                await RestartAsync().ConfigureAwait(false);
            }
        }
        finally
        {
            _switchLock.Release();
            first = true;
        }
    }
    private static Task RestartAsync()
    {
        return GlobalData.ChannelRuntimeService.RestartChannelAsync(GlobalData.ReadOnlyIdChannels.Values);
    }

    public async Task StartTaskAsync(CancellationToken cancellationToken)
    {
        await StopTaskAsync().ConfigureAwait(false);
        RedundancyOptions = (await _redundancyService.GetRedundancyAsync().ConfigureAwait(false)).AdaptRedundancyOptions();

        if (RedundancyOptions?.Enable == true)
        {
            if (RedundancyOptions.IsMaster)
            {
                _tcpDmtpService = await GetTcpDmtpService(RedundancyOptions).ConfigureAwait(false);

                await _tcpDmtpService.StartAsync().ConfigureAwait(false);//启动
                await ActiveAsync().ConfigureAwait(false);
            }
            else
            {
                _tcpDmtpClient = await GetTcpDmtpClient(RedundancyOptions).ConfigureAwait(false);
                await StandbyAsync().ConfigureAwait(false);
            }
        }
        else
        {
            await ActiveAsync().ConfigureAwait(false);
        }

        if (RedundancyOptions?.Enable == true)
        {
            LogMessage?.LogInformation($"Redundancy task started");
            if (RedundancyOptions.IsMaster)
            {
                scheduledTask = new ScheduledAsyncTask(10, DoMasterWork, null, null, cancellationToken);
            }
            else
            {
                scheduledTask = new ScheduledAsyncTask(10, DoSlaveWork, null, null, cancellationToken);
            }

            scheduledTask.Start();
        }

    }
    public async Task StopTaskAsync()
    {
        if (scheduledTask?.Enable == true)
        {
            LogMessage?.LogInformation($"Redundancy task stoped");
            scheduledTask?.Stop();
        }

        if (_tcpDmtpService != null)
        {
            try
            {
                await _tcpDmtpService.StopAsync().ConfigureAwait(false);
            }
            catch
            {
            }
        }
        if (_tcpDmtpClient != null)
        {
            try
            {
                await _tcpDmtpClient.CloseAsync().ConfigureAwait(false);
            }
            catch
            {
            }
        }

    }

    public async ValueTask DisposeAsync()
    {
        await StopTaskAsync().ConfigureAwait(false);
        TextLogger?.TryDispose();
        scheduledTask?.TryDispose();

        _tcpDmtpService?.TryDispose();
        _tcpDmtpClient?.TryDispose();
        _tcpDmtpService = null;
        _tcpDmtpClient = null;

    }

    #region

    private async Task<TcpDmtpClient> GetTcpDmtpClient(RedundancyOptions redundancy)
    {
        var log = new LoggerGroup() { LogLevel = TouchSocket.Core.LogLevel.Trace };
        log?.AddLogger(new EasyLogger(Log_Out) { LogLevel = TouchSocket.Core.LogLevel.Trace });
        log?.AddLogger(TextLogger);
        LogMessage = log;
        var tcpDmtpClient = new TcpDmtpClient();
        var config = new TouchSocketConfig()
               .SetRemoteIPHost(redundancy.MasterUri)
               .SetAdapterOption(new AdapterOption() { MaxPackageSize = 1024 * 1024 * 1024 })
               .SetDmtpOption(new DmtpOption() { VerifyToken = redundancy.VerifyToken })
               .ConfigureContainer(a =>
               {
                   a.AddLogger(LogMessage);
                   a.AddRpcStore(store =>
                   {
                       store.RegisterServer(new ReverseCallbackServer(this));
                   });
               })
               .ConfigurePlugins(a =>
               {
                   a.UseDmtpRpc();
                   a.UseDmtpHeartbeat()//使用Dmtp心跳
                   .SetTick(TimeSpan.FromMilliseconds(redundancy.HeartbeatInterval))
                   .SetMaxFailCount(redundancy.MaxErrorCount);
               });

        await tcpDmtpClient.SetupAsync(config).ConfigureAwait(false);
        return tcpDmtpClient;
    }

    private async Task<TcpDmtpService> GetTcpDmtpService(RedundancyOptions redundancy)
    {
        var log = new LoggerGroup() { LogLevel = TouchSocket.Core.LogLevel.Trace };
        log?.AddLogger(new EasyLogger(Log_Out) { LogLevel = TouchSocket.Core.LogLevel.Trace });
        log?.AddLogger(TextLogger);
        LogMessage = log;
        var tcpDmtpService = new TcpDmtpService();
        var config = new TouchSocketConfig()
               .SetListenIPHosts(redundancy.MasterUri)
               .SetAdapterOption(new AdapterOption() { MaxPackageSize = 1024 * 1024 * 1024 })
               .SetDmtpOption(new DmtpOption() { VerifyToken = redundancy.VerifyToken })
               .ConfigureContainer(a =>
               {
                   a.AddLogger(LogMessage);
                   a.AddRpcStore(store =>
                   {
                       store.RegisterServer(new ReverseCallbackServer(this));
                   });
               })
               .ConfigurePlugins(a =>
               {
                   a.UseDmtpRpc();
                   a.UseDmtpHeartbeat()//使用Dmtp心跳
                   .SetTick(TimeSpan.FromMilliseconds(redundancy.HeartbeatInterval))
                   .SetMaxFailCount(redundancy.MaxErrorCount);
               });

        await tcpDmtpService.SetupAsync(config).ConfigureAwait(false);
        return tcpDmtpService;
    }

    private DmtpInvokeOption CreateDmtpInvokeOption(CancellationToken cancellationToken)
    {
        return new DmtpInvokeOption()
        {
            FeedbackType = FeedbackType.WaitInvoke,
            Token = cancellationToken,
            Timeout = 30000,
            SerializationType = SerializationType.Json,
        };
    }
    private RedundancyOptions RedundancyOptions;


    #endregion

    #region ForcedSync

    WaitLock ForcedSyncWaitLock = new WaitLock();
    public async Task ForcedSync(CancellationToken cancellationToken = default)
    {
        await ForcedSyncWaitLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {

            if (!RedundancyOptions.IsMaster)
                return;

            var invokeOption = CreateDmtpInvokeOption(cancellationToken);

            try
            {
                await EnsureChannelOpenAsync(cancellationToken).ConfigureAwait(false);

                // 如果在线，执行同步
                bool online = RedundancyOptions.IsServer
                    ? _tcpDmtpService.Clients.Count > 0
                    : _tcpDmtpClient.Online;

                if (!online)
                {
                    LogMessage?.LogWarning("ForcedSync data error, no client online");
                    return;
                }

                if (RedundancyOptions.IsServer)
                {
                    foreach (var client in _tcpDmtpService.Clients)
                    {
                        await InvokeSyncDataAsync(client, invokeOption, cancellationToken).ConfigureAwait(false);
                    }
                }
                else
                {
                    await InvokeSyncDataAsync(_tcpDmtpClient, invokeOption, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                LogMessage?.LogWarning(ex, "ForcedSync data error");
            }

        }
        finally
        {
            ForcedSyncWaitLock.Release();
        }
    }

    private async Task InvokeSyncDataAsync(IDmtpActorObject client, DmtpInvokeOption invokeOption, CancellationToken cancellationToken)
    {

        await client.GetDmtpRpcActor().InvokeAsync(nameof(ReverseCallbackServer.SyncData), null, invokeOption, GlobalData.Channels.Select(a => a.Value).ToList(), GlobalData.Devices.Select(a => a.Value).ToList(), GlobalData.IdVariables.Select(a => a.Value).ToList())
            .ConfigureAwait(false);
        LogMessage?.LogTrace($"ForcedSync data success");

    }

    #endregion

    #region  Rpc

    /// <summary>
    /// 异步写入方法
    /// </summary>
    /// <param name="writeInfoLists">要写入的变量及其对应的数据</param>
    /// <param name="cancellationToken">取消操作的通知</param>
    /// <returns>写入操作的结果字典</returns>
    public async ValueTask<Dictionary<string, Dictionary<string, IOperResult>>> InvokeMethodAsync(Dictionary<VariableRuntime, JToken> writeInfoLists, CancellationToken cancellationToken)
    {
        return (await Rpc(writeInfoLists, cancellationToken).ConfigureAwait(false)).ToDictionary(a => a.Key, a => a.Value.ToDictionary(b => b.Key, b => (IOperResult)b.Value));
    }

    private async ValueTask<Dictionary<string, Dictionary<string, OperResult<object>>>> Rpc(Dictionary<VariableRuntime, JToken> writeInfoLists, CancellationToken cancellationToken)
    {
        var dataResult = new Dictionary<string, Dictionary<string, OperResult<object>>>();

        Dictionary<string, Dictionary<string, string>> deviceDatas = new();
        foreach (var item in writeInfoLists)
        {
            if (deviceDatas.TryGetValue(item.Key.DeviceName ?? string.Empty, out var variableDatas))
            {
                variableDatas.TryAdd(item.Key.Name, item.Value?.ToString() ?? string.Empty);
            }
            else
            {
                deviceDatas.TryAdd(item.Key.DeviceName ?? string.Empty, new());
                deviceDatas[item.Key.DeviceName ?? string.Empty].TryAdd(item.Key.Name, item.Value?.ToString() ?? string.Empty);
            }
        }

        if (RedundancyOptions.IsMaster)
            return NoOnline(dataResult, deviceDatas);

        var invokeOption = CreateDmtpInvokeOption(cancellationToken);

        try
        {
            await EnsureChannelOpenAsync(cancellationToken).ConfigureAwait(false);
            bool online = RedundancyOptions.IsServer ? _tcpDmtpService.Clients.Count > 0 : _tcpDmtpClient.Online;

            if (!online)
            {
                LogMessage?.LogWarning("Rpc error, no client online");
                return NoOnline(dataResult, deviceDatas);
            }

            if (RedundancyOptions.IsServer)
            {
                await InvokeRpcServerAsync(deviceDatas, dataResult, invokeOption).ConfigureAwait(false);
            }
            else
            {
                dataResult = await InvokeRpcClientAsync(deviceDatas, invokeOption).ConfigureAwait(false);
            }

            LogMessage?.LogTrace("Rpc success");
            return dataResult;
        }
        catch (OperationCanceledException)
        {
            return NoOnline(dataResult, deviceDatas);
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex, "Rpc error");
            return NoOnline(dataResult, deviceDatas);
        }
    }

    private async Task<Dictionary<string, Dictionary<string, OperResult<object>>>> InvokeRpcClientAsync(
        Dictionary<string, Dictionary<string, string>> deviceDatas,
        DmtpInvokeOption invokeOption)
    {

        return await _tcpDmtpClient.GetDmtpRpcActor()
            .InvokeTAsync<Dictionary<string, Dictionary<string, OperResult<object>>>>(
                nameof(ReverseCallbackServer.Rpc), invokeOption, deviceDatas)
            .ConfigureAwait(false);
    }
    private async Task InvokeRpcServerAsync(
        Dictionary<string, Dictionary<string, string>> deviceDatas,
        Dictionary<string, Dictionary<string, OperResult<object>>> dataResult,
        DmtpInvokeOption invokeOption)
    {
        foreach (var item in deviceDatas)
        {
            if (GlobalData.ReadOnlyDevices.TryGetValue(item.Key, out var device))
            {
                var key = device.Tag;
                if (_tcpDmtpService.TryGetClient(key, out var client))
                {
                    try
                    {
                        var data = await client.GetDmtpRpcActor().InvokeTAsync<Dictionary<string, Dictionary<string, OperResult<object>>>>(
                            nameof(ReverseCallbackServer.Rpc), invokeOption, new Dictionary<string, Dictionary<string, string>> { { item.Key, item.Value } })
                            .ConfigureAwait(false);

                        dataResult.AddRange(data);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        dataResult.TryAdd(item.Key, new Dictionary<string, OperResult<object>>());

                        foreach (var vItem in item.Value)
                        {
                            dataResult[item.Key].TryAdd(vItem.Key, new OperResult<object>(ex));
                        }
                        continue;
                    }
                }
            }

            // 没找到设备 或 客户端未在线
            dataResult.TryAdd(item.Key, item.Value.ToDictionary(v => v.Key, v => new OperResult<object>("No online")));
        }
    }


    private static Dictionary<string, Dictionary<string, OperResult<object>>> NoOnline(Dictionary<string, Dictionary<string, OperResult<object>>> dataResult, Dictionary<string, Dictionary<string, string>> deviceDatas)
    {
        foreach (var item in deviceDatas)
        {
            dataResult.TryAdd(item.Key, new Dictionary<string, OperResult<object>>());

            foreach (var vItem in item.Value)
            {
                dataResult[item.Key].TryAdd(vItem.Key, new OperResult<object>("No online"));
            }
        }
        return dataResult;

    }

    /// <summary>
    /// 异步写入方法
    /// </summary>
    /// <param name="writeInfoLists">要写入的变量及其对应的数据</param>
    /// <param name="cancellationToken">取消操作的通知</param>
    /// <returns>写入操作的结果字典</returns>
    public async ValueTask<Dictionary<string, Dictionary<string, IOperResult>>> InVokeWriteAsync(Dictionary<VariableRuntime, JToken> writeInfoLists, CancellationToken cancellationToken)
    {
        return (await Rpc(writeInfoLists, cancellationToken).ConfigureAwait(false)).ToDictionary(a => a.Key, a => a.Value.ToDictionary(b => b.Key, b => (IOperResult)b.Value));
    }
    private async Task EnsureChannelOpenAsync(CancellationToken cancellationToken)
    {
        if (RedundancyOptions.IsServer)
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
                await _tcpDmtpClient.TryConnectAsync().ConfigureAwait(false);
        }
    }





    #endregion
}
