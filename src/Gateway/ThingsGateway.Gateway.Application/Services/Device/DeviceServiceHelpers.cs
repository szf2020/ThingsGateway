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

using System.Collections.Concurrent;
using System.Reflection;

namespace ThingsGateway.Gateway.Application;

public static partial class DeviceServiceHelpers
{




    public static Dictionary<string, object> ExportSheets(
IAsyncEnumerable<Device>? data1,
IAsyncEnumerable<Device>? data2,
IReadOnlyDictionary<long, DeviceRuntime>? deviceDicts,
    IReadOnlyDictionary<long, ChannelRuntime> channelDicts,
HashSet<string> pluginSheetNames,
string? channelName = null)
    {
        if (data1 == null || data2 == null)
            return new();

        var result = new Dictionary<string, object>();
        result.Add(GatewayExportString.DeviceName, GetDeviceSheets(data1, deviceDicts, channelDicts, channelName));
        ConcurrentDictionary<string, (object, Dictionary<string, PropertyInfo>)> propertysDict = new();

        foreach (var plugin in pluginSheetNames)
        {
            var filtered = FilterPluginDevices(data2, plugin, channelDicts);
            var filtResult = PluginInfoUtil.GetFileNameAndTypeName(plugin);
            var pluginSheets = GetPluginSheets(filtered, propertysDict, plugin);
            result.Add(filtResult.TypeName, pluginSheets);
        }

        return result;
    }
    static IAsyncEnumerable<Device> FilterPluginDevices(IAsyncEnumerable<Device> data, string plugin, IReadOnlyDictionary<long, ChannelRuntime> channelDicts)
    {
        return data.Where(device =>
        {
            if (channelDicts.TryGetValue(device.ChannelId, out var channel))
            {
                if (channel.PluginName == plugin)
                    return true;
                else
                    return false;
            }
            else
            {
                return true;
            }
        });
    }



    static async IAsyncEnumerable<Dictionary<string, object>> GetDeviceSheets(
    IAsyncEnumerable<Device> data,
IReadOnlyDictionary<long, DeviceRuntime>? deviceDicts,
    IReadOnlyDictionary<long, ChannelRuntime> channelDicts,
    string? channelName)
    {
        var type = typeof(Device);
        var propertyInfos = type.GetRuntimeProperties()
            .Where(a => a.GetCustomAttribute<IgnoreExcelAttribute>(false) == null)
            .OrderBy(a =>
            {
                var order = a.GetCustomAttribute<AutoGenerateColumnAttribute>()?.Order ?? int.MaxValue;
                if (order < 0) order += 10000000;
                else if (order == 0) order = 10000000;
                return order;
            });

        var enumerator = data.GetAsyncEnumerator();
        while (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            var device = enumerator.Current;
            yield return GetDeviceRows(device, propertyInfos, type, deviceDicts, channelDicts, channelName);
        }
    }

    static async IAsyncEnumerable<Dictionary<string, object>> GetPluginSheets(
    IAsyncEnumerable<Device> data,
    ConcurrentDictionary<string, (object, Dictionary<string, PropertyInfo>)> propertysDict,
    string? plugin)
    {
        var enumerator = data.GetAsyncEnumerator();
        while (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            var device = enumerator.Current;
            var row = GetPluginRows(device, plugin, propertysDict);
            if (row != null)
            {
                yield return row;
            }
        }
    }


    public static async Task<Dictionary<string, ImportPreviewOutputBase>> ImportAsync(USheetDatas uSheetDatas)
    {
        var dataScope = await GlobalData.SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        // 获取所有设备，并将设备名称作为键构建设备字典
        var deviceDicts = GlobalData.Devices;

        // 获取所有通道，并将通道名称作为键构建通道字典
        var channelDicts = GlobalData.Channels;

        // 导入检验结果的预览字典，键为名称，值为导入预览对象
        Dictionary<string, ImportPreviewOutputBase> ImportPreviews = new();

        // 设备页的导入预览对象
        ImportPreviewOutput<Device> deviceImportPreview = new();

        // 获取所有驱动程序，并将驱动程序的完整名称作为键构建字典
        var driverPluginFullNameDict = GlobalData.PluginService.GetPluginList().ToDictionary(a => a.FullName);

        // 获取所有驱动程序，并将驱动程序名称作为键构建字典
        var driverPluginNameDict = GlobalData.PluginService.GetPluginList().DistinctBy(a => a.Name).ToDictionary(a => a.Name);
        ConcurrentDictionary<string, (Type, Dictionary<string, PropertyInfo>, Dictionary<string, PropertyInfo>)> propertysDict = new();

        var sheetNames = uSheetDatas.sheets.Keys.ToList();
        foreach (var sheetName in sheetNames)
        {
            List<IDictionary<string, object>> rows = new();
            var first = uSheetDatas.sheets[sheetName].cellData[0];

            foreach (var item in uSheetDatas.sheets[sheetName].cellData)
            {
                if (item.Key == 0)
                {
                    continue;
                }
                var expando = new Dictionary<string, object>();
                foreach (var keyValue in item.Value)
                {
                    expando.Add(first[keyValue.Key].v?.ToString(), keyValue.Value.v);
                }
                rows.Add(expando);
            }

            GlobalData.DeviceService.SetDeviceData(dataScope, deviceDicts, channelDicts, ImportPreviews, ref deviceImportPreview, driverPluginNameDict, propertysDict, sheetName, rows);
            if (ImportPreviews.Any(a => a.Value.HasError))
            {
                throw new(ImportPreviews.FirstOrDefault(a => a.Value.HasError).Value.Results.FirstOrDefault(a => !a.Success).ErrorMessage ?? "error");
            }
        }
        return ImportPreviews;
    }
}