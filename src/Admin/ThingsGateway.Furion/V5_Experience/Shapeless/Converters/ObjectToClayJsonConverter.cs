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

using System.Text.Json;
using System.Text.Json.Serialization;

namespace ThingsGateway.Shapeless;

/// <summary>
///     <see cref="object" /> 转 <see cref="Clay" /> JSON 序列化转换器
/// </summary>
public sealed class ObjectToClayJsonConverter : JsonConverter<object>
{
    /// <inheritdoc cref="Options" />
    public ClayOptions? Options { get; set; }

    /// <inheritdoc />
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // 将 Utf8JsonReader 转换为 JsonElement
        var jsonElement = JsonElement.ParseValue(ref reader);

        // 检查 JSON 是否是对象或数组类型
        if (jsonElement.ValueKind is not (JsonValueKind.Object or JsonValueKind.Array))
        {
            return jsonElement;
        }

        // 初始化 ClayOptions 实例
        var clayOptions = Options ?? ClayOptions.Default;
        clayOptions.JsonSerializerOptions = options;

        return Clay.Parse(jsonElement.ToString(), clayOptions);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        // 空检查
        if ((object?)value is null)
        {
            writer.WriteNullValue();
            return;
        }

        // 检查是否是流变对象
        if (value is Clay clay)
        {
            writer.WriteRawValue(clay.ToJsonString(options));
            return;
        }

        // 检查是否是 object 类型，解决 new object() 出现无限递归问题
        if (value.GetType() == typeof(object))
        {
            writer.WriteStartObject();
            writer.WriteEndObject();
            return;
        }

        // 对于其他类型，正常序列化
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}