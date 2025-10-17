//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation;

using BenchmarkDotNet.Attributes;

using PooledAwait;

using System.Threading;
using System.Threading.Tasks;

using ThingsGateway.NewLife;

[MemoryDiagnoser]
[ThreadingDiagnoser]
public class SemaphoreBenchmark
{
    private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);
    private readonly WaitLock _waitLock = new WaitLock("SemaphoreBenchmark");

    [Params(100)]
    public int Iterations;

    [Benchmark(Baseline = true)]
    public async Task SemaphoreSlim_WaitRelease()
    {

        for (int i = 0; i < Iterations; i++)
        {
            await _semaphoreSlim.WaitAsync().ConfigureAwait(false);

            await Task.Delay(1);
            _semaphoreSlim.Release();
        }

    }


    [Benchmark]
    public async Task ReusableAsyncSemaphore_WaitRelease()
    {
        for (int i = 0; i < Iterations; i++)
        {
            await _waitLock.WaitAsync().ConfigureAwait(false);

            await Task.Delay(1);
            _waitLock.Release();
        }
    }
    [Benchmark]
    public async Task SemaphoreSlim_WaitReleaseToken()
    {

        for (int i = 0; i < Iterations; i++)
        {
            using var cts = new CancellationTokenSource(1000);

            await _semaphoreSlim.WaitAsync(cts.Token).ConfigureAwait(false);

            await Task.Delay(1);
            _semaphoreSlim.Release();
        }

    }


    [Benchmark]
    public ValueTask ReusableAsyncSemaphore_WaitReleaseToken()
    {
        return ReusableAsyncSemaphore_WaitReleaseToken(this);

        static async PooledValueTask ReusableAsyncSemaphore_WaitReleaseToken(SemaphoreBenchmark @this)
        {
            for (int i = 0; i < @this.Iterations; i++)
            {
                using var cts = new CancellationTokenSource(1000);

                await @this._waitLock.WaitAsync(cts.Token).ConfigureAwait(false);

                await Task.Delay(1);
                @this._waitLock.Release();
            }
        }
    }
}

