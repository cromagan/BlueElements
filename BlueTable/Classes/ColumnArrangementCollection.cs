// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueTable.Classes;

/// <summary>
/// Beobachtbare Sammlung der Spaltenanordnungen (Ansichten) der Tabelle. Zwei
/// Betriebsarten: Im Legacy-Format läuft jede Änderung als ganzer
/// Ansichten-Satz über den ColumnArrangements-Setter der Tabelle. Im binären
/// Fragment-Format reichen Add, Remove und granulare Ersetzungen
/// FragmentChanged mit dem fertigen ColumnArrangements-Navigationspfad bis zur
/// Tabelle durch, die sie als gezielte Änderung in die Fragmentdatei schreibt.
/// Die Reihenfolge der Ansichten führt sie selbst (_internal) — Index 0 ist die
/// Standard-Ansicht.
/// </summary>
public sealed class ColumnArrangementCollection : KeyedItemCollection<ColumnViewCollection>, IHasTable {

    /// <summary>
    /// Verständlicher Name eines Empfänger-Typs im Ansichten-Pfad: Der Listen-,
    /// Nav- oder Chunk-Satz (ItemPath, ItemList, ItemListChunk) leitet den
    /// Zahlraum der Ansicht ein, sonst eigener Raum; unbekannte Werte liefern die Zahl.
    /// </summary>
    public static string GetNameOf(ReadOnlySpan<FragmentRecord> path) {
        if (path.Length == 1) { return ((FragmentTagType)path[0].ValueId).ToString(); }

        return path[0].TagType is FragmentTagType.ItemPath or FragmentTagType.ItemList or FragmentTagType.ItemListChunk
            ? ColumnViewCollection.GetNameOf(path[1..])
            : ((FragmentTagType)path[^1].ValueId).ToString();
    }

    #region Fields

    /// <summary>
    /// Reihenfolge-Träger der Ansichten; parallel dazu liegt die
    /// Schlüssel-Ablage der Basis (Internal).
    /// </summary>
    private readonly List<ColumnViewCollection> _internal = [];

    #endregion

    #region Constructors

    /// <summary>
    /// Erstellt die Sammlung; die Tabellen-Referenz wird zum Erzeugen neuer
    /// Ansichten beim Einlesen benötigt.
    /// </summary>
    public ColumnArrangementCollection(Table? table) {
        Table = table;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Die zugehörige Tabelle.
    /// </summary>
    public Table? Table { get; private set; }

    /// <summary>
    /// Anzahl der enthaltenen Ansichten.
    /// </summary>
    public override int Count => _internal.Count;

    #endregion

    #region Methods

    /// <summary>
    /// Liefert die Ansichten in ihrer gespeicherten Reihenfolge — Index 0
    /// ist die Standard-Ansicht.
    /// </summary>
    public override List<ColumnViewCollection> ToList() => [.. _internal];

    /// <summary>
    /// Enumeriert die Ansichten in ihrer gespeicherten Reihenfolge.
    /// </summary>
    public override IEnumerator<ColumnViewCollection> GetEnumerator() => _internal.GetEnumerator();

    /// <summary>
    /// Nach einer erfolgreichen Aufnahme: die Ansicht in die Eigenreihenfolge übernehmen.
    /// </summary>
    protected override void OnItemAdded(ColumnViewCollection item) {
        _internal.Add(item);
        base.OnItemAdded(item);
    }

    /// <summary>
    /// Nach einer erfolgreichen Entfernung: die Ansicht aus der Eigenreihenfolge nehmen.
    /// </summary>
    protected override void OnItemRemoved(ColumnViewCollection item) {
        _ = _internal.Remove(item);
        base.OnItemRemoved(item);
    }

    #endregion

    #region IFragmentStoreable

    /// <summary>
    /// Erzeugt die Ansicht zum ItemAdd-Kopf des Segment-Anfangs; den Inhalt
    /// (Einträge) liefert der Block der Ansicht, die Übernahme in die Sammlung
    /// macht Update (bestehende Instanzen behalten ihre Identität).
    /// </summary>
    protected override ColumnViewCollection? CreateItem(FragmentRecord first) {
        if (Table is not { IsDisposed: false } table) { return null; }

        return new ColumnViewCollection(table, string.Empty, first.KeyNameContent);
    }

    /// <summary>
    /// Übernimmt ein Journal-Add-Blatt: die Ansicht wird anhand des Schlüssels
    /// (Blatt-Inhalt bzw. Nav-Schlüssel) idempotent erzeugt; ihre Einträge tragen
    /// die Property-Blätter.
    /// </summary>
    protected override bool ApplyItemAdd(string keyName, string typeName) => GetOrCreateItem(keyName) is not null;

    /// <summary>
    /// Liefert die Ansicht unter dem Schlüssel; fehlt sie, wird sie erzeugt und
    /// aufgenommen (Journal kann vor der Aufnahme eintreffen).
    /// </summary>
    protected override ColumnViewCollection? GetOrCreateItem(string keyName) {
        if (GetByKey(keyName) is { } view) { return view; }

        if (Table is not { IsDisposed: false } table) { return null; }

        var neu = new ColumnViewCollection(table, string.Empty, keyName);
        Develop.Diagnose("ArrangementGetOrCreate", $"pid={Environment.ProcessId} Neue Ansicht erzeugt Key={keyName}");
        _ = Add(neu);
        return neu;
    }

    #endregion
}
