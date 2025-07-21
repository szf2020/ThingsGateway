#if NET6_0_OR_GREATER

using Microsoft.AspNetCore.DataProtection.Repositories;

using System.Xml.Linq;

using ThingsGateway.NewLife.Log;

namespace ThingsGateway.NewLife.Redis.Extensions;

/// <summary>在Redis中存储Xml</summary>
public class RedisXmlRepository : IXmlRepository
{
    private readonly ThingsGateway.NewLife.Caching.Redis? _redis;
    private readonly Func<ThingsGateway.NewLife.Caching.Redis>? _redisFactory;

    private readonly String _key;

    /// <summary>实例化</summary>
    /// <param name="redis"></param>
    /// <param name="key"></param>
    public RedisXmlRepository(ThingsGateway.NewLife.Caching.Redis redis, String key)
    {
        _redis = redis;
        _key = key;

        XTrace.WriteLine("DataProtection使用Redis持久化密钥，Key={0}", key);
    }

    /// <summary>实例化</summary>
    /// <param name="redisFactory"></param>
    /// <param name="key"></param>
    public RedisXmlRepository(Func<ThingsGateway.NewLife.Caching.Redis> redisFactory, String key)
    {
        _redisFactory = redisFactory;
        _key = key;

        XTrace.WriteLine("DataProtection使用Redis持久化密钥，Key={0}", key);
    }

    /// <summary>获取所有元素</summary>
    /// <returns></returns>
    public IReadOnlyCollection<XElement> GetAllElements() => GetAllElementsCore().ToList().AsReadOnly();

    /// <summary>遍历元素</summary>
    /// <returns></returns>
    private IEnumerable<XElement> GetAllElementsCore()
    {
        var rds = _redis ?? _redisFactory!();
        var list = rds.GetList<String>(_key) ?? [];
        foreach (var item in list)
        {
            yield return XElement.Parse(item);
        }
    }

    /// <summary>存储元素</summary>
    /// <param name="element"></param>
    /// <param name="friendlyName"></param>
    public void StoreElement(XElement element, String friendlyName)
    {
        var rds = _redis ?? _redisFactory!();
        var list = rds.GetList<String>(_key);
        list.Add(element.ToString(SaveOptions.DisableFormatting));
    }
}

#endif