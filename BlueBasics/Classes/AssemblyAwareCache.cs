// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Classes;

public class AssemblyAwareCache<T> {

    #region Fields

    private readonly object _lock = new();

    private int _assemblyCount;

    private Dictionary<string, T>? _instances;
    private Dictionary<string, Type>? _types;

    #endregion

    #region Constructors

    public AssemblyAwareCache() {
        Generic.LoadAllAssemblies();
    }

    #endregion

    #region Properties

    public IReadOnlyCollection<T> Instances {
        get {
            if (_instances is not null) { return _instances.Values; }

            // Wir rufen das Property Types auf, um sicherzustellen, dass _types gefüllt ist.
            _ = Types;

            lock (_lock) {
                if (_instances is not null) { return _instances.Values; }

                var result = new Dictionary<string, T>();

                // Da Types (oben) sicherstellt, dass _types nicht null ist,
                // können wir hier sicher darauf zugreifen.
                if (_types is not null) {
                    foreach (var t in _types.Values) {
                        try {
                            if (Activator.CreateInstance(t) is T inst) {
                                string key;
                                if (inst is IHasKeyName keyed && !string.IsNullOrEmpty(keyed.KeyName)) {
                                    key = keyed.KeyName;
                                } else {
                                    key = t.FullName ?? t.Name;
                                }

                                result[key] = inst;
                            }
                        } catch {
                            Develop.AbortAppIfStackOverflow();
                        }
                    }
                }

                _instances = result;
                return _instances.Values;
            }
        }
    }

    public IReadOnlyCollection<Type> Types {
        get {
            var currentCount = AppDomain.CurrentDomain.GetAssemblies().Length;
            if (_types is not null && _assemblyCount == currentCount) { return _types.Values; }

            lock (_lock) {
                currentCount = AppDomain.CurrentDomain.GetAssemblies().Length;
                if (_types is not null && _assemblyCount == currentCount) { return _types.Values; }

                var targetType = typeof(T);
                var result = _types is null ? new Dictionary<string, Type>() : new Dictionary<string, Type>(_types);

                foreach (var t in Generic.AllTypes) {
                    string key = t.FullName ?? t.Name;
                    if (result.ContainsKey(key)) { continue; }
                    if (!targetType.IsAssignableFrom(t)) { continue; }
                    if (t.GetConstructor(Type.EmptyTypes) is null) { continue; }
                    result[key] = t;
                }

                _assemblyCount = currentCount;
                _instances = null;
                _types = result;
                return _types.Values;
            }
        }
    }

    #endregion

    #region Indexers

    public T? this[string? keyName] {
        get {
            if (keyName is not { Length: > 0 }) { return default; }

            // Sicherstellen, dass Instanzen geladen sind
            if (_instances is null) { _ = Instances; }

            // Da wir die Instanzen direkt mit dem KeyName speichern,
            // reicht ein einfacher Dictionary-Zugriff aus.
            return _instances is not null && _instances.TryGetValue(keyName, out var val) ? val : default;
        }
    }

    #endregion
}