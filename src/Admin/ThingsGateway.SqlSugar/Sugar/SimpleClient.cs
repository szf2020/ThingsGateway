using System.Linq.Expressions;
using System.Reflection;

namespace ThingsGateway.SqlSugar
{
    public partial class SimpleClient<T> : ISugarRepository, ISimpleClient<T> where T : class, new()
    {
        #region Interface
        public virtual ISqlSugarClient Context { get; set; }

        public virtual ITenant AsTenant()
        {
            var result = this.Context as ITenant;
            if (result == null && this.Context is SqlSugarProvider)
            {
                result = (this.Context as SqlSugarProvider).Root as ITenant;
            }
            else if (result == null && this.Context is SqlSugarScopeProvider)
            {
                result = (this.Context as SqlSugarScopeProvider).conn.Root as ITenant;
            }
            return result;
        }
        public virtual ISqlSugarClient AsSugarClient()
        {
            return this.Context;
        }

        public SimpleClient()
        {
        }
        public SimpleClient(ISqlSugarClient context)
        {
            this.Context = context;
        }
        public SimpleClient<ChangeType> Change<ChangeType>() where ChangeType : class, new()
        {
            return this.Context.GetSimpleClient<ChangeType>();
        }
        public SimpleClient<T> CopyNew()
        {
            SimpleClient<T> sm = new SimpleClient<T>();
            sm.Context = this.Context.CopyNew();
            return sm;
        }
        public virtual RepositoryType CopyNew<RepositoryType>() where RepositoryType : ISugarRepository
        {
            Type type = typeof(RepositoryType);
            var isAnyParamter = type.GetConstructors().Any(z => z.GetParameters().Length != 0);
            object o = null;
            if (isAnyParamter)
            {
                object[] pars = type.GetConstructors().First().GetParameters()
                    .Select(it => (object)null).ToArray();
                o = Activator.CreateInstance(type, pars);
            }
            else
            {
                o = Activator.CreateInstance(type);
            }
            var result = (RepositoryType)o;
            if (result.Context != null)
            {
                result.Context = result.Context.CopyNew();
            }
            else
            {
                result.Context = this.Context.CopyNew();
            }
            return result;
        }
        public virtual RepositoryType CopyNew<RepositoryType>(IServiceProvider serviceProvider) where RepositoryType : ISugarRepository
        {
            var instance = handleDependencies(typeof(RepositoryType), serviceProvider, true);
            return (RepositoryType)instance;
        }

        private object handleDependencies(Type type, IServiceProvider serviceProvider, bool needNewCopy = false)
        {
            ConstructorInfo constructorInfo = null;
            var newInstanceType = type;
            if (type.IsInterface && IsAssignableToBaseRepository(type))
            {
                var dependencyInstanceType = serviceProvider.GetService(type)?.GetType();
                newInstanceType = dependencyInstanceType;
                constructorInfo = dependencyInstanceType.GetConstructors().FirstOrDefault();
            }
            else
            {
                constructorInfo = type.GetConstructors().FirstOrDefault();
            }
            var parameters = constructorInfo?.GetParameters();
            if (parameters == null || parameters.Length == 0)
            {
                object dependencyInstance = serviceProvider.GetService(type);
                if (dependencyInstance is ISugarRepository sugarRepository)
                {
                    return setContext(sugarRepository, needNewCopy);
                }
                else
                {
                    return dependencyInstance;
                }
            }

            var conParas = new List<object>();
            foreach (var parameter in parameters)
            {
                Type dependencyType = parameter.ParameterType;
                conParas.Add(handleDependencies(dependencyType, serviceProvider));
            }

            object instance = null;
            if (conParas?.Count > 0)
            {
                instance = Activator.CreateInstance(newInstanceType, conParas.ToArray());
            }
            else
            {
                instance = Activator.CreateInstance(newInstanceType);
            }
            return instance;
        }
        private ISugarRepository setContext(ISugarRepository sugarRepository, bool needNewCopy)
        {
            if (sugarRepository.Context != null)
            {
                if (needNewCopy)
                {
                    sugarRepository.Context = sugarRepository.Context.CopyNew();
                }
            }
            else
            {
                if (needNewCopy)
                {
                    sugarRepository.Context = this.Context.CopyNew();
                }
                else
                {
                    sugarRepository.Context = this.Context;
                }
            }
            return sugarRepository;
        }
        private bool IsAssignableToBaseRepository(Type type)
        {
            var baseInterfaces = type.GetInterfaces();
            foreach (Type interfaceType in baseInterfaces)
            {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(ISimpleClient<>))
                {
                    Type genericArgument = interfaceType.GetGenericArguments()[0];
                    Type baseRepositoryGenericType = typeof(ISimpleClient<>).MakeGenericType(genericArgument);

                    if (baseRepositoryGenericType.IsAssignableFrom(type))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public virtual RepositoryType ChangeRepository<RepositoryType>() where RepositoryType : ISugarRepository
        {
            Type type = typeof(RepositoryType);
            var isAnyParamter = type.GetConstructors().Any(z => z.GetParameters().Length != 0);
            object o = null;
            if (isAnyParamter)
            {
                o = Activator.CreateInstance(type, new string[] { null });
            }
            else
            {
                o = Activator.CreateInstance(type);
            }
            var result = (RepositoryType)o;
            if (result.Context == null)
            {
                result.Context = this.Context;
            }
            return result;
        }
        public virtual RepositoryType ChangeRepository<RepositoryType>(IServiceProvider serviceProvider) where RepositoryType : ISugarRepository
        {
            var instance = handleDependencies(typeof(RepositoryType), serviceProvider, false);
            return (RepositoryType)instance;
        }
        public virtual ISugarQueryable<T> AsQueryable()
        {
            return Context.Queryable<T>();
        }
        public virtual IInsertable<T> AsInsertableT(T insertObj)
        {
            return Context.InsertableT<T>(insertObj);
        }

        public virtual IInsertable<T> AsInsertable(IReadOnlyCollection<T> insertObjs)
        {
            return Context.Insertable<T>(insertObjs);
        }
        public virtual IUpdateable<T> AsUpdateableT(T updateObj)
        {
            return Context.UpdateableT<T>(updateObj);
        }

        public virtual IUpdateable<T> AsUpdateable(IReadOnlyCollection<T> updateObjs)
        {
            return Context.Updateable<T>(updateObjs);
        }
        public virtual IUpdateable<T> AsUpdateable()
        {
            return Context.Updateable<T>();
        }
        public virtual IDeleteable<T> AsDeleteable()
        {
            return Context.Deleteable<T>();
        }
        #endregion

        #region Method
        public virtual T GetById(object id)
        {
            return Context.Queryable<T>().InSingle(id);
        }
        public virtual List<T> GetList()
        {
            return Context.Queryable<T>().ToList();
        }

        public virtual List<T> GetList(Expression<Func<T, bool>> whereExpression)
        {
            return Context.Queryable<T>().Where(whereExpression).ToList();
        }
        public virtual T GetSingle(Expression<Func<T, bool>> whereExpression)
        {
            return Context.Queryable<T>().Single(whereExpression);
        }
        public virtual T GetFirst(Expression<Func<T, bool>> whereExpression)
        {
            return Context.Queryable<T>().First(whereExpression);
        }
        public virtual List<T> GetPageList(Expression<Func<T, bool>> whereExpression, PageModel page)
        {
            int count = 0;
            var result = Context.Queryable<T>().Where(whereExpression).ToPageList(page.PageIndex, page.PageSize, ref count);
            page.TotalCount = count;
            return result;
        }
        public virtual List<T> GetPageList(Expression<Func<T, bool>> whereExpression, PageModel page, Expression<Func<T, object>> orderByExpression = null, OrderByType orderByType = OrderByType.Asc)
        {
            int count = 0;
            var result = Context.Queryable<T>().OrderByIF(orderByExpression != null, orderByExpression, orderByType).Where(whereExpression).ToPageList(page.PageIndex, page.PageSize, ref count);
            page.TotalCount = count;
            return result;
        }
        public virtual List<T> GetPageList(List<IConditionalModel> conditionalList, PageModel page)
        {
            int count = 0;
            var result = Context.Queryable<T>().Where(conditionalList).ToPageList(page.PageIndex, page.PageSize, ref count);
            page.TotalCount = count;
            return result;
        }
        public virtual List<T> GetPageList(List<IConditionalModel> conditionalList, PageModel page, Expression<Func<T, object>> orderByExpression = null, OrderByType orderByType = OrderByType.Asc)
        {
            int count = 0;
            var result = Context.Queryable<T>().OrderByIF(orderByExpression != null, orderByExpression, orderByType).Where(conditionalList).ToPageList(page.PageIndex, page.PageSize, ref count);
            page.TotalCount = count;
            return result;
        }
        public virtual bool IsAny(Expression<Func<T, bool>> whereExpression)
        {
            return Context.Queryable<T>().Where(whereExpression).Any();
        }
        public virtual int Count(Expression<Func<T, bool>> whereExpression)
        {
            return Context.Queryable<T>().Where(whereExpression).Count();
        }

        public virtual bool Insert(T insertObj)
        {
            return this.Context.InsertableT(insertObj).ExecuteCommand() > 0;
        }

        public virtual bool InsertOrUpdateT(T data)
        {
            return this.Context.StorageableT(data).ExecuteCommand() > 0;
        }
        public virtual bool InsertOrUpdate(IReadOnlyCollection<T> datas)
        {
            return this.Context.Storageable(datas).ExecuteCommand() > 0;
        }

        public virtual int InsertReturnIdentity(T insertObj)
        {
            return this.Context.InsertableT(insertObj).ExecuteReturnIdentity();
        }
        public virtual long InsertReturnBigIdentity(T insertObj)
        {
            return this.Context.InsertableT(insertObj).ExecuteReturnBigIdentity();
        }
        public virtual long InsertReturnSnowflakeIdT(T insertObj)
        {
            return this.Context.InsertableT(insertObj).ExecuteReturnSnowflakeId();
        }
        public virtual List<long> InsertReturnSnowflakeId(IReadOnlyCollection<T> insertObjs)
        {
            return this.Context.Insertable(insertObjs).ExecuteReturnSnowflakeIdList();
        }
        public virtual Task<long> InsertReturnSnowflakeIdTAsync(T insertObj)
        {
            return this.Context.InsertableT(insertObj).ExecuteReturnSnowflakeIdAsync();
        }
        public virtual Task<List<long>> InsertReturnSnowflakeIdAsync(IReadOnlyCollection<T> insertObjs)
        {
            return this.Context.Insertable(insertObjs).ExecuteReturnSnowflakeIdListAsync();
        }

        public virtual T InsertReturnEntity(T insertObj)
        {
            return this.Context.InsertableT(insertObj).ExecuteReturnEntity();
        }

        public virtual bool InsertRange(IReadOnlyCollection<T> insertObjs)
        {
            return this.Context.Insertable(insertObjs).ExecuteCommand() > 0;
        }
        public virtual bool Update(T updateObj)
        {
            return this.Context.UpdateableT(updateObj).ExecuteCommand() > 0;
        }

        public virtual bool UpdateRange(IReadOnlyCollection<T> updateObjs)
        {
            return this.Context.Updateable(updateObjs).ExecuteCommand() > 0;
        }
        public virtual bool Update(Expression<Func<T, T>> columns, Expression<Func<T, bool>> whereExpression)
        {
            return this.Context.Updateable<T>().SetColumns(columns).Where(whereExpression).ExecuteCommand() > 0;
        }
        public virtual bool UpdateSetColumnsTrue(Expression<Func<T, T>> columns, Expression<Func<T, bool>> whereExpression)
        {
            return this.Context.Updateable<T>().SetColumns(columns, true).Where(whereExpression).ExecuteCommand() > 0;
        }
        public virtual bool Delete(T deleteObj)
        {
            return this.Context.Deleteable<T>().WhereT(deleteObj).ExecuteCommand() > 0;
        }
        public virtual bool Delete(List<T> deleteObjs)
        {
            return this.Context.Deleteable<T>().Where(deleteObjs).ExecuteCommand() > 0;
        }
        public virtual bool Delete(Expression<Func<T, bool>> whereExpression)
        {
            return this.Context.Deleteable<T>().Where(whereExpression).ExecuteCommand() > 0;
        }
        public virtual bool DeleteById<PKType>(PKType id)
        {
            return this.Context.Deleteable<T>().InT(id).ExecuteCommand() > 0;
        }

        public virtual bool DeleteByIds<PKType>(IReadOnlyCollection<PKType> ids)
        {
            return this.Context.Deleteable<T>().In(ids).ExecuteCommand() > 0;
        }
        public virtual bool DeleteById(object id)
        {
            return this.Context.Deleteable<T>().InT(id).ExecuteCommand() > 0;
        }

        public virtual bool DeleteByIds(IReadOnlyCollection<object> ids)
        {
            return this.Context.Deleteable<T>().In(ids).ExecuteCommand() > 0;
        }

        #endregion

        #region Async Method
        public virtual Task<T> GetByIdAsync(object id)
        {
            return Context.Queryable<T>().InSingleAsync(id);
        }
        public virtual Task<List<T>> GetListAsync()
        {
            return Context.Queryable<T>().ToListAsync();
        }

        public virtual Task<List<T>> GetListAsync(Expression<Func<T, bool>> whereExpression)
        {
            return Context.Queryable<T>().Where(whereExpression).ToListAsync();
        }
        public virtual Task<T> GetSingleAsync(Expression<Func<T, bool>> whereExpression)
        {
            return Context.Queryable<T>().SingleAsync(whereExpression);
        }
        public virtual Task<T> GetFirstAsync(Expression<Func<T, bool>> whereExpression)
        {
            return Context.Queryable<T>().FirstAsync(whereExpression);
        }
        public virtual async Task<List<T>> GetPageListAsync(Expression<Func<T, bool>> whereExpression, PageModel page)
        {
            RefAsync<int> count = 0;
            var result = await Context.Queryable<T>().Where(whereExpression).ToPageListAsync(page.PageIndex, page.PageSize, count).ConfigureAwait(false);
            page.TotalCount = count;
            return result;
        }
        public virtual async Task<List<T>> GetPageListAsync(Expression<Func<T, bool>> whereExpression, PageModel page, Expression<Func<T, object>> orderByExpression = null, OrderByType orderByType = OrderByType.Asc)
        {
            RefAsync<int> count = 0;
            var result = await Context.Queryable<T>().OrderByIF(orderByExpression != null, orderByExpression, orderByType).Where(whereExpression).ToPageListAsync(page.PageIndex, page.PageSize, count).ConfigureAwait(false);
            page.TotalCount = count;
            return result;
        }
        public virtual async Task<List<T>> GetPageListAsync(List<IConditionalModel> conditionalList, PageModel page)
        {
            RefAsync<int> count = 0;
            var result = await Context.Queryable<T>().Where(conditionalList).ToPageListAsync(page.PageIndex, page.PageSize, count).ConfigureAwait(false);
            page.TotalCount = count;
            return result;
        }
        public virtual async Task<List<T>> GetPageListAsync(List<IConditionalModel> conditionalList, PageModel page, Expression<Func<T, object>> orderByExpression = null, OrderByType orderByType = OrderByType.Asc)
        {
            RefAsync<int> count = 0;
            var result = await Context.Queryable<T>().OrderByIF(orderByExpression != null, orderByExpression, orderByType).Where(conditionalList).ToPageListAsync(page.PageIndex, page.PageSize, count).ConfigureAwait(false);
            page.TotalCount = count;
            return result;
        }
        public virtual Task<bool> IsAnyAsync(Expression<Func<T, bool>> whereExpression)
        {
            return Context.Queryable<T>().Where(whereExpression).AnyAsync();
        }
        public virtual Task<int> CountAsync(Expression<Func<T, bool>> whereExpression)
        {
            return Context.Queryable<T>().Where(whereExpression).CountAsync();
        }

        public virtual async Task<bool> InsertOrUpdateTAsync(T data)
        {
            return await Context.StorageableT(data).ExecuteCommandAsync().ConfigureAwait(false) > 0;
        }
        public virtual async Task<bool> InsertOrUpdateAsync(IReadOnlyCollection<T> datas)
        {
            return await Context.Storageable(datas).ExecuteCommandAsync().ConfigureAwait(false) > 0;
        }
        public virtual async Task<bool> InsertAsync(T insertObj)
        {
            return await Context.InsertableT(insertObj).ExecuteCommandAsync().ConfigureAwait(false) > 0;
        }
        public virtual Task<int> InsertReturnIdentityAsync(T insertObj)
        {
            return this.Context.InsertableT(insertObj).ExecuteReturnIdentityAsync();
        }
        public virtual Task<long> InsertReturnBigIdentityAsync(T insertObj)
        {
            return this.Context.InsertableT(insertObj).ExecuteReturnBigIdentityAsync();
        }
        public virtual async Task<T> InsertReturnEntityAsync(T insertObj)
        {
            return await Context.InsertableT(insertObj).ExecuteReturnEntityAsync().ConfigureAwait(false);
        }

        public virtual async Task<bool> InsertRangeAsync(IReadOnlyCollection<T> insertObjs)
        {
            return await Context.Insertable(insertObjs).ExecuteCommandAsync().ConfigureAwait(false) > 0;
        }
        public virtual async Task<bool> UpdateAsync(T updateObj)
        {
            return await Context.UpdateableT(updateObj).ExecuteCommandAsync().ConfigureAwait(false) > 0;
        }

        public virtual async Task<bool> UpdateRangeAsync(IReadOnlyCollection<T> updateObjs)
        {
            return await Context.Updateable(updateObjs).ExecuteCommandAsync().ConfigureAwait(false) > 0;
        }
        public virtual async Task<bool> UpdateAsync(Expression<Func<T, T>> columns, Expression<Func<T, bool>> whereExpression)
        {
            return await Context.Updateable<T>().SetColumns(columns).Where(whereExpression).ExecuteCommandAsync().ConfigureAwait(false) > 0;
        }
        public virtual async Task<bool> UpdateSetColumnsTrueAsync(Expression<Func<T, T>> columns, Expression<Func<T, bool>> whereExpression)
        {
            return await Context.Updateable<T>().SetColumns(columns, true).Where(whereExpression).ExecuteCommandAsync().ConfigureAwait(false) > 0;
        }
        public virtual async Task<bool> DeleteAsync(T deleteObj)
        {
            return await Context.Deleteable<T>().WhereT(deleteObj).ExecuteCommandAsync().ConfigureAwait(false) > 0;
        }
        public virtual async Task<bool> DeleteAsync(List<T> deleteObjs)
        {
            return await Context.Deleteable<T>().Where(deleteObjs).ExecuteCommandAsync().ConfigureAwait(false) > 0;
        }
        public virtual async Task<bool> DeleteAsync(Expression<Func<T, bool>> whereExpression)
        {
            return await Context.Deleteable<T>().Where(whereExpression).ExecuteCommandAsync().ConfigureAwait(false) > 0;
        }
        public virtual async Task<bool> DeleteByIdAsync(object id)
        {
            return await this.Context.Deleteable<T>().InT(id).ExecuteCommandAsync().ConfigureAwait(false) > 0;
        }
        public virtual async Task<bool> DeleteByIdsAsync(IReadOnlyCollection<object> ids)
        {
            return await Context.Deleteable<T>().In(ids).ExecuteCommandAsync().ConfigureAwait(false) > 0;
        }
        #endregion

        #region Async Method  CancellationToken
        public virtual Task<long> InsertReturnSnowflakeIdAsync(T insertObj, CancellationToken cancellationToken)
        {
            this.Context.Ado.CancellationToken = cancellationToken;
            return this.Context.InsertableT(insertObj).ExecuteReturnSnowflakeIdAsync(cancellationToken);
        }
        public virtual Task<List<long>> InsertReturnSnowflakeIdAsync(IReadOnlyCollection<T> insertObjs, CancellationToken cancellationToken)
        {
            this.Context.Ado.CancellationToken = cancellationToken;
            return this.Context.Insertable(insertObjs).ExecuteReturnSnowflakeIdListAsync(cancellationToken);
        }

        public virtual Task<T> GetByIdAsync(object id, CancellationToken cancellationToken)
        {
            this.Context.Ado.CancellationToken = cancellationToken;
            return Context.Queryable<T>().InSingleAsync(id);
        }
        public virtual Task<List<T>> GetListAsync(CancellationToken cancellationToken)
        {
            this.Context.Ado.CancellationToken = cancellationToken;
            return Context.Queryable<T>().ToListAsync(cancellationToken);
        }

        public virtual Task<List<T>> GetListAsync(Expression<Func<T, bool>> whereExpression, CancellationToken cancellationToken)
        {
            this.Context.Ado.CancellationToken = cancellationToken;
            return Context.Queryable<T>().Where(whereExpression).ToListAsync(cancellationToken);
        }
        public virtual Task<T> GetSingleAsync(Expression<Func<T, bool>> whereExpression, CancellationToken cancellationToken)
        {
            this.Context.Ado.CancellationToken = cancellationToken;
            return Context.Queryable<T>().SingleAsync(whereExpression);
        }
        public virtual Task<T> GetFirstAsync(Expression<Func<T, bool>> whereExpression, CancellationToken cancellationToken)
        {
            this.Context.Ado.CancellationToken = cancellationToken;
            return Context.Queryable<T>().FirstAsync(whereExpression, cancellationToken);
        }
        public virtual async Task<List<T>> GetPageListAsync(Expression<Func<T, bool>> whereExpression, PageModel page, CancellationToken cancellationToken)
        {
            this.Context.Ado.CancellationToken = cancellationToken;
            RefAsync<int> count = 0;
            var result = await Context.Queryable<T>().Where(whereExpression).ToPageListAsync(page.PageIndex, page.PageSize, count, cancellationToken).ConfigureAwait(false);
            page.TotalCount = count;
            return result;
        }
        public virtual async Task<List<T>> GetPageListAsync(Expression<Func<T, bool>> whereExpression, PageModel page, Expression<Func<T, object>> orderByExpression = null, OrderByType orderByType = OrderByType.Asc, CancellationToken cancellationToken = default)
        {
            this.Context.Ado.CancellationToken = cancellationToken;
            RefAsync<int> count = 0;
            var result = await Context.Queryable<T>().OrderByIF(orderByExpression != null, orderByExpression, orderByType).Where(whereExpression).ToPageListAsync(page.PageIndex, page.PageSize, count, cancellationToken).ConfigureAwait(false);
            page.TotalCount = count;
            return result;
        }
        public virtual async Task<List<T>> GetPageListAsync(List<IConditionalModel> conditionalList, PageModel page, CancellationToken cancellationToken)
        {
            this.Context.Ado.CancellationToken = cancellationToken;
            RefAsync<int> count = 0;
            var result = await Context.Queryable<T>().Where(conditionalList).ToPageListAsync(page.PageIndex, page.PageSize, count, cancellationToken).ConfigureAwait(false);
            page.TotalCount = count;
            return result;
        }
        public virtual async Task<List<T>> GetPageListAsync(List<IConditionalModel> conditionalList, PageModel page, Expression<Func<T, object>> orderByExpression = null, OrderByType orderByType = OrderByType.Asc, CancellationToken cancellationToken = default)
        {
            this.Context.Ado.CancellationToken = cancellationToken;
            RefAsync<int> count = 0;
            var result = await Context.Queryable<T>().OrderByIF(orderByExpression != null, orderByExpression, orderByType).Where(conditionalList).ToPageListAsync(page.PageIndex, page.PageSize, count, cancellationToken).ConfigureAwait(false);
            page.TotalCount = count;
            return result;
        }
        public virtual Task<bool> IsAnyAsync(Expression<Func<T, bool>> whereExpression, CancellationToken cancellationToken)
        {
            return Context.Queryable<T>().Where(whereExpression).AnyAsync(cancellationToken);
        }
        public virtual Task<int> CountAsync(Expression<Func<T, bool>> whereExpression, CancellationToken cancellationToken)
        {
            this.Context.Ado.CancellationToken = cancellationToken;
            return Context.Queryable<T>().Where(whereExpression).CountAsync(cancellationToken);
        }

        public virtual async Task<bool> InsertOrUpdateAsync(T data, CancellationToken cancellationToken)
        {
            this.Context.Ado.CancellationToken = cancellationToken;
            return await Context.StorageableT(data).ExecuteCommandAsync(cancellationToken).ConfigureAwait(false) > 0;
        }
        public virtual async Task<bool> InsertOrUpdateAsync(List<T> datas, CancellationToken cancellationToken)
        {
            this.Context.Ado.CancellationToken = cancellationToken;
            return await Context.Storageable(datas).ExecuteCommandAsync(cancellationToken).ConfigureAwait(false) > 0;
        }
        public virtual async Task<bool> InsertAsync(T insertObj, CancellationToken cancellationToken)
        {
            this.Context.Ado.CancellationToken = cancellationToken;
            return await Context.InsertableT(insertObj).ExecuteCommandAsync(cancellationToken).ConfigureAwait(false) > 0;
        }
        public virtual Task<int> InsertReturnIdentityAsync(T insertObj, CancellationToken cancellationToken)
        {
            this.Context.Ado.CancellationToken = cancellationToken;
            return this.Context.InsertableT(insertObj).ExecuteReturnIdentityAsync(cancellationToken);
        }
        public virtual Task<long> InsertReturnBigIdentityAsync(T insertObj, CancellationToken cancellationToken)
        {
            this.Context.Ado.CancellationToken = cancellationToken;
            return this.Context.InsertableT(insertObj).ExecuteReturnBigIdentityAsync(cancellationToken);
        }
        public virtual async Task<T> InsertReturnEntityAsync(T insertObj, CancellationToken cancellationToken)
        {
            this.Context.Ado.CancellationToken = cancellationToken;
            return await Context.InsertableT(insertObj).ExecuteReturnEntityAsync().ConfigureAwait(false);
        }

        public virtual async Task<bool> InsertRangeAsync(IReadOnlyCollection<T> insertObjs, CancellationToken cancellationToken)
        {
            this.Context.Ado.CancellationToken = cancellationToken;
            return await Context.Insertable(insertObjs).ExecuteCommandAsync(cancellationToken).ConfigureAwait(false) > 0;
        }
        public virtual async Task<bool> UpdateAsync(T updateObj, CancellationToken cancellationToken)
        {
            this.Context.Ado.CancellationToken = cancellationToken;
            return await Context.UpdateableT(updateObj).ExecuteCommandAsync(cancellationToken).ConfigureAwait(false) > 0;
        }

        public virtual async Task<bool> UpdateRangeAsync(IReadOnlyCollection<T> updateObjs, CancellationToken cancellationToken)
        {
            this.Context.Ado.CancellationToken = cancellationToken;
            return await Context.Updateable(updateObjs).ExecuteCommandAsync(cancellationToken).ConfigureAwait(false) > 0;
        }
        public virtual async Task<bool> UpdateAsync(Expression<Func<T, T>> columns, Expression<Func<T, bool>> whereExpression, CancellationToken cancellationToken)
        {
            this.Context.Ado.CancellationToken = cancellationToken;
            return await Context.Updateable<T>().SetColumns(columns).Where(whereExpression).ExecuteCommandAsync(cancellationToken).ConfigureAwait(false) > 0;
        }
        public virtual async Task<bool> UpdateSetColumnsTrueAsync(Expression<Func<T, T>> columns, Expression<Func<T, bool>> whereExpression, CancellationToken cancellationToken)
        {
            this.Context.Ado.CancellationToken = cancellationToken;
            return await Context.Updateable<T>().SetColumns(columns, true).Where(whereExpression).ExecuteCommandAsync(cancellationToken).ConfigureAwait(false) > 0;
        }
        public virtual async Task<bool> DeleteAsync(T deleteObj, CancellationToken cancellationToken)
        {
            this.Context.Ado.CancellationToken = cancellationToken;
            return await Context.Deleteable<T>().WhereT(deleteObj).ExecuteCommandAsync(cancellationToken).ConfigureAwait(false) > 0;
        }
        public virtual async Task<bool> DeleteAsync(List<T> deleteObjs, CancellationToken cancellationToken)
        {
            this.Context.Ado.CancellationToken = cancellationToken;
            return await Context.Deleteable<T>().Where(deleteObjs).ExecuteCommandAsync(cancellationToken).ConfigureAwait(false) > 0;
        }
        public virtual async Task<bool> DeleteAsync(Expression<Func<T, bool>> whereExpression, CancellationToken cancellationToken)
        {
            this.Context.Ado.CancellationToken = cancellationToken;
            return await Context.Deleteable<T>().Where(whereExpression).ExecuteCommandAsync(cancellationToken).ConfigureAwait(false) > 0;
        }
        public virtual async Task<bool> DeleteByIdAsync(object id, CancellationToken cancellationToken)
        {
            this.Context.Ado.CancellationToken = cancellationToken;
            return await this.Context.Deleteable<T>().InT(id).ExecuteCommandAsync(cancellationToken).ConfigureAwait(false) > 0;
        }
        public virtual async Task<bool> DeleteByIdsAsync(IReadOnlyCollection<object> ids, CancellationToken cancellationToken)
        {
            this.Context.Ado.CancellationToken = cancellationToken;
            return await Context.Deleteable<T>().In(ids).ExecuteCommandAsync(cancellationToken).ConfigureAwait(false) > 0;
        }

        public int Count(List<IConditionalModel> conditionalModels)
        {
            return Context.Queryable<T>().Where(conditionalModels).Count();
        }

        public bool Delete(List<IConditionalModel> conditionalModels)
        {
            return Context.Deleteable<T>().Where(conditionalModels).ExecuteCommandHasChange();
        }

        public List<T> GetList(Expression<Func<T, bool>> whereExpression, List<OrderByModel> orderByModels)
        {
            return Context.Queryable<T>().Where(whereExpression).OrderBy(orderByModels).ToList();
        }

        public List<T> GetList(List<IConditionalModel> conditionalList)
        {
            return Context.Queryable<T>().Where(conditionalList).ToList();
        }

        public List<T> GetList(List<IConditionalModel> conditionalList, List<OrderByModel> orderByModels)
        {
            return Context.Queryable<T>().Where(conditionalList).OrderBy(orderByModels).ToList();
        }

        public List<T> GetPageList(Expression<Func<T, bool>> whereExpression, PageModel page, List<OrderByModel> orderByModels)
        {
            var total = 0;
            var list = Context.Queryable<T>().Where(whereExpression).OrderBy(orderByModels).ToPageList(page.PageIndex, page.PageSize, ref total);
            page.TotalCount = total;
            return list;
        }

        public List<T> GetPageList(List<IConditionalModel> conditionalList, PageModel page, List<OrderByModel> orderByModels)
        {
            var total = 0;
            var list = Context.Queryable<T>().Where(conditionalList).OrderBy(orderByModels).ToPageList(page.PageIndex, page.PageSize, ref total);
            page.TotalCount = total;
            return list;
        }

        public T GetSingle(List<IConditionalModel> conditionalModels)
        {
            return Context.Queryable<T>().Where(conditionalModels).Single();
        }

        public T GetFirst(List<IConditionalModel> conditionalModels)
        {
            return Context.Queryable<T>().Where(conditionalModels).First();
        }

        public bool IsAny(List<IConditionalModel> conditionalModels)
        {
            return Context.Queryable<T>().Where(conditionalModels).Any();
        }

        public T GetFirst(Expression<Func<T, bool>> whereExpression, List<OrderByModel> orderByModels)
        {
            return Context.Queryable<T>().Where(whereExpression).OrderBy(orderByModels).First();
        }

        public T GetFirst(List<IConditionalModel> conditionalModels, List<OrderByModel> orderByModels)
        {
            return Context.Queryable<T>().Where(conditionalModels).OrderBy(orderByModels).First();
        }



        #endregion

    }
}
