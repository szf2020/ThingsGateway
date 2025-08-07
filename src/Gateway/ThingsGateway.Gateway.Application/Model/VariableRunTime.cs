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

using ThingsGateway.Gateway.Application.Extensions;
using ThingsGateway.NewLife.DictionaryExtensions;
using ThingsGateway.NewLife.Json.Extension;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 变量运行态
/// </summary>
public partial class VariableRuntime : Variable, IVariable, IDisposable
{


    private int index;
    private int sortCode;
    private DateTime changeTime = DateTime.UnixEpoch.ToLocalTime();

    private DateTime collectTime = DateTime.UnixEpoch.ToLocalTime();

    private bool _isOnline;
    private bool _isOnlineChanged;
    private bool _valueInited;

    private string _lastErrorMessage;
    private object _value;
    private object lastSetValue;
    private object rawValue;
    private DeviceRuntime? deviceRuntime;
    private IVariableSource? variableSource;
    private VariableMethod? variableMethod;
    private IThingsGatewayBitConverter? thingsGatewayBitConverter;

    /// <summary>
    /// 设置变量值与时间/质量戳
    /// </summary>
    /// <param name="value"></param>
    /// <param name="dateTime"></param>
    /// <param name="isOnline"></param>
    public OperResult SetValue(object value, DateTime dateTime, bool isOnline = true)
    {
        IsOnline = isOnline;
        RawValue = value;
        if (IsOnline == false)
        {
            Set(value, dateTime);
            return new();
        }
        if (!string.IsNullOrEmpty(ReadExpressions))
        {
            try
            {
                var data = ReadExpressions.GetExpressionsResult(RawValue, DeviceRuntime?.Driver?.LogMessage);
                Set(data, dateTime);
            }
            catch (Exception ex)
            {
                IsOnline = false;
                Set(null, dateTime);
                var oldMessage = _lastErrorMessage;
                if (ex.StackTrace != null)
                {
                    string stachTrace = string.Join(Environment.NewLine, ex.StackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Take(3));
                    _lastErrorMessage = $"{Name} Conversion expression failed：{ex.Message}{Environment.NewLine}{stachTrace}";
                }
                else
                {
                    _lastErrorMessage = $"{Name} Conversion expression failed：{ex.Message}{Environment.NewLine}";
                }
                if (oldMessage != _lastErrorMessage)
                {
                    DeviceRuntime?.Driver?.LogMessage?.LogWarning(_lastErrorMessage);
                }
                return new($"{Name} Conversion expression failed", ex);
            }
        }
        else
        {
            Set(value, dateTime);
        }
        return new();
    }

    /// <summary>
    /// 设置变量值与时间/质量戳
    /// </summary>
    /// <param name="dateTime"></param>
    public void SetNoChangedValue(DateTime dateTime)
    {
        DateTime time = dateTime != default ? dateTime : DateTime.Now;
        if (!IsOnline)
        {
            SetValue(RawValue, time, true);
        }
        else
        {
            CollectTime = time;
            GlobalData.VariableCollectChange(this);
        }
    }

    private void Set(object data, DateTime dateTime)
    {
        DateTime time = dateTime != default ? dateTime : DateTime.Now;
        CollectTime = time;

        bool changed = false;
        if (data == null)
        {
            if (IsOnline)
            {
                changed = (_value != null);
            }
        }
        else
        {
            if (data is JToken jToken)
            {
                data = jToken.GetObjectFromJToken();
            }

            //判断变化，插件传入的Value可能是基础类型，也有可能是class，比较器无法识别是否变化，这里json处理序列化比较
            //检查IComparable
            if (!data.Equals(_value))
            {
                if (data is IComparable)
                {
                    changed = true;
                }
                else
                {
                    if (_value != null)
                        changed = data.ToSystemTextJsonString(false) != _value.ToSystemTextJsonString(false);
                    else
                        changed = true;
                }
            }
            else
            {
                changed = false;
            }
        }
        if (changed || _isOnlineChanged == true)
        {
            ChangeTime = time;

            LastSetValue = _value;

            ValueInited = true;

            if (_isOnline == true)
            {
                _value = data;
            }

            GlobalData.VariableValueChange(this);
        }
        GlobalData.VariableCollectChange(this);
    }

    public void Init(DeviceRuntime deviceRuntime)
    {
        GlobalData.AlarmEnableIdVariables.Remove(Id);
        if (!AlarmEnable && GlobalData.RealAlarmIdVariables.TryRemove(Id, out var oldAlarm))
        {
            oldAlarm.EventType = EventTypeEnum.Restart;
            oldAlarm.EventTime = DateTime.Now;
            GlobalData.AlarmChange(this.AdaptAlarmVariable());
        }

        DeviceRuntime?.VariableRuntimes?.Remove(Name);

        DeviceRuntime = deviceRuntime;

        DeviceRuntime?.VariableRuntimes?.TryAdd(Name, this);
        GlobalData.IdVariables.Remove(Id);
        GlobalData.IdVariables.TryAdd(Id, this);
        if (AlarmEnable)
        {
            this.AlarmRuntimePropertys = new();
            GlobalData.AlarmEnableIdVariables.TryAdd(Id, this);
        }
    }

    public void Dispose()
    {
        DeviceRuntime?.VariableRuntimes?.Remove(Name);

        GlobalData.IdVariables.Remove(Id);

        GlobalData.AlarmEnableIdVariables.Remove(Id);
        if (GlobalData.RealAlarmIdVariables.TryRemove(Id, out var oldAlarm))
        {
            oldAlarm.EventType = EventTypeEnum.Restart;
            oldAlarm.EventTime = DateTime.Now;
            GlobalData.AlarmChange(this.AdaptAlarmVariable());
        }

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public async ValueTask<IOperResult> RpcAsync(string value, string executive = "brower", CancellationToken cancellationToken = default)
    {
        var data = await GlobalData.RpcService.InvokeDeviceMethodAsync(executive, new Dictionary<string, Dictionary<string, string>>()
        {
            { DeviceName, new Dictionary<string, string>()  {   { Name,value} }  }
        }, cancellationToken).ConfigureAwait(false);
        return data.FirstOrDefault().Value.FirstOrDefault().Value;
    }

    public void SetErrorMessage(string lastErrorMessage)
    {
        _lastErrorMessage = lastErrorMessage;
    }
}
