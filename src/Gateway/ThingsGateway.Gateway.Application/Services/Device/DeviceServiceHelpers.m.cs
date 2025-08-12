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

using ThingsGateway.Common.Extension;

namespace ThingsGateway.Gateway.Application;

public static partial class DeviceServiceHelpers
{
    public static USheetDatas ExportDevice(IEnumerable<Device> models)
    {
        var deviceDicts = GlobalData.IdDevices;
        var channelDicts = GlobalData.IdChannels;
        var pluginSheetNames = models.Select(a => a.ChannelId).Select(a =>
        {
            channelDicts.TryGetValue(a, out var channel);
            var pluginKey = channel?.PluginName;
            return pluginKey;
        }).ToHashSet();
        var data = ExportSheets(models, deviceDicts, channelDicts, pluginSheetNames); // IEnumerable 延迟执行
        return USheetDataHelpers.GetUSheetDatas(data);
    }

    public static Dictionary<string, object> ExportSheets(
    IEnumerable<Device>? data,
    IReadOnlyDictionary<long, DeviceRuntime>? deviceDicts,
IReadOnlyDictionary<long, ChannelRuntime> channelDicts,
HashSet<string> pluginSheetNames,
    string? channelName = null)
    {
        if (data?.Any() != true)
            data = new List<Device>();

        var result = new Dictionary<string, object>();
        result.Add(GatewayExportString.DeviceName, GetDeviceSheets(data, deviceDicts, channelDicts, channelName));
        ConcurrentDictionary<string, (object, Dictionary<string, PropertyInfo>)> propertysDict = new();

        foreach (var plugin in pluginSheetNames)
        {
            var filtered = FilterPluginDevices(data, plugin, channelDicts);
            var filtResult = PluginInfoUtil.GetFileNameAndTypeName(plugin);
            var pluginSheets = GetPluginSheets(filtered, propertysDict, plugin);
            result.Add(filtResult.TypeName, pluginSheets);
        }
        return result;
    }


    static IEnumerable<Dictionary<string, object>> GetDeviceSheets(
    IEnumerable<Device> data,
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

        foreach (var device in data)
        {
            yield return GetDeviceRows(device, propertyInfos, type, deviceDicts, channelDicts, channelName);
        }
    }
    static IEnumerable<Device> FilterPluginDevices(IEnumerable<Device> data, string plugin, IReadOnlyDictionary<long, ChannelRuntime> channelDicts)
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

    static Dictionary<string, object> GetDeviceRows(
Device device,
 IEnumerable<PropertyInfo>? propertyInfos,
 Type type,
IReadOnlyDictionary<long, DeviceRuntime>? deviceDicts,
 IReadOnlyDictionary<long, ChannelRuntime>? channelDicts,
string? channelName)
    {
        Dictionary<string, object> devExport = new();
        deviceDicts.TryGetValue(device.RedundantDeviceId ?? 0, out var redundantDevice);
        channelDicts.TryGetValue(device.ChannelId, out var channel);

        devExport.Add(GatewayExportString.ChannelName, channel?.Name ?? channelName);

        foreach (var item in propertyInfos)
        {
            //描述
            var desc = type.GetPropertyDisplayName(item.Name);
            //数据源增加
            devExport.Add(desc ?? item.Name, item.GetValue(device)?.ToString());
        }

        //设备实体没有包含冗余设备名称，手动插入
        devExport.Add(GatewayExportString.RedundantDeviceName, redundantDevice?.Name);
        return devExport;
    }




    static IEnumerable<Dictionary<string, object>> GetPluginSheets(
    IEnumerable<Device> data,
    ConcurrentDictionary<string, (object, Dictionary<string, PropertyInfo>)> propertysDict,
    string? plugin)
    {
        foreach (var device in data)
        {
            var row = GetPluginRows(device, plugin, propertysDict);
            if (row != null)
            {
                yield return row;
            }
        }
    }



    static Dictionary<string, object> GetPluginRows(Device device, string? plugin, ConcurrentDictionary<string, (object, Dictionary<string, PropertyInfo>)> propertysDict)
    {
        Dictionary<string, object> driverInfo = new();
        var propDict = device.DevicePropertys;
        if (!propertysDict.TryGetValue(plugin, out var propertys))
        {
            try
            {
                var driverProperties = GlobalData.PluginService.GetDriver(plugin).DriverProperties;
                propertys.Item1 = driverProperties;
                var driverPropertyType = driverProperties.GetType();
                propertys.Item2 = driverPropertyType.GetRuntimeProperties()
.Where(a => a.GetCustomAttribute<DynamicPropertyAttribute>() != null)
.ToDictionary(a => driverPropertyType.GetPropertyDisplayName(a.Name, a => a.GetCustomAttribute<DynamicPropertyAttribute>(true)?.Description), a => a);
                propertysDict.TryAdd(plugin, propertys);
            }
            catch
            {
            }
        }

        if (propertys.Item2 != null)
        {
            if (propertys.Item2.Count > 0)
            {
                //没有包含设备名称，手动插入
                driverInfo.Add(GatewayExportString.DeviceName, device.Name);
            }
            //根据插件的配置属性项生成列，从数据库中获取值或者获取属性默认值
            foreach (var item in propertys.Item2)
            {
                if (propDict.TryGetValue(item.Value.Name, out var dependencyProperty))
                {
                    driverInfo.Add(item.Key, dependencyProperty);
                }
                else
                {
                    //添加对应属性数据
                    driverInfo.Add(item.Key, ThingsGatewayStringConverter.Default.Serialize(null, item.Value.GetValue(propertys.Item1)));
                }
            }

            if (driverInfo.Count > 0)
                return driverInfo;
        }
        return null;
    }

}