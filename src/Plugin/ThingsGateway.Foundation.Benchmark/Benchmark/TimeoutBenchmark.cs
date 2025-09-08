//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

using ThingsGateway.NewLife.Collections;

namespace ThingsGateway.Foundation;

[MemoryDiagnoser]
public class TimeoutBenchmark
{


    [Benchmark]
    public async ValueTask CtsWaitAsync()
    {
        using var otherCts = new CancellationTokenSource();
        for (int i1 = 0; i1 < 10; i1++)
            for (int i = 0; i < 10; i++)
            {
                using var ctsTime = new CancellationTokenSource(TimeSpan.FromMilliseconds(10));
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ctsTime.Token, otherCts.Token);

                await Task.Delay(5, cts.Token).ConfigureAwait(false); // 模拟工作

            }
    }

    private ObjectPool<ReusableCancellationTokenSource> _reusableTimeouts;
    [Benchmark]
    public async ValueTask ReusableTimeoutWaitAsync()
    {
        _reusableTimeouts ??= new();
        using var otherCts = new CancellationTokenSource();
        for (int i1 = 0; i1 < 10; i1++)
            for (int i = 0; i < 10; i++)
            {
                var _reusableTimeout = _reusableTimeouts.Get();
                try
                {
                    await Task.Delay(5, _reusableTimeout.GetTokenSource(10, otherCts.Token).Token).ConfigureAwait(false); // 模拟工作
                }
                finally
                {
                    _reusableTimeouts.Return(_reusableTimeout);
                }
            }

        _reusableTimeouts.Dispose();
    }
}


