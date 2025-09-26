using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using JsonProperty = Newtonsoft.Json.Serialization.JsonProperty;

namespace ThingsGateway.SqlSugar
{
    public class SerializeService : ISerializeService
    {
        private static readonly JsonSerializerSettings _newtonsoftSettings = new()
        {
            NullValueHandling = NullValueHandling.Ignore,

            ContractResolver = new MyContractResolver()
        };

        public string SerializeObject(object value)
        {
            if (value == null) return "null";

            // 精准类型检查（避免反射）
            return value switch
            {
                System.Text.Json.JsonElement element => System.Text.Json.JsonSerializer.Serialize(element),
                System.Text.Json.JsonDocument document => System.Text.Json.JsonSerializer.Serialize(document),
                System.Text.Json.Nodes.JsonValue valueType => System.Text.Json.JsonSerializer.Serialize(valueType),
                _ => JsonConvert.SerializeObject(value, _newtonsoftSettings)
            };
        }

        public string SugarSerializeObject(object value)
        {
            return JsonConvert.SerializeObject(value, _newtonsoftSettings);
        }
        private static readonly Type JsonElementType = typeof(System.Text.Json.JsonElement);
        private static readonly Type JsonDocumentType = typeof(System.Text.Json.JsonDocument);
        private static readonly Type JsonValueType = typeof(System.Text.Json.Nodes.JsonValue);
        public T DeserializeObject<T>(string value)
        {
            var type = typeof(T);
            if (type == JsonElementType)
                return System.Text.Json.JsonSerializer.Deserialize<T>(value);
            else if (type == JsonDocumentType)
                return System.Text.Json.JsonSerializer.Deserialize<T>(value);
            else if (type == JsonValueType)
                return System.Text.Json.JsonSerializer.Deserialize<T>(value);
            else
                return JsonConvert.DeserializeObject<T>(value, _newtonsoftSettings);
        }
    }

    public class MyContractResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
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
}
