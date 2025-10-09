namespace ThingsGateway.SqlSugar
{
    public interface ISerializeService
    {
        string SerializeObject(object value);
        T DeserializeObject<T>(string value);
    }
}
