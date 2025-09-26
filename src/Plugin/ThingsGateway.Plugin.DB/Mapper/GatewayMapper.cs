//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Riok.Mapperly.Abstractions;

using ThingsGateway.DB;

namespace ThingsGateway.Plugin.DB;

[Mapper(UseDeepCloning = true, EnumMappingStrategy = EnumMappingStrategy.ByName, RequiredMappingStrategy = RequiredMappingStrategy.None)]
public static partial class GatewayMapper
{
    public static partial List<HistoryAlarm> AdaptListHistoryAlarm(this IEnumerable<AlarmVariable> src);
    public static partial IEnumerable<HistoryAlarm> AdaptEnumerableHistoryAlarm(this IEnumerable<AlarmVariable> src);

    [MapProperty(nameof(AlarmVariable.Id), nameof(HistoryAlarm.Id), Use = nameof(MapId))]
    private static partial HistoryAlarm AdaptHistoryAlarm(AlarmVariable src);

    [UserMapping(Default = false)]
    private static long MapId(long id) => CommonUtils.GetSingleId();
}
