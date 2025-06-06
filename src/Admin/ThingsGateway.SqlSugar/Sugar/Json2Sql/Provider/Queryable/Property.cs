using Newtonsoft.Json.Linq;

namespace SqlSugar
{
    /// <summary>
    /// Property
    /// </summary>
    public partial class JsonQueryableProvider : IJsonQueryableProvider<JsonQueryResult>
    {
        int appendIndex = 1000;
        List<JToken> appendTypeNames;
        JObject jobject;
        ISqlSugarClient context;
        ISugarQueryable<object> sugarQueryable;
        JsonCommonProvider jsonCommonProvider;
        List<JsonTableConfig> jsonTableConfigs = new List<JsonTableConfig>();
        bool IsDescription = false;
        List<JsonQueryableProvider_TableInfo> TableInfos = new List<JsonQueryableProvider_TableInfo>();
        bool IsExecutedBeforeWhereFunc = false;
        Action BeforeWhereFunc { get; set; }
    }
}
