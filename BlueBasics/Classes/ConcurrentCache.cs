// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Collections.Concurrent;

namespace BlueBasics.Classes;

public class ConcurrentCache<TKey, TValue> where TKey : notnull {

    #region Fields

    private readonly ConcurrentDictionary<TKey, TValue> _dict;
    private readonly int _maxCacheSize;

    #endregion

    #region Constructors

    public ConcurrentCache(int maxCacheSize) {
        _maxCacheSize = maxCacheSize;
        _dict = new ConcurrentDictionary<TKey, TValue>();
        Generic.RegisterCacheTrim(() => Trim(_maxCacheSize));
    }

    public ConcurrentCache(IEqualityComparer<TKey> comparer, int maxCacheSize) {
        _maxCacheSize = maxCacheSize;
        _dict = new ConcurrentDictionary<TKey, TValue>(comparer);
        Generic.RegisterCacheTrim(() => Trim(_maxCacheSize));
    }

    #endregion

    #region Properties

    public int Count => _dict.Count;
    public ICollection<TKey> Keys => _dict.Keys;
    public ICollection<TValue> Values => _dict.Values;

    #endregion

    #region Indexers

    public TValue this[TKey key] {
        get => _dict[key];
        set => _dict[key] = value!;
    }

    #endregion

    #region Methods

    public void Clear() {
        foreach (var value in _dict.Values) {
            if (value is IDisposable d) { d.Dispose(); }
        }
        _dict.Clear();
    }

    public bool ContainsKey(TKey key) => _dict.ContainsKey(key);

    public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory) => _dict.GetOrAdd(key, factory);

    public void Trim(int maxItems) {
        var excess = _dict.Count - maxItems;
        if (excess <= 0) { return; }
        foreach (var key in _dict.Keys.Take(excess).ToList()) {
            if (_dict.TryRemove(key, out var value) && value is IDisposable d) { d.Dispose(); }
        }
    }

    public bool TryAdd(TKey key, TValue value) => _dict.TryAdd(key, value);

    public bool TryGetValue(TKey key, out TValue value) => _dict.TryGetValue(key, out value);

    public bool TryRemove(TKey key, out TValue value) => _dict.TryRemove(key, out value);

    #endregion
}