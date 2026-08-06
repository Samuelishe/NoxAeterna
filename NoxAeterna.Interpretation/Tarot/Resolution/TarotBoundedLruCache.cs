namespace NoxAeterna.Interpretation.Tarot.Resolution;

/// <summary>Small deterministic least-recently-used cache with explicit invalidation.</summary>
internal sealed class TarotBoundedLruCache<TKey, TValue>
    where TKey : notnull
{
    private readonly int capacity;
    private readonly Dictionary<TKey, LinkedListNode<(TKey Key, TValue Value)>> items = [];
    private readonly LinkedList<(TKey Key, TValue Value)> recency = [];

    public TarotBoundedLruCache(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        this.capacity = capacity;
    }

    public int Capacity => capacity;
    public int Count => items.Count;

    public bool TryGetValue(TKey key, out TValue? value)
    {
        if (!items.TryGetValue(key, out var node))
        {
            value = default;
            return false;
        }

        recency.Remove(node);
        recency.AddFirst(node);
        value = node.Value.Value;
        return true;
    }

    public void Set(TKey key, TValue value)
    {
        if (items.TryGetValue(key, out var existing))
        {
            existing.Value = (key, value);
            recency.Remove(existing);
            recency.AddFirst(existing);
            return;
        }

        var node = recency.AddFirst((key, value));
        items.Add(key, node);
        if (items.Count <= capacity)
        {
            return;
        }

        var evicted = recency.Last!;
        recency.RemoveLast();
        items.Remove(evicted.Value.Key);
    }

    public void RemoveWhere(Func<TKey, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        foreach (var key in items.Keys.Where(predicate).ToArray())
        {
            var node = items[key];
            items.Remove(key);
            recency.Remove(node);
        }
    }

    public void Clear()
    {
        items.Clear();
        recency.Clear();
    }
}
