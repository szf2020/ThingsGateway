//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Newtonsoft.Json.Linq;

using ThingsGateway.NewLife.Extension;
using ThingsGateway.Plugin.QuestDB;
using ThingsGateway.Plugin.SqlDB;

namespace ThingsGateway.Plugin.DB;

internal static class Helper
{
    #region
    public static SQLHistoryValue AdaptSQLHistoryValue(this VariableRuntime src)
    {
        var dest = new SQLHistoryValue();
        dest.Id = src.Id;
        dest.Value = GetValue(src.Value);
        dest.CreateTime = DateTime.Now;

        dest.CollectTime = src.CollectTime;
        dest.DeviceName = src.DeviceName;
        dest.IsOnline = src.IsOnline;
        dest.Name = src.Name;
        return dest;
    }
    public static List<SQLHistoryValue> AdaptListSQLHistoryValue(this IEnumerable<VariableRuntime> src)
    {
        return Enumerable.ToList(
                Enumerable.Select(src, x => AdaptSQLHistoryValue(x))
            );
    }


    public static SQLHistoryValue AdaptSQLHistoryValue(this VariableBasicData src)
    {


        var dest = new SQLHistoryValue();
        dest.Id = src.Id;
        dest.Value = GetValue(src.Value);
        dest.CreateTime = DateTime.Now;

        dest.CollectTime = src.CollectTime;
        dest.DeviceName = src.DeviceName;
        dest.IsOnline = src.IsOnline;
        dest.Name = src.Name;
        return dest;
    }
    public static List<SQLHistoryValue> AdaptListSQLHistoryValue(this IEnumerable<VariableBasicData> src)
    {
        return Enumerable.ToList(
                Enumerable.Select(src, x => AdaptSQLHistoryValue(x))
            );
    }


    public static SQLNumberHistoryValue AdaptSQLNumberHistoryValue(this VariableRuntime src)
    {
        var dest = new SQLNumberHistoryValue();
        dest.Id = src.Id;
        dest.Value = src.Value.GetType() == typeof(bool) ? ConvertUtility.Convert.ToBoolean(src.Value, false) ? 1 : 0 : ConvertUtility.Convert.ToDecimal(src.Value, 0);
        dest.CreateTime = DateTime.Now;

        dest.CollectTime = src.CollectTime;
        dest.DeviceName = src.DeviceName;
        dest.IsOnline = src.IsOnline;
        dest.Name = src.Name;
        return dest;
    }
    public static List<SQLNumberHistoryValue> AdaptListSQLNumberHistoryValue(this IEnumerable<VariableRuntime> src)
    {
        return Enumerable.ToList(
                Enumerable.Select(src, x => AdaptSQLNumberHistoryValue(x))
            );
    }


    public static SQLNumberHistoryValue AdaptSQLNumberHistoryValue(this VariableBasicData src)
    {


        var dest = new SQLNumberHistoryValue();
        dest.Id = src.Id;
        dest.Value = src.Value.GetType() == typeof(bool) ? ConvertUtility.Convert.ToBoolean(src.Value, false) ? 1 : 0 : ConvertUtility.Convert.ToDecimal(src.Value, 0);
        dest.CreateTime = DateTime.Now;

        dest.CollectTime = src.CollectTime;
        dest.DeviceName = src.DeviceName;
        dest.IsOnline = src.IsOnline;
        dest.Name = src.Name;
        return dest;
    }
    public static List<SQLNumberHistoryValue> AdaptListSQLNumberHistoryValue(this IEnumerable<VariableBasicData> src)
    {
        return Enumerable.ToList(
                Enumerable.Select(src, x => AdaptSQLNumberHistoryValue(x))
            );
    }



    public static SQLRealValue AdaptSQLRealValue(this VariableBasicData src)
    {
        var dest = new SQLRealValue();
        dest.Id = src.Id;
        dest.Value = GetValue(src.Value);
        dest.CollectTime = src.CollectTime;
        dest.DeviceName = src.DeviceName;
        dest.IsOnline = src.IsOnline;
        dest.Name = src.Name;
        return dest;
    }

    public static List<SQLRealValue> AdaptListSQLRealValue(this IEnumerable<VariableBasicData> src)
    {
        return Enumerable.ToList(
                Enumerable.Select(src, x => AdaptSQLRealValue(x))
            );
    }

    public static SQLRealValue AdaptSQLRealValue(this VariableRuntime src)
    {
        var dest = new SQLRealValue();
        dest.Id = src.Id;
        dest.Value = GetValue(src.Value);
        dest.CollectTime = src.CollectTime;
        dest.DeviceName = src.DeviceName;
        dest.IsOnline = src.IsOnline;
        dest.Name = src.Name;
        return dest;
    }

    public static List<SQLRealValue> AdaptListSQLRealValue(this IEnumerable<VariableRuntime> src)
    {
        return Enumerable.ToList(
                Enumerable.Select(src, x => AdaptSQLRealValue(x))
            );
    }

    #endregion

    #region
    public static QuestDBHistoryValue AdaptQuestDBHistoryValue(this VariableRuntime src)
    {
        var dest = new QuestDBHistoryValue();
        dest.Id = src.Id;
        dest.Value = GetValue(src.Value);
        dest.CreateTime = DateTime.UtcNow;

        dest.CollectTime = src.CollectTime < DateTime.MinValue ? UtcTime1970 : src.CollectTime;
        dest.DeviceName = src.DeviceName;
        dest.IsOnline = src.IsOnline;
        dest.Name = src.Name;
        return dest;
    }
    public static List<QuestDBHistoryValue> AdaptListQuestDBHistoryValue(this IEnumerable<VariableRuntime> src)
    {
        return Enumerable.ToList(
                Enumerable.Select(src, x => AdaptQuestDBHistoryValue(x))
            );
    }


    public static QuestDBHistoryValue AdaptQuestDBHistoryValue(this VariableBasicData src)
    {


        var dest = new QuestDBHistoryValue();
        dest.Id = src.Id;
        dest.Value = GetValue(src.Value);
        dest.CreateTime = DateTime.UtcNow;

        dest.CollectTime = src.CollectTime < DateTime.MinValue ? UtcTime1970 : src.CollectTime;
        dest.DeviceName = src.DeviceName;
        dest.IsOnline = src.IsOnline;
        dest.Name = src.Name;
        return dest;
    }
    public static List<QuestDBHistoryValue> AdaptListQuestDBHistoryValue(this IEnumerable<VariableBasicData> src)
    {
        return Enumerable.ToList(
                Enumerable.Select(src, x => AdaptQuestDBHistoryValue(x))
            );
    }


    public static QuestDBNumberHistoryValue AdaptQuestDBNumberHistoryValue(this VariableRuntime src)
    {
        var dest = new QuestDBNumberHistoryValue();
        dest.Id = src.Id;
        dest.Value = src.Value.GetType() == typeof(bool) ? ConvertUtility.Convert.ToBoolean(src.Value, false) ? 1 : 0 : ConvertUtility.Convert.ToDecimal(src.Value, 0);
        dest.CreateTime = DateTime.UtcNow;

        dest.CollectTime = src.CollectTime < DateTime.MinValue ? UtcTime1970 : src.CollectTime;
        dest.DeviceName = src.DeviceName;
        dest.IsOnline = src.IsOnline;
        dest.Name = src.Name;
        return dest;
    }
    public static List<QuestDBNumberHistoryValue> AdaptListQuestDBNumberHistoryValue(this IEnumerable<VariableRuntime> src)
    {
        return Enumerable.ToList(
                Enumerable.Select(src, x => AdaptQuestDBNumberHistoryValue(x))
            );
    }


    public static QuestDBNumberHistoryValue AdaptQuestDBNumberHistoryValue(this VariableBasicData src)
    {


        var dest = new QuestDBNumberHistoryValue();
        dest.Id = src.Id;
        dest.Value = src.Value.GetType() == typeof(bool) ? ConvertUtility.Convert.ToBoolean(src.Value, false) ? 1 : 0 : ConvertUtility.Convert.ToDecimal(src.Value, 0);
        dest.CreateTime = DateTime.UtcNow;

        dest.CollectTime = src.CollectTime < DateTime.MinValue ? UtcTime1970 : src.CollectTime;
        dest.DeviceName = src.DeviceName;
        dest.IsOnline = src.IsOnline;
        dest.Name = src.Name;
        return dest;
    }
    public static List<QuestDBNumberHistoryValue> AdaptListQuestDBNumberHistoryValue(this IEnumerable<VariableBasicData> src)
    {
        return Enumerable.ToList(
                Enumerable.Select(src, x => AdaptQuestDBNumberHistoryValue(x))
            );
    }



    static DateTime UtcTime1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);


    #endregion





    private static string GetValue(object src)
    {
        if (src != null)
        {
            if (src is string strValue)
            {
                return strValue;
            }
            else if (src is bool boolValue)
            {
                return boolValue ? "1" : "0";
            }
            else
            {
                return JToken.FromObject(src).ToString();
            }
        }
        else
        {
            return string.Empty;
        }
    }


}
