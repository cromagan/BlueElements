// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Editoren;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using BlueTable.AdditionalScriptMethods;
using BlueTable.AdditionalScriptVariables;

namespace BlueControls.AdditionalScriptMethods;

public class Method_AddRow : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [TableVar, StringVal, [VariableListString.ShortName_Plain]];
    public override string Command => "addrow";

    public override string Description => "Fügt eine neue Zeile zur Tabelle hinzu.\r\n" +
            "Text ist die Überschrift, die dem Benutzer angezeigt wird.\r\n" +
            "Suggestions ist eine Liste mit Vorschlägen für den Benutzer.\r\nExistiert die Zeile bereits, wird der trotzdem der Bearbeiten Dialog geöffnet\r\n" +
            "Die eigene Zeile kann nur bearbeitet werden, wenn das Skript ReadOnly ist - wirft aber keinen Skriptfehler.";

    public override LastArgMinCountType LastArgMinCount => LastArgMinCountType.MinOnce;
    public override MethodType MethodLevel => MethodType.ManipulatesUser;

    public override string Returns => VariableRowItem.ShortName_Variable;
    public override string Syntax => "AddRow(Table, Text, Suggestions);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.Attributes[0] is not VariableTable vtb || vtb.Table is not { IsDisposed: false } tb) {
            return new DoItFeedback("Tabelle nicht vorhanden", true, ld);
        }

        var f = tb.IsGenericEditable(false);
        if (!string.IsNullOrEmpty(f)) {
            return new DoItFeedback($"Tabellensperre: {f}", true, ld);
        }

        var text = attvar.ValueStringGet(1);
        if (string.IsNullOrEmpty(text)) { text = "Neuer Eintrag:"; }

        var suggestions = attvar.ValueListStringGet(2);

        if (tb.Column.First is not ColumnItem colFirst) {
            return new DoItFeedback($"In der Tabelle '{tb.Caption}' fehlt die Angabe der ersten Spalte.", true, ld);
        }

        if (tb.Column.ChunkValueColumn is ColumnItem chunk && chunk != colFirst) {
            return new DoItFeedback($"In der Tabelle '{tb.Caption}' wegen Chunks nicht möglich.", true, ld);
        }

        text += $"\r\n\r\n<b>{colFirst.Caption}:";

        // Messagebox mit Text und Suggestions anzeigen
        var gewählt = InputBoxComboStyle.Show(text, colFirst, suggestions, true);

        gewählt = colFirst.AutoCorrect(gewählt, true);

        if (string.IsNullOrEmpty(gewählt)) {
            return new DoItFeedback("Eingabe durch Benutzer abgebrochen", false, ld);
        }

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(ld); }

        if (tb.Row[gewählt] is RowItem existingRow) {
            if (existingRow == BlockedRow(scp)) {
                MessageBox.Show("Wert bereits vorhanden,\r\bBearbeitung aktuell nicht möglich.", BlueBasics.Enums.ImageCode.Warnung, "OK");
            } else {
                MessageBox.Show("Wert bereits vorhanden,\r\bBearbeitungsdialog wird geöffnet.", BlueBasics.Enums.ImageCode.Information, "OK");
                existingRow.Edit(typeof(RowEditor), true);
            }

            return Method_Row.RowToObjectFeedback(existingRow);
        }

        var newRow = tb.Row.GenerateAndAdd(gewählt, "Method_AddRow");
        if (newRow is not { IsDisposed: false }) {
            return new DoItFeedback("Zeile konnte nicht erstellt werden", true, ld);
        }

        newRow.Edit(typeof(RowEditor), true);
        return Method_Row.RowToObjectFeedback(newRow);
    }

    #endregion
}