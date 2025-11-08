//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BenchmarkDotNet.Diagnosers;

namespace ThingsGateway.Foundation;

using BenchmarkDotNet.Attributes;

using System;
using System.Collections.Generic;

using ThingsGateway.NewLife;

using TouchSocket.Core;

public class Foo
{
    public string Name { get; set; }
    public int Age { get; set; }
    public int Age1 { get; set; }
    public int Age2 { get; set; }
    public int Age3 { get; set; }
    public int Age11 { get; set; }
    public int Age21 { get; set; }
    public int Age31 { get; set; }

}

public class Bar
{
    public string Name { get; set; }
    public int Age { get; set; }
    public Bar Child { get; set; }
    public int Age1 { get; set; }
    public int Age2 { get; set; }
    public int Age3 { get; set; }
    public int Age31 { get; set; }
    public int Age32 { get; set; }
    public int Age33 { get; set; }
}

[RankColumn]
[MemoryDiagnoser]
public class MapperBench
{
    private List<Foo> foos;

    [GlobalSetup]
    public void Setup()
    {
        const int N = 100_000;
        foos = new List<Foo>(N);
        for (int i = 0; i < N; i++)
        {
            foos.Add(new Foo
            {
                Name = $"Name{i}",
                Age = i,
            });
        }
    }

    [Benchmark(Baseline = true)]
    public List<Bar> ManualConstructionMap100_000()
    {
        var bars = new List<Bar>(foos.Count);
        foreach (var f in foos)
        {
            bars.Add(new Bar
            {
                Name = f.Name,
                Age = f.Age,
            });
        }
        return bars;
    }

    [Benchmark]
    public List<Bar> ReflectionMap100_000()
    {
        var bars = new List<Bar>(foos.Count);
        foreach (var f in foos)
        {
            bars.Add(OriginalMapper(f));
        }
        return bars;
    }

    [Benchmark]
    public List<Bar> FastMapper100_000()
    {
        var bars = new List<Bar>(foos.Count);
        foreach (var f in foos)
        {
            bars.Add(FastMapper.Mapper<Foo, Bar>(f));
        }
        return bars;
    }



    private static Bar OriginalMapper(Foo source)
    {
        if (source == null) return null;

        var target = new Bar();
        var st = typeof(Foo).GetProperties();
        var tt = typeof(Bar).GetProperties();

        foreach (var sp in st)
        {
            var tp = Array.Find(tt, x => x.Name == sp.Name);
            if (tp == null || !tp.CanWrite) continue;

            var value = sp.GetValue(source);

            if (value == null)
            {
                tp.SetValue(target, null);
            }
            else if (IsSimpleType(sp.PropertyType))
            {
                // 基础类型直接赋值
                tp.SetValue(target, value);
            }
            else
            {
                // 嵌套对象递归映射
                tp.SetValue(target, value);
            }
        }

        return target;
    }

    private static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime);
    }

}




