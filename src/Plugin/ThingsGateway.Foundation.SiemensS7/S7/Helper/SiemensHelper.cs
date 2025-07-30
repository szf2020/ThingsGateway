//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Text;

using ThingsGateway.NewLife.Extension;

namespace ThingsGateway.Foundation.SiemensS7;

internal sealed partial class SiemensHelper
{
    public static List<List<SiemensS7Address>> GroupByLength(SiemensS7Address[] a, int pduLength)
    {
        List<List<SiemensS7Address>> groups = new List<List<SiemensS7Address>>();
        List<SiemensS7Address> sortedItems = a.OrderByDescending(item => item.Length).ToList(); // 按长度降序排序

        while (sortedItems.Count > 0)
        {
            List<SiemensS7Address> currentGroup = new List<SiemensS7Address>();
            int currentGroupLength = 0;

            for (int i = 0; i < sortedItems.Count; i++)
            {
                SiemensS7Address item = sortedItems[i];
                if (currentGroupLength + item.Length <= pduLength) // 如果可以添加到当前组
                {
                    currentGroup.Add(item);
                    currentGroupLength += item.Length;
                    sortedItems.RemoveAt(i); // 从列表中移除已添加到组的项
                    i--; // 因为我们移除了一个元素，所以索引需要回退
                }
                else if (i == sortedItems.Count - 1) // 如果这是最后一个元素且不能添加到当前组
                {
                    // 创建一个新组并添加这个元素
                    groups.Add(new List<SiemensS7Address> { item });
                    sortedItems.RemoveAt(i);
                }
            }

            if (currentGroup.Count > 0) // 如果当前组不为空
            {
                groups.Add(currentGroup);
            }
        }

        return groups;
    }

    internal static string GetCpuError(ushort Error)
    {
        return Error switch
        {
            0x01 => AppResource.ERROR1,
            0x03 => AppResource.ERROR3,
            0x05 => AppResource.ERROR5,
            0x06 => AppResource.ERROR6,
            0x07 => AppResource.ERROR7,
            0x0a => AppResource.ERROR10,
            _ => "Unknown",
        };
    }

    internal static async ValueTask<OperResult<string>> ReadStringAsync(SiemensS7Master plc, string address, Encoding encoding, CancellationToken cancellationToken)
    {
        //先读取一次获取长度，再读取实际值
        if (plc.SiemensS7Type != SiemensTypeEnum.S200Smart)
        {
            var result1 = await plc.ReadAsync(address, 2, cancellationToken).ConfigureAwait(false);
            if (!result1.IsSuccess)
            {
                return new OperResult<string>(result1);
            }
            var span = result1.Content.Span;
            if (span[0] == 0 || span[0] == byte.MaxValue)
            {
                return new OperResult<string>(AppResource.NotString);
            }
            var result2 = await plc.ReadAsync(address, 2 + span[1], cancellationToken).ConfigureAwait(false);
            if (!result2.IsSuccess)
            {
                return new OperResult<string>(result2);
            }
            else
            {
                return OperResult.CreateSuccessResult(encoding.GetString(result2.Content.Span.Slice(2, result2.Content.Length - 2)));
            }
        }
        else
        {
            var result1 = await plc.ReadAsync(address, 1, cancellationToken).ConfigureAwait(false);
            if (!result1.IsSuccess)
                return new OperResult<string>(result1);
            var span = result1.Content.Span;
            var result2 = await plc.ReadAsync(address, 1 + span[0], cancellationToken).ConfigureAwait(false);
            if (!result2.IsSuccess)
            {
                return new OperResult<string>(result2);
            }
            else
            {
                return OperResult.CreateSuccessResult(encoding.GetString(result2.Content.Span.Slice(1, result2.Content.Length - 1)));
            }
        }
    }

    internal static async ValueTask<OperResult> WriteStringAsync(SiemensS7Master plc, string address, string value, Encoding encoding, CancellationToken cancellationToken = default)
    {
        value ??= string.Empty;
        byte[] inBytes = encoding.GetBytes(value);
        if (plc.SiemensS7Type != SiemensTypeEnum.S200Smart)
        {
            var result = await plc.ReadAsync(address, 2, cancellationToken).ConfigureAwait(false);
            if (!result.IsSuccess) return result;
            var span = result.Content.Span;
            var len = span[0];
            if (len == byte.MaxValue) return new OperResult<string>(AppResource.NotString);
            if (len == 0) len = 254;
            if (value.Length > span[0]) return new OperResult<string>(AppResource.WriteDataLengthMore);
            return await plc.WriteAsync(
                address,
                DataTransUtil.SpliceArray([len, (byte)value.Length],
                inBytes
                ), DataTypeEnum.String, cancellationToken).ConfigureAwait(false);
        }
        return await plc.WriteAsync(address, DataTransUtil.SpliceArray([(byte)value.Length], inBytes), DataTypeEnum.String, cancellationToken).ConfigureAwait(false);
    }

    internal static async ValueTask<OperResult<string>> ReadWStringAsync(SiemensS7Master plc, string address, Encoding encoding, CancellationToken cancellationToken)
    {
        //先读取一次获取长度，再读取实际值
        if (plc.SiemensS7Type != SiemensTypeEnum.S200Smart)
        {
            encoding ??= Encoding.BigEndianUnicode;
            var result1 = await plc.ReadAsync(address, 4, cancellationToken).ConfigureAwait(false);
            if (!result1.IsSuccess)
            {
                return new OperResult<string>(result1);
            }
            var span = result1.Content.Span;
            if (span[0] == 0 || span[0] == byte.MaxValue)
            {
                return new OperResult<string>(AppResource.NotString);
            }
            var result2 = await plc.ReadAsync(address, 4 + (plc.ThingsGatewayBitConverter.ToUInt16(span, 2) * 2), cancellationToken).ConfigureAwait(false);
            if (!result2.IsSuccess)
            {
                return new OperResult<string>(result2);
            }
            else
            {
                return OperResult.CreateSuccessResult(encoding.GetString(result2.Content.Span.Slice(4, result2.Content.Length - 4)));
            }
        }
        else
        {
            encoding ??= Encoding.Unicode;
            var result1 = await plc.ReadAsync(address, 1, cancellationToken).ConfigureAwait(false);
            if (!result1.IsSuccess)
                return new OperResult<string>(result1);
            var result2 = await plc.ReadAsync(address, 1 + (result1.Content.Span[0] * 2), cancellationToken).ConfigureAwait(false);
            if (!result2.IsSuccess)
            {
                return new OperResult<string>(result2);
            }
            else
            {
                return OperResult.CreateSuccessResult(encoding.GetString(result2.Content.Span.Slice(1, result2.Content.Length - 1)));
            }
        }
    }

    internal static async ValueTask<OperResult> WriteWStringAsync(SiemensS7Master plc, string address, string value, Encoding encoding, CancellationToken cancellationToken = default)
    {
        value ??= string.Empty;
        if (plc.SiemensS7Type != SiemensTypeEnum.S200Smart)
        {
            byte[] inBytes1 = (encoding ?? Encoding.BigEndianUnicode).GetBytes(value);
            var result = await plc.ReadAsync(address, 4, cancellationToken).ConfigureAwait(false);
            if (!result.IsSuccess) return result;
            var num = plc.ThingsGatewayBitConverter.ToUInt16(result.Content.Span, 0);
            if (num == 0)
                num = 254;
            if (value.Length > num) return new OperResult<string>(AppResource.WriteDataLengthMore);
            return await plc.WriteAsync(
                address,
                DataTransUtil.SpliceArray(plc.ThingsGatewayBitConverter.GetBytes(num), plc.ThingsGatewayBitConverter.GetBytes((ushort)value.Length),
                inBytes1
                ), DataTypeEnum.String, cancellationToken).ConfigureAwait(false);
        }
        byte[] inBytes2 = (encoding ?? Encoding.Unicode).GetBytes(value);
        return await plc.WriteAsync(address, DataTransUtil.SpliceArray([(byte)value.Length], inBytes2), DataTypeEnum.String, cancellationToken).ConfigureAwait(false);
    }
}
