using System.Text;
namespace ThingsGateway.SqlSugar
{
    internal static partial class ErrorMessage
    {
        internal static LanguageType SugarLanguageType { get; set; } = LanguageType.Chinese;

        internal static CompositeFormat ObjNotExistCompositeFormat => SugarLanguageType == LanguageType.English ? privateEObjNotExistCompositeFormat : privateCObjNotExistCompositeFormat;
        private static readonly CompositeFormat privateEObjNotExistCompositeFormat = CompositeFormat.Parse("{0} does not exist.");
        private static readonly CompositeFormat privateCObjNotExistCompositeFormat = CompositeFormat.Parse("{0}不存在。");

        internal const string EntityMappingErrorCompositeFormat = "Select entity and table mapping error, you can annotate the fields in the entity class to investigate which specific field. [Note: If CodeFirt is used, configure to prohibit column deletion or updates first] . ";

        internal const string NotSupportedDictionaryCompositeFormat = "This type of Dictionary is not supported for the time being. You can try Dictionary<string, string>";

        internal const string NotSupportedArrayCompositeFormat = "This type of Array is not supported for the time being. You can try object[]";

        internal static string GetThrowMessage(string enMessage, string cnMessage, params string[] args)
        {
            if (SugarLanguageType == LanguageType.Default)
            {
                List<string> formatArgs = new List<string>() { enMessage, cnMessage };
                formatArgs.AddRange(args);
                return string.Format(@"中文提示 : {1}
English Message : {0}", formatArgs.ToArray());
            }
            else if (SugarLanguageType == LanguageType.English)
            {
                return enMessage;
            }
            else
            {
                return cnMessage;
            }
        }
    }
}
