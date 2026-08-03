// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Collections.Concurrent;

namespace BlueBasics.Interfaces;

/// <summary>
/// EventArgs für das <see cref="LiveInstanceCache{T}.Added" />-Event. Hält die
/// neu erzeugte Live-Instanz bereit.
/// </summary>
public sealed class LiveInstanceEventArgs<T> : System.EventArgs {

    #region Constructors

    public LiveInstanceEventArgs(T instance) => Instance = instance;

    #endregion

    #region Properties

    /// <summary>Die neu erzeugte und registrierte Live-Instanz.</summary>
    public T Instance { get; }

    #endregion
}

/// <summary>
/// Abstrakte Basisklasse für Objekte mit eigenem Register lebender Instanzen
/// (z. B. <c>Chunk</c>, <c>ConnectedFormula</c>, <c>Table</c>). Stellt pro Typ
/// das statische LiveInstances-Register (protected), das moderne
/// <see cref="Added" />-Event, einen Snapshot-Zugriff über <see cref="AllInstances" />,
/// das Sync-Root <see cref="AllFilesLocker" /> sowie die Race-Safe-Factory
/// <see cref="GetOrCreate" /> bereit.
/// </summary>
/// <remarks>
/// Da statisch abstrakte Member nur in Interfaces, nicht aber in Klassen
/// erlaubt sind, wird der Typ-spezifische Zustand (Register, Event,
/// AllowDuplicates-Schalter) als statische Member auf dem generischen Typ
/// <c>LiveInstanceCache&lt;T&gt;</c> untergebracht — jedes <c>T</c> erhält
/// dadurch seine eigenen statischen Felder. Die konkrete Klasse implementiert
/// ihre eigene <c>Get(string key)</c>-Methode und ruft darin
/// <see cref="GetOrCreate" /> mit ihrer typspezifischen Factory auf.
/// </remarks>
/// <typeparam name="T">Der konkrete Live-Typ. Muss selbst
/// <see cref="LiveInstanceCache{T}" /> erben (CRTP) und zusätzlich
/// <see cref="IDisposableExtended" /> sowie <see cref="IHasKeyName" />
/// implementieren. Bei Vererbungshierarchien (z. B.
/// <c>BlockableFile</c> → <c>ConnectedFormula</c>) gibt der ableitende
/// Basistyp den Typ-Parameter vor.</typeparam>
public abstract class LiveInstanceCache<T> where T : LiveInstanceCache<T>, IDisposableExtended, IHasKeyName {

    #region Fields

    /// <summary>
    /// Sync-Root für Snapshot-Iterationen über LiveInstances.
    /// Die Threadsicherheit des ConcurrentDictionary selbst bleibt erhalten,
    /// aber Aufrufer, die einen konsistenten Snapshot benötigen, sperren
    /// darüber.
    /// </summary>
    public static readonly object AllFilesLocker = new();

    // Race-Condition-Schutz für parallele GetOrCreate-Aufrufe: pro
    // Key ein eigenes Lock-Objekt, damit die Instanz nur von einem Thread
    // erzeugt wird. Der zweite Aufrufer findet die fertig konstruierte
    // Instanz in LiveInstances vor. Case-Insensitive wie LiveInstances,
    // damit "Foo" und "foo" (derselbe Cache-Eintrag) auch denselben Lock
    // erhalten — sonst wäre die Serialisierung wirkungslos.
    private static readonly Dictionary<string, object> _loadLocks = new(StringComparer.OrdinalIgnoreCase);

    private static readonly object _loadLocksLocker = new();

    #endregion

    #region Events

    /// <summary>
    /// Wird ausgelöst, sobald eine neue Live-Instanz erzeugt und im Register
    /// eingetragen wurde. Ersetzt bei früheren ObservableCollection-basierten
    /// Implementierungen das <c>CollectionChanged</c>-Event. Moderne
    /// EventHandler-Signatur — der Sender ist <c>null</c>, die Instanz steckt
    /// in <see cref="LiveInstanceEventArgs{T}.Instance" />. Ausgelöst wird es
    /// über <see cref="OnAdded" /> aus <see cref="GetOrCreate" /> nach der
    /// erfolgreichen Konstruktion und Registrierung.
    /// <para>
    /// <b>Achtung — Subscriber-Leak:</b> Da es sich um ein statisches Event
    /// handelt, hält es starke Referenzen auf jeden Subscriber. Instanz-Handler
    /// (z. B. aus Forms) müssen zwingend in <c>Dispose</c> via
    /// <c>Added -= Handler</c> abgemeldet werden, sonst wird der Subscriber
    /// niemals garbage-collected. Bevorzugt werden statische Handler-Methoden
    /// verwendet, die von sich aus nicht leaken.
    /// </para>
    /// </summary>
    public static event EventHandler<LiveInstanceEventArgs<T>>? Added;

    #endregion

    #region Properties

    /// <summary>
    /// Stresstest-Schalter: ist er <c>true</c>, wird für jeden Aufruf von
    /// <see cref="GetOrCreate" /> eine neue Instanz erzeugt und nicht im
    /// Register gehalten. Default: <c>false</c>.
    /// </summary>
    public static bool AllowDuplicates { get; set; }

    /// <summary>
    /// Eigenes Register aller lebenden Instanzen dieses Typs, geordnet nach
    /// KeyName. Schlüsselseitig Case-Insensitive
    /// (<see cref="StringComparer.OrdinalIgnoreCase" />). Protected —
    /// externe Aufrufer nutzen <see cref="AllInstances" />.
    /// </summary>
    protected static ConcurrentDictionary<string, T> LiveInstances { get; } = new(StringComparer.OrdinalIgnoreCase);

    #endregion

    #region Methods

    /// <summary>
    /// Liefert einen Snapshot-Iterator über alle im Register eingetragenen
    /// Instanzen dieses Typs. HINWEIS: Das Register wird nur asynchron
    /// bereinigt (z. B. über das <c>BlockableFile</c>-Polling oder beim
    /// nächsten <see cref="GetOrCreate" />-Aufruf für denselben Key),
    /// daher können zwischenzeitlich bereits disposed Instanzen enthalten
    /// sein. Aufrufer müssen <c>IDisposableExtended.IsDisposed</c> prüfen,
    /// bevor sie auf die Instanz zugreifen.
    /// Kann in abgeleiteten Klassen per Method-Hiding (<c>new static</c>)
    /// spezialisiert werden — z. B. <c>ConnectedFormula.AllInstances()</c>
    /// filtert auf den konkreten Typ.
    /// </summary>
    public static IEnumerable<T> AllInstances() => LiveInstances.Values;

    /// <summary>
    /// Holt eine bestehende oder erstellt eine neue Live-Instanz für den
    /// angegebenen Key. Race-Safe: konstruieren zwei Threads gleichzeitig eine
    /// Instanz für denselben Key, serialisiert ein Per-Key-Lock die Konstruktion.
    /// Der Key wird nur einmal erzeugt, beide Aufrufer erhalten dieselbe Instanz.
    /// Gibt <c>null</c> zurück, wenn <paramref name="key" /> leer ist.
    /// </summary>
    /// <param name="key">KeyName bzw. Dateiname der zu holenden Instanz.</param>
    /// <param name="factory">Erzeugt aus dem Key eine neue Instanz. Der
    /// Konstruktor muss sich selbst in LiveInstances eintragen.
    /// Wird nur bei Cache-Miss aufgerufen. Nach erfolgreicher Konstruktion wird
    /// <see cref="OnAdded" /> ausgelöst.</param>
    protected static T? GetOrCreate(string key, Func<string, T> factory) {
        if (string.IsNullOrEmpty(key)) { return null; }

        // Bei erlaubten Duplikaten: nicht cachen, einfach Factory aufrufen.
        // Die Factory (bzw. der Konstruktor) registriert die Instanz ggf. selbst.
        if (AllowDuplicates) {
            var duplicate = factory(key);
            if (duplicate is null) { return null; }
            OnAdded(duplicate);
            return duplicate;
        }

        // Fast Path: bestehende lebende Instanz ohne Lock zurückgeben.
        // Der teure Konstruktions-/Lade-Pfad wird nur betreten, wenn wirklich
        // keine verwendbare Instanz existiert.
        if (LiveInstances.TryGetValue(key, out var existing)) {
            if (existing.IsDisposed) {
                // Bedingtes Remove: nur entfernen, wenn noch genau diese
                // (disposed) Instanz hinterlegt ist. Ein paralleler
                // Konstruktions-Thread im Per-Key-Lock könnte bereits eine
                // frische Instanz eingetragen haben — deren Eintrag darf hier
                // nicht gelöscht werden (sonst Datenverlust).
                LiveInstances.TryRemove(new KeyValuePair<string, T>(key, existing));
            } else {
                return existing;
            }
        }

        // Per-Key-Lock ermitteln. Serialisiert parallele
        // Konstruktionsversuche für denselben Key, damit die Factory
        // (und damit das Laden) nur einmal aufgerufen wird.
        object perKeyLock;
        lock (_loadLocksLocker) {
            if (!_loadLocks.TryGetValue(key, out var lk)) {
                lk = new object();
                _loadLocks[key] = lk;
            }
            perKeyLock = lk;
        }

        // success wird true, sobald die Factory erfolgreich war und die
        // erzeugte Instanz verbindlich im Cache liegt (GetOrAdd). Werfen
        // nachfolgende Schritte (Dispose des Verlierers, OnAdded), ist das
        // unschädlich — die Instanz steht im Cache, ein Folgetheruf für
        // denselben Key findet sie ohne erneute Factory. Das Lock wird dann
        // trotzdem entfernt, kein Memory-Leak.
        // Wirft hingegen die Factory selbst, bleibt success false und das
        // Lock wird NICHT aus _loadLocks entfernt — sonst könnte ein
        // nachfolgender Thread für denselben Key ein neues Lock erzeugen
        // und die Factory parallel zum noch wartenden Gewinner-Thread
        // aufrufen (Race trotz Per-Key-Lock).
        var success = false;
        try {
            lock (perKeyLock) {
                // Double-Check: ein anderer Thread könnte die Instanz inzwischen
                // konstruiert und eingetragen haben.
                if (LiveInstances.TryGetValue(key, out var raceWinner)) {
                    if (raceWinner.IsDisposed) {
                        // Bedingtes Remove (siehe Fast Path): nur diese
                        // disposed Instanz entfernen, nie eine frisch
                        // eingetragene eines parallelen Gewinners.
                        LiveInstances.TryRemove(new KeyValuePair<string, T>(key, raceWinner));
                    } else {
                        success = true;
                        return raceWinner;
                    }
                }

                // Neue Instanz erzeugen. Die Factory (bzw. der Konstruktor)
                // registriert sich selbst in LiveInstances.
                var created = factory(key);

                // Factory-Vertragsbruch abfangen: null würde in GetOrAdd eine
                // kryptische ArgumentNullException auslösen und danach success
                // auf false lassen — das Per-Key-Lock bliebe dauerhaft stehen.
                // Stattdessen null durchreichen und Lock freigeben.
                if (created is null) {
                    success = true;
                    return null;
                }

                // Sicherheitshalber GetOrAdd: falls trotz Lock ein anderer Thread
                // eingetragen hat, gewinnt der bereits eingetragene. Die eigene
                // Instanz wird verworfen.
                var winner = LiveInstances.GetOrAdd(key, created);

                // Ab hier gilt die Operation als erfolgreich: eine verwendbare
                // Instanz steht verbindlich im Cache. success VOR Dispose/
                // OnAdded setzen, damit ein Werfen dieser Schritte das Lock
                // trotzdem freigibt (kein Memory-Leak).
                success = true;

                if (!ReferenceEquals(winner, created)) {
                    created.Dispose();
                } else {
                    // Added erst hier feuern — die Instanz ist vollständig
                    // konstruiert und endgültig registriert (kein Race-Verlierer).
                    OnAdded(winner);
                }
                return winner;
            }
        } finally {
            // Per-Key-Lock nur bei erfolgreicher Ausführung entfernen, damit
            // _loadLocks nicht unbegrenzt wächst (Memory Leak bei vielen Keys).
            // Bei Factory-Exceptions bleibt es erhalten und serialisiert
            // nachfolgende Aufrufe für denselben Key korrekt — ein neuer Thread
            // würde sonst ein zweites Lock erzeugen und die Factory parallel
            // zum noch wartenden Thread des vorherigen Aufrufs aufrufen.
            // Conditional Remove: nur löschen, wenn noch unser Lock-Objekt
            // hinterlegt ist — ein zwischenzeitlich neu erzeugtes Lock eines
            // anderen Threads bleibt unangetastet. Memory-seitig begrenzt:
            // Jeder Key taucht maximal einmal auf, erfolgreiche Aufrufe
            // räumen ihr Lock auf.
            if (success) {
                lock (_loadLocksLocker) {
                    if (_loadLocks.TryGetValue(key, out var current) && ReferenceEquals(current, perKeyLock)) {
                        _loadLocks.Remove(key);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Holt eine bestehende oder erzeugt eine neue Live-Instanz für den
    /// angegebenen Key, ohne dass der Aufrufer eine Factory übergeben muss —
    /// die Konstruktion erfolgt über den statisch abstrakten Member
    /// <see cref="ICreateByKey{T}.Create" /> von <typeparamref name="TDerived" />.
    /// </summary>
    /// <remarks>
    /// Gedacht für Vererbungshierarchien (z. B.
    /// <c>BlockableFile</c> → <c>ConnectedFormula</c>), bei denen der
    /// Typparameter von <see cref="LiveInstanceCache{T}" /> nicht der
    /// konkrete zu erzeugende Typ ist: <typeparamref name="TDerived" /> ist
    /// der Subtyp, <typeparamref name="T" /> der vom Cache verwaltete Basistyp.
    /// Die eigentliche Race-Safe-Logik übernimmt
    /// <see cref="GetOrCreate(string, Func{string, T})" />; diese Überladung
    /// reicht <c><typeparamref name="TDerived" />.Create</c> als Factory durch
    /// und wandelt das Ergebnis typsicher zurück.
    /// </remarks>
    /// <typeparam name="TDerived">Der konkret zu erzeugende Typ. Muss von
    /// <typeparamref name="T" /> abgeleitet sein und
    /// <see cref="ICreateByKey{TDerived}" /> implementieren, damit die
    /// Factory statisch aufgerufen werden kann.</typeparam>
    /// <param name="key">KeyName bzw. Dateiname der zu holenden Instanz.</param>
    /// <returns>Die bestehende oder neu erzeugte Instanz vom Typ
    /// <typeparamref name="TDerived" /> oder <c>null</c>, wenn der Key leer ist
    /// oder im Cache eine Instanz eines anderen Typs liegt.</returns>
    protected static TDerived? GetOrCreate<TDerived>(string key) where TDerived : T, ICreateByKey<TDerived>
        => GetOrCreate(key, TDerived.Create) as TDerived;

    /// <summary>
    /// Löst das <see cref="Added" />-Event aus. Wird aus <see cref="GetOrCreate" />
    /// nach der erfolgreichen Registrierung aufgerufen — nicht mehr aus dem
    /// Konstruktor der konkreten Klasse. Aufrufer, die eine Instanz direkt per
    /// <c>new</c> erzeugen (außerhalb von <see cref="GetOrCreate" />), müssen
    /// <see cref="OnAdded" /> selbst auslösen.
    /// </summary>
    protected static void OnAdded(T instance) => Added?.Invoke(null, new LiveInstanceEventArgs<T>(instance));

    #endregion
}