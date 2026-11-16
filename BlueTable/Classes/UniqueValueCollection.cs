// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Collections.ObjectModel;

namespace BlueTable.Classes;

/// <summary>
/// Beobachtbare Sammlung der Unique-Wert-Definitionen der Tabelle. Zwei
/// Betriebsarten: Im Legacy-Format ist sie schreibgeschützt — jede Änderung
/// überschreibt den ganzen Definitions-Satz über den UniqueValues-Setter der
/// Tabelle. Im binären Fragment-Format gibt die Tabelle die Sammlung zum direkten
/// Ändern frei (ReadOnly = false): Add, Remove und granulare Ersetzungen reichen
/// FragmentChanged mit dem fertigen Uniques-Navigationspfad bis zur Tabelle durch,
/// die sie als gezielte Änderung in die Fragmentdatei schreibt.
/// </summary>
public sealed class UniqueValueCollection : KeyedItemCollection<UniqueValueDefinition>, IHasTable {

    #region Enums

    private enum UniqueCollectionFragmentType : byte {

        /// <summary>
        /// ListKeyObject-Satz: die Definitionen folgen als Liste eine Ebene tiefer,
        /// zwischen ihnen je ein Trenner zwei Ebenen unter der Liste.
        /// </summary>
        Uniques = 1,

        /// <summary>
        /// Blatt-Satz: übernimmt eine ganze neue Definition im Classic-Format
        /// (Content = Parseable-String der Definition).
        /// </summary>
        UniqueAdd = 2,

        /// <summary>
        /// Blatt-Satz: entfernt die Definition des Journal-Blocks (Nav-Schlüssel).
        /// </summary>
        UniqueRemove = 3
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Erstellt die schreibgeschützte Sammlung (Legacy); die Tabellen-Referenz
    /// wird zum Erzeugen neuer Definitionen beim Einlesen benötigt.
    /// </summary>
    public UniqueValueCollection(Table? table) {
        ReadOnly = true;
        Table = table;
    }

    /// <summary>
    /// Erstellt die Sammlung mit Startbestand für den klassischen Ganzsatz-Schreibweg
    /// (Legacy): die Items werden normal aufgenommen und die Sammlung danach gesperrt;
    /// gleichnamige Doppel werden verworfen.
    /// </summary>
    public UniqueValueCollection(Table? table, IEnumerable<UniqueValueDefinition> uniques) {
        Table = table;
        _ = AddRange(uniques);
        ReadOnly = true;
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
    /// Übernimmt eine frisch gelesene Definition: fehlt sie, wird sie aufgenommen;
    /// sonst behält die bestehende Instanz ihre Identität (alle Werte übernommen).
    /// </summary>
    private bool ApplyUnique(UniqueValueDefinition unique) {
        if (GetByKey(unique.KeyName) is not { } existing) { return Add(unique); }

        _ = existing.Update(unique);
        return true;
    }

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
    /// Die Liste unter dem Uniques-Satz.
    /// </summary>
    protected override Enum ListType => UniqueCollectionFragmentType.Uniques;

    /// <summary>
    /// Aufnahmen eines ganzen Classic-Definitions-Strings.
    /// </summary>
    protected override Enum AddItemType => UniqueCollectionFragmentType.UniqueAdd;

    /// <summary>
    /// Entfernungen per Journal-Block.
    /// </summary>
    protected override Enum RemoveItemType => UniqueCollectionFragmentType.UniqueRemove;

    /// <summary>
    /// Pfad-Segment des Bubbling.
    /// </summary>
    protected override Enum ItemSubType => UniqueCollectionFragmentType.Uniques;

    /// <summary>
    /// Erzeugt die Definition zum Namens-Satz des Segment-Anfangs; den Inhalt
    /// (Spalten) liefert der Block der Definition, die Übernahme in die Sammlung
    /// macht ApplyLoadedItems (bestehende Instanzen behalten ihre Identität).
    /// </summary>
    protected override UniqueValueDefinition? CreateItem(FragmentRecord first) => new(Table, []);

    /// <summary>
    /// Übernimmt die gelesene Liste per Update; bestehende Instanzen behalten ihre Identität.
    /// </summary>
    protected override bool ApplyLoadedItems(ReadOnlyCollection<UniqueValueDefinition> items) {
        _ = Update(items.ToList(), true);
        return true;
    }

    /// <summary>
    /// Übernimmt eine ganze, im Classic-Format gereichte Definition.
    /// </summary>
    protected override bool ApplyItemAdd(string toParse, string keyName) => ApplyUnique(new UniqueValueDefinition(Table, toParse));

    #endregion
}
