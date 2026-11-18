// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.IO;

namespace BlueTable.Classes;

/// <summary>
/// Beobachtbare Skript-Sammlung der Tabelle. Zwei Betriebsarten: Im Legacy-Format
/// läuft jede Skript-Änderung als ganzer Skript-Satz über den EventScript-Setter
/// der Tabelle. Im binären Fragment-Format reichen Add, Remove und granulare
/// Ersetzungen FragmentChanged mit dem fertigen Scripts-Navigationspfad bis zur
/// Tabelle durch, die sie als gezielte Änderung in die Fragmentdatei schreibt.
/// </summary>
public sealed class TableScriptCollection : KeyedItemCollection<TableScriptDescription>, IHasTable {

    #region Enums

    /// <summary>
    /// Pfad-Typen der Sammlung im binären Fragment-Format; eigener Zahlraum der
    /// Sammlung, die Tabelle routet über ihren eigenen Segmenteintritt eine Stufe
    /// darüber. Überschneidungen mit dem Zahlraum der Skript-Eigenschaften sind
    /// unschädlich, denn die Stufen werden strukturgenau geroutet.
    /// </summary>
    private enum ScriptCollectionFragmentType : byte {

        /// <summary>
        /// ListKeyObject-Satz: die Skripte folgen als Liste eine Ebene tiefer,
        /// zwischen ihnen je ein Trenner zwei Ebenen unter der Liste.
        /// </summary>
        Scripts = 1,

        /// <summary>
        /// Blatt-Satz: übernimmt ein ganzes neues Skript (Content = kompletter
        /// Skript-Block im binären Fragment-Format; ältere Journale reichen den
        /// Classic-Parseable-String des Skripts).
        /// </summary>
        ScriptAdd = 2,

        /// <summary>
        /// Blatt-Satz: entfernt das Skript des Journal-Blocks (Nav-Schlüssel).
        /// </summary>
        ScriptRemove = 3,

        /// <summary>
        /// Nav-Satz des Bubbling; Content = Schlüssel des Skripts, dessen Block
        /// eine Ebene tiefer folgt.
        /// </summary>
        SubScript = 4
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Erstellt die Sammlung; die Tabellen-Referenz wird zum Erzeugen neuer
    /// Skripte beim Einlesen benötigt.
    /// </summary>
    public TableScriptCollection(Table? table) {
        Table = table;
    }

    /// <summary>
    /// Erstellt die Sammlung mit Startbestand; gleichnamige Doppel werden verworfen.
    /// </summary>
    public TableScriptCollection(Table? table, IEnumerable<TableScriptDescription> scripts) {
        Table = table;
        _ = AddRange(scripts);
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
    /// Ersetzt ein Skript granular (Binärformat): Ein geändertes Skript übernimmt die
    /// Werte auf die beobachtete Instanz — die Setter melden nur geänderte Blätter
    /// unter dem Pfad Scripts/Skriptname. Fehlt das Skript, wird es als Aufnahme
    /// gemeldet; bei Namenswechsel als Entfernung unter dem alten und Aufnahme unter
    /// dem neuen Namen. Inhaltlich unveränderte Skripte erzeugen keinen Journal-Eintrag.
    /// </summary>
    public bool Update(TableScriptDescription? script) {
        if (script is null) { return false; }

        // Bestehende Instanz suchen: gleicher Name oder gleiche Instanz (umbenannt)
        var existing = ExistingOrThis(script);

        // Neu: als Aufnahme melden
        if (existing is null) { return Add(script); }

        // Unverändert und nicht die beobachtete Instanz selbst: kein Journal-Eintrag
        if (!ReferenceEquals(existing, script) && existing.ParseableItems().FinishParseable() == script.ParseableItems().FinishParseable()) { return true; }

        // Gleicher Name: die Setter melden nur die tatsächlich geänderten Blätter
        if (string.Equals(existing.KeyName, script.KeyName, StringComparison.OrdinalIgnoreCase)) {
            return existing.Update(script) is { Length: 0 };
        }

        // Umbenennung: Entfernung unter dem alten und Aufnahme unter dem neuen Namen
        _ = Remove(existing.KeyName);
        return Add(script);
    }

    /// <summary>
    /// Gleicht die Sammlung granular an: Fehlende Skripte werden entfernt, neue als
    /// Aufnahme gemeldet; geänderte übernehmen die Werte auf die beobachtete Instanz,
    /// sodass nur geänderte Blätter journalt werden.
    /// </summary>
    public override void UpdateChild(IEnumerable<TableScriptDescription> sourceItems) {
        if (IsDisposed) { return; }

        var sourceKeys = new HashSet<string>(sourceItems.Select(s => s.KeyName), StringComparer.OrdinalIgnoreCase);

        foreach (var keyName in Keys()) {
            if (!sourceKeys.Contains(keyName)) { _ = Remove(keyName); }
        }

        foreach (var sourceItem in sourceItems) {
            _ = Update(sourceItem);
        }
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
    /// Aufnahmen eines ganzen Skripts als Fragment-Block.
    /// </summary>
    protected override Enum AddItemType => ScriptCollectionFragmentType.ScriptAdd;

    /// <summary>
    /// Entfernungen per Journal-Block.
    /// </summary>
    protected override Enum RemoveItemType => ScriptCollectionFragmentType.ScriptRemove;

    /// <summary>
    /// Pfad-Segment des Bubbling.
    /// </summary>
    protected override Enum ItemSubType => ScriptCollectionFragmentType.SubScript;

    /// <summary>
    /// Erzeugt das Skript zum Name-Satz des Segment-Anfangs; die Übernahme in die
    /// Sammlung macht Update (bestehende Instanzen behalten ihre Identität).
    /// </summary>
    protected override TableScriptDescription? CreateItem(FragmentRecord first) => new(Table, first.StringContent, string.Empty);

    /// <summary>
    /// Übernimmt ein ganzes neues Skript aus einem Add-Blatt: Bei älteren Journalen
    /// ist der Content der Classic-Parseable-String des Skripts; der Fragment-Block
    /// neuer Journale läuft über die Basis (ApplyItemAddFragment).
    /// </summary>
    protected override bool ApplyItemAdd(string toParse, string keyName) => ApplyItemAdd(new TableScriptDescription(Table, toParse));

    /// <summary>
    /// Fängt den SubScript-Nav-Satz des Journals ab: Vor dem Anwenden wird der
    /// erste Blocksatz gelesen, um reine Laufzeit-Statistik zu erkennen — in dem
    /// Fall bleiben Zeilen-Prüfung und Script-Event aus. Alles andere macht die
    /// Basis.
    /// </summary>
    public override bool DeserializeThis(Stream stream, FragmentRecord record) {
        if (record.LeafType == (byte)ScriptCollectionFragmentType.SubScript && record.DataType == FragmentDataType.String && record.StringContent is { Length: > 0 }) {
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
