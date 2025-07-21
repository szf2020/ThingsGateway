
using System.Collections.Concurrent;

using ThingsGateway.Blazor.Diagrams.Core.Geometry;
using ThingsGateway.Common.Extension;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

[CategoryNode(Category = "Trigger", ImgUrl = "_content/ThingsGateway.Gateway.Razor/img/ValueChanged.svg", Desc = nameof(AlarmChangedTriggerNode), LocalizerType = typeof(ThingsGateway.Gateway.Application.DefaultDiagram), WidgetType = "ThingsGateway.Gateway.Razor.VariableWidget,ThingsGateway.Gateway.Razor")]
public class AlarmChangedTriggerNode : VariableNode, ITriggerNode, IDisposable
{
    public AlarmChangedTriggerNode(string id, Point? position = null) : base(id, position) { Title = "AlarmChangedTriggerNode"; }

    private Func<NodeOutput, CancellationToken, Task> Func { get; set; }
    Task ITriggerNode.StartAsync(Func<NodeOutput, CancellationToken, Task> func, CancellationToken cancellationToken)
    {
        Func = func;
        FuncDict.TryAdd(this, func);
        if (AlarmChangedTriggerNodeDict.TryGetValue(DeviceText, out var deviceVariableDict))
        {
            if (deviceVariableDict.TryGetValue(Text, out var alarmChangedTriggerNodes))
            {
                alarmChangedTriggerNodes.Add(this);
            }
            else
            {
                deviceVariableDict.TryAdd(Text, new());
                deviceVariableDict[Text].Add(this);
            }
        }
        else
        {
            AlarmChangedTriggerNodeDict.TryAdd(DeviceText, new());
            AlarmChangedTriggerNodeDict[DeviceText].TryAdd(Text, new());
            AlarmChangedTriggerNodeDict[DeviceText][Text].Add(this);
        }
        return Task.CompletedTask;
    }

    public static ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentList<AlarmChangedTriggerNode>>> AlarmChangedTriggerNodeDict = new();

    public static ConcurrentDictionary<AlarmChangedTriggerNode, Func<NodeOutput, CancellationToken, Task>> FuncDict = new();

    public static BlockingCollection<AlarmVariable> AlarmVariables = new();
    static AlarmChangedTriggerNode()
    {
        Task.Factory.StartNew(RunAsync);
        GlobalData.AlarmChangedEvent -= AlarmHostedService_OnAlarmChanged;
        GlobalData.ReadOnlyRealAlarmIdVariables?.ForEach(a => AlarmHostedService_OnAlarmChanged(a.Value));
        GlobalData.AlarmChangedEvent += AlarmHostedService_OnAlarmChanged;
    }
    private static void AlarmHostedService_OnAlarmChanged(AlarmVariable alarmVariable)
    {
        if (AlarmChangedTriggerNodeDict.TryGetValue(alarmVariable.DeviceName, out var alarmNodeDict) &&
            alarmNodeDict.TryGetValue(alarmVariable.Name, out var alarmChangedTriggerNodes) &&
            alarmChangedTriggerNodes?.Count > 0)
        {
            if (!AlarmVariables.IsAddingCompleted)
            {
                try
                {
                    AlarmVariables.Add(alarmVariable);
                    return;
                }
                catch (InvalidOperationException) { }
                catch { }
            }
        }
    }
    static Task RunAsync()
    {
        return AlarmVariables.GetConsumingEnumerable().ParallelForEachStreamedAsync((async (alarmVariable, token) =>
            {
                if (AlarmChangedTriggerNodeDict.TryGetValue(alarmVariable.DeviceName, out var alarmNodeDict) &&
            alarmNodeDict.TryGetValue(alarmVariable.Name, out var alarmChangedTriggerNodes))
                {
                    await alarmChangedTriggerNodes.ParallelForEachAsync(async (item, token) =>
                       {
                           try
                           {
                               if (FuncDict.TryGetValue(item, out var func))
                               {
                                   item.Logger?.Trace($"Alarm changed: {item.Text}");
                                   await func.Invoke(new NodeOutput() { Value = alarmVariable }, token).ConfigureAwait(false);
                               }
                           }
                           catch (Exception ex)
                           {
                               item.Logger?.LogWarning(ex);
                           }
                       }, token).ConfigureAwait(false);
                }
            }), default);
    }

    public void Dispose()
    {
        FuncDict.TryRemove(this, out _);
        if (AlarmChangedTriggerNodeDict.TryGetValue(DeviceText, out var alarmNodeDict) &&
            alarmNodeDict.TryGetValue(Text, out var alarmChangedTriggerNodes))
        {
            alarmChangedTriggerNodes.Remove(this);
        }
    }
}
