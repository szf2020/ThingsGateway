//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation;
#pragma warning disable CA1851

public static class PackHelpers
{
    public static List<T> GetSourceRead<T>(this IEnumerable<IVariable> deviceVariables, IDevice device, string defaultIntervalTime) where T : IVariableSource, new()
    {
        var byteConverter = device.ThingsGatewayBitConverter;
        var result = new List<T>();
        //需要先剔除额外信息，比如dataformat等
        foreach (var item in deviceVariables)
        {
            var address = item.RegisterAddress;
            if (address == null)
                continue;
            IThingsGatewayBitConverter transformParameter = byteConverter.GetTransByAddress(address);
            item.ThingsGatewayBitConverter = transformParameter;
            item.Index = 0;
            if (item.DataType == DataTypeEnum.Boolean)
                item.Index = device.GetBitOffsetDefault(item.RegisterAddress);
        }
        var group = deviceVariables.GroupBy(a => a.RegisterAddress);
        foreach (var item in group)
        {
            var r = new T()
            {
                RegisterAddress = item.Key!,
                Length = 1,
                IntervalTime = string.IsNullOrWhiteSpace(item.FirstOrDefault().IntervalTime) ? defaultIntervalTime : item.FirstOrDefault().IntervalTime,
            };
            r.AddVariableRange(item);
            result.Add(r);
        }

        return result;
    }
}