//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;

using ThingsGateway.Gateway.Application.Extensions;
using ThingsGateway.NewLife.Extension;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 设备采集报警后台服务
/// </summary>
internal sealed class AlarmTask : IDisposable
{
    private readonly ILogger _logger;
    private ScheduledSyncTask scheduledTask;
    public AlarmTask(ILogger logger)
    {
        _logger = logger;
    }

    public void StartTask(CancellationToken cancellationToken)
    {
        _logger.LogInformation(AppResource.RealAlarmTaskStart);
        scheduledTask = new ScheduledSyncTask(10, DoWork, null, null, cancellationToken);
        scheduledTask.Start();
    }

    public void StopTask()
    {
        _logger.LogInformation(AppResource.RealAlarmTaskStop);
        scheduledTask?.Stop();
    }
    public void Dispose()
    {
        StopTask();
        scheduledTask?.SafeDispose();
    }

    #region 核心实现

    /// <summary>
    /// 获取bool报警类型
    /// </summary>
    /// <param name="tag">要检查的变量</param>
    /// <param name="limit">报警限制值</param>
    /// <param name="expressions">报警约束表达式</param>
    /// <param name="text">报警文本</param>
    /// <returns>报警类型枚举</returns>
    private static AlarmTypeEnum? GetBoolAlarmCode(VariableRuntime tag, out string limit, out string expressions, out string text)
    {
        limit = string.Empty; // 初始化报警限制值为空字符串
        expressions = string.Empty; // 初始化报警约束表达式为空字符串
        text = string.Empty; // 初始化报警文本为空字符串

        if (tag?.Value == null) // 检查变量是否为null或其值为null
        {
            return null; // 如果是，则返回null
        }

        if (tag.AlarmPropertys.BoolCloseAlarmEnable && !tag.Value.ToBoolean(true)) // 检查是否启用了关闭报警功能，并且变量的布尔值为false
        {
            limit = false.ToString(); // 将报警限制值设置为"false"
            expressions = tag.AlarmPropertys.BoolCloseRestrainExpressions!; // 获取关闭报警的约束表达式
            text = tag.AlarmPropertys.BoolCloseAlarmText!; // 获取关闭报警时的报警文本
            return AlarmTypeEnum.Close; // 返回关闭报警类型枚举
        }

        if (tag.AlarmPropertys.BoolOpenAlarmEnable && tag.Value.ToBoolean(false)) // 检查是否启用了开启报警功能，并且变量的布尔值为true
        {
            limit = true.ToString(); // 将报警限制值设置为"true"
            expressions = tag.AlarmPropertys.BoolOpenRestrainExpressions!; // 获取开启报警的约束表达式
            text = tag.AlarmPropertys.BoolOpenAlarmText!; // 获取开启报警时的报警文本
            return AlarmTypeEnum.Open; // 返回开启报警类型枚举
        }

        return null; // 如果不符合任何报警条件，则返回null
    }

    /// <summary>
    /// 获取自定义报警类型
    /// </summary>
    /// <param name="tag">要检查的变量</param>
    /// <param name="limit">报警限制值</param>
    /// <param name="expressions">报警约束表达式</param>
    /// <param name="text">报警文本</param>
    /// <returns>报警类型枚举</returns>
    private static AlarmTypeEnum? GetCustomAlarmDegree(VariableRuntime tag, out string limit, out string expressions, out string text)
    {
        limit = string.Empty; // 初始化报警限制值为空字符串
        expressions = string.Empty; // 初始化报警约束表达式为空字符串
        text = string.Empty; // 初始化报警文本为空字符串

        if (tag?.Value == null) // 检查变量是否为null或其值为null
        {
            return null; // 如果是，则返回null
        }

        if (tag.AlarmPropertys.CustomAlarmEnable) // 检查是否启用了自定义报警功能
        {
            // 调用变量的CustomAlarmCode属性的GetExpressionsResult方法，传入变量的值，获取报警表达式的计算结果
            var result = tag.AlarmPropertys.CustomAlarmCode.GetExpressionsResult(tag.Value, tag.LogMessage);

            if (result is bool boolResult) // 检查计算结果是否为布尔类型
            {
                if (boolResult) // 如果计算结果为true
                {
                    limit = tag.AlarmPropertys.CustomAlarmCode; // 将报警限制值设置为自定义报警代码
                    expressions = tag.AlarmPropertys.CustomRestrainExpressions!; // 获取自定义报警时的报警约束表达式
                    text = tag.AlarmPropertys.CustomAlarmText!; // 获取自定义报警时的报警文本
                    return AlarmTypeEnum.Custom; // 返回自定义报警类型枚举
                }
            }
        }

        return null; // 如果不符合自定义报警条件，则返回null
    }

    /// <summary>
    /// 获取decimal类型的报警类型
    /// </summary>
    /// <param name="tag">要检查的变量</param>
    /// <param name="limit">报警限制值</param>
    /// <param name="expressions">报警约束表达式</param>
    /// <param name="text">报警文本</param>
    /// <returns>报警类型枚举</returns>
    private static AlarmTypeEnum? GetDecimalAlarmDegree(VariableRuntime tag, out string limit, out string expressions, out string text)
    {
        limit = string.Empty; // 初始化报警限制值为空字符串
        expressions = string.Empty; // 初始化报警约束表达式为空字符串
        text = string.Empty; // 初始化报警文本为空字符串

        if (tag?.Value == null) // 检查变量是否为null或其值为null
        {
            return null; // 如果是，则返回null
        }

        // 检查是否启用了高高报警功能，并且变量的值大于高高报警的限制值
        if (tag.AlarmPropertys.HHAlarmEnable && tag.Value.ToDecimal() > tag.AlarmPropertys.HHAlarmCode)
        {
            limit = tag.AlarmPropertys.HHAlarmCode.ToString()!; // 将报警限制值设置为高高报警的限制值
            expressions = tag.AlarmPropertys.HHRestrainExpressions!; // 获取高高报警的约束表达式
            text = tag.AlarmPropertys.HHAlarmText!; // 获取高高报警时的报警文本
            return AlarmTypeEnum.HH; // 返回高高报警类型枚举
        }

        // 检查是否启用了高报警功能，并且变量的值大于高报警的限制值
        if (tag.AlarmPropertys.HAlarmEnable && tag.Value.ToDecimal() > tag.AlarmPropertys.HAlarmCode)
        {
            limit = tag.AlarmPropertys.HAlarmCode.ToString()!; // 将报警限制值设置为高报警的限制值
            expressions = tag.AlarmPropertys.HRestrainExpressions!; // 获取高报警的约束表达式
            text = tag.AlarmPropertys.HAlarmText!; // 获取高报警时的报警文本
            return AlarmTypeEnum.H; // 返回高报警类型枚举
        }

        // 检查是否启用了低低报警功能，并且变量的值小于低低报警的限制值
        if (tag.AlarmPropertys.LLAlarmEnable && tag.Value.ToDecimal() < tag.AlarmPropertys.LLAlarmCode)
        {
            limit = tag.AlarmPropertys.LLAlarmCode.ToString()!; // 将报警限制值设置为低低报警的限制值
            expressions = tag.AlarmPropertys.LLRestrainExpressions!; // 获取低低报警的约束表达式
            text = tag.AlarmPropertys.LLAlarmText!; // 获取低低报警时的报警文本
            return AlarmTypeEnum.LL; // 返回低低报警类型枚举
        }

        // 检查是否启用了低报警功能，并且变量的值小于低报警的限制值
        if (tag.AlarmPropertys.LAlarmEnable && tag.Value.ToDecimal() < tag.AlarmPropertys.LAlarmCode)
        {
            limit = tag.AlarmPropertys.LAlarmCode.ToString()!; // 将报警限制值设置为低报警的限制值
            expressions = tag.AlarmPropertys.LRestrainExpressions!; // 获取低报警的约束表达式
            text = tag.AlarmPropertys.LAlarmText!; // 获取低报警时的报警文本
            return AlarmTypeEnum.L; // 返回低报警类型枚举
        }

        return null; // 如果不符合任何报警条件，则返回null
    }

    /// <summary>
    /// 对变量进行报警分析，并根据需要触发相应的报警事件或恢复事件。
    /// </summary>
    /// <param name="item">要进行报警分析的变量</param>
    private static void AlarmAnalysis(VariableRuntime item)
    {
        string limit; // 报警限制值
        string ex; // 报警约束表达式
        string text; // 报警文本
        AlarmTypeEnum? alarmEnum; // 报警类型枚举
        int delay = item.AlarmPropertys.AlarmDelay; // 获取报警延迟时间

        // 检查变量的数据类型
        if (item.Value?.GetType() == typeof(bool))
        {
            // 如果数据类型为布尔型，则调用GetBoolAlarmCode方法获取布尔型报警类型及相关信息
            alarmEnum = GetBoolAlarmCode(item, out limit, out ex, out text);
        }
        else
        {
            // 如果数据类型为非布尔型，则调用GetDecimalAlarmDegree方法获取数值型报警类型及相关信息
            alarmEnum = GetDecimalAlarmDegree(item, out limit, out ex, out text);
        }

        // 如果未获取到报警类型，则尝试获取自定义报警类型
        if (alarmEnum == null)
        {
            alarmEnum = GetCustomAlarmDegree(item, out limit, out ex, out text);
        }

        if (alarmEnum == null)
        {
            // 如果仍未获取到报警类型，则触发需恢复报警事件（如果存在）
            AlarmChange(item, null, text, true, alarmEnum, delay);
        }
        else
        {
            // 如果获取到了报警类型，则需触发报警事件或更新报警状态

            if (!string.IsNullOrEmpty(ex))
            {
                // 如果存在报警约束表达式，则计算表达式结果，以确定是否触发报警事件
                var data = ex.GetExpressionsResult(item.Value, item.LogMessage);
                if (data is bool result)
                {
                    if (result)
                    {
                        // 如果表达式结果为true，则触发报警事件
                        AlarmChange(item, limit, text, false, alarmEnum, delay);
                    }
                    else
                    {
                        AlarmChange(item, limit, text, true, alarmEnum, delay);
                    }
                }
            }
            else
            {
                // 如果不存在报警约束表达式，则直接触发报警事件
                AlarmChange(item, limit, text, false, alarmEnum, delay);
            }
        }
    }

    /// <summary>
    /// 根据报警事件类型进行相应的处理操作，包括触发报警事件或更新报警状态。
    /// </summary>
    /// <param name="item">要处理的变量</param>
    /// <param name="limit">报警限制值</param>
    /// <param name="text">报警文本</param>
    /// <param name="finish">是否恢复</param>
    /// <param name="alarmEnum">报警类型枚举</param>
    /// <param name="delay">报警延时</param>
    private static void AlarmChange(VariableRuntime item, object limit, string text, bool finish, AlarmTypeEnum? alarmEnum, int delay)
    {
        lock (item.AlarmRuntimePropertys.AlarmLockObject)
        {
            bool changed = false;
            if (finish)
            {
                // 如果是需恢复报警事件
                // 如果实时报警列表中不存在该变量，则直接返回
                if (!GlobalData.RealAlarmIdVariables.ContainsKey(item.Id))
                {
                    return;
                }
            }
            else
            {
                if (item.AlarmRuntimePropertys.EventType != EventTypeEnum.Confirm)
                    item.AlarmRuntimePropertys.AlarmConfirm = false;
                // 如果是触发报警事件
                // 在实时报警列表中查找该变量
                if (GlobalData.RealAlarmIdVariables.TryGetValue(item.Id, out var variable) && (variable.EventType == EventTypeEnum.Alarm || variable.EventType == EventTypeEnum.Confirm))
                {
                    // 如果变量已经处于相同的报警类型，则直接返回
                    if (item.AlarmRuntimePropertys.AlarmType == alarmEnum)
                        return;
                }
            }

            // 更新变量的报警信息和事件时间
            if (!finish)
            {
                var now = DateTime.Now;
                //添加报警延时策略
                if (delay > 0)
                {
                    if (item.AlarmRuntimePropertys.EventType != EventTypeEnum.Alarm && item.AlarmRuntimePropertys.EventType != EventTypeEnum.PrepareAlarm)
                    {
                        item.AlarmRuntimePropertys.EventType = EventTypeEnum.PrepareAlarm;//准备报警
                        item.AlarmRuntimePropertys.PrepareAlarmEventTime = now;
                    }
                    else
                    {
                        if (item.AlarmRuntimePropertys.EventType == EventTypeEnum.PrepareAlarm)
                        {
                            if ((now - item.AlarmRuntimePropertys.PrepareAlarmEventTime!.Value).TotalMilliseconds > delay)
                            {
                                //超过延时时间，触发报警
                                item.AlarmRuntimePropertys.EventType = EventTypeEnum.Alarm;
                                item.AlarmRuntimePropertys.AlarmTime = now;
                                item.AlarmRuntimePropertys.EventTime = now;
                                item.AlarmRuntimePropertys.AlarmType = alarmEnum;
                                item.AlarmRuntimePropertys.AlarmLimit = limit.ToString();
                                item.AlarmRuntimePropertys.AlarmCode = item.Value.ToString();
                                item.AlarmRuntimePropertys.RecoveryCode = string.Empty;
                                item.AlarmRuntimePropertys.AlarmText = text;
                                item.AlarmRuntimePropertys.PrepareAlarmEventTime = null;

                                changed = true;
                            }
                        }
                        else if (item.AlarmRuntimePropertys.EventType == EventTypeEnum.Alarm && item.AlarmRuntimePropertys.AlarmType != alarmEnum)
                        {
                            //报警类型改变，重新计时
                            if (item.AlarmRuntimePropertys.PrepareAlarmEventTime == null)
                                item.AlarmRuntimePropertys.PrepareAlarmEventTime = now;
                            if ((now - item.AlarmRuntimePropertys.PrepareAlarmEventTime!.Value).TotalMilliseconds > delay)
                            {
                                //超过延时时间，触发报警
                                item.AlarmRuntimePropertys.EventType = EventTypeEnum.Alarm;
                                item.AlarmRuntimePropertys.AlarmTime = now;
                                item.AlarmRuntimePropertys.EventTime = now;
                                item.AlarmRuntimePropertys.AlarmType = alarmEnum;
                                item.AlarmRuntimePropertys.AlarmLimit = limit.ToString();
                                item.AlarmRuntimePropertys.AlarmCode = item.Value.ToString();
                                item.AlarmRuntimePropertys.RecoveryCode = string.Empty;
                                item.AlarmRuntimePropertys.AlarmText = text;
                                item.AlarmRuntimePropertys.PrepareAlarmEventTime = null;
                                changed = true;
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                else
                {
                    // 如果是触发报警事件
                    item.AlarmRuntimePropertys.EventType = EventTypeEnum.Alarm;
                    item.AlarmRuntimePropertys.AlarmTime = now;
                    item.AlarmRuntimePropertys.EventTime = now;
                    item.AlarmRuntimePropertys.AlarmType = alarmEnum;
                    item.AlarmRuntimePropertys.AlarmLimit = limit.ToString();
                    item.AlarmRuntimePropertys.AlarmCode = item.Value.ToString();
                    item.AlarmRuntimePropertys.RecoveryCode = string.Empty;
                    item.AlarmRuntimePropertys.AlarmText = text;
                    item.AlarmRuntimePropertys.PrepareAlarmEventTime = null;
                    changed = true;
                }
            }
            else
            {
                var now = DateTime.Now;
                //添加报警延时策略
                if (delay > 0)
                {
                    if (item.AlarmRuntimePropertys.EventType != EventTypeEnum.Finish && item.AlarmRuntimePropertys.EventType != EventTypeEnum.PrepareFinish)
                    {
                        item.AlarmRuntimePropertys.EventType = EventTypeEnum.PrepareFinish;
                        item.AlarmRuntimePropertys.PrepareFinishEventTime = now;
                    }
                    else
                    {
                        if (item.AlarmRuntimePropertys.EventType == EventTypeEnum.PrepareFinish)
                        {
                            if ((now - item.AlarmRuntimePropertys.PrepareFinishEventTime!.Value).TotalMilliseconds > delay)
                            {
                                if (GlobalData.RealAlarmIdVariables.TryGetValue(item.Id, out var oldAlarm))
                                {
                                    item.AlarmRuntimePropertys.AlarmType = oldAlarm.AlarmType;
                                    item.AlarmRuntimePropertys.AlarmLimit = oldAlarm.AlarmLimit;
                                    item.AlarmRuntimePropertys.AlarmCode = oldAlarm.AlarmCode;
                                    item.AlarmRuntimePropertys.RecoveryCode = item.Value.ToString();
                                    item.AlarmRuntimePropertys.AlarmText = oldAlarm.AlarmText;
                                    if (item.AlarmRuntimePropertys.EventType != EventTypeEnum.Finish)
                                    {
                                        item.AlarmRuntimePropertys.FinishTime = now;
                                        item.AlarmRuntimePropertys.EventTime = now;
                                    }
                                    item.AlarmRuntimePropertys.EventType = EventTypeEnum.Finish;
                                    item.AlarmRuntimePropertys.PrepareFinishEventTime = null;
                                    changed = true;
                                }
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                else
                {
                    // 如果是需恢复报警事件
                    // 获取旧的报警信息
                    if (item.AlarmRuntimePropertys.EventType != EventTypeEnum.Finish && item.AlarmRuntimePropertys.EventType != EventTypeEnum.PrepareFinish)
                    {
                        if (GlobalData.RealAlarmIdVariables.TryGetValue(item.Id, out var oldAlarm))
                        {
                            item.AlarmRuntimePropertys.AlarmType = oldAlarm.AlarmType;
                            item.AlarmRuntimePropertys.AlarmLimit = oldAlarm.AlarmLimit;
                            item.AlarmRuntimePropertys.AlarmCode = oldAlarm.AlarmCode;
                            item.AlarmRuntimePropertys.RecoveryCode = item.Value.ToString();
                            item.AlarmRuntimePropertys.AlarmText = oldAlarm.AlarmText;
                            if (item.AlarmRuntimePropertys.EventType != EventTypeEnum.Finish)
                            {
                                item.AlarmRuntimePropertys.FinishTime = now;
                                item.AlarmRuntimePropertys.EventTime = now;
                            }
                            item.AlarmRuntimePropertys.EventType = EventTypeEnum.Finish;
                            item.AlarmRuntimePropertys.PrepareFinishEventTime = null;
                            changed = true;
                        }
                    }
                }
            }

            // 触发报警变化事件
            if (changed)
            {
                if (item.AlarmRuntimePropertys.EventType == EventTypeEnum.Alarm)
                {
                    // 如果是触发报警事件
                    //lock (GlobalData. RealAlarmVariables)
                    {
                        // 从实时报警列表中移除旧的报警信息，并添加新的报警信息
                        GlobalData.RealAlarmIdVariables.AddOrUpdate(item.Id, a => item.AdaptAlarmVariable(), (a, b) => item.AdaptAlarmVariable());
                    }
                }
                else if (item.AlarmRuntimePropertys.EventType == EventTypeEnum.Finish)
                {

                    // 如果是需恢复报警事件，则从实时报警列表中移除该变量
                    if (item.AlarmRuntimePropertys.AlarmConfirm)
                    {
                        GlobalData.RealAlarmIdVariables.TryRemove(item.Id, out _);
                        item.AlarmRuntimePropertys.EventType = EventTypeEnum.ConfirmAndFinish;
                    }
                    else
                    {
                        GlobalData.RealAlarmIdVariables.AddOrUpdate(item.Id, a => item.AdaptAlarmVariable(), (a, b) => item.AdaptAlarmVariable());
                    }

                }
                GlobalData.AlarmChange(item.AdaptAlarmVariable());
            }
        }
    }

    public void ConfirmAlarm(long variableId)
    {
        // 如果是确认报警事件
        if (GlobalData.AlarmEnableIdVariables.TryGetValue(variableId, out var item))
        {
            lock (item.AlarmRuntimePropertys.AlarmLockObject)
            {
                item.AlarmRuntimePropertys.AlarmConfirm = true;
                item.AlarmRuntimePropertys.ConfirmTime = DateTime.Now;
                item.AlarmRuntimePropertys.EventTime = item.AlarmRuntimePropertys.ConfirmTime;

                if (item.AlarmRuntimePropertys.EventType == EventTypeEnum.Finish)
                {
                    item.AlarmRuntimePropertys.EventType = EventTypeEnum.ConfirmAndFinish;
                    GlobalData.RealAlarmIdVariables.TryRemove(variableId, out _);
                }
                else
                {
                    item.AlarmRuntimePropertys.EventType = EventTypeEnum.Confirm;
                    GlobalData.RealAlarmIdVariables.AddOrUpdate(variableId, a => item.AdaptAlarmVariable(), (a, b) => item.AdaptAlarmVariable());
                }

                GlobalData.AlarmChange(item.AdaptAlarmVariable());
            }
        }
    }

    #endregion 核心实现

    ParallelOptions ParallelOptions = new()
    {
        MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount / 2)
    };

    /// <summary>
    /// 执行工作任务，对设备变量进行报警分析。
    /// </summary>
    /// <param name="state"></param>
    /// <param name="cancellation">取消任务的 CancellationToken</param>
    private void DoWork(object? state, CancellationToken cancellation)
    {
        try
        {
            if (!GlobalData.StartBusinessChannelEnable)
                return;

            //System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

            if (scheduledTask.Period < 100 && scheduledTask.Period > 1 && GlobalData.AlarmEnableIdVariables.Count > 50000)
            {
                scheduledTask.Change(100, 100);
            }
            ParallelOptions.CancellationToken = cancellation;
            // 遍历设备变量列表
            if (!GlobalData.AlarmEnableIdVariables.IsEmpty)
            {
                // 使用 Parallel.ForEach 执行指定的操作
                Parallel.ForEach(GlobalData.AlarmEnableIdVariables, ParallelOptions, Analysis);
            }

            else
            {
                scheduledTask.SetNext(5000); // 如果没有启用报警的变量，则设置下次执行时间为5秒后
            }

            //stopwatch.Stop();
            //_logger.LogInformation("报警分析耗时：" + stopwatch.ElapsedMilliseconds + "ms");
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Alarm analysis fail");
        }
    }

    private static void Analysis(KeyValuePair<long, VariableRuntime> item, ParallelLoopState state, long index)
    {
        // 如果取消请求已经被触发，则结束任务
        if (state.ShouldExitCurrentIteration)
            return;

        // 如果该变量的报警功能未启用，则跳过该变量
        if (!item.Value.AlarmEnable)
            return;

        // 如果该变量离线，则跳过该变量
        if (!item.Value.IsOnline)
            return;

        // 对该变量进行报警分析
        AlarmAnalysis(item.Value);
    }
}
