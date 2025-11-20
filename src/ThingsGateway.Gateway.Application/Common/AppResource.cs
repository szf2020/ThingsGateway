//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Gateway.Application;

public static class AppResource
{
    public static string RulesEngineTaskStart => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.RulesEngineTaskStart : EnglishResource.RulesEngineTaskStart;

    public static string RealAlarmTaskStart => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.RealAlarmTaskStart : EnglishResource.RealAlarmTaskStart;
    public static string RealAlarmTaskStop => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.RealAlarmTaskStop : EnglishResource.RealAlarmTaskStop;
    public static string IntervalInsertAlarmFail => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.IntervalInsertAlarmFail : EnglishResource.IntervalInsertAlarmFail;
    public static string IntervalInsertDeviceFail => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.IntervalInsertDeviceFail : EnglishResource.IntervalInsertDeviceFail;
    public static string IntervalInsertVariableFail => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.IntervalInsertVariableFail : EnglishResource.IntervalInsertVariableFail;
    public static string PluginNotFound => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.PluginNotFound : EnglishResource.PluginNotFound;
    public static string WriteVariable => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.WriteVariable : EnglishResource.WriteVariable;
    public static string VariablePackError => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.VariablePackError : EnglishResource.VariablePackError;
    public static string GetMethodError => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.GetMethodError : EnglishResource.GetMethodError;
    public static string MethodNotNull => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.MethodNotNull : EnglishResource.MethodNotNull;
    public static string MethodFail => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.MethodFail : EnglishResource.MethodFail;
    public static string CollectFail => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.CollectFail : EnglishResource.CollectFail;
    public static string CollectSuccess => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.CollectSuccess : EnglishResource.CollectSuccess;
    public static string WriteExpressionsError => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.WriteExpressionsError : EnglishResource.WriteExpressionsError;
    public static string ChannelCreate => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.ChannelCreate : EnglishResource.ChannelCreate;
    public static string ChannelDispose => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.ChannelDispose : EnglishResource.ChannelDispose;
    public static string InitFail => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.InitFail : EnglishResource.InitFail;
    public static string DeviceTaskContinue => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.DeviceTaskContinue : EnglishResource.DeviceTaskContinue;
    public static string DeviceTaskPause => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.DeviceTaskPause : EnglishResource.DeviceTaskPause;
    public static string DeviceTaskStart => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.DeviceTaskStart : EnglishResource.DeviceTaskStart;
    public static string DeviceTaskStartTimeout => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.DeviceTaskStartTimeout : EnglishResource.DeviceTaskStartTimeout;
    public static string DeviceTaskStop => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.DeviceTaskStop : EnglishResource.DeviceTaskStop;
    public static string AddPluginFile => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.AddPluginFile : EnglishResource.AddPluginFile;
    public static string LoadOtherFileFail => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.LoadOtherFileFail : EnglishResource.LoadOtherFileFail;
    public static string LoadPluginFail => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.LoadPluginFail : EnglishResource.LoadPluginFail;
    public static string LoadTypeFail1 => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.LoadTypeFail1 : EnglishResource.LoadTypeFail1;
    public static string LoadTypeFail2 => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.LoadTypeFail2 : EnglishResource.LoadTypeFail2;
    public static string LoadTypeSuccess => ThingsGateway.Foundation.AppResource.Lang == Language.Chinese ? ChineseResource.LoadTypeSuccess : EnglishResource.LoadTypeSuccess;
}

public static class ChineseResource
{
    public const string RulesEngineTaskStart = "规则引擎线程启动";

    public const string RealAlarmTaskStart = "实时报警服务启动";
    public const string RealAlarmTaskStop = "实时报警服务停止";
    public const string IntervalInsertAlarmFail = "间隔上传报警失败";
    public const string IntervalInsertDeviceFail = "间隔上传设备失败";
    public const string IntervalInsertVariableFail = "间隔上传变量失败";
    public const string PluginNotFound = "没有发现插件类型";
    public const string WriteVariable = "写入变量";
    public const string VariablePackError = "变量打包失败 {0} ";
    public const string GetMethodError = "插件方法初始化失败 {0} ";
    public const string MethodNotNull = "特殊方法变量 {0} 找不到执行方法 {1}，请检查现有方法列表";
    public const string MethodFail = "{0} - 执行方法 [{1}] - 失败  {2}";
    public const string CollectFail = "{0} - 采集[{1} - {2}] 数据失败  {3}";
    public const string CollectSuccess = "{0} - 采集[{1} - {2}] 数据成功  {3}";
    public const string WriteExpressionsError = " {0} 转换写入表达式 {1} 失败 {2} ";
    public const string ChannelCreate = "通道 {0} 创建";
    public const string ChannelDispose = "通道 {0} 销毁";
    public const string InitFail = "插件 {0} 设备 {1} 初始化失败";
    public const string DeviceTaskContinue = "设备 {0} 线程继续";
    public const string DeviceTaskPause = "设备 {0} 线程暂停";
    public const string DeviceTaskStart = "设备 {0} 线程开始";
    public const string DeviceTaskStartTimeout = "设备 {0} 线程启动超时 {1} s";
    public const string DeviceTaskStop = "设备 {0} 线程停止";

    public const string AddPluginFile = "添加插件文件 {0}";
    public const string LoadOtherFileFail = "尝试加载附属程序集 {0} 失败，如果此程序集为DllImport，可以忽略此警告。";
    public const string LoadPluginFail = "加载插件 {0} 失败";
    public const string LoadTypeFail1 = "加载插件 {0} 失败，插件类型不存在";
    public const string LoadTypeFail2 = "加载插件文件 {0} 失败，文件不存在";
    public const string LoadTypeSuccess = "加载插件 {0} 成功";
}

public static class EnglishResource
{
    public const string RulesEngineTaskStart = "Rules engine service started";

    public const string RealAlarmTaskStart = "Real-time alarm service started";
    public const string RealAlarmTaskStop = "Real-time alarm service stoped";

    public const string IntervalInsertAlarmFail = "Failed to upload alarms periodically";
    public const string IntervalInsertDeviceFail = "Failed to upload device data periodically";
    public const string IntervalInsertVariableFail = "Failed to upload variable data periodically";
    public const string PluginNotFound = "No plugin type found";
    public const string WriteVariable = "Write variable";
    public const string VariablePackError = "Failed to pack variable {0}";
    public const string GetMethodError = "Failed to initialize plugin method {0}";
    public const string MethodNotNull = "Special method variable {0} could not find method {1}, please check the available method list";
    public const string MethodFail = "{0} - Method [{1}] execution failed {2}";
    public const string CollectFail = "{0} - Data collection failed [{1} - {2}] {3}";
    public const string CollectSuccess = "{0} - Data collected successfully [{1} - {2}] {3}";
    public const string WriteExpressionsError = "{0} failed to convert write expression {1} {2}";
    public const string ChannelCreate = "Channel {0} created";
    public const string ChannelDispose = "Channel {0} disposed";
    public const string InitFail = "Plugin {0}, device {1} initialization failed";
    public const string DeviceTaskContinue = "Device {0} thread resumed";
    public const string DeviceTaskPause = "Device {0} thread paused";
    public const string DeviceTaskStart = "Device {0} thread started";
    public const string DeviceTaskStartTimeout = "Device {0} thread start timed out after {1} seconds";
    public const string DeviceTaskStop = "Device {0} thread stopped";

    public const string AddPluginFile = "Added plugin file {0}";
    public const string LoadOtherFileFail = "Failed to load dependent assembly {0}. If this is a DllImport assembly, this warning can be ignored.";
    public const string LoadPluginFail = "Failed to load plugin {0}";
    public const string LoadTypeFail1 = "Failed to load plugin {0}, plugin type not found";
    public const string LoadTypeFail2 = "Failed to load plugin file {0}, file does not exist";
    public const string LoadTypeSuccess = "Plugin {0} loaded successfully";
}