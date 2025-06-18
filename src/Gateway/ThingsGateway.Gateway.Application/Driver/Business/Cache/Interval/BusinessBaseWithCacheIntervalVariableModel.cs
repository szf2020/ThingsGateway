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
/// 抽象类 <see cref="BusinessBaseWithCacheIntervalVariableModel{VarModel}"/>，表示具有缓存间隔功能的业务基类，其中 T 代表变量模型。
/// </summary>
/// <typeparam name="VarModel">变量模型类型</typeparam>
public abstract class BusinessBaseWithCacheIntervalVariableModel<VarModel> : BusinessBaseWithCacheVariableModel<VarModel>
{

    /// <summary>
    /// 获取具体业务属性的缓存设置。
    /// </summary>
    protected sealed override BusinessPropertyWithCache _businessPropertyWithCache => _businessPropertyWithCacheInterval;

    /// <summary>
    /// 获取具体业务属性的缓存间隔设置。
    /// </summary>
    protected abstract BusinessPropertyWithCacheInterval _businessPropertyWithCacheInterval { get; }

    protected internal override async Task InitChannelAsync(IChannel? channel, CancellationToken cancellationToken)
    {
        // 注册变量值变化事件处理程序
        if (_businessPropertyWithCacheInterval.BusinessUpdateEnum != BusinessUpdateEnum.Interval)
        {
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

        // 触发一次变量值变化事件
        IdVariableRuntimes.ForEach(a =>
        {
            if (a.Value.IsOnline && _businessPropertyWithCacheInterval.BusinessUpdateEnum != BusinessUpdateEnum.Interval)
                VariableValueChange(a.Value, a.Value.Adapt<VariableBasicData>());
        });
    }

    /// <summary>
    /// 释放资源的方法。
    /// </summary>
    /// <param name="disposing">是否释放托管资源</param>
    protected override void Dispose(bool disposing)
    {
        GlobalData.VariableValueChangeEvent -= VariableValueChange;
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
        }

    }

    protected override List<IScheduledTask> ProtectedGetTasks(CancellationToken cancellationToken)
    {
        var list = base.ProtectedGetTasks(cancellationToken);
        list.Add(ScheduledTaskHelper.GetTask(_businessPropertyWithCacheInterval.BusinessInterval, IntervalInsert, null, LogMessage, cancellationToken));
        return list;
    }

    /// <summary>
    /// 当变量状态变化时发生，通常需要执行<see cref="BusinessBaseWithCacheVariableModel{T}.AddQueueVarModel(CacheDBItem{T})"/>。
    /// </summary>
    /// <param name="variableRuntime">变量运行时对象</param>
    /// <param name="variable">变量运行时对象</param>
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
    /// 当变量值发生变化时调用的方法。
    /// </summary>
    /// <param name="variableRuntime">变量运行时对象</param>
    /// <param name="variable">变量数据</param>
    protected void VariableValueChange(VariableRuntime variableRuntime, VariableBasicData variable)
    {
        if (CurrentDevice.Pause == true)
            return;
        //if (_businessPropertyWithCacheInterval?.IsInterval != true)
        {
            //筛选
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
                IdVariableRuntimes.ForEach(a =>
                {
                    if (a.Value.IsOnline && _businessPropertyWithCacheInterval.BusinessUpdateEnum != BusinessUpdateEnum.Interval)
                        VariableValueChange(a.Value, a.Value.Adapt<VariableBasicData>());
                });
            }
        }
    }

}
