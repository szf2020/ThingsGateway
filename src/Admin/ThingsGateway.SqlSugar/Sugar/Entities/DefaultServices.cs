namespace ThingsGateway.SqlSugar
{
    public static class DefaultServices
    {
        public static ICacheService ReflectionInoCache = ReflectionInoCacheService.Instance;
        public static ICacheService DataInoCache = null;
        public static ISerializeService Serialize = new SerializeService();
    }
}