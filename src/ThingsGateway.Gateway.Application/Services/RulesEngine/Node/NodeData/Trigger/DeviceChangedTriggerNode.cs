
using System.Collections.Concurrent;

using ThingsGateway.Blazor.Diagrams.Core.Geometry;
using ThingsGateway.Common.Extension;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

[CategoryNode(Category = "Trigger", ImgUrl = $"{INode.ModuleBasePath}img/ValueChanged.svg", Desc = nameof(DeviceChangedTriggerNode), LocalizerType = typeof(ThingsGateway.Gateway.Application.INode), WidgetType = $"ThingsGateway.Gateway.Razor.DeviceWidget,{INode.FileModulePath}")]
public class DeviceChangedTriggerNode : TextNode, ITriggerNode, IDisposable
{
    public DeviceChangedTriggerNode(string id, Point? position = null) : base(id, position) { Title = "DeviceChangedTriggerNode"; Placeholder = "Device.Placeholder"; }


    private Func<NodeOutput, CancellationToken, Task> Func { get; set; }
    Task ITriggerNode.StartAsync(Func<NodeOutput, CancellationToken, Task> func, CancellationToken cancellationToken)
    {
        Func = func;
        FuncDict.Add(this, func);
        if (!DeviceChangedTriggerNodeDict.TryGetValue(Text ?? string.Empty, out var list))
        {
            var deviceChangedTriggerNodes = new ConcurrentList<DeviceChangedTriggerNode>();
            deviceChangedTriggerNodes.Add(this);
            DeviceChangedTriggerNodeDict.Add(Text, deviceChangedTriggerNodes);
        }
        else
        {
            list.Add(this);
        }
        return Task.CompletedTask;
    }
    public static Dictionary<string, ConcurrentList<DeviceChangedTriggerNode>> DeviceChangedTriggerNodeDict = new();
    public static Dictionary<DeviceChangedTriggerNode, Func<NodeOutput, CancellationToken, Task>> FuncDict = new();

    public static BlockingCollection<DeviceBasicData> DeviceDatas = new();

    static DeviceChangedTriggerNode()
    {
        GlobalData.DeviceStatusChangeEvent -= GlobalData_DeviceStatusChangeEvent;
        Task.Factory.StartNew(RunAsync, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        GlobalData.DeviceStatusChangeEvent += GlobalData_DeviceStatusChangeEvent;
    }

    private static void GlobalData_DeviceStatusChangeEvent(DeviceRuntime deviceRunTime, DeviceBasicData deviceData)
    {
        if (DeviceChangedTriggerNodeDict.TryGetValue(deviceData.Name ?? string.Empty, out var deviceChangedTriggerNodes) && deviceChangedTriggerNodes?.Count > 0)
        {
            if (!DeviceDatas.IsAddingCompleted)
            {
                try
                {
                    DeviceDatas.Add(deviceData);
                    return;
                }
                catch (InvalidOperationException) { }
                catch { }
            }
        }
    }
    static Task RunAsync()
    {
        return DeviceDatas.GetConsumingEnumerable().ParallelForEachStreamedAsync((async (deviceDatas, token) =>
            {
                if (DeviceChangedTriggerNodeDict.TryGetValue(deviceDatas.Name ?? string.Empty, out var valueChangedTriggerNodes))
                {
                    await valueChangedTriggerNodes.ParallelForEachAsync(async (item, token) =>
                     {
                         try
                         {
                             if (FuncDict.TryGetValue(item, out var func))
                             {
                                 item.Logger?.Trace($"Device changed: {item.Text}");
                                 await func.Invoke(new NodeOutput() { Value = deviceDatas }, token).ConfigureAwait(false);
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
        FuncDict.Remove(this);
        if (DeviceChangedTriggerNodeDict.TryGetValue(Text ?? string.Empty, out var list))
        {
            list.Remove(this);
        }
    }

}
