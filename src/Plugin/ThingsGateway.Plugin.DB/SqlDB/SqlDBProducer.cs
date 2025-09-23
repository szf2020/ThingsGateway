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
using ThingsGateway.Extension.Generic;
using ThingsGateway.Foundation;
using ThingsGateway.NewLife;
using ThingsGateway.NewLife.DictionaryExtensions;
using ThingsGateway.NewLife.Extension;
using ThingsGateway.NewLife.Threading;
using ThingsGateway.Plugin.DB;
using ThingsGateway.SqlSugar;

namespace ThingsGateway.Plugin.SqlDB;

/// <summary>
/// SqlDBProducer
/// </summary>
public partial class SqlDBProducer : BusinessBaseWithCacheIntervalVariable
#if !Management
    , IDBHistoryValueService
#endif
{


    internal readonly SqlDBProducerProperty _driverPropertys = new();
    private readonly SqlDBProducerVariableProperty _variablePropertys = new();

    public override Type DriverPropertyUIType => typeof(SqlDBProducerPropertyRazor);

    /// <inheritdoc/>
    public override Type DriverUIType
    {
        get
        {
#if !Management
            if (_driverPropertys.BigTextScriptHistoryTable.IsNullOrEmpty() && _driverPropertys.BigTextScriptRealTable.IsNullOrEmpty())
                return typeof(SqlDBPage);
            else
                return null;
#else
            return null;
#endif
        }
    }

    public override VariablePropertyBase VariablePropertys => _variablePropertys;

    protected override BusinessPropertyWithCacheInterval _businessPropertyWithCacheInterval => _driverPropertys;
    private SqlSugarClient _db;
    protected override Task DisposeAsync(bool disposing)
    {
        _db?.TryDispose();
        return base.DisposeAsync(disposing);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $" {nameof(SqlDBProducer)}";
    }




#if !Management
    protected override List<IScheduledTask> ProtectedGetTasks(CancellationToken cancellationToken)
    {
        var list = base.ProtectedGetTasks(cancellationToken);
        list.Add(ScheduledTaskHelper.GetTask("0 0 * * *", DeleteByDayAsync, null, LogMessage, cancellationToken));

        return list;
    }

    private async Task DeleteByDayAsync(object? state, CancellationToken cancellationToken)
    {
        try
        {
            using var db = SqlDBBusinessDatabaseUtil.GetDb(_driverPropertys);
            if (!_driverPropertys.BigTextScriptHistoryTable.IsNullOrEmpty())
            {
                var hisModel = CSharpScriptEngineExtension.Do<DynamicSQLBase>(_driverPropertys.BigTextScriptHistoryTable);

                if (_driverPropertys.IsHistoryDB)
                {
                    await hisModel.DBDeleteable(db, _driverPropertys.SaveDays, cancellationToken).ConfigureAwait(false);

                }
            }
            else
            {
                if (_driverPropertys.IsHistoryDB)
                {
                    {
                        var time = TimerX.Now - TimeSpan.FromDays(-_driverPropertys.SaveDays);
                        var tableNames = db.SplitHelper<SQLHistoryValue>().GetTables();//根据时间获取表名
                        var filtered = tableNames.Where(a => a.Date < time).ToList();
                        // 去掉最后一个
                        var oldTable = filtered.Take(filtered.Count - 1);

                        foreach (var table in oldTable)
                        {
                            db.DbMaintenance.DropTable(table.TableName);
                        }
                        var deldata = filtered.LastOrDefault();
                        if (deldata != null)
                        {
                            await db.Deleteable<SQLHistoryValue>().AS(deldata.TableName).Where(a => a.CreateTime < time).ExecuteCommandAsync(cancellationToken).ConfigureAwait(false);
                        }
                    }

                    {
                        var time = TimerX.Now - TimeSpan.FromDays(-_driverPropertys.SaveDays);
                        var tableNames = db.SplitHelper<SQLNumberHistoryValue>().GetTables();//根据时间获取表名
                        var filtered = tableNames.Where(a => a.Date < time).ToList();
                        // 去掉最后一个
                        var oldTable = filtered.Take(filtered.Count - 1);

                        foreach (var table in oldTable)
                        {
                            db.DbMaintenance.DropTable(table.TableName);
                        }
                        var deldata = filtered.LastOrDefault();
                        if (deldata != null)
                        {
                            await db.Deleteable<SQLNumberHistoryValue>().AS(deldata.TableName).Where(a => a.CreateTime < time).ExecuteCommandAsync(cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
                LogMessage?.LogInformation($"Clean up historical data from {_driverPropertys.SaveDays} days ago");
            }

        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex, "Clearing historical data error");
        }
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



    protected override async Task InitChannelAsync(IChannel? channel, CancellationToken cancellationToken)
    {
        _db = SqlDBBusinessDatabaseUtil.GetDb(_driverPropertys);

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
                _db.CodeFirst.AS<SQLRealValue>(_driverPropertys.ReadDBTableName).InitTables<SQLRealValue>();
        }

        await base.ProtectedStartAsync(cancellationToken).ConfigureAwait(false);
    }

    protected override async Task ProtectedExecuteAsync(object? state, CancellationToken cancellationToken)
    {
        if (_driverPropertys.IsReadDB)
        {
            var list = RealTimeVariables.ToListWithDequeue();
            try
            {
                var varLists = list.Batch(_driverPropertys.SplitSize);
                foreach (var varList in varLists)
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

                list.ForEach(variable => RealTimeVariables.AddOrUpdate(variable.Id, variable, (key, oldValue) => variable));
            }
        }

        if (_driverPropertys.IsHistoryDB)
        {
            await Update(cancellationToken).ConfigureAwait(false);
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

#endif
}
