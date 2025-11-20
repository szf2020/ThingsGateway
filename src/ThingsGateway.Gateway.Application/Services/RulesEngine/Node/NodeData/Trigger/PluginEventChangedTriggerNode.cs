
using System.Collections.Concurrent;

using ThingsGateway.Blazor.Diagrams.Core.Geometry;
using ThingsGateway.Common.Extension;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

[CategoryNode(Category = "Trigger", ImgUrl = $"{INode.ModuleBasePath}img/ValueChanged.svg", Desc = nameof(PluginEventChangedTriggerNode), LocalizerType = typeof(ThingsGateway.Gateway.Application.INode), WidgetType = $"ThingsGateway.Gateway.Razor.DeviceWidget,{INode.FileModulePath}")]
public class PluginEventChangedTriggerNode : TextNode, ITriggerNode, IDisposable
{
    public PluginEventChangedTriggerNode(string id, Point? position = null) : base(id, position) { Title = "PluginEventChangedTriggerNode"; Placeholder = "Device.Placeholder"; }


#if !Management
    private Func<NodeOutput, CancellationToken, Task> Func { get; set; }
    Task ITriggerNode.StartAsync(Func<NodeOutput, CancellationToken, Task> func, CancellationToken cancellationToken)
    {
        Func = func;
        FuncDict.Add(this, func);
        if (!DeviceChangedTriggerNodeDict.TryGetValue(Text ?? string.Empty, out var list))
        {
            var deviceChangedTriggerNodes = new ConcurrentList<PluginEventChangedTriggerNode>();
            deviceChangedTriggerNodes.Add(this);
            DeviceChangedTriggerNodeDict.Add(Text, deviceChangedTriggerNodes);
        }
        else
        {
            list.Add(this);
        }
        return Task.CompletedTask;
    }
    public static Dictionary<string, ConcurrentList<PluginEventChangedTriggerNode>> DeviceChangedTriggerNodeDict = new();
    public static Dictionary<PluginEventChangedTriggerNode, Func<NodeOutput, CancellationToken, Task>> FuncDict = new();

    public static BlockingCollection<PluginEventData> DeviceDatas = new();

    static PluginEventChangedTriggerNode()
    {
        GlobalData.PluginEventHandler -= GlobalData_PluginEventHandler;
        Task.Factory.StartNew(RunAsync, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        GlobalData.PluginEventHandler += GlobalData_PluginEventHandler;
    }

    private static void GlobalData_PluginEventHandler(PluginEventData pluginEventData)
    {
        if (DeviceChangedTriggerNodeDict.TryGetValue(pluginEventData.DeviceName ?? string.Empty, out var deviceChangedTriggerNodes) && deviceChangedTriggerNodes?.Count > 0)
        {
            if (!DeviceDatas.IsAddingCompleted)
            {
                try
                {
                    DeviceDatas.Add(pluginEventData);
                    return;
                }
                catch (InvalidOperationException) { }
                catch { }
            }
        }
    }


    static Task RunAsync()
    {
        return DeviceDatas.GetConsumingEnumerable().ParallelForEachStreamedAsync((async (plgunEventData, token) =>
        {
            if (DeviceChangedTriggerNodeDict.TryGetValue(plgunEventData.DeviceName ?? string.Empty, out var valueChangedTriggerNodes))
            {
                await valueChangedTriggerNodes.ParallelForEachAsync(async (item, token) =>
                {
                    try
                    {
                        if (FuncDict.TryGetValue(item, out var func))
                        {
                            item.Logger?.Trace($"Device changed: {item.Text}");
                            await func.Invoke(new NodeOutput() { Value = plgunEventData }, token).ConfigureAwait(false);
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
#else

    Task ITriggerNode.StartAsync(Func<NodeOutput, CancellationToken, Task> func, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }


    public void Dispose()
    {

    }

#endif
}
