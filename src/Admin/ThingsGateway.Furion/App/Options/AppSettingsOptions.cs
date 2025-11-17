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

using Microsoft.Extensions.Configuration;

using ThingsGateway.ConfigurableOptions;

namespace ThingsGateway;

/// <summary>
/// 应用全局配置
/// </summary>
public sealed class AppSettingsOptions : IConfigurableOptions<AppSettingsOptions>
{

    /// <summary>
    /// 是否启用规范化文档
    /// </summary>
    public bool? InjectSpecificationDocument { get; set; }

    /// <summary>
    /// 是否启用引用程序集扫描
    /// </summary>
    public bool? EnabledReferenceAssemblyScan { get; set; }

    /// <summary>
    /// 外部程序集
    /// </summary>
    /// <remarks>扫描 dll 文件，如果是单文件发布，需拷贝放在根目录下</remarks>
    public string[] ExternalAssemblies { get; set; }

    /// <summary>
    /// 排除扫描的程序集
    /// </summary>
    public string[] ExcludeAssemblies { get; set; }

    /// <summary>
    /// 配置支持的包前缀名
    /// </summary>
    public string[] SupportPackageNamePrefixs { get; set; }

    /// <summary>
    /// 【部署】二级虚拟目录
    /// </summary>
    public string VirtualPath { get; set; }
    /// <summary>
    /// JSON 文件扫描配置
    /// </summary>
    public JsonFileScanner JsonFileScanner { get; set; }

    /// <summary>
    /// 是否禁用 AppStartup 自动扫描
    /// </summary>
    public bool? DisableAppStartupScan { get; set; }


    /// <summary>
    /// 后期配置
    /// </summary>
    /// <param name="options"></param>
    /// <param name="configuration"></param>
    public void PostConfigure(AppSettingsOptions options, IConfiguration configuration)
    {
        options.InjectSpecificationDocument ??= true;
        options.EnabledReferenceAssemblyScan ??= false;
        options.ExternalAssemblies ??= Array.Empty<string>();
        options.ExcludeAssemblies ??= Array.Empty<string>();
        options.SupportPackageNamePrefixs ??= Array.Empty<string>();
        options.VirtualPath ??= string.Empty;
        options.DisableAppStartupScan ??= false;
    }
}

/// <summary>
/// JSON 文件扫描配置
/// </summary>
/// <remarks>修复 docker 中挂载大文件数据卷导致启动缓慢的问题。</remarks>
public class JsonFileScanner
{
    /// <summary>
    /// 是否可选
    /// </summary>
    public bool Optional { get; set; } = true;

    /// <summary>
    /// 是否改变的时候重载
    /// </summary>
    public bool ReloadOnChange { get; set; } = true;
}