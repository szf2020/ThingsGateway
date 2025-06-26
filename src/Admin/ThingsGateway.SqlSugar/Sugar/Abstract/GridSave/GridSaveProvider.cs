namespace ThingsGateway.SqlSugar
{
    /// <summary>网格数据保存提供者</summary>
    public class GridSaveProvider<T> where T : class, new()
    {
        /// <summary>SqlSugar上下文</summary>
        internal SqlSugarProvider Context { get; set; }
        /// <summary>原始数据列表</summary>
        internal List<T> OldList { get; set; }
        /// <summary>待保存数据列表</summary>
        internal List<T> SaveList { get; set; }
        /// <summary>是否包含第一层导航属性</summary>
        internal bool IsIncluesFirstAll { get; set; }
        /// <summary>忽略的列名数组</summary>
        internal string[] IgnoreColumnsSaveInclues { get; set; }

        /// <summary>执行保存命令</summary>
        public bool ExecuteCommand()
        {
            var deleteList = GetDeleteList();
            this.Context.Deleteable(deleteList).PageSize(1000).ExecuteCommand();
            if (IsIncludesSave())
            {
                this.Context.Utilities.PageEach(SaveList, 1000, pageList =>
                {
                    var options = new UpdateNavRootOptions() { IsInsertRoot = true };
                    this.Context.UpdateNav(pageList, options)
                    .IncludesAllFirstLayer(IgnoreColumnsSaveInclues).ExecuteCommand();
                });
            }
            else
            {
                this.Context.Storageable(SaveList).PageSize(1000).ExecuteCommand();
            }
            return true;
        }

        /// <summary>异步执行保存命令</summary>
        public async Task<bool> ExecuteCommandAsync()
        {
            var deleteList = GetDeleteList();
            await Context.Deleteable(deleteList).PageSize(1000).ExecuteCommandAsync().ConfigureAwait(false);
            if (IsIncludesSave())
            {
                await Context.Utilities.PageEachAsync(SaveList, 1000, async pageList =>
                {
                    var options = new UpdateNavRootOptions() { IsInsertRoot = true };
                    await Context.UpdateNav(pageList, options)
                    .IncludesAllFirstLayer(IgnoreColumnsSaveInclues).ExecuteCommandAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
            else
            {
                await Context.Storageable(SaveList).PageSize(1000).ExecuteCommandAsync().ConfigureAwait(false);
            }
            return true;
        }

        /// <summary>获取需要删除的数据列表</summary>
        public List<T> GetDeleteList()
        {
            string[] primaryKeys = this.Context.EntityMaintenance.GetEntityInfo<T>().Columns.Where(it => it.IsPrimarykey).Select(it => it.PropertyName).ToArray();
            var saveListDictionary = this.SaveList.Select(item =>
            new { Key = CreateCompositeKey(primaryKeys, item), Value = item });
            var deleteList = this.OldList.Where(oldItem =>
            {
                var compositeKey = CreateCompositeKey(primaryKeys, oldItem);
                return !saveListDictionary.Any(it => it.Key == compositeKey);
            }).ToList();
            return deleteList;
        }

        /// <summary>设置包含所有第一层导航属性</summary>
        public GridSaveProvider<T> IncludesAllFirstLayer(params string[] ignoreColumns)
        {
            this.IsIncluesFirstAll = true;
            IgnoreColumnsSaveInclues = ignoreColumns;
            return this;
        }

        /// <summary>判断是否需要包含导航属性保存</summary>
        private bool IsIncludesSave()
        {
            return IsIncluesFirstAll && this.Context.EntityMaintenance.GetEntityInfo<T>().Columns.Any(it => it.Navigat != null);
        }

        /// <summary>创建复合主键</summary>
        private string CreateCompositeKey(string[] propertyNames, object obj)
        {
            var keyValues = propertyNames.Select(propertyName => GetPropertyValue(obj, propertyName)?.ToString() ?? "");
            return string.Join("|", keyValues);
        }

        /// <summary>获取属性值</summary>
        private object GetPropertyValue(object obj, string propertyName)
        {
            var property = obj.GetType().GetProperty(propertyName);
            return property != null ? property.GetValue(obj) : null;
        }
    }
}