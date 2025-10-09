using System.Linq.Expressions;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 插入导航数据提供者(用于处理级联插入操作)
    /// </summary>
    public partial class InsertNavProvider<Root, T> where T : class, new() where Root : class, new()
    {
        /// <summary>
        /// 根插入选项
        /// </summary>
        public InsertNavRootOptions _RootOptions { get; set; }

        /// <summary>
        /// 根实体列表
        /// </summary>
        public List<Root> _Roots { get; set; }

        /// <summary>
        /// 父实体列表
        /// </summary>
        public List<object> _ParentList { get; set; }

        /// <summary>
        /// 根实体列表(对象形式)
        /// </summary>
        public List<object> _RootList { get; set; }

        /// <summary>
        /// 父实体信息
        /// </summary>
        public EntityInfo _ParentEntity { get; set; }

        /// <summary>
        /// 父实体主键列信息
        /// </summary>
        public EntityColumnInfo _ParentPkColumn { get; set; }

        /// <summary>
        /// SqlSugar提供者实例
        /// </summary>
        public SqlSugarProvider _Context { get; set; }

        /// <summary>
        /// 导航类型
        /// </summary>
        public NavigateType? _NavigateType { get; set; }

        /// <summary>
        /// 是否是第一次操作
        /// </summary>
        public bool IsFirst { get; set; }

        /// <summary>
        /// 插入导航选项
        /// </summary>
        public InsertNavOptions _navOptions { get; set; }

        /// <summary>
        /// 是否是导航操作
        /// </summary>
        public bool IsNav { get; internal set; }

        /// <summary>
        /// 导航上下文
        /// </summary>
        internal NavContext NavContext { get; set; }

        /// <summary>
        /// 作为导航操作入口
        /// </summary>
        /// <returns>插入导航提供者实例</returns>
        public InsertNavProvider<Root, Root> AsNav()
        {
            return new InsertNavProvider<Root, Root>
            {
                _Context = _Context,
                _ParentEntity = null,
                _ParentList = null,
                _Roots = _Roots,
                _ParentPkColumn = this._Context.EntityMaintenance.GetEntityInfo<Root>().Columns.First(it => it.IsPrimarykey)
            };
        }

        /// <summary>
        /// 包含导航属性并执行插入操作(单个对象带选项)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <param name="options">插入选项</param>
        /// <returns>插入导航提供者实例</returns>
        public InsertNavProvider<Root, TChild> ThenInclude<TChild>(Expression<Func<T, TChild>> expression, InsertNavOptions options) where TChild : class, new()
        {
            _navOptions = options;
            return _ThenInclude(expression);
        }

        /// <summary>
        /// 包含导航属性并执行插入操作(集合带选项)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <param name="options">插入选项</param>
        /// <returns>插入导航提供者实例</returns>
        public InsertNavProvider<Root, TChild> ThenInclude<TChild>(Expression<Func<T, List<TChild>>> expression, InsertNavOptions options) where TChild : class, new()
        {
            _navOptions = options;
            return _ThenInclude(expression);
        }

        /// <summary>
        /// 包含导航属性并执行插入操作(单个对象)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <returns>插入导航提供者实例</returns>
        public InsertNavProvider<Root, TChild> ThenInclude<TChild>(Expression<Func<T, TChild>> expression) where TChild : class, new()
        {
            return _ThenInclude(expression);
        }

        /// <summary>
        /// 包含导航属性并执行插入操作(集合)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <returns>插入导航提供者实例</returns>
        public InsertNavProvider<Root, TChild> ThenInclude<TChild>(Expression<Func<T, List<TChild>>> expression) where TChild : class, new()
        {
            return _ThenInclude(expression);
        }

        /// <summary>
        /// 内部方法：处理单个对象的导航插入
        /// </summary>
        private InsertNavProvider<Root, TChild> _ThenInclude<TChild>(Expression<Func<T, TChild>> expression) where TChild : class, new()
        {
            var name = ExpressionTool.GetMemberName(expression);
            var isRoot = false;
            if (this._ParentEntity == null)
            {
                this._ParentEntity = this._Context.EntityMaintenance.GetEntityInfo<Root>();
                this.IsFirst = true;
                isRoot = true;
            }
            var nav = this._ParentEntity.Columns.FirstOrDefault(x => x.PropertyName == name);
            if (nav.Navigat == null)
            {
                Check.ExceptionLang($"{name} no navigate attribute", $"{this._ParentEntity.EntityName}的属性{name}没有导航属性");
            }
            if (nav.Navigat.NavigatType == NavigateType.OneToOne || nav.Navigat.NavigatType == NavigateType.ManyToOne)
            {
                InitParentList();
                InsertOneToOne<TChild>(name, nav);
            }
            else if (nav.Navigat.NavigatType == NavigateType.OneToMany)
            {
                _NavigateType = NavigateType.OneToMany;
                InitParentList();
                InsertOneToMany<TChild>(name, nav);
            }
            else
            {
                InitParentList();
                InsertManyToMany<TChild>(name, nav);
            }
            AddContextInfo(name, isRoot);
            return GetResult<TChild>();
        }

        /// <summary>
        /// 内部方法：处理集合的导航插入
        /// </summary>
        private InsertNavProvider<Root, TChild> _ThenInclude<TChild>(Expression<Func<T, List<TChild>>> expression) where TChild : class, new()
        {
            var name = ExpressionTool.GetMemberName(expression);
            if (name == null)
            {
                name = ExpressionTool.GetMemberNameByMethod(expression, name);
            }
            var isRoot = false;
            if (this._ParentEntity == null)
            {
                this._ParentEntity = this._Context.EntityMaintenance.GetEntityInfo<Root>();
                IsFirst = true;
                isRoot = true;
            }
            var nav = this._ParentEntity.Columns.FirstOrDefault(x => x.PropertyName == name);
            if (nav.Navigat == null)
            {
                Check.ExceptionLang($"{name} no navigate attribute", $"{this._ParentEntity.EntityName}的属性{name}没有导航属性");
            }
            if (nav.Navigat.NavigatType == NavigateType.OneToOne || nav.Navigat.NavigatType == NavigateType.ManyToOne)
            {
                InitParentList();
                InsertOneToOne<TChild>(name, nav);
            }
            else if (nav.Navigat.NavigatType == NavigateType.OneToMany)
            {
                _NavigateType = NavigateType.OneToMany;
                InitParentList();
                InsertOneToMany<TChild>(name, nav);
            }
            else
            {
                InitParentList();
                InsertManyToMany<TChild>(name, nav);
            }
            AddContextInfo(name, isRoot);
            return GetResult<TChild>();
        }

        /// <summary>
        /// 添加上下文信息
        /// </summary>
        /// <param name="name">导航属性名称</param>
        /// <param name="isRoot">是否是根节点</param>
        private void AddContextInfo(string name, bool isRoot)
        {
            if (IsNav || isRoot)
            {
                if (this.NavContext?.Items != null)
                {
                    this.NavContext.Items.Add(new NavContextItem() { Level = 0, RootName = name });
                }
            }
        }

        /// <summary>
        /// 检查是否不存在指定名称的导航
        /// </summary>
        /// <param name="name">导航属性名称</param>
        /// <returns>是否不存在</returns>
        private bool NotAny(string name)
        {
            if (IsFirst) return true;
            if (this.NavContext == null) return true;
            return this.NavContext?.Items?.Any(it => it.RootName == name) == false;
        }
    }
}
