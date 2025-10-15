//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ThingsGateway.Foundation;

[DebuggerDisplay("Count={Count}")]
internal class InternalConcurrentDictionary<TKey, TValue> : IEnumerable<TValue>
{
    private readonly NonBlockingDictionary<TKey, TValue> m_clients = new();

    public int Count => m_clients.Count;

    public TValue this[TKey id]
    {
        get
        {
            return m_clients[id];
        }
        set
        {
            m_clients[id] = value;
        }
    }

    public bool ClientExist(TKey id)
    {
        return id != null && m_clients.ContainsKey(id);
    }

    IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
    {
        return m_clients.Values.GetEnumerator();
    }

    public IEnumerator GetEnumerator()
    {
        return m_clients.Values.GetEnumerator();
    }

    /// <inheritdoc/>
    public IEnumerable<TKey> GetIds()
    {
        return m_clients.Keys;
    }

    public bool TryAdd(TKey id, TValue client)
    {
        return m_clients.TryAdd(id, client);
    }

    /// <inheritdoc/>
    public bool TryGetValue(TKey id, out TValue client)
    {
        if (id == null)
        {
            client = default;
            return false;
        }

        return m_clients.TryGetValue(id, out client);
    }

    /// <summary>
    /// 移除对应客户端
    /// </summary>
    /// <param name="id"></param>
    /// <param name="client"></param>
    /// <returns></returns>
    public bool TryRemove(TKey id, out TValue client)
    {
        if (id == null)
        {
            client = default;
            return false;
        }

        if (m_clients.TryRemove(id, out var newClient))
        {
            client = newClient;
            return true;
        }
        client = default;
        return false;
    }
}