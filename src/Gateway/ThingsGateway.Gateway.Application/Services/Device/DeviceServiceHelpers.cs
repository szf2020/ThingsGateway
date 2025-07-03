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

public static class DeviceServiceHelpers
{

    public static async Task<USheetDatas> ExportDeviceAsync(IEnumerable<Device> models)
    {
        var deviceDicts = (await GlobalData.DeviceService.GetAllAsync().ConfigureAwait(false)).ToDictionary(a => a.Id);
        var channelDicts = (await GlobalData.ChannelService.GetAllAsync().ConfigureAwait(false)).ToDictionary(a => a.Id);
        var pluginSheetNames = models.Select(a => a.ChannelId).Select(a =>
        {
            channelDicts.TryGetValue(a, out var channel);
            var pluginKey = channel?.PluginName;
            return (a, pluginKey);
        }).ToList();
        var data = ExportSheets(models, deviceDicts, channelDicts, pluginSheetNames); // IEnumerable 延迟执行
        return USheetDataHelpers.GetUSheetDatas(data);

    }


    public static Dictionary<string, object> ExportSheets(
        IEnumerable<Device>? data,
        Dictionary<long, Device>? deviceDicts,
    Dictionary<long, Channel> channelDicts,
IEnumerable<(long, string)>? pluginSheetNames,
        string? channelName = null)
    {
        if (data?.Any() != true)
            data = new List<Device>();


        var result = new Dictionary<string, object>();
        result.Add(ExportString.DeviceName, GetDeviceSheets(data, deviceDicts, channelDicts, channelName));
        ConcurrentDictionary<string, (object, Dictionary<string, PropertyInfo>)> propertysDict = new();


        foreach (var dName in pluginSheetNames.DistinctBy(a => a.Item2))
        {
            var filtResult = PluginServiceUtil.GetFileNameAndTypeName(dName.Item2);
            var ids = pluginSheetNames.Where(a => a.Item2 == dName.Item2).Select(a => a.Item1).ToHashSet();
            var pluginSheets = GetPluginSheets(data.Where(a => ids.Contains(a.ChannelId)), propertysDict, dName.Item2);
            result.Add(filtResult.TypeName, pluginSheets);
        }

        return result;
    }


    public static Dictionary<string, object> ExportSheets(
IAsyncEnumerable<Device>? data1,
IAsyncEnumerable<Device>? data2,
Dictionary<long, Device>? deviceDicts,
    Dictionary<long, Channel> channelDicts,
IEnumerable<(long, string)>? pluginSheetNames,
string? channelName = null)
    {
        if (data1 == null || data2 == null)
            return new();

        var result = new Dictionary<string, object>();
        result.Add(ExportString.DeviceName, GetDeviceSheets(data1, deviceDicts, channelDicts, channelName));
        ConcurrentDictionary<string, (object, Dictionary<string, PropertyInfo>)> propertysDict = new();

        foreach (var dName in pluginSheetNames.DistinctBy(a => a.Item2))
        {
            var filtResult = PluginServiceUtil.GetFileNameAndTypeName(dName.Item2);
            var ids = pluginSheetNames.Where(a => a.Item2 == dName.Item2).Select(a => a.Item1).ToHashSet();
            var pluginSheets = GetPluginSheets(data2.Where(a => ids.Contains(a.ChannelId)), propertysDict, dName.Item2);
            result.Add(filtResult.TypeName, pluginSheets);
        }

        return result;
    }




    static IEnumerable<Dictionary<string, object>> GetDeviceSheets(
    IEnumerable<Device> data,
Dictionary<long, Device>? deviceDicts,
    Dictionary<long, Channel> channelDicts,
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



    static async IAsyncEnumerable<Dictionary<string, object>> GetDeviceSheets(
    IAsyncEnumerable<Device> data,
Dictionary<long, Device>? deviceDicts,
    Dictionary<long, Channel> channelDicts,
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

    static Dictionary<string, object> GetDeviceRows(
Device device,
 IEnumerable<PropertyInfo>? propertyInfos,
 Type type,
Dictionary<long, Device>? deviceDicts,
 Dictionary<long, Channel>? channelDicts,
string? channelName)
    {

        Dictionary<string, object> devExport = new();
        deviceDicts.TryGetValue(device.RedundantDeviceId ?? 0, out var redundantDevice);
        channelDicts.TryGetValue(device.ChannelId, out var channel);

        devExport.Add(ExportString.ChannelName, channel?.Name ?? channelName);

        foreach (var item in propertyInfos)
        {
            //描述
            var desc = type.GetPropertyDisplayName(item.Name);
            //数据源增加
            devExport.Add(desc ?? item.Name, item.GetValue(device)?.ToString());
        }

        //设备实体没有包含冗余设备名称，手动插入
        devExport.Add(ExportString.RedundantDeviceName, redundantDevice?.Name);
        return devExport;
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
                driverInfo.Add(ExportString.DeviceName, device.Name);
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

    public static async Task<Dictionary<string, ImportPreviewOutputBase>> ImportAsync(USheetDatas uSheetDatas)
    {
        var dataScope = await GlobalData.SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        try
        {
            // 获取所有设备，并将设备名称作为键构建设备字典
            var deviceDicts = (await GlobalData.DeviceService.GetAllAsync().ConfigureAwait(false)).ToDictionary(a => a.Name);

            // 获取所有通道，并将通道名称作为键构建通道字典
            var channelDicts = (await GlobalData.ChannelService.GetAllAsync().ConfigureAwait(false)).ToDictionary(a => a.Name);

            // 导入检验结果的预览字典，键为名称，值为导入预览对象
            Dictionary<string, ImportPreviewOutputBase> ImportPreviews = new();

            // 设备页的导入预览对象
            ImportPreviewOutput<Device> deviceImportPreview = new();

            // 获取所有驱动程序，并将驱动程序的完整名称作为键构建字典
            var driverPluginFullNameDict = GlobalData.PluginService.GetList().ToDictionary(a => a.FullName);

            // 获取所有驱动程序，并将驱动程序名称作为键构建字典
            var driverPluginNameDict = GlobalData.PluginService.GetList().DistinctBy(a => a.Name).ToDictionary(a => a.Name);
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
        finally
        {
        }


    }

}