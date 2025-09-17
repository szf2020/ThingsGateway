//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

using ThingsGateway.Foundation;

namespace BenchmarkConsoleApp
{
    internal class Program
    {
        public static int ClientCount = 50;
        public static int TaskNumberOfItems = 1;
        public static int NumberOfItems = 50;

        private static async Task Main(string[] args)
        {
            Console.WriteLine("开始测试前，请先启动ModbusSlave，建议使用本项目自带的ThingsGateway.Debug.Photino软件开启，S7可以用KEPSERVER的S7模拟服务");
            Console.WriteLine($"多客户端({ClientCount}),多线程({TaskNumberOfItems})并发读取({NumberOfItems})测试，共{ClientCount * TaskNumberOfItems * NumberOfItems}次");
            await Task.CompletedTask;
            //ModbusBenchmark modbusBenchmark = new ModbusBenchmark();
            //System.Diagnostics.Stopwatch stopwatch = new();
            //stopwatch.Start();
            //await modbusBenchmark.ThingsGateway();
            //stopwatch.Stop();
            //Console.WriteLine($"ThingsGateway耗时：{stopwatch.ElapsedMilliseconds}ms");
            //stopwatch.Restart();
            //await modbusBenchmark.TouchSocket();
            //stopwatch.Stop();
            //Console.WriteLine($"TouchSocket耗时：{stopwatch.ElapsedMilliseconds}ms");
            //Console.ReadLine();

            //            BenchmarkRunner.Run<TimeoutBenchmark>(
            //ManualConfig.Create(DefaultConfig.Instance)
            //.WithOptions(ConfigOptions.DisableOptimizationsValidator)
            //);
            BenchmarkRunner.Run<ModbusBenchmark>(
       ManualConfig.Create(DefaultConfig.Instance)
           .WithOptions(ConfigOptions.DisableOptimizationsValidator)
   );
            //            BenchmarkRunner.Run<S7Benchmark>(
            //ManualConfig.Create(DefaultConfig.Instance)
            //.WithOptions(ConfigOptions.DisableOptimizationsValidator)
            //);

        }
    }
}