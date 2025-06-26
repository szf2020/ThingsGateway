using System.Linq.Expressions;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 插入导航任务初始化类
    /// </summary>
    /// <typeparam name="Root">根实体类型</typeparam>
    /// <typeparam name="T">当前实体类型</typeparam>
    public class InsertNavTaskInit<Root, T> where T : class, new() where Root : class, new()
    {
        /// <summary>
        /// SqlSugar提供者实例
        /// </summary>
        internal SqlSugarProvider Context { get; set; }

        /// <summary>
        /// 插入导航提供者实例
        /// </summary>
        internal InsertNavProvider<Root, Root> insertNavProvider { get; set; }

        /// <summary>
        /// 导航上下文
        /// </summary>
        internal NavContext NavContext { get; set; }

        /// <summary>
        /// 通过名称字符串包含导航属性
        /// </summary>
        /// <param name="navMemberName">导航属性名称</param>
        /// <param name="insertNavOptions">插入导航选项</param>
        /// <returns>插入导航方法信息</returns>
        public InsertNavMethodInfo IncludeByNameString(string navMemberName, InsertNavOptions insertNavOptions = null)
        {
            InsertNavMethodInfo result = new InsertNavMethodInfo();
            result.Context = insertNavProvider._Context;
            var entityInfo = result.Context.EntityMaintenance.GetEntityInfo<T>();
            Type properyItemType;
            bool isList;
            Expression exp = UtilMethods.GetIncludeExpression(navMemberName, entityInfo, out properyItemType, out isList);
            var method = this.GetType().GetMyMethod("Include", 2, isList)
                            .MakeGenericMethod(properyItemType);
            var obj = method.Invoke(this, new object[] { exp, insertNavOptions });
            result.MethodInfos = obj;
            return result;
        }

        /// <summary>
        /// 包含所有第一层导航属性(排除指定列)
        /// </summary>
        /// <param name="ignoreColumns">要忽略的列名</param>
        /// <returns>插入导航方法信息</returns>
        public InsertNavMethodInfo IncludesAllFirstLayer(params string[] ignoreColumns)
        {
            if (ignoreColumns == null)
            {
                ignoreColumns = Array.Empty<string>();
            }
            this.Context = insertNavProvider._Context;
            var navColumns = this.Context.EntityMaintenance.GetEntityInfo<Root>().Columns
                .Where(it => !ignoreColumns.Contains(it.PropertyName) || !ignoreColumns.Any(z => z.EqualCase(it.DbColumnName)))
                .Where(it => it.Navigat != null).ToList();
            var updateNavs = this;
            InsertNavMethodInfo methodInfo = updateNavs.IncludeByNameString(navColumns[0].PropertyName);
            foreach (var item in navColumns.Skip(1))
            {
                methodInfo = methodInfo.IncludeByNameString(item.PropertyName);
            }
            return methodInfo;
        }

        /// <summary>
        /// 包含所有第一层导航属性(带选项)
        /// </summary>
        /// <param name="insertNavOptions">插入导航选项</param>
        /// <param name="ignoreColumns">要忽略的列名</param>
        /// <returns>插入导航方法信息</returns>
        public InsertNavMethodInfo IncludesAllFirstLayer(InsertNavOptions insertNavOptions, params string[] ignoreColumns)
        {
            if (ignoreColumns == null)
            {
                ignoreColumns = Array.Empty<string>();
            }
            this.Context = insertNavProvider._Context;
            var navColumns = this.Context.EntityMaintenance.GetEntityInfo<Root>().Columns
                .Where(it => !ignoreColumns.Contains(it.PropertyName) || !ignoreColumns.Any(z => z.EqualCase(it.DbColumnName)))
                .Where(it => it.Navigat != null).ToList();
            var updateNavs = this;
            InsertNavMethodInfo methodInfo = updateNavs.IncludeByNameString(navColumns[0].PropertyName);
            foreach (var item in navColumns.Skip(1))
            {
                methodInfo = methodInfo.IncludeByNameString(item.PropertyName, insertNavOptions);
            }
            return methodInfo;
        }

        /// <summary>
        /// 包含导航属性(单个对象)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <returns>插入导航任务</returns>
        public InsertNavTask<Root, TChild> Include<TChild>(Expression<Func<Root, TChild>> expression) where TChild : class, new()
        {
            Check.ExceptionEasy(typeof(TChild).FullName.Contains("System.Collections.Generic.List`"),
                "  need  where T: class, new() ",
                "需要Class,new()约束，并且类属性中不能有required修饰符");
            this.Context = insertNavProvider._Context;
            insertNavProvider.NavContext = this.NavContext;
            InsertNavTask<Root, TChild> result = new InsertNavTask<Root, TChild>();
            Func<InsertNavProvider<Root, TChild>> func = () => insertNavProvider.ThenInclude(expression);
            result.PreFunc = func;
            result.Context = this.Context;
            result.NavContext = this.NavContext;
            return result;
        }

        /// <summary>
        /// 包含导航属性(集合)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <returns>插入导航任务</returns>
        public InsertNavTask<Root, TChild> Include<TChild>(Expression<Func<Root, List<TChild>>> expression) where TChild : class, new()
        {
            this.Context = insertNavProvider._Context;
            insertNavProvider.NavContext = this.NavContext;
            InsertNavTask<Root, TChild> result = new InsertNavTask<Root, TChild>();
            Func<InsertNavProvider<Root, TChild>> func = () => insertNavProvider.ThenInclude(expression);
            result.PreFunc = func;
            result.Context = this.Context;
            result.NavContext = this.NavContext;
            return result;
        }

        /// <summary>
        /// 包含导航属性(单个对象带选项)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <param name="options">插入选项</param>
        /// <returns>插入导航任务</returns>
        public InsertNavTask<Root, TChild> Include<TChild>(Expression<Func<Root, TChild>> expression, InsertNavOptions options) where TChild : class, new()
        {
            Check.ExceptionEasy(typeof(TChild).FullName.Contains("System.Collections.Generic.List`"),
                "  need  where T: class, new() ",
                "需要Class,new()约束，并且类属性中不能有required修饰符");
            this.Context = insertNavProvider._Context;
            insertNavProvider.NavContext = this.NavContext;
            InsertNavTask<Root, TChild> result = new InsertNavTask<Root, TChild>();
            Func<InsertNavProvider<Root, TChild>> func = () => insertNavProvider.ThenInclude(expression, options);
            result.PreFunc = func;
            result.Context = this.Context;
            result.NavContext = this.NavContext;
            return result;
        }

        /// <summary>
        /// 包含导航属性(集合带选项)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <param name="options">插入选项</param>
        /// <returns>插入导航任务</returns>
        public InsertNavTask<Root, TChild> Include<TChild>(Expression<Func<Root, List<TChild>>> expression, InsertNavOptions options) where TChild : class, new()
        {
            this.Context = insertNavProvider._Context;
            insertNavProvider.NavContext = this.NavContext;
            InsertNavTask<Root, TChild> result = new InsertNavTask<Root, TChild>();
            Func<InsertNavProvider<Root, TChild>> func = () => insertNavProvider.ThenInclude(expression, options);
            result.PreFunc = func;
            result.Context = this.Context;
            result.NavContext = this.NavContext;
            return result;
        }
    }

    /// <summary>
    /// 插入导航任务类
    /// </summary>
    /// <typeparam name="Root">根实体类型</typeparam>
    /// <typeparam name="T">当前实体类型</typeparam>
    public class InsertNavTask<Root, T> where T : class, new() where Root : class, new()
    {
        /// <summary>
        /// SqlSugar提供者实例
        /// </summary>
        public SqlSugarProvider Context { get; set; }

        /// <summary>
        /// 前置函数
        /// </summary>
        public Func<InsertNavProvider<Root, T>> PreFunc { get; set; }

        /// <summary>
        /// 导航上下文
        /// </summary>
        internal NavContext NavContext { get; set; }

        /// <summary>
        /// 包含下一级导航属性(单个对象)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <returns>插入导航任务</returns>
        public InsertNavTask<Root, TChild> ThenInclude<TChild>(Expression<Func<T, TChild>> expression) where TChild : class, new()
        {
            InsertNavTask<Root, TChild> result = new InsertNavTask<Root, TChild>();
            Func<InsertNavProvider<Root, TChild>> func = () =>
            {
                var nav = PreFunc().ThenInclude(expression);
                nav.NavContext = this.NavContext;
                return nav;
            };
            result.PreFunc = func;
            result.Context = this.Context;
            result.NavContext = this.NavContext;
            return result;
        }

        /// <summary>
        /// 包含下一级导航属性(集合)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <returns>插入导航任务</returns>
        public InsertNavTask<Root, TChild> ThenInclude<TChild>(Expression<Func<T, List<TChild>>> expression) where TChild : class, new()
        {
            InsertNavTask<Root, TChild> result = new InsertNavTask<Root, TChild>();
            Func<InsertNavProvider<Root, TChild>> func = () =>
            {
                var nav = PreFunc().ThenInclude(expression);
                nav.NavContext = this.NavContext;
                return nav;
            };
            result.PreFunc = func;
            result.Context = this.Context;
            result.NavContext = this.NavContext;
            return result;
        }

        /// <summary>
        /// 包含导航属性(单个对象)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <returns>插入导航任务</returns>
        public InsertNavTask<Root, TChild> Include<TChild>(Expression<Func<Root, TChild>> expression) where TChild : class, new()
        {
            return AsNav().ThenInclude(expression);
        }

        /// <summary>
        /// 包含导航属性(集合)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <returns>插入导航任务</returns>
        public InsertNavTask<Root, TChild> Include<TChild>(Expression<Func<Root, List<TChild>>> expression) where TChild : class, new()
        {
            return AsNav().ThenInclude(expression);
        }

        /// <summary>
        /// 包含下一级导航属性(单个对象带选项)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <param name="options">插入选项</param>
        /// <returns>插入导航任务</returns>
        public InsertNavTask<Root, TChild> ThenInclude<TChild>(Expression<Func<T, TChild>> expression, InsertNavOptions options) where TChild : class, new()
        {
            InsertNavTask<Root, TChild> result = new InsertNavTask<Root, TChild>();
            Func<InsertNavProvider<Root, TChild>> func = () => PreFunc().ThenInclude(expression, options);
            result.PreFunc = func;
            result.Context = this.Context;
            result.NavContext = this.NavContext;
            return result;
        }

        /// <summary>
        /// 包含下一级导航属性(集合带选项)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <param name="options">插入选项</param>
        /// <returns>插入导航任务</returns>
        public InsertNavTask<Root, TChild> ThenInclude<TChild>(Expression<Func<T, List<TChild>>> expression, InsertNavOptions options) where TChild : class, new()
        {
            InsertNavTask<Root, TChild> result = new InsertNavTask<Root, TChild>();
            Func<InsertNavProvider<Root, TChild>> func = () => PreFunc().ThenInclude(expression, options);
            result.PreFunc = func;
            result.Context = this.Context;
            result.NavContext = this.NavContext;
            return result;
        }

        /// <summary>
        /// 包含导航属性(单个对象带选项)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <param name="options">插入选项</param>
        /// <returns>插入导航任务</returns>
        public InsertNavTask<Root, TChild> Include<TChild>(Expression<Func<Root, TChild>> expression, InsertNavOptions options) where TChild : class, new()
        {
            return AsNav().ThenInclude(expression, options);
        }

        /// <summary>
        /// 包含导航属性(集合带选项)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <param name="options">插入选项</param>
        /// <returns>插入导航任务</returns>
        public InsertNavTask<Root, TChild> Include<TChild>(Expression<Func<Root, List<TChild>>> expression, InsertNavOptions options) where TChild : class, new()
        {
            return AsNav().ThenInclude(expression, options);
        }

        /// <summary>
        /// 执行并返回实体
        /// </summary>
        /// <returns>根实体</returns>
        public Root ExecuteReturnEntity()
        {
            var hasTran = this.Context.Ado.Transaction != null;
            if (hasTran)
            {
                return (Root)PreFunc()?._RootList?.FirstOrDefault();
            }
            else
            {
                Root result = null;
                this.Context.Ado.UseTran(() =>
                {
                    result = (Root)PreFunc()?._RootList?.FirstOrDefault();
                }, ex => throw ex);
                return result;
            }
        }

        /// <summary>
        /// 异步执行并返回实体
        /// </summary>
        /// <returns>根实体</returns>
        public async Task<Root> ExecuteReturnEntityAsync()
        {
            Root result = null;
            await Task.Run(async () =>
            {
                result = ExecuteReturnEntity();
                await Task.Delay(0).ConfigureAwait(false);
            }).ConfigureAwait(false);
            return result;
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
        /// 转换为导航操作
        /// </summary>
        /// <returns>插入导航任务</returns>
        private InsertNavTask<Root, Root> AsNav()
        {
            InsertNavTask<Root, Root> result = new InsertNavTask<Root, Root>();
            Func<InsertNavProvider<Root, Root>> func = () =>
            {
                var navas = PreFunc().AsNav();
                navas.NavContext = this.NavContext;
                navas.IsNav = true;
                return navas;
            };
            result.PreFunc = func;
            result.Context = this.Context;
            result.NavContext = this.NavContext;
            return result;
        }
    }

}
