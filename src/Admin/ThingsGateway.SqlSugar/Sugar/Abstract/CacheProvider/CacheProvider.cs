namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 提供基于SqlSugar的缓存功能
    /// </summary>
    public class SugarCacheProvider
    {
        /// <summary>
        /// 获取SugarCacheProvider的单例实例
        /// </summary>
        public static SugarCacheProvider Instance { get; } = new SugarCacheProvider();

        /// <summary>
        /// 获取或设置缓存服务实例
        /// </summary>
        public ICacheService Servie { get; set; }

        /// <summary>
        /// 根据模糊匹配字符串移除缓存
        /// </summary>
        /// <param name="likeString">模糊匹配字符串</param>
        public void RemoveDataCache(string likeString)
        {
            if (Servie == null) return;
            CacheSchemeMain.RemoveCacheByLike(Servie, likeString);
        }

        /// <summary>
        /// 获取所有缓存键
        /// </summary>
        /// <returns>缓存键列表</returns>
        public List<string> GetAllKey()
        {
            if (Servie == null) return new List<string>();
            return Servie.GetAllKey<string>()?.ToList();
        }

        /// <summary>
        /// 添加缓存项（默认过期时间100天）
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        public void Add(string key, object value)
        {
            if (Servie == null) return;
            Servie.Add(key, value, 60 * 60 * 24 * 100);
        }

        /// <summary>
        /// 添加缓存项（指定过期时间）
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="seconds">过期时间（秒）</param>
        public void Add(string key, object value, int seconds)
        {
            if (Servie == null) return;
            Servie.Add(key, value, seconds);
        }

        /// <summary>
        /// 获取指定类型的缓存值
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <returns>缓存值</returns>
        public T Get<T>(string key)
        {
            if (Servie == null) return default(T);
            return Servie.Get<T>(key);
        }
    }
}