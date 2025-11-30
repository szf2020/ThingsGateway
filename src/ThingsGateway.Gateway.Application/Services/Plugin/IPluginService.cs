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

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 驱动插件服务
/// </summary>
public interface IPluginService
#if !Management
    : IPluginPageService
#endif
{
    Type GetDebugUI(string pluginName);
    Type GetAddressUI(string pluginName);

#if !Management
    /// <summary>
    /// 根据插件类型获取信息
    /// </summary>
    /// <param name="pluginType"></param>
    /// <returns></returns>
    List<PluginInfo> GetPluginList(PluginTypeEnum? pluginType = null);
#endif

    /// <summary>
    /// 根据插件全名称构建插件实例
    /// </summary>
    /// <param name="pluginName"></param>
    /// <returns></returns>
    DriverBase GetDriver(string pluginName);

    /// <summary>
    /// 获取插件动态注册的方法
    /// </summary>
    List<DriverMethodInfo> GetDriverMethodInfos(string pluginName, IDriver? driver = null);

    /// <summary>
    /// 获取插件属性
    /// </summary>
    /// <param name="pluginName"></param>
    /// <param name="driver"></param>
    /// <returns></returns>
    (IEnumerable<IEditorItem> EditorItems, object Model, Type PropertyUIType) GetDriverPropertyTypes(string pluginName, IDriver? driver = null);



    /// <summary>
    /// 获取变量属性
    /// </summary>
    /// <param name="pluginName"></param>
    /// <param name="businessBase"></param>
    /// <returns></returns>
    (IEnumerable<IEditorItem> EditorItems, object Model, Type VariablePropertyUIType) GetVariablePropertyTypes(string pluginName, BusinessBase? businessBase = null);


    /// <summary>
    /// 设置插件动态属性
    /// </summary>
    void SetDriverProperties(object protperties, Dictionary<string, string> deviceProperties);
}
