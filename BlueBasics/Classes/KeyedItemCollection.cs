// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BlueBasics.Classes;

/// <summary>
/// Schlüsselbasierte Item-Sammlung auf ConcurrentDictionary-Basis (Key = KeyName,
/// Groß-/Kleinschreibung wird ignoriert). Stellt Speicher, Lese-/Schreib-API
/// und das komplette Fragment-Gerüst bereit: Add/Remove/Clear
/// melden die Änderung als Journal-Satz, SerializeCore/DeserializeThis implementieren
/// IFragmentStoreable — der konkrete Typ wird beim Serialize über TypeName abgefragt.
/// Die Pfad-/Blatt-Typen kommen unverändert aus den privaten Enums der Ableitungen
/// und werden nur transportiert — der Byte-Code auf der Leitung bleibt identisch.
/// Journal-Nav-Sätze (ItemSubType mit Schlüssel-Content) wendet die Klasse granular
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
    /// Erstellt eine leere, änderbare Sammlung.
    /// </summary>
    protected KeyedItemCollection() { }

    /// <summary>
    /// Übernimmt die Items (gleichnamige Doppel werden verworfen).
    /// </summary>
    protected KeyedItemCollection(IEnumerable<TItem> items) {
        AddRange(items);
    }

    #endregion

    #region Abstract

    /// <summary>
    /// ListKeyObject-Typ der Liste im Fragment-Format (aus dem Enum der Ableitung).
    /// </summary>
    protected abstract Enum ListType { get; }

    /// <summary>
    /// Blatt-Typ einer Aufnahme im Fragment-Format.
    /// </summary>
    protected abstract Enum AddItemType { get; }

    /// <summary>
    /// Blatt-Typ einer Entfernung im Fragment-Format.
    /// </summary>
    protected abstract Enum RemoveItemType { get; }

    /// <summary>
    /// Pfad-Segment, das die Ableitung beim FragmentChanged-Bubbling voranstellt.
    /// </summary>
    protected abstract Enum ItemSubType { get; }

    /// <summary>
    /// Erzeugt das Item zum ersten Satz eines Listen-Segments; null lässt das Segment fallen.
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
    /// Interne Ablage der Items unter ihrem KeyName.
    /// </summary>
    protected ConcurrentDictionary<string, TItem> Internal { get; } = new(StringComparer.OrdinalIgnoreCase);

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
    /// Liefert eine flache Kopie der Items, sortiert nach KeyName (OrdinalIgnoreCase) —
    /// die einzige stabile Reihenfolge, die die Sammlung anbieten kann.
    /// </summary>
    public virtual List<TItem> ToList() {
        var l = Items();
        l.Sort((a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.KeyName, b.KeyName));
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
        var sourceKeys = new HashSet<string>(source.Select(s => s.KeyName), StringComparer.OrdinalIgnoreCase);
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

        var sourceKeys = new HashSet<string>(sourceItems.Select(s => s.KeyName), StringComparer.OrdinalIgnoreCase);

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
    /// beide Schlüssel werden vorab großgeschrieben. Liefert einen Fehlergrund oder
    /// string.Empty; identischer oder fehlender Quell-Schlüssel ist kein Fehler.
    /// </summary>
    public virtual string ChangeKey(string oldKey, string newKey) {
        oldKey = oldKey.ToUpperInvariant();
        newKey = newKey.ToUpperInvariant();

        if (oldKey == newKey) { return string.Empty; }
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
    /// Nach einer erfolgreichen Aufnahme: verdrahtet das Kind neu, erzeugt bei eigenem
    /// Segment die Argumente mit Navigationssatz und meldet die Aufnahme als
    /// Journal-Blatt AddFragmentType mit dem kompletten Item-Block im binären
    /// Fragment-Format ans Root. Ableitungen überschreiben die Routine, um z. B.
    /// Caches zu invalidieren.
    /// </summary>
    protected virtual void OnItemAdded(TItem item) {
        this.Reregister(item);

        using var block = new MemoryStream();
        _ = item.SerializeCore(block, 1);

        var e = new FragmentChangedEventArgs(this, AddItemType, Encoding.UTF8.GetString(block.ToArray())).WithNavigation(ItemSubType, item.KeyName);

        OnFragmentChanged(e);
    }

    /// <summary>
    /// Nach einer erfolgreichen Entfernung: löst die Verdrahtung, meldet die Entfernung
    /// mit Navigationssatz (Nav-Schlüssel = KeyName des Items) und RemoveFragmentType
    /// ans Root. Ableitungen überschreiben die Routine, um z. B. Caches zu invalidieren.
    /// </summary>
    protected virtual void OnItemRemoved(TItem item) {
        this.Unregister(item);

        var e = new FragmentChangedEventArgs(this, RemoveItemType, string.Empty)
            .WithNavigation(ItemSubType, item.KeyName);

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
    /// Reicht eine Item-Änderung mit eigenem Navigationssatz (ItemSubType, Key des
    /// Kindes je Ereignis frisch gelesen) eine Ebene weiter — das Root erhält ihn
    /// unmittelbar vor dem Blatt-Satz.
    /// </summary>
    public virtual void ChildFragmentChanged(object? sender, FragmentChangedEventArgs e) {
        if (IsDisposed || IsEventsSuppressed) { return; }

        // Nur das Blatt liefert seinen Blatt-Satz; diese Stufe feuert mit ihrem
        // eigenen Navigationssatz weiter.
        FragmentChanged?.Invoke(this, e.WithNavigation(ItemSubType, FragmentChangedEventArgs.SenderKey(sender)));
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
    /// Schreibt die Items als Liste: ein ListFragmentType-Satz (ListKeyObject), darunter je Item
    /// der Item-Block eine Ebene tiefer. Reihenfolge = ToList(); Ableitungen mit anderer
    /// Reihenfolge oder Teilmengen überschreiben die Routine.
    /// </summary>
    public virtual bool SerializeCore(Stream stream, int depth) {
        if (IsDisposed) { return true; }

        return FragmentRecord.Append(stream, depth, ListType, ToList());
    }

    /// <summary>
    /// Verarbeitet die Listen- und Journal-Sätze der Sammlung: Nav-Sätze (Content =
    /// Schlüssel) wenden den Block eine Ebene tiefer granular auf den benannten
    /// Eintrag an, ListFragmentType liest die ganze Liste; Add- und Remove-Blätter
    /// stammen aus Journal-Fragmenten, nie aus der Voll-Speicherung (das Add-Blatt
    /// trägt den Item-Block im Fragment-Format bzw. den Classic-String älterer
    /// Journale); alles andere geht an DeserializeOther.
    /// </summary>
    public virtual bool DeserializeThis(Stream stream, FragmentRecord record) {
        if (IsDisposed) { return false; }

        // Journal-Nav-Satz (Content = Schlüssel statt Liste): den Block eine Ebene
        // tiefer auf den benannten Eintrag anwenden. Leerer Key = Segmenteintritt
        // des Übergeordneten: die Folge-Sätze gehören dieser Ebene und werden
        // dort gelesen (Granular-Navs tragen den Item-Schlüssel).
        if (record.LeafType == FragmentRecord.EnumByte(ItemSubType) && record.DataType == FragmentDataType.String) {
            return record.StringContent is { Length: > 0 }
                ? ApplySubItem(stream, record)
                : this.Deserialize(stream, record.Depth + 1);
        }

        // Die ganze Liste lesen und per Update angleichen: bestehende Instanzen
        // behalten ihre Identität, Überbleibsel werden entfernt.
        if (record.LeafType == FragmentRecord.EnumByte(ListType)) {
            if (FragmentStoreableExtension.DeserializeList<TItem>(stream, record.Depth, first => CreateItem(first)) is not { } items) { return false; }

            return Update(items) is { Length: 0 };
        }

        if (record.LeafType == FragmentRecord.EnumByte(AddItemType)) { return ApplyItemAddFragment(record.StringContent); }

        if (record.LeafType == FragmentRecord.EnumByte(RemoveItemType)) { return ApplyItemRemove(record.StringContent); }

        return DeserializeOther(stream, record);
    }

    /// <summary>
    /// Übernimmt ein Item aus einem Journal-Add-Blatt im Classic-Parseable-Format
    /// (ältere Journale); die Basis kennt kein Classic-Format. Neue Journale tragen
    /// den Item-Block im Fragment-Format und laufen über ApplyItemAdd(TItem).
    /// </summary>
    protected virtual bool ApplyItemAdd(string toParse, string keyName) => true;

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

        return existing is not null ? existing.Update(item) is { Length: 0 } : Add(item);
    }

     /// <summary>
    /// Wendet ein Add-Blatt im Fragment-Format an: Der komplette Item-Block wird über
    /// CreateItem (erster Satz) beschafft, frisch deserialisiert und über
    /// ApplyItemAdd(TItem) übernommen.
    /// </summary>
    protected bool ApplyItemAddFragment(string block) {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(block));

        if (FragmentRecord.TryReadNext(stream, 0) is not { } first) { return false; }
        if (CreateItem(first) is not { } item) { return false; }

        stream.Position = 0;

        if (!item.Deserialize(stream, 0)) { return false; }

        return ApplyItemAdd(item);
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
    /// erste Blocksatz entscheidet — RemoveItemType entfernt den benannten Eintrag,
    /// AddItemType übernimmt das Item aus seinem Block (Fragment-Format bzw.
    /// Classic-String älterer Journale), übrige Sätze gehen als Property-Block
    /// direkt auf den Eintrag; fehlt er, wird er über GetOrCreateItem
    /// beschafft. Ein leerer Block wird übersprungen.
    /// </summary>
    protected bool ApplySubItem(Stream stream, FragmentRecord record) {
        // Ersten Blocksatz vorab lesen: Journal-Blatt oder Property-Satz; danach zurückschauen.
        var blockStart = stream.Position;
        FragmentRecord? firstLeaf = FragmentRecord.TryReadNext(stream, record.Depth + 1);
        stream.Position = blockStart;

        if (firstLeaf is null) { return true; }

        Develop.Diagnose("ApplySubItem", "Key=" + record.StringContent + ", Leaf=" + firstLeaf.LeafType + "@T" + firstLeaf.Depth + ", IsTag=" + firstLeaf.IsTag + ", RemoveByte=" + FragmentRecord.EnumByte(RemoveItemType) + ", AddByte=" + FragmentRecord.EnumByte(AddItemType) + ", Stack: " + Develop.DiagStack());

        if (firstLeaf is { IsTag: false } leaf && leaf.LeafType == FragmentRecord.EnumByte(RemoveItemType)) { return ApplyItemRemove(record.StringContent); }

        if (firstLeaf is { IsTag: false } addLeaf && addLeaf.LeafType == FragmentRecord.EnumByte(AddItemType)) { return ApplyItemAddFragment(addLeaf.StringContent); }

        // Property-Journal: Eintrag beschaffen, notfalls erzeugen, Block anwenden.
        // Fehlt der Eintrag (kein Auto-Create — nur Add-Blätter erzeugen), muss der
        // Block dennoch konsumiert werden, damit der Stream nicht versetzt weiterliest.
        if (GetOrCreateItem(record.StringContent) is not { } item) {
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

        return item.Deserialize(stream, record.Depth + 1);
    }

    /// <summary>
    /// Verarbeitet übrige Satz-Typen (Navigation, Unbekanntes); true = Satz übersprungen.
    /// </summary>
    protected virtual bool DeserializeOther(Stream stream, FragmentRecord record) => true;

    #endregion
}
