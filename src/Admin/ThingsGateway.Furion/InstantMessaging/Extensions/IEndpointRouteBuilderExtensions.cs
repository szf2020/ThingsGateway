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

using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;

using System.Reflection;

using ThingsGateway;
using ThingsGateway.Extensions;
using ThingsGateway.InstantMessaging;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// 终点路由构建器拓展
/// </summary>
[SuppressSniffer]
public static class IEndpointRouteBuilderExtensions
{
    /// <summary>
    /// 扫描配置所有集线器
    /// </summary>
    /// <param name="endpoints"></param>
    public static void MapHubs(this IEndpointRouteBuilder endpoints)
    {
        // 扫描所有集线器类型并且贴有 [MapHub] 特性且继承 Hub 或 Hub<>
        var hubs = App.EffectiveTypes.Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass
            && u.IsDefined(typeof(MapHubAttribute), true)
            && (typeof(Hub).IsAssignableFrom(u) || u.HasImplementedRawGeneric(typeof(Hub<>))));

#pragma warning disable CA1851
        if (!hubs.Any()) return;

        // 反射获取 MapHub 拓展方法
        var mapHubMethod = typeof(HubEndpointRouteBuilderExtensions).GetMethods().Where(u => u.Name == "MapHub" && u.IsGenericMethod && u.GetParameters().Length == 3).FirstOrDefault();
        if (mapHubMethod == null) return;

        // 遍历所有集线器并注册
        foreach (var hub in hubs)
        {
            // 解析集线器特性
            var mapHubAttribute = hub.GetCustomAttribute<MapHubAttribute>(true);

            // 创建连接分发器委托
            Action<HttpConnectionDispatcherOptions> configureOptions = options =>
            {
                // 执行连接分发器选项配置
                hub.GetMethod("HttpConnectionDispatcherOptionsSettings", BindingFlags.Public | BindingFlags.Static)
                ?.Invoke(null, new object[] { options });
            };

            // 注册集线器
            var hubEndpointConventionBuilder = mapHubMethod.MakeGenericMethod(hub).Invoke(null, new object[] { endpoints, mapHubAttribute.Pattern, configureOptions }) as HubEndpointConventionBuilder;

            // 执行终点转换器配置
            hub.GetMethod("HubEndpointConventionBuilderSettings", BindingFlags.Public | BindingFlags.Static)
                ?.Invoke(null, new object[] { hubEndpointConventionBuilder });
        }
#pragma warning restore CA1851
    }
}