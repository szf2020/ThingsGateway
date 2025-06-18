//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using ThingsGateway.Extension.Generic;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 业务插件的抽象基类，用于实现设备和变量之间的间隔上传功能。
/// </summary>
/// <typeparam name="VarModel">变量数据类型</typeparam>
/// <typeparam name="DevModel">设备数据类型</typeparam>
public abstract class BusinessBaseWithCacheIntervalDeviceModel<VarModel, DevModel> : BusinessBaseWithCacheDeviceModel<VarModel, DevModel>
{
    /// <summary>
    /// 获取具体业务属性的缓存设置。
    /// </summary>
    protected sealed override BusinessPropertyWithCache _businessPropertyWithCache => _businessPropertyWithCacheInterval;

    /// <summary>
    /// 获取业务属性与缓存间隔的抽象属性。
    /// </summary>
    protected abstract BusinessPropertyWithCacheInterval _businessPropertyWithCacheInterval { get; }



    protected internal override async Task InitChannelAsync(IChannel? channel, CancellationToken cancellationToken)
    {
        // 如果不是间隔上传，则订阅全局变量值改变事件和设备状态改变事件，并触发一次事件处理
        if (_businessPropertyWithCacheInterval.BusinessUpdateEnum != BusinessUpdateEnum.Interval)
        {
            GlobalData.DeviceStatusChangeEvent += DeviceStatusChange;
            GlobalData.VariableValueChangeEvent += VariableValueChange;

        }

        await base.InitChannelAsync(channel, cancellationToken).ConfigureAwait(false);
    }
    public override async Task AfterVariablesChangedAsync(CancellationToken cancellationToken)
    {
        // 如果业务属性指定了全部变量，则设置当前设备的变量运行时列表和采集设备列表
        if (_businessPropertyWithCacheInterval.IsAllVariable)
        {
            LogMessage?.LogInformation("Refresh variable");
            IdVariableRuntimes.Clear();
            IdVariableRuntimes.AddRange(GlobalData.GetEnableVariables().ToDictionary(a => a.Id));
            CollectDevices = GlobalData.GetEnableDevices().Where(a => a.IsCollect == true).ToDictionary(a => a.Id);

            VariableRuntimeGroups = IdVariableRuntimes.GroupBy(a => a.Value.BusinessGroup ?? string.Empty).ToDictionary(a => a.Key, a => a.Select(a => a.Value).ToList());

        }
        else
        {
            await base.AfterVariablesChangedAsync(cancellationToken).ConfigureAwait(false);
        }


        CollectDevices?.ForEach(a =>
        {
            if (a.Value.DeviceStatus == DeviceStatusEnum.OnLine && _businessPropertyWithCacheInterval.BusinessUpdateEnum != BusinessUpdateEnum.Interval)
                DeviceStatusChange(a.Value, a.Value.Adapt<DeviceBasicData>());
        });
        IdVariableRuntimes.ForEach(a =>
        {
            if (a.Value.IsOnline && _businessPropertyWithCacheInterval.BusinessUpdateEnum != BusinessUpdateEnum.Interval)
                VariableValueChange(a.Value, a.Value.Adapt<VariableBasicData>());
        });
    }


    /// <summary>
    /// 设备状态变化时发生的虚拟方法，用于处理设备状态变化事件。
    /// </summary>
    /// <param name="deviceRuntime">设备运行时对象</param>
    /// <param name="deviceData">设备数据对象</param>
    protected virtual void DeviceChange(DeviceRuntime deviceRuntime, DeviceBasicData deviceData)
    {
    }
    /// <summary>
    /// 当设备状态定时变化时触发此方法。如果不需要进行设备上传，则可以忽略此方法。通常情况下，需要在此方法中执行 <see cref="BusinessBaseWithCacheDeviceModel{T,T2}.AddQueueDevModel(CacheDBItem{T2})"/> 方法。
    /// </summary>
    /// <param name="deviceRuntime">设备运行时信息</param>
    /// <param name="deviceData">设备数据</param>
    protected virtual void DeviceTimeInterval(DeviceRuntime deviceRuntime, DeviceBasicData deviceData)
    {
        // 在设备状态变化时执行的自定义逻辑
    }
    /// <summary>
    /// 释放资源的方法，释放插件相关资源。
    /// </summary>
    /// <param name="disposing">是否释放托管资源</param>
    protected override void Dispose(bool disposing)
    {
        // 注销全局变量值改变事件和设备状态改变事件的订阅
        GlobalData.VariableValueChangeEvent -= VariableValueChange;
        GlobalData.DeviceStatusChangeEvent -= DeviceStatusChange;

        // 清空内存队列
        _memoryDevModelQueue.Clear();
        _memoryVarModelQueue.Clear();
        _memoryVarModelsQueue.Clear();
        base.Dispose(disposing);
    }


    /// <summary>
    /// 间隔上传数据的方法
    /// </summary>
    protected void IntervalInsert(object? state, CancellationToken cancellationToken)
    {
        if (CurrentDevice.Pause == true)
        {
            return;
        }

        // 如果业务属性的缓存为间隔上传，则根据定时器间隔执行相应操作
        if (_businessPropertyWithCacheInterval.BusinessUpdateEnum != BusinessUpdateEnum.Change)
        {
            try
            {
                if (LogMessage?.LogLevel <= LogLevel.Debug)
                    LogMessage?.LogDebug($"Interval  {typeof(VarModel).Name}  data, count {IdVariableRuntimes.Count}");
                // 上传所有变量信息
                var variableRuntimes = IdVariableRuntimes.Select(a => a.Value);
                VariableTimeInterval(variableRuntimes, variableRuntimes.Adapt<List<VariableBasicData>>());
            }
            catch (Exception ex)
            {
                LogMessage?.LogWarning(ex, AppResource.IntervalInsertVariableFail);
            }
            try
            {
                if (CollectDevices != null)
                {
                    if (LogMessage?.LogLevel <= LogLevel.Debug)
                        LogMessage?.LogDebug($"Interval  {typeof(DevModel).Name}  data, count {CollectDevices.Count}");
                    // 上传所有设备信息
                    foreach (var deviceRuntime in CollectDevices.Select(a => a.Value))
                    {
                        DeviceTimeInterval(deviceRuntime, deviceRuntime.Adapt<DeviceBasicData>());
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage?.LogWarning(ex, AppResource.IntervalInsertDeviceFail);
            }
        }

    }

    protected override List<IScheduledTask> ProtectedGetTasks(CancellationToken cancellationToken)
    {
        var list = base.ProtectedGetTasks(cancellationToken);
        list.Add(ScheduledTaskHelper.GetTask(_businessPropertyWithCacheInterval.BusinessInterval, IntervalInsert, null, LogMessage, cancellationToken));
        return list;
    }
    /// <summary>
    /// 变量状态变化时发生的虚拟方法，用于处理变量状态变化事件。
    /// </summary>
    /// <param name="variableRuntime">变量运行时对象</param>
    /// <param name="variable">变量数据对象</param>
    protected virtual void VariableChange(VariableRuntime variableRuntime, VariableBasicData variable)
    {
    }
    /// <summary>
    /// 当变量定时变化时触发此方法。如果不需要进行变量上传，则可以忽略此方法。通常情况下，需要在此方法中执行 <see cref="BusinessBaseWithCacheVariableModel{T}.AddQueueVarModel(CacheDBItem{T})"/> 方法。
    /// </summary>
    /// <param name="variableRuntimes">变量运行时信息</param>
    /// <param name="variables">变量数据</param>
    protected virtual void VariableTimeInterval(IEnumerable<VariableRuntime> variableRuntimes, List<VariableBasicData> variables)
    {
        // 在变量状态变化时执行的自定义逻辑
    }
    /// <summary>
    /// 设备状态改变时的事件处理方法。
    /// </summary>
    /// <param name="deviceRuntime">设备运行时对象</param>
    /// <param name="deviceData">设备数据对象</param>
    private void DeviceStatusChange(DeviceRuntime deviceRuntime, DeviceBasicData deviceData)
    {
        // 如果当前设备已停止运行，则直接返回，不进行处理
        if (CurrentDevice.Pause == true)
            return;

        // 如果业务属性不是间隔上传，则执行设备状态改变的处理逻辑
        //if (_businessPropertyWithCacheInterval?.IsInterval != true)
        {
            // 检查当前设备集合中是否包含该设备，并进行相应处理
            if (CollectDevices?.ContainsKey(deviceRuntime.Id) == true)
                DeviceChange(deviceRuntime, deviceData);
        }
    }

    /// <summary>
    /// 变量值改变时的事件处理方法。
    /// </summary>
    /// <param name="variableRuntime">变量运行时对象</param>
    /// <param name="variable">变量数据对象</param>
    private void VariableValueChange(VariableRuntime variableRuntime, VariableBasicData variable)
    {
        // 如果当前设备已停止运行，则直接返回，不进行处理
        if (CurrentDevice.Pause == true)
            return;

        // 如果业务属性不是间隔上传，则执行变量状态改变的处理逻辑
        //if (_businessPropertyWithCacheInterval?.IsInterval != true)
        {
            // 检查当前设备是否包含该变量，并进行相应处理
            if (IdVariableRuntimes.ContainsKey(variableRuntime.Id))
                VariableChange(variableRuntime, variable);
        }
    }

    public override void PauseThread(bool pause)
    {
        lock (this)
        {
            var oldV = CurrentDevice.Pause;
            base.PauseThread(pause);
            if (!pause && oldV != pause)
            {
                CollectDevices?.ForEach(a =>
                {
                    if (a.Value.DeviceStatus == DeviceStatusEnum.OnLine && _businessPropertyWithCacheInterval.BusinessUpdateEnum != BusinessUpdateEnum.Interval)
                        DeviceStatusChange(a.Value, a.Value.Adapt<DeviceBasicData>());
                });
                IdVariableRuntimes.ForEach(a =>
                {
                    if (a.Value.IsOnline && _businessPropertyWithCacheInterval.BusinessUpdateEnum != BusinessUpdateEnum.Interval)
                        VariableValueChange(a.Value, a.Value.Adapt<VariableBasicData>());
                });
            }
        }
    }

}
