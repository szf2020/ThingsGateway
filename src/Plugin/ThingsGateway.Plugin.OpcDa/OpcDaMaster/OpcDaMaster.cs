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

using System.Collections.Concurrent;

using ThingsGateway.Foundation.OpcDa;
using ThingsGateway.Foundation.OpcDa.Da;
using ThingsGateway.Gateway.Application;
using ThingsGateway.NewLife.Json.Extension;

using TouchSocket.Core;

namespace ThingsGateway.Plugin.OpcDa;

/// <summary>
/// <inheritdoc/>
/// </summary>
[OnlyWindowsSupport]
public class OpcDaMaster : CollectBase
{
    private readonly OpcDaMasterProperty _driverProperties = new();

    private ThingsGateway.Foundation.OpcDa.OpcDaMaster _plc;

    private volatile bool success = true;

    /// <inheritdoc/>
    public override CollectPropertyBase CollectProperties => _driverProperties;

    /// <inheritdoc/>
    public override Type DriverDebugUIType => typeof(ThingsGateway.Debug.OpcDaMaster);

    /// <inheritdoc/>
    protected override async Task InitChannelAsync(IChannel? channel, CancellationToken cancellationToken)
    {
        //载入配置
        OpcDaProperty opcNode = new()
        {
            OpcIP = _driverProperties.OpcIP,
            OpcName = _driverProperties.OpcName,
            UpdateRate = _driverProperties.UpdateRate,
            DeadBand = _driverProperties.DeadBand,
            GroupSize = _driverProperties.GroupSize,
            ActiveSubscribe = _driverProperties.ActiveSubscribe,
            CheckRate = _driverProperties.CheckRate
        };

        var plc = _plc;
        _plc = new();
        if (plc != null)
        {
            plc.DataChangedHandler -= DataChangedHandler;
            plc.LogEvent = null;
            plc.SafeDispose();
        }

        _plc.DataChangedHandler += DataChangedHandler;
        _plc.LogEvent = Log;
        _plc.Init(opcNode);
        await base.InitChannelAsync(channel, cancellationToken).ConfigureAwait(false);
    }

    private void Log(byte level, object sender, string message, Exception ex)
    {
        LogMessage?.Log((LogLevel)level, sender, message, ex);
    }

    /// <inheritdoc/>
    public override bool IsConnected() => _plc?.IsConnected == true;

    public override string ToString()
    {
        return $"{_driverProperties.OpcIP}-{_driverProperties.OpcName}";
    }

    /// <inheritdoc/>
    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (_plc != null)
            _plc.DataChangedHandler -= DataChangedHandler;
        _plc?.SafeDispose();

        VariableAddresDicts?.Clear();
        base.Dispose(disposing);
    }

    public override string GetAddressDescription()
    {
        return _plc?.GetAddressDescription();
    }

    protected override bool VariableSourceReadsEnable => !_driverProperties.ActiveSubscribe;

    /// <inheritdoc/>
    protected override Task<List<VariableSourceRead>> ProtectedLoadSourceReadAsync(List<VariableRuntime> deviceVariables)
    {
        if (deviceVariables.Count > 0)
        {
            List<VariableSourceRead> variableSourceReads = new List<VariableSourceRead>();
            foreach (var deviceVariableGroups in deviceVariables.GroupBy(a => a.CollectGroup))
            {
                var result = _plc.AddItemsWithSave(deviceVariableGroups.Where(a => !string.IsNullOrEmpty(a.RegisterAddress)).Select(a => a.RegisterAddress!).ToList());
                var sourVars = result?.Select(
          it =>
          {
              var read = new VariableSourceRead()
              {
                  IntervalTime = _driverProperties.UpdateRate.ToString(),
                  RegisterAddress = it.Key,
              };
              HashSet<string> ids = new(it.Value.Select(b => b.ItemID));

              var variables = deviceVariableGroups.Where(a => ids.Contains(a.RegisterAddress));
              foreach (var v in variables)
              {
                  read.AddVariable(v);
              }
              return read;
          }).ToList();
                variableSourceReads.AddRange(sourVars);
            }
            return Task.FromResult(variableSourceReads);
        }
        else
        {
            return Task.FromResult(new List<VariableSourceRead>());
        }
    }

    /// <inheritdoc/>
    protected override ValueTask<OperResult<ReadOnlyMemory<byte>>> ReadSourceAsync(VariableSourceRead deviceVariableSourceRead, CancellationToken cancellationToken)
    {
        try
        {
            _plc.ReadItemsWithGroup(deviceVariableSourceRead.RegisterAddress);
            return ValueTask.FromResult(OperResult.CreateSuccessResult(ReadOnlyMemory<byte>.Empty));
        }
        catch (Exception ex)
        {
            return ValueTask.FromResult(new OperResult<ReadOnlyMemory<byte>>($"ReadSourceAsync Error：{Environment.NewLine}{ex}"));
        }
    }

    /// <inheritdoc/>
    protected override async ValueTask<Dictionary<string, OperResult>> WriteValuesAsync(Dictionary<VariableRuntime, JToken> writeInfoLists, CancellationToken cancellationToken)
    {
        using var writeLock = ReadWriteLock.WriterLock();
        await ValueTask.CompletedTask.ConfigureAwait(false);
        var result = _plc.WriteItem(writeInfoLists.ToDictionary(a => a.Key.RegisterAddress!, a => a.Value.GetObjectFromJToken()!));
        var results = new ConcurrentDictionary<string, OperResult>(result.ToDictionary<KeyValuePair<string, Tuple<bool, string>>, string, OperResult>(a => writeInfoLists.Keys.FirstOrDefault(b => b.RegisterAddress == a.Key).Name, a =>
        {
            if (!a.Value.Item1)
                return new OperResult(a.Value.Item2);
            else
                return OperResult.Success;
        }
             ));

        await Check(writeInfoLists, results, cancellationToken).ConfigureAwait(false);

        return new(results);
    }
    public override async Task AfterVariablesChangedAsync(CancellationToken cancellationToken)
    {
        _plc?.Disconnect();
        await base.AfterVariablesChangedAsync(cancellationToken).ConfigureAwait(false);

        VariableAddresDicts = IdVariableRuntimes.Select(a => a.Value).Where(it => !it.RegisterAddress.IsNullOrEmpty()).GroupBy(a => a.RegisterAddress).ToDictionary(a => a.Key!, b => b.ToList());
        try
        {
            _plc?.Connect();
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }

        RefreshVariableTasks(cancellationToken);
    }

    private Dictionary<string, List<VariableRuntime>> VariableAddresDicts { get; set; } = new();

    private void DataChangedHandler(string name, int serverGroupHandle, List<ItemReadResult> values)
    {
        DateTime time = DateTime.Now;
        try
        {
            if (CurrentDevice.Pause)
                return;
            if (TaskSchedulerLoop?.Stoped == true) return;
            if (DisposedValue)
                return;
            LogMessage?.Trace($"{ToString()} Change:{Environment.NewLine} {values?.ToSystemTextJsonString()}");

            foreach (var data in values)
            {
                if (CurrentDevice.Pause)
                    return;
                if (TaskSchedulerLoop?.Stoped == true) return;
                if (DisposedValue)
                    return;
                var type = data.Value.GetType();
                if (data.Value is Array)
                {
                    type = type.GetElementType();
                }
                if (!VariableAddresDicts.TryGetValue(data.Name, out var itemReads)) return;

                foreach (var item in itemReads)
                {
                    if (CurrentDevice.Pause)
                        return;
                    if (TaskSchedulerLoop?.Stoped == true) return;
                    if (DisposedValue)
                        return;
                    var value = data.Value;
                    var quality = data.Quality;
                    if (_driverProperties.SourceTimestampEnable)
                    {
                        time = data.TimeStamp.ToLocalTime();
                    }
                    if (quality == 192)
                    {
                        item.SetValue(value, time);
                    }
                    else
                    {
                        item.SetValue(null, time, false);
                        item.VariableSource.LastErrorMessage = $"Bad quality：{quality}";
                    }
                }
            }

            success = true;
        }
        catch (Exception ex)
        {
            if (success)
                LogMessage?.LogWarning(ex);
            success = false;
        }
    }
}
