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
using ThingsGateway.NewLife.Caching;

namespace ThingsGateway;

public enum CacheType
{
    /// <summary>
    /// 内存缓存
    /// </summary>
    Memory,
    /// <summary>
    /// Redis 缓存
    /// </summary>
    Redis
}

/// <summary>
/// 应用全局配置
/// </summary>
public sealed class CacheOptions : IConfigurableOptions<CacheOptions>
{
    public CacheType CacheType { get; set; }

    public MemoryCacheOptions MemoryCacheOptions { get; set; } = new MemoryCacheOptions();

    public RedisCacheOptions RedisCacheOptions { get; set; } = new RedisCacheOptions();

    /// <summary>
    /// 后期配置
    /// </summary>
    /// <param name="options"></param>
    /// <param name="configuration"></param>
    public void PostConfigure(CacheOptions options, IConfiguration configuration)
    {

    }
}
public class MemoryCacheOptions
{
    /// <summary>默认过期时间。避免Set操作时没有设置过期时间，默认3600秒</summary>
    public Int32 Expire { get; set; } = 3600;
    /// <summary>容量。容量超标时，采用LRU机制删除，默认100_000</summary>
    public Int32 Capacity { get; set; } = 100_000;

    /// <summary>定时清理时间，默认60秒</summary>
    public Int32 Period { get; set; } = 60;
}

public class RedisCacheOptions : RedisOptions
{

}