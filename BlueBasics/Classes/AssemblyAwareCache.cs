// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Collections;
using System.Threading;

namespace BlueBasics.Classes;

public class AssemblyAwareCache<T> : IEnumerable<T> {

    #region Fields

    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private int _assemblyCount;
    private List<T>? _items;

    #endregion

    #region Constructors

    public AssemblyAwareCache() {
        Generic.LoadAllAssemblies();
    }

    #endregion

    #region Properties

    private List<T> Items {
        get {
            if (_items != null && _assemblyCount == AppDomain.CurrentDomain.GetAssemblies().Length) { return _items; }

            if (!_semaphore.Wait(0)) {
                _semaphore.Wait();
                _semaphore.Release();
                return _items ?? [];
            }

            try {
                _items ??= [];
                AddNewTypes(_items);
                _assemblyCount = AppDomain.CurrentDomain.GetAssemblies().Length;

                if (typeof(IComparable).IsAssignableFrom(typeof(T))) {
                    _items.Sort(Comparer<T>.Default);
                }
            } finally {
                _semaphore.Release();
            }

            return _items;
        }
    }

    #endregion

    #region Methods

    public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private static void AddNewTypes(List<T> result) {
        var targetType = typeof(T);
        var existingTypes = new HashSet<Type>(result.Select(x => x!.GetType()));

        var allTypes = Generic.AllTypes.ToArray();
        foreach (var thist in allTypes) {
            if (existingTypes.Contains(thist)) { continue; }
            if (!targetType.IsAssignableFrom(thist)) { continue; }
            try {
                if (thist.GetConstructor(Type.EmptyTypes) == null) { continue; }
                if (Activator.CreateInstance(thist) is T t) {
                    result.Add(t);
                }
            } catch {
                Develop.AbortAppIfStackOverflow();
            }
        }
    }

    #endregion
}