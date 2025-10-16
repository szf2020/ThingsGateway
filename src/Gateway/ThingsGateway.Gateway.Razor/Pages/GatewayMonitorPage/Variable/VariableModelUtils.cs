using Microsoft.CSharp.RuntimeBinder;

using System.Dynamic;
using System.Linq.Expressions;

using ThingsGateway.Common.Extension;
using ThingsGateway.NewLife.Caching;
using ThingsGateway.NewLife.Json.Extension;
namespace ThingsGateway.Gateway.Razor;

public static class VariableModelUtils
{
    static MemoryCache MemoryCache = new();
    public static object GetPropertyValue(VariableRuntime model, string fieldName)
    {
        if (model == null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        if (MemoryCache.TryGetValue(fieldName, out Func<VariableRuntime, object> data))
        {
            return data(model);
        }
        else
        {
            var ret = MemoryCache.GetOrAdd(fieldName, (fieldName) =>
        {
            return GetPropertyValueLambda<VariableRuntime, object?>(fieldName).Compile();
        })(model);
            return ret;
        }
    }
    /// <summary>
    /// 获取属性方法 Lambda 表达式
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static Expression<Func<TModel, TResult>> GetPropertyValueLambda<TModel, TResult>(string propertyName) where TModel : class, new()
    {

        var type = typeof(TModel);
        var parameter = Expression.Parameter(typeof(TModel));

        return !type.Assembly.IsDynamic && propertyName.Contains('.')
            ? GetComplexPropertyExpression()
            : GetSimplePropertyExpression();

        Expression<Func<TModel, TResult>> GetSimplePropertyExpression()
        {
            Expression body;
            var p = type.GetPropertyByName(propertyName);
            if (p != null)
            {
                body = Expression.Property(Expression.Convert(parameter, type), p);
            }
            else if (type.IsAssignableTo(typeof(IDynamicMetaObjectProvider)))
            {
                var binder = Microsoft.CSharp.RuntimeBinder.Binder.GetMember(
                    CSharpBinderFlags.None,
                    propertyName,
                    type,
                    [CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)]);
                body = Expression.Dynamic(binder, typeof(object), parameter);
            }
            else
            {
                throw new InvalidOperationException($"类型 {type.Name} 未找到 {propertyName} 属性，无法获取其值");
            }

            return Expression.Lambda<Func<TModel, TResult>>(Expression.Convert(body, typeof(TResult)), parameter);
        }

        Expression<Func<TModel, TResult>> GetComplexPropertyExpression()
        {
            var propertyNames = propertyName.Split(".");
            Expression? body = null;
            Type t = type;
            object? propertyInstance = new TModel();
            foreach (var name in propertyNames)
            {
                var p = t.GetPropertyByName(name) ?? throw new InvalidOperationException($"类型 {type.Name} 未找到 {name} 属性，无法获取其值");
                propertyInstance = p.GetValue(propertyInstance);
                if (propertyInstance != null)
                {
                    t = propertyInstance.GetType();
                }

                body = Expression.Property(body ?? Expression.Convert(parameter, type), p);
            }
            return Expression.Lambda<Func<TModel, TResult>>(Expression.Convert(body!, typeof(TResult)), parameter);
        }
    }

    public static string GetValue(VariableRuntime row, string fieldName)
    {
        switch (fieldName)
        {
            case nameof(VariableRuntime.Value):
             return row.Value?.ToSystemTextJsonString(false) ?? string.Empty;
            case nameof(VariableRuntime.RawValue):
             return row.RawValue?.ToSystemTextJsonString(false) ?? string.Empty;
            case nameof(VariableRuntime.LastSetValue):
             return row.LastSetValue?.ToSystemTextJsonString(false) ?? string.Empty;
            case nameof(VariableRuntime.ChangeTime):
                return row.ChangeTime.ToString("dd-HH:mm:ss.fff");

            case nameof(VariableRuntime.CollectTime):
                return row.CollectTime.ToString("dd-HH:mm:ss.fff");

            case nameof(VariableRuntime.IsOnline):
                return row.IsOnline.ToString();

            case nameof(VariableRuntime.LastErrorMessage):
              return row.LastErrorMessage;


            case nameof(VariableRuntime.RuntimeType):
              return row.RuntimeType;
            default:

                var ret = VariableModelUtils.GetPropertyValue(row, fieldName);

                if (ret != null)
                {
                    var t = ret.GetType();
                    if (t.IsEnum)
                    {
                        // 如果是枚举这里返回 枚举的描述信息
                        var itemName = ret.ToString();
                        if (!string.IsNullOrEmpty(itemName))
                        {
                            ret = Utility.GetDisplayName(t, itemName);
                        }
                    }
                }
                return  ret is string str ? str : ret?.ToString() ?? string.Empty;
        }
    }

    internal static Alignment GetAlign(this ITableColumn col) => col.Align ?? Alignment.None;
    internal static bool GetTextWrap(this ITableColumn col) => col.TextWrap ?? false;
    internal static bool GetShowTips(this ITableColumn col) => col.ShowTips ?? false;

    internal static bool GetTextEllipsis(this ITableColumn col) => col.TextEllipsis ?? false;
}
