// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;

namespace ThingsGateway.Gateway.Application;

public interface IChannelRuntimeService : IChannelPageService
{
    /// <summary>
    /// 保存通道
    /// </summary>
    /// <param name="input">通道对象</param>
    /// <param name="type">保存类型</param>
    /// <param name="restart">重启</param>
    Task<bool> BatchSaveChannelAsync(List<Channel> input, ItemChangedType type, bool restart);


    /// <summary>
    /// 导入通道数据
    /// </summary>
    Task ImportChannelAsync(Dictionary<string, ImportPreviewOutputBase> input, bool restart);


    Task<Dictionary<string, object>> ExportChannelAsync(GatewayExportFilter exportFilter);
    Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile browserFile);
    Task<MemoryStream> ExportMemoryStream(IEnumerable<Channel> data);
    Task RestartChannelAsync(IEnumerable<ChannelRuntime> oldChannelRuntimes);
    Task<bool> CopyAsync(List<Channel> models, Dictionary<Device, List<Variable>> devices, bool restart);
    Task<bool> UpdateAsync(List<Channel> models, List<Device> devices, List<Variable> variables, bool restart);
    Task<bool> InsertAsync(List<Channel> models, List<Device> devices, List<Variable> variables, bool restart);
    Task<Dictionary<string, ImportPreviewOutputBase>> ImportChannelAsync(IFormFile file, bool restart);
}