//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Diagnostics;

using ThingsGateway.Extension.Generic;
using ThingsGateway.Foundation;
using ThingsGateway.Plugin.DB;
using ThingsGateway.SqlSugar;

using TouchSocket.Core;

namespace ThingsGateway.Plugin.QuestDB;

/// <summary>
/// RabbitMQProducer
/// </summary>
public partial class QuestDBProducer : BusinessBaseWithCacheIntervalVariable
{
    protected override ValueTask<OperResult> UpdateVarModel(List<CacheDBItem<VariableBasicData>> item, CancellationToken cancellationToken)
    {
        return UpdateVarModel(item.Select(a => a.Value).OrderBy(a => a.Id), cancellationToken);
    }
    protected override void VariableTimeInterval(IEnumerable<VariableRuntime> variableRuntimes, IEnumerable<VariableBasicData> variables)
    {
        TimeIntervalUpdateVariable(variables);
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
        if (_driverPropertys.GroupUpdate && variable.BusinessGroupUpdateTrigger && !variable.BusinessGroup.IsNullOrEmpty() && VariableRuntimeGroups.TryGetValue(variable.BusinessGroup, out var variableRuntimeGroup))
        {
            AddQueueVarModel(new CacheDBItem<List<VariableBasicData>>(variableRuntimeGroup.AdaptListVariableBasicData()));
        }
        else
        {
            AddQueueVarModel(new CacheDBItem<VariableBasicData>(variable));
        }
    }
    private async ValueTask<OperResult> UpdateVarModel(IEnumerable<VariableBasicData> item, CancellationToken cancellationToken)
    {
        var result = await InserableAsync(item.WhereIf(_driverPropertys.OnlineFilter, a => a.IsOnline == true).ToList(), cancellationToken).ConfigureAwait(false);
        if (success != result.IsSuccess)
        {
            if (!result.IsSuccess)
                LogMessage?.LogWarning(result.ToString());
            success = result.IsSuccess;
        }

        return result;
    }

    #region 方法

    private async ValueTask<OperResult> InserableAsync(List<VariableBasicData> dbInserts, CancellationToken cancellationToken)
    {
        try
        {
            _db.Ado.CancellationToken = cancellationToken;
            if (!_driverPropertys.BigTextScriptHistoryTable.IsNullOrEmpty())
            {
                var getDeviceModel = CSharpScriptEngineExtension.Do<DynamicSQLBase>(_driverPropertys.BigTextScriptHistoryTable);
                getDeviceModel.Logger = LogMessage;

                await getDeviceModel.DBInsertable(_db, dbInserts, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var stringData = dbInserts.Where(a => (!a.IsNumber && a.Value is not bool));
                var numberData = dbInserts.Where(a => (a.IsNumber || a.Value is bool));

                if (numberData.Any())
                {
                    Stopwatch stopwatch = new();
                    stopwatch.Start();
                    var data = numberData.AdaptListQuestDBNumberHistoryValue();
                    int result = 0;
                    if (_driverPropertys.RestApi)
                    {
                        result = await _db.RestApi(_driverPropertys.HttpPort).BulkCopyAsync(data, _driverPropertys.NumberTableName).ConfigureAwait(false);//不要加分表
                    }
                    else
                    {
                        result = await _db.Insertable(data).AS(_driverPropertys.NumberTableName).ExecuteCommandAsync(cancellationToken).ConfigureAwait(false);//不要加分表
                    }
                    stopwatch.Stop();

                    //var result = await db.Insertable(dbInserts).SplitTable().ExecuteCommandAsync().ConfigureAwait(false);
                    if (result > 0)
                    {
                        LogMessage?.Trace($"TableName：{_driverPropertys.NumberTableName}，Count：{result}，watchTime:  {stopwatch.ElapsedMilliseconds} ms");
                    }
                }

                if (stringData.Any())
                {
                    Stopwatch stopwatch = new();
                    stopwatch.Start();
                    var data = stringData.AdaptListQuestDBHistoryValue();
                    int result = 0;
                    if (_driverPropertys.RestApi)
                    {
                        result = await _db.RestApi(_driverPropertys.HttpPort).BulkCopyAsync(data, _driverPropertys.StringTableName).ConfigureAwait(false);//不要加分表
                    }
                    else
                    {
                        result = await _db.Insertable(data).AS(_driverPropertys.StringTableName).ExecuteCommandAsync(cancellationToken).ConfigureAwait(false);//不要加分表
                    }

                    stopwatch.Stop();

                    //var result = await db.Insertable(dbInserts).SplitTable().ExecuteCommandAsync().ConfigureAwait(false);
                    if (result > 0)
                    {
                        LogMessage?.Trace($"TableName：{_driverPropertys.StringTableName}，Count：{result}，watchTime:  {stopwatch.ElapsedMilliseconds} ms");
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

    #endregion 方法
}
