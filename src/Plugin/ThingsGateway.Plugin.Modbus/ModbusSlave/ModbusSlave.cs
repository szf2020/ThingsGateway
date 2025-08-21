//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Localization;

using Newtonsoft.Json.Linq;

using System.Collections.Concurrent;

using ThingsGateway.Foundation.Modbus;
using ThingsGateway.Gateway.Application;
using ThingsGateway.NewLife;
using ThingsGateway.NewLife.DictionaryExtensions;
using ThingsGateway.NewLife.Extension;
using ThingsGateway.NewLife.Json.Extension;
using ThingsGateway.SqlSugar;

using TouchSocket.Core;

namespace ThingsGateway.Plugin.Modbus;

public class ModbusSlave : BusinessBase
{
    private readonly ModbusSlaveProperty _driverPropertys = new();

    private readonly ConcurrentDictionary<string, long> ModbusVariableQueue = new();

    private readonly ModbusSlaveVariableProperty _variablePropertys = new();

    private Dictionary<ModbusAddress, VariableRuntime> ModbusVariables;

    private ThingsGateway.Foundation.Modbus.ModbusSlave _plc = new();

    private volatile bool success = true;

    /// <inheritdoc/>
    public override Type DriverDebugUIType => typeof(ThingsGateway.Debug.ModbusSlave);



    /// <inheritdoc/>
    public override VariablePropertyBase VariablePropertys => _variablePropertys;

    /// <inheritdoc/>
    protected override BusinessPropertyBase _businessPropertyBase => _driverPropertys;

    protected IStringLocalizer Localizer { get; private set; }


    public override string ToString()
    {
        return _plc?.ToString() ?? base.ToString();
    }

#if !Management

    /// <summary>
    /// 是否连接成功
    /// </summary>
    public override bool IsConnected()
    {
        return _plc?.OnLine == true;
    }
    /// <inheritdoc/>
    public override Type DriverUIType
    {
        get
        {
            if (_plc.Channel?.ChannelType == ChannelTypeEnum.TcpService)
                return typeof(ThingsGateway.Gateway.Razor.TcpServiceComponent);
            else
                return null;
        }
    }

    /// <summary>
    /// 开始通讯执行的方法
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override async Task ProtectedStartAsync(CancellationToken cancellationToken)
    {
        await _plc.ConnectAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task InitChannelAsync(IChannel? channel, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(channel);
        var plc = _plc;
        _plc = new();
        if (plc != null)
            await plc.SafeDisposeAsync().ConfigureAwait(false);
        //载入配置
        _plc.DataFormat = _driverPropertys.DataFormat;
        _plc.IsStringReverseByteWord = _driverPropertys.IsStringReverseByteWord;
        _plc.Station = _driverPropertys.Station;
        _plc.IsWriteMemory = _driverPropertys.IsWriteMemory;
        _plc.MulStation = _driverPropertys.MulStation;
        _plc.ModbusType = _driverPropertys.ModbusType;
        _plc.SendDelayTime = _driverPropertys.SendDelayTime;
        _plc.InitChannel(channel, LogMessage);
        await base.InitChannelAsync(channel, cancellationToken).ConfigureAwait(false);

        _plc.WriteData -= OnWriteData;
        _plc.WriteData += OnWriteData;

        try
        {
            await _plc.ConnectAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
        }

        Localizer = App.CreateLocalizerByType(typeof(ModbusSlave))!;

        GlobalData.VariableValueChangeEvent -= VariableValueChange;
        GlobalData.VariableValueChangeEvent += VariableValueChange;
    }
    public override async Task AfterVariablesChangedAsync(CancellationToken cancellationToken)
    {
        await base.AfterVariablesChangedAsync(cancellationToken).ConfigureAwait(false);
        ModbusVariableQueue?.Clear();
        IdVariableRuntimes.ForEach(a => VariableValueChange(a.Value, default));

        ModbusVariables = IdVariableRuntimes.ToDictionary(a =>
        {
            ModbusAddress address = _plc.GetModbusAddress(
                a.Value.GetPropertyValue(DeviceId,
                nameof(_variablePropertys.ServiceAddress)), _driverPropertys.Station, isCache: false);
            return address!;
        },
        a => a.Value
        );
    }
    /// <inheritdoc/>
    protected override async Task DisposeAsync(bool disposing)
    {
        ModbusVariables?.Clear();
        ModbusVariableQueue?.Clear();
        GlobalData.VariableValueChangeEvent -= VariableValueChange;
        if (_plc != null)
            await _plc.SafeDisposeAsync().ConfigureAwait(false);
        await base.DisposeAsync(disposing).ConfigureAwait(false);
    }

    protected override async Task ProtectedExecuteAsync(object? state, CancellationToken cancellationToken)
    {
        //获取设备连接状态
        if (!IsConnected())
        {
            try
            {
                if (cancellationToken.IsCancellationRequested)
                    return;
                await _plc.ConnectAsync(cancellationToken).ConfigureAwait(false);
                success = true;
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                if (success)
                    LogMessage?.LogWarning(ex, "Failed to start service");
                success = false;
                await Task.Delay(10000, cancellationToken).ConfigureAwait(false);
            }
        }
        var list = ModbusVariableQueue.ToDictWithDequeue();
        foreach (var item in list)
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            if (!IdVariableRuntimes.TryGetValue(item.Value, out var variableRuntime))
                break;

            var type = variableRuntime.GetPropertyValue(CurrentDevice.Id, nameof(ModbusSlaveVariableProperty.DataType));
            if (Enum.TryParse(type, out DataTypeEnum result))
            {
                await _plc.WriteJTokenAsync(item.Key, JToken.FromObject(variableRuntime.Value), result, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await _plc.WriteJTokenAsync(item.Key, JToken.FromObject(variableRuntime.Value), variableRuntime.DataType, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// RPC写入
    /// </summary>
    private async ValueTask<IOperResult> OnWriteData(ModbusRequest modbusRequest, IThingsGatewayBitConverter bitConverter, IChannel channel)
    {
        try
        {
            var tag = ModbusVariables.Where(a => a.Key?.StartAddress == modbusRequest.StartAddress && a.Key?.Station == modbusRequest.Station && a.Key?.FunctionCode == modbusRequest.FunctionCode).ToArray();
            if (tag.Length == 0) return OperResult.Success;


            foreach (var item in tag)
            {
                if (!(item.Value.GetPropertyValue(DeviceId, nameof(_variablePropertys.VariableRpcEnable)).ToBoolean(false) && _driverPropertys.DeviceRpcEnable))
                    return new OperResult("Not Permitted to Write");

                var type = item.Value.GetPropertyValue(DeviceId, nameof(ModbusSlaveVariableProperty.DataType));
                var dType = Enum.TryParse(type, out DataTypeEnum dataType) ? dataType : item.Value.DataType;
                var addressStr = item.Value.GetPropertyValue(DeviceId, nameof(ModbusSlaveVariableProperty.ServiceAddress));

                var thingsGatewayBitConverter = bitConverter.GetTransByAddress(addressStr);

                var bitIndex = _plc.GetBitOffset(addressStr);
                if (modbusRequest.FunctionCode == 0x03 && dType == DataTypeEnum.Boolean && bitIndex != null)
                {
                    var int16Data = thingsGatewayBitConverter.ToUInt16(modbusRequest.MasterWriteDatas.Span, 0);
                    var wData = BitHelper.GetBit(int16Data, bitIndex.Value);

                    var result = await item.Value.RpcAsync(wData.ToSystemTextJsonString(), $"{nameof(ModbusSlave)}-{CurrentDevice.Name}-{$"{channel}"}").ConfigureAwait(false);

                    if (!result.IsSuccess)
                        return result;
                }
                else
                {
                    _ = thingsGatewayBitConverter.GetChangedDataFormBytes(_plc, addressStr, modbusRequest.MasterWriteDatas.Span, 0, dType, item.Value.ArrayLength ?? 1, null, out var data);

                    var result = await item.Value.RpcAsync(data.ToSystemTextJsonString(), $"{nameof(ModbusSlave)}-{CurrentDevice.Name}-{$"{channel}"}").ConfigureAwait(false);

                    if (!result.IsSuccess)
                        return result;
                }
            }
            return OperResult.Success;
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    /// <inheritdoc/>
    private void VariableValueChange(VariableRuntime variableRuntime, VariableBasicData variableData)
    {
        if (CurrentDevice?.Pause != false)
            return;
        if (TaskSchedulerLoop?.Stoped == true) return;
        var address = variableRuntime.GetPropertyValue(DeviceId, nameof(_variablePropertys.ServiceAddress));
        if (address != null && variableRuntime.Value != null)
        {
            ModbusVariableQueue?.AddOrUpdate(address, address => variableRuntime.Id, (address, addvalue) => variableRuntime.Id);
        }
    }

    public override void PauseThread(bool pause)
    {
        lock (pauseLock)
        {
            var oldV = CurrentDevice.Pause;
            base.PauseThread(pause);
            if (!pause && oldV != pause)
            {
                IdVariableRuntimes.ForEach(a => VariableValueChange(a.Value, null));
            }
        }
    }


#endif
}
