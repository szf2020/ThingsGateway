using Newtonsoft.Json;

namespace SqlSugar
{
    public class ValueToStringConverter : JsonConverter
    {
        public override bool CanRead => false;

        public override bool CanConvert(Type objectType) => objectType.IsValueType;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            => throw new NotSupportedException();

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var str = value?.ToString();
            writer.WriteValue(str);
        }
    }
}
