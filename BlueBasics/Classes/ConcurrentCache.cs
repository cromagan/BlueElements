// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace BlueBasics.Classes;

/// <summary>
/// Thread-sicherer Cache auf Basis einer <see cref="ConcurrentDictionary{TKey, TValue}" />,
/// der die Anzahl der Einträge auf ein konfigurierbares Maximum begrenzt.
/// <para>
/// Beim Entfernen oder Leeren von Einträgen werden Werte, die <see cref="IDisposable" />
/// implementieren, automatisch verworfen. Jede Instanz registriert sich zusätzlich bei
/// <see cref="RegisterCacheTrim" />, sodass globaler Memory-Druck zum
/// automatischen Verkleinern des Caches führt.
/// </para>
/// </summary>
public sealed class ConcurrentCache<TKey, TValue> : IDisposableExtended where TKey : notnull {

    #region Fields

    /// <summary>
    /// Interne, thread-sichere Datenstruktur zur Aufnahme der Cache-Einträge.
    /// </summary>
    private readonly ConcurrentDictionary<TKey, TValue> _dict;

    /// <summary>
    /// Maximale Anzahl von Einträgen, ab der <see cref="Trim" /> Einträge entfernt.
    /// Wird zur Laufzeit nicht verändert.
    /// </summary>
    private readonly int _maxCacheSize;

    /// <summary>
    /// Trim-Delegate, das bei <see cref="RegisterCacheTrim" /> angemeldet
    /// wurde. In einem Feld gehalten, damit <see cref="Dispose" /> die exakt
    /// gleiche Instanz wieder abmelden kann (Method-Group-Erzeugung liefert
    /// sonst jedes Mal ein neues Delegate). Zeigt auf <see cref="TrimToMax" />,
    /// damit <see cref="TrimAllCaches" /> niemals eine
    /// <see cref="ObjectDisposedException" /> auslöst, wenn ein Cache während
    /// des Trimmens freigegeben wird.
    /// </summary>
    private readonly Action _trimAction;

    private int _isDisposedFlag;

    #endregion

    #region Constructors

    /// <summary>
    /// Erstellt einen neuen Cache mit Standard-Gleichheitsvergleich für <typeparamref name="TKey" />.
    /// </summary>
    /// <param name="maxCacheSize">Maximale Anzahl von Einträgen im Cache.</param>
    public ConcurrentCache(int maxCacheSize) {
        _maxCacheSize = maxCacheSize;
        _dict = new ConcurrentDictionary<TKey, TValue>();
        _trimAction = TrimToMax;
        RegisterCacheTrim(_trimAction);
    }

    /// <summary>
    /// Erstellt einen neuen Cache mit dem angegebenen <see cref="IEqualityComparer{T}" />
    /// für <typeparamref name="TKey" />.
    /// </summary>
    /// <param name="comparer">Gleichheitsvergleich für die Schlüssel.</param>
    /// <param name="maxCacheSize">Maximale Anzahl von Einträgen im Cache.</param>
    public ConcurrentCache(IEqualityComparer<TKey> comparer, int maxCacheSize) {
        _maxCacheSize = maxCacheSize;
        _dict = new ConcurrentDictionary<TKey, TValue>(comparer);
        _trimAction = TrimToMax;
        RegisterCacheTrim(_trimAction);
    }

    #endregion

    #region Events

    public event EventHandler? Disposed;

    #endregion

    #region Properties

    /// <summary>Aktuelle Anzahl der Einträge im Cache.</summary>
    public int Count {
        get {
            ThrowIfDisposed();
            return _dict.Count;
        }
    }

    /// <summary>
    /// Gibt an, ob der Cache bereits über <see cref="Dispose" /> freigegeben wurde.
    /// Erlaubt sichere Lesezugriffe aus Consumern, die ihr eigenes Lifetime-Ende
    /// nicht synchron kontrollieren können — typischerweise <c>null</c>-Rückgaben
    /// in Such-Properties statt <see cref="ObjectDisposedException" />.
    /// </summary>
    public bool IsDisposed => _isDisposedFlag != 0;

    /// <summary>Sammlung aller aktuell im Cache enthaltenen Schlüssel.</summary>
    public ICollection<TKey> Keys {
        get {
            ThrowIfDisposed();
            return _dict.Keys;
        }
    }

    /// <summary>Sammlung aller aktuell im Cache enthaltenen Werte.</summary>
    public ICollection<TValue> Values {
        get {
            ThrowIfDisposed();
            return _dict.Values;
        }
    }

    #endregion

    #region Indexers

    /// <summary>
    /// Ruft den Wert zum angegebenen <paramref name="key" /> ab oder legt ihn fest.
    /// Beim Lesen eines nicht vorhandenen Schlüssels wird eine
    /// <see cref="KeyNotFoundException" /> geworfen — für nulltoleranten Zugriff
    /// <see cref="TryGetValue" /> verwenden.
    /// <para>
    /// Sowohl beim Lesen als auch beim Setzen wird <c>null</c> als Schlüssel
    /// abgelehnt; beim Setzen zusätzlich <c>null</c> als Wert
    /// (<see cref="ArgumentNullException" />). Beim Überschreiben eines
    /// vorhandenen Eintrags wird der bisherige Wert — sofern er
    /// <see cref="IDisposable" /> implementiert — verworfen. Die dafür genutzte
    /// <see cref="ConcurrentDictionary{TKey, TValue}.TryUpdate" />-Schleife
    /// garantiert, dass jeder Wert maximal einmal disposet wird (kein
    /// Doppel-Dispose bei konkurrierendem Schreiben).
    /// </para>
    /// </summary>
    public TValue this[TKey key] {
        get {
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(key);
            return _dict[key];
        }
        set {
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(value);
            while (true) {
                if (!_dict.TryGetValue(key, out var existing)) {
                    if (_dict.TryAdd(key, value)) { return; }
                    continue;
                }
                if (_dict.TryUpdate(key, value, existing)) {
                    if (!ReferenceEquals(existing, value) && existing is IDisposable d) { d.Dispose(); }
                    return;
                }
            }
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Entfernt alle aktuell vorhandenen Einträge. Werte, die
    /// <see cref="IDisposable" /> implementieren, werden dabei verworfen.
    /// </summary>
    /// <remarks>
    /// Über <see cref="ConcurrentDictionary{TKey, TValue}.TryRemove(TKey, out TValue)" /> wird
    /// jeder Wert nur dann disposet, wenn er tatsächlich aus dem Cache entfernt
    /// wurde — so ist <c>Clear</c> sicher gegen konkurrierendes
    /// <c>TryRemove</c>/<c>Trim</c> auf anderen Threads (kein Doppel-Dispose).
    /// </remarks>
    public void Clear() {
        ThrowIfDisposed();
        ClearCore();
    }

    /// <summary>Bestimmt, ob der angegebene Schlüssel im Cache enthalten ist.</summary>
    public bool ContainsKey(TKey key) {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(key);
        return _dict.ContainsKey(key);
    }

    /// <summary>
    /// Gibt den Cache frei: Alle Einträge werden entfernt, disposable Werte
    /// werden verworfen und das Trim-Delegate wird bei
    /// <see cref="UnregisterCacheTrim" /> abgemeldet. Mehrfaches Aufrufen
    /// ist sicher und hat ab dem zweiten Mal keinen Effekt.
    /// </summary>
    public void Dispose() {
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) != 0) { return; }
        UnregisterCacheTrim(_trimAction);
        ClearCore();
        OnDisposed();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Liefert den Wert zu <paramref name="key" />. Ist kein Eintrag vorhanden,
    /// wird er über <paramref name="factory" /> erzeugt und gespeichert.
    /// <para>
    /// Die Factory darf nicht <c>null</c> zurückgeben — die Null-Toleranz wird
    /// hier genauso durchgesetzt wie bei <see cref="TryAdd" /> und dem
    /// <see cref="this[TKey]" />-Setter, sodass der Cache niemals null-Werte
    /// enthält.
    /// </para>
    /// <para>
    /// Melden zwei Threads gleichzeitig denselben Schlüssel an, kann die
    /// <paramref name="factory" /> auf mehreren Threads ausgeführt werden.
    /// <see cref="ConcurrentDictionary{TKey, TValue}.GetOrAdd(TKey, Func{TKey, TValue})" />
    /// lieferte früher den "Verlierer"-Wert stillschweigend an den Aufrufer
    /// zurück, wodurch GDI-Resources (Pen/Brush/Font/Bitmap/GraphicsPath)
    /// geleakt sind. Diese Implementierung disposet den verworfenen Wert,
    /// sofern er <see cref="IDisposable" /> ist, und liefert stattdessen den
    /// im Cache gespeicherten Wert an alle Aufrufer zurück.
    /// </para>
    /// </summary>
    public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory) {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(factory);

        if (_dict.TryGetValue(key, out var existing)) { return existing; }

        var value = factory(key) ?? throw new InvalidOperationException("Factory returned null.");

        while (true) {
            if (_dict.TryAdd(key, value)) { return value; }
            if (_dict.TryGetValue(key, out existing)) {
                if (!ReferenceEquals(value, existing) && value is IDisposable d) { d.Dispose(); }
                return existing;
            }
        }
    }

    /// <summary>
    /// Verkleinert den Cache auf höchstens <paramref name="maxItems" /> Einträge.
    /// Überzählige Einträge werden entfernt; ihre Werte werden bei Bedarf verworfen
    /// (<see cref="IDisposable" />).
    /// </summary>
    public void Trim(int maxItems) {
        ThrowIfDisposed();
        TrimCore(maxItems);
    }

    /// <summary>
    /// Versucht, den Schlüssel/Wert hinzuzufügen. <c>null</c> als Wert oder
    /// Schlüssel ist unzulässig (<see cref="ArgumentNullException" />).
    /// </summary>
    /// <returns><c>true</c>, wenn der Eintrag hinzugefügt wurde; <c>false</c>, wenn der Schlüssel bereits existiert.</returns>
    public bool TryAdd(TKey key, TValue value) {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);
        return _dict.TryAdd(key, value);
    }

    /// <summary>Versucht, den Wert zum angegebenen Schlüssel abzurufen.</summary>
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(key);
        return _dict.TryGetValue(key, out value);
    }

    /// <summary>
    /// Versucht, den Eintrag zu entfernen. Der Wert wird bei Erfolg zurückgegeben
    /// und bei Bedarf verworfen (<see cref="IDisposable" />).
    /// </summary>
    public bool TryRemove(TKey key, [MaybeNullWhen(false)] out TValue value) {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(key);
        return _dict.TryRemove(key, out value);
    }

    /// <summary>
    /// Internes Clear ohne Dispose-Prüfung. Darf auch aus <see cref="Dispose" />
    /// heraus aufgerufen werden, nachdem das Dispose-Flag bereits gesetzt wurde.
    /// </summary>
    private void ClearCore() {
        foreach (var kvp in _dict.ToArray()) {
            if (_dict.TryRemove(kvp.Key, out var value) && value is IDisposable d) { d.Dispose(); }
        }
    }

    private void OnDisposed() => Disposed?.Invoke(this, System.EventArgs.Empty);

    /// <summary>
    /// Wirft eine <see cref="ObjectDisposedException" />, wenn die Instanz bereits
    /// über <see cref="Dispose" /> freigegeben wurde. Schutz vor
    /// Use-After-Dispose — insbesondere vor dem stillen Hinzufügen von Einträgen,
    /// die nie wieder disposet würden (Memory-Leak).
    /// </summary>
    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_isDisposedFlag != 0, this);

    /// <summary>
    /// Internes Trim ohne Dispose-Prüfung. Wird vom registrierten
    /// <see cref="_trimAction" />-Delegate aus aufgerufen und muss daher auch
    /// während eines konkurrierenden <see cref="Dispose" /> sicher bleiben —
    /// eine <see cref="ObjectDisposedException" /> würde sonst die Iteration in
    /// <see cref="TrimAllCaches" /> abbrechen und alle anderen Caches
    /// ungetrimmt lassen. Doppel-Dispose ist ausgeschlossen, weil
    /// <see cref="ConcurrentDictionary{TKey, TValue}.TryRemove(TKey, out TValue)" /> atomar ist.
    /// </summary>
    private void TrimCore(int maxItems) {
        var excess = _dict.Count - maxItems;
        if (excess <= 0) { return; }
        // ConcurrentDictionary-Enumerator liefert einen approximativen Snapshot
        // (siehe Bemerkung bei GetEnumerator). Über TryRemove wird jeder Wert
        // nur dann disposet, wenn er tatsächlich entfernt wurde — sicher gegen
        // konkurrierende Writes/Dispose auf anderen Threads (kein Doppel-Dispose).
        // Ohne ToList()-Allocation: removed zählt nur erfolgreiche Removes.
        var removed = 0;
        foreach (var key in _dict.Keys) {
            if (removed >= excess) { break; }
            if (_dict.TryRemove(key, out var value)) {
                if (value is IDisposable d) { d.Dispose(); }
                removed++;
            }
        }
    }

    /// <summary>
    /// Trim-Einstiegspunkt für <see cref="TrimAllCaches" />. Ruft
    /// <see cref="TrimCore" /> mit der konfigurierten <see cref="_maxCacheSize" />
    /// auf. Bewusst ohne <see cref="ThrowIfDisposed" /> — siehe Dokumentation
    /// von <see cref="TrimCore" />.
    /// </summary>
    private void TrimToMax() => TrimCore(_maxCacheSize);

    #endregion
}