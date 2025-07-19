namespace ThingsGateway.SqlSugar
{
    public partial class FastestProvider<T> : IFastest<T> where T : class, new()
    {
        /// <summary>表别名</summary>
        private string AsName { get; set; }
        /// <summary>分页大小</summary>
        private int Size { get; set; }
        /// <summary>缓存键</summary>
        private string CacheKey { get; set; }
        /// <summary>模糊缓存键</summary>
        private string CacheKeyLike { get; set; }
        /// <summary>字符集</summary>
        private string CharacterSet { get; set; }
        /// <summary>是否启用数据AOP</summary>
        private bool IsDataAop { get; set; }
        /// <summary>是否关闭自增</summary>
        private bool IsOffIdentity { get; set; }
        /// <summary>是否忽略插入错误</summary>
        private bool IsIgnoreInsertError { get; set; }

        /// <summary>设置字符集</summary>
        public IFastest<T> SetCharacterSet(string CharacterSet)
        {
            this.CharacterSet = CharacterSet;
            return this;
        }

        /// <summary>启用数据AOP</summary>
        public IFastest<T> EnableDataAop()
        {
            this.IsDataAop = true;
            return this;
        }
        public IFastest<T> IgnoreInsertError()
        {
            this.IsIgnoreInsertError = true;
            return this;
        }

        /// <summary>移除数据缓存</summary>
        public IFastest<T> RemoveDataCache()
        {
            CacheKey = typeof(T).FullName;
            return this;
        }

        /// <summary>移除指定缓存键的数据缓存</summary>
        public IFastest<T> RemoveDataCache(string cacheKey)
        {
            CacheKeyLike = this.context.EntityMaintenance.GetTableName<T>();
            return this;
        }

        /// <summary>设置表别名</summary>
        public IFastest<T> AS(string tableName)
        {
            this.AsName = tableName;
            return this;
        }

        /// <summary>设置分页大小</summary>
        public IFastest<T> PageSize(int size)
        {
            this.Size = size;
            return this;
        }

        /// <summary>关闭自增</summary>
        public IFastest<T> OffIdentity()
        {
            this.IsOffIdentity = true;
            return this;
        }

        /// <summary>分表操作</summary>
        public SplitFastest<T> SplitTable()
        {
            SplitFastest<T> result = new SplitFastest<T>();
            result.FastestProvider = this;
            return result;
        }
    }
}