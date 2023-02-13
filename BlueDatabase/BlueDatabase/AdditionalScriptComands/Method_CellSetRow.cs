// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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
using BlueScript;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueDatabase.AdditionalScriptComands;

public class Method_CellSetRow : Method_Database {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableString.ShortName_Plain }, new List<string> { VariableString.ShortName_Plain }, new List<string> { VariableRowItem.ShortName_Variable } };

    public override string Description => "Setzt den Wert. Gibt TRUE zurück, wenn der Wert erfolgreich gesetzt wurde.";

    public override bool EndlessArgs => false;

    public override string EndSequence => ")";

    public override bool GetCodeBlockAfter => false;

    public override string Returns => VariableBool.ShortName_Plain;
    public override string StartSequence => "(";

    public override string Syntax => "CellSetRow(Value, Column, Row)";

    #endregion

    #region Methods

    public override List<string> Comand(List<Variable> currentvariables) => new() { "cellsetrow" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos, this, attvar); }

        var row = Method_Row.ObjectToRow(attvar.Attributes[2]);
        if (row?.Database is null || row.Database.IsDisposed) { return new DoItFeedback(infos, "Fehler in der Zeile"); }

        var columnToSet = row.Database.Column.Exists(((VariableString)attvar.Attributes[1]).ValueString);
        if (columnToSet == null) { return new DoItFeedback(infos, "Spalte nicht gefunden: " + ((VariableString)attvar.Attributes[1]).ValueString); }

        if (row?.Database?.ReadOnly ?? true) { return new DoItFeedback(infos, "Datenbank schreibgeschützt."); }
        if (!s.ChangeValues) { return new DoItFeedback(infos, "Zellen setzen Testmodus deaktiviert."); }

        row.CellSet(columnToSet, ((VariableString)attvar.Attributes[0]).ValueString);

        return row.CellGetString(columnToSet) == ((VariableString)attvar.Attributes[0]).ValueString ? DoItFeedback.Wahr(infos) : DoItFeedback.Falsch(infos);
    }

    #endregion
}