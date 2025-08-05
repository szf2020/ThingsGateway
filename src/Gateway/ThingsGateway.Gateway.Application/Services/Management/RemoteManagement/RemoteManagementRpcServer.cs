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

using ThingsGateway.Gateway.Application;

using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace ThingsGateway.Management;

public partial class RemoteManagementRpcServer : SingletonRpcServer,
    IBackendLogService
{
    [DmtpRpc]
    public Task DeleteBackendLogAsync() => App.GetService<IBackendLogService>().DeleteBackendLogAsync();
    [DmtpRpc]
    public Task<List<BackendLogDayStatisticsOutput>> StatisticsByDayAsync(int day) => App.GetService<IBackendLogService>().StatisticsByDayAsync(day);

    [DmtpRpc]
    public Task<List<BackendLog>> GetNewLog() => App.GetService<IBackendLogService>().GetNewLog();

    [DmtpRpc]
    public Task<QueryData<BackendLog>> PageAsync(QueryPageOptions option) => App.GetService<IBackendLogService>().PageAsync(option);


    [DmtpRpc]
    public async Task<Dictionary<string, Dictionary<string, OperResult<object>>>> Rpc(ICallContext callContext, Dictionary<string, Dictionary<string, string>> deviceDatas, CancellationToken cancellationToken)
    {
        var data = await GlobalData.RpcService.InvokeDeviceMethodAsync($"RemoteManagement[{(callContext.Caller is ITcpSession tcpSession ? tcpSession.GetIPPort() : string.Empty)}]", deviceDatas, cancellationToken).ConfigureAwait(false);

        return data.ToDictionary(a => a.Key, a => a.Value.ToDictionary(b => b.Key, b => b.Value.GetOperResult()));
    }


}
