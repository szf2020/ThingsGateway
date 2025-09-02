//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Net;
using System.Text;

using ThingsGateway.NewLife.Collections;

namespace ThingsGateway.Foundation;

/// <summary>
/// UDP适配器基类
/// </summary>
public class DeviceUdpDataHandleAdapter<TRequest> : UdpDataHandlingAdapter, IDeviceDataHandleAdapter where TRequest : MessageBase, new()
{
    private ILog logger;

    public new ILog Logger
    {
        get => logger ?? base.Logger;
        set
        {
            if (value != logger && value != null)
            {
                logger = value;
            }
        }
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
        if (IsSingleThread)
        {
            if (Request != null)
            {
                _requestPool.Return(Request);
            }
        }
        var request = GetInstance();
        request.Sign = sendMessage.Sign;
        request.SendInfo(sendMessage);
        Request = request;
    }

    /// <inheritdoc/>
    public override string? ToString()
    {
        return Owner.ToString();
    }

    private static ObjectPool<TRequest> _requestPool { get; } = new ObjectPool<TRequest>();


    /// <summary>
    /// 获取泛型实例。
    /// </summary>
    /// <returns></returns>
    protected virtual TRequest GetInstance()
    {
        if (IsSingleThread)
        {
            var request = _requestPool.Get();
            request.OperCode = -1;
            request.Sign = -1;
            return request;
        }
        else
        {
            return new TRequest() { OperCode = -1, Sign = -1 };
        }
    }


    #region ParseRequest
    /// <summary>
    /// 尝试从字节读取器中解析出请求信息。
    /// </summary>
    /// <param name="remoteEndPoint">remoteEndPoint。</param>
    /// <param name="memory">memory。</param>
    /// <param name="request">解析出的请求信息。</param>
    /// <returns>解析成功返回 true，否则返回 false。</returns>
    public bool TryParseRequest(EndPoint remoteEndPoint, ReadOnlyMemory<byte> memory, out TRequest request)
    {
        return this.ParseRequestCore(remoteEndPoint, memory, out request);
    }

    protected virtual bool ParseRequestCore(EndPoint remoteEndPoint, ReadOnlyMemory<byte> memory, out TRequest request1)
    {
        request1 = null;
        try
        {
            if (Logger?.LogLevel <= LogLevel.Trace)
                Logger?.Trace($"{remoteEndPoint}- Receive:{(IsHexLog ? memory.Span.ToHexString(' ') : memory.Span.ToString(Encoding.UTF8))}");

            TRequest request = null;
            if (IsSingleThread)
                request = Request == null ? Request = GetInstance() : Request;
            else
            {
                request = GetInstance();
            }
            request1 = request;

            var byteBlock = new ClassBytesReader(memory);
            byteBlock.BytesRead = 0;

            var pos = byteBlock.BytesRead;

            if (request.HeaderLength > byteBlock.BytesRead + byteBlock.BytesRemaining)
            {
                return false;//当头部都无法解析时，直接缓存
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
                    return false;
                }
                if (request.BodyLength + request.HeaderLength > byteBlock.BytesRead + byteBlock.BytesRemaining)
                {
                    return false;
                }

                var headPos = pos + request.HeaderLength;
                byteBlock.BytesRead = headPos;
                var result = request.CheckBody(ref byteBlock);
                if (result == FilterResult.Cache)
                {
                    if (Logger?.LogLevel <= LogLevel.Trace)
                        Logger?.Trace($"{ToString()}-Received incomplete, cached message, need length:{request.HeaderLength + request.BodyLength} ,current length:{byteBlock.BytesRead + byteBlock.BytesRemaining}  {request?.ErrorMessage}");
                    request.OperCode = -1;
                }
                else if (result == FilterResult.GoOn)
                {
                    if (Logger?.LogLevel <= LogLevel.Trace)
                        Logger?.Trace($"{ToString()}-{request?.ToString()}");
                    request.OperCode = -1;
                }
                else if (result == FilterResult.Success)
                {
                    request1 = request;
                    return true;
                }
            }
            else
            {
                request.OperCode = -1;
                return false;
            }
            return false;
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex, $"{ToString()} Received parsing error");
            return false;
        }
    }

    #endregion

    /// <inheritdoc/>
    protected override Task PreviewReceivedAsync(EndPoint remoteEndPoint, ReadOnlyMemory<byte> memory)
    {
        if (ParseRequestCore(remoteEndPoint, memory, out var request))
        {
            return GoReceived(remoteEndPoint, null, request);
        }
        return EasyTask.CompletedTask;
    }

    /// <inheritdoc/>
    protected override Task PreviewSendAsync(EndPoint endPoint, ReadOnlyMemory<byte> memory, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (Logger?.LogLevel <= LogLevel.Trace)
            Logger?.Trace($"{ToString()}- Send:{(IsHexLog ? memory.Span.ToHexString(' ') : (memory.Span.ToString(Encoding.UTF8)))}");
        //发送
        return GoSendAsync(endPoint, memory, cancellationToken);
    }

    /// <inheritdoc/>
    protected override Task PreviewSendAsync(EndPoint endPoint, IRequestInfo requestInfo, CancellationToken cancellationToken)
    {
        if (!(requestInfo is ISendMessage sendMessage))
        {
            throw new Exception($"Unable to convert {nameof(requestInfo)} to {nameof(ISendMessage)}");
        }
        cancellationToken.ThrowIfCancellationRequested();

        var byteBlock = new ValueByteBlock(sendMessage.MaxLength);
        try
        {
            sendMessage.Build(ref byteBlock);
            if (Logger?.LogLevel <= LogLevel.Trace)
                Logger?.Trace($"{endPoint}- Send:{(IsHexLog ? byteBlock.Span.ToHexString(' ') : (byteBlock.Span.ToString(Encoding.UTF8)))}");

            if (IsSingleThread)
            {
                SetRequest(sendMessage);
            }
            return GoSendAsync(endPoint, byteBlock.Memory, cancellationToken);
        }
        finally
        {
            byteBlock.SafeDispose();
        }
    }
}
