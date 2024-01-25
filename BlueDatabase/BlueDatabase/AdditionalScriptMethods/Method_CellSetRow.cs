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

using System.Collections.Generic;
using BlueBasics;
using BlueBasics.Enums;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
public class Method_CellSetRow : Method_Database {

    #region Properties

    public override List<List<string>> Args => [[VariableString.ShortName_Plain, VariableListString.ShortName_Plain, VariableFloat.ShortName_Plain], StringVal, RowVar];
    public override string Command => "cellsetrow";
    public override string Description => "Setzt den Wert. Gibt TRUE zurück, wenn genau der Wert erfolgreich gesetzt wurde.\r\nWenn automatische Korrektur-Routinen (z.B. Runden) den Wert ändern, wird ebenfalls false zurück gegeben.";
     public override int LastArgMinCount => -1;
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.ChangeAnyDatabaseOrRow;
    public override bool MustUseReturnValue => false;
    public override string Returns => VariableBool.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "CellSetRow(Value, Column, Row)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, LastArgMinCount, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var row = Method_Row.ObjectToRow(attvar.Attributes[2]);
        if (row?.Database is not Database db || db.IsDisposed) { return new DoItFeedback(infos.Data, "Fehler in der Zeile"); }

        var columnToSet = db.Column.Exists(attvar.ValueStringGet(1));
        if (columnToSet == null) { return new DoItFeedback(infos.Data, "Spalte nicht gefunden: " + attvar.ValueStringGet(1)); }

        var m = CellCollection.EditableErrorReason(columnToSet, row, EditableErrorReasonType.EditAcut, false, false, true, false);
        if (!string.IsNullOrEmpty(m)) { return new DoItFeedback(infos.Data, "Datenbank-Meldung: " + m); }
        if (!scp.ChangeValues) { return new DoItFeedback(infos.Data, "Zellen setzen Testmodus deaktiviert."); }

        if (row == MyRow(scp)) {
            return new DoItFeedback(infos.Data, "Die eigene Zelle kann nur über die Variabeln geändert werden.");
        }

        var value = string.Empty;
        if (attvar.Attributes[0] is VariableString vs) { value = vs.ValueString; }
        if (attvar.Attributes[0] is VariableListString vl) { value = vl.ValueList.JoinWithCr(); }
        if (attvar.Attributes[0] is VariableFloat vf) { value = vf.ValueForReplace; }

        value = columnToSet.AutoCorrect(value, true);

        row.CellSet(columnToSet, value);
        return row.CellGetString(columnToSet) == value ? DoItFeedback.Wahr() : DoItFeedback.Falsch();
    }

    #endregion
}