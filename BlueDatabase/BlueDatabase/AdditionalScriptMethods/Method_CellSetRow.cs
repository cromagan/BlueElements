// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
public class Method_CellSetRow : Method_Database {

    #region Properties

    public override List<List<string>> Args => [[VariableString.ShortName_Plain, VariableListString.ShortName_Plain, VariableFloat.ShortName_Plain], StringVal, RowVar];
    public override string Command => "cellsetrow";
    public override List<string> Constants => [];
    public override string Description => "Setzt den Wert. Gibt TRUE zurück, wenn genau der Wert erfolgreich gesetzt wurde.\r\nWenn automatische Korrektur-Routinen (z.B. Runden) den Wert ändern, wird ebenfalls false zurück gegeben.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;


    // Manipulates User deswegen, weil dann der eigene Benutzer gesetzt wird und das Extended bearbeitungen auslösen könnte
    public override MethodType MethodType => MethodType.Database | MethodType.ManipulatesUser;
    public override bool MustUseReturnValue => false;
    public override string Returns => VariableBool.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "CellSetRow(Value, Column, Row)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var row = Method_Row.ObjectToRow(attvar.Attributes[2]);
        if (row?.Database is not Database db || db.IsDisposed) { return new DoItFeedback(ld, "Fehler in der Zeile"); }

        var columnToSet = db.Column[attvar.ValueStringGet(1)];
        if (columnToSet == null) { return new DoItFeedback(ld, "Spalte nicht gefunden: " + attvar.ValueStringGet(1)); }

        var m = CellCollection.EditableErrorReason(columnToSet, row, EditableErrorReasonType.EditAcut, false, false, true, false);
        if (!string.IsNullOrEmpty(m)) { return DoItFeedback.Falsch(); }
        if (!scp.ProduktivPhase) { return new DoItFeedback(ld, "Zellen setzen Testmodus deaktiviert."); }

        if (row == MyRow(scp)) {
            return new DoItFeedback(ld, "Die eigene Zelle kann nur über die Variabeln geändert werden.");
        }

        var value = string.Empty;
        if (attvar.Attributes[0] is VariableString vs) { value = vs.ValueString; }
        if (attvar.Attributes[0] is VariableListString vl) { value = vl.ValueList.JoinWithCr(); }
        if (attvar.Attributes[0] is VariableFloat vf) { value = vf.ValueForReplace; }

        value = columnToSet.AutoCorrect(value, true);

        row.CellSet(columnToSet, value, "Skript: '" + scp.ScriptName + "' aus '" + db.Caption + "'");
        return row.CellGetString(columnToSet) == value ? DoItFeedback.Wahr() : DoItFeedback.Falsch();
    }

    #endregion
}