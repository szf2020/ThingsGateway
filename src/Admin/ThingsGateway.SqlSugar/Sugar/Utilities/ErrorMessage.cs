using System.Text;
namespace ThingsGateway.SqlSugar
{
    internal static partial class ErrorMessage
    {
        internal static LanguageType SugarLanguageType { get; set; } = LanguageType.Chinese;

        internal static CompositeFormat ObjNotExistCompositeFormat => SugarLanguageType == LanguageType.English ? privateEObjNotExistCompositeFormat : privateCObjNotExistCompositeFormat;
        private static readonly CompositeFormat privateEObjNotExistCompositeFormat = CompositeFormat.Parse("{0} does not exist.");
        private static readonly CompositeFormat privateCObjNotExistCompositeFormat = CompositeFormat.Parse("{0}不存在。");

        internal static CompositeFormat EntityMappingErrorCompositeFormat => SugarLanguageType == LanguageType.English ? privateEEntityMappingErrorCompositeFormat : privateCEntityMappingErrorCompositeFormat;
        private static readonly CompositeFormat privateEEntityMappingErrorCompositeFormat = CompositeFormat.Parse("Entity mapping error.{0}");
        private static readonly CompositeFormat privateCEntityMappingErrorCompositeFormat = CompositeFormat.Parse("Select 实体与表映射出错,可以注释实体类中的字段排查具体哪一个字段。【注意：如果用CodeFirt先配置禁止删列或更新】 {0}");

        internal static CompositeFormat NotSupportedDictionaryCompositeFormat => SugarLanguageType == LanguageType.English ? privateENotSupportedDictionaryCompositeFormat : privateCNotSupportedDictionaryCompositeFormat;
        private static readonly CompositeFormat privateENotSupportedDictionaryCompositeFormat = CompositeFormat.Parse("This type of Dictionary is not supported for the time being. You can try Dictionary<string, string>, or contact the author!");
        private static readonly CompositeFormat privateCNotSupportedDictionaryCompositeFormat = CompositeFormat.Parse("暂时不支持该类型的Dictionary 你可以试试 Dictionary<string ,string>或者联系作者！");

        internal static CompositeFormat NotSupportedArrayCompositeFormat => SugarLanguageType == LanguageType.English ? privateENotSupportedArrayCompositeFormat : privateCNotSupportedArrayCompositeFormat;
        private static readonly CompositeFormat privateENotSupportedArrayCompositeFormat = CompositeFormat.Parse("This type of Array is not supported for the time being. You can try object[] or contact the author!");
        private static readonly CompositeFormat privateCNotSupportedArrayCompositeFormat = CompositeFormat.Parse("暂时不支持该类型的Array 你可以试试 object[] 或者联系作者！");

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
