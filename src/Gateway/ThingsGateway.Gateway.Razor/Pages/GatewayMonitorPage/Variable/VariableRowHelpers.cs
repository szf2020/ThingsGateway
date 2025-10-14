//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Gateway.Razor;

internal static class VariableRowHelpers
{
    internal static Alignment GetAlign(this ITableColumn col) => col.Align ?? Alignment.None;
    internal static bool GetTextWrap(this ITableColumn col) => col.TextWrap ?? false;
    internal static bool GetShowTips(this ITableColumn col) => col.ShowTips ?? false;


    internal static RenderFragment RenderColor<TItem>(this ITableColumn col, TItem item) => builder =>
    {
        var val = GetItemValue(col, item);
        var v = val?.ToString() ?? "#000";
        var style = $"background-color: {v};";
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "is-color");
        builder.AddAttribute(2, "style", style);
        builder.CloseElement();
    };
    internal static object? GetItemValue<TItem>(this ITableColumn col, TItem item)
    {
        var fieldName = col.GetFieldName();
        object? ret;
        if (item is IDynamicObject dynamicObject)
        {
            ret = dynamicObject.GetValue(fieldName);
        }
        else
        {
            ret = Utility.GetPropertyValue<TItem, object?>(item, fieldName);

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
        }
        return ret;
    }
    internal static bool GetTextEllipsis(this ITableColumn col) => col.TextEllipsis ?? false;

}