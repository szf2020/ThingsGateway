//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Newtonsoft.Json.Linq;

using System.Diagnostics;
using System.Text;

using ThingsGateway.Extension.Generic;
using ThingsGateway.Foundation;
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
    protected override ValueTask<OperResult> UpdateVarModel(IEnumerable<CacheDBItem<VariableBasicData>> item, CancellationToken cancellationToken)
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
    protected override ValueTask<OperResult> UpdateVarModels(IEnumerable<VariableBasicData> item, CancellationToken cancellationToken)
    {
        return UpdateVarModel(item, cancellationToken);
    }
    private void TimeIntervalUpdateVariable(IEnumerable<VariableBasicData> variables)
    {
        if (_driverPropertys.GroupUpdate)
        {
            var varList = variables.Where(a => a.BusinessGroup.IsNullOrEmpty());
            var varGroup = variables.Where(a => !a.BusinessGroup.IsNullOrEmpty()).GroupBy(a => a.BusinessGroup);

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

                await InserableAsync(numberData, _driverPropertys.NumberTableNameLow, cancellationToken).ConfigureAwait(false);

                await InserableAsync(stringData, _driverPropertys.StringTableNameLow, cancellationToken).ConfigureAwait(false);
            }
            return OperResult.Success;
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    private async Task InserableAsync(IEnumerable<VariableBasicData> dbInserts, string tableName, CancellationToken cancellationToken)
    {
        if (!dbInserts.Any())
        {
            return;
        }
        Stopwatch stopwatch = new();
        stopwatch.Start();

        StringBuilder stringBuilder = new();
        stringBuilder.Append($"INSERT INTO");
        //(`id`,`createtime`,`collecttime`,`isonline`,`value`) 
        foreach (var deviceGroup in dbInserts.GroupBy(a => a.DeviceName))
        {
            foreach (var variableGroup in deviceGroup.GroupBy(a => a.Name))
            {
                stringBuilder.Append($"""

                     `{tableName}_{deviceGroup.Key}_{variableGroup.Key}` 
                     USING `{tableName}` TAGS ("{deviceGroup.Key}", "{variableGroup.Key}") 
                    VALUES 

                    """);

                foreach (var item in variableGroup)
                {
                    stringBuilder.Append($"""(NOW,"{item.CollectTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}",{item.Id},{item.IsOnline},"{GetValue(item)}"),""");
                }
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }
        }
        stringBuilder.Append(';');
        stringBuilder.AppendLine();

        var result = await _db.Ado.ExecuteCommandAsync(stringBuilder.ToString(), default, cancellationToken: cancellationToken).ConfigureAwait(false);

        stopwatch.Stop();
        //if (result > 0)
        {
            LogMessage?.Trace($"TableName：{tableName}，Count：{result}，watchTime:  {stopwatch.ElapsedMilliseconds} ms");
        }
    }
    private string GetValue(VariableBasicData src)
    {
        if (src.Value != null)
        {
            if (src.Value is string strValue)
            {
                return strValue;
            }
            else if (src.Value is bool boolValue)
            {
                return boolValue ? "1" : "0";
            }
            else
            {
                return JToken.FromObject(src.Value).ToString();
            }
        }
        else
        {
            return string.Empty;
        }
    }
    #endregion 方法
}
