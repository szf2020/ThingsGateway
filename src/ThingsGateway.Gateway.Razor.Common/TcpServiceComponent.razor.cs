//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Foundation.Common.Json.Extension;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Razor;

public partial class TcpServiceComponent : IDriverUIBase
{
    [Parameter, EditorRequired]
    public long DeviceId { get; set; }

    public ITcpServiceChannel? TcpServiceChannel => GlobalData.ReadOnlyIdDevices.TryGetValue(DeviceId, out DeviceRuntime deviceRuntime) ? deviceRuntime.Driver?.Channel as ITcpServiceChannel : null;

    [Inject]
    private ToastService ToastService { get; set; }

    public TcpSessionClientDto SearchModel { get; set; } = new TcpSessionClientDto();

    private async Task<bool> OnDeleteAsync(IEnumerable<TcpSessionClientDto> tcpSessionClientDtos)
    {
        if (TcpServiceChannel == null) return false;
        try
        {
            foreach (var item in tcpSessionClientDtos)
            {
                await TcpServiceChannel.ClientDisposeAsync(item.Id);
            }
            return true;
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
            return false;
        }
    }

    private Task<QueryData<TcpSessionClientDto>> OnQueryAsync(QueryPageOptions options)
    {
        if (TcpServiceChannel != null)
        {
            var clients = TcpServiceChannel.Clients.ToList();
            var data = clients.AdaptListTcpSessionClientDto();
            for (int i = 0; i < clients.Count; i++)
            {
                var client = clients[i];
                var pluginInfos = client.PluginManager.Plugins.Select(a =>
                {
                    var data = new
                    {
                        Name = a.GetType().Name,
                        Dict = new Dictionary<string, string>()
                    };

                    var propertyInfos = a.GetType().GetProperties();
                    foreach (var item in propertyInfos)
                    {
                        var type = item.PropertyType;
                        if (type.IsPrimitive || type.IsEnum || type == TouchSocketCoreUtility.StringType)
                        {
                            data.Dict.Add(item.Name, item.GetValue(a)?.ToString());
                        }
                    }
                    return data;
                }).ToList();
                data[i].PluginInfos = pluginInfos.ToSystemTextJsonString();
            }

            var query = data.GetQueryData(options);

            return Task.FromResult(query);
        }
        else
        {
            return Task.FromResult(new QueryData<TcpSessionClientDto>());
        }
    }

}
