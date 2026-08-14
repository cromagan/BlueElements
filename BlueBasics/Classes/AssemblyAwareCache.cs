// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Classes;

/// <summary>
/// Thread-sicherer Cache, der über alle geladenen Assemblies hinweg konkrete
/// (nicht abstrakte) Typen ermittelt, die <typeparamref name="T" /> zuweisen lassen
/// und über einen parameterlosen Konstruktor verfügen.
/// <para>
/// Der Typ-Bestand wird neu aufgebaut, sobald sich die Anzahl der geladenen Assemblies
/// im <see cref="AppDomain" /> ändert. Instanzen werden lazy erzeugt und können über
/// <see cref="IHasKeyName.KeyName" /> oder den vollen Typnamen abgerufen werden.
/// </para>
/// </summary>
public class AssemblyAwareCache<T> {

    #region Fields

    /// <summary>
    /// Synchronisationsobjekt für den thread-sicheren Zugriff auf die Caches.
    /// </summary>
    private readonly object _lock = new();

    /// <summary>
    /// Anzahl der Assemblies, die beim letzten Aufbau von <see cref="_types" /> geladen waren.
    /// Dient der Erkennung, ob der Cache erneuert werden muss.
    /// <c>volatile</c>, damit der Lesezugriff außerhalb des Locks immer den aktuellen Wert sieht.
    /// </summary>
    private volatile int _assemblyCount;

    /// <summary>
    /// Instanz-Cache, schlüsselweise nach <see cref="IHasKeyName.KeyName" />
    /// oder vollem Typnamen. Wird lazy über <see cref="Instances" /> befüllt und bei einer
    /// Änderung des Assembly-Bestands verworfen (<c>null</c>).
    /// <c>volatile</c> sichert das Double-Checked-Locking in <see cref="Instances" />.
    /// </summary>
    private volatile Dictionary<string, T>? _instances;

    /// <summary>
    /// Typ-Cache, schlüsselweise nach vollem Typnamen. Wird lazy über <see cref="Types" />
    /// befüllt und beim Erkennen neu geladener Assemblies erneuert.
    /// <c>volatile</c> sichert das Double-Checked-Locking in <see cref="Types" />.
    /// </summary>
    private volatile Dictionary<string, Type>? _types;

    #endregion

    #region Constructors

    /// <summary>
    /// Erstellt einen neuen Cache. Assemblies und Typen werden lazy beim ersten
    /// Zugriff geladen (siehe <see cref="Types" /> und <see cref="Instances" />),
    /// damit der Konstruktor keine statischen Initialisierungen fremder Assemblies
    /// anstößt, die rekursiv wieder auf diesen Cache zugreifen würden.
    /// </summary>
    public AssemblyAwareCache() { }

    #endregion

    #region Properties

    /// <summary>
    /// Liefert alle Instanzen der registrierten Typen. Beim ersten Zugriff werden die Typen
    /// instanziiert (parameterloser Konstruktor) und dauerhaft gecacht. Der Cache wird neu
    /// aufgebaut, sobald sich der Assembly-Bestand geändert hat.
    /// </summary>
    public IReadOnlyCollection<T> Instances => GetOrBuildInstances().Values;

    /// <summary>
    /// Liefert die Instanz-Map race-safe. Aufrufer außerhalb des Locks prüfen
    /// zuerst den schnellen Pfad (nicht-null <c>_instances</c>). Falls dieser
    /// fehlschlägt, wird innerhalb des Locks aufgebaut. Die zurückgegebene Map
    /// wird nach der Rückgabe nie mutiert, deshalb ist sie ohne Lock sicher
    /// konsumierbar.
    /// </summary>
    private Dictionary<string, T> GetOrBuildInstances() {
        // Schneller Pfad ohne Lock: Snapshot ziehen, damit null-Prüfung und
        // Verwendung nicht durch eine parallele Invalidierung auseinandergerissen werden.
        var snapshot = _instances;
        if (snapshot is not null) { return snapshot; }

        lock (_lock) {
            // Erneut prüfen - ein anderer Thread könnte bereits aufgebaut haben.
            if (_instances is not null) { return _instances; }

            // Stellen sicher, dass der Typ-Bestand aktuell ist. Der Aufruf erfolgt
            // innerhalb des Locks; Types betritt denselben Lock reentrant und kann
            // dabei _instances auf null setzen — was harmlos ist, weil wir es im
            // anschließenden Aufbau ohnehin neu befüllen.
            _ = Types;

            // Wenn Types den Bestand nicht aufbauen konnte (z. B. weil
            // Generic.AllTypes während des eigenen Aufbaus eine leere Liste
            // liefert), ist _types null. In diesem Fall dürfen wir das leere
            // Ergebnis NICHT in _instances persistieren, sonst bleibt der Cache
            // dauerhaft leer, weil der schnelle Pfad künftig das nicht-null
            // Dictionary zurückgibt, ohne Types erneut zu befragen.
            if (_types is null) { return new Dictionary<string, T>(); }

            var result = new Dictionary<string, T>();
            foreach (var t in _types.Values) {
                try {
                    if (Activator.CreateInstance(t) is T inst) {
                        string key;
                        if (inst is IHasKeyName keyed && keyed.KeyName is { Length: > 0 } keyName) {
                            key = keyName;
                        } else {
                            key = t.FullName ?? t.Name;
                        }

                        result[key] = inst;
                    }
                } catch (Exception e) {
                    // Früher wurde hier nur auf Stack-Overflow geprüft und jede
                    // andere Exception (z. B. fehlende DLL, TypeLoadException)
                    // still geschluckt. Das führt zu schwer auffindbaren Bugs,
                    // weil Typen scheinbar "verschwinden". Mindestens loggen.
                    Develop.DebugPrint("Fehler beim Instanziieren von " + t.FullName, e);
                    Develop.AbortAppIfStackOverflow();
                }
            }

            _instances = result;
            return result;
        }
    }

    /// <summary>
    /// Liefert alle konkreten, zu <typeparamref name="T" /> kompatiblen Typen mit
    /// parameterlosem Konstruktor. Wird nur neu aufgebaut, wenn sich seit dem letzten
    /// Aufbau weitere Assemblies geladen wurden.
    /// </summary>
    public IReadOnlyCollection<Type> Types {
        get {
            _ = AllTypes;

            var currentCount = AppDomain.CurrentDomain.GetAssemblies().Length;
            if (_types is not null && _assemblyCount == currentCount) { return _types.Values; }

            lock (_lock) {
                currentCount = AppDomain.CurrentDomain.GetAssemblies().Length;
                if (_types is not null && _assemblyCount == currentCount) { return _types.Values; }

                var targetType = typeof(T);
                var allTypes = AllTypes;

                // Generic.AllTypes liefert während seines eigenen Aufbaus eine leere
                // Liste zurück (siehe Generic.AllTypes: `_allTypesLoading`). Würden wir
                // hier den leeren Bestand persistieren und _assemblyCount setzen,
                // bliebe der Cache dauerhaft leer, weil künftige Aufrufe denken, alles
                // sei aktuell. In diesem Fall unverändert lassen und beim nächsten
                // Aufruf neu versuchen — analog zu Generic.GetTypeByClassId.
                if (allTypes.Count == 0) {
                    return _types is not null ? _types.Values : Array.Empty<Type>();
                }

                var result = _types is null ? new Dictionary<string, Type>() : new Dictionary<string, Type>(_types);

                foreach (var t in allTypes) {
                    string key = t.FullName ?? t.Name;
                    if (result.ContainsKey(key)) { continue; }
                    if (!targetType.IsAssignableFrom(t)) { continue; }
                    if (t.IsAbstract) { continue; }
                    if (t.ContainsGenericParameters) { continue; }
                    if (t.GetConstructor(Type.EmptyTypes) is null) { continue; }
                    result[key] = t;
                }

                // Reihenfolge wichtig (volatile Writes, kein Lock für Leser nötig):
                // 1. _instances invalidieren, damit niemand veraltete Instanzen sieht.
                //    Die alten Instanzen werden bewusst NICHT disposet, da sie über
                //    Instances/den Indexer nach außen gereicht werden und Aufrufer
                //    außerhalb des Locks noch Referenzen halten könnten — ein Dispose
                //    hier würde zu Use-After-Dispose/ObjectDisposedException führen.
                //    Nicht-managed Ressourcen (sofern vorhanden) werden erst vom GC
                //    finalisiert, was im seltenen Fall eines Assembly-Reloads
                //    akzeptiert wird (Lieferung: Crash-Vermeidung vor Leak-Vermeidung).
                // 2. _types aktualisieren, damit ein Leser, der ein nicht-null _types
                //    sieht, auch schon die neue Sammlung bekommt.
                // 3. _assemblyCount zuletzt setzen — erst danach gilt der neue Cache
                //    für Leser außerhalb des Locks als "aktuell" (siehe Check oben).
                //    Würde man _assemblyCount vor _types setzen, könnte ein Leser
                //    _assemblyCount == currentCount sehen, aber noch das alte _types.
                _instances = null;
                _types = result;
                _assemblyCount = currentCount;
                return result.Values;
            }
        }
    }

    #endregion

    #region Indexers

    /// <summary>
    /// Ruft die Instanz mit dem angegebenen <paramref name="keyName" /> ab
    /// (<see cref="IHasKeyName.KeyName" /> oder voller Typname).
    /// Gibt <c>default</c> zurück, wenn kein passender Schlüssel vorhanden ist oder
    /// <paramref name="keyName" /> leer/<c>null</c> ist.
    /// </summary>
    public T? this[string? keyName] {
        get {
            if (keyName is not { Length: > 0 }) { return default; }

            // GetOrBuildInstances() liefert immer eine nicht-null Map (baut sie
            // gegebenenfalls innerhalb des Locks auf). Damit entfällt die frühere
            // Race: zwischen `_ = Instances;` und `snapshot = _instances;` konnte
            // ein paralleler Types-Aufbau _instances wieder invalidieren, wonach
            // der Lookup fälschlich default zurückgegeben hat.
            return GetOrBuildInstances().TryGetValue(keyName, out var val) ? val : default;
        }
    }

    #endregion
}
