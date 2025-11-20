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
/// 连读报文信息
/// </summary>
public class VariableSourceRead : IVariableSource<VariableRuntime>
{
    private List<VariableRuntime> _variableRuntimes = new List<VariableRuntime>();

    /// <summary>
    /// 离线原因
    /// </summary>
    public string? LastErrorMessage { get; set; }

    /// <summary>
    /// 读取长度
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// 读取地址
    /// </summary>
    public string RegisterAddress { get; set; }
    public IDeviceAddress DeviceAddress { get; set; }

    public string IntervalTime { get; set; }

    /// <summary>
    /// 需分配的变量列表
    /// </summary>
    public ICollection<VariableRuntime> Variables => _variableRuntimes;

    public void AddVariable(VariableRuntime variable)
    {
        variable.VariableSource = this;
        _variableRuntimes.Add(variable);
    }

    /// <inheritdoc/>
    public virtual void AddVariableRange(IEnumerable<VariableRuntime> variables)
    {
        foreach (var variable in variables)
        {
            variable.VariableSource = this;
        }
        _variableRuntimes.AddRange(variables);
    }
}
