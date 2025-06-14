using System.Text.Json;

namespace ThingsGateway.Admin.Application;

public static class OAuthUserExtensions
{
    public static string TryGetValue(this JsonElement.ObjectEnumerator target, string propertyName)
    {
        return target.FirstOrDefault<JsonProperty>((Func<JsonProperty, bool>)(t => t.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))).Value.ToString() ?? string.Empty;
    }
}
