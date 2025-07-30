//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation;


/// <summary>
/// 定义了字节块构建器的接口，用于从内存池中构建和管理字节块。
/// </summary>
public interface IByteBlockWriterBuilder
{
    /// <summary>
    /// 构建数据时，指示内存池的申请长度。
    /// <para>
    /// 建议：该值可以尽可能的设置大一些，这样可以避免内存池扩容。
    /// </para>
    /// </summary>
    int MaxLength { get; }

    /// <summary>
    /// 构建对象到<see cref="ByteBlock"/>
    /// </summary>
    /// <param name="writer">要构建的字节块对象引用。</param>
    void Build<TWriter>(ref TWriter writer) where TWriter : IByteBlockWriter
#if AllowsRefStruct
,allows ref struct
#endif
        ;
}



/// <summary>
/// 指示<see cref="IRequestInfo"/>应当如何构建
/// </summary>
public interface IRequestInfoByteBlockWriterBuilder : IRequestInfo, IByteBlockWriterBuilder
{

}