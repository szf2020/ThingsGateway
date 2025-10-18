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

using RabbitMQ.Client;

using ThingsGateway.Foundation;

namespace ThingsGateway.Plugin.RabbitMQ;

/// <summary>
/// RabbitMQProducer
/// </summary>
public partial class RabbitMQProducer : BusinessBaseWithCacheIntervalScriptAll
{
    private readonly RabbitMQProducerProperty _driverPropertys = new();
    private readonly RabbitMQProducerVariableProperty _variablePropertys = new();
    public override VariablePropertyBase VariablePropertys => _variablePropertys;

    protected override BusinessPropertyWithCacheIntervalScript _businessPropertyWithCacheIntervalScript => _driverPropertys;
    /// <inheritdoc/>
    public override string ToString()
    {
        return $" {nameof(RabbitMQProducer)} IP:{_driverPropertys.IP} Port:{_driverPropertys.Port}";
    }


#if !Management
    protected override async Task InitChannelAsync(Foundation.IChannel? channel, CancellationToken cancellationToken)
    {
        #region 初始化

        _connectionFactory = new ConnectionFactory
        {
            HostName = _driverPropertys.IP,
            Port = _driverPropertys.Port,
            UserName = _driverPropertys.UserName,
            Password = _driverPropertys.Password,
            VirtualHost = _driverPropertys.VirtualHost,
        };

        #endregion 初始化
        await base.InitChannelAsync(channel, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override bool IsConnected() => success;



    /// <inheritdoc/>
    protected override async Task DisposeAsync(bool disposing)
    {
        if (_channel != null)
            await _channel.SafeDisposeAsync().ConfigureAwait(false);
        if (_connection != null)
            await _connection.SafeDisposeAsync().ConfigureAwait(false);
        await base.DisposeAsync(disposing).ConfigureAwait(false);
    }

    protected override Task ProtectedExecuteAsync(object? state, CancellationToken cancellationToken)
    {
        return ProtectedExecuteAsync(this, cancellationToken);

        static async PooledTask ProtectedExecuteAsync(RabbitMQProducer @this, CancellationToken cancellationToken)
        {
            if (@this._channel == null)
            {
                try
                {
                    // 创建连接
                    @this._connection ??= await @this._connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
                    // 创建通道
                    @this._channel ??= await @this._connection.CreateChannelAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
                    // 声明路由队列
                    if (@this._driverPropertys.IsQueueDeclare)
                    {
                        await (@this._channel?.QueueDeclareAsync(@this._driverPropertys.VariableTopic, true, false, false, cancellationToken: cancellationToken)).ConfigureAwait(false);
                        await (@this._channel?.QueueDeclareAsync(@this._driverPropertys.DeviceTopic, true, false, false, cancellationToken: cancellationToken)).ConfigureAwait(false);
                        await (@this._channel?.QueueDeclareAsync(@this._driverPropertys.AlarmTopic, true, false, false, cancellationToken: cancellationToken)).ConfigureAwait(false);
                    }
                    @this.success = true;
                }
                catch (Exception ex)
                {
                    if (@this.success)
                    {
                        @this.LogMessage?.LogWarning(ex);
                        @this.success = false;
                    }
                    await Task.Delay(10000, cancellationToken).ConfigureAwait(false);
                }
            }
            else
            {
                await @this.Update(cancellationToken).ConfigureAwait(false);
            }
        }
    }

#endif
}
