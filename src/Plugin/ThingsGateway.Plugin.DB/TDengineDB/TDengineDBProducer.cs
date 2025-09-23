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

using System.Reflection;

using ThingsGateway.Common;
using ThingsGateway.DB;
using ThingsGateway.Debug;
using ThingsGateway.Foundation;
using ThingsGateway.NewLife;
using ThingsGateway.NewLife.Extension;
using ThingsGateway.Plugin.DB;
using ThingsGateway.SqlSugar;

namespace ThingsGateway.Plugin.TDengineDB;

/// <summary>
/// TDengineDBProducer
/// </summary>
public partial class TDengineDBProducer : BusinessBaseWithCacheIntervalVariable
#if !Management
    , IDBHistoryValueService
#endif
{

    internal readonly TDengineDBProducerProperty _driverPropertys = new();
    private readonly TDengineDBProducerVariableProperty _variablePropertys = new();
    /// <inheritdoc/>
    public override Type DriverPropertyUIType => typeof(SqlDBProducerPropertyRazor);

    /// <inheritdoc/>
    public override Type DriverUIType
    {
        get
        {
#if !Management
            if (_driverPropertys.BigTextScriptHistoryTable.IsNullOrEmpty())
                return typeof(TDengineDBPage);
            else
                return null;
#else
            return null;
#endif

        }
    }


    public override VariablePropertyBase VariablePropertys => _variablePropertys;

    protected override BusinessPropertyWithCacheInterval _businessPropertyWithCacheInterval => _driverPropertys;

    protected override Task DisposeAsync(bool disposing)
    {
        _db?.TryDispose();
        return base.DisposeAsync(disposing);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $" {nameof(TDengineDBProducer)}";
    }
    private SqlSugarClient _db;

#if !Management

    protected override async Task InitChannelAsync(IChannel? channel, CancellationToken cancellationToken)
    {
        InstanceFactory.RemoveCache();
        _db = TDengineDBUtil.GetDb(_driverPropertys.DbType, _driverPropertys.BigTextConnectStr, _driverPropertys.NumberTableNameLow);
        List<Assembly> assemblies = new();
        foreach (var item in InstanceFactory.CustomAssemblies)
        {
            if (item.FullName != typeof(TDengineProvider).Assembly.FullName)
            {
                assemblies.Add(item);
            }
        }
        assemblies.Add(typeof(TDengineProvider).Assembly);
        InstanceFactory.CustomAssemblies = assemblies.ToArray();

        await base.InitChannelAsync(channel, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override bool IsConnected() => success;




    protected override async Task ProtectedStartAsync(CancellationToken cancellationToken)
    {
        _db.DbMaintenance.CreateDatabase();

        if (!_driverPropertys.BigTextScriptHistoryTable.IsNullOrEmpty())
        {
            var hisModel = CSharpScriptEngineExtension.Do<DynamicSQLBase>(_driverPropertys.BigTextScriptHistoryTable);
            {
                await hisModel.DBInit(_db, cancellationToken).ConfigureAwait(false);
            }
        }
        else
        {
            var sql = $"""
                CREATE STABLE IF NOT EXISTS  `{_driverPropertys.StringTableNameLow}`(
                `createtime` TIMESTAMP   ,
                `collecttime` TIMESTAMP   ,
                `id` BIGINT   ,
                `isonline` BOOL   ,
                `value` VARCHAR(255)    ) TAGS(`devicename`  VARCHAR(100) ,`name`  VARCHAR(100))
                """;
            await _db.Ado.ExecuteCommandAsync(sql, default, cancellationToken: cancellationToken).ConfigureAwait(false);

            sql = $"""
                CREATE STABLE IF NOT EXISTS  `{_driverPropertys.NumberTableNameLow}`(
                `createtime` TIMESTAMP   ,
                `collecttime` TIMESTAMP   ,
                `id` BIGINT   ,
                `isonline` BOOL   ,
                `value` DOUBLE    ) TAGS(`devicename`  VARCHAR(100) ,`name`  VARCHAR(100))
                """;
            await _db.Ado.ExecuteCommandAsync(sql, default, cancellationToken: cancellationToken).ConfigureAwait(false);



            await _db.Ado.ExecuteCommandAsync($"ALTER STABLE `{_driverPropertys.StringTableNameLow}` KEEP 10;", default, cancellationToken: cancellationToken).ConfigureAwait(false);
            await _db.Ado.ExecuteCommandAsync($"ALTER STABLE `{_driverPropertys.NumberTableNameLow}` KEEP 10;", default, cancellationToken: cancellationToken).ConfigureAwait(false);

        }
        await base.ProtectedStartAsync(cancellationToken).ConfigureAwait(false);
    }
    public async Task<SqlSugarPagedList<IDBHistoryValue>> GetDBHistoryValuePagesAsync(DBHistoryValuePageInput input)
    {
        var data = await Query(input, _driverPropertys.NumberTableNameLow).ToPagedListAsync<TDengineDBNumberHistoryValue, IDBHistoryValue>(input.Current, input.Size).ConfigureAwait(false);//分页
        return data;
    }

    public async Task<List<IDBHistoryValue>> GetDBHistoryValuesAsync(DBHistoryValuePageInput input)
    {
        var data = await Query(input, _driverPropertys.NumberTableNameLow).ToListAsync().ConfigureAwait(false);
        return data.Cast<IDBHistoryValue>().ToList();
    }
    internal ISugarQueryable<TDengineDBNumberHistoryValue> Query(DBHistoryValuePageInput input, string tableName)
    {
        var db = TDengineDBUtil.GetDb(_driverPropertys.DbType, _driverPropertys.BigTextConnectStr, tableName);
        var query = db.Queryable<TDengineDBNumberHistoryValue>().AsTDengineSTable()
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

    internal async Task<QueryData<TDengineDBNumberHistoryValue>> QueryData(QueryPageOptions option)
    {
        var db = TDengineDBUtil.GetDb(_driverPropertys.DbType, _driverPropertys.BigTextConnectStr, _driverPropertys.NumberTableNameLow);
        var ret = new QueryData<TDengineDBNumberHistoryValue>()
        {
            IsSorted = option.SortOrder != SortOrder.Unset,
            IsFiltered = option.Filters.Count > 0,
            IsAdvanceSearch = option.AdvanceSearches.Count > 0 || option.CustomerSearches.Count > 0,
            IsSearch = option.Searches.Count > 0
        };
        var query = db.Queryable<TDengineDBNumberHistoryValue>().AsTDengineSTable();

        query = db.GetQuery<TDengineDBNumberHistoryValue>(option, query);

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

#endif
}
