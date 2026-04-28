// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Collections;

namespace BlueBasics.Classes;

public class AssemblyAwareCache<T> : IEnumerable<T> {

    #region Fields

    private int _assemblyCount;
    private List<T>? _items;
    private bool _loading;

    #endregion

    #region Constructors

    public AssemblyAwareCache() {
        Generic.LoadAllAssemblies();
    }

    #endregion

    #region Properties

    private List<T> Items {
        get {
            if (_loading) { return []; }

            var currentCount = AppDomain.CurrentDomain.GetAssemblies().Length;
            if (_items != null && _assemblyCount == currentCount) { return _items; }

            _loading = true;
            _items ??= [];
            AddNewTypes(_items);
            _assemblyCount = AppDomain.CurrentDomain.GetAssemblies().Length;
            _loading = false;

            if (typeof(IComparable).IsAssignableFrom(typeof(T))) {
                _items.Sort(Comparer<T>.Default);
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

        foreach (var thist in Generic.AllTypes) {
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