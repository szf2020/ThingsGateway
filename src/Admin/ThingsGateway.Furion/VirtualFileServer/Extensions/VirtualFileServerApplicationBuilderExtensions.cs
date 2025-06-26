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

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// 虚拟文件服务中间件
/// </summary>
[SuppressSniffer]
public static class VirtualFileServerApplicationBuilderExtensions
{
    /// <summary>
    /// 虚拟文件系统中间件
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseVirtualFileServer(this IApplicationBuilder app)
    {
        return app;
    }
}