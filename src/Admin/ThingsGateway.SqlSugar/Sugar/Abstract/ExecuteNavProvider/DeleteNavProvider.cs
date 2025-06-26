using System.Linq.Expressions;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 删除导航数据提供者(核心类，用于处理级联删除操作)
    /// </summary>
    public partial class DeleteNavProvider<Root, T> where T : class, new() where Root : class, new()
    {
        /// <summary>
        /// 删除导航选项
        /// </summary>
        internal DeleteNavOptions deleteNavOptions;

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
        /// 根删除选项
        /// </summary>
        internal DeleteNavRootOptions _RootOptions { get; set; }

        /// <summary>
        /// 是否已删除父实体
        /// </summary>
        public bool _IsDeletedParant { get; set; }

        /// <summary>
        /// 条件列表
        /// </summary>
        public List<string> _WhereList = new List<string>();

        /// <summary>
        /// 参数列表
        /// </summary>
        public List<SugarParameter> _Parameters = new List<SugarParameter>();

        /// <summary>
        /// 包含导航属性并执行删除操作(单个对象)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <returns>删除导航提供者实例</returns>
        public DeleteNavProvider<Root, TChild> ThenInclude<TChild>(Expression<Func<T, TChild>> expression)
            where TChild : class, new()
        {
            this._Context.InitMappingInfo<TChild>();
            InitParentList();
            Expression newExp = GetMamber(expression);
            var name = ExpressionTool.GetMemberName(expression);
            var nav = this._ParentEntity.Columns.FirstOrDefault(x => x.PropertyName == name);
            if (nav.Navigat == null)
            {
                Check.ExceptionEasy($"{name} no navigate attribute", $"{this._ParentEntity.EntityName}的属性{name}没有导航属性");
            }
            if (nav.Navigat.NavigatType == NavigateType.OneToOne || nav.Navigat.NavigatType == NavigateType.ManyToOne)
            {
                DeleteOneToOne<TChild>(name, nav);
            }
            else if (nav.Navigat.NavigatType == NavigateType.OneToMany)
            {
                DeleteOneToMany<TChild>(name, nav);
            }
            else
            {
                DeleteManyToMany<TChild>(name, nav);
            }
            return GetResult<TChild>();
        }

        /// <summary>
        /// 包含导航属性并执行删除操作(集合)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <returns>删除导航提供者实例</returns>
        public DeleteNavProvider<Root, TChild> ThenInclude<TChild>(Expression<Func<T, List<TChild>>> expression)
         where TChild : class, new()
        {
            this._Context.InitMappingInfo<TChild>();
            InitParentList();
            Expression newExp = GetMamber(expression);
            var name = ExpressionTool.GetMemberName(newExp);
            var nav = this._ParentEntity.Columns.FirstOrDefault(x => x.PropertyName == name);
            if (nav.Navigat == null)
            {
                Check.ExceptionEasy($"{name} no navigate attribute", $"{this._ParentEntity.EntityName}的属性{name}没有导航属性");
            }
            if (nav.Navigat.NavigatType == NavigateType.OneToOne || nav.Navigat.NavigatType == NavigateType.ManyToOne)
            {
                DeleteOneToOne<TChild>(name, nav);
            }
            else if (nav.Navigat.NavigatType == NavigateType.OneToMany)
            {
                DeleteOneToMany<TChild>(name, nav);
            }
            else
            {
                DeleteManyToMany<TChild>(name, nav);
            }
            return GetResult<TChild>();
        }

        /// <summary>
        /// 获取成员表达式
        /// </summary>
        /// <param name="expression">原始表达式</param>
        /// <returns>处理后的表达式</returns>
        private Expression GetMamber(Expression expression)
        {
            int i = 0;
            Expression newExp = ExpressionTool.GetLambdaExpressionBody(expression);
            while (newExp is MethodCallExpression)
            {
                var callMethod = (newExp as MethodCallExpression);
                ActionMethodCallExpression(callMethod);
                newExp = callMethod.Arguments[0];
                i++;
                Check.Exception(i > 10000, expression + "  is error");
            }
            return newExp;
        }

        /// <summary>
        /// 处理方法调用表达式
        /// </summary>
        /// <param name="method">方法调用表达式</param>
        private void ActionMethodCallExpression(MethodCallExpression method)
        {
            var queryBuilder = GetQueryBuilder();
            NavigatManager<T> navigatManager = new NavigatManager<T>()
            {
                Context = this._Context
            };
            if (method.Method.Name == "ToList")
            {
                // 不处理ToList方法
            }
            else if (method.Method.Name == "Where")
            {
                navigatManager.CheckHasRootShortName(method.Arguments[0], method.Arguments[1]);
                var exp = method.Arguments[1];
                _WhereList.Add(" " + queryBuilder.GetExpressionValue(exp, ResolveExpressType.WhereSingle).GetString());
            }
            else if (method.Method.Name == "WhereIF")
            {
                var isOk = LambdaExpression.Lambda(method.Arguments[1]).Compile().DynamicInvoke();
                if (isOk.ObjToBool())
                {
                    var exp = method.Arguments[2];
                    navigatManager.CheckHasRootShortName(method.Arguments[1], method.Arguments[2]);
                    _WhereList.Add(" " + queryBuilder.GetExpressionValue(exp, ResolveExpressType.WhereSingle).GetString());
                }
            }
            if (queryBuilder.Parameters != null)
            {
                _Parameters.AddRange(queryBuilder.Parameters);
            }
        }

        /// <summary>
        /// 获取查询构建器
        /// </summary>
        /// <returns>查询构建器实例</returns>
        private QueryBuilder GetQueryBuilder()
        {
            return this._Context.Queryable<T>().QueryBuilder;
        }

        /// <summary>
        /// 获取处理结果
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <returns>删除导航提供者实例</returns>
        private DeleteNavProvider<Root, TChild> GetResult<TChild>() where TChild : class, new()
        {
            return new DeleteNavProvider<Root, TChild>()
            {
                _Context = this._Context,
                _ParentEntity = this._ParentEntity,
                _ParentList = this._ParentList,
                _Roots = this._Roots,
                _ParentPkColumn = this._ParentPkColumn,
                _RootList = this._RootList,
                _IsDeletedParant = this._IsDeletedParant
            };
        }

        /// <summary>
        /// 作为导航操作入口
        /// </summary>
        /// <returns>删除导航提供者实例</returns>
        public DeleteNavProvider<Root, Root> AsNav()
        {
            return new DeleteNavProvider<Root, Root>
            {
                _Context = _Context,
                _ParentEntity = null,
                _ParentList = null,
                _Roots = _Roots,
                _IsDeletedParant = this._IsDeletedParant,
                _ParentPkColumn = this._Context.EntityMaintenance.GetEntityInfo<Root>().Columns.First(it => it.IsPrimarykey)
            };
        }

        /// <summary>
        /// 初始化父实体列表
        /// </summary>
        private void InitParentList()
        {
            this._ParentEntity = this._Context.EntityMaintenance.GetEntityInfo<T>();
            if (_RootList == null)
            {
                _RootList = _ParentList = _Roots.Cast<object>().ToList();
            }
            else if (_ParentList == null)
            {
                _ParentList = _RootList;
                var pkColumn = this._Context.EntityMaintenance.GetEntityInfo<T>().Columns.FirstOrDefault(it => it.IsPrimarykey);
                this._ParentPkColumn = pkColumn;
            }
        }
    }
}
