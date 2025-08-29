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

using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.WebApi;

namespace ThingsGateway.Gateway.Application;

#if Management
[GeneratorRpcProxy(GeneratorFlag = GeneratorFlag.ExtensionAsync)]
#endif
[TouchSocket.WebApi.Router("/miniapi/managementrpc/[action]")]
public interface IManagementRpcServer : IRpcServer
{
    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<QueryData<BackendLog>> BackendLogPageAsync(QueryPageOptions option);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Get)]
    Task<List<BackendLogDayStatisticsOutput>> BackendLogStatisticsByDayAsync(int day);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<bool> BatchEditChannelAsync(List<Channel> models, Channel oldModel, Channel model, bool restart);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<bool> BatchEditDeviceAsync(List<Device> models, Device oldModel, Device model, bool restart);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<bool> BatchEditVariableAsync(List<Variable> models, Variable oldModel, Variable model, bool restart);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<bool> BatchSaveVariableAsync(List<Variable> input, ItemChangedType type, bool restart);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Get)]
    Task<TouchSocket.Core.LogLevel> ChannelLogLevelAsync(long id);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<bool> ClearChannelAsync(bool restart);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<bool> ClearDeviceAsync(bool restart);

    /// <summary>
    /// 清除所有规则
    /// </summary>
    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task ClearRulesAsync();

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<bool> ClearVariableAsync(bool restart);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task CopyChannelAsync(int CopyCount, string CopyChannelNamePrefix, int CopyChannelNameSuffixNumber, string CopyDeviceNamePrefix, int CopyDeviceNameSuffixNumber, long channelId, bool AutoRestartThread);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task CopyDeviceAsync(int CopyCount, string CopyDeviceNamePrefix, int CopyDeviceNameSuffixNumber, long deviceId, bool AutoRestartThread);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task CopyVariableAsync(List<Variable> Model, int CopyCount, string CopyVariableNamePrefix, int CopyVariableNameSuffixNumber, bool AutoRestartThread);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task DeleteBackendLogAsync();

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<bool> DeleteChannelAsync(List<long> ids, bool restart);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<bool> DeleteDeviceAsync(List<long> ids, bool restart);

    /// <summary>
    /// 删除 RpcLog 表中的所有记录
    /// </summary>
    /// <remarks>
    /// 调用此方法会删除 RpcLog 表中的所有记录。
    /// </remarks>
    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task DeleteRpcLogAsync();

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task DeleteRuleRuntimesAsync(List<long> ids);

    /// <summary>
    /// 删除规则
    /// </summary>
    /// <param name="ids">待删除规则的ID列表</param>
    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<bool> DeleteRulesAsync(List<long> ids);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<bool> DeleteVariableAsync(List<long> ids, bool restart);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Get)]
    Task<TouchSocket.Core.LogLevel> DeviceLogLevelAsync(long id);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task DeviceRedundantThreadAsync(long id);

    /// <summary>
    /// 修改冗余设置
    /// </summary>
    /// <param name="input"></param>
    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task EditRedundancyOptionAsync(RedundancyOptions input);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task EditRuleRuntimesAsync(Rules rules);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<USheetDatas> ExportChannelAsync(List<Channel> channels);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<string> ExportChannelFileAsync(GatewayExportFilter exportFilter);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<USheetDatas> ExportDeviceAsync(List<Device> devices);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<string> ExportDeviceFileAsync(GatewayExportFilter exportFilter);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<USheetDatas> ExportVariableAsync(List<Variable> models, string? sortName, SortOrder sortOrder);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<string> ExportVariableFileAsync(GatewayExportFilter exportFilter);

    /// <summary>
    /// 从缓存/数据库获取全部信息
    /// </summary>
    /// <returns>规则列表</returns>
    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Get)]
    Task<List<Rules>> GetAllRulesAsync();

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<List<Channel>> GetChannelListAsync(QueryPageOptions options, int max = 0);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Get)]
    Task<string> GetChannelNameAsync(long channelId);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Get)]
    Task<IEnumerable<SelectedItem>> GetCurrentUserDeviceSelectedItemsAsync(string searchText, int startIndex, int count);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Get)]
    Task<QueryData<SelectedItem>> GetCurrentUserDeviceVariableSelectedItemsAsync(string deviceText, string searchText, int startIndex, int count);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Get)]
    Task<IEnumerable<AlarmVariable>> GetCurrentUserRealAlarmVariablesAsync();

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Get)]
    Task<Dictionary<long, Tuple<string, string>>> GetDeviceIdNamesAsync();

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<List<Device>> GetDeviceListAsync(QueryPageOptions option, int v);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Get)]
    Task<string> GetDeviceNameAsync(long redundantDeviceId);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Get)]
    Task<string> GetDevicePluginNameAsync(long id);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Get)]
    Task<OperResult<List<string>>> GetLogFilesAsync(string directoryPath);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Get)]
    Task<List<BackendLog>> GetNewBackendLogAsync();

    /// <summary>
    /// 获取最新的十条 RpcLog 记录
    /// </summary>
    /// <returns>最新的十条记录</returns>
    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Get)]
    Task<List<RpcLog>> GetNewRpcLogAsync();

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Get)]
    Task<string> GetPluginNameAsync(long channelId);

    /// <summary>
    /// 根据插件类型获取信息
    /// </summary>
    /// <param name="pluginType"></param>
    /// <returns></returns>
    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Get)]
    Task<List<PluginInfo>> GetPluginsAsync(PluginTypeEnum? pluginType = null);

    /// <summary>
    /// 获取冗余设置
    /// </summary>
    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Get)]
    Task<RedundancyOptions> GetRedundancyAsync();

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Get)]
    Task<Rules> GetRuleRuntimesAsync(long rulesId);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<List<Variable>> GetVariableListAsync(QueryPageOptions option, int v);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task ImportChannelAsync(List<Channel> upData, List<Channel> insertData, bool restart);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<Dictionary<string, ImportPreviewOutputBase>> ImportChannelFileAsync(string filePath, bool restart);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<Dictionary<string, ImportPreviewOutputBase>> ImportChannelUSheetDatasAsync(USheetDatas input, bool restart);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<Dictionary<string, ImportPreviewOutputBase>> ImportDeviceFileAsync(string filePath, bool restart);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<Dictionary<string, ImportPreviewOutputBase>> ImportDeviceUSheetDatasAsync(USheetDatas input, bool restart);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<Dictionary<string, ImportPreviewOutputBase>> ImportVariableFileAsync(string filePath, bool restart);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<Dictionary<string, ImportPreviewOutputBase>> ImportVariableUSheetDatasAsync(USheetDatas data, bool restart);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task InsertTestDataAsync(int testVariableCount, int testDeviceCount, string slaveUrl, bool businessEnable, bool restart);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Get)]
    Task<bool> IsRedundantDeviceAsync(long id);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Get)]
    Task<OperResult<List<LogData>>> LastLogDataAsync(string file, int lineCount = 200);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<QueryData<ChannelRuntime>> OnChannelQueryAsync(QueryPageOptions options);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<QueryData<SelectedItem>> OnChannelSelectedItemQueryAsync(VirtualizeQueryOption option);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<QueryData<DeviceRuntime>> OnDeviceQueryAsync(QueryPageOptions options);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<QueryData<SelectedItem>> OnDeviceSelectedItemQueryAsync(VirtualizeQueryOption option, bool isCollect);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<QueryData<SelectedItem>> OnRedundantDevicesQueryAsync(VirtualizeQueryOption option, long deviceId, long channelId);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<QueryData<VariableRuntime>> OnVariableQueryAsync(QueryPageOptions options);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<OperResult<object>> OnWriteVariableAsync(long id, string writeData);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task PauseThreadAsync(long id);

    /// <summary>
    /// 分页显示插件
    /// </summary>
    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<QueryData<PluginInfo>> PluginPageAsync(QueryPageOptions options, PluginTypeEnum? pluginTypeEnum = null);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task RedundancyForcedSync();

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Get)]
    Task<TouchSocket.Core.LogLevel> RedundancyLogLevelAsync();

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Get)]
    Task<string> RedundancyLogPathAsync();

    /// <summary>
    /// 重载插件
    /// </summary>
    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task ReloadPluginAsync();

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task RestartChannelAsync(long channelId);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task RestartChannelsAsync();

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task RestartDeviceAsync(long id, bool deleteCache);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task RestartServerAsync();

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<Dictionary<string, Dictionary<string, OperResult<object>>>> RpcAsync(ICallContext callContext, Dictionary<string, Dictionary<string, string>> deviceDatas);
    /// <summary>
    /// 分页查询 RpcLog 数据
    /// </summary>
    /// <param name="option">查询选项</param>
    /// <returns>查询到的数据</returns>
    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<QueryData<RpcLog>> RpcLogPageAsync(QueryPageOptions option);

    /// <summary>
    /// 按天统计 RpcLog 数据
    /// </summary>
    /// <param name="day">统计的天数</param>
    /// <returns>按天统计的结果列表</returns>
    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Get)]
    Task<List<RpcLogDayStatisticsOutput>> RpcLogStatisticsByDayAsync(int day);
    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Get)]
    Task<TouchSocket.Core.LogLevel> RulesLogLevelAsync(long rulesId);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Get)]
    Task<string> RulesLogPathAsync(long rulesId);

    /// <summary>
    /// 报表查询
    /// </summary>
    /// <param name="option">查询条件</param>
    /// <param name="filterKeyValueAction">查询条件</param>
    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<QueryData<Rules>> RulesPageAsync(QueryPageOptions option, FilterKeyValueAction filterKeyValueAction = null);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<bool> SaveChannelAsync(Channel input, ItemChangedType type, bool restart);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<bool> SaveDeviceAsync(Device input, ItemChangedType type, bool restart);

    /// <summary>
    /// 添加插件
    /// </summary>
    /// <param name="plugin"></param>
    /// <returns></returns>
    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task SavePluginByPathAsync(PluginAddPathInput plugin);

    /// <summary>
    /// 保存规则
    /// </summary>
    /// <param name="input">规则对象</param>
    /// <param name="type">保存类型</param>
    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<bool> SaveRulesAsync(Rules input, ItemChangedType type);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<bool> SaveVariableAsync(Variable input, ItemChangedType type, bool restart);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task SetChannelLogLevelAsync(long id, TouchSocket.Core.LogLevel logLevel);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task SetDeviceLogLevelAsync(long id, TouchSocket.Core.LogLevel logLevel);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task SetRedundancyLogLevelAsync(TouchSocket.Core.LogLevel logLevel);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task SetRulesLogLevelAsync(long rulesId, TouchSocket.Core.LogLevel logLevel);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<bool> StartBusinessChannelEnableAsync();

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<bool> StartCollectChannelEnableAsync();

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task StartRedundancyTaskAsync();

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task StopRedundancyTaskAsync();

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task<AuthorizeInfo> TryAuthorizeAsync(string password);

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Get)]
    Task<AuthorizeInfo> TryGetAuthorizeInfoAsync();

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Post)]
    Task UnAuthorizeAsync();

    [DmtpRpc]
    [WebApi(Method = HttpMethodType.Get)]
    Task<string> UUIDAsync();
}