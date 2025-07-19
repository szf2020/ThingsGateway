using System.Linq.Expressions;

namespace ThingsGateway.SqlSugar
{
    public partial class QueryableProvider<T> : QueryableAccessory, ISugarQueryable<T>
    {
        /// <summary>
        /// 包含单个关联属性的查询方法
        /// </summary>
        private void _Includes<T1, TReturn1>(SqlSugarProvider context, params Expression[] expressions)
        {
            Func<ISugarQueryable<object>, List<object>> selectR1 = it => it.Select<TReturn1>().ToList().Cast<object>().ToList();
            var navigat = new NavigatManager<T>();
            navigat.SelectR1 = selectR1;
            navigat.Expressions = expressions;
            navigat.Context = this.Context;
            navigat.IsCrossQueryWithAttr = this.QueryBuilder.IsCrossQueryWithAttr;
            navigat.CrossQueryItems = this.QueryBuilder.CrossQueryItems;
            navigat.QueryBuilder = this.QueryBuilder;
            if (this.QueryBuilder.Includes == null) this.QueryBuilder.Includes = new List<object>();
            this.QueryBuilder.Includes.Add(navigat);
        }

        /// <summary>
        /// 包含两个关联属性的查询方法
        /// </summary>
        private void _Includes<T1, TReturn1, TReturn2>(SqlSugarProvider context, params Expression[] expressions)
        {
            Func<ISugarQueryable<object>, List<object>> selectR1 = it => it.Select<TReturn1>().ToList().Cast<object>().ToList();
            Func<ISugarQueryable<object>, List<object>> selectR2 = it => it.Select<TReturn2>().ToList().Cast<object>().ToList();
            var navigat = new NavigatManager<T>();
            navigat.SelectR1 = selectR1;
            navigat.SelectR2 = selectR2;
            navigat.IsCrossQueryWithAttr = this.QueryBuilder.IsCrossQueryWithAttr;
            navigat.CrossQueryItems = this.QueryBuilder.CrossQueryItems;
            navigat.Expressions = expressions;
            navigat.Context = this.Context;
            navigat.QueryBuilder = this.QueryBuilder;
            if (this.QueryBuilder.Includes == null) this.QueryBuilder.Includes = new List<object>();
            this.QueryBuilder.Includes.Add(navigat);
        }

        /// <summary>
        /// 包含三个关联属性的查询方法
        /// </summary>
        private void _Includes<T1, TReturn1, TReturn2, TReturn3>(SqlSugarProvider context, params Expression[] expressions)
        {
            Func<ISugarQueryable<object>, List<object>> selectR1 = it => it.Select<TReturn1>().ToList().Cast<object>().ToList();
            Func<ISugarQueryable<object>, List<object>> selectR2 = it => it.Select<TReturn2>().ToList().Cast<object>().ToList();
            Func<ISugarQueryable<object>, List<object>> selectR3 = it => it.Select<TReturn3>().ToList().Cast<object>().ToList();
            var navigat = new NavigatManager<T>();
            navigat.SelectR1 = selectR1;
            navigat.SelectR2 = selectR2;
            navigat.SelectR3 = selectR3;
            navigat.IsCrossQueryWithAttr = this.QueryBuilder.IsCrossQueryWithAttr;
            navigat.CrossQueryItems = this.QueryBuilder.CrossQueryItems;
            navigat.Expressions = expressions;
            navigat.Context = this.Context;
            navigat.QueryBuilder = this.QueryBuilder;
            if (this.QueryBuilder.Includes == null) this.QueryBuilder.Includes = new List<object>();
            this.QueryBuilder.Includes.Add(navigat);
        }

        /// <summary>
        /// 包含四个关联属性的查询方法
        /// </summary>
        private void _Includes<T1, TReturn1, TReturn2, TReturn3, TReturn4>(SqlSugarProvider context, params Expression[] expressions)
        {
            Func<ISugarQueryable<object>, List<object>> selectR1 = it => it.Select<TReturn1>().ToList().Cast<object>().ToList();
            Func<ISugarQueryable<object>, List<object>> selectR2 = it => it.Select<TReturn2>().ToList().Cast<object>().ToList();
            Func<ISugarQueryable<object>, List<object>> selectR3 = it => it.Select<TReturn3>().ToList().Cast<object>().ToList();
            Func<ISugarQueryable<object>, List<object>> selectR4 = it => it.Select<TReturn4>().ToList().Cast<object>().ToList();
            var navigat = new NavigatManager<T>();
            navigat.SelectR1 = selectR1;
            navigat.SelectR2 = selectR2;
            navigat.SelectR3 = selectR3;
            navigat.SelectR4 = selectR4;
            navigat.Expressions = expressions;
            navigat.IsCrossQueryWithAttr = this.QueryBuilder.IsCrossQueryWithAttr;
            navigat.CrossQueryItems = this.QueryBuilder.CrossQueryItems;
            navigat.Context = this.Context;
            navigat.QueryBuilder = this.QueryBuilder;
            if (this.QueryBuilder.Includes == null) this.QueryBuilder.Includes = new List<object>();
            this.QueryBuilder.Includes.Add(navigat);
        }

        /// <summary>
        /// 包含五个关联属性的查询方法
        /// </summary>
        private void _Includes<T1, TReturn1, TReturn2, TReturn3, TReturn4, TReturn5>(SqlSugarProvider context, params Expression[] expressions)
        {
            Func<ISugarQueryable<object>, List<object>> selectR1 = it => it.Select<TReturn1>().ToList().Cast<object>().ToList();
            Func<ISugarQueryable<object>, List<object>> selectR2 = it => it.Select<TReturn2>().ToList().Cast<object>().ToList();
            Func<ISugarQueryable<object>, List<object>> selectR3 = it => it.Select<TReturn3>().ToList().Cast<object>().ToList();
            Func<ISugarQueryable<object>, List<object>> selectR4 = it => it.Select<TReturn4>().ToList().Cast<object>().ToList();
            Func<ISugarQueryable<object>, List<object>> selectR5 = it => it.Select<TReturn5>().ToList().Cast<object>().ToList();
            var navigat = new NavigatManager<T>();
            navigat.SelectR1 = selectR1;
            navigat.SelectR2 = selectR2;
            navigat.SelectR3 = selectR3;
            navigat.SelectR4 = selectR4;
            navigat.SelectR5 = selectR5;
            navigat.Expressions = expressions;
            navigat.IsCrossQueryWithAttr = this.QueryBuilder.IsCrossQueryWithAttr;
            navigat.CrossQueryItems = this.QueryBuilder.CrossQueryItems;
            navigat.Context = this.Context;
            navigat.QueryBuilder = this.QueryBuilder;
            if (this.QueryBuilder.Includes == null) this.QueryBuilder.Includes = new List<object>();
            this.QueryBuilder.Includes.Add(navigat);
        }

        /// <summary>
        /// 转换为导航查询对象
        /// </summary>
        public NavISugarQueryable<T> AsNavQueryable()
        {
            return GetNavSugarQueryable();
        }

        /// <summary>
        /// 获取导航查询对象
        /// </summary>
        private NavQueryableProvider<T> GetNavSugarQueryable()
        {
            var result = new NavQueryableProvider<T>();
            result.Context = this.Context;
            var clone = this.Clone();
            result.SqlBuilder = clone.SqlBuilder;
            result.QueryBuilder = clone.QueryBuilder;
            return result;
        }

        /// <summary>
        /// 获取多级关联查询对象
        /// </summary>
        private ISugarQueryable<T> GetManyQueryable<TReturn1>(Expression<Func<T, TReturn1>> include1)
        {
            ISugarQueryable<T> result = null;
            var isManyMembers = IsMembers(include1);
            if (isManyMembers)
            {
                var array = ExpressionTool.ExtractMemberNames(include1);
                if (array.Count > 1)
                {

                    if (array.Count == 2)
                    {
                        result = this.IncludesByNameString(array[0], array[1]);
                    }
                    else if (array.Count == 3)
                    {
                        result = this.IncludesByNameString(array[0], array[1], array[2]);
                    }
                    else if (array.Count == 4)
                    {
                        result = this.IncludesByNameString(array[0], array[1], array[2], array[3]);
                    }
                    else if (array.Count == 5)
                    {
                        result = this.IncludesByNameString(array[0], array[1], array[2], array[3], array[4]);
                    }
                    else if (array.Count == 6)
                    {
                        throw new Exception("Multiple levels of expression exceeded the upper limit");
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 检查是否是多级成员表达式
        /// </summary>
        private static bool IsMembers<TReturn1>(Expression<Func<T, TReturn1>> include1)
        {
            var isManyMembers = false;
            var x = ((include1 as LambdaExpression).Body as MemberExpression)?.Expression;
            if (x is MemberExpression)
            {
                var exp = (x as MemberExpression)?.Expression;
                if (exp != null)
                {
                    isManyMembers = true;
                }
            }
            return isManyMembers;
        }
    }

    public partial class NavQueryableProvider<T> : QueryableProvider<T>, NavISugarQueryable<T>
    {
        /// <summary>
        /// 包含单个关联属性的导航查询方法
        /// </summary>
        private void _Includes<T1, TReturn1>(SqlSugarProvider context, params Expression[] expressions)
        {
            Func<ISugarQueryable<object>, List<object>> selectR1 = it => it.Select<TReturn1>().ToList().Cast<object>().ToList();
            var navigat = new NavigatManager<T>();
            navigat.SelectR1 = selectR1;
            navigat.Expressions = expressions;
            navigat.Context = this.Context;
            navigat.IsCrossQueryWithAttr = this.QueryBuilder.IsCrossQueryWithAttr;
            navigat.CrossQueryItems = this.QueryBuilder.CrossQueryItems;
            navigat.QueryBuilder = this.QueryBuilder;
            if (this.QueryBuilder.Includes == null) this.QueryBuilder.Includes = new List<object>();
            this.QueryBuilder.Includes.Add(navigat);
        }

        /// <summary>
        /// 包含两个关联属性的导航查询方法
        /// </summary>
        private void _Includes<T1, TReturn1, TReturn2>(SqlSugarProvider context, params Expression[] expressions)
        {
            Func<ISugarQueryable<object>, List<object>> selectR1 = it => it.Select<TReturn1>().ToList().Cast<object>().ToList();
            Func<ISugarQueryable<object>, List<object>> selectR2 = it => it.Select<TReturn2>().ToList().Cast<object>().ToList();
            var navigat = new NavigatManager<T>();
            navigat.SelectR1 = selectR1;
            navigat.SelectR2 = selectR2;
            navigat.Expressions = expressions;
            navigat.IsCrossQueryWithAttr = this.QueryBuilder.IsCrossQueryWithAttr;
            navigat.CrossQueryItems = this.QueryBuilder.CrossQueryItems;
            navigat.Context = this.Context;
            navigat.QueryBuilder = this.QueryBuilder;
            if (this.QueryBuilder.Includes == null) this.QueryBuilder.Includes = new List<object>();
            this.QueryBuilder.Includes.Add(navigat);
        }

        /// <summary>
        /// 包含三个关联属性的导航查询方法
        /// </summary>
        private void _Includes<T1, TReturn1, TReturn2, TReturn3>(SqlSugarProvider context, params Expression[] expressions)
        {
            Func<ISugarQueryable<object>, List<object>> selectR1 = it => it.Select<TReturn1>().ToList().Cast<object>().ToList();
            Func<ISugarQueryable<object>, List<object>> selectR2 = it => it.Select<TReturn2>().ToList().Cast<object>().ToList();
            Func<ISugarQueryable<object>, List<object>> selectR3 = it => it.Select<TReturn3>().ToList().Cast<object>().ToList();
            var navigat = new NavigatManager<T>();
            navigat.SelectR1 = selectR1;
            navigat.SelectR2 = selectR2;
            navigat.SelectR3 = selectR3;
            navigat.Expressions = expressions;
            navigat.IsCrossQueryWithAttr = this.QueryBuilder.IsCrossQueryWithAttr;
            navigat.CrossQueryItems = this.QueryBuilder.CrossQueryItems;
            navigat.Context = this.Context;
            navigat.QueryBuilder = this.QueryBuilder;
            if (this.QueryBuilder.Includes == null) this.QueryBuilder.Includes = new List<object>();
            this.QueryBuilder.Includes.Add(navigat);
        }

        /// <summary>
        /// 包含四个关联属性的导航查询方法
        /// </summary>
        private void _Includes<T1, TReturn1, TReturn2, TReturn3, TReturn4>(SqlSugarProvider context, params Expression[] expressions)
        {
            Func<ISugarQueryable<object>, List<object>> selectR1 = it => it.Select<TReturn1>().ToList().Cast<object>().ToList();
            Func<ISugarQueryable<object>, List<object>> selectR2 = it => it.Select<TReturn2>().ToList().Cast<object>().ToList();
            Func<ISugarQueryable<object>, List<object>> selectR3 = it => it.Select<TReturn3>().ToList().Cast<object>().ToList();
            Func<ISugarQueryable<object>, List<object>> selectR4 = it => it.Select<TReturn4>().ToList().Cast<object>().ToList();
            var navigat = new NavigatManager<T>();
            navigat.SelectR1 = selectR1;
            navigat.SelectR2 = selectR2;
            navigat.SelectR3 = selectR3;
            navigat.SelectR4 = selectR4;
            navigat.Expressions = expressions;
            navigat.IsCrossQueryWithAttr = this.QueryBuilder.IsCrossQueryWithAttr;
            navigat.CrossQueryItems = this.QueryBuilder.CrossQueryItems;
            navigat.Context = this.Context;
            navigat.QueryBuilder = this.QueryBuilder;
            if (this.QueryBuilder.Includes == null) this.QueryBuilder.Includes = new List<object>();
            this.QueryBuilder.Includes.Add(navigat);
        }

        /// <summary>
        /// 包含五个关联属性的导航查询方法
        /// </summary>
        private void _Includes<T1, TReturn1, TReturn2, TReturn3, TReturn4, TReturn5>(SqlSugarProvider context, params Expression[] expressions)
        {
            Func<ISugarQueryable<object>, List<object>> selectR1 = it => it.Select<TReturn1>().ToList().Cast<object>().ToList();
            Func<ISugarQueryable<object>, List<object>> selectR2 = it => it.Select<TReturn2>().ToList().Cast<object>().ToList();
            Func<ISugarQueryable<object>, List<object>> selectR3 = it => it.Select<TReturn3>().ToList().Cast<object>().ToList();
            Func<ISugarQueryable<object>, List<object>> selectR4 = it => it.Select<TReturn4>().ToList().Cast<object>().ToList();
            Func<ISugarQueryable<object>, List<object>> selectR5 = it => it.Select<TReturn5>().ToList().Cast<object>().ToList();
            var navigat = new NavigatManager<T>();
            navigat.SelectR1 = selectR1;
            navigat.SelectR2 = selectR2;
            navigat.SelectR3 = selectR3;
            navigat.SelectR4 = selectR4;
            navigat.SelectR5 = selectR5;
            navigat.Expressions = expressions;
            navigat.IsCrossQueryWithAttr = this.QueryBuilder.IsCrossQueryWithAttr;
            navigat.CrossQueryItems = this.QueryBuilder.CrossQueryItems;
            navigat.Context = this.Context;
            navigat.QueryBuilder = this.QueryBuilder;
            if (this.QueryBuilder.Includes == null) this.QueryBuilder.Includes = new List<object>();
            this.QueryBuilder.Includes.Add(navigat);
        }

        /// <summary>
        /// 包含六个关联属性的导航查询方法
        /// </summary>
        private void _Includes<T1, TReturn1, TReturn2, TReturn3, TReturn4, TReturn5, TReturn6>(SqlSugarProvider context, params Expression[] expressions)
        {
            Func<ISugarQueryable<object>, List<object>> selectR1 = it => it.Select<TReturn1>().ToList().Cast<object>().ToList();
            Func<ISugarQueryable<object>, List<object>> selectR2 = it => it.Select<TReturn2>().ToList().Cast<object>().ToList();
            Func<ISugarQueryable<object>, List<object>> selectR3 = it => it.Select<TReturn3>().ToList().Cast<object>().ToList();
            Func<ISugarQueryable<object>, List<object>> selectR4 = it => it.Select<TReturn4>().ToList().Cast<object>().ToList();
            Func<ISugarQueryable<object>, List<object>> selectR5 = it => it.Select<TReturn5>().ToList().Cast<object>().ToList();
            Func<ISugarQueryable<object>, List<object>> selectR6 = it => it.Select<TReturn6>().ToList().Cast<object>().ToList();
            var navigat = new NavigatManager<T>();
            navigat.SelectR1 = selectR1;
            navigat.SelectR2 = selectR2;
            navigat.SelectR3 = selectR3;
            navigat.SelectR4 = selectR4;
            navigat.SelectR5 = selectR5;
            navigat.SelectR6 = selectR6;
            navigat.Expressions = expressions;
            navigat.IsCrossQueryWithAttr = this.QueryBuilder.IsCrossQueryWithAttr;
            navigat.CrossQueryItems = this.QueryBuilder.CrossQueryItems;
            navigat.Context = this.Context;
            navigat.QueryBuilder = this.QueryBuilder;
            if (this.QueryBuilder.Includes == null) this.QueryBuilder.Includes = new List<object>();
            this.QueryBuilder.Includes.Add(navigat);
        }

        /// <summary>
        /// 包含七个关联属性的导航查询方法
        /// </summary>
        private void _Includes<T1, TReturn1, TReturn2, TReturn3, TReturn4, TReturn5, TReturn6, TReturn7>(SqlSugarProvider context, params Expression[] expressions)
        {
            Func<ISugarQueryable<object>, List<object>> selectR1 = it => it.Select<TReturn1>().ToList().Cast<object>().ToList();
            Func<ISugarQueryable<object>, List<object>> selectR2 = it => it.Select<TReturn2>().ToList().Cast<object>().ToList();
            Func<ISugarQueryable<object>, List<object>> selectR3 = it => it.Select<TReturn3>().ToList().Cast<object>().ToList();
            Func<ISugarQueryable<object>, List<object>> selectR4 = it => it.Select<TReturn4>().ToList().Cast<object>().ToList();
            Func<ISugarQueryable<object>, List<object>> selectR5 = it => it.Select<TReturn5>().ToList().Cast<object>().ToList();
            Func<ISugarQueryable<object>, List<object>> selectR6 = it => it.Select<TReturn6>().ToList().Cast<object>().ToList();
            Func<ISugarQueryable<object>, List<object>> selectR7 = it => it.Select<TReturn7>().ToList().Cast<object>().ToList();
            var navigat = new NavigatManager<T>();
            navigat.SelectR1 = selectR1;
            navigat.SelectR2 = selectR2;
            navigat.SelectR3 = selectR3;
            navigat.SelectR4 = selectR4;
            navigat.SelectR5 = selectR5;
            navigat.SelectR6 = selectR6;
            navigat.SelectR7 = selectR7;
            navigat.Expressions = expressions;
            navigat.QueryBuilder = this.QueryBuilder;
            navigat.IsCrossQueryWithAttr = this.QueryBuilder.IsCrossQueryWithAttr;
            navigat.CrossQueryItems = this.QueryBuilder.CrossQueryItems;
            navigat.Context = this.Context;
            if (this.QueryBuilder.Includes == null) this.QueryBuilder.Includes = new List<object>();
            this.QueryBuilder.Includes.Add(navigat);
        }
    }
}