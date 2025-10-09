using System.Linq.Expressions;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 删除导航任务初始化类
    /// </summary>
    /// <typeparam name="Root">根实体类型</typeparam>
    /// <typeparam name="T">当前实体类型</typeparam>
    public class DeleteNavTaskInit<Root, T> where T : class, new() where Root : class, new()
    {
        /// <summary>
        /// 根实体列表
        /// </summary>
        internal List<T> Roots { get; set; }

        /// <summary>
        /// SqlSugar提供者实例
        /// </summary>
        internal SqlSugarProvider Context { get; set; }

        /// <summary>
        /// 删除导航提供者实例
        /// </summary>
        internal DeleteNavProvider<Root, Root> deleteNavProvider { get; set; }

        /// <summary>
        /// 包含所有第一层导航属性(排除指定列)
        /// </summary>
        /// <param name="ignoreColumns">要忽略的列名</param>
        /// <returns>删除导航方法信息</returns>
        public DeleteNavMethodInfo IncludesAllFirstLayer(params string[] ignoreColumns)
        {
            if (ignoreColumns == null)
            {
                ignoreColumns = Array.Empty<string>();
            }
            this.Context = deleteNavProvider._Context;
            var navColumns = this.Context.EntityMaintenance.GetEntityInfo<Root>().Columns
                .Where(it => (!ignoreColumns.Contains(it.PropertyName) || !ignoreColumns.Contains(it.DbColumnName)) && it.Navigat != null).ToList();
            var updateNavs = this;
            DeleteNavMethodInfo methodInfo = updateNavs.IncludeByNameString(navColumns[0].PropertyName);
            foreach (var item in navColumns.Skip(1))
            {
                methodInfo = methodInfo.IncludeByNameString(item.PropertyName);
            }
            return methodInfo;
        }

        /// <summary>
        /// 包含指定导航属性(单个对象)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <returns>删除导航任务</returns>
        public DeleteNavTask<Root, TChild> Include<TChild>(Expression<Func<Root, TChild>> expression) where TChild : class, new()
        {
            this.Context = deleteNavProvider._Context;
            DeleteNavTask<Root, TChild> result = new DeleteNavTask<Root, TChild>();
            Func<DeleteNavProvider<Root, TChild>> func = () => deleteNavProvider.ThenInclude(expression);
            result.PreFunc = func;
            result.Context = this.Context;
            return result;
        }

        /// <summary>
        /// 通过名称字符串包含导航属性
        /// </summary>
        /// <param name="navMemberName">导航属性名称</param>
        /// <param name="deleteNavOptions">删除导航选项</param>
        /// <returns>删除导航方法信息</returns>
        public DeleteNavMethodInfo IncludeByNameString(string navMemberName, DeleteNavOptions deleteNavOptions = null)
        {
            DeleteNavMethodInfo result = new DeleteNavMethodInfo();
            result.Context = deleteNavProvider._Context;
            var entityInfo = result.Context.EntityMaintenance.GetEntityInfo<T>();
            Type propertyItemType;
            bool isList;
            Expression exp = UtilMethods.GetIncludeExpression(navMemberName, entityInfo, out propertyItemType, out isList);
            var method = this.GetType().GetMyMethod("Include", 2, isList)
                            .MakeGenericMethod(propertyItemType);
            var obj = method.Invoke(this, new object[] { exp, deleteNavOptions });
            result.MethodInfos = obj;
            return result;
        }

        /// <summary>
        /// 包含指定导航属性(集合)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <returns>删除导航任务</returns>
        public DeleteNavTask<Root, TChild> Include<TChild>(Expression<Func<Root, List<TChild>>> expression) where TChild : class, new()
        {
            this.Context = deleteNavProvider._Context;
            DeleteNavTask<Root, TChild> result = new DeleteNavTask<Root, TChild>();
            Func<DeleteNavProvider<Root, TChild>> func = () => deleteNavProvider.ThenInclude(expression);
            result.PreFunc = func;
            result.Context = this.Context;
            return result;
        }

        /// <summary>
        /// 包含指定导航属性(带选项)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <param name="deleteNavOptions">删除导航选项</param>
        /// <returns>删除导航任务</returns>
        public DeleteNavTask<Root, TChild> Include<TChild>(Expression<Func<Root, List<TChild>>> expression, DeleteNavOptions deleteNavOptions) where TChild : class, new()
        {
            var result = Include(expression);
            deleteNavProvider.deleteNavOptions = deleteNavOptions;
            return result;
        }
    }

    /// <summary>
    /// 删除导航任务类
    /// </summary>
    /// <typeparam name="Root">根实体类型</typeparam>
    /// <typeparam name="T">当前实体类型</typeparam>
    public class DeleteNavTask<Root, T> where T : class, new() where Root : class, new()
    {
        /// <summary>
        /// SqlSugar提供者实例
        /// </summary>
        public SqlSugarProvider Context { get; set; }

        /// <summary>
        /// 前置函数
        /// </summary>
        public Func<DeleteNavProvider<Root, T>> PreFunc { get; set; }

        /// <summary>
        /// 包含下一级导航属性(单个对象)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <returns>删除导航任务</returns>
        public DeleteNavTask<Root, TChild> ThenInclude<TChild>(Expression<Func<T, TChild>> expression) where TChild : class, new()
        {
            DeleteNavTask<Root, TChild> result = new DeleteNavTask<Root, TChild>();
            Func<DeleteNavProvider<Root, TChild>> func = () => PreFunc().ThenInclude(expression);
            result.PreFunc = func;
            result.Context = this.Context;
            return result;
        }

        /// <summary>
        /// 包含下一级导航属性(带选项)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <param name="deleteNavOptions">删除导航选项</param>
        /// <returns>删除导航任务</returns>
        public DeleteNavTask<Root, TChild> ThenInclude<TChild>(Expression<Func<T, TChild>> expression, DeleteNavOptions deleteNavOptions) where TChild : class, new()
        {
            DeleteNavTask<Root, TChild> result = new DeleteNavTask<Root, TChild>();
            Func<DeleteNavProvider<Root, TChild>> func = () =>
            {
                var dev = PreFunc();
                dev.deleteNavOptions = deleteNavOptions;
                return dev.ThenInclude(expression);
            };
            result.PreFunc = func;
            result.Context = this.Context;
            return result;
        }

        /// <summary>
        /// 包含下一级导航属性(集合)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <returns>删除导航任务</returns>
        public DeleteNavTask<Root, TChild> ThenInclude<TChild>(Expression<Func<T, List<TChild>>> expression) where TChild : class, new()
        {
            DeleteNavTask<Root, TChild> result = new DeleteNavTask<Root, TChild>();
            Func<DeleteNavProvider<Root, TChild>> func = () => PreFunc().ThenInclude(expression);
            result.PreFunc = func;
            result.Context = this.Context;
            return result;
        }

        /// <summary>
        /// 包含下一级导航属性(集合带选项)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <param name="deleteNavOptions">删除导航选项</param>
        /// <returns>删除导航任务</returns>
        public DeleteNavTask<Root, TChild> ThenInclude<TChild>(Expression<Func<T, List<TChild>>> expression, DeleteNavOptions deleteNavOptions) where TChild : class, new()
        {
            DeleteNavTask<Root, TChild> result = new DeleteNavTask<Root, TChild>();
            Func<DeleteNavProvider<Root, TChild>> func = () =>
            {
                var dev = PreFunc();
                dev.deleteNavOptions = deleteNavOptions;
                return dev.ThenInclude(expression);
            };
            result.PreFunc = func;
            result.Context = this.Context;
            return result;
        }

        /// <summary>
        /// 包含导航属性(单个对象)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <returns>删除导航任务</returns>
        public DeleteNavTask<Root, TChild> Include<TChild>(Expression<Func<Root, TChild>> expression) where TChild : class, new()
        {
            return AsNav().ThenInclude(expression);
        }

        /// <summary>
        /// 包含导航属性(带选项)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <param name="options">删除导航选项</param>
        /// <returns>删除导航任务</returns>
        public DeleteNavTask<Root, TChild> Include<TChild>(Expression<Func<Root, TChild>> expression, DeleteNavOptions options) where TChild : class, new()
        {
            return AsNav().ThenInclude(expression, options);
        }

        /// <summary>
        /// 包含导航属性(集合)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <returns>删除导航任务</returns>
        public DeleteNavTask<Root, TChild> Include<TChild>(Expression<Func<Root, List<TChild>>> expression) where TChild : class, new()
        {
            return AsNav().ThenInclude(expression);
        }

        /// <summary>
        /// 包含导航属性(集合带选项)
        /// </summary>
        /// <typeparam name="TChild">子实体类型</typeparam>
        /// <param name="expression">导航属性表达式</param>
        /// <param name="options">删除导航选项</param>
        /// <returns>删除导航任务</returns>
        public DeleteNavTask<Root, TChild> Include<TChild>(Expression<Func<Root, List<TChild>>> expression, DeleteNavOptions options) where TChild : class, new()
        {
            return AsNav().ThenInclude(expression, options);
        }

        /// <summary>
        /// 执行删除命令
        /// </summary>
        /// <returns>是否成功</returns>
        public bool ExecuteCommand()
        {
            PreFunc();

            var hasTran = this.Context.Ado.Transaction != null;
            if (hasTran)
            {
                ExecTasks();
            }
            else
            {
                this.Context.Ado.UseTran(() => ExecTasks(), ex => throw ex);
            }
            return true;
        }

        /// <summary>
        /// 异步执行删除命令
        /// </summary>
        /// <returns>是否成功</returns>
        public async Task<bool> ExecuteCommandAsync()
        {
            await Task.Run(() =>
            {
                ExecuteCommand();
            }).ConfigureAwait(false);
            return true;
        }

        /// <summary>
        /// 转换为导航操作
        /// </summary>
        /// <returns>删除导航任务</returns>
        private DeleteNavTask<Root, Root> AsNav()
        {
            DeleteNavTask<Root, Root> result = new DeleteNavTask<Root, Root>();
            Func<DeleteNavProvider<Root, Root>> func = () => PreFunc().AsNav();
            result.PreFunc = func;
            result.Context = this.Context;
            return result;
        }

        /// <summary>
        /// 执行删除任务
        /// </summary>
        private void ExecTasks()
        {
            var tasks = (List<Action>)this.Context.TempItems["_DeleteNavTask"];
            tasks.Reverse();
            foreach (var task in tasks)
            {
                task();
            }
            this.Context.TempItems.Remove("_DeleteNavTask");
        }
    }
}
