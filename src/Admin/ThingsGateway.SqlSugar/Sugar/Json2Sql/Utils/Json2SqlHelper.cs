using Newtonsoft.Json.Linq;

using System.Text.RegularExpressions;
namespace ThingsGateway.SqlSugar
{
    internal static class Json2SqlHelper
    {
        public static bool IsSqlValue(string valueString)
        {
            return Regex.IsMatch(valueString, @"^\{\w{1,10}\}\:");
        }
        public static string GetType(string valueString)
        {
            return Regex.Match(valueString, @"^\{(\w+)\}\:").Groups[1].Value;
        }
        public static string GetValue(string valueString)
        {
            return Regex.Replace(valueString, @"^\{\w{1,10}\}\:", "");
        }

        public static List<string> GetTableNames(string json)
        {
            List<string> result = new List<string>();
            var mainTable = JObject.Parse(json).AsJEnumerable().Where(it =>
              it.Path.IsInCase(
                  JsonProviderConfig.KeyInsertable.Get(),
                  JsonProviderConfig.KeyUpdateable.Get(),
                  JsonProviderConfig.KeyDeleteable.Get(),
                  JsonProviderConfig.KeyQueryable.Get()
              )).FirstOrDefault();
            if (mainTable != null)
                result.Add(mainTable.First().ToString());
            return result;
        }
    }
}
