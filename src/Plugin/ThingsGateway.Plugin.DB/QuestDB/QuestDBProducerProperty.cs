//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using System.ComponentModel.DataAnnotations;

using ThingsGateway.SqlSugar;

namespace ThingsGateway.Plugin.DB;

public class QuestDBProducerProperty : RealDBProducerProperty
{
    public override DbType DbType { get; } = DbType.QuestDB;

    [DynamicProperty]
    [Required]
    [AutoGenerateColumn(ComponentType = typeof(Textarea), Rows = 1)]
    public override string BigTextConnectStr { get; set; } = "host=localhost;port=8812;username=admin;password=quest;database=qdb;ServerCompatibilityMode=NoTypeLoading;";

    [DynamicProperty]
    public bool BulkCopy { get; set; } = true;

    [DynamicProperty]
    public int HttpPort { get; set; } = 9000;

}
