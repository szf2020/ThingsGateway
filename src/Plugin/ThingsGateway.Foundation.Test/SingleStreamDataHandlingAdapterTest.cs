// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using System.IO.Pipelines;

using TouchSocket.Core;

namespace ThingsGateway.Foundation.Test;

public class SingleStreamDataHandlingAdapterTest
{
    private readonly Pipe m_pipe = new Pipe(new PipeOptions(new SmallBlockMemoryPool(1), default, default, 1024 * 1024 * 1024, 1024 * 1024 * 512, -1));
    public async Task SendCallback(ReadOnlyMemory<byte> memory, Func<int, Task> func, int bufferLength = 1, CancellationToken token = default)
    {
        try
        {
            var offset = 0;
            while (true)
            {
                token.ThrowIfCancellationRequested();

                var remainingLength = memory.Length - offset;
                if (remainingLength <= 0)
                {
                    break;
                }
                var sliceMemory = memory.Slice(offset, Math.Min(remainingLength, bufferLength));
                await this.m_pipe.Writer.WriteAsync(sliceMemory, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                offset += sliceMemory.Length;

                //await this.m_pipe.Writer.FlushAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                await func(offset).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
        }
        finally
        {
            await this.m_pipe.Writer.CompleteAsync().ConfigureAwait(false);
            await this.m_pipe.Reader.CompleteAsync().ConfigureAwait(false);
        }
    }
    public async Task ReceivedAsync(SingleStreamDataHandlingAdapter adapter, CancellationToken token)
    {
        try
        {
            var readResult = await this.m_pipe.Reader.ReadAsync(token).ConfigureAwait(false);
            var sequence = readResult.Buffer;
            var reader = new ClassBytesReader(sequence);
            await adapter.ReceivedInputAsync(reader).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            var position = sequence.GetPosition(reader.BytesRead);
            // 通知PipeReader已消费
            this.m_pipe.Reader.AdvanceTo(position, sequence.End);
            // 如果本次ReadAsync已完成或被取消，直接返回
            if (readResult.IsCanceled || readResult.IsCompleted)
            {
                return;
            }

        }
        finally
        {

        }
    }
}
