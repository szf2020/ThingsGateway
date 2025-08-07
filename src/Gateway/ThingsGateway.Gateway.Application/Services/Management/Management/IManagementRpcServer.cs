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
    Task<Dictionary<string, Dictionary<string, OperResult<object>>>> Rpc(ICallContext callContext, Dictionary<string, Dictionary<string, string>> deviceDatas);


    [DmtpRpc]
    Task DeleteBackendLogAsync();
    [DmtpRpc]
    Task<List<BackendLog>> GetNewBackendLog();
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
    Task<List<RpcLog>> GetNewRpcLog();

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
    Task RestartServer();

    [DmtpRpc]
    Task<string> UUID();

    [DmtpRpc]
    Task<AuthorizeInfo> TryAuthorize(string password);
    [DmtpRpc]
    Task<AuthorizeInfo> TryGetAuthorizeInfo();
    [DmtpRpc]
    Task UnAuthorize();
    [DmtpRpc]
    Task<bool> StartBusinessChannelEnable();
    [DmtpRpc]
    Task<bool> StartCollectChannelEnable();


    [DmtpRpc]
    Task StartRedundancyTaskAsync();
    [DmtpRpc]
    Task StopRedundancyTaskAsync();
    [DmtpRpc]
    Task RedundancyForcedSync();

    [DmtpRpc]
    public Task<TouchSocket.Core.LogLevel> RedundancyLogLevel();
    [DmtpRpc]
    public Task SetRedundancyLogLevel(TouchSocket.Core.LogLevel logLevel);
    [DmtpRpc]
    public Task<string> RedundancyLogPath();

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
    Task<OperResult<List<string>>> GetLogFiles(string directoryPath);
    [DmtpRpc]
    Task<OperResult<List<LogData>>> LastLogData(string file, int lineCount = 200);






    /// <summary>
    /// 根据插件类型获取信息
    /// </summary>
    /// <param name="pluginType"></param>
    /// <returns></returns>
    [DmtpRpc]
    Task<List<PluginInfo>> GetPluginListAsync(PluginTypeEnum? pluginType = null);

    /// <summary>
    /// 分页显示插件
    /// </summary>
    [DmtpRpc]
    public Task<QueryData<PluginInfo>> PluginPage(QueryPageOptions options, PluginTypeEnum? pluginTypeEnum = null);

    /// <summary>
    /// 重载插件
    /// </summary>
    [DmtpRpc]
    Task ReloadPlugin();


    /// <summary>
    /// 添加插件
    /// </summary>
    /// <param name="plugin"></param>
    /// <returns></returns>
    [DmtpRpc]
    Task SavePluginByPath(PluginAddPathInput plugin);


    [DmtpRpc]
    Task<IEnumerable<AlarmVariable>> GetCurrentUserRealAlarmVariables();

}