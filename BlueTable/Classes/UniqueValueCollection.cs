// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueTable.Classes;

/// <summary>
/// Beobachtbare Sammlung der Unique-Wert-Definitionen der Tabelle. Zwei
/// Betriebsarten: Im Legacy-Format läuft jede Änderung als ganzer
/// Definitions-Satz über den UniqueValues-Setter der Tabelle. Im binären
/// Fragment-Format reichen Add, Remove und granulare Ersetzungen
/// FragmentChanged mit dem fertigen Uniques-Navigationspfad bis zur Tabelle durch,
/// die sie als gezielte Änderung in die Fragmentdatei schreibt.
/// </summary>
public sealed class UniqueValueCollection : KeyedItemCollection<UniqueValueDefinition>, IHasTable {

    /// <summary>
    /// Verständlicher Name eines Empfänger-Typs im Unique-Pfad: Der Listen- oder
    /// Nav-Satz (ItemPath) leitet den Zahlraum der Definition ein, sonst eigener
    /// Raum; unbekannte Werte liefern die Zahl.
    /// </summary>
    public static string GetNameOf(ReadOnlySpan<FragmentRecord> path) {
        if (path.Length == 1) { return ((CollectionFragmentType)path[0].LeafType).ToString(); }

        return path[0].LeafType is (byte)CollectionFragmentType.ItemPath or (byte)CollectionFragmentType.ItemList
            ? UniqueValueDefinition.GetNameOf(path[1..])
            : ((CollectionFragmentType)path[^1].LeafType).ToString();
    }

    #region Constructors

    /// <summary>
    /// Erstellt die Sammlung; die Tabellen-Referenz wird zum Erzeugen neuer
    /// Definitionen beim Einlesen benötigt.
    /// </summary>
    public UniqueValueCollection(Table? table) {
        Table = table;
    }

    /// <summary>
    /// Erstellt die Sammlung mit Startbestand; gleichnamige Doppel werden verworfen.
    /// </summary>
    public UniqueValueCollection(Table? table, IEnumerable<UniqueValueDefinition> uniques) {
        Table = table;
        _ = AddRange(uniques);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Die zugehörige Tabelle.
    /// </summary>
    public Table? Table { get; private set; }

    #endregion

    #region Methods

    /// <summary>
    /// Liefert die Definition unter dem Schlüssel; fehlt sie, wird sie erzeugt
    /// und aufgenommen (Journal kann vor der Aufnahme eintreffen).
    /// </summary>
    protected override UniqueValueDefinition GetOrCreateItem(string keyName) {
        if (GetByKey(keyName) is { } unique) { return unique; }

        var neu = new UniqueValueDefinition(Table, []);
        _ = Add(neu);
        return neu;
    }

    #endregion

    #region IFragmentStoreable

    /// <summary>
    /// Erzeugt die Definition zum Namens-Satz des Segment-Anfangs; den Inhalt
    /// (Spalten) liefert der Block der Definition, die Übernahme in die Sammlung
    /// macht Update (bestehende Instanzen behalten ihre Identität).
    /// </summary>
    protected override UniqueValueDefinition? CreateItem(FragmentRecord first) => new(Table, []);

    /// <summary>
    /// Übernimmt ein Journal-Add-Blatt: die Definition wird anhand des Schlüssels
    /// (Blatt-Inhalt bzw. Nav-Schlüssel) idempotent erzeugt; ihre Spalten tragen
    /// die Property-Blätter.
    /// </summary>
    protected override bool ApplyItemAdd(string toParse, string keyName, string typeName) => GetOrCreateItem(keyName) is not null;

    #endregion
}
