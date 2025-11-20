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

using System.Collections.Concurrent;
using System.Text.Json.Nodes;

using ThingsGateway.Common.Extension;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// <para></para>
/// 采集插件，继承实现不同PLC通讯
/// <para></para>
/// </summary>
public abstract class CollectFoundationBase : CollectBase
{
    /// <summary>
    /// 底层驱动，有可能为null
    /// </summary>
    public virtual IDevice? FoundationDevice { get; }



    public override string ToString()
    {
        return FoundationDevice?.ToString() ?? base.ToString();
    }

    /// <inheritdoc/>
    protected override async Task DisposeAsync(bool disposing)
    {
        if (FoundationDevice != null)
            await FoundationDevice.SafeDisposeAsync().ConfigureAwait(false);
        await base.DisposeAsync(disposing).ConfigureAwait(false);
    }




    public override string GetAddressDescription()
    {
        return FoundationDevice?.GetAddressDescription();
    }
#if !Management

    /// <summary>
    /// 是否连接成功
    /// </summary>
    public override bool IsConnected()
    {
        return FoundationDevice?.OnLine == true;
    }

    /// <summary>
    /// 开始通讯执行的方法
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override async Task ProtectedStartAsync(CancellationToken cancellationToken)
    {
        if (FoundationDevice != null)
        {
            await FoundationDevice.ConnectAsync(cancellationToken).ConfigureAwait(false);
        }
    }


    protected override ValueTask TestOnline(object? state, CancellationToken cancellationToken)
    {
        return TestOnline(this, cancellationToken);


        static async PooledValueTask TestOnline(CollectFoundationBase @this, CancellationToken cancellationToken)
        {
            if (@this.FoundationDevice != null)
            {
                if (!@this.FoundationDevice.OnLine)
                {
                    if (!@this.FoundationDevice.DisposedValue || @this.FoundationDevice.Channel?.DisposedValue != false) return;
                    Exception exception = null;
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            if (!@this.FoundationDevice.DisposedValue || @this.FoundationDevice.Channel?.DisposedValue != false) return;

                            await @this.FoundationDevice.ConnectAsync(cancellationToken).ConfigureAwait(false);

                            if (@this.CurrentDevice.DeviceStatusChangeTime < TimerX.Now.AddMinutes(-1))
                            {
                                await Task.Delay(30000, cancellationToken).ConfigureAwait(false);
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                    if (@this.FoundationDevice.OnLine == false && exception != null)
                    {
                        foreach (var item in @this.CurrentDevice.VariableSourceReads)
                        {
                            if (item.LastErrorMessage != exception.Message)
                            {
                                if (!cancellationToken.IsCancellationRequested)
                                    @this.LogMessage?.LogWarning(exception, string.Format(AppResource.CollectFail, @this.DeviceName, item?.RegisterAddress, item?.Length, exception.Message));
                            }
                            item.LastErrorMessage = exception.Message;
                            @this.CurrentDevice.SetDeviceStatus(TimerX.Now, null, exception.Message);
                            var time = DateTime.Now;
                            item.Variables.ForEach(a => a.SetValue(null, time, isOnline: false));
                        }
                        foreach (var item in @this.CurrentDevice.ReadVariableMethods)
                        {
                            if (item.LastErrorMessage != exception.Message)
                            {
                                if (!cancellationToken.IsCancellationRequested)
                                    @this.LogMessage?.LogWarning(exception, string.Format(AppResource.MethodFail, @this.DeviceName, item.MethodInfo.Name, exception.Message));
                            }
                            item.LastErrorMessage = exception.Message;
                            @this.CurrentDevice.SetDeviceStatus(TimerX.Now, null, exception.Message);
                            var time = DateTime.Now;
                            item.Variable.SetValue(null, time, isOnline: false);
                        }

                    }
                }
            }

        }

    }



    protected override ValueTask<OperResult<ReadOnlyMemory<byte>>> ReadSourceAsync(VariableSourceRead variableSourceRead, CancellationToken cancellationToken)
    {
        return ReadSourceAsync(this, variableSourceRead, cancellationToken);


        static async PooledValueTask<OperResult<ReadOnlyMemory<byte>>> ReadSourceAsync(CollectFoundationBase @this, VariableSourceRead variableSourceRead, CancellationToken cancellationToken)
        {
            try
            {

                if (cancellationToken.IsCancellationRequested)
                    return new(new OperationCanceledException());

                // 从协议读取数据
                OperResult<ReadOnlyMemory<byte>> read = default;
                var readTask = @this.FoundationDevice.ReadByteAsync(variableSourceRead.DeviceAddress, cancellationToken);
                if (!readTask.IsCompletedSuccessfully)
                {
                    read = await readTask.ConfigureAwait(false);
                }
                else
                {
                    read = readTask.Result;
                }

                // 如果读取成功且有有效内容，则解析结构化内容
                if (read.IsSuccess)
                {
                    var prase = variableSourceRead.Variables.PraseStructContent(@this.FoundationDevice, read.Content, false);
                    return new OperResult<ReadOnlyMemory<byte>>(prase);
                }

                // 返回读取结果
                return read;
            }
            catch (Exception ex)
            {
                // 捕获异常并返回失败结果
                return new OperResult<ReadOnlyMemory<byte>>(ex);
            }
        }

    }

    /// <summary>
    /// 批量写入变量值,需返回变量名称/结果，注意非通用设备需重写
    /// </summary>
    /// <returns></returns>
    protected override ValueTask<Dictionary<string, OperResult>> WriteValuesAsync(Dictionary<VariableRuntime, JsonNode> writeInfoLists, CancellationToken cancellationToken)
    {
        return WriteValuesAsync(this, writeInfoLists, cancellationToken);

        static async PooledValueTask<Dictionary<string, OperResult>> WriteValuesAsync(CollectFoundationBase @this, Dictionary<VariableRuntime, JsonNode> writeInfoLists, CancellationToken cancellationToken)
        {
            using var writeLock = await @this.ReadWriteLock.WriterLockAsync(cancellationToken).ConfigureAwait(false);
            // 检查协议是否为空，如果为空则抛出异常
            if (@this.FoundationDevice == null)
                throw new NotSupportedException();

            // 创建用于存储操作结果的并发字典
            NonBlockingDictionary<string, OperResult> operResults = new();
            // 使用并发方式遍历写入信息列表，并进行异步写入操作
            await writeInfoLists.ParallelForEachAsync(async (writeInfo, cancellationToken) =>
            {
                try
                {

                    // 调用协议的写入方法，将写入信息中的数据写入到对应的寄存器地址，并获取操作结果
                    var result = await @this.FoundationDevice.WriteJsonNodeAsync(writeInfo.Key.RegisterAddress, writeInfo.Value, writeInfo.Key.DataType, writeInfo.Key.BitConverter, cancellationToken).ConfigureAwait(false);

                    if (result.IsSuccess)
                    {
                        if (@this.LogMessage?.LogLevel <= TouchSocket.Core.LogLevel.Debug)
                            @this.LogMessage?.LogDebug(string.Format("{0} - Write [{1} - {2} - {3}] data succeeded", @this.DeviceName, writeInfo.Key.RegisterAddress, writeInfo.Value, writeInfo.Key.DataType));
                    }
                    else
                    {
                        @this.LogMessage?.LogWarning(string.Format("{0} - Write [{1} - {2} - {3}] data failed {4}", @this.DeviceName, writeInfo.Key.RegisterAddress, writeInfo.Value, writeInfo.Key.DataType, result.ToString()));
                    }
                    // 将操作结果添加到结果字典中，使用变量名称作为键
                    operResults.TryAdd(writeInfo.Key.Name, result);
                }
                catch (Exception ex)
                {
                    operResults.TryAdd(writeInfo.Key.Name, new(ex));
                }
            }, @this.CollectProperties.MaxConcurrentCount, cancellationToken).ConfigureAwait(false);

            await @this.Check(writeInfoLists, operResults, cancellationToken).ConfigureAwait(false);

            // 返回包含操作结果的字典
            return new Dictionary<string, OperResult>(operResults);
        }
    }


#endif
}
