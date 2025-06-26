// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// ------------------------------------------------------------------------

namespace ThingsGateway.HttpRemote;

/// <summary>
///     HTTP 声明式路径片段特性
/// </summary>
/// <remarks>支持多次指定。</remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Parameter,
    AllowMultiple = true)]
public sealed class PathSegmentAttribute : Attribute
{
    /// <summary>
    ///     <inheritdoc cref="PathSegmentAttribute" />
    /// </summary>
    /// <remarks>特性作用于参数时有效。</remarks>
    public PathSegmentAttribute()
    {
    }

    /// <summary>
    ///     <inheritdoc cref="PathSegmentAttribute" />
    /// </summary>
    /// <remarks>
    ///     <para>当特性作用于方法或接口时，则表示添加指定路径片段操作。</para>
    ///     <para>当特性作用于参数且参数值为 <c>null</c> 时，表示将路径片段设置为 <c>segment</c> 的值。</para>
    /// </remarks>
    /// <param name="segment">路径片段</param>
    public PathSegmentAttribute(string segment)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(segment);

        Segment = segment;
    }

    /// <summary>
    ///     路径片段
    /// </summary>
    public string? Segment { get; set; }

    /// <summary>
    ///     是否标记为待删除
    /// </summary>
    /// <remarks>默认为 <c>false</c>。设置为 <c>true</c> 表示移除路径片段。</remarks>
    public bool Remove { get; set; }
}