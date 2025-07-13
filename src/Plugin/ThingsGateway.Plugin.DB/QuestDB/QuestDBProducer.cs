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

using ThingsGateway.Common;
using ThingsGateway.DB;
using ThingsGateway.Debug;
using ThingsGateway.Foundation;
using ThingsGateway.NewLife;
using ThingsGateway.NewLife.Extension;
using ThingsGateway.Plugin.DB;
using ThingsGateway.SqlSugar;

namespace ThingsGateway.Plugin.QuestDB;

/// <summary>
/// QuestDBProducer
/// </summary>
public partial class QuestDBProducer : BusinessBaseWithCacheIntervalVariable, IDBHistoryValueService
{


    internal readonly RealDBProducerProperty _driverPropertys = new();
    private readonly QuestDBProducerVariableProperty _variablePropertys = new();

    /// <inheritdoc/>
    public override Type DriverPropertyUIType => typeof(SqlDBProducerPropertyRazor);

    /// <inheritdoc/>
    public override Type DriverUIType
    {
        get
        {
            if (_driverPropertys.BigTextScriptHistoryTable.IsNullOrEmpty())
                return typeof(QuestDBPage);
            else
                return null;
        }
    }
    public override VariablePropertyBase VariablePropertys => _variablePropertys;

    protected override BusinessPropertyWithCacheInterval _businessPropertyWithCacheInterval => _driverPropertys;



    private SqlSugarClient _db;

    protected override void Dispose(bool disposing)
    {
        _db?.TryDispose();
        base.Dispose(disposing);
    }


    protected override async Task InitChannelAsync(IChannel? channel, CancellationToken cancellationToken)
    {
        _db = BusinessDatabaseUtil.GetDb(_driverPropertys.DbType, _driverPropertys.BigTextConnectStr);

        await base.InitChannelAsync(channel, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override bool IsConnected() => success;

    /// <inheritdoc/>
    public override string ToString()
    {
        return $" {nameof(QuestDBProducer)}";
    }

    protected override async Task ProtectedStartAsync(CancellationToken cancellationToken)
    {

        _db.DbMaintenance.CreateDatabase();

        //必须为间隔上传
        if (!_driverPropertys.BigTextScriptHistoryTable.IsNullOrEmpty())
        {
            DynamicSQLBase? hisModel = CSharpScriptEngineExtension.Do<DynamicSQLBase>(_driverPropertys.BigTextScriptHistoryTable);
            hisModel.Logger = LogMessage;
            await hisModel.DBInit(_db, cancellationToken).ConfigureAwait(false);

        }
        else
        {
            _db.CodeFirst.As<QuestDBNumberHistoryValue>(_driverPropertys.NumberTableName).InitTables(typeof(QuestDBNumberHistoryValue));
            _db.CodeFirst.As<QuestDBHistoryValue>(_driverPropertys.StringTableName).InitTables(typeof(QuestDBHistoryValue));
        }

        await base.ProtectedStartAsync(cancellationToken).ConfigureAwait(false);
    }


    public async Task<SqlSugarPagedList<IDBHistoryValue>> GetDBHistoryValuePagesAsync(DBHistoryValuePageInput input)
    {
        var data = await Query(input).ToPagedListAsync<QuestDBNumberHistoryValue, IDBHistoryValue>(input.Current, input.Size).ConfigureAwait(false);//分页
        return data;
    }

    public async Task<List<IDBHistoryValue>> GetDBHistoryValuesAsync(DBHistoryValuePageInput input)
    {
        var data = await Query(input).ToListAsync().ConfigureAwait(false);
        return data.Cast<IDBHistoryValue>().ToList();
    }
    internal ISugarQueryable<QuestDBNumberHistoryValue> Query(DBHistoryValuePageInput input)
    {
        var db = BusinessDatabaseUtil.GetDb(_driverPropertys.DbType, _driverPropertys.BigTextConnectStr);
        var query = db.Queryable<QuestDBNumberHistoryValue>().AS(_driverPropertys.NumberTableName)
                             .WhereIF(input.StartTime != null, a => a.CreateTime >= input.StartTime)
                           .WhereIF(input.EndTime != null, a => a.CreateTime <= input.EndTime)
                           .WhereIF(!string.IsNullOrEmpty(input.VariableName), it => it.Name.Contains(input.VariableName))
                           .WhereIF(input.VariableNames != null, it => input.VariableNames.Contains(it.Name))
                           ;

        for (int i = input.SortField.Count - 1; i >= 0; i--)
        {
            query = query.OrderByIF(!string.IsNullOrEmpty(input.SortField[i]), $"{input.SortField[i]} {(input.SortDesc[i] ? "desc" : "asc")}");
        }
        query = query.OrderBy(it => it.Id, OrderByType.Desc);//排序

        return query;
    }

    internal async Task<QueryData<QuestDBNumberHistoryValue>> QueryData(QueryPageOptions option)
    {
        using var db = BusinessDatabaseUtil.GetDb(_driverPropertys.DbType, _driverPropertys.BigTextConnectStr);
        var ret = new QueryData<QuestDBNumberHistoryValue>()
        {
            IsSorted = option.SortOrder != SortOrder.Unset,
            IsFiltered = option.Filters.Count > 0,
            IsAdvanceSearch = option.AdvanceSearches.Count > 0 || option.CustomerSearches.Count > 0,
            IsSearch = option.Searches.Count > 0
        };
        var query = db.Queryable<QuestDBNumberHistoryValue>()
            .AS(_driverPropertys.NumberTableName);

        query = db.GetQuery<QuestDBNumberHistoryValue>(option, query);

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

}
