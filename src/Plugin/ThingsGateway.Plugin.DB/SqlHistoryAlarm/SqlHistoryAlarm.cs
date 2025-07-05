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

using ThingsGateway.Admin.Application;
using ThingsGateway.Foundation;
using ThingsGateway.NewLife;
using ThingsGateway.SqlSugar;

namespace ThingsGateway.Plugin.SqlHistoryAlarm;

/// <summary>
/// SqlHistoryAlarm
/// </summary>
public partial class SqlHistoryAlarm : BusinessBaseWithCacheAlarm, IDBHistoryAlarmService
{

    internal readonly SqlHistoryAlarmProperty _driverPropertys = new();
    private readonly SqlHistoryAlarmVariableProperty _variablePropertys = new();
    public override Type DriverUIType => typeof(HistoryAlarmPage);

    /// <inheritdoc/>
    public override VariablePropertyBase VariablePropertys => _variablePropertys;

    protected override BusinessPropertyWithCache _businessPropertyWithCache => _driverPropertys;

    private SqlSugarClient _db;

    protected override async Task InitChannelAsync(IChannel? channel, CancellationToken cancellationToken)
    {
        _db = BusinessDatabaseUtil.GetDb((DbType)_driverPropertys.DbType, _driverPropertys.BigTextConnectStr);

        await base.InitChannelAsync(channel, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    public override bool IsConnected() => success;

    protected override void Dispose(bool disposing)
    {
        _db?.TryDispose();
        base.Dispose(disposing);
    }

    protected override Task ProtectedStartAsync(CancellationToken cancellationToken)
    {
        _db.DbMaintenance.CreateDatabase();
        _db.CodeFirst.As<HistoryAlarm>(_driverPropertys.TableName).InitTables<HistoryAlarm>();
        return base.ProtectedStartAsync(cancellationToken);
    }

    #region 数据查询

    public async Task<SqlSugarPagedList<IDBHistoryAlarm>> GetDBHistoryAlarmPagesAsync(DBHistoryAlarmPageInput input)
    {
        var data = await Query(input).ToPagedListAsync<HistoryAlarm, IDBHistoryAlarm>(input.Current, input.Size).ConfigureAwait(false);//分页
        return data;
    }

    public async Task<List<IDBHistoryAlarm>> GetDBHistoryAlarmsAsync(DBHistoryAlarmPageInput input)
    {
        var data = await Query(input).ToListAsync().ConfigureAwait(false);
        return data.Cast<IDBHistoryAlarm>().ToList();
    }

    internal ISugarQueryable<HistoryAlarm> Query(DBHistoryAlarmPageInput input)
    {
        using var db = BusinessDatabaseUtil.GetDb((DbType)_driverPropertys.DbType, _driverPropertys.BigTextConnectStr);
        var query = db.Queryable<HistoryAlarm>().AS(_driverPropertys.TableName)
                             .WhereIF(input.StartTime != null, a => a.EventTime >= input.StartTime)
                           .WhereIF(input.EndTime != null, a => a.EventTime <= input.EndTime)
                           .WhereIF(!string.IsNullOrEmpty(input.VariableName), it => it.Name.Contains(input.VariableName))
                           .WhereIF(input.AlarmType != null, a => a.AlarmType == input.AlarmType)
                           .WhereIF(input.EventType != null, a => a.EventType == input.EventType)
                           ;

        for (int i = input.SortField.Count - 1; i >= 0; i--)
        {
            query = query.OrderByIF(!string.IsNullOrEmpty(input.SortField[i]), $"{input.SortField[i]} {(input.SortDesc[i] ? "desc" : "asc")}");
        }
        query = query.OrderBy(it => it.Id, OrderByType.Desc);//排序

        return query;
    }

    internal async Task<QueryData<HistoryAlarm>> QueryData(QueryPageOptions option)
    {
        using var db = BusinessDatabaseUtil.GetDb((DbType)_driverPropertys.DbType, _driverPropertys.BigTextConnectStr);
        var ret = new QueryData<HistoryAlarm>()
        {
            IsSorted = option.SortOrder != SortOrder.Unset,
            IsFiltered = option.Filters.Count > 0,
            IsAdvanceSearch = option.AdvanceSearches.Count > 0 || option.CustomerSearches.Count > 0,
            IsSearch = option.Searches.Count > 0
        };

        var query = db.Queryable<HistoryAlarm>().AS(_driverPropertys.TableName);
        query = db.GetQuery<HistoryAlarm>(option, query);

        if (option.IsPage)
        {
            RefAsync<int> totalCount = 0;

            var items = await query.ToPageListAsync(option.PageIndex, option.PageItems, totalCount).ConfigureAwait(false);

            ret.TotalCount = totalCount;
            ret.Items = items;
        }
        else if (option.IsVirtualScroll)
        {
            RefAsync<int> totalCount = 0;

            var items = await query.ToPageListAsync(option.StartIndex, option.PageItems, totalCount).ConfigureAwait(false);

            ret.TotalCount = totalCount;
            ret.Items = items;
        }
        else
        {
            var items = await query.ToListAsync().ConfigureAwait(false);
            ret.TotalCount = items.Count;
            ret.Items = items;
        }
        return ret;
    }

    #endregion 数据查询
}
