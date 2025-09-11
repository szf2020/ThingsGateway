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

/// <summary>
/// 插件配置项
/// <br></br>
/// 使用<see cref="DynamicPropertyAttribute"/> 标识所需的配置属性
/// </summary>
public class MempryDevicePropertyBase : CollectPropertyBase
{

}
/// <summary>
/// <para></para>
/// 采集插件，继承实现不同PLC通讯
/// <para></para>
/// </summary>
public class MempryDevice : CollectBase
{
    private MempryDevicePropertyBase _driverPropertyBase = new MempryDevicePropertyBase();
    public override CollectPropertyBase CollectProperties => _driverPropertyBase;

#if !Management

    /// <summary>
    /// 是否连接成功
    /// </summary>
    public override bool IsConnected()
    {
        return true;
    }

    protected override Task<List<VariableSourceRead>> ProtectedLoadSourceReadAsync(List<VariableRuntime> deviceVariables)
    {
        return Task.FromResult(new List<VariableSourceRead>());
    }

    protected override bool IsRuntimeSourceValid(VariableRuntime a)
    {
        return false;
    }

#endif
}
