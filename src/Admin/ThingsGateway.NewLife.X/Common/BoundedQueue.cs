namespace ThingsGateway.NewLife;

using System;
using System.Collections;
using System.Collections.Generic;

public class BoundedQueue<T> : IEnumerable<T>
{
    private readonly Queue<T> _queue;
    private readonly int _capacity;
    private readonly object _syncRoot = new object();

    public BoundedQueue(int capacity)
    {
        if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        _capacity = capacity;
        _queue = new Queue<T>(capacity);
    }

    public void Enqueue(T item)
    {
        lock (_syncRoot)
        {
            if (_queue.Count == _capacity)
                _queue.Dequeue();
            _queue.Enqueue(item);
        }
    }

    public int Count
    {
        get { lock (_syncRoot) return _queue.Count; }
    }

    public IEnumerator<T> GetEnumerator()
    {
        lock (_syncRoot)
        {
            return new List<T>(_queue).GetEnumerator();
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

