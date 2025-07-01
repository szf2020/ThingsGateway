namespace ThingsGateway.NewLife.Caching;

/// <summary>Redis异常</summary>
public class RedisException : XException
{
    /// <summary>实例化Redis异常</summary>
    /// <param name="message"></param>
    public RedisException(String message) : base(message) { }

    /// <summary>实例化Redis异常</summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public RedisException(String message, Exception innerException) : base(message, innerException) { }

    public RedisException() : base()
    {
    }

    public RedisException(string format, params object?[] args) : base(format, args)
    {
    }

    public RedisException(Exception innerException, string format, params object?[] args) : base(innerException, format, args)
    {
    }

    public RedisException(Exception innerException) : base(innerException)
    {
    }
}