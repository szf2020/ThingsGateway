using System.Linq.Expressions;

namespace ThingsGateway.SqlSugar
{
    public partial class UpdateNavProvider<Root, T> where T : class, new() where Root : class, new()
    {
        /// <summary>
        /// 根节点更新选项
        /// </summary>
        internal UpdateNavRootOptions _RootOptions { get; set; }

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
        /// SqlSugar上下文
        /// </summary>
        public SqlSugarProvider _Context { get; set; }

        /// <summary>
        /// 更新选项
        /// </summary>
        public UpdateNavOptions _Options { get; set; }

        /// <summary>
        /// 是否首次操作
        /// </summary>
        public bool IsFirst { get; set; }

        /// <summary>
        /// 是否作为导航属性操作
        /// </summary>
        public bool IsAsNav { get; set; }

        /// <summary>
        /// 导航上下文
        /// </summary>
        internal NavContext NavContext { get; set; }

        /// <summary>
        /// 作为导航属性操作
        /// </summary>
        /// <returns>更新导航提供者</returns>
        public UpdateNavProvider<Root, Root> AsNav()
        {
            return new UpdateNavProvider<Root, Root>
            {
                _Context = _Context,
                _ParentEntity = null,
                _ParentList = null,
                _Roots = _Roots,
                _ParentPkColumn = this._Context.EntityMaintenance.GetEntityInfo<Root>().Columns.First(it => it.IsPrimarykey)
            };
        }

        /// <summary>
        /// 包含子实体(单个)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <returns>更新导航提供者</returns>
        public UpdateNavProvider<Root, TChild> ThenInclude<TChild>(Expression<Func<T, TChild>> expression) where TChild : class, new()
        {
            return _ThenInclude(expression);
        }

        /// <summary>
        /// 包含子实体(列表)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <returns>更新导航提供者</returns>
        public UpdateNavProvider<Root, TChild> ThenInclude<TChild>(Expression<Func<T, List<TChild>>> expression) where TChild : class, new()
        {
            return _ThenInclude(expression);
        }

        /// <summary>
        /// 包含子实体(单个)并指定选项
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <param name="options">更新选项</param>
        /// <returns>更新导航提供者</returns>
        public UpdateNavProvider<Root, TChild> ThenInclude<TChild>(Expression<Func<T, TChild>> expression, UpdateNavOptions options) where TChild : class, new()
        {
            _Options = options;
            return _ThenInclude(expression);
        }

        /// <summary>
        /// 包含子实体(列表)并指定选项
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <param name="options">更新选项</param>
        /// <returns>更新导航提供者</returns>
        public UpdateNavProvider<Root, TChild> ThenInclude<TChild>(Expression<Func<T, List<TChild>>> expression, UpdateNavOptions options) where TChild : class, new()
        {
            _Options = options;
            return _ThenInclude(expression);
        }

        /// <summary>
        /// 内部方法-包含子实体(单个)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <returns>更新导航提供者</returns>
        private UpdateNavProvider<Root, TChild> _ThenInclude<TChild>(Expression<Func<T, TChild>> expression) where TChild : class, new()
        {
            var isRoot = _RootList == null;
            IsFirst = isRoot && this._ParentList == null;
            InitParentList();
            var name = ExpressionTool.GetMemberName(expression);
            var nav = this._ParentEntity.Columns.FirstOrDefault(x => x.PropertyName == name);
            if (nav.Navigat == null)
            {
                Check.ExceptionEasy($"{name} no navigate attribute", $"{this._ParentEntity.EntityName}的属性{name}没有导航属性");
            }
            if (_RootOptions?.IsDisableUpdateRoot == true)
            {
                //Future
            }
            else
            {
                UpdateRoot(isRoot, nav);
            }
            IsFirst = false;
            if (nav.Navigat.NavigatType == NavigateType.OneToOne || nav.Navigat.NavigatType == NavigateType.ManyToOne)
            {
                UpdateOneToOne<TChild>(name, nav);
            }
            else if (nav.Navigat.NavigatType == NavigateType.OneToMany)
            {
                UpdateOneToMany<TChild>(name, nav);
            }
            else
            {
                UpdateManyToMany<TChild>(name, nav);
            }
            AddContextInfo(name, isRoot);
            return GetResult<TChild>();
        }

        /// <summary>
        /// 内部方法-包含子实体(列表)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <returns>更新导航提供者</returns>
        private UpdateNavProvider<Root, TChild> _ThenInclude<TChild>(Expression<Func<T, List<TChild>>> expression) where TChild : class, new()
        {
            var isRoot = _RootList == null;
            IsFirst = isRoot && this._ParentList == null;
            InitParentList();
            var name = ExpressionTool.GetMemberName(expression);
            if (name == null)
            {
                name = ExpressionTool.GetMemberNameByMethod(expression, name);
            }
            var nav = this._ParentEntity.Columns.FirstOrDefault(x => x.PropertyName == name);
            if (nav.Navigat == null)
            {
                Check.ExceptionEasy($"{name} no navigate attribute", $"{this._ParentEntity.EntityName}的属性{name}没有导航属性");
            }
            UpdateRoot(isRoot, nav);
            IsFirst = false;
            if (nav.Navigat.NavigatType == NavigateType.OneToOne || nav.Navigat.NavigatType == NavigateType.ManyToOne)
            {
                UpdateOneToOne<TChild>(name, nav);
            }
            else if (nav.Navigat.NavigatType == NavigateType.OneToMany)
            {
                UpdateOneToMany<TChild>(name, nav);
            }
            else
            {
                UpdateManyToMany<TChild>(name, nav);
            }
            AddContextInfo(name, isRoot);
            return GetResult<TChild>();
        }

        /// <summary>
        /// 更新根实体
        /// </summary>
        /// <param name="isRoot">是否根实体</param>
        /// <param name="nav">导航列信息</param>
        private void UpdateRoot(bool isRoot, EntityColumnInfo nav)
        {
            if (isRoot && nav.Navigat.NavigatType != NavigateType.ManyToMany && _RootOptions?.IsDisableUpdateRoot != true)
            {
                UpdateRoot();
            }
            else if (isRoot && _RootOptions?.IsInsertRoot == true && nav.Navigat.NavigatType == NavigateType.ManyToMany)
            {
                UpdateRoot();
            }
            else
            {
                if (_Options?.ManyToManyIsUpdateA == true)
                {
                    UpdateRoot();
                }
            }
        }

        /// <summary>
        /// 更新根实体
        /// </summary>
        private void UpdateRoot()
        {
            if (IsAsNav)
            {
                return;
            }
            if (_Options?.RootFunc != null)
            {
                var updateable = this._Context.Updateable(_Roots);
                var exp = _Options.RootFunc as Expression<Action<IUpdateable<Root>>>;
                Check.ExceptionEasy(exp == null, "UpdateOptions.RootFunc is error", "UpdateOptions.RootFunc");
                var com = exp.Compile();
                com(updateable);
                updateable.ExecuteCommand();
            }
            else if (IsFirst && _RootOptions != null)
            {
                var isInsert = _RootOptions.IsInsertRoot;
                if (isInsert)
                {
                    var newRoots = new List<Root>();
                    foreach (var item in _Roots)
                    {
                        var x = this._Context.Storageable(item).ToStorage();
                        if (x.InsertList.HasValue())
                        {
                            newRoots.Add(x.AsInsertable.IgnoreColumns(_RootOptions.IgnoreInsertColumns).EnableDiffLogEventIF(_RootOptions.IsDiffLogEvent, _RootOptions.DiffLogBizData).ExecuteReturnEntity());
                        }
                        else
                        {
                            x.AsUpdateable
                                .EnableDiffLogEventIF(_RootOptions.IsDiffLogEvent, _RootOptions.DiffLogBizData)
                                .UpdateColumns(_RootOptions.UpdateColumns)
                                .IgnoreColumns(_RootOptions.IgnoreColumns)
                                .IgnoreNullColumns(_RootOptions.IsIgnoreAllNullColumns)
                                .ExecuteCommandWithOptLockIF(_RootOptions?.IsOptLock, _RootOptions?.IsOptLock);
                            newRoots.Add(item);
                        }
                    }
                    _ParentList = _RootList = newRoots.Cast<object>().ToList();
                }
                else
                {
                    if (_Roots.Count == 1 && _RootOptions?.IsOptLock == true)
                    {
                        this._Context.Updateable(_Roots.First())
                          .EnableDiffLogEventIF(_RootOptions.IsDiffLogEvent, _RootOptions.DiffLogBizData)
                          .UpdateColumns(_RootOptions.UpdateColumns)
                          .IgnoreColumns(_RootOptions.IgnoreColumns)
                          .IgnoreNullColumns(_RootOptions.IsIgnoreAllNullColumns)
                          .ExecuteCommandWithOptLockIF(_RootOptions?.IsOptLock, _RootOptions?.IsOptLock);
                    }
                    else
                    {
                        this._Context.Updateable(_Roots)
                            .EnableDiffLogEventIF(_RootOptions.IsDiffLogEvent, _RootOptions.DiffLogBizData)
                            .UpdateColumns(_RootOptions.UpdateColumns)
                            .IgnoreColumns(_RootOptions.IgnoreColumns)
                            .IgnoreNullColumns(_RootOptions.IsIgnoreAllNullColumns)
                            .ExecuteCommandWithOptLockIF(_RootOptions?.IsOptLock, _RootOptions?.IsOptLock);
                    }
                }
            }
            else if (_RootOptions != null && _RootOptions?.IsDiffLogEvent == true)
            {
                this._Context.Updateable(_Roots).EnableDiffLogEvent(_RootOptions.DiffLogBizData).ExecuteCommand();
            }
            else
            {
                this._Context.Updateable(_Roots).ExecuteCommand();
            }
        }

        /// <summary>
        /// 添加上下文信息
        /// </summary>
        /// <param name="name">导航属性名称</param>
        /// <param name="isRoot">是否根实体</param>
        private void AddContextInfo(string name, bool isRoot)
        {
            if (IsAsNav || isRoot)
            {
                if (this.NavContext?.Items != null)
                {
                    this.NavContext.Items.Add(new NavContextItem() { Level = 0, RootName = name });
                }
            }
        }

        /// <summary>
        /// 检查是否不存在指定名称的导航属性
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