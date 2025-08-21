// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.Test;

using System;
using System.Buffers;

public class SmallBlockMemoryPool : MemoryPool<byte>
{
    private readonly int _blockSize;

    public SmallBlockMemoryPool(int blockSize)
    {
        _blockSize = blockSize;
    }

    public override int MaxBufferSize => _blockSize;

    public override IMemoryOwner<byte> Rent(int minBufferSize = -1)
    {
        if (minBufferSize <= 0) minBufferSize = _blockSize;
        return new SmallBlockOwner(new byte[Math.Min(minBufferSize, _blockSize)]);
    }

    protected override void Dispose(bool disposing) { }

    private class SmallBlockOwner : IMemoryOwner<byte>
    {
        private readonly byte[] _array;
        public SmallBlockOwner(byte[] array) => _array = array;
        public Memory<byte> Memory => _array;
        public void Dispose() { }
    }
}

