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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using ThingsGateway;

namespace System;

/// <summary>
/// <see cref="WebApplication"/> 方式配置选项
/// </summary>
[SuppressSniffer]
public sealed class MiniRunOptions : IRunOptions
{
    /// <summary>
    /// 内部构造函数
    /// </summary>
    internal MiniRunOptions()
    {
    }

    /// <summary>
    /// 默认配置
    /// </summary>
    public static MiniRunOptions Default { get; } = new MiniRunOptions();

    /// <summary>
    /// 默认配置（带启动参数）
    /// </summary>
    public static MiniRunOptions Main(string[] args)
    {
        return Default.WithArgs(args);
    }

    /// <summary>
    /// 默认配置（静默启动）
    /// </summary>
    public static MiniRunOptions DefaultSilence { get; } = new MiniRunOptions().Silence();

    /// <summary>
    /// 默认配置（静默启动 + 启动参数）
    /// </summary>
    public static MiniRunOptions MainSilence(string[] args)
    {
        return DefaultSilence.WithArgs(args);
    }

    /// <summary>
    /// 配置 <see cref="WebApplicationOptions"/>
    /// </summary>
    /// <param name="options"></param>
    /// <returns><see cref="MiniRunOptions"/></returns>
    public MiniRunOptions ConfigureOptions(WebApplicationOptions options)
    {
        Options = options;
        return this;
    }

    /// <summary>
    /// 配置 <see cref="IWebHostBuilder"/>
    /// </summary>
    /// <param name="configureAction"></param>
    /// <returns><see cref="MiniRunOptions"/></returns>
    public MiniRunOptions ConfigureBuilder(Action<IWebHostBuilder> configureAction)
    {
        ActionBuilder = configureAction;
        return this;
    }
    /// <summary>
    /// 配置 <see cref="IHostBuilder"/>
    /// </summary>
    /// <param name="configureAction"></param>
    /// <returns><see cref="MiniRunOptions"/></returns>
    public MiniRunOptions ConfigureFirstActionBuilder(Action<IHostBuilder> configureAction)
    {
        FirstActionBuilder = configureAction;
        return this;
    }

    /// <summary>
    /// 配置 <see cref="IServiceCollection"/>
    /// </summary>
    /// <param name="configureAction"></param>
    /// <returns><see cref="MiniRunOptions"/></returns>
    public MiniRunOptions ConfigureServices(Action<IServiceCollection> configureAction)
    {
        ActionServices = configureAction;
        return this;
    }

    /// <summary>
    /// 配置 <see cref="InjectOptions"/>
    /// </summary>
    /// <param name="configureAction"></param>
    /// <returns><see cref="MiniRunOptions"/></returns>
    public MiniRunOptions ConfigureInject(Action<IWebHostBuilder, InjectOptions> configureAction)
    {
        ActionInject = configureAction;
        return this;
    }

    /// <summary>
    /// 配置 <see cref="WebApplication"/>
    /// </summary>
    /// <param name="configureAction">配置委托</param>
    /// <returns><see cref="MiniRunOptions"/></returns>
    public MiniRunOptions Configure(Action<IHost> configureAction)
    {
        ActionConfigure = configureAction;
        return this;
    }

    /// <summary>
    /// 配置 <see cref="ConfigurationManager"/>
    /// </summary>
    /// <param name="configureAction">配置委托</param>
    /// <returns><see cref="MiniRunOptions"/></returns>
    public MiniRunOptions ConfigureConfiguration(Action<IHostEnvironment, IConfiguration> configureAction)
    {
        ActionConfigurationManager = configureAction;
        return this;
    }

    /// <summary>
    /// 添加应用服务组件
    /// </summary>
    /// <typeparam name="TComponent">组件类型</typeparam>
    /// <returns></returns>
    public MiniRunOptions AddComponent<TComponent>()
        where TComponent : class, IServiceComponent, new()
    {
        ServiceComponents.Add(typeof(TComponent), null);
        return this;
    }

    /// <summary>
    /// 添加应用服务组件
    /// </summary>
    /// <typeparam name="TComponent">组件类型</typeparam>
    /// <typeparam name="TComponentOptions"></typeparam>
    /// <param name="options">组件参数</param>
    /// <returns></returns>
    public MiniRunOptions AddComponent<TComponent, TComponentOptions>(TComponentOptions options)
        where TComponent : class, IServiceComponent, new()
    {
        ServiceComponents.Add(typeof(TComponent), options);
        return this;
    }

    /// <summary>
    /// 添加应用服务组件
    /// </summary>
    /// <param name="componentType">组件类型</param>
    /// <param name="options">组件参数</param>
    /// <returns></returns>
    public MiniRunOptions AddComponent(Type componentType, object options)
    {
        ServiceComponents.Add(componentType, options);
        return this;
    }

    /// <summary>
    /// 添加应用中间件组件
    /// </summary>
    /// <typeparam name="TComponent">组件类型</typeparam>
    /// <returns></returns>
    public MiniRunOptions UseComponent<TComponent>()
        where TComponent : class, IApplicationComponent, new()
    {
        ApplicationComponents.Add(typeof(TComponent), null);
        return this;
    }

    /// <summary>
    /// 添加应用中间件组件
    /// </summary>
    /// <typeparam name="TComponent">组件类型</typeparam>
    /// <typeparam name="TComponentOptions"></typeparam>
    /// <param name="options">组件参数</param>
    /// <returns></returns>
    public MiniRunOptions UseComponent<TComponent, TComponentOptions>(TComponentOptions options)
        where TComponent : class, IApplicationComponent, new()
    {
        ApplicationComponents.Add(typeof(TComponent), options);
        return this;
    }

    /// <summary>
    /// 添加应用中间件组件
    /// </summary>
    /// <param name="componentType">组件类型</param>
    /// <param name="options">组件参数</param>
    /// <returns></returns>
    public MiniRunOptions UseComponent(Type componentType, object options)
    {
        ApplicationComponents.Add(componentType, options);
        return this;
    }

    /// <summary>
    /// 添加 IWebHostBuilder 组件
    /// </summary>
    /// <typeparam name="TComponent">组件类型</typeparam>
    /// <returns></returns>
    public MiniRunOptions AddWebComponent<TComponent>()
        where TComponent : class, IWebComponent, new()
    {
        WebComponents.Add(typeof(TComponent), null);
        return this;
    }

    /// <summary>
    /// 添加 IWebHostBuilder 组件
    /// </summary>
    /// <typeparam name="TComponent">组件类型</typeparam>
    /// <typeparam name="TComponentOptions"></typeparam>
    /// <param name="options">组件参数</param>
    /// <returns></returns>
    public MiniRunOptions AddWebComponent<TComponent, TComponentOptions>(TComponentOptions options)
        where TComponent : class, IWebComponent, new()
    {
        WebComponents.Add(typeof(TComponent), options);
        return this;
    }

    /// <summary>
    /// 添加 IWebHostBuilder 组件
    /// </summary>
    /// <param name="componentType">组件类型</param>
    /// <param name="options">组件参数</param>
    /// <returns></returns>
    public MiniRunOptions AddWebComponent(Type componentType, object options)
    {
        WebComponents.Add(componentType, options);
        return this;
    }

    /// <summary>
    /// 标识主机静默启动
    /// </summary>
    /// <remarks>不阻塞程序运行</remarks>
    /// <param name="silence">静默启动</param>
    /// <param name="logging">静默启动日志状态，默认 false</param>
    /// <returns></returns>
    public MiniRunOptions Silence(bool silence = true, bool logging = false)
    {
        IsSilence = silence;
        SilenceLogging = logging;
        return this;
    }

    /// <summary>
    /// 设置进程启动参数
    /// </summary>
    /// <param name="args">启动参数</param>
    /// <returns></returns>
    public MiniRunOptions WithArgs(string[] args)
    {
        Args = args;
        return this;
    }

    /// <summary>
    /// <see cref="WebApplicationOptions"/>
    /// </summary>
    internal WebApplicationOptions Options { get; set; }

    /// <summary>
    /// 自定义 <see cref="IServiceCollection"/> 委托
    /// </summary>
    internal Action<IServiceCollection> ActionServices { get; set; }

    /// <summary>
    /// 自定义 <see cref="IWebHostBuilder"/> 委托
    /// </summary>
    internal Action<IHostBuilder> FirstActionBuilder { get; set; }

    /// <summary>
    /// 自定义 <see cref="IWebHostBuilder"/> 委托
    /// </summary>
    internal Action<IWebHostBuilder> ActionBuilder { get; set; }

    /// <summary>
    /// 自定义 <see cref="InjectOptions"/> 委托
    /// </summary>
    internal Action<IWebHostBuilder, InjectOptions> ActionInject { get; set; }

    /// <summary>
    /// 自定义 <see cref="IHost"/> 委托
    /// </summary>
    internal Action<IHost> ActionConfigure { get; set; }

    /// <summary>
    /// 自定义 <see cref="IConfiguration"/> 委托
    /// </summary>
    internal Action<IHostEnvironment, IConfiguration> ActionConfigurationManager { get; set; }

    /// <summary>
    /// 应用服务组件
    /// </summary>
    internal Dictionary<Type, object> ServiceComponents { get; set; } = new();

    /// <summary>
    /// IWebHostBuilder 组件
    /// </summary>
    internal Dictionary<Type, object> WebComponents { get; set; } = new();

    /// <summary>
    /// 应用中间件组件
    /// </summary>
    internal Dictionary<Type, object> ApplicationComponents { get; set; } = new();

    /// <summary>
    /// 静默启动
    /// </summary>
    /// <remarks>不阻塞程序运行</remarks>
    internal bool IsSilence { get; private set; }

    /// <summary>
    /// 静默启动日志状态
    /// </summary>
    internal bool SilenceLogging { get; set; }

    /// <summary>
    /// 命令行参数
    /// </summary>
    internal string[] Args { get; set; }
}