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

using Microsoft.AspNetCore.Components.Forms;

using ThingsGateway.Authentication;

using TouchSocket.Core;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace ThingsGateway.Gateway.Application;

public partial class ManagementRpcServer : IRpcServer, IManagementRpcServer, IBackendLogService, IRpcLogService, IRestartService, IAuthenticationService, IChannelEnableService, IRedundancyHostedService, IRedundancyService, ITextFileReadService, IPluginPageService, IRealAlarmService, IChannelPageService, IDevicePageService, IVariablePageService
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

    public Task<List<PluginInfo>> GetPluginsAsync(PluginTypeEnum? pluginType = null) => App.GetService<IPluginPageService>().GetPluginsAsync(pluginType);

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

    public Task<string> GetPluginNameAsync(long channelId) => App.GetService<IChannelPageService>().GetPluginNameAsync(channelId);

    public Task RestartChannelAsync(long channelId) =>
    App.GetService<IChannelPageService>().RestartChannelAsync(channelId);
    public Task RestartChannelsAsync() =>
    App.GetService<IChannelPageService>().RestartChannelsAsync();
    public Task<LogLevel> ChannelLogLevelAsync(long id) =>
        App.GetService<IChannelPageService>().ChannelLogLevelAsync(id);

    public Task SetChannelLogLevelAsync(long id, LogLevel logLevel) =>
        App.GetService<IChannelPageService>().SetChannelLogLevelAsync(id, logLevel);

    public Task CopyChannelAsync(int copyCount, string copyChannelNamePrefix, int copyChannelNameSuffixNumber,
        string copyDeviceNamePrefix, int copyDeviceNameSuffixNumber, long channelId, bool restart) =>
        App.GetService<IChannelPageService>().CopyChannelAsync(copyCount, copyChannelNamePrefix, copyChannelNameSuffixNumber,
            copyDeviceNamePrefix, copyDeviceNameSuffixNumber, channelId, restart);

    public Task<QueryData<ChannelRuntime>> OnChannelQueryAsync(QueryPageOptions options) =>
        App.GetService<IChannelPageService>().OnChannelQueryAsync(options);

    public Task<List<Channel>> GetChannelListAsync(QueryPageOptions options, int max = 0) =>
        App.GetService<IChannelPageService>().GetChannelListAsync(options, max);

    public Task<bool> SaveChannelAsync(Channel input, ItemChangedType type, bool restart) =>
        App.GetService<IChannelPageService>().SaveChannelAsync(input, type, restart);

    public Task<bool> BatchEditChannelAsync(List<Channel> models, Channel oldModel, Channel model, bool restart) =>
        App.GetService<IChannelPageService>().BatchEditChannelAsync(models, oldModel, model, restart);

    public Task<bool> DeleteChannelAsync(List<long> ids, bool restart) =>
        App.GetService<IChannelPageService>().DeleteChannelAsync(ids, restart);

    public Task<bool> ClearChannelAsync(bool restart) =>
        App.GetService<IChannelPageService>().ClearChannelAsync(restart);

    public Task ImportChannelAsync(List<Channel> upData, List<Channel> insertData, bool restart) =>
        App.GetService<IChannelPageService>().ImportChannelAsync(upData, insertData, restart);

    public Task<Dictionary<string, ImportPreviewOutputBase>> ImportChannelUSheetDatasAsync(USheetDatas input, bool restart) =>
        App.GetService<IChannelPageService>().ImportChannelUSheetDatasAsync(input, restart);

    public Task<Dictionary<string, ImportPreviewOutputBase>> ImportChannelFileAsync(string filePath, bool restart) =>
        App.GetService<IChannelPageService>().ImportChannelFileAsync(filePath, restart);

    public Task<USheetDatas> ExportChannelAsync(List<Channel> channels) =>
        App.GetService<IChannelPageService>().ExportChannelAsync(channels);

    public Task<QueryData<SelectedItem>> OnChannelSelectedItemQueryAsync(VirtualizeQueryOption option) =>
        App.GetService<IChannelPageService>().OnChannelSelectedItemQueryAsync(option);

    public Task<Dictionary<string, ImportPreviewOutputBase>> ImportChannelAsync(IBrowserFile file, bool restart) =>
        App.GetService<IChannelPageService>().ImportChannelAsync(file, restart);

    public Task<string> ExportChannelFileAsync(GatewayExportFilter exportFilter) =>
        App.GetService<IChannelPageService>().ExportChannelFileAsync(exportFilter);

    public Task<string> GetChannelNameAsync(long id) =>
        App.GetService<IChannelPageService>().GetChannelNameAsync(id);

    public Task SetDeviceLogLevelAsync(long id, LogLevel logLevel) =>
        App.GetService<IDevicePageService>().SetDeviceLogLevelAsync(id, logLevel);

    public Task CopyDeviceAsync(int CopyCount, string CopyDeviceNamePrefix, int CopyDeviceNameSuffixNumber, long deviceId, bool AutoRestartThread) =>
    App.GetService<IDevicePageService>().CopyDeviceAsync(CopyCount, CopyDeviceNamePrefix, CopyDeviceNameSuffixNumber, deviceId, AutoRestartThread);

    public Task<LogLevel> DeviceLogLevelAsync(long id) =>
        App.GetService<IDevicePageService>().DeviceLogLevelAsync(id);

    public Task<bool> BatchEditDeviceAsync(List<Device> models, Device oldModel, Device model, bool restart) =>
        App.GetService<IDevicePageService>().BatchEditDeviceAsync(models, oldModel, model, restart);

    public Task<bool> SaveDeviceAsync(Device input, ItemChangedType type, bool restart) =>
        App.GetService<IDevicePageService>().SaveDeviceAsync(input, type, restart);

    public Task<bool> DeleteDeviceAsync(List<long> ids, bool restart) =>
        App.GetService<IDevicePageService>().DeleteDeviceAsync(ids, restart);

    public Task<Dictionary<string, ImportPreviewOutputBase>> ImportDeviceUSheetDatasAsync(USheetDatas input, bool restart) =>
        App.GetService<IDevicePageService>().ImportDeviceUSheetDatasAsync(input, restart);

    public Task<USheetDatas> ExportDeviceAsync(List<Device> devices) =>
        App.GetService<IDevicePageService>().ExportDeviceAsync(devices);

    public Task<string> ExportDeviceFileAsync(GatewayExportFilter exportFilter) =>
        App.GetService<IDevicePageService>().ExportDeviceFileAsync(exportFilter);

    public Task<QueryData<SelectedItem>> OnRedundantDevicesQueryAsync(VirtualizeQueryOption option, long deviceId, long channelId) =>
        App.GetService<IDevicePageService>().OnRedundantDevicesQueryAsync(option, deviceId, channelId);

    public Task<Dictionary<string, ImportPreviewOutputBase>> ImportDeviceFileAsync(string filePath, bool restart) =>
        App.GetService<IDevicePageService>().ImportDeviceFileAsync(filePath, restart);

    public Task DeviceRedundantThreadAsync(long id) =>
        App.GetService<IDevicePageService>().DeviceRedundantThreadAsync(id);

    public Task RestartDeviceAsync(long id, bool deleteCache) =>
        App.GetService<IDevicePageService>().RestartDeviceAsync(id, deleteCache);

    public Task PauseThreadAsync(long id) =>
        App.GetService<IDevicePageService>().PauseThreadAsync(id);

    public Task<QueryData<DeviceRuntime>> OnDeviceQueryAsync(QueryPageOptions options) =>
        App.GetService<IDevicePageService>().OnDeviceQueryAsync(options);

    public Task<List<Device>> GetDeviceListAsync(QueryPageOptions option, int v) =>
        App.GetService<IDevicePageService>().GetDeviceListAsync(option, v);

    public Task<bool> ClearDeviceAsync(bool restart) =>
        App.GetService<IDevicePageService>().ClearDeviceAsync(restart);

    public Task<bool> IsRedundantDeviceAsync(long id) =>
        App.GetService<IDevicePageService>().IsRedundantDeviceAsync(id);

    public Task<string> GetDeviceNameAsync(long redundantDeviceId) =>
        App.GetService<IDevicePageService>().GetDeviceNameAsync(redundantDeviceId);

    public Task<Dictionary<string, ImportPreviewOutputBase>> ImportDeviceAsync(IBrowserFile file, bool restart) =>
        App.GetService<IDevicePageService>().ImportDeviceAsync(file, restart);

    public Task<QueryData<SelectedItem>> OnDeviceSelectedItemQueryAsync(VirtualizeQueryOption option, bool isCollect) =>
        App.GetService<IDevicePageService>().OnDeviceSelectedItemQueryAsync(option, isCollect);

    public Task<string> GetDevicePluginNameAsync(long id) =>
        App.GetService<IDevicePageService>().GetDevicePluginNameAsync(id);

    public Task<bool> BatchEditVariableAsync(List<Variable> models, Variable oldModel, Variable model, bool restart) =>
        App.GetService<IVariablePageService>().BatchEditVariableAsync(models, oldModel, model, restart);

    public Task<bool> DeleteVariableAsync(List<long> ids, bool restart) =>
    App.GetService<IVariablePageService>().DeleteVariableAsync(ids, restart);

    public Task<bool> ClearVariableAsync(bool restart) =>
        App.GetService<IVariablePageService>().ClearVariableAsync(restart);

    public Task InsertTestDataAsync(int testVariableCount, int testDeviceCount, string slaveUrl, bool businessEnable, bool restart) =>
        App.GetService<IVariablePageService>().InsertTestDataAsync(testVariableCount, testDeviceCount, slaveUrl, businessEnable, restart);

    public Task<bool> BatchSaveVariableAsync(List<Variable> input, ItemChangedType type, bool restart) =>
        App.GetService<IVariablePageService>().BatchSaveVariableAsync(input, type, restart);

    public Task<bool> SaveVariableAsync(Variable input, ItemChangedType type, bool restart) =>
        App.GetService<IVariablePageService>().SaveVariableAsync(input, type, restart);

    public Task CopyVariableAsync(List<Variable> model, int copyCount, string copyVariableNamePrefix, int copyVariableNameSuffixNumber, bool restart) =>
        App.GetService<IVariablePageService>().CopyVariableAsync(model, copyCount, copyVariableNamePrefix, copyVariableNameSuffixNumber, restart);

    public Task<QueryData<VariableRuntime>> OnVariableQueryAsync(QueryPageOptions options) =>
        App.GetService<IVariablePageService>().OnVariableQueryAsync(options);

    public Task<List<Variable>> GetVariableListAsync(QueryPageOptions option, int v) =>
        App.GetService<IVariablePageService>().GetVariableListAsync(option, v);

    public Task<USheetDatas> ExportVariableAsync(List<Variable> models, string? sortName, SortOrder sortOrder) =>
        App.GetService<IVariablePageService>().ExportVariableAsync(models, sortName, sortOrder);

    public Task<Dictionary<string, ImportPreviewOutputBase>> ImportVariableUSheetDatasAsync(USheetDatas data, bool restart) =>
        App.GetService<IVariablePageService>().ImportVariableUSheetDatasAsync(data, restart);

    public Task<OperResult<object>> OnWriteVariableAsync(long id, string writeData) =>
        App.GetService<IVariablePageService>().OnWriteVariableAsync(id, writeData);

    public Task<Dictionary<string, ImportPreviewOutputBase>> ImportVariableAsync(IBrowserFile a, bool restart) =>
        App.GetService<IVariablePageService>().ImportVariableAsync(a, restart);

    public Task<Dictionary<string, ImportPreviewOutputBase>> ImportVariableFileAsync(string filePath, bool restart) =>
        App.GetService<IVariablePageService>().ImportVariableFileAsync(filePath, restart);

    public Task<string> ExportVariableFileAsync(GatewayExportFilter exportFilter) => App.GetService<IVariablePageService>().ExportVariableFileAsync(exportFilter);

    public Task<Dictionary<long, Tuple<string, string>>> GetDeviceIdNamesAsync() => App.GetService<IDevicePageService>().GetDeviceIdNamesAsync();
}
