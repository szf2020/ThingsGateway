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
            return LambdaExtensions.GetPropertyValueLambda<VariableRuntime, object?>(model, fieldName).Compile();
        })(model);
            return ret;
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
