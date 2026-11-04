// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.Renderer;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.ScriptVariables;

namespace BlueControls.ScriptCommands;

/// <summary>
/// Setzt die optischen Ersetzungen ('Text ersetzen') einer Spalte, eine Regel pro Listeneintrag im Format alt|neu.
/// Eine leere Liste löscht die Ersetzungen. Nur wirksam bei Spalten im Renderer 'ImageAndText' mit sichtbarem Text.
/// </summary>
public class SetOpticalReplacementsScriptCommand : TableGenericScriptCommand {

    #region Properties

    public override List<List<string>> Args => [[StringScriptVariable.ClassId, ListOfStringsScriptVariable.ClassId], [ListOfStringsScriptVariable.ClassId]];
    public override string Command => "setopticalreplacements";
    public override ScriptCommandType ScriptCommandLevel => ScriptCommandType.Sub;
    public override string Syntax => "SetOpticalReplacements(Column, Replacements);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp) {
        if (MyTable(scp) is not { IsDisposed: false } tb) {
            return new DoItFeedback("Tabelle nicht vorhanden", true);
        }

        var f = tb.IsGenericEditable(false);
        if (!string.IsNullOrEmpty(f)) {
            return new DoItFeedback($"Tabellensperre: {f}", true);
        }

        var replacements = attvar.ValueListStringGet(1);

        var invalid = replacements.Where(l => l.Length > 0 && !IsValidRule(l)).ToList();
        if (invalid.Count > 0) {
            return new DoItFeedback("Ungültige Regeln, eine Regel pro Listeneintrag im Format alt|neu: " + string.Join(" ; ", invalid), true);
        }

        var column = Column(scp, attvar, 0);
        if (column is not { IsDisposed: false }) {
            return new DoItFeedback("Spalte in Tabelle nicht gefunden", true);
        }

        if (ImageAndTextRenderer.Of(column) is not { } r) {
            return new DoItFeedback($"Die Spalte '{column.KeyName}' nutzt nicht den Renderer '{ImageAndTextRenderer.ClassId}' — Optische Ersetzungen gibt es nur dort.", true);
        }

        if (!r.Text_anzeigen) {
            return new DoItFeedback($"Die Spalte '{column.KeyName}' zeigt keinen Text an — Optische Ersetzungen wären wirkungslos.", true);
        }

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(); }

        r.Text_ersetzen = new(replacements);
        column.RendererSettings = r.ParseableItems().FinishParseable();

        return DoItFeedback.Null();
    }

    /// <summary>
    /// Prüft, ob eine Regel dem Format alt|neu mit Nicht-leerem Suchtext entspricht.
    /// </summary>
    private static bool IsValidRule(string rule) => rule.SplitBy("|") is { Length: 2 } parts && parts[0].Length > 0;

    #endregion
}
