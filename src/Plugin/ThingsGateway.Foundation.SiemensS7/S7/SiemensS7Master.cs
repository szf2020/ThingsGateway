//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using PooledAwait;

using ThingsGateway.Foundation.Extension.String;
using ThingsGateway.NewLife;
using ThingsGateway.NewLife.Extension;

using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ThingsGateway.Foundation.SiemensS7;

/// <inheritdoc/>
public partial class SiemensS7Master : DeviceBase
{
    public override void InitChannel(IChannel channel, ILog? deviceLog = null)
    {
        base.InitChannel(channel, deviceLog);

        RegisterByteLength = 1;
    }

    protected override void SetChannel()
    {
    }

    public override IThingsGatewayBitConverter ThingsGatewayBitConverter => s7BitConverter;
    private S7BitConverter s7BitConverter = new S7BitConverter(EndianType.Big);
    /// <summary>
    /// PduLength
    /// </summary>
    public int PduLength { get; private set; } = 100;

    #region 设置

    /// <summary>
    /// 本地TSAP，需重新连接
    /// </summary>
    public int LocalTSAP { get; set; }

    /// <summary>
    /// 机架号，需重新连接
    /// </summary>
    public byte Rack { get; set; }

    /// <summary>
    /// S7类型
    /// </summary>
    public SiemensTypeEnum SiemensS7Type
    {
        get => siemensS7Type; set
        {
            siemensS7Type = value;
            s7BitConverter.SMART200 = value == SiemensTypeEnum.S200Smart;
        }
    }

    /// <summary>
    /// 槽号，需重新连接
    /// </summary>
    public byte Slot { get; set; }

    #endregion 设置

    /// <inheritdoc/>
    public override bool BitReverse(string address)
    {
        return false;
    }

    /// <inheritdoc/>
    public override string GetAddressDescription()
    {
        return $"{base.GetAddressDescription()}{Environment.NewLine}{AppResource.S7_AddressDes}";
    }

    /// <inheritdoc/>
    public override int? GetBitOffset(string address)
    {
        if (address.IndexOf('.') > 0)
        {
            var addressSplits1 = address.SplitStringBySemicolon().Where(a => !a.Contains('=')).FirstOrDefault();
            string[] addressSplits = addressSplits1.SplitStringByDelimiter();
            try
            {
                var hasDB = address.Contains("DB", StringComparison.OrdinalIgnoreCase);
                int bitIndex = 0;
                if ((addressSplits.Length == 2 && !hasDB) || (addressSplits.Length >= 3 && hasDB))
                    bitIndex = Convert.ToInt32(addressSplits.Last());
                return bitIndex;
            }
            catch
            {
                return 0;
            }
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public override DataHandlingAdapter GetDataAdapter()
    {
        switch (Channel.ChannelType)
        {
            case ChannelTypeEnum.TcpClient:
            case ChannelTypeEnum.TcpService:
            case ChannelTypeEnum.SerialPort:
                return new DeviceSingleStreamDataHandleAdapter<S7Message>
                {
                    CacheTimeout = TimeSpan.FromMilliseconds(Channel.ChannelOptions.CacheTimeout),
                    //IsSingleThread = false
                };

            case ChannelTypeEnum.UdpSession:
                return new DeviceUdpDataHandleAdapter<S7Message>()
                {
                    //IsSingleThread = false
                };
        }

        return new DeviceSingleStreamDataHandleAdapter<S7Message>
        {
            CacheTimeout = TimeSpan.FromMilliseconds(Channel.ChannelOptions.CacheTimeout),
            //IsSingleThread = false
        };
    }

    /// <inheritdoc/>
    public override List<T> LoadSourceRead<T>(IEnumerable<IVariable> deviceVariables, int maxPack, string defaultIntervalTime)
    {
        return PackHelper.LoadSourceRead<T>(this, deviceVariables, maxPack, defaultIntervalTime);
    }

    /// <summary>
    /// 此方法并不会智能分组以最大化效率，减少传输次数，因为返回值是byte[]，所以一切都按地址数组的顺序执行，最后合并数组
    /// </summary>
    public ValueTask<OperResult<ReadOnlyMemory<byte>>> S7ReadAsync(
        SiemensS7Address[] addresses,
        CancellationToken cancellationToken = default)
    {
        return S7ReadAsync(this, addresses, cancellationToken);

        static async PooledValueTask<OperResult<ReadOnlyMemory<byte>>> S7ReadAsync(SiemensS7Master @this, SiemensS7Address[] addresses, CancellationToken cancellationToken)
        {
            var byteBuffer = new ValueByteBlock(512);

            try
            {
                foreach (var address in addresses)
                {
                    int readCount = 0;
                    int totalLength = address.Length == 0 ? 1 : address.Length;
                    int originalStart = address.AddressStart;

                    try
                    {
                        while (readCount < totalLength)
                        {
                            // 每次读取的 PDU 长度，循环直到读取完整
                            int chunkLength = Math.Min(totalLength - readCount, @this.PduLength);
                            address.Length = chunkLength;

                            var result = await @this.SendThenReturnAsync(
                                new S7Send([address], true),
                                cancellationToken: cancellationToken
                            ).ConfigureAwait(false);

                            if (!result.IsSuccess)
                                return result;

                            byteBuffer.Write(result.Content.Span);

                            if (readCount + chunkLength >= totalLength)
                            {
                                if (addresses.Length == 1)
                                {
                                    return result;
                                }
                                break;
                            }

                            readCount += chunkLength;

                            // 更新地址起点
                            if (address.DataCode == S7Area.TM || address.DataCode == S7Area.CT)
                                address.AddressStart += chunkLength / 2;
                            else
                                address.AddressStart += chunkLength * 8;
                        }
                    }
                    finally
                    {
                        address.AddressStart = originalStart;
                    }
                }

                return new OperResult<ReadOnlyMemory<byte>> { Content = byteBuffer.ToArray() };
            }
            catch (Exception ex)
            {
                return new OperResult<ReadOnlyMemory<byte>>(ex);
            }
            finally
            {
                byteBuffer.SafeDispose();
            }
        }
    }


    /// <summary>
    /// 此方法并不会智能分组以最大化效率，减少传输次数，因为返回值是byte[]，所以一切都按地址数组的顺序执行，最后合并数组
    /// </summary>
    public ValueTask<Dictionary<SiemensS7Address, OperResult>> S7WriteAsync(
    SiemensS7Address[] addresses,
    CancellationToken cancellationToken = default)
    {
        return S7WriteAsync(this, addresses, cancellationToken);

        static async PooledValueTask<Dictionary<SiemensS7Address, OperResult>> S7WriteAsync(SiemensS7Master @this, SiemensS7Address[] addresses, CancellationToken cancellationToken)
        {
            var dictOperResult = new Dictionary<SiemensS7Address, OperResult>();

            void SetFailOperResult(OperResult operResult)
            {
                foreach (var address in addresses)
                {
                    dictOperResult.TryAdd(address, operResult);
                }
            }

            var firstAddress = addresses[0];

            // 单位写入（位写入）
            if (addresses.Length <= 1 && firstAddress.IsBit)
            {
                var byteBuffer = new ValueByteBlock(512);
                try
                {
                    var writeResult = await @this.SendThenReturnAsync(
                        new S7Send([firstAddress], false),
                        cancellationToken: cancellationToken
                    ).ConfigureAwait(false);

                    dictOperResult.TryAdd(firstAddress, writeResult);
                    return dictOperResult;
                }
                catch (Exception ex)
                {
                    SetFailOperResult(new OperResult(ex));
                    return dictOperResult;
                }
                finally
                {
                    byteBuffer.SafeDispose();
                }
            }
            else
            {
                // 多写入
                var addressChunks = new List<List<SiemensS7Address>>();
                ushort dataLength = 0;
                ushort itemCount = 1;
                var currentChunk = new List<SiemensS7Address>();

                for (int i = 0; i < addresses.Length; i++)
                {
                    var address = addresses[i];
                    dataLength += (ushort)(address.Data.Length + 4);
                    ushort telegramLength = (ushort)(itemCount * 12 + 19 + dataLength);

                    if (telegramLength < @this.PduLength)
                    {
                        currentChunk.Add(address);
                        itemCount++;

                        if (i == addresses.Length - 1)
                            addressChunks.Add(currentChunk);
                    }
                    else
                    {
                        addressChunks.Add(currentChunk);
                        currentChunk = new List<SiemensS7Address>();
                        dataLength = 0;
                        itemCount = 1;

                        dataLength += (ushort)(address.Data.Length + 4);
                        telegramLength = (ushort)(itemCount * 12 + 19 + dataLength);

                        if (telegramLength < @this.PduLength)
                        {
                            currentChunk.Add(address);
                            itemCount++;

                            if (i == addresses.Length - 1)
                                addressChunks.Add(currentChunk);
                        }
                        else
                        {
                            SetFailOperResult(new OperResult("Write length exceeds limit"));
                            return dictOperResult;
                        }
                    }
                }

                foreach (var chunk in addressChunks)
                {
                    try
                    {
                        var result = await @this.SendThenReturnAsync(
                            new S7Send(chunk.ToArray(), false),
                            cancellationToken: cancellationToken
                        ).ConfigureAwait(false);

                        foreach (var addr in chunk)
                        {
                            dictOperResult.TryAdd(addr, result);
                        }
                    }
                    catch (Exception ex)
                    {
                        SetFailOperResult(new OperResult(ex));
                        return dictOperResult;
                    }
                }

                return dictOperResult;
            }
        }
    }


    #region 读写

    /// <inheritdoc/>
    public override ValueTask<OperResult<ReadOnlyMemory<byte>>> ReadAsync(string address, int length, CancellationToken cancellationToken = default)
    {
        try
        {
            var sAddress = SiemensS7Address.ParseFrom(address, length);
            return S7ReadAsync([sAddress], cancellationToken);
        }
        catch (Exception ex)
        {
            return EasyValueTask.FromResult(new OperResult<ReadOnlyMemory<byte>>(ex));
        }
    }

    public override ValueTask<OperResult<ReadOnlyMemory<byte>>> ReadAsync(object state, CancellationToken cancellationToken = default)
    {
        try
        {
            if (state is SiemensS7Address sAddress)
            {
                return S7ReadAsync([sAddress], cancellationToken);
            }
            else
            {
                return EasyValueTask.FromResult(new OperResult<ReadOnlyMemory<byte>>(new ArgumentException("State must be of type SiemensS7Address", nameof(state))));
            }
        }
        catch (Exception ex)
        {
            return EasyValueTask.FromResult(new OperResult<ReadOnlyMemory<byte>>(ex));
        }
    }

    /// <inheritdoc/>
    public override async ValueTask<OperResult> WriteAsync(string address, ReadOnlyMemory<byte> value, DataTypeEnum dataType, CancellationToken cancellationToken = default)
    {
        try
        {
            var sAddress = SiemensS7Address.ParseFrom(address);
            sAddress.Data = value;
            sAddress.Length = value.Length;
            return (await S7WriteAsync([sAddress], cancellationToken).ConfigureAwait(false)).FirstOrDefault().Value;
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    /// <inheritdoc/>
    public override async ValueTask<OperResult> WriteAsync(string address, ReadOnlyMemory<bool> value, CancellationToken cancellationToken = default)
    {
        try
        {
            var sAddress = SiemensS7Address.ParseFrom(address);
            sAddress.Data = value.Span.BoolArrayToByte();
            sAddress.Length = sAddress.Data.Length;
            sAddress.BitLength = value.Length;
            sAddress.IsBit = true;
            return (await S7WriteAsync([sAddress], cancellationToken).ConfigureAwait(false)).FirstOrDefault().Value;
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    #endregion 读写

    #region 初始握手
    private WaitLock ChannelStartedWaitLock = new(nameof(SiemensS7Master));
    private SiemensTypeEnum siemensS7Type;

    /// <inheritdoc/>
    protected override async ValueTask<bool> ChannelStarted(IClientChannel channel, bool last)
    {
        try
        {
            await ChannelStartedWaitLock.WaitAsync().ConfigureAwait(false);
            SetDataAdapter(channel);
            AutoConnect = false;
            if (channel?.Online != true)
            {
                return true;
            }
            var ISO_CR = SiemensHelper.ISO_CR;

            var S7_PN = SiemensHelper.S7_PN;
            //获取正确的ISO_CR/S7_PN
            //类型
            switch (SiemensS7Type)
            {
                case SiemensTypeEnum.S1200:
                    ISO_CR[21] = 0x00;
                    break;

                case SiemensTypeEnum.S300:
                    ISO_CR[21] = 0x02;
                    break;

                case SiemensTypeEnum.S400:
                    ISO_CR[21] = 0x03;
                    ISO_CR[17] = 0x00;
                    break;

                case SiemensTypeEnum.S1500:
                    ISO_CR[21] = 0x00;
                    break;

                case SiemensTypeEnum.S200Smart:
                    ISO_CR = SiemensHelper.ISO_CR200SMART;
                    S7_PN = SiemensHelper.S7200SMART_PN;
                    break;

                case SiemensTypeEnum.S200:
                    ISO_CR = SiemensHelper.ISO_CR200;
                    S7_PN = SiemensHelper.S7200_PN;
                    break;
            }

            if (LocalTSAP > 0)
            {
                //本地TSAP
                if (SiemensS7Type == SiemensTypeEnum.S200 || SiemensS7Type == SiemensTypeEnum.S200Smart)
                {
                    var data = s7BitConverter.GetBytes(LocalTSAP);
                    ISO_CR[13] = data[0];
                    ISO_CR[14] = data[1];
                }
                else
                {
                    var data = s7BitConverter.GetBytes(LocalTSAP);
                    ISO_CR[16] = data[0];
                    ISO_CR[17] = data[1];
                }
            }
            if (Rack > 0 || Slot > 0)
            {
                //槽号/机架号
                if (SiemensS7Type != SiemensTypeEnum.S200 && SiemensS7Type != SiemensTypeEnum.S200Smart)
                {
                    ISO_CR[21] = (byte)((Rack * 0x20) + Slot);
                }
            }

            try
            {
                var result2 = await SendThenReturnMessageAsync(new S7Send(ISO_CR), channel, channel.ClosedToken).ConfigureAwait(false);
                if (!result2.IsSuccess)
                {
                    await channel.CloseAsync().ConfigureAwait(false);

                    if (result2.Exception is not OperationCanceledException)
                        Logger?.LogWarning(string.Format(AppResource.HandshakeError1, channel.ToString(), result2));
                    return true;
                }
            }
            catch (Exception ex)
            {
                if (ex is not OperationCanceledException)
                    Logger?.LogWarning(string.Format(AppResource.HandshakeError1, channel.ToString(), ex));
                return true;
            }
            try
            {
                var result2 = await SendThenReturnMessageAsync(new S7Send(S7_PN), channel, channel.ClosedToken).ConfigureAwait(false);
                if (!result2.IsSuccess)
                {
                    await channel.CloseAsync().ConfigureAwait(false);
                    if (result2.Exception is not OperationCanceledException)
                        Logger?.LogWarning(string.Format(AppResource.HandshakeError2, channel.ToString(), result2));
                    return true;
                }
                if (result2.Content.IsEmpty)
                {
                    await channel.CloseAsync().ConfigureAwait(false);
                    return true;
                }
                PduLength = ThingsGatewayBitConverter.ToUInt16(result2.Content.Span, 0) - 28;
                Logger?.LogInformation($"PduLength：{PduLength}");
                PduLength = PduLength < 200 ? 200 : PduLength;
            }
            catch (Exception ex)
            {
                if (ex is not OperationCanceledException)
                    Logger?.LogWarning(string.Format(AppResource.HandshakeError2, channel.ToString(), ex));
                await channel.CloseAsync().ConfigureAwait(false);
                return true;
            }
        }
        catch (Exception ex)
        {
            await channel.CloseAsync().ConfigureAwait(false);
            if (ex is not OperationCanceledException)
                Logger?.Exception(ex);
        }
        finally
        {
            AutoConnect = true;
            ChannelStartedWaitLock.Release();
            await base.ChannelStarted(channel, last).ConfigureAwait(false);
        }

        return true;
    }

    #endregion 初始握手

    #region 其他方法

    /// <summary>
    /// 读取日期
    /// </summary>
    /// <returns></returns>
    public async ValueTask<OperResult<System.DateTime>> ReadDateAsync(string address, CancellationToken cancellationToken)
    {
        return (await ReadAsync(address, 2, cancellationToken).ConfigureAwait(false)).
             Then(m => OperResult.CreateSuccessResult(S7DateTime.SpecMinimumDateTime.AddDays(
                 ThingsGatewayBitConverter.ToUInt16(m.Span, 0)))
             );
    }

    /// <summary>
    /// 读取时间
    /// </summary>
    /// <returns></returns>
    public async ValueTask<OperResult<System.DateTime>> ReadDateTimeAsync(string address, CancellationToken cancellationToken)
    {
        return OperResultExtension.GetResultFromBytes(await ReadAsync(address, 8, cancellationToken).ConfigureAwait(false), (a) => S7DateTime.FromByteArray(a.Span));
    }

    /// <summary>
    /// 写入日期
    /// </summary>
    /// <returns></returns>
    public ValueTask<OperResult> WriteDateAsync(string address, System.DateTime dateTime, CancellationToken cancellationToken)
    {
        return base.WriteAsync(address, Convert.ToUInt16((dateTime - S7DateTime.SpecMinimumDateTime).TotalDays), null, cancellationToken);
    }

    /// <summary>
    /// 写入时间
    /// </summary>
    /// <returns></returns>
    public ValueTask<OperResult> WriteDateTimeAsync(string address, System.DateTime dateTime, CancellationToken cancellationToken)
    {
        return WriteAsync(address, S7DateTime.ToByteArray(dateTime), DataTypeEnum.Byte, cancellationToken);
    }

    #endregion 其他方法

    #region 字符串读写

    /// <inheritdoc/>
    public override async ValueTask<OperResult<string[]>> ReadStringAsync(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);

        if (((S7BitConverter)bitConverter)?.WStringEnable == true)
        {
            if (length > 1)
            {
                return new OperResult<string[]>(AppResource.StringLengthReadError);
            }
            var result = await SiemensHelper.ReadWStringAsync(this, address, bitConverter.Encoding, cancellationToken).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                return OperResult.CreateSuccessResult(new string[] { result.Content });
            }
            else
            {
                return new OperResult<string[]>(result);
            }
        }
        else if (bitConverter.IsVariableStringLength)
        {
            if (length > 1)
            {
                return new OperResult<string[]>(AppResource.StringLengthReadError);
            }
            var result = await SiemensHelper.ReadStringAsync(this, address, bitConverter.EncodingValue, cancellationToken).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                return OperResult.CreateSuccessResult(new string[] { result.Content });
            }
            else
            {
                return new OperResult<string[]>(result);
            }
        }
        else
        {
            return await base.ReadStringAsync(address, length, bitConverter, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public override ValueTask<OperResult> WriteAsync(string address, string value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(address);

        if (((S7BitConverter)bitConverter)?.WStringEnable == true)
        {
            return SiemensHelper.WriteWStringAsync(this, address, value, bitConverter.Encoding, cancellationToken);
        }
        if (bitConverter.IsVariableStringLength)
        {
            return SiemensHelper.WriteStringAsync(this, address, value, bitConverter.EncodingValue, cancellationToken);
        }
        else
        {
            return base.WriteAsync(address, value, bitConverter, cancellationToken);
        }
    }

    #endregion 字符串读写
}
