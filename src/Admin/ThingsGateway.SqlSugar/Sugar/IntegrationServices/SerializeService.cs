using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using ThingsGateway.NewLife.Json.Extension;

using JsonProperty = Newtonsoft.Json.Serialization.JsonProperty;

namespace ThingsGateway.SqlSugar
{
    public class SerializeService : ISerializeService
    {
        private static readonly JsonSerializerSettings _newtonsoftSettings = new()
        {
            NullValueHandling = NullValueHandling.Ignore,

            ContractResolver = new NewtonsoftResolver()
        };

        private static readonly JsonSerializerOptions _systemTextJsonSettings = new()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        };

        private static readonly ConcurrentDictionary<Type, SugarColumn> _typeInfoCache = new();

        static SerializeService()
        {
            var resolver = new DefaultJsonTypeInfoResolver
            {
            };
            resolver.Modifiers.Add(
                    ti =>
                    {
                        foreach (var prop in ti.Properties)
                        {
                            var sugar = _typeInfoCache.GetOrAdd(prop.PropertyType, t => t.GetCustomAttribute<SugarColumn>(true));
                            if (sugar?.NoSerialize == true)
                            {
                                prop.ShouldSerialize = static (obj, propInfo) => false;
                            }

                            if (prop.PropertyType == typeof(DateTime) && !string.IsNullOrEmpty(sugar?.SerializeDateTimeFormat))
                            {
                                prop.CustomConverter = new SystemTextJsonDateTimeJsonConverter(sugar.SerializeDateTimeFormat);
                            }

                        }
                    });

            _systemTextJsonSettings.Converters.Add(new JTokenSystemTextJsonConverter());
            _systemTextJsonSettings.Converters.Add(new JValueSystemTextJsonConverter());
            _systemTextJsonSettings.Converters.Add(new JObjectSystemTextJsonConverter());
            _systemTextJsonSettings.Converters.Add(new JArraySystemTextJsonConverter());
            _systemTextJsonSettings.TypeInfoResolver = resolver;
        }
        public static bool UseNewtonsoftJson = false;



        public string SerializeObject(object value)
        {
            if (value == null) return "null";

            switch (value)
            {
                case System.Text.Json.JsonElement element:
                    return System.Text.Json.JsonSerializer.Serialize(element, _systemTextJsonSettings);
                case System.Text.Json.JsonDocument document:
                    return System.Text.Json.JsonSerializer.Serialize(document, _systemTextJsonSettings);
                case System.Text.Json.Nodes.JsonValue valueType:
                    return System.Text.Json.JsonSerializer.Serialize(valueType, _systemTextJsonSettings);
                case JToken jToken:
                    return JsonConvert.SerializeObject(jToken, _newtonsoftSettings);

                default:
                    if (UseNewtonsoftJson)
                        return JsonConvert.SerializeObject(value, _newtonsoftSettings);
                    else
                        return System.Text.Json.JsonSerializer.Serialize(value, value.GetType(), _systemTextJsonSettings);
            }
        }


        private static readonly Type JsonElementType = typeof(System.Text.Json.JsonElement);
        private static readonly Type JsonDocumentType = typeof(System.Text.Json.JsonDocument);
        private static readonly Type JsonValueType = typeof(System.Text.Json.Nodes.JsonValue);
        private static readonly Type JTokenType = typeof(JToken);
        public T DeserializeObject<T>(string value)
        {
            var type = typeof(T);
            if (type == JsonElementType)
                return System.Text.Json.JsonSerializer.Deserialize<T>(value, _systemTextJsonSettings);
            else if (type == JsonDocumentType)
                return System.Text.Json.JsonSerializer.Deserialize<T>(value, _systemTextJsonSettings);
            else if (type == JsonValueType)
                return System.Text.Json.JsonSerializer.Deserialize<T>(value, _systemTextJsonSettings);
            else if (JTokenType.IsAssignableFrom(type))
                return JsonConvert.DeserializeObject<T>(value, _newtonsoftSettings);
            else
            {
                if (UseNewtonsoftJson)
                    return JsonConvert.DeserializeObject<T>(value, _newtonsoftSettings);
                else
                    return System.Text.Json.JsonSerializer.Deserialize<T>(value, _systemTextJsonSettings);
            }
        }
    }

    public class NewtonsoftResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            // 先用基类得到完整属性列表（包含 JsonPropertyAttribute 等配置）
            var props = base.CreateProperties(type, memberSerialization);

            // 1. 忽略 SugarColumn.NoSerialize
            props = props
                .Where(p =>
                {
                    var pi = type.GetProperty(p.UnderlyingName);
                    if (pi == null) return true;
                    var sugarAttr = pi.GetCustomAttributes(typeof(SugarColumn), true)
                                      .OfType<SugarColumn>()
                                      .FirstOrDefault();
                    return sugarAttr == null || sugarAttr.NoSerialize != true;
                })
                .ToList();

            // 2. DateTime 自定义格式
            foreach (var p in props)
            {
                var pi = type.GetProperty(p.UnderlyingName);
                if (pi == null) continue;
                var sugarAttr = pi.GetCustomAttributes(typeof(SugarColumn), true)
                                  .OfType<SugarColumn>()
                                  .FirstOrDefault();
                if (sugarAttr?.SerializeDateTimeFormat?.Length > 0 &&
                    UtilMethods.GetUnderType(pi) == UtilConstants.DateType)
                {
                    p.Converter = new IsoDateTimeConverter { DateTimeFormat = sugarAttr.SerializeDateTimeFormat };
                }
            }

            return props;
        }


    }









    /// <summary>
    /// DateTime 类型序列化
    /// </summary>
    class SystemTextJsonDateTimeJsonConverter : System.Text.Json.Serialization.JsonConverter<DateTime>
    {
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public SystemTextJsonDateTimeJsonConverter()
            : this(default)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="format"></param>
        public SystemTextJsonDateTimeJsonConverter(string format = "yyyy-MM-dd HH:mm:ss")
        {
            Format = format;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="format"></param>
        /// <param name="outputToLocalDateTime"></param>
        public SystemTextJsonDateTimeJsonConverter(string format = "yyyy-MM-dd HH:mm:ss", bool outputToLocalDateTime = false)
            : this(format)
        {
            Localized = outputToLocalDateTime;
        }

        /// <summary>
        /// 时间格式化格式
        /// </summary>
        public string Format { get; private set; }

        /// <summary>
        /// 是否输出为为当地时间
        /// </summary>
        public bool Localized { get; private set; } = false;

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return Penetrates.ConvertToDateTime(ref reader);
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // 判断是否序列化成当地时间
            var formatDateTime = Localized ? value.ToLocalTime() : value;
            writer.WriteStringValue(formatDateTime.ToString(Format));
        }
    }

    /// <summary>
    /// 常量、公共方法配置类
    /// </summary>
    static class Penetrates
    {

        /// <summary>
        /// 将时间戳转换为 DateTime
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        internal static DateTime ConvertToDateTime(this long timestamp)
        {
            var timeStampDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var digitCount = (int)Math.Floor(Math.Log10(timestamp) + 1);

            if (digitCount != 13 && digitCount != 10)
            {
                throw new ArgumentException("Data is not a valid timestamp format.");
            }

            return (digitCount == 13
                ? timeStampDateTime.AddMilliseconds(timestamp)  // 13 位时间戳
                : timeStampDateTime.AddSeconds(timestamp)).ToLocalTime();   // 10 位时间戳
        }
        /// <summary>
        /// 转换
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        internal static DateTime ConvertToDateTime(ref Utf8JsonReader reader)
        {
            // 处理时间戳自动转换
            if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt64(out var longValue))
            {
                return longValue.ConvertToDateTime();
            }

            var stringValue = reader.GetString();

            // 处理时间戳自动转换
            if (long.TryParse(stringValue, out var longValue2))
            {
                return longValue2.ConvertToDateTime();
            }

            return Convert.ToDateTime(stringValue);
        }

        /// <summary>
        /// 转换
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        internal static DateTime ConvertToDateTime(ref JsonReader reader)
        {
            if (reader.TokenType == JsonToken.Integer)
            {
                return JValue.ReadFrom(reader).Value<long>().ConvertToDateTime();
            }

            var stringValue = JValue.ReadFrom(reader).Value<string>();

            // 处理时间戳自动转换
            if (long.TryParse(stringValue, out var longValue2))
            {
                return longValue2.ConvertToDateTime();
            }

            return Convert.ToDateTime(stringValue);
        }
    }
    /// <summary>
    /// DateTime? 类型序列化
    /// </summary>

    class SystemTextJsonNullableDateTimeJsonConverter : System.Text.Json.Serialization.JsonConverter<DateTime?>
    {
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public SystemTextJsonNullableDateTimeJsonConverter()
            : this(default)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="format"></param>
        public SystemTextJsonNullableDateTimeJsonConverter(string format = "yyyy-MM-dd HH:mm:ss")
        {
            Format = format;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="format"></param>
        /// <param name="outputToLocalDateTime"></param>
        public SystemTextJsonNullableDateTimeJsonConverter(string format = "yyyy-MM-dd HH:mm:ss", bool outputToLocalDateTime = false)
            : this(format)
        {
            Localized = outputToLocalDateTime;
        }

        /// <summary>
        /// 时间格式化格式
        /// </summary>
        public string Format { get; private set; }

        /// <summary>
        /// 是否输出为为当地时间
        /// </summary>
        public bool Localized { get; private set; } = false;

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return Penetrates.ConvertToDateTime(ref reader);
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value == null) writer.WriteNullValue();
            else
            {
                // 判断是否序列化成当地时间
                var formatDateTime = Localized ? value.Value.ToLocalTime() : value.Value;
                writer.WriteStringValue(formatDateTime.ToString(Format));
            }
        }
    }

}
