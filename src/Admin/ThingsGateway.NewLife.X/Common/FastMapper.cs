using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace ThingsGateway.NewLife;

public class FastMapperOption
{
    public Dictionary<string, string> MapperProperties { get; set; } = new();
    public HashSet<string> IgnoreProperties { get; set; } = new();
}

public static class FastMapper
{
    // 泛型 + 非泛型共用缓存
    private static readonly ConcurrentDictionary<(Type Source, Type Target), Delegate> _mapCache
        = new ConcurrentDictionary<(Type, Type), Delegate>();

    #region 泛型入口
    public static TTarget Mapper<TSource, TTarget>(TSource source, FastMapperOption option = null)
        where TTarget : class, new()
    {
        if (source == null) return null;

        var key = (typeof(TSource), typeof(TTarget));
        if (!_mapCache.TryGetValue(key, out var del))
        {
            del = CreateMapFunc<TSource, TTarget>();
            _mapCache[key] = del;
        }

        var func = (Func<TSource, FastMapperOption, TTarget>)del;
        return func(source, option);
    }
    #endregion

    #region 非泛型入口
    public static object Mapper(object source, Type targetType, FastMapperOption option = null)
    {
        if (source == null) return null;

        var sourceType = source.GetType();
        var key = (sourceType, targetType);

        if (!_mapCache.TryGetValue(key, out var del))
        {
            // 动态生成泛型委托并缓存
            var method = typeof(FastMapper).GetMethod(nameof(CreateMapFunc), BindingFlags.NonPublic | BindingFlags.Static);
            var genericMethod = method.MakeGenericMethod(sourceType, targetType);
            del = genericMethod.Invoke(null, null) as Delegate;
            _mapCache[key] = del;
        }

        return del.DynamicInvoke(source, option);
    }
    #endregion

    #region Expression Tree 创建委托
    private static Func<TSource, FastMapperOption, TTarget> CreateMapFunc<TSource, TTarget>()
        where TTarget : class, new()
    {
        var sourceType = typeof(TSource);
        var targetType = typeof(TTarget);

        var sourceParam = Expression.Parameter(sourceType, "src");
        var optionParam = Expression.Parameter(typeof(FastMapperOption), "opt");
        var targetVar = Expression.Variable(targetType, "dest");

        var expressions = new List<Expression>
        {
            Expression.Assign(targetVar, Expression.New(targetType))
        };

        var sourceProperties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var targetProperties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var targetDict = targetProperties.Where(p => p.CanWrite).ToDictionary(p => p.Name);

        foreach (var sp in sourceProperties)
        {
            if (!sp.CanRead) continue;

            // FastMapperOption 重命名
            var pNameExpr = Expression.Constant(sp.Name);
            if (targetDict.TryGetValue(sp.Name, out PropertyInfo? tp))
            {
                // Ignore check
                Expression ignoreCheck = Expression.Call(
                    Expression.Property(optionParam, nameof(FastMapperOption.IgnoreProperties)),
                    nameof(HashSet<string>.Contains),
                    null,
                    pNameExpr
                );
                Expression assign;

                // 1️⃣ 简单类型直接赋值
                if (IsSimpleType(sp.PropertyType))
                {
                    assign = Expression.Assign(
                        Expression.Property(targetVar, tp),
                        Expression.Convert(Expression.Property(sourceParam, sp), tp.PropertyType)
                    );
                }
                // 2️⃣ 集合类型
                else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(sp.PropertyType) && sp.PropertyType != typeof(string))
                {
                    var elementSourceType = sp.PropertyType.IsArray
                        ? sp.PropertyType.GetElementType()
                        : sp.PropertyType.GetGenericArguments().FirstOrDefault();

                    var elementTargetType = tp.PropertyType.IsArray
                        ? tp.PropertyType.GetElementType()
                        : tp.PropertyType.GetGenericArguments().FirstOrDefault();

                    if (elementSourceType != null && elementTargetType != null)
                    {
                        var mapListMethod = typeof(FastMapper).GetMethod(nameof(MapList), BindingFlags.Public | BindingFlags.Static)
                            .MakeGenericMethod(elementSourceType, elementTargetType);

                        assign = Expression.Assign(
                            Expression.Property(targetVar, tp),
                            Expression.Call(mapListMethod, Expression.Property(sourceParam, sp), optionParam)
                        );
                    }
                    else continue;
                }
                // 3️⃣ 引用类型/嵌套对象
                else
                {
                    var mapMethod = typeof(FastMapper).GetMethod(nameof(Mapper), new Type[] { typeof(object), typeof(Type), typeof(FastMapperOption) });
                    var arg0 = Expression.Convert(Expression.Property(sourceParam, sp), typeof(object)); // ✅ fix Nullable/ValueType
                    assign = Expression.Assign(
                        Expression.Property(targetVar, tp),
                        Expression.Convert(
                            Expression.Call(mapMethod, arg0, Expression.Constant(tp.PropertyType), optionParam),
                            tp.PropertyType
                        )
                    );
                }

                expressions.Add(assign);
            }
        }

        expressions.Add(targetVar);
        var body = Expression.Block(new[] { targetVar }, expressions);
        return Expression.Lambda<Func<TSource, FastMapperOption, TTarget>>(body, sourceParam, optionParam).Compile();
    }
    #endregion

    #region 泛型集合映射
    public static IEnumerable<TTarget> MapList<TSource, TTarget>(IEnumerable<TSource> list, FastMapperOption option = null)
        where TTarget : class, new()
    {
        if (list == null) yield break;
        foreach (var item in list)
            yield return Mapper<TSource, TTarget>(item, option);
    }
    #endregion

    private static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive
               || type.IsEnum
               || type == typeof(string)
               || type == typeof(decimal)
               || type == typeof(DateTime)
               || (Nullable.GetUnderlyingType(type) != null && IsSimpleType(Nullable.GetUnderlyingType(type)));
    }
}
