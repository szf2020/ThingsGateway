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

using System.ComponentModel;

namespace ThingsGateway.VirtualFileServer;

/// <summary>
/// 文件提供器类型
/// </summary>
[Description("文件提供器类型")]
public enum FileProviderTypes
{
    /// <summary>
    /// 物理文件
    /// </summary>
    [Description("物理文件")]
    Physical,

    /// <summary>
    /// 嵌入资源文件
    /// </summary>
    [Description("嵌入资源文件")]
    Embedded
}