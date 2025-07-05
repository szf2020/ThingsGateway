//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Riok.Mapperly.Abstractions;

using ThingsGateway.Management;

namespace ThingsGateway.Gateway.Application;
[Mapper(UseDeepCloning = true, EnumMappingStrategy = EnumMappingStrategy.ByName, RequiredMappingStrategy = RequiredMappingStrategy.None)]
public static partial class GatewayMapper
{
    public static partial AlarmVariable AdaptAlarmVariable(this VariableRuntime src);
    public static partial DeviceBasicData AdaptDeviceBasicData(this DeviceRuntime src);
    public static partial IEnumerable<DeviceBasicData> AdaptIEnumerableDeviceBasicData(this IEnumerable<DeviceRuntime> src);
    public static partial List<DeviceBasicData> AdaptListDeviceBasicData(this IEnumerable<DeviceRuntime> src);
    public static partial VariableBasicData AdaptVariableBasicData(this VariableRuntime src);
    public static partial IEnumerable<VariableBasicData> AdaptIEnumerableVariableBasicData(this IEnumerable<VariableRuntime> src);
    public static partial List<VariableBasicData> AdaptListVariableBasicData(this IEnumerable<VariableRuntime> src);

    public static partial Variable AdaptVariable(this VariableRuntime src);

    [MapProperty(nameof(Variable.InitValue), nameof(VariableRuntime.Value))]
    public static partial VariableRuntime AdaptVariableRuntime(this Variable src);
    public static partial List<Variable> AdaptListVariable(this IEnumerable<Variable> src);

    public static partial DeviceRuntime AdaptDeviceRuntime(this Device src);

    public static partial ChannelRuntime AdaptChannelRuntime(this Channel src);

    public static partial List<VariableRuntime> AdaptListVariableRuntime(this IEnumerable<Variable> src);
    public static partial List<DeviceRuntime> AdaptListDeviceRuntime(this IEnumerable<Device> src);
    public static partial List<Device> AdaptListDevice(this IEnumerable<Device> src);

    public static partial List<ChannelRuntime> AdaptListChannelRuntime(this IEnumerable<Channel> src);
    public static partial List<Channel> AdaptListChannel(this IEnumerable<Channel> src);

    public static partial List<Variable> AdaptListVariable(this IEnumerable<VariableRuntime> src);

    public static partial List<DeviceDataWithValue> AdaptDeviceDataWithValue(this IEnumerable<DeviceRuntime> src);

    public static partial RedundancyOptions AdaptRedundancyOptions(this RedundancyOptions src);

    public static partial List<DeviceDataWithValue> AdaptListDeviceDataWithValue(this IEnumerable<DeviceRuntime> src);
    public static partial List<Channel> AdaptListChannel(this List<ChannelInput> src);
    public static partial List<Device> AdaptListDevice(this List<DeviceInput> src);
    public static partial List<Variable> AdaptListVariable(this List<VariableInput> src);
    public static partial Channel AdaptChannel(this Channel src);
    public static partial Device AdaptDevice(this Device src);
    public static partial Variable AdaptVariable(this Variable src);
    public static partial List<PluginInfo> AdaptListPluginInfo(this List<PluginInfo> src);
}


