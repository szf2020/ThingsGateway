//------------------------------------------------------------------------------
//此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using ThingsGateway.Gateway.Application;
using ThingsGateway.NewLife.Json.Extension;
using ThingsGateway.Plugin.DB;
using ThingsGateway.SqlSugar;

using TouchSocket.Core;

public class TestSQL : DynamicSQLBase
{
    public override Task DBInit(ISqlSugarClient db, CancellationToken cancellationToken)
    {
        db.DbMaintenance.CreateDatabase();
        db.CodeFirst.InitTables<ThingsGateway.Plugin.OpcAe.OpcAeEventData>();
        return Task.CompletedTask;
    }

    public override async Task DBInsertable(ISqlSugarClient db, IEnumerable<object> datas, CancellationToken cancellationToken)
    {
        var pluginEventDatas = datas.Cast<PluginEventData>();
        var opcDatas = pluginEventDatas.Select(
            a =>
            {
                if (a.ObjectValue == null)
                {
                    a.ObjectValue = a.Value.ToObject(Type.GetType(a.ValueType));
                }
                return a.ObjectValue is ThingsGateway.Plugin.OpcAe.OpcAeEventData opcData ? opcData : null;
            }
            ).Where(a => a != null).ToList();
        if (opcDatas.Count == 0)
            return;
        Logger?.Info(opcDatas.ToSystemTextJsonString());

        await db.Insertable(opcDatas).ExecuteCommandAsync(cancellationToken).ConfigureAwait(false);
    }
}
