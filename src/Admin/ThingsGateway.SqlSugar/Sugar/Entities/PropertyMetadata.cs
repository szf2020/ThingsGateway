using System.Reflection.Emit;

namespace ThingsGateway.SqlSugar
{
    public class PropertyMetadata
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public List<CustomAttributeBuilder> CustomAttributes { get; set; }
    }
}
