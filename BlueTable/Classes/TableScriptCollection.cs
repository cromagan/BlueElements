// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Collections.ObjectModel;
using System.IO;

namespace BlueTable.Classes;

/// <summary>
/// Beobachtbare Skript-Sammlung der Tabelle. Zwei Betriebsarten: Im Legacy-Format
/// ist sie schreibgeschützt — jede Skript-Änderung überschreibt den ganzen
/// Skript-Satz über den EventScript-Setter der Tabelle. Im binären Fragment-Format
/// gibt die Tabelle die Sammlung zum direkten Ändern frei (ReadOnly = false): Add,
/// Remove und granulare Ersetzungen reichen FragmentChanged mit dem fertigen
/// Scripts-Navigationspfad bis zur Tabelle durch, die sie als gezielte Änderung in die
/// Fragmentdatei schreibt.
/// </summary>
public sealed class TableScriptCollection : KeyedItemCollection<TableScriptDescription>, IHasTable {

    #region Enums

    /// <summary>
    /// Pfad-Typen der Sammlung im binären Fragment-Format; eigener Zahlraum der
    /// Sammlung, die Tabelle routet über ihren eigenen Segmenteintritt eine Stufe
    /// darüber. Der Scripts-Satz trägt als ListKeyObject die ganze Liste bzw. als
    /// Nav-Satz den Schlüssel des Journal-Blocks. Die Werte 21/22 kollidieren
    /// bewusst nicht mit den Skript-Eigenschaften (1-13).
    /// </summary>
    private enum ScriptCollectionFragmentType : byte {

        /// <summary>
        /// ListKeyObject-Satz: die Skripte folgen als Liste eine Ebene tiefer,
        /// zwischen ihnen je ein Trenner zwei Ebenen unter der Liste.
        /// </summary>
        Scripts = 1,

        /// <summary>
        /// Blatt-Satz: übernimmt ein ganzes neues Skript im Classic-Format
        /// (Content = Parseable-String des Skripts).
        /// </summary>
        ScriptAdd = 2,

        /// <summary>
        /// Blatt-Satz: entfernt das Skript des Journal-Blocks (Nav-Schlüssel).
        /// </summary>
        ScriptRemove = 3
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Erstellt die schreibgeschützte Sammlung (Legacy); die Tabellen-Referenz
    /// wird zum Erzeugen neuer Skripte beim Einlesen benötigt.
    /// </summary>
    public TableScriptCollection(Table? table) {
        ReadOnly = true;
        Table = table;
    }

    /// <summary>
    /// Erstellt die Sammlung mit Startbestand für den klassischen Ganzsatz-Schreibweg
    /// (Legacy): die Items werden normal aufgenommen und die Sammlung danach gesperrt;
    /// gleichnamige Doppel werden verworfen.
    /// </summary>
    public TableScriptCollection(Table? table, IEnumerable<TableScriptDescription> scripts) {
        Table = table;
        _ = AddRange(scripts);
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
    /// Ersetzt ein Skript granular (Binärformat): Ein geändertes Skript wird als
    /// ScriptRemove/ScriptAdd gemeldet, bei Namenswechsel unter dem alten bzw. neuen
    /// Namen. Fehlt das Skript, wird es aufgenommen; inhaltlich unveränderte Skripte
    /// erzeugen keinen Journal-Eintrag.
    /// </summary>
    public bool Update(TableScriptDescription? script) {
        if (ReadOnly) { return false; }
        if (script is null) { return false; }

        // Bestehende Instanz suchen: gleicher Name oder gleiche Instanz (umbenannt)
        var existing = ExistingOrThis(script);

        // Neu: als Aufnahme melden
        if (existing is null) { return Add(script); }

        // Unverändert und nicht die beobachtete Instanz selbst: kein Journal-Eintrag
        if (!ReferenceEquals(existing, script) && existing.ParseableItems().FinishParseable() == script.ParseableItems().FinishParseable()) { return true; }

        // Entfernung und Aufnahme melden die Journal-Sätze selbst (ScriptRemove vor ScriptAdd)
        _ = Remove(existing.KeyName);
        return Add(script);
    }

    /// <summary>
    /// Übernimmt ein frisch gelesenes Skript: fehlt es, wird es aufgenommen;
    /// sonst behält die bestehende Instanz ihre Identität (alle Werte übernommen).
    /// </summary>
    private bool ApplyScript(TableScriptDescription script) {
        if (GetByKey(script.KeyName) is not { } existing) { return Add(script); }

        existing.UpdateFrom(script);
        return true;
    }

    /// <summary>
    /// Liefert das Skript unter dem Schlüssel; fehlt es, wird es erzeugt und
    /// aufgenommen (Journal kann vor der Aufnahme eintreffen).
    /// </summary>
    protected override TableScriptDescription GetOrCreateItem(string keyName) {
        if (GetByKey(keyName) is { } script) { return script; }

        var neu = new TableScriptDescription(Table, keyName, string.Empty);
        _ = Add(neu);
        return neu;
    }

    #endregion

    #region IFragmentStoreable

    /// <summary>
    /// Die Liste unter dem Scripts-Satz.
    /// </summary>
    protected override Enum ListType => ScriptCollectionFragmentType.Scripts;

    /// <summary>
    /// Aufnahmen eines ganzen Classic-Skript-Strings.
    /// </summary>
    protected override Enum AddItemType => ScriptCollectionFragmentType.ScriptAdd;

    /// <summary>
    /// Entfernungen per Journal-Block.
    /// </summary>
    protected override Enum RemoveItemType => ScriptCollectionFragmentType.ScriptRemove;

    /// <summary>
    /// Pfad-Segment des Bubbling.
    /// </summary>
    protected override Enum ItemSubType => ScriptCollectionFragmentType.Scripts;

    /// <summary>
    /// Erzeugt das Skript zum Name-Satz des Segment-Anfangs; die Übernahme in die
    /// Sammlung macht ApplyLoadedItems (bestehende Instanzen behalten ihre Identität).
    /// </summary>
    protected override TableScriptDescription? CreateItem(FragmentRecord first) => new(Table, first.StringContent, string.Empty);

    /// <summary>
    /// Übernimmt die gelesene Liste per Update; bestehende Instanzen behalten ihre Identität.
    /// </summary>
    protected override bool ApplyLoadedItems(ReadOnlyCollection<TableScriptDescription> items) {
        _ = Update(items.ToList(), true);
        return true;
    }

    /// <summary>
    /// Übernimmt ein ganzes, im Classic-Format gereichtes Skript.
    /// </summary>
    protected override bool ApplyItemAdd(string toParse, string keyName) => ApplyScript(new TableScriptDescription(Table, toParse));

    /// <summary>
    /// Fängt den Scripts-Nav-Satz des Journals ab: Vor dem Anwenden wird der
    /// erste Blocksatz gelesen, um reine Laufzeit-Statistik zu erkennen — in dem
    /// Fall bleiben Zeilen-Prüfung und Script-Event aus. Alles andere macht die
    /// Basis.
    /// </summary>
    public override bool DeserializeThis(Stream stream, FragmentRecord record) {
        if (record.LeafType == (byte)ScriptCollectionFragmentType.Scripts && record.DataType == FragmentDataType.String && record.StringContent is { Length: > 0 }) {
            // Ersten Blocksatz vorab lesen; die Anwendung selbst macht ApplySubItem (liest den Satz erneut).
            var blockStart = stream.Position;
            FragmentRecord? firstLeaf = FragmentRecord.TryReadNext(stream, record.Depth + 1);
            stream.Position = blockStart;

            var statisticsOnly = firstLeaf is { IsTag: false } first && (TableScriptDescription.ScriptFragmentType)first.LeafType is TableScriptDescription.ScriptFragmentType.StoppedTimeCount or TableScriptDescription.ScriptFragmentType.AverageRunTime;

            if (!base.DeserializeThis(stream, record)) { return false; }

            Table?.InvalidateHeadCaches();

            if (!statisticsOnly) {
                Table?.Row.InvalidateAllCheckData();
                Table?.OnScriptChanged();
            }

            return true;
        }

        return base.DeserializeThis(stream, record);
    }

    #endregion
}
