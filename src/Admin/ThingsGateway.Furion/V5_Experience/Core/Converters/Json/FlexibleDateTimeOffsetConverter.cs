// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// ------------------------------------------------------------------------

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ThingsGateway.Converters.Json;

/// <summary>
///     <see cref="DateTimeOffset" /> JSON 序列化转换器
/// </summary>
public partial class FlexibleDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    private static readonly DateTimeOffset s_epoch = new(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly Regex s_regex = Regex();

    /// <inheritdoc />
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var formatted = reader.GetString()!;
        var match = s_regex.Match(formatted);

        // 尝试获取 Unix epoch 日期格式
        // 参考文献：https://learn.microsoft.com/zh-cn/dotnet/standard/datetime/system-text-json-support#use-unix-epoch-date-format
        if (match.Success &&
            long.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture,
                out var unixTime) &&
            int.TryParse(match.Groups[3].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var hours) &&
            int.TryParse(match.Groups[4].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var minutes))
        {
            var sign = match.Groups[2].Value[0] == '+' ? 1 : -1;
            TimeSpan utcOffset = new(hours * sign, minutes * sign, 0);

            return s_epoch.AddMilliseconds(unixTime).ToOffset(utcOffset);
        }

        // 尝试获取日期
        if (reader.TryGetDateTimeOffset(out var value))
        {
            return value;
        }

        // 尝试获取 ISO 8601-1:2019 日期格式
        if (!DateTimeOffset.TryParse(formatted, out value))
        {
            throw new JsonException();
        }

        return value;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value);

    [GeneratedRegex(@"^/Date\(([+-]*\d+)([+-])(\d{2})(\d{2})\)/$", RegexOptions.CultureInvariant)]
    private static partial Regex Regex();
}