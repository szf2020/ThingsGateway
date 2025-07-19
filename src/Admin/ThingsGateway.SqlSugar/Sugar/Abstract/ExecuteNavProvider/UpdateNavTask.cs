using System.Linq.Expressions;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 更新导航任务初始化类
    /// </summary>
    /// <typeparam name="Root">根实体类型</typeparam>
    /// <typeparam name="T">当前实体类型</typeparam>
    public class UpdateNavTaskInit<Root, T> where T : class, new() where Root : class, new()
    {
        /// <summary>
        /// SqlSugar上下文
        /// </summary>
        internal SqlSugarProvider Context { get; set; }

        /// <summary>
        /// 更新导航提供者
        /// </summary>
        internal UpdateNavProvider<Root, Root> UpdateNavProvider { get; set; }

        /// <summary>
        /// 导航上下文
        /// </summary>
        internal NavContext NavContext { get; set; }

        /// <summary>
        /// 包含子实体(单个)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <returns>更新导航任务</returns>
        public UpdateNavTask<Root, TChild> Include<TChild>(Expression<Func<Root, TChild>> expression) where TChild : class, new()
        {
            this.Context = UpdateNavProvider._Context;
            UpdateNavProvider.NavContext = this.NavContext;
            UpdateNavTask<Root, TChild> result = new UpdateNavTask<Root, TChild>();
            Func<UpdateNavProvider<Root, TChild>> func = () => UpdateNavProvider.ThenInclude(expression);
            result.PreFunc = func;
            result.Context = this.Context;
            result.NavContext = this.NavContext;
            return result;
        }

        /// <summary>
        /// 包含子实体(列表)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <returns>更新导航任务</returns>
        public UpdateNavTask<Root, TChild> Include<TChild>(Expression<Func<Root, List<TChild>>> expression) where TChild : class, new()
        {
            this.Context = UpdateNavProvider._Context;
            UpdateNavProvider.NavContext = this.NavContext;
            UpdateNavTask<Root, TChild> result = new UpdateNavTask<Root, TChild>();
            Func<UpdateNavProvider<Root, TChild>> func = () => UpdateNavProvider.ThenInclude(expression);
            result.PreFunc = func;
            result.Context = this.Context;
            result.NavContext = UpdateNavProvider.NavContext;
            return result;
        }

        /// <summary>
        /// 包含子实体(单个)并指定选项
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <param name="options">更新选项</param>
        /// <returns>更新导航任务</returns>
        public UpdateNavTask<Root, TChild> Include<TChild>(Expression<Func<Root, TChild>> expression, UpdateNavOptions options) where TChild : class, new()
        {
            this.Context = UpdateNavProvider._Context;
            UpdateNavProvider.NavContext = this.NavContext;
            UpdateNavTask<Root, TChild> result = new UpdateNavTask<Root, TChild>();
            Func<UpdateNavProvider<Root, TChild>> func = () => UpdateNavProvider.ThenInclude(expression, options);
            result.PreFunc = func;
            result.Context = this.Context;
            result.NavContext = UpdateNavProvider.NavContext;
            return result;
        }

        /// <summary>
        /// 包含子实体(列表)并指定选项
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <param name="options">更新选项</param>
        /// <returns>更新导航任务</returns>
        public UpdateNavTask<Root, TChild> Include<TChild>(Expression<Func<Root, List<TChild>>> expression, UpdateNavOptions options) where TChild : class, new()
        {
            this.Context = UpdateNavProvider._Context;
            UpdateNavProvider.NavContext = this.NavContext;
            UpdateNavTask<Root, TChild> result = new UpdateNavTask<Root, TChild>();
            Func<UpdateNavProvider<Root, TChild>> func = () => UpdateNavProvider.ThenInclude(expression, options);
            result.PreFunc = func;
            result.Context = this.Context;
            result.NavContext = UpdateNavProvider.NavContext;
            return result;
        }

        /// <summary>
        /// 包含所有第一层导航属性(排除指定列)
        /// </summary>
        /// <param name="ignoreColumns">要忽略的列名</param>
        /// <returns>更新导航方法信息</returns>
        public UpdateNavMethodInfo IncludesAllFirstLayer(params string[] ignoreColumns)
        {
            if (ignoreColumns == null)
            {
                ignoreColumns = Array.Empty<string>();
            }
            this.Context = UpdateNavProvider._Context;
            var navColumns = this.Context.EntityMaintenance.GetEntityInfo<Root>().Columns.Where(it => !ignoreColumns.Contains(it.PropertyName) || !ignoreColumns.Any(z => z.EqualCase(it.DbColumnName))).Where(it => it.Navigat != null).ToList();
            var updateNavs = this;
            UpdateNavMethodInfo methodInfo = updateNavs.IncludeByNameString(navColumns[0].PropertyName);
            foreach (var item in navColumns.Skip(1))
            {
                methodInfo = methodInfo.IncludeByNameString(item.PropertyName);
            }
            return methodInfo;
        }

        /// <summary>
        /// 包含所有第一层导航属性并指定选项(排除指定列)
        /// </summary>
        /// <param name="updateNavOptions">更新选项</param>
        /// <param name="ignoreColumns">要忽略的列名</param>
        /// <returns>更新导航方法信息</returns>
        public UpdateNavMethodInfo IncludesAllFirstLayer(UpdateNavOptions updateNavOptions, params string[] ignoreColumns)
        {
            if (ignoreColumns == null)
            {
                ignoreColumns = Array.Empty<string>();
            }
            this.Context = UpdateNavProvider._Context;
            var navColumns = this.Context.EntityMaintenance.GetEntityInfo<Root>().Columns.Where(it => !ignoreColumns.Contains(it.PropertyName) || !ignoreColumns.Any(z => z.EqualCase(it.DbColumnName))).Where(it => it.Navigat != null).ToList();
            var updateNavs = this;
            UpdateNavMethodInfo methodInfo = updateNavs.IncludeByNameString(navColumns[0].PropertyName);
            foreach (var item in navColumns.Skip(1))
            {
                methodInfo = methodInfo.IncludeByNameString(item.PropertyName, updateNavOptions);
            }
            return methodInfo;
        }

        /// <summary>
        /// 通过属性名包含导航属性
        /// </summary>
        /// <param name="navMemberName">导航属性名称</param>
        /// <param name="updateNavOptions">更新选项</param>
        /// <returns>更新导航方法信息</returns>
        public UpdateNavMethodInfo IncludeByNameString(string navMemberName, UpdateNavOptions updateNavOptions = null)
        {
            UpdateNavMethodInfo result = new UpdateNavMethodInfo();
            result.Context = UpdateNavProvider._Context;
            var entityInfo = result.Context.EntityMaintenance.GetEntityInfo<T>();
            Type propertyItemType;
            bool isList;
            Expression exp = UtilMethods.GetIncludeExpression(navMemberName, entityInfo, out propertyItemType, out isList);
            var method = this.GetType().GetMyMethod("Include", 2, isList)
                            .MakeGenericMethod(propertyItemType);
            var obj = method.Invoke(this, new object[] { exp, updateNavOptions });
            result.MethodInfos = obj;
            return result;
        }
    }

    /// <summary>
    /// 更新导航任务类
    /// </summary>
    /// <typeparam name="Root">根实体类型</typeparam>
    /// <typeparam name="T">当前实体类型</typeparam>
    public class UpdateNavTask<Root, T> where T : class, new() where Root : class, new()
    {
        /// <summary>
        /// SqlSugar上下文
        /// </summary>
        public SqlSugarProvider Context { get; set; }

        /// <summary>
        /// 前置函数
        /// </summary>
        public Func<UpdateNavProvider<Root, T>> PreFunc { get; set; }

        /// <summary>
        /// 导航上下文
        /// </summary>
        internal NavContext NavContext { get; set; }

        /// <summary>
        /// 包含子实体(单个)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <returns>更新导航任务</returns>
        public UpdateNavTask<Root, TChild> ThenInclude<TChild>(Expression<Func<T, TChild>> expression) where TChild : class, new()
        {
            UpdateNavTask<Root, TChild> result = new UpdateNavTask<Root, TChild>();
            Func<UpdateNavProvider<Root, TChild>> func = () => PreFunc().ThenInclude(expression);
            result.PreFunc = func;
            result.Context = this.Context;
            result.NavContext = this.NavContext;
            return result;
        }

        /// <summary>
        /// 包含子实体(列表)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <returns>更新导航任务</returns>
        public UpdateNavTask<Root, TChild> ThenInclude<TChild>(Expression<Func<T, List<TChild>>> expression) where TChild : class, new()
        {
            UpdateNavTask<Root, TChild> result = new UpdateNavTask<Root, TChild>();
            Func<UpdateNavProvider<Root, TChild>> func = () => PreFunc().ThenInclude(expression);
            result.PreFunc = func;
            result.Context = this.Context;
            result.NavContext = this.NavContext;
            return result;
        }

        /// <summary>
        /// 包含子实体(单个)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <returns>更新导航任务</returns>
        public UpdateNavTask<Root, TChild> Include<TChild>(Expression<Func<Root, TChild>> expression) where TChild : class, new()
        {
            return AsNav().ThenInclude(expression);
        }

        /// <summary>
        /// 包含子实体(列表)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <returns>更新导航任务</returns>
        public UpdateNavTask<Root, TChild> Include<TChild>(Expression<Func<Root, List<TChild>>> expression) where TChild : class, new()
        {
            return AsNav().ThenInclude(expression);
        }

        /// <summary>
        /// 包含子实体(单个)并指定选项
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <param name="options">更新选项</param>
        /// <returns>更新导航任务</returns>
        public UpdateNavTask<Root, TChild> ThenInclude<TChild>(Expression<Func<T, TChild>> expression, UpdateNavOptions options) where TChild : class, new()
        {
            UpdateNavTask<Root, TChild> result = new UpdateNavTask<Root, TChild>();
            Func<UpdateNavProvider<Root, TChild>> func = () =>
            {
                var nav = PreFunc().ThenInclude(expression, options);
                nav.NavContext = this.NavContext;
                return nav;
            };
            result.PreFunc = func;
            result.Context = this.Context;
            return result;
        }

        /// <summary>
        /// 包含子实体(列表)并指定选项
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <param name="options">更新选项</param>
        /// <returns>更新导航任务</returns>
        public UpdateNavTask<Root, TChild> ThenInclude<TChild>(Expression<Func<T, List<TChild>>> expression, UpdateNavOptions options) where TChild : class, new()
        {
            UpdateNavTask<Root, TChild> result = new UpdateNavTask<Root, TChild>();
            Func<UpdateNavProvider<Root, TChild>> func = () =>
            {
                var nav = PreFunc().ThenInclude(expression, options);
                result.NavContext = this.NavContext;
                return nav;
            };
            result.PreFunc = func;
            result.Context = this.Context;
            return result;
        }

        /// <summary>
        /// 包含子实体(单个)并指定选项
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <param name="options">更新选项</param>
        /// <returns>更新导航任务</returns>
        public UpdateNavTask<Root, TChild> Include<TChild>(Expression<Func<Root, TChild>> expression, UpdateNavOptions options) where TChild : class, new()
        {
            return AsNav().ThenInclude(expression, options);
        }

        /// <summary>
        /// 包含子实体(列表)并指定选项
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <param name="options">更新选项</param>
        /// <returns>更新导航任务</returns>
        public UpdateNavTask<Root, TChild> Include<TChild>(Expression<Func<Root, List<TChild>>> expression, UpdateNavOptions options) where TChild : class, new()
        {
            return AsNav().ThenInclude(expression, options);
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <returns>是否成功</returns>
        public bool ExecuteCommand()
        {
            var hasTran = this.Context.Ado.Transaction != null;
            if (hasTran)
            {
                PreFunc();
            }
            else
            {
                this.Context.Ado.UseTran(() =>
                {
                    PreFunc();
                }, ex => throw ex);
            }
            return true;
        }

        /// <summary>
        /// 异步执行命令
        /// </summary>
        /// <returns>是否成功</returns>
        public async Task<bool> ExecuteCommandAsync()
        {
            await Task.Run(async () =>
            {
                ExecuteCommand();
                await Task.Delay(0).ConfigureAwait(false);
            }).ConfigureAwait(false);
            return true;
        }

        /// <summary>
        /// 作为导航属性操作
        /// </summary>
        /// <returns>更新导航任务</returns>
        private UpdateNavTask<Root, Root> AsNav()
        {
            UpdateNavTask<Root, Root> result = new UpdateNavTask<Root, Root>();
            Func<UpdateNavProvider<Root, Root>> func = () =>
            {
                var navres = PreFunc().AsNav();
                navres.IsAsNav = true;
                navres.NavContext = this.NavContext;
                return navres;
            };
            result.PreFunc = func;
            result.Context = this.Context;
            result.NavContext = this.NavContext;
            return result;
        }
    }
}