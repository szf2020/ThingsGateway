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

using TouchSocket.Core;

#if !Management
namespace ThingsGateway.Gateway.Application;
#else
namespace ThingsGateway.Management.Application;
#endif

public interface IDevicePageService
{
    Task SetDeviceLogLevelAsync(long id, TouchSocket.Core.LogLevel logLevel);
    Task<Dictionary<string, ImportPreviewOutputBase>> ImportDeviceAsync(IBrowserFile file, bool restart);

    Task CopyDeviceAsync(int CopyCount, string CopyDeviceNamePrefix, int CopyDeviceNameSuffixNumber, long deviceId, bool AutoRestartThread);
    Task<LogLevel> DeviceLogLevelAsync(long id);
    Task<bool> BatchEditDeviceAsync(List<Device> models, Device oldModel, Device model, bool restart);
    Task<bool> SaveDeviceAsync(Device input, ItemChangedType type, bool restart);
    Task<bool> DeleteDeviceAsync(List<long> ids, bool restart);
    Task<Dictionary<string, ImportPreviewOutputBase>> ImportDeviceUSheetDatasAsync(USheetDatas input, bool restart);
    Task<USheetDatas> ExportDeviceAsync(List<Device> devices);

    Task<string> ExportDeviceFileAsync(GatewayExportFilter exportFilter);


    Task<QueryData<SelectedItem>> OnRedundantDevicesQueryAsync(VirtualizeQueryOption option, long deviceId, long channelId);
    Task<Dictionary<string, ImportPreviewOutputBase>> ImportDeviceFileAsync(string filePath, bool restart);
    Task DeviceRedundantThreadAsync(long id);
    Task RestartDeviceAsync(long id, bool deleteCache);
    Task PauseThreadAsync(long id);
    Task<QueryData<DeviceRuntime>> OnDeviceQueryAsync(QueryPageOptions options);
    Task<List<Device>> GetDeviceListAsync(QueryPageOptions option, int v);
    Task<bool> ClearDeviceAsync(bool restart);
    Task<bool> IsRedundantDeviceAsync(long id);
    Task<string> GetDeviceNameAsync(long redundantDeviceId);
    Task<QueryData<SelectedItem>> OnDeviceSelectedItemQueryAsync(VirtualizeQueryOption option, bool isCollect);
    Task<string> GetDevicePluginNameAsync(long id);

    Task<Dictionary<long, Tuple<string, string>>> GetDeviceIdNamesAsync();
}