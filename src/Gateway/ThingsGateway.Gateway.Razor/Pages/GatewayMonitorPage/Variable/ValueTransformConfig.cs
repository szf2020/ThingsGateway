//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Gateway.Razor;

public class ValueTransformConfig
{
    /// <summary>
    /// 保留小数位
    /// </summary>
    public int DecimalPlaces { get; set; } = 2;

    /// <summary>
    /// 限制范围
    /// </summary>
    public bool ClampToRawRange { get; set; }

    public ValueTransformType TransformType { get; set; }

    /// <summary>
    /// 原始低
    /// </summary>
    public decimal RawMin { get; set; }
    /// <summary>
    /// 原始高
    /// </summary>
    public decimal RawMax { get; set; }
    /// <summary>
    /// 实际低
    /// </summary>
    public decimal ActualMin { get; set; }
    /// <summary>
    /// 实际高
    /// </summary>
    public decimal ActualMax { get; set; }
}

public enum ValueTransformType
{
    /// <summary>
    /// 不转换，仅保留小数位
    /// </summary>
    None,
    /// <summary>
    /// 线性转换
    /// </summary>
    Linear,
    /// <summary>
    /// 开方转换
    /// </summary>
    Sqrt
}
