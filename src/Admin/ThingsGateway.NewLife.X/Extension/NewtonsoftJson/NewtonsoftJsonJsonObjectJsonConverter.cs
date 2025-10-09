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
#if NET6_0_OR_GREATER
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ThingsGateway.JsonSerialization;


/// <summary>
/// <see cref="System.Text.Json.Nodes.JsonObject"/> 类型序列化
/// </summary>
public class NewtonsoftJsonJsonObjectJsonConverter : JsonConverter<System.Text.Json.Nodes.JsonObject>
{
    /// <summary>
    /// 反序列化
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="objectType"></param>
    /// <param name="existingValue"></param>
    /// <param name="hasExistingValue"></param>
    /// <param name="serializer"></param>
    /// <returns></returns>
    public override System.Text.Json.Nodes.JsonObject ReadJson(JsonReader reader, Type objectType, System.Text.Json.Nodes.JsonObject existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return System.Text.Json.Nodes.JsonObject.Parse(JObject.Load(reader).ToString()).AsObject();
    }

    /// <summary>
    /// 序列化
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="serializer"></param>
    public override void WriteJson(JsonWriter writer, System.Text.Json.Nodes.JsonObject value, JsonSerializer serializer)
    {
        writer.WriteRawValue(value.ToJsonString());
    }
}

/// <summary>
/// <see cref="System.Text.Json.Nodes.JsonArray"/> 类型序列化
/// </summary>
public class NewtonsoftJsonJsonArrayJsonConverter : JsonConverter<System.Text.Json.Nodes.JsonArray>
{
    /// <summary>
    /// 反序列化
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="objectType"></param>
    /// <param name="existingValue"></param>
    /// <param name="hasExistingValue"></param>
    /// <param name="serializer"></param>
    /// <returns></returns>
    public override System.Text.Json.Nodes.JsonArray ReadJson(JsonReader reader, Type objectType, System.Text.Json.Nodes.JsonArray existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return System.Text.Json.Nodes.JsonArray.Parse(JArray.Load(reader).ToString()).AsArray();
    }

    /// <summary>
    /// 序列化
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="serializer"></param>
    public override void WriteJson(JsonWriter writer, System.Text.Json.Nodes.JsonArray value, JsonSerializer serializer)
    {
        writer.WriteRawValue(value.ToJsonString());
    }
}

#endif