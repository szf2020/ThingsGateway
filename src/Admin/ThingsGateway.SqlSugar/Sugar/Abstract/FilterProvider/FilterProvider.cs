using System.Linq.Expressions;

namespace ThingsGateway.SqlSugar
{
    /// <summary>查询过滤器提供者</summary>
    public class QueryFilterProvider : IFilter
    {
        /// <summary>SqlSugar上下文</summary>
        internal SqlSugarProvider Context { get; set; }
        /// <summary>过滤器列表</summary>
        private List<SqlFilterItem> _Filters { get; set; }
        /// <summary>备份的过滤器列表</summary>
        private List<SqlFilterItem> _BackUpFilters { get; set; }

        /// <summary>判断是否存在过滤器</summary>
        public bool Any()
        {
            return _Filters != null && _Filters.Count != 0;
        }

        /// <summary>添加过滤器</summary>
        public IFilter Add(SqlFilterItem filter)
        {
            if (_Filters == null)
                _Filters = new List<SqlFilterItem>();
            _Filters.Add(filter);
            return this;
        }

        /// <summary>移除指定名称的过滤器</summary>
        public void Remove(string filterName)
        {
            if (_Filters == null)
                _Filters = new List<SqlFilterItem>();
            _Filters.RemoveAll(it => it.FilterName == filterName);
        }

        /// <summary>获取过滤器列表</summary>
        public List<SqlFilterItem> GetFilterList
        {
            get
            {
                if (_Filters == null)
                    _Filters = new List<SqlFilterItem>();
                return _Filters;
            }
        }

        /// <summary>清空所有过滤器</summary>
        public void Clear()
        {
            _Filters = new List<SqlFilterItem>();
        }

        /// <summary>清空指定类型的过滤器</summary>
        public void Clear<T>()
        {
            _Filters = _Filters.Where(it => !(it is TableFilterItem<T>)).ToList();
        }

        /// <summary>清空多个类型的过滤器</summary>
        public void Clear(params Type[] types)
        {
            _Filters = _Filters.Where(it => !types.Contains(it.type)).ToList();
        }

        /// <summary>清空两种类型的过滤器</summary>
        public void Clear<T, T2>()
        {
            _Filters = _Filters.Where(it => !(it is TableFilterItem<T>) && !(it is TableFilterItem<T2>)).ToList();
        }

        /// <summary>清空三种类型的过滤器</summary>
        public void Clear<T, T2, T3>()
        {
            _Filters = _Filters.Where(it => !(it is TableFilterItem<T>) && !(it is TableFilterItem<T2>) && !(it is TableFilterItem<T3>)).ToList();
        }

        /// <summary>清空并备份当前过滤器</summary>
        public void ClearAndBackup()
        {
            _BackUpFilters = _Filters;
            _Filters = new List<SqlFilterItem>();
        }

        /// <summary>清空并备份指定类型的过滤器</summary>
        public void ClearAndBackup<T>()
        {
            _BackUpFilters = _Filters;
            _Filters = _BackUpFilters.Where(it => !(it is TableFilterItem<T>)).ToList();
        }

        /// <summary>清空并备份两种类型的过滤器</summary>
        public void ClearAndBackup<T, T2>()
        {
            _BackUpFilters = _Filters;
            _Filters = _BackUpFilters.Where(it => !(it is TableFilterItem<T>) && !(it is TableFilterItem<T2>)).ToList();
        }

        /// <summary>清空并备份三种类型的过滤器</summary>
        public void ClearAndBackup<T, T2, T3>()
        {
            _BackUpFilters = _Filters;
            _Filters = _BackUpFilters.Where(it => !(it is TableFilterItem<T>) && !(it is TableFilterItem<T2>) && !(it is TableFilterItem<T3>)).ToList();
        }

        /// <summary>清空并备份多个类型的过滤器</summary>
        public void ClearAndBackup(params Type[] types)
        {
            _BackUpFilters = _Filters;
            _Filters = _BackUpFilters.Where(it => !types.Contains(it.type)).ToList();
        }

        /// <summary>恢复备份的过滤器</summary>
        public void Restore()
        {
            _Filters = _BackUpFilters;
            if (_Filters == null)
            {
                _Filters = new List<SqlFilterItem>();
            }
        }

        /// <summary>添加表过滤器</summary>
        public QueryFilterProvider AddTableFilter<T>(Expression<Func<T, bool>> expression, FilterJoinPosition filterJoinType = FilterJoinPosition.On)
        {
            var isOn = filterJoinType == FilterJoinPosition.On;
            var tableFilter = new TableFilterItem<T>(expression, isOn);
            this.Add(tableFilter);
            return this;
        }

        /// <summary>条件添加表过滤器</summary>
        public QueryFilterProvider AddTableFilterIF<T>(bool isAppendFilter, Expression<Func<T, bool>> expression, FilterJoinPosition filterJoinType = FilterJoinPosition.On)
        {
            if (isAppendFilter)
            {
                AddTableFilter(expression, filterJoinType);
            }
            return this;
        }

        /// <summary>添加表过滤器(使用格式化字符串)</summary>
        public QueryFilterProvider AddTableFilter(Type type, string shortName, FormattableString expString, FilterJoinPosition filterJoinType = FilterJoinPosition.On)
        {
            var exp = DynamicCoreHelper.GetWhere(type, shortName, expString);
            return AddTableFilter(type, exp, filterJoinType);
        }

        /// <summary>添加表过滤器</summary>
        public QueryFilterProvider AddTableFilter(Type type, Expression expression, FilterJoinPosition filterJoinType = FilterJoinPosition.On)
        {
            var isOn = filterJoinType == FilterJoinPosition.On;
            this.Add(new TableFilterItem<object>(type, expression, isOn));
            return this;
        }

        /// <summary>条件添加表过滤器</summary>
        public QueryFilterProvider AddTableFilterIF(bool isAppendFilter, Type type, Expression expression, FilterJoinPosition posType = FilterJoinPosition.On)
        {
            if (isAppendFilter)
            {
                AddTableFilter(type, expression, posType);
            }
            return this;
        }

        /// <summary>过滤器连接位置枚举</summary>
        public enum FilterJoinPosition
        {
            /// <summary>ON条件</summary>
            On = 0,
            /// <summary>WHERE条件</summary>
            Where = 1
        }
    }
}