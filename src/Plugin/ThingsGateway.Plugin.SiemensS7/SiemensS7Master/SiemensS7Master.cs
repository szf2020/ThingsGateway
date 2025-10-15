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

using ThingsGateway.Debug;
using ThingsGateway.Foundation.SiemensS7;
using ThingsGateway.Gateway.Application;
using ThingsGateway.NewLife.Json.Extension;

using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ThingsGateway.Plugin.SiemensS7;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class SiemensS7Master : CollectFoundationBase
{
    private readonly SiemensS7MasterProperty _driverPropertys = new();

    private ThingsGateway.Foundation.SiemensS7.SiemensS7Master _plc = new();

    /// <inheritdoc/>
    public override CollectPropertyBase CollectProperties => _driverPropertys;

    /// <inheritdoc/>
    public override Type DriverDebugUIType => typeof(ThingsGateway.Debug.SiemensS7Master);




    /// <inheritdoc/>
    public override IDevice FoundationDevice => _plc;

    public override Type DriverVariableAddressUIType => typeof(SiemensS7AddressComponent);

    [ThingsGateway.Gateway.Application.DynamicMethod("ReadWriteDateAsync", "读写日期格式")]
    public async Task<IOperResult<System.DateTime>> ReadWriteDateAsync(string address, System.DateTime? value = null, CancellationToken cancellationToken = default)
    {
        if (value == null)
            return await _plc.ReadDateAsync(address, cancellationToken).ConfigureAwait(false);
        else
            return new OperResult<System.DateTime>(await _plc.WriteDateAsync(address, value.Value, cancellationToken).ConfigureAwait(false));
    }

    [ThingsGateway.Gateway.Application.DynamicMethod("ReadWriteDateTimeAsync", "读写日期时间格式")]
    public async Task<IOperResult<System.DateTime>> ReadWriteDateTimeAsync(string address, System.DateTime? value = null, CancellationToken cancellationToken = default)
    {
        if (value == null)
            return await _plc.ReadDateTimeAsync(address, cancellationToken).ConfigureAwait(false);
        else
            return new OperResult<System.DateTime>(await _plc.WriteDateTimeAsync(address, value.Value, cancellationToken).ConfigureAwait(false));
    }

#if !Management

    /// <inheritdoc/>
    public override Type DriverUIType
    {
        get
        {
            if (FoundationDevice.Channel?.ChannelType == ChannelTypeEnum.TcpService)
                return typeof(ThingsGateway.Gateway.Razor.TcpServiceComponent);
            else
                return null;
        }
    }



    protected override async Task InitChannelAsync(IChannel? channel = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(channel);

        var plc = _plc;
        _plc = new();
        if (plc != null)
            await plc.SafeDisposeAsync().ConfigureAwait(false);

        //载入配置
        _plc.DataFormat = _driverPropertys.DataFormat;
        _plc.SendDelayTime = _driverPropertys.SendDelayTime;
        _plc.SiemensS7Type = _driverPropertys.SiemensS7Type;
        _plc.Timeout = _driverPropertys.Timeout;
        _plc.LocalTSAP = _driverPropertys.LocalTSAP;
        _plc.Rack = _driverPropertys.Rack;
        _plc.Slot = _driverPropertys.Slot;
        _plc.InitChannel(channel, LogMessage);
        await base.InitChannelAsync(channel, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 获取jtoken值代表的字节数组，不包含字符串
    /// </summary>
    /// <param name="dataType"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public ReadOnlyMemory<byte> GetBytes(DataTypeEnum dataType, JToken value)
    {
        //排除字符串
        if (value is JArray jArray && jArray.Count > 1)
        {
            return this._plc.ThingsGatewayBitConverter.GetBytesFormData(value, dataType, true);
        }
        else
        {
            return this._plc.ThingsGatewayBitConverter.GetBytesFormData(value, dataType, false);
        }
    }

    protected override async ValueTask<Dictionary<string, OperResult>> WriteValuesAsync(Dictionary<VariableRuntime, JToken> writeInfoLists, CancellationToken cancellationToken)
    {
        using var writeLock = await ReadWriteLock.WriterLockAsync(cancellationToken).ConfigureAwait(false);

        // 检查协议是否为空，如果为空则抛出异常
        if (FoundationDevice == null)
            throw new NotSupportedException();

        // 创建用于存储操作结果的并发字典
        NonBlockingDictionary<string, OperResult> operResults = new();

        //转换
        Dictionary<VariableRuntime, SiemensS7Address> addresses = new();
        var w1 = writeInfoLists.Where(a => a.Key.DataType != DataTypeEnum.String);
        var w2 = writeInfoLists.Where(a => a.Key.DataType == DataTypeEnum.String);
        foreach (var item in w1)
        {
            SiemensS7Address siemensS7Address = SiemensS7Address.ParseFrom(item.Key.RegisterAddress);
            siemensS7Address.Data = GetBytes(item.Key.DataType, item.Value);
            siemensS7Address.Length = siemensS7Address.Data.Length;
            siemensS7Address.BitLength = 1;
            siemensS7Address.IsBit = item.Key.DataType == DataTypeEnum.Boolean;
            if (item.Key.DataType == DataTypeEnum.Boolean)
            {
                if (item.Value is JArray jArray)
                {
                    siemensS7Address.BitLength = jArray.ToObject<Boolean[]>().Length;
                }
            }
            addresses.Add(item.Key, siemensS7Address);
        }
        if (addresses.Count > 0)
        {
            var result = await _plc.S7WriteAsync(addresses.Select(a => a.Value).ToArray(), cancellationToken).ConfigureAwait(false);
            foreach (var writeInfo in addresses)
            {
                if (result.TryGetValue(writeInfo.Value, out var r1))
                {
                    operResults.TryAdd(writeInfo.Key.Name, r1);
                }
            }



        }

        // 使用并发方式遍历写入信息列表，并进行异步写入操作
        await w2.ForEachAsync(async (writeInfo) =>
        {
            try
            {
                // 调用协议的写入方法，将写入信息中的数据写入到对应的寄存器地址，并获取操作结果
                var result = await FoundationDevice.WriteJTokenAsync(writeInfo.Key.RegisterAddress, writeInfo.Value, writeInfo.Key.DataType, cancellationToken).ConfigureAwait(false);

                // 将操作结果添加到结果字典中，使用变量名称作为键
                operResults.TryAdd(writeInfo.Key.Name, result);
            }
            catch (Exception ex)
            {
                operResults.TryAdd(writeInfo.Key.Name, new(ex));
            }
        }).ConfigureAwait(false);

        await Check(writeInfoLists, operResults, cancellationToken).ConfigureAwait(false);
        if (LogMessage?.LogLevel <= TouchSocket.Core.LogLevel.Debug)
            LogMessage?.Debug(string.Format("Write result: {0} - {1}", DeviceName, operResults.Select(a => $"{a.Key} - {a.Key.Length} - {(a.Value.IsSuccess ? "Success" : a.Value.ErrorMessage)}").ToSystemTextJsonString(false)));
        // 返回包含操作结果的字典
        return new Dictionary<string, OperResult>(operResults);
    }



    /// <inheritdoc/>
    protected override async Task<List<VariableSourceRead>> ProtectedLoadSourceReadAsync(List<VariableRuntime> deviceVariables)
    {
        try
        {
            await _plc.Channel.ConnectAsync().ConfigureAwait(false);
        }
        catch
        {
        }
        List<VariableSourceRead> variableSourceReads = new();
        foreach (var deviceVariable in deviceVariables.GroupBy(a => a.CollectGroup))
        {
            variableSourceReads.AddRange(_plc.LoadSourceRead<VariableSourceRead>(deviceVariable, _driverPropertys.MaxPack, CurrentDevice.IntervalTime));
        }
        return variableSourceReads;
    }

#endif
}
