//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using PooledAwait;

using System.Collections.Concurrent;
using System.Diagnostics;

using ThingsGateway.Extension.Generic;
using ThingsGateway.Foundation;
using ThingsGateway.Plugin.DB;

using TouchSocket.Core;

namespace ThingsGateway.Plugin.SqlDB;

/// <summary>
/// SqlDBProducer
/// </summary>
public partial class SqlDBProducer : BusinessBaseWithCacheIntervalVariable
{
#if !Management
    private volatile bool _initRealData;
    private NonBlockingDictionary<long, VariableBasicData> RealTimeVariables { get; } = new NonBlockingDictionary<long, VariableBasicData>();

    protected override ValueTask<OperResult> UpdateVarModel(List<CacheDBItem<VariableBasicData>> item, CancellationToken cancellationToken)
    {
        return UpdateVarModel(item.Select(a => a.Value).OrderBy(a => a.Id), cancellationToken);
    }
    protected override void VariableTimeInterval(IEnumerable<VariableRuntime> variableRuntimes, IEnumerable<VariableBasicData> variables)
    {
        if (_driverPropertys.IsHistoryDB)
        {
            TimeIntervalUpdateVariable(variables);
        }

        base.VariableTimeInterval(variableRuntimes, variables);
    }
    protected override void VariableChange(VariableRuntime variableRuntime, VariableBasicData variable)
    {
        UpdateVariable(variableRuntime, variable);
        base.VariableChange(variableRuntime, variable);
    }
    protected override ValueTask<OperResult> UpdateVarModels(List<VariableBasicData> item, CancellationToken cancellationToken)
    {
        return UpdateVarModel(item, cancellationToken);
    }

    private void TimeIntervalUpdateVariable(IEnumerable<VariableBasicData> variables)
    {
        if (_driverPropertys.GroupUpdate)
        {
            var data = variables is System.Collections.IList ? variables : variables.ToArray();
            var varList = data.Where(a => a.BusinessGroup.IsNullOrEmpty());
            var varGroup = data.Where(a => !a.BusinessGroup.IsNullOrEmpty()).GroupBy(a => a.BusinessGroup);

            foreach (var group in varGroup)
            {
                AddQueueVarModel(new CacheDBItem<List<VariableBasicData>>(group.ToList()));
            }
            foreach (var variable in varList)
            {
                AddQueueVarModel(new CacheDBItem<VariableBasicData>(variable));
            }
        }
        else
        {
            foreach (var variable in variables)
            {
                AddQueueVarModel(new CacheDBItem<VariableBasicData>(variable));
            }
        }
    }

    private void UpdateVariable(VariableRuntime variableRuntime, VariableBasicData variable)
    {
        if (_driverPropertys.IsHistoryDB && _businessPropertyWithCacheInterval.BusinessUpdateEnum != BusinessUpdateEnum.Interval)
        {
            if (_driverPropertys.GroupUpdate && variable.BusinessGroupUpdateTrigger && !variable.BusinessGroup.IsNullOrEmpty() && VariableRuntimeGroups.TryGetValue(variable.BusinessGroup, out var variableRuntimeGroup))
            {
                AddQueueVarModel(new CacheDBItem<List<VariableBasicData>>(variableRuntimeGroup.AdaptListVariableBasicData()));
            }
            else
            {
                AddQueueVarModel(new CacheDBItem<VariableBasicData>(variable));
            }
        }

        if (_driverPropertys.IsReadDB)
        {
            RealTimeVariables.AddOrUpdate(variable.Id, variable, (key, oldValue) => variable);
        }
    }

    private ValueTask<OperResult> UpdateVarModel(IEnumerable<VariableBasicData> item, CancellationToken cancellationToken)
    {
        return UpdateVarModel(this, item, cancellationToken);

        static async PooledValueTask<OperResult> UpdateVarModel(SqlDBProducer @this, IEnumerable<VariableBasicData> item, CancellationToken cancellationToken)
        {
            var result = await @this.InserableAsync(item.WhereIf(@this._driverPropertys.OnlineFilter, a => a.IsOnline == true).ToList(), cancellationToken).ConfigureAwait(false);
            if (@this.success != result.IsSuccess)
            {
                if (!result.IsSuccess)
                    @this.LogMessage?.LogWarning(result.ToString());
                @this.success = result.IsSuccess;
            }

            return result;
        }
    }

    #region 方法

    private ValueTask<OperResult> InserableAsync(List<VariableBasicData> dbInserts, CancellationToken cancellationToken)
    {
        return InserableAsync(this, dbInserts, cancellationToken);

        static async PooledValueTask<OperResult> InserableAsync(SqlDBProducer @this, List<VariableBasicData> dbInserts, CancellationToken cancellationToken)
        {
            try
            {
                @this._db.Ado.CancellationToken = cancellationToken;
                if (!@this._driverPropertys.BigTextScriptHistoryTable.IsNullOrEmpty())
                {
                    var getDeviceModel = CSharpScriptEngineExtension.Do<DynamicSQLBase>(@this._driverPropertys.BigTextScriptHistoryTable);

                    getDeviceModel.Logger = @this.LogMessage;

                    await getDeviceModel.DBInsertable(@this._db, dbInserts, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    var stringData = dbInserts.Where(a => (!a.IsNumber && a.Value is not bool));
                    var numberData = dbInserts.Where(a => (a.IsNumber || a.Value is bool));

                    if (numberData.Any())
                    {
                        var data = numberData.AdaptEnumerableSQLNumberHistoryValue();
                        Stopwatch stopwatch = new();
                        stopwatch.Start();
                        var result = await @this._db.Fastest<SQLNumberHistoryValue>().SplitTable().BulkCopyAsync(data).ConfigureAwait(false);
                        stopwatch.Stop();

                        //var result = await db.Insertable(dbInserts).SplitTable().ExecuteCommandAsync().ConfigureAwait(false);
                        if (result > 0)
                        {
                            @this.LogMessage?.Trace($"TableName：{@this._driverPropertys.NumberTableName}，Count：{result}，watchTime:  {stopwatch.ElapsedMilliseconds} ms");
                        }
                    }

                    if (stringData.Any())
                    {
                        Stopwatch stopwatch = new();
                        stopwatch.Start();
                        var data = stringData.AdaptEnumerableSQLHistoryValue();
                        var result = await @this._db.Fastest<SQLHistoryValue>().SplitTable().BulkCopyAsync(data).ConfigureAwait(false);
                        stopwatch.Stop();

                        //var result = await db.Insertable(dbInserts).SplitTable().ExecuteCommandAsync().ConfigureAwait(false);
                        if (result > 0)
                        {
                            @this.LogMessage?.Trace($"TableName：{@this._driverPropertys.StringTableName}，Count：{result}，watchTime:  {stopwatch.ElapsedMilliseconds} ms");
                        }
                    }
                }

                return OperResult.Success;
            }
            catch (Exception ex)
            {
                return new OperResult(ex);
            }
        }
    }

    private ValueTask<OperResult> UpdateAsync(List<VariableBasicData> datas, CancellationToken cancellationToken)
    {
        return UpdateAsync(this, datas, cancellationToken);

        static async PooledValueTask<OperResult> UpdateAsync(SqlDBProducer @this, List<VariableBasicData> datas, CancellationToken cancellationToken)
        {
            try
            {
                @this._db.Ado.CancellationToken = cancellationToken;

                if (!@this._driverPropertys.BigTextScriptRealTable.IsNullOrEmpty())
                {
                    var getDeviceModel = CSharpScriptEngineExtension.Do<DynamicSQLBase>(@this._driverPropertys.BigTextScriptRealTable);
                    getDeviceModel.Logger = @this.LogMessage;

                    await getDeviceModel.DBInsertable(@this._db, datas, cancellationToken).ConfigureAwait(false);
                    return OperResult.Success;
                }
                else
                {
                    if (!@this._initRealData)
                    {
                        Stopwatch stopwatch = new();
                        stopwatch.Start();
                        var ids = (await @this._db.Queryable<SQLRealValue>().AS(@this._driverPropertys.ReadDBTableName).Select(a => a.Id).ToListAsync(cancellationToken).ConfigureAwait(false)).ToHashSet();
                        var InsertData = @this.IdVariableRuntimes.Where(a => !ids.Contains(a.Key)).Select(a => a.Value).AdaptEnumerableSQLRealValue();
                        var result = await @this._db.Fastest<SQLRealValue>().AS(@this._driverPropertys.ReadDBTableName).BulkCopyAsync(InsertData).ConfigureAwait(false);
                        @this._initRealData = true;
                        stopwatch.Stop();
                        if (result > 0)
                        {
                            @this.LogMessage?.Trace($"RealTable Insert Data Count：{result}，watchTime:  {stopwatch.ElapsedMilliseconds} ms");
                        }
                    }
                    {
                        if (datas?.Count > 0)
                        {
                            Stopwatch stopwatch = new();
                            stopwatch.Start();

                            var data = datas.AdaptEnumerableSQLRealValue();
                            var result = await @this._db.Fastest<SQLRealValue>().AS(@this._driverPropertys.ReadDBTableName).BulkUpdateAsync(data).ConfigureAwait(false);

                            stopwatch.Stop();
                            if (result > 0)
                            {
                                @this.LogMessage?.Trace($"RealTable Data Count：{result}，watchTime:  {stopwatch.ElapsedMilliseconds} ms");
                            }
                            return OperResult.Success;
                        }
                        return OperResult.Success;
                    }
                }
            }
            catch (Exception ex)
            {
                return new OperResult(ex);
            }
        }
    }

    #endregion 方法

#endif
}
