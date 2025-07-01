namespace ThingsGateway.NewLife.Data;

/// <summary>具有可读写的扩展数据</summary>
/// <remarks>
/// 仅限于扩展属性，不包括基本属性，区别于 IModel
/// </remarks>
public interface IExtend
{
    /// <summary>数据项</summary>
    IDictionary<String, Object?> Items { get; }

    /// <summary>设置 或 获取 数据项</summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Object? this[String key] { get; set; }
}
