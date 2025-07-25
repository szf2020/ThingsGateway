using System.Data;
using System.Linq.Expressions;

namespace ThingsGateway.SqlSugar
{
    public partial class QueryableProvider<T> : QueryableAccessory, ISugarQueryable<T>
    {
        public virtual async Task<T[]> ToArrayAsync()
        {
            var result = await ToListAsync().ConfigureAwait(false);
            if (result.HasValue())
                return result.ToArray();
            else
                return null;
        }

        public virtual async Task<T> InSingleAsync(object pkValue)
        {
            if (pkValue == null)
            {
                return default(T);
            }
            Check.Exception(this.QueryBuilder.SelectValue.HasValue(), "'InSingle' and' Select' can't be used together,You can use .Select(it=>...).Single(it.id==1)");
            var list = await In([pkValue]).ToListAsync().ConfigureAwait(false);
            if (list == null) return default(T);
            else return list.SingleOrDefault();
        }
        public virtual async Task<T> SingleAsync()
        {
            if (QueryBuilder.OrderByValue.IsNullOrEmpty())
            {
                QueryBuilder.OrderByValue = QueryBuilder.DefaultOrderByTemplate;
            }
            var oldSkip = QueryBuilder.Skip;
            var oldTake = QueryBuilder.Take;
            var oldOrderBy = QueryBuilder.OrderByValue;
            QueryBuilder.Skip = null;
            QueryBuilder.Take = null;
            QueryBuilder.OrderByValue = null;
            var result = await ToListAsync().ConfigureAwait(false);
            QueryBuilder.Skip = oldSkip;
            QueryBuilder.Take = oldTake;
            QueryBuilder.OrderByValue = oldOrderBy;
            if (result == null || result.Count == 0)
            {
                return default(T);
            }
            else if (result.Count == 2)
            {
                Check.Exception(true, ErrorMessage.GetThrowMessage(".Single()  result must not exceed one . You can use.First()", "使用single查询结果集不能大于1，适合主键查询，如果大于1你可以使用Queryable.First"));
                return default(T);
            }
            else
            {
                return result.SingleOrDefault();
            }
        }
        public virtual async Task<T> SingleAsync(Expression<Func<T, bool>> expression)
        {
            _Where(expression);
            var result = await SingleAsync().ConfigureAwait(false);
            this.QueryBuilder.WhereInfos.Remove(this.QueryBuilder.WhereInfos.Last());
            return result;
        }
        public Task<T> FirstAsync(CancellationToken token)
        {
            this.Context.Ado.CancellationToken = token;
            return FirstAsync();
        }
        public virtual async Task<T> FirstAsync()
        {
            if (QueryBuilder.OrderByValue.IsNullOrEmpty())
            {
                QueryBuilder.OrderByValue = QueryBuilder.DefaultOrderByTemplate;
            }
            if (QueryBuilder.Skip.HasValue)
            {
                QueryBuilder.Take = 1;
                var list = await ToListAsync().ConfigureAwait(false);
                return list.FirstOrDefault();
            }
            else
            {
                QueryBuilder.Skip = 0;
                QueryBuilder.Take = 1;
                var result = await ToListAsync().ConfigureAwait(false);
                if (result.HasValue())
                    return result.FirstOrDefault();
                else
                    return default(T);
            }
        }
        public Task<T> FirstAsync(Expression<Func<T, bool>> expression, CancellationToken token)
        {
            this.Context.Ado.CancellationToken = token;
            return FirstAsync(expression);
        }
        public virtual async Task<T> FirstAsync(Expression<Func<T, bool>> expression)
        {
            _Where(expression);
            var result = await FirstAsync().ConfigureAwait(false);
            this.QueryBuilder.WhereInfos.Remove(this.QueryBuilder.WhereInfos.Last());
            return result;
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> expression)
        {
            _Where(expression);
            var result = await AnyAsync().ConfigureAwait(false);
            this.QueryBuilder.WhereInfos.Remove(this.QueryBuilder.WhereInfos.Last());
            return result;
        }

        public Task<bool> AnyAsync(Expression<Func<T, bool>> expression, CancellationToken token)
        {
            this.Context.Ado.CancellationToken = token;
            return AnyAsync(expression);
        }

        public virtual async Task<bool> AnyAsync()
        {
            return (await Clone().Take(1).Select("1").ToListAsync().ConfigureAwait(false)).Count > 0;
        }
        public virtual Task<bool> AnyAsync(CancellationToken token)
        {
            this.Context.Ado.CancellationToken = token;
            return this.AnyAsync();
        }
        public Task<int> CountAsync(CancellationToken token)
        {
            this.Context.Ado.CancellationToken = token;
            return CountAsync();
        }
        public virtual async Task<int> CountAsync()
        {
            if (this.QueryBuilder.Skip == null &&
             this.QueryBuilder.Take == null &&
             this.QueryBuilder.OrderByValue == null &&
             this.QueryBuilder.PartitionByValue == null &&
             this.QueryBuilder.SelectValue == null &&
             this.QueryBuilder.Includes == null &&
             this.QueryBuilder.IsDistinct == false)
            {
                var list = await Clone().Select<int>(" COUNT(1) ").ToListAsync().ConfigureAwait(false);
                return list.FirstOrDefault();
            }
            MappingTableList expMapping;
            int result;
            _CountBegin(out expMapping, out result);
            if (IsCache)
            {
                var cacheService = this.Context.CurrentConnectionConfig.ConfigureExternalServices.DataInfoCacheService;
                result = CacheSchemeMain.GetOrCreate<int>(cacheService, this.QueryBuilder, () => GetCount(), CacheTime, this.Context, CacheKey);
            }
            else
            {
                result = await GetCountAsync().ConfigureAwait(false);
            }
            _CountEnd(expMapping);
            return result;
        }
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> expression)
        {
            _Where(expression);
            var result = await CountAsync().ConfigureAwait(false);
            this.QueryBuilder.WhereInfos.Remove(this.QueryBuilder.WhereInfos.Last());
            return result;
        }

        public Task<int> CountAsync(Expression<Func<T, bool>> expression, CancellationToken token)
        {
            this.Context.Ado.CancellationToken = token;
            return CountAsync(expression);
        }

        public virtual async Task<TResult> MaxAsync<TResult>(string maxField)
        {
            this.Select(string.Format(QueryBuilder.MaxTemplate, maxField));
            var list = await _ToListAsync<TResult>().ConfigureAwait(false);
            var result = list.SingleOrDefault();
            return result;
        }

        public Task<TResult> MaxAsync<TResult>(string maxField, CancellationToken token)
        {
            this.Context.Ado.CancellationToken = token;
            return MaxAsync<TResult>(maxField);
        }

        public Task<TResult> MaxAsync<TResult>(Expression<Func<T, TResult>> expression)
        {
            return _MaxAsync<TResult>(expression);
        }

        public Task<TResult> MaxAsync<TResult>(Expression<Func<T, TResult>> expression, CancellationToken token)
        {
            this.Context.Ado.CancellationToken = token;
            return MaxAsync(expression);
        }

        public virtual async Task<TResult> MinAsync<TResult>(string minField)
        {
            this.Select(string.Format(QueryBuilder.MinTemplate, minField));
            var list = await _ToListAsync<TResult>().ConfigureAwait(false);
            var result = list.SingleOrDefault();
            return result;
        }
        public Task<TResult> MinAsync<TResult>(Expression<Func<T, TResult>> expression)
        {
            return _MinAsync<TResult>(expression);
        }

        public virtual async Task<TResult> SumAsync<TResult>(string sumField)
        {
            this.Select(string.Format(QueryBuilder.SumTemplate, sumField));
            var list = await _ToListAsync<TResult>().ConfigureAwait(false);
            var result = list.SingleOrDefault();
            return result;
        }
        public Task<TResult> SumAsync<TResult>(Expression<Func<T, TResult>> expression)
        {
            return _SumAsync<TResult>(expression);
        }

        public virtual async Task<TResult> AvgAsync<TResult>(string avgField)
        {
            this.Select(string.Format(QueryBuilder.AvgTemplate, avgField));
            var list = await _ToListAsync<TResult>().ConfigureAwait(false);
            var result = list.SingleOrDefault();
            return result;
        }
        public Task<TResult> AvgAsync<TResult>(Expression<Func<T, TResult>> expression)
        {
            return _AvgAsync<TResult>(expression);
        }

        public virtual async Task<List<TResult>> ToListAsync<TResult>(Expression<Func<T, TResult>> expression)
        {
            if (this.QueryBuilder.Includes?.Count > 0)
            {
                return await NavSelectHelper.GetListAsync(expression, this).ConfigureAwait(false);
            }
            else
            {
                var list = await Select(expression).ToListAsync().ConfigureAwait(false);
                return list;
            }
        }
        public Task<List<T>> ToListAsync()
        {
            InitMapping();
            return _ToListAsync<T>();
        }

        public Task<List<T>> ToListAsync(CancellationToken token)
        {
            this.Context.Ado.CancellationToken = token;
            return ToListAsync();
        }
        public Task<List<T>> ToPageListAsync(int pageNumber, int pageSize, CancellationToken token)
        {
            this.Context.Ado.CancellationToken = token;
            return ToPageListAsync(pageNumber, pageSize);
        }
        public Task<List<T>> ToPageListAsync(int pageIndex, int pageSize)
        {
            pageIndex = _PageList(pageIndex, pageSize);
            return ToListAsync();
        }
        public virtual async Task<List<TResult>> ToPageListAsync<TResult>(int pageIndex, int pageSize, RefAsync<int> totalNumber, Expression<Func<T, TResult>> expression)
        {
            if (this.QueryBuilder.Includes?.Count > 0)
            {
                if (pageIndex == 0)
                    pageIndex = 1;
                var list = await Clone().Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(expression).ConfigureAwait(false);
                var countQueryable = this.Clone();
                countQueryable.QueryBuilder.Includes = null;
                totalNumber.Value = await countQueryable.CountAsync().ConfigureAwait(false);
                return list;
            }
            else
            {
                var list = await Select(expression).ToPageListAsync(pageIndex, pageSize, totalNumber).ConfigureAwait(false);
                return list;
            }
        }
        public Task<List<T>> ToPageListAsync(int pageNumber, int pageSize, RefAsync<int> totalNumber, CancellationToken token)
        {
            this.Context.Ado.CancellationToken = token;
            return ToPageListAsync(pageNumber, pageSize, totalNumber);
        }
        public virtual async Task<List<T>> ToPageListAsync(int pageIndex, int pageSize, RefAsync<int> totalNumber)
        {
            var oldMapping = this.Context.MappingTables;
            var countQueryable = this.Clone();
            if (countQueryable.QueryBuilder.Offset == "true")
            {
                countQueryable.QueryBuilder.Offset = null;
            }
            totalNumber.Value = await countQueryable.CountAsync().ConfigureAwait(false);
            this.Context.MappingTables = oldMapping;
            return await Clone().ToPageListAsync(pageIndex, pageSize).ConfigureAwait(false);
        }
        public virtual async Task<List<T>> ToPageListAsync(int pageNumber, int pageSize, RefAsync<int> totalNumber, RefAsync<int> totalPage)
        {
            var result = await ToPageListAsync(pageNumber, pageSize, totalNumber).ConfigureAwait(false);
            totalPage.Value = (totalNumber.Value + pageSize - 1) / pageSize;
            return result;
        }

        public Task<List<T>> ToPageListAsync(int pageNumber, int pageSize, RefAsync<int> totalNumber, RefAsync<int> totalPage, CancellationToken token)
        {
            this.Context.Ado.CancellationToken = token;
            return ToPageListAsync(pageNumber, pageSize, totalNumber, totalPage);
        }

        public virtual async Task<string> ToJsonAsync()
        {
            if (IsCache)
            {
                var cacheService = this.Context.CurrentConnectionConfig.ConfigureExternalServices.DataInfoCacheService;
                var result = CacheSchemeMain.GetOrCreate<string>(cacheService, this.QueryBuilder, () => this.Context.Utilities.SerializeObject(this.ToList(), typeof(T)), CacheTime, this.Context, CacheKey);
                return result;
            }
            else
            {
                return this.Context.Utilities.SerializeObject(await ToListAsync().ConfigureAwait(false), typeof(T));
            }
        }
        public virtual async Task<string> ToJsonPageAsync(int pageIndex, int pageSize)
        {
            return this.Context.Utilities.SerializeObject(await ToPageListAsync(pageIndex, pageSize).ConfigureAwait(false), typeof(T));
        }
        public virtual async Task<string> ToJsonPageAsync(int pageIndex, int pageSize, RefAsync<int> totalNumber)
        {
            var oldMapping = this.Context.MappingTables;
            totalNumber.Value = await Clone().CountAsync().ConfigureAwait(false);
            this.Context.MappingTables = oldMapping;
            return await Clone().ToJsonPageAsync(pageIndex, pageSize).ConfigureAwait(false);
        }
        public virtual async Task<DataTable> ToDataTableByEntityAsync()
        {
            var list = await ToListAsync().ConfigureAwait(false);
            return this.Context.Utilities.ListToDataTable(list);
        }

        public Task<DataTable> ToOffsetDataTablePageAsync(int pageNumber, int pageSize)
        {
            if (this.Context.CurrentConnectionConfig.DbType != DbType.SqlServer)
            {
                this.QueryBuilder.Offset = "true";
                return this.ToDataTablePageAsync(pageNumber, pageSize);
            }
            else
            {
                _ToOffsetPage(pageNumber, pageSize);
                return this.ToDataTableAsync();
            }
        }
        public virtual async Task<DataTable> ToOffsetDataTablePageAsync(int pageNumber, int pageSize, RefAsync<int> totalNumber)
        {
            if (this.Context.CurrentConnectionConfig.DbType != DbType.SqlServer)
            {
                this.QueryBuilder.Offset = "true";
                return await ToDataTablePageAsync(pageNumber, pageSize, totalNumber).ConfigureAwait(false);
            }
            else
            {
                totalNumber.Value = await Clone().CountAsync().ConfigureAwait(false);
                _ToOffsetPage(pageNumber, pageSize);
                return await Clone().ToDataTableAsync().ConfigureAwait(false);
            }
        }
        public virtual async Task<DataTable> ToOffsetDataTableByEntityPageAsync(int pageNumber, int pageSize, RefAsync<int> totalNumber)
        {
            return this.Context.Utilities.ListToDataTable(await ToOffsetPageAsync(pageNumber, pageSize, totalNumber).ConfigureAwait(false));
        }

        public virtual async Task<DataTable> ToDataTableAsync()
        {
            QueryBuilder.ResultType = typeof(SugarCacheDataTable);
            InitMapping();
            var sqlObj = this._ToSql();
            RestoreMapping();
            DataTable result = null;
            if (IsCache)
            {
                var cacheService = this.Context.CurrentConnectionConfig.ConfigureExternalServices.DataInfoCacheService;
                result = CacheSchemeMain.GetOrCreate<DataTable>(cacheService, this.QueryBuilder, () => this.Db.GetDataTable(sqlObj.Key, sqlObj.Value), CacheTime, this.Context, CacheKey);
            }
            else
            {
                result = await Db.GetDataTableAsync(sqlObj.Key, sqlObj.Value).ConfigureAwait(false);
            }
            return result;
        }
        public Task<DataTable> ToDataTablePageAsync(int pageIndex, int pageSize)
        {
            pageIndex = _PageList(pageIndex, pageSize);
            return ToDataTableAsync();
        }
        public virtual async Task<DataTable> ToDataTablePageAsync(int pageIndex, int pageSize, RefAsync<int> totalNumber)
        {
            var oldMapping = this.Context.MappingTables;
            totalNumber.Value = await Clone().CountAsync().ConfigureAwait(false);
            this.Context.MappingTables = oldMapping;
            return await Clone().ToDataTablePageAsync(pageIndex, pageSize).ConfigureAwait(false);
        }
        public virtual async Task<DataTable> ToDataTableByEntityPageAsync(int pageNumber, int pageSize, RefAsync<int> totalNumber)
        {
            var list = await ToPageListAsync(pageNumber, pageSize, totalNumber).ConfigureAwait(false);
            return this.Context.Utilities.ListToDataTable(list);
        }
        public Task<List<T>> ToOffsetPageAsync(int pageNumber, int pageSize, RefAsync<int> totalNumber, CancellationToken token)
        {
            this.Context.Ado.CancellationToken = token;
            return ToOffsetPageAsync(pageNumber, pageSize, totalNumber);
        }
        public Task<List<T>> ToOffsetPageAsync(int pageNumber, int pageSize, RefAsync<int> totalNumber, RefAsync<int> totalPage, CancellationToken token)
        {
            this.Context.Ado.CancellationToken = token;
            return ToOffsetPageAsync(pageNumber, pageSize, totalNumber, totalPage);
        }
        public virtual async Task<List<T>> ToOffsetPageAsync(int pageNumber, int pageSize, RefAsync<int> totalNumber, RefAsync<int> totalPage)
        {
            var result = await ToOffsetPageAsync(pageNumber, pageSize, totalNumber).ConfigureAwait(false);
            totalPage.Value = (totalNumber.Value + pageSize - 1) / pageSize;
            return result;
        }
        public virtual async Task<List<T>> ToOffsetPageAsync(int pageIndex, int pageSize, RefAsync<int> totalNumber)
        {
            if (this.Context.CurrentConnectionConfig.DbType != DbType.SqlServer)
            {
                this.QueryBuilder.Offset = "true";
                return await ToPageListAsync(pageIndex, pageSize, totalNumber).ConfigureAwait(false);
            }
            else
            {
                totalNumber.Value = await Clone().CountAsync().ConfigureAwait(false);
                _ToOffsetPage(pageIndex, pageSize);
                return await Clone().ToListAsync().ConfigureAwait(false);
            }
        }

        public virtual async Task ForEachAsync(Action<T> action, int singleMaxReads = 300, System.Threading.CancellationTokenSource cancellationTokenSource = null)
        {
            Check.Exception(this.QueryBuilder.Skip > 0 || this.QueryBuilder.Take > 0, ErrorMessage.GetThrowMessage("no support Skip take, use PageForEach", "不支持Skip Take,请使用 Queryale.PageForEach"));
            RefAsync<int> totalNumber = 0;
            RefAsync<int> totalPage = 1;
            for (int i = 1; i <= totalPage; i++)
            {
                if (cancellationTokenSource?.IsCancellationRequested == true) return;
                var queryable = this.Clone();
                var page =
                    totalPage == 1 ?
                    await queryable.ToPageListAsync(i, singleMaxReads, totalNumber, totalPage).ConfigureAwait(false) :
                    await queryable.ToPageListAsync(i, singleMaxReads).ConfigureAwait(false);
                foreach (var item in page)
                {
                    if (cancellationTokenSource?.IsCancellationRequested == true) return;
                    action.Invoke(item);
                }
            }
        }
        public virtual async Task ForEachByPageAsync(Action<T> action, int pageIndex, int pageSize, RefAsync<int> totalNumber, int singleMaxReads = 300, System.Threading.CancellationTokenSource cancellationTokenSource = null)
        {
            int count = await this.Clone().CountAsync().ConfigureAwait(false);
            if (count > 0)
            {
                if (pageSize > singleMaxReads && count - ((pageIndex - 1) * pageSize) > singleMaxReads)
                {
                    Int32 Skip = (pageIndex - 1) * pageSize;
                    Int32 NowCount = count - Skip;
                    Int32 number = 0;
                    if (NowCount > pageSize) NowCount = pageSize;
                    while (NowCount > 0)
                    {
                        if (cancellationTokenSource?.IsCancellationRequested == true) return;
                        if (number + singleMaxReads > pageSize) singleMaxReads = NowCount;
                        foreach (var item in await Clone().Skip(Skip).Take(singleMaxReads).ToListAsync().ConfigureAwait(false))
                        {
                            if (cancellationTokenSource?.IsCancellationRequested == true) return;
                            action.Invoke(item);
                        }
                        NowCount -= singleMaxReads;
                        Skip += singleMaxReads;
                        number += singleMaxReads;
                    }
                }
                else
                {
                    if (cancellationTokenSource?.IsCancellationRequested == true) return;
                    foreach (var item in await this.Clone().ToPageListAsync(pageIndex, pageSize).ConfigureAwait(false))
                    {
                        if (cancellationTokenSource?.IsCancellationRequested == true) return;
                        action.Invoke(item);
                    }
                }
            }
            totalNumber.Value = count;
        }

        public virtual async Task<List<T>> SetContextAsync<ParameterT>(Expression<Func<T, object>> thisField1, Expression<Func<object>> mappingField1,
Expression<Func<T, object>> thisField2, Expression<Func<object>> mappingField2,
ParameterT parameter)
        {
            if (parameter == null)
            {
                return new List<T>();
            }
            var rightEntity = this.Context.EntityMaintenance.GetEntityInfo<ParameterT>();
            var leftEntity = this.Context.EntityMaintenance.GetEntityInfo<T>();
            List<T> result = new List<T>();
            var queryableContext = this.Context.TempItems["Queryable_To_Context"] as MapperContext<ParameterT>;
            var list = queryableContext.list;
            var key = thisField1.ToString() + mappingField1.ToString() +
                      thisField2.ToString() + mappingField2.ToString() +
                       typeof(ParameterT).FullName + typeof(T).FullName;
            MappingFieldsHelper<ParameterT> fieldsHelper = new MappingFieldsHelper<ParameterT>();
            var mappings = new List<MappingFieldsExpression>() {
               new MappingFieldsExpression(){
               LeftColumnExpression=thisField1,
               LeftEntityColumn=leftEntity.Columns.First(it=>it.PropertyName==ExpressionTool.GetMemberName(thisField1)),
               RightColumnExpression=mappingField1,
               RightEntityColumn=rightEntity.Columns.First(it=>it.PropertyName==ExpressionTool.GetMemberName(mappingField1))
             },
               new MappingFieldsExpression(){
               LeftColumnExpression=thisField2,
               LeftEntityColumn=leftEntity.Columns.First(it=>it.PropertyName==ExpressionTool.GetMemberName(thisField2)),
               RightColumnExpression=mappingField2,
               RightEntityColumn=rightEntity.Columns.First(it=>it.PropertyName==ExpressionTool.GetMemberName(mappingField2))
             }
            };
            var conditionals = fieldsHelper.GetMappingSql(list.Cast<object>().ToList(), mappings);
            if (queryableContext.TempChildLists == null)
                queryableContext.TempChildLists = new Dictionary<string, object>();
            if (list != null && queryableContext.TempChildLists.TryGetValue(key, out object? value))
            {
                result = (List<T>)value;
            }
            else
            {
                result = await Clone().Where(conditionals, true).ToListAsync().ConfigureAwait(false);
                queryableContext.TempChildLists[key] = result;
            }
            List<object> listObj = result.Select(it => (object)it).ToList();
            object obj = (object)parameter;
            var newResult = fieldsHelper.GetSetList(obj, listObj, mappings).Select(it => (T)it).ToList();
            return newResult;
        }
        public virtual async Task<List<T>> SetContextAsync<ParameterT>(Expression<Func<T, object>> thisField, Expression<Func<object>> mappingField, ParameterT parameter)
        {
            List<T> result = new List<T>();
            var entity = this.Context.EntityMaintenance.GetEntityInfo<ParameterT>();
            var queryableContext = this.Context.TempItems["Queryable_To_Context"] as MapperContext<ParameterT>;
            var list = queryableContext.list;
            var pkName = "";
            if ((mappingField as LambdaExpression).Body is UnaryExpression)
            {
                pkName = (((mappingField as LambdaExpression).Body as UnaryExpression).Operand as MemberExpression).Member.Name;
            }
            else
            {
                pkName = ((mappingField as LambdaExpression).Body as MemberExpression).Member.Name;
            }
            var key = thisField.ToString() + mappingField.ToString() + typeof(ParameterT).FullName + typeof(T).FullName;
            var ids = list.Select(it => it.GetType().GetProperty(pkName).GetValue(it)).Distinct();
            if (queryableContext.TempChildLists == null)
                queryableContext.TempChildLists = new Dictionary<string, object>();
            if (list != null && queryableContext.TempChildLists.TryGetValue(key, out object? value))
            {
                result = (List<T>)value;
            }
            else
            {
                if (queryableContext.TempChildLists == null)
                    queryableContext.TempChildLists = new Dictionary<string, object>();
                await Context.Utilities.PageEachAsync(ids, 200, async pageIds => result.AddRange(await Clone().In(thisField, pageIds).ToListAsync().ConfigureAwait(false))).ConfigureAwait(false);
                queryableContext.TempChildLists[key] = result;
            }
            var name = "";
            if ((thisField as LambdaExpression).Body is UnaryExpression)
            {
                name = (((thisField as LambdaExpression).Body as UnaryExpression).Operand as MemberExpression).Member.Name;
            }
            else
            {
                name = ((thisField as LambdaExpression).Body as MemberExpression).Member.Name;
            }
            var pkValue = parameter.GetType().GetProperty(pkName).GetValue(parameter);
            result = result.Where(it => it.GetType().GetProperty(name).GetValue(it).ObjToString() == pkValue.ObjToString()).ToList();
            return result;
        }
        public virtual async Task<Dictionary<string, ValueType>> ToDictionaryAsync<ValueType>(Expression<Func<T, object>> key, Expression<Func<T, object>> value)
        {
            return (await ToDictionaryAsync(key, value).ConfigureAwait(false)).ToDictionary(it => it.Key, it => (ValueType)UtilMethods.ChangeType2(it.Value, typeof(ValueType)));
        }
        public virtual async Task<Dictionary<string, object>> ToDictionaryAsync(Expression<Func<T, object>> key, Expression<Func<T, object>> value)
        {
            if (this.QueryBuilder.IsSingle() == false && (this.QueryBuilder.AsTables == null || this.QueryBuilder.AsTables.Count == 0))
            {
                return await MergeTable().ToDictionaryAsync(key, value).ConfigureAwait(false);
            }
            this.QueryBuilder.ResultType = typeof(SugarCacheDictionary);
            var keyName = QueryBuilder.GetExpressionValue(key, ResolveExpressType.FieldSingle).GetResultString();
            var valueName = QueryBuilder.GetExpressionValue(value, ResolveExpressType.FieldSingle).GetResultString();
            var list = await Select<KeyValuePair<string, object>>(keyName + "," + valueName).ToListAsync().ConfigureAwait(false);
            var isJson = this.Context.EntityMaintenance.GetEntityInfo<T>().Columns.Where(it => it.IsJson && it.PropertyName == ExpressionTool.GetMemberName(value)).Any();
            if (isJson)
            {
                var result = (await this.Select<T>(keyName + "," + valueName).ToListAsync().ConfigureAwait(false)).ToDictionary(ExpressionTool.GetMemberName(key), ExpressionTool.GetMemberName(value));
                return result;
            }
            else
            {
                var result = list.ToDictionary(it => it.Key.ObjToString(), it => it.Value);
                return result;
            }
        }
        public virtual async Task<List<T>> ToTreeAsync(string childPropertyName, string parentIdPropertyName, object rootValue, string primaryKeyPropertyName)
        {
            var entity = this.Context.EntityMaintenance.GetEntityInfo<T>();
            var pk = primaryKeyPropertyName;
            var list = await ToListAsync().ConfigureAwait(false);
            Expression<Func<T, IEnumerable<object>>> childListExpression = (Expression<Func<T, IEnumerable<object>>>)ExpressionBuilderHelper.CreateExpressionSelectField(typeof(T), childPropertyName, typeof(IEnumerable<object>));
            Expression<Func<T, object>> parentIdExpression = (Expression<Func<T, object>>)ExpressionBuilderHelper.CreateExpressionSelectFieldObject(typeof(T), parentIdPropertyName);
            return GetTreeRoot(childListExpression, parentIdExpression, pk, list, rootValue) ?? new List<T>();
        }
        public virtual async Task<List<T>> ToTreeAsync(Expression<Func<T, IEnumerable<object>>> childListExpression, Expression<Func<T, object>> parentIdExpression, object rootValue, object[] childIds)
        {
            var list = await ToListAsync().ConfigureAwait(false);
            return TreeAndFilterIds(childListExpression, parentIdExpression, rootValue, childIds, ref list) ?? new List<T>();
        }
        public virtual async Task<List<T>> ToTreeAsync(Expression<Func<T, IEnumerable<object>>> childListExpression, Expression<Func<T, object>> parentIdExpression, object rootValue, object[] childIds, Expression<Func<T, object>> primaryKeyExpression)
        {
            var list = await ToListAsync().ConfigureAwait(false);
            return TreeAndFilterIds(childListExpression, parentIdExpression, primaryKeyExpression, rootValue, childIds, ref list) ?? new List<T>();
        }
        public virtual async Task<List<T>> ToTreeAsync(Expression<Func<T, IEnumerable<object>>> childListExpression, Expression<Func<T, object>> parentIdExpression, object rootValue)
        {
            var entity = this.Context.EntityMaintenance.GetEntityInfo<T>();
            var pk = GetTreeKey(entity);
            var list = await ToListAsync().ConfigureAwait(false);
            return GetTreeRoot(childListExpression, parentIdExpression, pk, list, rootValue) ?? new List<T>();
        }
        public virtual async Task<List<T>> ToTreeAsync(Expression<Func<T, IEnumerable<object>>> childListExpression, Expression<Func<T, object>> parentIdExpression, object rootValue, Expression<Func<T, object>> primaryKeyExpression)
        {
            var entity = this.Context.EntityMaintenance.GetEntityInfo<T>();
            var pk = ExpressionTool.GetMemberName(primaryKeyExpression);
            var list = await ToListAsync().ConfigureAwait(false);
            return GetTreeRoot(childListExpression, parentIdExpression, pk, list, rootValue) ?? new List<T>();
        }
        public virtual async Task<List<T>> ToParentListAsync(Expression<Func<T, object>> parentIdExpression, object primaryKeyValue)
        {
            List<T> result = new List<T>();
            var entity = this.Context.EntityMaintenance.GetEntityInfo<T>();
            var isTreeKey = entity.Columns.Any(it => it.IsTreeKey);
            if (isTreeKey)
            {
                return await _ToParentListByTreeKeyAsync(parentIdExpression, primaryKeyValue).ConfigureAwait(false);
            }
            Check.Exception(entity.Columns.Where(it => it.IsPrimarykey).Any(), "No Primary key");
            var parentIdName = UtilConvert.ToMemberExpression((parentIdExpression as LambdaExpression).Body).Member.Name;
            var ParentInfo = entity.Columns.First(it => it.PropertyName == parentIdName);
            var parentPropertyName = ParentInfo.DbColumnName;
            var tableName = this.QueryBuilder.GetTableNameString;
            if (this.QueryBuilder.IsSingle() == false)
            {
                if (this.QueryBuilder.JoinQueryInfos.Count > 0)
                {
                    tableName = this.QueryBuilder.JoinQueryInfos[0].TableName;
                }
                if (this.QueryBuilder.EasyJoinInfos.Count > 0)
                {
                    tableName = this.QueryBuilder.JoinQueryInfos[0].TableName;
                }
            }
            var current = await Context.Queryable<T>().AS(tableName).WithCacheIF(IsCache, CacheTime).Filter(null, QueryBuilder.IsDisabledGobalFilter).InSingleAsync(primaryKeyValue).ConfigureAwait(false);
            if (current != null)
            {
                result.Add(current);
                object parentId = ParentInfo.PropertyInfo.GetValue(current, null);
                int i = 0;
                while (parentId != null && await Context.Queryable<T>().AS(tableName).Filter(null, QueryBuilder.IsDisabledGobalFilter).In([parentId]).AnyAsync().ConfigureAwait(false))
                {
                    Check.Exception(i > 100, ErrorMessage.GetThrowMessage("Dead cycle", "出现死循环或超出循环上限（100），检查最顶层的ParentId是否是null或者0"));
                    var parent = await Context.Queryable<T>().AS(tableName).WithCacheIF(IsCache, CacheTime).Filter(null, QueryBuilder.IsDisabledGobalFilter).InSingleAsync(parentId).ConfigureAwait(false);
                    result.Add(parent);
                    parentId = ParentInfo.PropertyInfo.GetValue(parent, null);
                    ++i;
                }
            }
            return result;
        }
        public virtual async Task<List<T>> ToParentListAsync(Expression<Func<T, object>> parentIdExpression, object primaryKeyValue, Expression<Func<T, bool>> parentWhereExpression)
        {
            List<T> result = new List<T>();
            var entity = this.Context.EntityMaintenance.GetEntityInfo<T>();
            var isTreeKey = entity.Columns.Any(it => it.IsTreeKey);
            if (isTreeKey)
            {
                return await _ToParentListByTreeKeyAsync(parentIdExpression, primaryKeyValue, parentWhereExpression).ConfigureAwait(false);
            }
            Check.Exception(entity.Columns.Where(it => it.IsPrimarykey).Any(), "No Primary key");
            var parentIdName = UtilConvert.ToMemberExpression((parentIdExpression as LambdaExpression).Body).Member.Name;
            var ParentInfo = entity.Columns.First(it => it.PropertyName == parentIdName);
            var parentPropertyName = ParentInfo.DbColumnName;
            var tableName = this.QueryBuilder.GetTableNameString;
            if (this.QueryBuilder.IsSingle() == false)
            {
                if (this.QueryBuilder.JoinQueryInfos.Count > 0)
                {
                    tableName = this.QueryBuilder.JoinQueryInfos[0].TableName;
                }
                if (this.QueryBuilder.EasyJoinInfos.Count > 0)
                {
                    tableName = this.QueryBuilder.JoinQueryInfos[0].TableName;
                }
            }
            var current = await Context.Queryable<T>().AS(tableName).WhereIF(parentWhereExpression != default, parentWhereExpression).Filter(null, QueryBuilder.IsDisabledGobalFilter).InSingleAsync(primaryKeyValue).ConfigureAwait(false);
            if (current != null)
            {
                result.Add(current);
                object parentId = ParentInfo.PropertyInfo.GetValue(current, null);
                int i = 0;
                while (parentId != null && await Context.Queryable<T>().AS(tableName).WhereIF(parentWhereExpression != default, parentWhereExpression).Filter(null, QueryBuilder.IsDisabledGobalFilter).In([parentId]).AnyAsync().ConfigureAwait(false))
                {
                    Check.Exception(i > 100, ErrorMessage.GetThrowMessage("Dead cycle", "出现死循环或超出循环上限（100），检查最顶层的ParentId是否是null或者0"));
                    var parent = await Context.Queryable<T>().AS(tableName).WhereIF(parentWhereExpression != default, parentWhereExpression).Filter(null, QueryBuilder.IsDisabledGobalFilter).InSingleAsync(parentId).ConfigureAwait(false);
                    result.Add(parent);
                    parentId = ParentInfo.PropertyInfo.GetValue(parent, null);
                    ++i;
                }
            }
            return result;
        }
        public virtual async Task<List<T>> ToChildListAsync(Expression<Func<T, object>> parentIdExpression, object primaryKeyValue, bool isContainOneself = true)
        {
            var entity = this.Context.EntityMaintenance.GetEntityInfo<T>();
            var pk = GetTreeKey(entity);
            var list = await ToListAsync().ConfigureAwait(false);
            return GetChildList(parentIdExpression, pk, list, primaryKeyValue, isContainOneself);
        }

        public virtual async Task<List<T>> ToChildListAsync(Expression<Func<T, object>> parentIdExpression, object[] primaryKeyValues, bool isContainOneself = true)
        {
            var entity = this.Context.EntityMaintenance.GetEntityInfo<T>();
            var pk = GetTreeKey(entity);
            var list = await ToListAsync().ConfigureAwait(false);
            List<T> result = new List<T>();
            foreach (var item in primaryKeyValues)
            {
                result.AddRange(GetChildList(parentIdExpression, pk, list, item, isContainOneself));
            }
            return result;
        }
        public Task<int> IntoTableAsync<TableEntityType>(CancellationToken cancellationToken = default)
        {
            return IntoTableAsync(typeof(TableEntityType), cancellationToken);
        }
        public Task<int> IntoTableAsync<TableEntityType>(string TableName, CancellationToken cancellationToken = default)
        {
            return IntoTableAsync(typeof(TableEntityType), TableName, cancellationToken);
        }
        public Task<int> IntoTableAsync(Type TableEntityType, CancellationToken cancellationToken = default)
        {
            var entityInfo = this.Context.EntityMaintenance.GetEntityInfo(TableEntityType);
            var name = this.SqlBuilder.GetTranslationTableName(entityInfo.DbTableName);
            return IntoTableAsync(TableEntityType, name, cancellationToken);
        }
        public virtual async Task<int> IntoTableAsync(Type TableEntityType, string TableName, CancellationToken cancellationToken = default)
        {
            this.Context.Ado.CancellationToken = cancellationToken;
            KeyValuePair<string, IReadOnlyCollection<SugarParameter>> sqlInfo;
            string sql;
            OutIntoTableSql(TableName, out sqlInfo, out sql, TableEntityType);
            return await Context.Ado.ExecuteCommandAsync(sql, sqlInfo.Value).ConfigureAwait(false);
        }
    }
}
