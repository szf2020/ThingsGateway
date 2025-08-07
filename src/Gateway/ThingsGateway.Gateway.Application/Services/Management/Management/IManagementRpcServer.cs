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

namespace ThingsGateway.Gateway.Application;

#if Management
[GeneratorRpcProxy(GeneratorFlag = GeneratorFlag.ExtensionAsync)]
#endif
public interface IManagementRpcServer : IRpcServer
{
    [DmtpRpc]
    Task<Dictionary<string, Dictionary<string, OperResult<object>>>> RpcAsync(ICallContext callContext, Dictionary<string, Dictionary<string, string>> deviceDatas);


    [DmtpRpc]
    Task DeleteBackendLogAsync();
    [DmtpRpc]
    Task<List<BackendLog>> GetNewBackendLogAsync();
    [DmtpRpc]
    Task<QueryData<BackendLog>> BackendLogPageAsync(QueryPageOptions option);
    [DmtpRpc]
    Task<List<BackendLogDayStatisticsOutput>> BackendLogStatisticsByDayAsync(int day);


    /// <summary>
    /// 删除 RpcLog 表中的所有记录
    /// </summary>
    /// <remarks>
    /// 调用此方法会删除 RpcLog 表中的所有记录。
    /// </remarks>
    [DmtpRpc]
    Task DeleteRpcLogAsync();

    /// <summary>
    /// 获取最新的十条 RpcLog 记录
    /// </summary>
    /// <returns>最新的十条记录</returns>
    [DmtpRpc]
    Task<List<RpcLog>> GetNewRpcLogAsync();

    /// <summary>
    /// 分页查询 RpcLog 数据
    /// </summary>
    /// <param name="option">查询选项</param>
    /// <returns>查询到的数据</returns>
    [DmtpRpc]
    Task<QueryData<RpcLog>> RpcLogPageAsync(QueryPageOptions option);

    /// <summary>
    /// 按天统计 RpcLog 数据
    /// </summary>
    /// <param name="day">统计的天数</param>
    /// <returns>按天统计的结果列表</returns>
    [DmtpRpc]
    Task<List<RpcLogDayStatisticsOutput>> RpcLogStatisticsByDayAsync(int day);

    [DmtpRpc]
    Task RestartServerAsync();

    [DmtpRpc]
    Task<string> UUIDAsync();

    [DmtpRpc]
    Task<AuthorizeInfo> TryAuthorizeAsync(string password);
    [DmtpRpc]
    Task<AuthorizeInfo> TryGetAuthorizeInfoAsync();
    [DmtpRpc]
    Task UnAuthorizeAsync();
    [DmtpRpc]
    Task<bool> StartBusinessChannelEnableAsync();
    [DmtpRpc]
    Task<bool> StartCollectChannelEnableAsync();


    [DmtpRpc]
    Task StartRedundancyTaskAsync();
    [DmtpRpc]
    Task StopRedundancyTaskAsync();
    [DmtpRpc]
    Task RedundancyForcedSync();

    [DmtpRpc]
    public Task<TouchSocket.Core.LogLevel> RedundancyLogLevelAsync();
    [DmtpRpc]
    public Task SetRedundancyLogLevelAsync(TouchSocket.Core.LogLevel logLevel);
    [DmtpRpc]
    public Task<string> RedundancyLogPathAsync();

    /// <summary>
    /// 修改冗余设置
    /// </summary>
    /// <param name="input"></param>
    [DmtpRpc]
    Task EditRedundancyOptionAsync(RedundancyOptions input);
    /// <summary>
    /// 获取冗余设置
    /// </summary>
    [DmtpRpc]
    Task<RedundancyOptions> GetRedundancyAsync();
    [DmtpRpc]
    Task<OperResult<List<string>>> GetLogFilesAsync(string directoryPath);
    [DmtpRpc]
    Task<OperResult<List<LogData>>> LastLogDataAsync(string file, int lineCount = 200);






    /// <summary>
    /// 根据插件类型获取信息
    /// </summary>
    /// <param name="pluginType"></param>
    /// <returns></returns>
    [DmtpRpc]
    Task<List<PluginInfo>> GetPluginsAsync(PluginTypeEnum? pluginType = null);

    /// <summary>
    /// 分页显示插件
    /// </summary>
    [DmtpRpc]
    public Task<QueryData<PluginInfo>> PluginPageAsync(QueryPageOptions options, PluginTypeEnum? pluginTypeEnum = null);

    /// <summary>
    /// 重载插件
    /// </summary>
    [DmtpRpc]
    Task ReloadPluginAsync();


    /// <summary>
    /// 添加插件
    /// </summary>
    /// <param name="plugin"></param>
    /// <returns></returns>
    [DmtpRpc]
    Task SavePluginByPathAsync(PluginAddPathInput plugin);


    [DmtpRpc]
    Task<IEnumerable<AlarmVariable>> GetCurrentUserRealAlarmVariablesAsync();



    [DmtpRpc]
    Task<IEnumerable<SelectedItem>> GetCurrentUserDeviceSelectedItemsAsync(string searchText, int startIndex, int count);
    [DmtpRpc]
    Task<QueryData<SelectedItem>> GetCurrentUserDeviceVariableSelectedItemsAsync(string deviceText, string searchText, int startIndex, int count);


    [DmtpRpc]
    Task<TouchSocket.Core.LogLevel> RulesLogLevelAsync(long rulesId);
    [DmtpRpc]
    Task SetRulesLogLevelAsync(long rulesId, TouchSocket.Core.LogLevel logLevel);
    [DmtpRpc]
    Task<string> RulesLogPathAsync(long rulesId);
    [DmtpRpc]
    Task<Rules> GetRuleRuntimesAsync(long rulesId);
    [DmtpRpc]
    Task DeleteRuleRuntimesAsync(List<long> ids);
    [DmtpRpc]
    Task EditRuleRuntimesAsync(Rules rules);



    /// <summary>
    /// 清除所有规则
    /// </summary>
    [DmtpRpc]
    Task ClearRulesAsync();

    /// <summary>
    /// 删除规则
    /// </summary>
    /// <param name="ids">待删除规则的ID列表</param>
    [DmtpRpc]
    Task<bool> DeleteRulesAsync(List<long> ids);

    /// <summary>
    /// 从缓存/数据库获取全部信息
    /// </summary>
    /// <returns>规则列表</returns>
    [DmtpRpc]
    Task<List<Rules>> GetAllAsync();

    /// <summary>
    /// 报表查询
    /// </summary>
    /// <param name="option">查询条件</param>
    /// <param name="filterKeyValueAction">查询条件</param>
    [DmtpRpc]
    Task<QueryData<Rules>> RulesPageAsync(QueryPageOptions option, FilterKeyValueAction filterKeyValueAction = null);

    /// <summary>
    /// 保存规则
    /// </summary>
    /// <param name="input">规则对象</param>
    /// <param name="type">保存类型</param>
    [DmtpRpc]
    Task<bool> SaveRulesAsync(Rules input, ItemChangedType type);
}