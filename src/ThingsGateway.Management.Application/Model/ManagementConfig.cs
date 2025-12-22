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

using Microsoft.Extensions.Logging;

using Riok.Mapperly.Abstractions;

using System.ComponentModel.DataAnnotations;

namespace ThingsGateway.Management.Application;

[OrmTable("remote_management_config", TableDescription = "远程管理配置")]
[OrmIndex("unique_management_name", nameof(ManagementConfig.Name), OrderByType.Asc, true)]
[Tenant(SqlOrmConst.DB_Custom)]
public partial class ManagementConfig : BaseDataEntity, IAsyncDisposable
{

    [OrmColumn(ColumnDescription = "名称", IsNullable = false)]
    [AutoGenerateColumn(Visible = true, DefaultSort = false, Filterable = true, Sortable = true, DefaultSortOrder = SortOrder.Desc)]
    [Required]
    public string Name { get; set; }


    [OrmColumn(ColumnDescription = "IP地址", IsNullable = false)]
    [AutoGenerateColumn(Visible = true, DefaultSort = false, Filterable = true, Sortable = true, DefaultSortOrder = SortOrder.Desc)]
    [Required]
    [UriValidation]
    public string ServerUri { get; set; } = "127.0.0.1:8399";

    [OrmColumn(ColumnDescription = "启用", IsNullable = false)]
    [AutoGenerateColumn(Visible = true, DefaultSort = false, Filterable = true, Sortable = true, DefaultSortOrder = SortOrder.Desc)]
    public bool Enable { get; set; } = true;

    [OrmColumn(ColumnDescription = "Tcp服务", IsNullable = false)]
    [AutoGenerateColumn(Visible = true, DefaultSort = false, Filterable = true, Sortable = true, DefaultSortOrder = SortOrder.Desc)]
    public bool IsServer { get; set; }


    [OrmColumn(ColumnDescription = "验证标识", IsNullable = false)]
    [AutoGenerateColumn(Visible = true, DefaultSort = false, Filterable = true, Sortable = true, DefaultSortOrder = SortOrder.Desc)]
    [Required]
    public string VerifyToken { get; set; } = "ThingsGateway";

    [OrmColumn(ColumnDescription = "心跳间隔", IsNullable = false)]
    [AutoGenerateColumn(Visible = true, DefaultSort = false, Filterable = true, Sortable = true, DefaultSortOrder = SortOrder.Desc)]
    [MinValue(3000)]
    public int HeartbeatInterval { get; set; } = 3000;

    [MapperIgnore]
    internal bool IsUp;

    [MapperIgnore]
    public ManagementTask ManagementTask;

    public async Task InitAsync()
    {


        var task = ManagementTask;
        ManagementTask = null;
        ManagementTask = new ManagementTask(App.GetService<ILoggerFactory>().CreateLogger($"nameof(ManagementTask)-{Name}"), this);
        if (task != null)
        {
            await task.DisposeAsync().ConfigureAwait(false);
        }


        ManagementGlobalData.ManagementConfigs.TryRemove(Name, out _);
        ManagementGlobalData.IdManagementConfigs.TryRemove(Id, out _);
        ManagementGlobalData.ManagementConfigs.TryAdd(Name, this);
        ManagementGlobalData.IdManagementConfigs.TryAdd(Id, this);
    }

    public async ValueTask DisposeAsync()
    {

        if (ManagementTask != null)
        {
            await ManagementTask.DisposeAsync().ConfigureAwait(false);
            ManagementTask = null;
        }

        ManagementGlobalData.ManagementConfigs.TryRemove(Name, out _);
        ManagementGlobalData.IdManagementConfigs.TryRemove(Id, out _);
    }
}
