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

namespace ThingsGateway.Foundation;

/// <summary>
/// TCP/Serial适配器基类
/// </summary>
public class DeviceSingleStreamDataHandleAdapter<TRequest> : CustomDataHandlingAdapter<TRequest>, IDeviceDataHandleAdapter where TRequest : MessageBase, new()
{
    public new ILog Logger { get; set; }


    /// <inheritdoc cref="DeviceSingleStreamDataHandleAdapter{TRequest}"/>
    public DeviceSingleStreamDataHandleAdapter()
    {
        CacheTimeoutEnable = true;
    }


    /// <inheritdoc/>
    public override bool CanSendRequestInfo => true;

    /// <summary>
    /// 报文输出时采用字符串还是HexString
    /// </summary>
    public virtual bool IsHexLog { get; set; } = true;

    public virtual bool IsSingleThread { get; set; } = true;

    /// <summary>
    /// 非并发协议中，每次交互的对象，会在发送时重新获取
    /// </summary>
    public TRequest Request { get; set; }

    /// <inheritdoc />
    public void SetRequest(ISendMessage sendMessage)
    {
        var request = GetInstance();
        request.Sign = sendMessage.Sign;
        request.SendInfo(sendMessage);
        Request = request;
    }

    /// <inheritdoc/>
    public override string? ToString()
    {
        return Owner?.ToString();
    }

    /// <inheritdoc />
    protected override FilterResult Filter<TReader>(ref TReader byteBlock, bool beCached, ref TRequest request)
    {
        if (Logger?.LogLevel <= LogLevel.Trace)
            Logger?.Trace($"{ToString()}- Receive:{(IsHexLog ? byteBlock.ToHexString(byteBlock.BytesRead, ' ') : byteBlock.ToString(byteBlock.BytesRead))}");

        try
        {
            if (IsSingleThread)
                request = Request == null ? Request = GetInstance() : Request;
            else
            {
                if (!beCached)
                    request = GetInstance();
            }

            var pos = byteBlock.BytesRead;

            if (request.HeaderLength > byteBlock.BytesRemaining)
            {
                return FilterResult.Cache;//当头部都无法解析时，直接缓存
            }

            //检查头部合法性
            if (request.CheckHead(ref byteBlock))
            {
                byteBlock.BytesRead = pos;
                if (request.BodyLength > MaxPackageSize)
                {
                    request.OperCode = -1;
                    request.ErrorMessage = $"Received BodyLength={request.BodyLength}, greater than the set MaxPackageSize={MaxPackageSize}";
                    Reset();
                    Logger?.LogWarning($"{ToString()} {request.ErrorMessage}");
                    return FilterResult.GoOn;
                }
                if (request.BodyLength + request.HeaderLength > byteBlock.BytesRemaining)
                {
                    //body不满足解析，开始缓存，然后保存对象
                    return FilterResult.Cache;
                }
                //if (request.BodyLength <= 0)
                //{
                //    //如果body长度无法确定，直接读取全部
                //    request.BodyLength = byteBlock.Length;
                //}
                var headPos = pos + request.HeaderLength;
                byteBlock.BytesRead = headPos;
                var result = request.CheckBody(ref byteBlock);
                if (result == FilterResult.Cache)
                {
                    byteBlock.BytesRead = pos;
                    if (Logger?.LogLevel <= LogLevel.Trace)
                        Logger?.Trace($"{ToString()}-Received incomplete, cached message, need length:{request.HeaderLength + request.BodyLength} ,current length:{byteBlock.BytesRead + byteBlock.BytesRemaining}  {request?.ErrorMessage}");
                    request.OperCode = -1;
                }
                else if (result == FilterResult.GoOn)
                {
                    var addLen = request.HeaderLength + request.BodyLength;
                    byteBlock.BytesRead = pos + (addLen > 0 ? addLen : 1);
                    if (Logger?.LogLevel <= LogLevel.Trace)
                        Logger?.Trace($"{ToString()}-{request?.ToString()}");
                    request.OperCode = -1;
                }
                else if (result == FilterResult.Success)
                {
                    var addLen = request.HeaderLength + request.BodyLength;
                    byteBlock.BytesRead = pos + (addLen > 0 ? addLen : 1);
                }
                return result;
            }
            else
            {
                byteBlock.BytesRead = pos + 1;//移动游标
                request.OperCode = -1;
                return FilterResult.GoOn;//放弃解析
            }
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex, $"{ToString()} Received parsing error");
            byteBlock.BytesRead = byteBlock.BytesRead + byteBlock.BytesRemaining;//移动游标
            request.Exception = ex;
            request.OperCode = -1;
            return FilterResult.GoOn;//放弃解析
        }
    }


    /// <summary>
    /// 获取泛型实例。
    /// </summary>
    /// <returns></returns>
    protected virtual TRequest GetInstance()
    {
        return new TRequest() { OperCode = -1, Sign = -1 };
    }

    public override void SendInput<TWriter>(ref TWriter writer, in ReadOnlyMemory<byte> memory)
    {
        if (Logger?.LogLevel <= LogLevel.Trace)
            Logger?.Trace($"{ToString()}- Send:{(IsHexLog ? memory.Span.ToHexString(' ') : (memory.Span.ToString(Encoding.UTF8)))}");

        writer.Write(memory.Span);
    }

    public override void SendInput<TWriter>(ref TWriter writer, IRequestInfo requestInfo)
    {
        if (!(requestInfo is ISendMessage sendMessage))
        {
            throw new Exception($"Unable to convert {nameof(requestInfo)} to {nameof(ISendMessage)}");
        }
        var span = writer.GetSpan(sendMessage.MaxLength);
        sendMessage.Build(ref writer);
        if (Logger?.LogLevel <= LogLevel.Trace)
        {
            Logger?.Trace($"{ToString()}- Send:{(IsHexLog ? span.Slice(0, (int)writer.WrittenCount).ToHexString(' ') : (span.Slice(0, (int)writer.WrittenCount).ToString(Encoding.UTF8)))}");
        }
        //非并发主从协议
        if (IsSingleThread)
        {
            SetRequest(sendMessage);
        }

    }

}
