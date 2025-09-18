//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

#if NET6_0_OR_GREATER
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Globalization;
using System.Numerics;
using System.Text.Json;

namespace ThingsGateway.NewLife.Json.Extension;

public static class JsonElementExtensions
{
    public static string GetValue(object src, bool parseBoolNumber = false)
    {
        if (src == null)
            return string.Empty;

        switch (src)
        {
            case string strValue:
                return strValue;

            case bool boolValue:
                return boolValue ? parseBoolNumber ? "1" : "True" : parseBoolNumber ? "0" : "False";

            case JsonElement elem: // System.Text.Json.JsonElement
                return elem.ValueKind switch
                {
                    JsonValueKind.String => elem.GetString(),
                    JsonValueKind.Number => elem.GetRawText(),  // 或 elem.GetDecimal().ToString()
                    JsonValueKind.True => "1",
                    JsonValueKind.False => "0",
                    JsonValueKind.Null => string.Empty,
                    _ => elem.GetRawText(), // 对象、数组等直接输出 JSON
                };

            default:
                return (src).GetJTokenFromObj().ToString();
        }
    }


    /// <summary>
    /// 将 System.Text.Json.JsonElement 递归转换为 Newtonsoft.Json.Linq.JToken
    /// - tryParseDates: 是否尝试把字符串解析为 DateTime/DateTimeOffset
    /// - tryParseGuids: 是否尝试把字符串解析为 Guid
    /// </summary>
    public static JToken ToJToken(this JsonElement element, bool tryParseDates = true, bool tryParseGuids = true)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var obj = new JObject();
                foreach (var prop in element.EnumerateObject())
                    obj.Add(prop.Name, prop.Value.ToJToken(tryParseDates, tryParseGuids));
                return obj;

            case JsonValueKind.Array:
                var arr = new JArray();
                foreach (var item in element.EnumerateArray())
                    arr.Add(item.ToJToken(tryParseDates, tryParseGuids));
                return arr;

            case JsonValueKind.String:
                // 优先按语义尝试解析 Guid / DateTimeOffset / DateTime
                if (tryParseGuids && element.TryGetGuid(out Guid g))
                    return new JValue(g);

                if (tryParseDates && element.TryGetDateTimeOffset(out DateTimeOffset dto))
                    return new JValue(dto);

                if (tryParseDates && element.TryGetDateTime(out DateTime dt))
                    return new JValue(dt);

                return new JValue(element.GetString());

            case JsonValueKind.Number:
                return NumberElementToJToken(element);

            case JsonValueKind.True:
                return new JValue(true);

            case JsonValueKind.False:
                return new JValue(false);

            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
            default:
                return JValue.CreateNull();
        }
    }

    private static JToken NumberElementToJToken(JsonElement element)
    {
        // 取原始文本（保持原始表示，方便处理超出标准类型范围的数字）
        string raw = element.GetRawText(); // 例如 "123", "1.23e4"

        // 如果不含小数点或指数，优先尝试整数解析（long / ulong / BigInteger）
        if (!raw.Contains('.') && !raw.Contains('e') && !raw.Contains('E'))
        {
            if (long.TryParse(raw, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var l))
                return new JValue(l);

            if (ulong.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out var ul))
                return new JValue(ul);

            if (BigInteger.TryParse(raw, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var bi))
                // BigInteger 可能不被 JValue 直接识别为数字类型，使用 FromObject 保证正确表示
                return JToken.FromObject(bi);
        }

        // 含小数或指数，或整数解析失败，尝试 decimal -> double
        if (decimal.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var dec))
            return new JValue(dec);

        if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
            return new JValue(d);

        // 最后兜底：把原始文本当字符串返回（极端情况）
        return new JValue(raw);
    }

    /// <summary>
    /// 把 JToken 转成“平面”字符串，适合用于日志或写入 CSV 的单元格：
    /// - string -> 原文
    /// - bool -> "1"/"0"
    /// - number -> 原始数字文本
    /// - date -> ISO 8601 (o)
    /// - object/array -> 紧凑的 JSON 文本
    /// - null/undefined -> empty string
    /// </summary>
    public static string JTokenToPlainString(this JToken token)
    {
        if (token == null || token.Type == JTokenType.Null || token.Type == JTokenType.Undefined)
            return string.Empty;

        switch (token.Type)
        {
            case JTokenType.String:
                return token.Value<string>() ?? string.Empty;

            case JTokenType.Boolean:
                return token.Value<bool>() ? "1" : "0";

            case JTokenType.Integer:
            case JTokenType.Float:
                // 保持紧凑数字文本（不加引号）
                return token.ToString(Formatting.None);

            case JTokenType.Date:
                {
                    // Date 类型可能是 DateTime 或 DateTimeOffset
                    var val = token.Value<object>();
                    if (val is DateTimeOffset dto) return dto.ToString("o");
                    if (val is DateTime dt) return dt.ToString("o");
                    return token.ToString(Formatting.None);
                }

            default:
                // 对象/数组等，返回紧凑 JSON 表示
                return token.ToString(Formatting.None);
        }
    }
}
#endif