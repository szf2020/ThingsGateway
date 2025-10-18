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

using ThingsGateway.Foundation.Extension.String;

namespace ThingsGateway.Foundation;

[PluginOption(Singleton = true)]
internal sealed class HeartbeatAndReceivePlugin : PluginBase, ITcpConnectedPlugin, ITcpReceivingPlugin, ITcpClosedPlugin
{
    public string DtuId
    {
        get
        {
            return _dtuId;
        }
        set
        {
            _dtuId = value;
            if (!dtuIdHex)
                DtuIdByte = (Encoding.UTF8.GetBytes(_dtuId ?? string.Empty));
            else
                DtuIdByte = (_dtuId?.HexStringToBytes() ?? Array.Empty<byte>());
        }
    }
    private string _dtuId;
    private Memory<byte> DtuIdByte;

    /// <summary>
    /// 心跳字符串
    /// </summary>
    public string Heartbeat
    {
        get
        {
            return _heartbeat;
        }
        set
        {
            _heartbeat = value;
            if (!heartbeatHex)
                HeartbeatByte = (Encoding.UTF8.GetBytes(_heartbeat ?? string.Empty));
            else
                HeartbeatByte = (_heartbeat?.HexStringToBytes() ?? Array.Empty<byte>());
        }
    }
    private string _heartbeat;
    private Memory<byte> HeartbeatByte;






    private bool heartbeatHex;
    public bool HeartbeatHex
    {
        get
        {
            return heartbeatHex;
        }
        set
        {
            heartbeatHex = value;
            if (!heartbeatHex)
            {
                HeartbeatByte = (Encoding.UTF8.GetBytes(_heartbeat ?? string.Empty));

            }
            else
            {
                HeartbeatByte = (_heartbeat?.HexStringToBytes() ?? Array.Empty<byte>());

            }
        }
    }
    private bool dtuIdHex;
    public bool DtuIdHex
    {
        get
        {
            return dtuIdHex;
        }
        set
        {
            dtuIdHex = value;
            if (!dtuIdHex)
            {
                DtuIdByte = (Encoding.UTF8.GetBytes(_dtuId ?? string.Empty));

            }
            else
            {
                DtuIdByte = (_dtuId?.HexStringToBytes() ?? Array.Empty<byte>());

            }
        }
    }
    private Task _task;
    private bool SendHeartbeat;
    public int HeartbeatTime { get; set; } = 3000;

    public async Task OnTcpConnected(ITcpSession client, ConnectedEventArgs e)
    {
        if (client is ITcpSessionClient)
        {
            return;//此处可判断，如果为服务器，则不用使用心跳。
        }
        if (HeartbeatTime > 0)
            SendHeartbeat = true;
        HeartbeatTime = Math.Max(HeartbeatTime, 1000);

        if (DtuId.IsNullOrWhiteSpace()) return;

        if (client is ITcpClient tcpClient)
        {
            await tcpClient.SendAsync(DtuIdByte, tcpClient.ClosedToken).ConfigureAwait(false);

            if (_task == null)
            {
                _task = Task.Run(async () =>
                 {
                     var failedCount = 0;
                     while (SendHeartbeat)
                     {
                         await Task.Delay(HeartbeatTime, client.ClosedToken).ConfigureAwait(false);
                         if (!client.Online)
                         {
                             break;
                         }

                         try
                         {
                             if (DateTimeOffset.Now - tcpClient.LastSentTime < TimeSpan.FromMilliseconds(200))
                             {
                                 await Task.Delay(200).ConfigureAwait(false);
                             }

                             await tcpClient.SendAsync(HeartbeatByte, tcpClient.ClosedToken).ConfigureAwait(false);
                             tcpClient.Logger?.Trace($"{tcpClient}- Heartbeat");
                             failedCount = 0;
                         }
                         catch
                         {
                             failedCount++;
                         }
                         if (failedCount > 3)
                         {
                             await client.CloseAsync("The automatic heartbeat has failed more than 3 times and has been disconnected.").ConfigureAwait(false);
                         }
                     }

                     _task = null;
                 });
            }
        }

        await e.InvokeNext().ConfigureAwait(false);
    }

    public Task OnTcpReceiving(ITcpSession client, BytesReaderEventArgs e)
    {
        if (client is ITcpSessionClient)
        {
            return Task.CompletedTask;//此处可判断，如果为服务器，则不用使用心跳。
        }

        if (DtuId.IsNullOrWhiteSpace()) return Task.CompletedTask;

        if (client is ITcpClient tcpClient)
        {
            var len = HeartbeatByte.Length;
            if (len > 0)
            {
                if (HeartbeatByte.Span.SequenceEqual(e.Reader.TotalSequence.Slice(0, (int)Math.Min(len, e.Reader.BytesRemaining + e.Reader.BytesRead)).First.Span))
                {
                    e.Reader.Advance((int)Math.Min(len, e.Reader.BytesRemaining));
                    e.Handled = true;
                }
            }
            return e.InvokeNext();//如果本插件无法处理当前数据，请将数据转至下一个插件。
        }
        return Task.CompletedTask;
    }


    public Task OnTcpClosed(ITcpSession client, ClosedEventArgs e)
    {
        SendHeartbeat = false;
        return EasyTask.CompletedTask;
    }
}
