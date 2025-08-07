//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using ThingsGateway.Authentication;

using TouchSocket.Core;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace ThingsGateway.Gateway.Application;

public partial class ManagementRpcServer : IRpcServer, IManagementRpcServer, IBackendLogService, IRpcLogService, IRestartService, IAuthenticationService, IChannelEnableService, IRedundancyHostedService, IRedundancyService, ITextFileReadService, IPluginPageService, IRealAlarmService
{


    [DmtpRpc]
    public Task DeleteBackendLogAsync() => App.GetService<IBackendLogService>().DeleteBackendLogAsync();
    [DmtpRpc]
    public Task<List<BackendLogDayStatisticsOutput>> BackendLogStatisticsByDayAsync(int day) => App.GetService<IBackendLogService>().BackendLogStatisticsByDayAsync(day);

    [DmtpRpc]
    public Task<List<BackendLog>> GetNewBackendLog() => App.GetService<IBackendLogService>().GetNewBackendLog();

    [DmtpRpc]
    public Task<QueryData<BackendLog>> BackendLogPageAsync(QueryPageOptions option) => App.GetService<IBackendLogService>().BackendLogPageAsync(option);


    [DmtpRpc]
    public async Task<Dictionary<string, Dictionary<string, OperResult<object>>>> Rpc(ICallContext callContext, Dictionary<string, Dictionary<string, string>> deviceDatas)
    {
        var data = await GlobalData.RpcService.InvokeDeviceMethodAsync($"Management[{(callContext.Caller is ITcpSession tcpSession ? tcpSession.GetIPPort() : string.Empty)}]", deviceDatas, callContext.Token).ConfigureAwait(false);

        return data.ToDictionary(a => a.Key, a => a.Value.ToDictionary(b => b.Key, b => b.Value.GetOperResult()));
    }

    public Task DeleteRpcLogAsync() => App.GetService<IRpcLogService>().DeleteRpcLogAsync();

    public Task<List<RpcLog>> GetNewRpcLog() => App.GetService<IRpcLogService>().GetNewRpcLog();

    public Task<QueryData<RpcLog>> RpcLogPageAsync(QueryPageOptions option) => App.GetService<IRpcLogService>().RpcLogPageAsync(option);

    public Task<List<RpcLogDayStatisticsOutput>> RpcLogStatisticsByDayAsync(int day) => App.GetService<IRpcLogService>().RpcLogStatisticsByDayAsync(day);

    public Task RestartServer() => App.GetService<IRestartService>().RestartServer();

    public Task<string> UUID() => App.GetService<IAuthenticationService>().UUID();

    public Task<AuthorizeInfo> TryAuthorize(string password) => App.GetService<IAuthenticationService>().TryAuthorize(password);

    public Task<AuthorizeInfo> TryGetAuthorizeInfo() => App.GetService<IAuthenticationService>().TryGetAuthorizeInfo();

    public Task UnAuthorize() => App.GetService<IAuthenticationService>().UnAuthorize();

    public Task<bool> StartBusinessChannelEnable() => App.GetService<IChannelEnableService>().StartBusinessChannelEnable();


    public Task<bool> StartCollectChannelEnable() => App.GetService<IChannelEnableService>().StartCollectChannelEnable();

    public Task StartRedundancyTaskAsync() => App.GetService<IRedundancyHostedService>().StartRedundancyTaskAsync();

    public Task StopRedundancyTaskAsync() => App.GetService<IRedundancyHostedService>().StopRedundancyTaskAsync();

    public Task RedundancyForcedSync() => App.GetService<IRedundancyHostedService>().RedundancyForcedSync();


    public Task EditRedundancyOptionAsync(RedundancyOptions input) => App.GetService<IRedundancyService>().EditRedundancyOptionAsync(input);

    public Task<RedundancyOptions> GetRedundancyAsync() => App.GetService<IRedundancyService>().GetRedundancyAsync();

    public Task<LogLevel> RedundancyLogLevel() => App.GetService<IRedundancyHostedService>().RedundancyLogLevel();


    public Task SetRedundancyLogLevel(LogLevel logLevel) => App.GetService<IRedundancyHostedService>().SetRedundancyLogLevel(logLevel);

    public Task<string> RedundancyLogPath() => App.GetService<IRedundancyHostedService>().RedundancyLogPath();

    public Task<OperResult<List<string>>> GetLogFiles(string directoryPath) => App.GetService<ITextFileReadService>().GetLogFiles(directoryPath);

    public Task<OperResult<List<LogData>>> LastLogData(string file, int lineCount = 200) => App.GetService<ITextFileReadService>().LastLogData(file, lineCount);

    public Task<List<PluginInfo>> GetPluginListAsync(PluginTypeEnum? pluginType = null) => App.GetService<IPluginPageService>().GetPluginListAsync(pluginType);

    public Task<QueryData<PluginInfo>> PluginPage(QueryPageOptions options, PluginTypeEnum? pluginTypeEnum = null) => App.GetService<IPluginPageService>().PluginPage(options, pluginTypeEnum);

    public Task ReloadPlugin() => App.GetService<IPluginPageService>().ReloadPlugin();



    public Task SavePluginByPath(PluginAddPathInput plugin) => App.GetService<IPluginPageService>().SavePluginByPath(plugin);

    public Task<IEnumerable<AlarmVariable>> GetCurrentUserRealAlarmVariables() => App.GetService<IRealAlarmService>().GetCurrentUserRealAlarmVariables();
}
