//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.ComponentModel;
using System.Runtime;

using ThingsGateway.NewLife;

namespace ThingsGateway.Admin.Application;

/// <inheritdoc/>
public class HardwareInfo
{
    /// <summary>
    /// 当前磁盘信息
    /// </summary>
    public DriveInfo DriveInfo { get; set; }
 

    /// <summary>
    /// 主机环境
    /// </summary>
    public string Environment { get; set; }

    /// <summary>
    /// NET框架
    /// </summary>
    public string FrameworkDescription { get; set; }

    /// <summary>
    /// 系统架构
    /// </summary>
    public string OsArchitecture { get; set; }


    /// <summary>系统名称</summary>
    public String OSName { get; set; }

    /// <summary>系统版本</summary>
    public String OSVersion { get; set; }
    public String UUID { get; set; }

    /// <summary>内存总量。单位MB</summary>
    public UInt64 Memory { get; set; }

    /// <summary>可用内存。单位MB</summary>
    public UInt64 AvailableMemory { get; set; }

    /// <summary>CPU占用率</summary>
    public Double CpuRate { get; set; }
    public Double Battery { get; set; }
    public Double Temperature { get; set; }


    /// <summary>处理器型号</summary>
    public String? Processor { get; set; }
    #region GC与进程内存信息

    /// <summary>GC 认为“内存吃紧”的阈值。单位：MB</summary>
    [DisplayName("GC高内存阈值")]
    public UInt64 HighMemoryLoadThreshold { get; set; }

    /// <summary>GC 可用内存上限。单位：MB</summary>
    [DisplayName("GC可用内存上限")]
    public UInt64 TotalAvailableMemory { get; set; }

    /// <summary>当前托管堆容量。单位：MB</summary>
    [DisplayName("托管堆容量")]
    public UInt64 HeapSize { get; set; }

    /// <summary>托管堆已用内存。单位：MB</summary>
    [DisplayName("托管堆已用")]
    public UInt64 TotalMemory { get; set; }

    /// <summary>托管堆碎片大小。单位：MB</summary>
    [DisplayName("托管堆碎片")]
    public UInt64 FragmentedBytes { get; set; }

    /// <summary>GC识别可用内存。单位：MB</summary>
    [DisplayName("GC识别可用内存")]
    public UInt64 GCAvailableMemory { get; set; }

    /// <summary>GC 已提交的内存。单位：MB</summary>
    [DisplayName("GC已提交内存")]
    public UInt64 CommittedBytes { get; set; }

    /// <summary>GC 累计分配的托管内存。单位：MB</summary>
    [DisplayName("GC累计分配")]
    public UInt64 TotalAllocatedBytes { get; set; }
    /// <summary>GC 暂停累计时间。单位：毫秒</summary>
    [DisplayName("GC累计暂停时间")]
    public UInt64 TotalPauseDurationMs { get; set; }
    /// <summary>GC 代0收集次数</summary>
    [DisplayName("GC Gen0 次数")]
    public Int32 GcGen0Count { get; set; }

    /// <summary>GC 代1收集次数</summary>
    [DisplayName("GC Gen1 次数")]
    public Int32 GcGen1Count { get; set; }

    /// <summary>GC 代2收集次数</summary>
    [DisplayName("GC Gen2 次数")]
    public Int32 GcGen2Count { get; set; }

    /// <summary>Server GC 是否启用</summary>
    [DisplayName("是否使用Server GC")]
    public Boolean IsServerGC { get; set; }

    /// <summary>GC 延迟模式</summary>
    [DisplayName("GC延迟模式")]
    public GCLatencyMode? GCLatencyMode { get; set; }

    /// <summary>GC 固定对象数</summary>
    [DisplayName("固定对象数")]
    public Int64 PinnedObjectsCount { get; set; }

    /// <summary>终结队列挂起对象数</summary>
    [DisplayName("终结挂起数")]
    public Int64 FinalizationPendingCount { get; set; }

    #endregion

    #region 进程内存信息

    /// <summary>进程虚拟内存使用量。单位：MB</summary>
    [DisplayName("虚拟内存")]
    public UInt64 VirtualMemory { get; set; }

    /// <summary>进程私有内存使用量。单位：MB</summary>
    [DisplayName("私有内存")]
    public UInt64 PrivateMemory { get; set; }

    /// <summary>进程峰值工作集。单位：MB</summary>
    [DisplayName("峰值工作集")]
    public UInt64 PeakWorkingSet { get; set; }

    /// <summary>进程当前工作集。单位：MB</summary>
    [DisplayName("当前工作集")]
    public UInt64 WorkingSet { get; set; }

    #endregion

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdateTime { get; set; }
    public ulong AppRunTotalMinute { get;  set; }
    public ulong SystemRunTotalMinute { get;  set; }
}
