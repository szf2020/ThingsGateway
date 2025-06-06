namespace System.Web.Script.Serialization
{
    /// <summary>忽略Json序列化</summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ScriptIgnoreAttribute : Attribute { }
}