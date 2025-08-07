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
    public Task<List<BackendLog>> GetNewBackendLogAsync() => App.GetService<IBackendLogService>().GetNewBackendLogAsync();

    [DmtpRpc]
    public Task<QueryData<BackendLog>> BackendLogPageAsync(QueryPageOptions option) => App.GetService<IBackendLogService>().BackendLogPageAsync(option);


    [DmtpRpc]
    public async Task<Dictionary<string, Dictionary<string, OperResult<object>>>> RpcAsync(ICallContext callContext, Dictionary<string, Dictionary<string, string>> deviceDatas)
    {
        var data = await GlobalData.RpcService.InvokeDeviceMethodAsync($"Management[{(callContext.Caller is ITcpSession tcpSession ? tcpSession.GetIPPort() : string.Empty)}]", deviceDatas, callContext.Token).ConfigureAwait(false);

        return data.ToDictionary(a => a.Key, a => a.Value.ToDictionary(b => b.Key, b => b.Value.GetOperResult()));
    }

    public Task DeleteRpcLogAsync() => App.GetService<IRpcLogService>().DeleteRpcLogAsync();

    public Task<List<RpcLog>> GetNewRpcLogAsync() => App.GetService<IRpcLogService>().GetNewRpcLogAsync();

    public Task<QueryData<RpcLog>> RpcLogPageAsync(QueryPageOptions option) => App.GetService<IRpcLogService>().RpcLogPageAsync(option);

    public Task<List<RpcLogDayStatisticsOutput>> RpcLogStatisticsByDayAsync(int day) => App.GetService<IRpcLogService>().RpcLogStatisticsByDayAsync(day);

    public Task RestartServerAsync() => App.GetService<IRestartService>().RestartServerAsync();

    public Task<string> UUIDAsync() => App.GetService<IAuthenticationService>().UUIDAsync();

    public Task<AuthorizeInfo> TryAuthorizeAsync(string password) => App.GetService<IAuthenticationService>().TryAuthorizeAsync(password);

    public Task<AuthorizeInfo> TryGetAuthorizeInfoAsync() => App.GetService<IAuthenticationService>().TryGetAuthorizeInfoAsync();

    public Task UnAuthorizeAsync() => App.GetService<IAuthenticationService>().UnAuthorizeAsync();

    public Task<bool> StartBusinessChannelEnableAsync() => App.GetService<IChannelEnableService>().StartBusinessChannelEnableAsync();


    public Task<bool> StartCollectChannelEnableAsync() => App.GetService<IChannelEnableService>().StartCollectChannelEnableAsync();

    public Task StartRedundancyTaskAsync() => App.GetService<IRedundancyHostedService>().StartRedundancyTaskAsync();

    public Task StopRedundancyTaskAsync() => App.GetService<IRedundancyHostedService>().StopRedundancyTaskAsync();

    public Task RedundancyForcedSync() => App.GetService<IRedundancyHostedService>().RedundancyForcedSync();


    public Task EditRedundancyOptionAsync(RedundancyOptions input) => App.GetService<IRedundancyService>().EditRedundancyOptionAsync(input);

    public Task<RedundancyOptions> GetRedundancyAsync() => App.GetService<IRedundancyService>().GetRedundancyAsync();

    public Task<LogLevel> RedundancyLogLevelAsync() => App.GetService<IRedundancyHostedService>().RedundancyLogLevelAsync();


    public Task SetRedundancyLogLevelAsync(LogLevel logLevel) => App.GetService<IRedundancyHostedService>().SetRedundancyLogLevelAsync(logLevel);

    public Task<string> RedundancyLogPathAsync() => App.GetService<IRedundancyHostedService>().RedundancyLogPathAsync();

    public Task<OperResult<List<string>>> GetLogFilesAsync(string directoryPath) => App.GetService<ITextFileReadService>().GetLogFilesAsync(directoryPath);

    public Task<OperResult<List<LogData>>> LastLogDataAsync(string file, int lineCount = 200) => App.GetService<ITextFileReadService>().LastLogDataAsync(file, lineCount);

    public Task<List<PluginInfo>> GetPluginListAsync(PluginTypeEnum? pluginType = null) => App.GetService<IPluginPageService>().GetPluginListAsync(pluginType);

    public Task<QueryData<PluginInfo>> PluginPageAsync(QueryPageOptions options, PluginTypeEnum? pluginTypeEnum = null) => App.GetService<IPluginPageService>().PluginPageAsync(options, pluginTypeEnum);

    public Task ReloadPluginAsync() => App.GetService<IPluginPageService>().ReloadPluginAsync();



    public Task SavePluginByPathAsync(PluginAddPathInput plugin) => App.GetService<IPluginPageService>().SavePluginByPathAsync(plugin);

    public Task<IEnumerable<AlarmVariable>> GetCurrentUserRealAlarmVariablesAsync() => App.GetService<IRealAlarmService>().GetCurrentUserRealAlarmVariablesAsync();

    public Task<IEnumerable<SelectedItem>> GetCurrentUserDeviceSelectedItemsAsync(string searchText, int startIndex, int count) => App.GetService<IGlobalDataService>().GetCurrentUserDeviceSelectedItemsAsync(searchText, startIndex, count);

    public Task<QueryData<SelectedItem>> GetCurrentUserDeviceVariableSelectedItemsAsync(string deviceText, string searchText, int startIndex, int count) => App.GetService<IGlobalDataService>().GetCurrentUserDeviceVariableSelectedItemsAsync(deviceText, searchText, startIndex, count);

    public Task<TouchSocket.Core.LogLevel> RulesLogLevelAsync(long rulesId) => App.GetService<IRulesEngineHostedService>().RulesLogLevelAsync(rulesId);
    public Task SetRulesLogLevelAsync(long rulesId, TouchSocket.Core.LogLevel logLevel) => App.GetService<IRulesEngineHostedService>().SetRulesLogLevelAsync(rulesId, logLevel);
    public Task<string> RulesLogPathAsync(long rulesId) => App.GetService<IRulesEngineHostedService>().RulesLogPathAsync(rulesId);
    public Task<Rules> GetRuleRuntimesAsync(long rulesId) => App.GetService<IRulesEngineHostedService>().GetRuleRuntimesAsync(rulesId);


    public Task DeleteRuleRuntimesAsync(List<long> ids) => App.GetService<IRulesEngineHostedService>().DeleteRuleRuntimesAsync(ids);

    public Task EditRuleRuntimesAsync(Rules rules) => App.GetService<IRulesEngineHostedService>().EditRuleRuntimesAsync(rules);

    public Task ClearRulesAsync() => App.GetService<IRulesService>().ClearRulesAsync();


    public Task<bool> DeleteRulesAsync(List<long> ids) => App.GetService<IRulesService>().DeleteRulesAsync(ids);

    public Task<List<Rules>> GetAllAsync() => App.GetService<IRulesService>().GetAllAsync();


    public Task<QueryData<Rules>> RulesPageAsync(QueryPageOptions option, FilterKeyValueAction filterKeyValueAction = null) => App.GetService<IRulesService>().RulesPageAsync(option, filterKeyValueAction);


    public Task<bool> SaveRulesAsync(Rules input, ItemChangedType type) => App.GetService<IRulesService>().SaveRulesAsync(input, type);

}
