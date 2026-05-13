// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Interfaces;

namespace BlueBasics.Classes;

public class AssemblyAwareCache<T> {

    #region Fields

    private readonly object _lock = new();

    #endregion

    #region Constructors

    public AssemblyAwareCache() {
        Generic.LoadAllAssemblies();
    }

    #endregion

    #region Properties

    public IReadOnlyList<T> Instances {
        get {
            if (field is not null) { return field; }
            var types = Types;

            lock (_lock) {
                if (field is not null) { return field; }

                var result = new List<T>(types.Count);
                foreach (var t in types) {
                    try {
                        if (Activator.CreateInstance(t) is T inst) {
                            result.Add(inst);
                        }
                    } catch {
                        Develop.AbortAppIfStackOverflow();
                    }
                }

                if (typeof(IComparable).IsAssignableFrom(typeof(T))) {
                    result.Sort(Comparer<T>.Default);
                }

                field = result;
                return field;
            }
        }
        private set;
    }

    public IReadOnlyList<Type> Types {
        get {
            var currentCount = AppDomain.CurrentDomain.GetAssemblies().Length;
            if (field is not null && AssemblyCount == currentCount) { return field; }

            lock (_lock) {
                currentCount = AppDomain.CurrentDomain.GetAssemblies().Length;
                if (field is not null && AssemblyCount == currentCount) { return field; }

                var targetType = typeof(T);
                var result = field is null ? [] : new List<Type>(field);
                var existing = new HashSet<Type>(result);

                foreach (var t in Generic.AllTypes) {
                    if (!existing.Add(t)) { continue; }
                    if (!targetType.IsAssignableFrom(t)) { continue; }
                    if (t.GetConstructor(Type.EmptyTypes) is null) { continue; }
                    result.Add(t);
                }

                AssemblyCount = currentCount;
                Instances = null; // Invalidate, wenn neue Typen dazugekommen sind
                ByKey = null;
                field = result;
                return field;
            }
        }
        private set;
    }

    private int AssemblyCount { get; set; }

    private Dictionary<string, T>? ByKey {
        get {
            if (field is not null) { return field; }

            lock (_lock) {
                if (field is not null) { return field; }

                var dict = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
                foreach (var item in Instances) {
                    if (item is IHasKeyName k && k.KeyName is { Length: > 0 }) {
                        dict[k.KeyName] = item;
                    }
                }
                field = dict;
                return field;
            }
        }
        set;
    }

    #endregion

    #region Indexers

    public T? this[string? keyName] {
        get {
            if (keyName is not { Length: > 0 }) { return default; }
            return ByKey is { } dict && dict.TryGetValue(keyName, out var val) ? val : default;
        }
    }

    #endregion
}