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

using System.Diagnostics;
using System.Text;

using ThingsGateway.Extension.Generic;
using ThingsGateway.Foundation;
using ThingsGateway.NewLife.Json.Extension;
using ThingsGateway.Plugin.DB;
using ThingsGateway.SqlSugar;
using ThingsGateway.SqlSugar.TDengine;

using TouchSocket.Core;

namespace ThingsGateway.Plugin.TDengineDB;

/// <summary>
/// RabbitMQProducer
/// </summary>
public partial class TDengineDBProducer : BusinessBaseWithCacheIntervalVariable
{
#if !Management
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
    private ValueTask<OperResult> UpdateVarModel(IEnumerable<VariableBasicData> item, CancellationToken cancellationToken)
    {
        return UpdateVarModel(this, item, cancellationToken);

        static async PooledValueTask<OperResult> UpdateVarModel(TDengineDBProducer @this, IEnumerable<VariableBasicData> item, CancellationToken cancellationToken)
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

        static async PooledValueTask<OperResult> InserableAsync(TDengineDBProducer @this, List<VariableBasicData> dbInserts, CancellationToken cancellationToken)
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

                    await @this.InserableAsync(numberData, @this._driverPropertys.NumberTableNameLow, cancellationToken).ConfigureAwait(false);

                    await @this.InserableAsync(stringData, @this._driverPropertys.StringTableNameLow, cancellationToken).ConfigureAwait(false);
                }
                return OperResult.Success;
            }
            catch (Exception ex)
            {
                return new OperResult(ex);
            }
        }
    }

    private Task InserableAsync(IEnumerable<VariableBasicData> dbInserts, string tableName, CancellationToken cancellationToken)
    {
        return InserableAsync(this, dbInserts, tableName, cancellationToken);


        static async PooledTask InserableAsync(TDengineDBProducer @this, IEnumerable<VariableBasicData> dbInserts, string tableName, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();

            StringBuilder stringBuilder = new();
            stringBuilder.Append($"INSERT INTO");
            bool any = false;
            //(`id`,`createtime`,`collecttime`,`isonline`,`value`) 
            foreach (var deviceGroup in dbInserts.GroupBy(a => a.DeviceName))
            {
                foreach (var variableGroup in deviceGroup.GroupBy(a => a.Name))
                {
                    any = true;
                    stringBuilder.Append($"""

                     `{tableName}_{deviceGroup.Key}_{variableGroup.Key}` 
                     USING `{tableName}` TAGS ("{deviceGroup.Key}", "{variableGroup.Key}") 
                    VALUES 

                    """);

                    foreach (var item in variableGroup)
                    {
                        stringBuilder.Append($"""(NOW,"{item.CollectTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}",{item.Id},{item.IsOnline},"{JsonElementExtensions.GetValue(item.Value, true)}"),""");
                    }
                    stringBuilder.Remove(stringBuilder.Length - 1, 1);
                }
            }

            if (!any) return;

            stringBuilder.Append(';');
            stringBuilder.AppendLine();

            var result = await @this._db.Ado.ExecuteCommandAsync(stringBuilder.ToString(), default, cancellationToken: cancellationToken).ConfigureAwait(false);

            stopwatch.Stop();
            //if (result > 0)
            {
                @this.LogMessage?.Trace($"TableName：{tableName}，Count：{result}，watchTime:  {stopwatch.ElapsedMilliseconds} ms");
            }

            return;
        }
    }
    #endregion 方法

#endif
}
