// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Collections;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BlueBasics.Classes;

/// <summary>
/// Schlüsselbasierte Item-Sammlung auf ConcurrentDictionary-Basis (Key = KeyName;
/// Groß-/Kleinschreibung gemäß CaseSensitive). Stellt Speicher, Lese-/Schreib-API
/// und das komplette Fragment-Gerüst bereit: Add/Remove/Clear
/// melden die Änderung als Journal-Satz, SerializeCore/DeserializeThis implementieren
/// IFragmentStoreable — der konkrete Typ wird beim Serialize über TypeName abgefragt.
/// Alle Ableitungen nutzen dieselben reservierten Pfad-/Blatt-Typen
/// (FragmentTagType, Werte ab 200); die Bytes sind je Rolle fest vergeben.
/// Journal-Nav-Sätze (ItemPath mit Schlüssel-Content) wendet die Klasse granular
/// auf den benannten Eintrag an (ApplySubItem); unbekannte Sätze gehen an
/// DeserializeOther.
/// Alle Calls sind threadsicher: Die Ablage ist ein ConcurrentDictionary, zusammengesetzte
/// Schreiboperationen und Dispose sind über einen Lock bzw. ein Interlocked-Flag abgesichert.
/// </summary>
public abstract class KeyedItemCollection<TItem> : IEnumerable<TItem>, IFragmentStoreable, IDisposableExtended where TItem : class, IHasKeyName, IFragmentStoreable, IDisposableExtended {

    #region Fields

    /// <summary>
    /// 1 = Sammlung verworfen; Interlocked-gesichert, damit Dispose nur einmal läuft.
    /// </summary>
    private volatile int _isDisposedFlag;

    /// <summary>
    /// Sperrt zusammengesetzte Schreiboperationen (Rename, Register-Buchführung), damit sie atomar bleiben.
    /// </summary>
    private readonly Lock _mutationLock = new();

    #endregion

    #region Constructors

    /// <summary>
    /// Erstellt eine leere, änderbare Sammlung; die Schlüssel-Strategie entspricht CaseSensitive.
    /// </summary>
    protected KeyedItemCollection() => Internal = new(CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Übernimmt die Items (gleichnamige Doppel werden verworfen).
    /// </summary>
    protected KeyedItemCollection(IEnumerable<TItem> items) : this() {
        AddRange(items);
    }

    #endregion

    #region Abstract

    /// <summary>
    /// Erzeugt das Item zum ItemAdd-Kopf („KeyName|TypeName“) eines Listen-Segments; null lässt das Segment fallen.
    /// </summary>
    protected abstract TItem? CreateItem(FragmentRecord first);

    #endregion

    #region Properties

    /// <summary>
    /// Anzahl der enthaltenen Items.
    /// </summary>
    public virtual int Count => Internal.Count;

    /// <summary>
    /// True, wenn die Sammlung verworfen wurde; schaltet den FragmentChanged-Versand ab und schützt alle schreibenden Calls.
    /// </summary>
    public bool IsDisposed => _isDisposedFlag == 1;

    /// <summary>
    /// Zähler der aktiven SuppressEvents-Aufrufe; Werte größer 0 unterdrücken FragmentChanged.
    /// </summary>
    private int _eventsSuppressCount;

    /// <summary>
    /// Zeigt an, ob FragmentChanged-Events aktuell unterdrückt sind.
    /// </summary>
    public bool IsEventsSuppressed => _eventsSuppressCount > 0;

    /// <summary>
    /// True, wenn Schlüssel groß-/kleinschreibungssensitiv abgelegt und gesucht werden. Default: false.
    /// Muss je Instanz konstant sein: Der Comparer der Ablage wird im Konstruktor daraus fixiert.
    /// </summary>
    protected virtual bool CaseSensitive => false;

    /// <summary>
    /// Schlüssel-Ablage der Items unter ihrem KeyName; Roh-Ablage für Ableitungen,
    /// die ihre Aufnahme/Entfernung selbst journaled.
    /// </summary>
    protected ConcurrentDictionary<string, TItem> Internal { get; }

    #endregion

    #region Indexers

    /// <summary>
    /// Liefert das Item zum KeyName oder null; verworfene Items werden gemeldet und nicht geliefert.
    /// </summary>
    public TItem? this[string keyName] => GetByKey(keyName);

    #endregion

    #region Lookup

    /// <summary>
    /// Liefert die beobachtete Instanz eines Items: zum KeyName, sonst die Instanz, die bereits in der Ablage liegt; null, wenn neu.
    /// </summary>
    protected TItem? ExistingOrThis(TItem item) {
        if (GetByKey(item.KeyName) is { } existing) { return existing; }

        foreach (var thisItem in Items()) {
            if (ReferenceEquals(thisItem, item)) { return thisItem; }
        }

        return null;
    }

    /// <summary>
    /// Liefert eine flache Kopie der Items in Enumerationsreihenfolge.
    /// </summary>
    protected List<TItem> Items() => [.. Internal.Values];

    /// <summary>
    /// Liefert eine flache Kopie der Schlüssel.
    /// </summary>
    protected List<string> Keys() => [.. Internal.Keys];

    #endregion

    #region Events

    /// <summary>
    /// Bubbling-Event für Fragment-Änderungen (Pendant zu OnPropertyChangedExt).
    /// </summary>
    public event EventHandler<FragmentChangedEventArgs>? FragmentChanged;

    /// <summary>
    /// Wird beim Verwerfen ausgelöst; IsDisposed ist zu diesem Zeitpunkt bereits true.
    /// </summary>
    public event EventHandler? Disposed;

    #endregion

    #region Dispose

    /// <summary>
    /// Verwirft die Sammlung: Ablage leeren, alle Items verwerfen, Events lösen. Mehrfachaufrufe sind wirkungslos.
    /// </summary>
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Räumt die Sammlung idempotent auf: Ablage leeren; bei disposing=true die Items verwerfen und die Events lösen.
    /// Ableitungen ergänzen hier ihr eigenes Aufräumen und rufen die Basis auf.
    /// </summary>
    protected virtual void Dispose(bool disposing) {
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) != 0) { return; }

        var items = Items();
        Internal.Clear();

        if (disposing) {
            OnDisposed();
            Disposed = null;
            FragmentChanged = null;

            Parallel.ForEach(items, item => item.Dispose());

        }
    }

    /// <summary>
    /// Löst das Disposed-Event aus; IsDisposed ist zu diesem Zeitpunkt bereits true.
    /// </summary>
    private void OnDisposed() => Disposed?.Invoke(this, System.EventArgs.Empty);

    #endregion

    #region Methods

    /// <summary>
    /// Liefert einen Fehlergrund, wenn die Sammlung gerade nicht editierbar ist; sonst string.Empty
    /// (erfüllt IEditable der Ableitungen).
    /// </summary>
    public virtual string IsNowEditable() => IsDisposed ? "Sammlung ist verworfen" : string.Empty;

    /// <summary>
    /// Liefert das Item zum KeyName oder null; verworfene Items werden gemeldet und nicht geliefert.
    /// </summary>
    public virtual TItem? GetByKey(string? keyName) {
        if (IsDisposed || string.IsNullOrEmpty(keyName)) { return null; }

        try {
            var item = Internal.TryGetValue(keyName, out var value) ? value : null;
            if (item is { IsDisposed: true }) {
                Develop.DebugError("Interner Sammlungsfehler, Item verworfen: " + keyName);
                return null;
            }

            return item;
        } catch {
            Develop.AbortAppIfStackOverflow();
            return GetByKey(keyName);
        }
    }

    /// <summary>
    /// Liefert eine flache Kopie der Items, sortiert nach KeyName gemäß CaseSensitive —
    /// die einzige stabile Reihenfolge, die die Sammlung anbieten kann.
    /// </summary>
    public virtual List<TItem> ToList() {
        var l = Items();
        l.Sort((a, b) => (CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase).Compare(a.KeyName, b.KeyName));
        return l;
    }

    /// <summary>
    /// Enumeriert die Items live in der Dictionary-Reihenfolge.
    /// </summary>
    public virtual IEnumerator<TItem> GetEnumerator() => Internal.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Nimmt ein Item unter seinem KeyName auf; die Aufnahme meldet OnItemAdded.
    /// </summary>
    public virtual bool Add(TItem? item) {
        if (IsDisposed || item is null) { return false; }

        lock (_mutationLock) {
            if (IsDisposed || !Internal.TryAdd(item.KeyName, item)) { return false; }
        }

        OnItemAdded(item);
        return true;
    }

    /// <summary>
    /// Nimmt mehrere Items auf; true, wenn alle erfolgreich aufgenommen wurden.
    /// </summary>
    public bool AddRange(IEnumerable<TItem> items) {
        if (IsDisposed) { return false; }

        var ok = true;
        foreach (var item in items) {
            if (!Add(item)) { ok = false; }
        }

        return ok;
    }

    /// <summary>
    /// Entfernt das Item zum KeyName; die Entfernung meldet OnItemRemoved.
    /// </summary>
    public virtual bool Remove(string keyName) {
        if (IsDisposed) { return false; }

        TItem? item;

        lock (_mutationLock) {
            if (IsDisposed || !Internal.TryRemove(keyName, out item)) { return false; }
        }

        OnItemRemoved(item);
        return true;
    }

    /// <summary>
    /// Entfernt alle Items; jede Entfernung meldet OnItemRemoved.
    /// </summary>
    public virtual bool Clear() {
        if (IsDisposed) { return false; }

        var ok = true;
        foreach (var item in ToList()) {
            if (!Remove(item.KeyName)) { ok = false; }
        }

        return ok;
    }

    /// <summary>
    /// Gleicht die Sammlung an die Quelle an: Items, die in der Quelle fehlen, werden
    /// entfernt und neue aufgenommen — beides meldet OnItemRemoved/OnItemAdded — und
    /// gemeinsame Items übernehmen die Werte per Item-Update. Die Instanzen der Quelle
    /// werden übernommen, nicht kopiert; Ableitungen mit tabellengebundenen Items
    /// überschreiben die Routine entsprechend.
    /// </summary>
    public virtual string Update(IFragmentStoreable source) {
        if (IsDisposed) { return "Sammlung ist verworfen"; }
        if (source is not KeyedItemCollection<TItem> sourceCollection) { return "Quelltyp ungültig"; }
        if (sourceCollection.IsDisposed) { return "Quellsammlung ist verworfen"; }

        return Update(sourceCollection.ToList());
    }

    /// <summary>
    /// Gleicht die Sammlung an die Quelle an: Items, die in der Quelle fehlen, werden
    /// entfernt und verworfen, neue aufgenommen — beides meldet OnItemRemoved/OnItemAdded — und
    /// gemeinsame Items übernehmen die Werte per Item-Update und behalten ihre Instanz.
    /// Verworfene Quell-Items gelten als fehlend; bereits verworfene Items der Ablage
    /// werden still entfernt. Liefert einen Fehlergrund oder string.Empty. Ableitungen
    /// überschreiben die Routine für sammlungsspezifische Nacharbeit.
    /// </summary>
    public virtual string Update(IEnumerable<TItem> sourceItems) {
        if (IsDisposed) { return "Sammlung ist verworfen"; }

        var source = sourceItems.Where(s => !s.IsDisposed).ToList();
        var sourceKeys = new HashSet<string>(source.Select(s => s.KeyName), Internal.Comparer);
        var obsolete = new List<TItem>();

        foreach (var keyName in Keys()) {
            if (sourceKeys.Contains(keyName)) { continue; }

            // Direkt in der Ablage nachsehen, nicht über den Indexer: Verworfene Rest-Items
            // (z. B. Skripte, die sich beim Disposed-Event der Tabelle selbst verwerfen)
            // werden beim Aufräumen still entfernt statt als interner Fehler zu gelten.
            if (!Internal.TryGetValue(keyName, out var item) || !Remove(keyName)) { return "Entfernen von '" + keyName + "' fehlgeschlagen"; }

            obsolete.Add(item);
        }

        foreach (var sourceItem in source) {
            if (GetByKey(sourceItem.KeyName) is not { } existing) {
                if (!Add(sourceItem)) { return "Hinzufügen von '" + sourceItem.KeyName + "' fehlgeschlagen"; }
            } else if (!ReferenceEquals(existing, sourceItem)) {
                // Identische Instanz: nichts zu übernehmen — ein Selbst-Update würde
                // Inhalte zerstören (Clear + AddRange derselben Liste).
                var error = existing.Update(sourceItem);

                if (error is { Length: > 0 }) { return error; }
            }
        }

        foreach (var item in obsolete) {
            if (item is { IsDisposed: false }) { item.Dispose(); }
        }

        return string.Empty;
    }

    /// <summary>
    /// Gleicht die Sammlung granular an: Items, die in der Quelle fehlen, werden
    /// entfernt, neue aufgenommen; geänderte werden als Entfernung + Aufnahme gemeldet,
    /// sodass Add/Remove die Journal-Sätze erzeugen. Inhaltlich unveränderte Items
    /// (erkennbar bei IStringable) erzeugen keinen Journal-Eintrag. Ableitungen mit
    /// granular journallenden Items überschreiben die Routine.
    /// </summary>
    public virtual void UpdateChild(IEnumerable<TItem> sourceItems) {
        if (IsDisposed) { return; }

        var sourceKeys = new HashSet<string>(sourceItems.Select(s => s.KeyName), Internal.Comparer);

        foreach (var keyName in Keys()) {
            if (!sourceKeys.Contains(keyName)) { _ = Remove(keyName); }
        }

        foreach (var sourceItem in sourceItems) {
            var existing = ExistingOrThis(sourceItem);

            // Neu: als Aufnahme melden
            if (existing is null) { _ = Add(sourceItem); continue; }

            // Unverändert und nicht die beobachtete Instanz selbst: kein Journal-Eintrag
            if (!ReferenceEquals(existing, sourceItem)
                && existing is IStringable existingStringable
                && sourceItem is IStringable sourceStringable
                && existingStringable.ParseableItems().FinishParseable() == sourceStringable.ParseableItems().FinishParseable()) { continue; }

            // Entfernung und Aufnahme melden die Journal-Sätze selbst
            _ = Remove(existing.KeyName);
            _ = Add(sourceItem);
        }
    }

    /// <summary>
    /// Bucht ein Item unter einem neuen Schlüssel um: Die Ablage wechselt, Instanz und
    /// Verdrahtung bleiben. Belegt ein neuerer Eintrag inzwischen den Zielkey, bleibt der
    /// alte bestehen (false). Die Umbenennung selbst wird über das Key-Fragment des Items
    /// gemeldet — die Routine ist deren Anwendepfad und erzeugt keinen eigenen Journal-Satz.
    /// </summary>
    protected bool RenameKey(string oldKey, string newKey) {
        if (IsDisposed) { return false; }

        lock (_mutationLock) {
            if (IsDisposed || !Internal.TryRemove(oldKey, out var item)) { return false; }

            if (Internal.TryAdd(newKey, item)) { return true; }

            // Zielkey inzwischen belegt: Item unter dem alten Key zurücklegen, damit es nicht verloren geht.
            _ = Internal.TryAdd(oldKey, item);
            return false;
        }
    }

    /// <summary>
    /// Benennt das Item zum alten Schlüssel unter dem neuen Schlüssel um (via RenameKey);
    /// der Vergleich beider Schlüssel folgt KeyComparer. Liefert einen Fehlergrund oder
    /// string.Empty; identischer oder fehlender Quell-Schlüssel ist kein Fehler.
    /// </summary>
    public virtual string ChangeKey(string oldKey, string newKey) {
        if (Internal.Comparer.Equals(oldKey, newKey)) { return string.Empty; }
        if (IsDisposed) { return "Sammlung ist verworfen"; }

        if (GetByKey(oldKey) is null) { return string.Empty; }

        return RenameKey(oldKey, newKey) ? string.Empty : "Umbenennen fehlgeschlagen";
    }

    /// <summary>
    /// Entfernt Items, die null sind oder sich als leer melden; true, wenn mindestens eines entfernt wurde.
    /// Verlangt, dass der Item-Typ ICanBeEmpty erfüllt; sonst Fehlermeldung und Abbruch.
    /// </summary>
    public bool RemoveNullOrEmpty() {
        if (!typeof(ICanBeEmpty).IsAssignableFrom(typeof(TItem))) {
            Develop.DebugError("RemoveNullOrEmpty verlangt ICanBeEmpty am Item-Typ: " + typeof(TItem).Name);
            return false;
        }

        if (Internal is not { Count: not 0 }) { return false; }

        var remo = Internal.ToArray()
            .Where(pair => pair.Value is null || ((ICanBeEmpty)pair.Value).IsNullOrEmpty())
            .Select(pair => pair.Key)
            .ToList();

        if (remo.Count == 0) { return false; }

        var removedAny = false;

        foreach (var key in remo) {
            while (Internal.ContainsKey(key)) {
                if (Internal.TryRemove(key, out _)) {
                    removedAny = true;
                    break;
                }
                Thread.Sleep(1);
            }
        }

        return removedAny;
    }

    /// <summary>
    /// Nach einer erfolgreichen Aufnahme: verdrahtet das Kind neu und meldet die
    /// Aufnahme als Journal-Blatt ItemAdd mit „KeyName|TypeName“ ans
    /// Root — direkt unter dem eigenen Segment, ohne Nav-Satz und ohne Item-Block;
    /// die Eigenschaften tragen die Property-Blätter. Ableitungen überschreiben
    /// die Routine, um z. B. Caches zu invalidieren.
    /// </summary>
    protected virtual void OnItemAdded(TItem item) {
        this.Reregister(item);

        OnFragmentChanged(new FragmentChangedEventArgs(this, FragmentTagType.ItemAdd, item));
    }

    /// <summary>
    /// Nach einer erfolgreichen Entfernung: löst die Verdrahtung, meldet die Entfernung
    /// mit Navigationssatz (Nav-Schlüssel = KeyName des Items) und ItemRemove-Blatt
    /// ans Root. Ableitungen überschreiben die Routine, um z. B. Caches zu invalidieren.
    /// </summary>
    protected virtual void OnItemRemoved(TItem item) {
        this.Unregister(item);

        var e = new FragmentChangedEventArgs(this, FragmentTagType.ItemRemove, string.Empty)
            .WithNavigation(FragmentTagType.ItemPath, item.KeyName);

        OnFragmentChanged(e);
    }

    /// <summary>
    /// Löst das FragmentChanged-Event mit den fertigen Argumenten aus (Pendant zu OnPropertyChangedExt); nach dem Verwerfen wird nicht mehr versendet.
    /// </summary>
    public virtual void OnFragmentChanged(FragmentChangedEventArgs e) {
        if (IsDisposed || IsEventsSuppressed) { return; }

        FragmentChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Reicht eine Item-Änderung mit eigenem Navigationssatz (ItemPath, Key des
    /// Kindes je Ereignis frisch gelesen) eine Ebene weiter — das Root erhält ihn
    /// unmittelbar vor dem Blatt-Satz.
    /// </summary>
    public virtual void ChildFragmentChanged(object? sender, FragmentChangedEventArgs e) {
        if (IsDisposed || IsEventsSuppressed) { return; }

        // Nur das Blatt liefert seinen Blatt-Satz; diese Stufe feuert mit ihrem
        // eigenen Navigationssatz weiter.
        FragmentChanged?.Invoke(this, e.WithNavigation(FragmentTagType.ItemPath, FragmentChangedEventArgs.SenderKey(sender)));
    }

    /// <summary>
    /// Unterdrückt FragmentChanged-Events der Sammlung und aller enthaltenen Items.
    /// </summary>
    public void SuppressEvents() {
        _eventsSuppressCount++;
        foreach (var item in Items()) { item.SuppressEvents(); }
    }

    /// <summary>
    /// Hebt die Unterdrückung der Sammlung und aller enthaltenen Items wieder auf.
    /// </summary>
    public void ResumeEvents() {
        if (_eventsSuppressCount > 0) { _eventsSuppressCount--; }
        foreach (var item in Items()) { item.ResumeEvents(); }
    }

    /// <summary>
    /// Schreibt die Items als Liste: ein ItemList-Satz (ListKeyObject), darunter je Item
    /// ein ItemAdd-Kopf („KeyName|TypeName“) und darunter der Item-Block. Reihenfolge =
    /// ToList(); Ableitungen mit anderer Reihenfolge oder Teilmengen überschreiben die Routine.
    /// </summary>
    public virtual bool SerializeCore(Stream stream, int depth) {
        if (IsDisposed) { return true; }

        return FragmentRecord.Append(stream, depth, FragmentTagType.ItemList, ToList());
    }

    /// <summary>
    /// Verarbeitet die Listen- und Journal-Sätze der Sammlung: Nav-Sätze (Content =
    /// Schlüssel) wenden den Block eine Ebene tiefer granular auf den benannten
    /// Eintrag an, ItemList liest die ganze Liste; Add- und Remove-Blätter
    /// stammen aus Journal-Fragmenten, nie aus der Voll-Speicherung (das Add-Blatt
    /// trägt „KeyName|TypeName“); alles andere geht an DeserializeOther.
    /// </summary>
    public virtual bool DeserializeThis(Stream stream, FragmentRecord record) {
        if (IsDisposed) {
            Develop.Diagnose("KeyedDeserializeThis", $"pid={Environment.ProcessId} IsDisposed bei {GetType().Name}, Leaf={record.LeafType}, T{record.Depth}");
            return false;
        }

        // Journal-Nav-Satz (Content = Schlüssel statt Liste): den Block eine Ebene
        // tiefer auf den benannten Eintrag anwenden. Leerer Key = Segmenteintritt
        // des Übergeordneten: die Folge-Sätze gehören dieser Ebene und werden
        // dort gelesen (Granular-Navs tragen den Item-Schlüssel).
        if (record.TagType == FragmentTagType.ItemPath && record.DataType == FragmentDataType.String) {
            return record.StringContent is { Length: > 0 }
                ? ApplySubItem(stream, record)
                : this.Deserialize(stream, record.Depth + 1);
        }

        // Die ganze Liste lesen und per Update angleichen: bestehende Instanzen
        // behalten ihre Identität, Überbleibsel werden entfernt.
        if (record.TagType == FragmentTagType.ItemList) {
            if (FragmentStoreableExtension.DeserializeList<TItem>(stream, record.Depth, first => CreateItem(first)) is not { } items) {
                Develop.Diagnose("KeyedDeserializeThis", $"pid={Environment.ProcessId} ItemList=lesen fehlgeschlagen bei {GetType().Name}, T{record.Depth}");
                return false;
            }

            var upFehler = Update(items);
            if (upFehler is { Length: > 0 }) {
                Develop.Diagnose("KeyedDeserializeThis", $"pid={Environment.ProcessId} ItemList Update fehlgeschlagen bei {GetType().Name}, T{record.Depth}, Grund={upFehler}");
                return false;
            }

            return true;
        }

        // Teil-Bestand eines Daten-Chunks: nur den Bestand des eigenen Chunks
        // angleichen; Items anderer Chunks bleiben unberührt.
        if (record.TagType == FragmentTagType.ItemListChunk) {
            if (FragmentStoreableExtension.DeserializeList<TItem>(stream, record.Depth, first => CreateItem(first)) is not { } items) {
                Develop.Diagnose("KeyedDeserializeThis", $"pid={Environment.ProcessId} ItemListChunk=lesen fehlgeschlagen bei {GetType().Name}, T{record.Depth}");
                return false;
            }

            var chunkFehler = UpdateChunk(items, record.StringContent);
            if (chunkFehler is { Length: > 0 }) {
                Develop.Diagnose("KeyedDeserializeThis", $"pid={Environment.ProcessId} ItemListChunk Update fehlgeschlagen bei {GetType().Name}, Chunk={record.StringContent}, Grund={chunkFehler}");
                return false;
            }

            return true;
        }

        if (record.TagType == FragmentTagType.ItemAdd) {
            var addOk = ApplyItemAdd(record.KeyNameContent, record.TypeContent);
            if (!addOk) {
                Develop.Diagnose("KeyedDeserializeThis", $"pid={Environment.ProcessId} ItemAdd=false Key={record.KeyNameContent} Typ={record.TypeContent} an {GetType().Name}");
            }

            return addOk;
        }

        if (record.TagType == FragmentTagType.ItemRemove) {
            var removeOk = ApplyItemRemove(record.StringContent);
            if (!removeOk) {
                Develop.Diagnose("KeyedDeserializeThis", $"pid={Environment.ProcessId} ItemRemove=false Key={record.StringContent} an {GetType().Name}");
            }

            return removeOk;
        }

        return DeserializeOther(stream, record);
    }

    /// <summary>
    /// Übernimmt ein Journal-Add-Blatt „KeyName|TypeName“: keyName der Schlüssel
    /// (Blatt-Inhalt bzw. Nav-Schlüssel), typeName der Typ-Anteil. Die Basis
    /// übernimmt nichts; Ableitungen erzeugen das Item idempotent.
    /// </summary>
    protected virtual bool ApplyItemAdd(string keyName, string typeName) => true;

    /// <summary>
    /// Übernimmt ein deserialisiertes Item aus einem Journal-Add-Blatt: fehlt der
    /// Eintrag, wird er aufgenommen; sonst behält die bestehende Instanz ihre
    /// Identität (Werte per Item-Update übernommen, ein Fehlergrund ergibt false).
    /// CreateItem bereits angehängte Instanzen gelten als angewendet.
    /// </summary>
    protected bool ApplyItemAdd(TItem item) {
        if (string.IsNullOrEmpty(item.KeyName)) { return false; }

        var existing = GetByKey(item.KeyName);

        if (ReferenceEquals(existing, item)) { return true; }

        if (existing is not null) {
            Develop.Diagnose("ApplyItemAdd", $"update {item.KeyName} in bestehender Instanz");
            return existing.Update(item) is { Length: 0 };
    }

        Develop.Diagnose("ApplyItemAdd", $"neu {item.KeyName}");
        return Add(item);
    }

    /// <summary>
    /// Gleicht den Teil-Bestand eines Daten-Chunks an (ItemListChunk): Items des
    /// Chunks werden aufgenommen bzw. übernommen; Bestand mit gleichem Chunk-Wert,
    /// der im Chunk fehlt, wird entfernt. Items ohne Chunk-Wert und Bestand
    /// anderer Chunks bleiben unberührt.
    /// </summary>
    private string UpdateChunk(ReadOnlyCollection<TItem> items, string chunkId) {
        if (IsDisposed) { return "Sammlung ist verworfen"; }

        Develop.Diagnose("UpdateChunk", $"chunk={chunkId} anzahl={items.Count}");

        var chunkKeys = new HashSet<string>(items.Select(s => s.KeyName), Internal.Comparer);

        foreach (var item in items) {
            if (!ApplyItemAdd(item)) {
                var grund = "Hinzufügen von '" + item.KeyName + "' fehlgeschlagen";
                Develop.Diagnose("UpdateChunk", $"chunk={chunkId} FEHLER {grund}");
                return grund;
        }
        }

        List<TItem> obsolete = [];

        foreach (var keyName in Keys()) {
            if (chunkKeys.Contains(keyName)) { continue; }

            if (!Internal.TryGetValue(keyName, out var item) || item is not IHasChunkValue hasChunkValue ||
                !string.Equals(hasChunkValue.ChunkId(), chunkId, StringComparison.OrdinalIgnoreCase)) { continue; }

            if (!Remove(keyName)) { return "Entfernen von '" + keyName + "' fehlgeschlagen"; }

            obsolete.Add(item);
        }

        foreach (var item in obsolete) {
            if (item is { IsDisposed: false }) { item.Dispose(); }
        }

        return string.Empty;
    }

    /// <summary>
    /// Wendet eine Entfernung aus einem Journal-Block an; true, auch wenn der Eintrag
    /// fehlt. Ableitungen überschreiben die Routine, um z. B. still zu entfernen.
    /// </summary>
    protected virtual bool ApplyItemRemove(string keyName) {
        if (IsDisposed || GetByKey(keyName) is null) { return true; }


        var removed = Remove(keyName);


        return removed;
    }

    /// <summary>
    /// Liefert das Item zum Schlüssel eines Journal-Blocks; null lässt den
    /// Property-Block fallen. Die Basis liefert nur bestehende Items; Ableitungen
    /// überschreiben die Routine, um fehlende Items zu erzeugen und still anzuhängen.
    /// </summary>
    protected virtual TItem? GetOrCreateItem(string keyName) => GetByKey(keyName);

    /// <summary>
    /// Wendet einen Journal-Block an (Nav-Satz mit Schlüssel + tiefere Sätze): Der
    /// erste Blocksatz entscheidet — ItemRemove entfernt den benannten Eintrag,
    /// ItemAdd übernimmt den benannten Eintrag, übrige Sätze gehen als
    /// Property-Block direkt auf den Eintrag; fehlt er, wird er über
    /// GetOrCreateItem beschafft. Ein leerer Block wird übersprungen.
    /// </summary>
    protected bool ApplySubItem(Stream stream, FragmentRecord record) {
        // Ersten Blocksatz vorab lesen: Journal-Blatt oder Property-Satz; danach zurückschauen.
        var blockStart = stream.Position;
        FragmentRecord? firstLeaf = FragmentRecord.TryReadNext(stream, record.Depth + 1);
        stream.Position = blockStart;

        if (firstLeaf is null) { return true; }

        //Develop.Diagnose("ApplySubItem", "Key=" + record.StringContent + ", Leaf=" + firstLeaf.ValueId + "@T" + firstLeaf.Depth + ", IsTag=" + firstLeaf.IsTag + ", RemoveByte=" + (byte)FragmentTagType.ItemRemove + ", AddByte=" + (byte)FragmentTagType.ItemAdd);

        if (firstLeaf is { TagType: FragmentTagType.ItemRemove }) {
            Develop.Diagnose("ApplySubItem", $"pid={Environment.ProcessId} Remove Key={record.StringContent} an {GetType().Name}");
            return ApplyItemRemove(record.StringContent);
        }

        if (firstLeaf is { TagType: FragmentTagType.ItemAdd } addLeaf) {
            Develop.Diagnose("ApplySubItem", $"pid={Environment.ProcessId} Add Key={record.StringContent} Typ={addLeaf.TypeContent} an {GetType().Name}");
            return ApplyItemAdd(record.StringContent, addLeaf.TypeContent);
        }

        // Property-Journal: Eintrag beschaffen, notfalls erzeugen, Block anwenden.
        // Fehlt der Eintrag (kein Auto-Create — nur Add-Blätter erzeugen), muss der
        // Block dennoch konsumiert werden, damit der Stream nicht versetzt weiterliest.
        if (GetOrCreateItem(record.StringContent) is not { } item) {
            Develop.Diagnose("ApplySubItem", $"pid={Environment.ProcessId} Block übersprungen (Eintrag fehlt) Key={record.StringContent} an {GetType().Name}");
            // Block konsumieren: alle Sätze tiefer als der Nav-Satz gehören zum
            // fehlenden Eintrag; der Stream bleibt vor dem nächsten Satz der
            // eigenen Ebene stehen, damit die Folgeblöcke korrekt lesen.
            while (true) {
                var pos = stream.Position;

                if (FragmentRecord.TryReadNext(stream) is not { } skip) { stream.Position = pos; break; }

                if (skip.Depth <= record.Depth) { stream.Position = pos; break; }
            }

            return true;
        }

        Develop.Diagnose("ApplySubItem", $"pid={Environment.ProcessId} Item Key={record.StringContent} Typ={item.GetType().Name} an {GetType().Name}");
        var ok = item.Deserialize(stream, record.Depth + 1);
        if (!ok) {
            Develop.Diagnose("ApplySubItem", $"pid={Environment.ProcessId} Item-Block=false Key={record.StringContent} Typ={item.GetType().Name} an {GetType().Name}");
        }

        return ok;
    }

    /// <summary>
    /// Verarbeitet übrige Satz-Typen (Navigation, Unbekanntes); true = Satz übersprungen.
    /// </summary>
    protected virtual bool DeserializeOther(Stream stream, FragmentRecord record) => true;

    #endregion
}
