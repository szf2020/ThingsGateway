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

using Mapster;

using ThingsGateway.Admin.Application;
using ThingsGateway.Debug;
using ThingsGateway.Foundation;
using ThingsGateway.NewLife;
using ThingsGateway.NewLife.Extension;
using ThingsGateway.Plugin.DB;
using ThingsGateway.SqlSugar;

namespace ThingsGateway.Plugin.SqlDB;

/// <summary>
/// SqlDBProducer
/// </summary>
public partial class SqlDBProducer : BusinessBaseWithCacheIntervalVariableModel<VariableBasicData>, IDBHistoryValueService
{
    internal readonly SqlDBProducerProperty _driverPropertys = new();
    private readonly SqlDBProducerVariableProperty _variablePropertys = new();

    public override Type DriverPropertyUIType => typeof(SqlDBProducerPropertyRazor);

    /// <inheritdoc/>
    public override Type DriverUIType
    {
        get
        {
            if (_driverPropertys.BigTextScriptHistoryTable.IsNullOrEmpty() && _driverPropertys.BigTextScriptRealTable.IsNullOrEmpty())
                return typeof(SqlDBPage);
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
    public async Task<SqlSugarPagedList<IDBHistoryValue>> GetDBHistoryValuePagesAsync(DBHistoryValuePageInput input)
    {
        var data = await Query(input).ToPagedListAsync<SQLNumberHistoryValue, IDBHistoryValue>(input.Current, input.Size).ConfigureAwait(false);//分页
        return data;
    }

    public async Task<List<IDBHistoryValue>> GetDBHistoryValuesAsync(DBHistoryValuePageInput input)
    {
        var data = await Query(input).ToListAsync().ConfigureAwait(false);
        return data.Cast<IDBHistoryValue>().ToList();
    }

    /// <inheritdoc/>
    public override bool IsConnected() => success;

    /// <inheritdoc/>
    public override string ToString()
    {
        return $" {nameof(SqlDBProducer)}";
    }

    internal async Task<QueryData<SQLNumberHistoryValue>> QueryHistoryData(QueryPageOptions option)
    {
        var db = SqlDBBusinessDatabaseUtil.GetDb(_driverPropertys);
        var ret = new QueryData<SQLNumberHistoryValue>()
        {
            IsSorted = option.SortOrder != SortOrder.Unset,
            IsFiltered = option.Filters.Count > 0,
            IsAdvanceSearch = option.AdvanceSearches.Count > 0 || option.CustomerSearches.Count > 0,
            IsSearch = option.Searches.Count > 0
        };

        var query = db.Queryable<SQLNumberHistoryValue>().SplitTable();
        query = db.GetQuery<SQLNumberHistoryValue>(option, query);
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

    internal async Task<QueryData<SQLRealValue>> QueryRealData(QueryPageOptions option)
    {
        var db = SqlDBBusinessDatabaseUtil.GetDb(_driverPropertys);
        var ret = new QueryData<SQLRealValue>()
        {
            IsSorted = option.SortOrder != SortOrder.Unset,
            IsFiltered = option.Filters.Count > 0,
            IsAdvanceSearch = option.AdvanceSearches.Count > 0 || option.CustomerSearches.Count > 0,
            IsSearch = option.Searches.Count > 0
        };

        var query = db.Queryable<SQLRealValue>().AS(_driverPropertys.ReadDBTableName);
        query = db.GetQuery<SQLRealValue>(option, query);

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

    protected override async Task InitChannelAsync(IChannel? channel, CancellationToken cancellationToken)
    {
        _db = SqlDBBusinessDatabaseUtil.GetDb(_driverPropertys);

        _config = new TypeAdapterConfig();
        _config.ForType<VariableRuntime, SQLHistoryValue>()
            //.Map(dest => dest.Id, (src) =>CommonUtils.GetSingleId())
            .Map(dest => dest.Id, src => src.Id)//Id更改为变量Id
            .Map(dest => dest.Value, src => src.Value == null ? string.Empty : src.Value.ToString() ?? string.Empty)
            .Map(dest => dest.CreateTime, (src) => DateTime.Now);

        _config.ForType<VariableBasicData, SQLHistoryValue>()
    //.Map(dest => dest.Id, (src) =>CommonUtils.GetSingleId())
    .Map(dest => dest.Id, src => src.Id)//Id更改为变量Id
    .Map(dest => dest.Value, src => src.Value == null ? string.Empty : src.Value.ToString() ?? string.Empty)
    .Map(dest => dest.CreateTime, (src) => DateTime.Now);

        _config.ForType<VariableRuntime, SQLNumberHistoryValue>()
           //.Map(dest => dest.Id, (src) =>CommonUtils.GetSingleId())
           .Map(dest => dest.Id, src => src.Id)//Id更改为变量Id
    .Map(dest => dest.Value, src => src.Value.GetType() == typeof(bool) ? ConvertUtility.Convert.ToBoolean(src.Value, false) ? 1 : 0 : ConvertUtility.Convert.ToDecimal(src.Value, 0))
           .Map(dest => dest.CreateTime, (src) => DateTime.Now);

        _config.ForType<VariableBasicData, SQLNumberHistoryValue>()
    //.Map(dest => dest.Id, (src) =>CommonUtils.GetSingleId())
    .Map(dest => dest.Id, src => src.Id)//Id更改为变量Id
    .Map(dest => dest.Value, src => src.Value.GetType() == typeof(bool) ? ConvertUtility.Convert.ToBoolean(src.Value, false) ? 1 : 0 : ConvertUtility.Convert.ToDecimal(src.Value, 0))
    .Map(dest => dest.CreateTime, (src) => DateTime.Now);

        if (_businessPropertyWithCacheInterval.BusinessUpdateEnum == BusinessUpdateEnum.Interval && _driverPropertys.IsReadDB)
        {
            GlobalData.VariableValueChangeEvent += VariableValueChange;
        }

        await base.InitChannelAsync(channel, cancellationToken).ConfigureAwait(false);


    }
    public override Task AfterVariablesChangedAsync(CancellationToken cancellationToken)
    {
        RealTimeVariables.Clear();
        _initRealData = false;
        return base.AfterVariablesChangedAsync(cancellationToken);
    }
    protected override async Task ProtectedStartAsync(CancellationToken cancellationToken)
    {
        _db.DbMaintenance.CreateDatabase();

        //必须为间隔上传
        if (!_driverPropertys.BigTextScriptHistoryTable.IsNullOrEmpty())
        {
            var hisModel = CSharpScriptEngineExtension.Do<DynamicSQLBase>(_driverPropertys.BigTextScriptHistoryTable);

            if (_driverPropertys.IsHistoryDB)
            {
                await hisModel.DBInit(_db, cancellationToken).ConfigureAwait(false);
            }

        }
        else
        {
            if (_driverPropertys.IsHistoryDB)
            {
                _db.CodeFirst.InitTables(typeof(SQLHistoryValue));
                _db.CodeFirst.InitTables(typeof(SQLNumberHistoryValue));

            }
        }
        if (!_driverPropertys.BigTextScriptRealTable.IsNullOrEmpty())
        {
            var realModel = CSharpScriptEngineExtension.Do<DynamicSQLBase>(_driverPropertys.BigTextScriptRealTable);

            if (_driverPropertys.IsReadDB)
            {
                await realModel.DBInit(_db, cancellationToken).ConfigureAwait(false);
            }

        }
        else
        {
            if (_driverPropertys.IsReadDB)
                _db.CodeFirst.As<SQLRealValue>(_driverPropertys.ReadDBTableName).InitTables<SQLRealValue>();
        }

        await base.ProtectedStartAsync(cancellationToken).ConfigureAwait(false);
    }

    protected override async Task ProtectedExecuteAsync(object? state, CancellationToken cancellationToken)
    {
        if (_driverPropertys.IsReadDB)
        {
            try
            {
                var varList = RealTimeVariables.ToListWithDequeue();
                if (varList.Count > 0)
                {
                    var result = await UpdateAsync(varList, cancellationToken).ConfigureAwait(false);
                    if (success != result.IsSuccess)
                    {
                        if (!result.IsSuccess)
                            LogMessage?.LogWarning(result.ToString());
                        success = result.IsSuccess;
                    }
                }
            }
            catch (Exception ex)
            {
                if (success)
                    LogMessage?.LogWarning(ex);
                success = false;
            }
        }

        if (_driverPropertys.IsHistoryDB)
        {
            await UpdateVarModelMemory(cancellationToken).ConfigureAwait(false);
            await UpdateVarModelsMemory(cancellationToken).ConfigureAwait(false);
            await UpdateVarModelCache(cancellationToken).ConfigureAwait(false);
            await UpdateVarModelsCache(cancellationToken).ConfigureAwait(false);
        }
    }

    private ISugarQueryable<SQLNumberHistoryValue> Query(DBHistoryValuePageInput input)
    {
        var db = SqlDBBusinessDatabaseUtil.GetDb(_driverPropertys);

        var query = db.Queryable<SQLNumberHistoryValue>().SplitTable()
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
}
