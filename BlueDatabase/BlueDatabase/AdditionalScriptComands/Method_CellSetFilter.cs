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

public class Method_CellSetFilter : MethodDatabase {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableString.ShortName_Plain }, new List<string> { VariableString.ShortName_Plain }, new List<string> { VariableFilterItem.ShortName_Variable } };
    public override string Description => "Lädt eine andere Datenbank sucht eine Zeile mit einem Filter und setzt den Wert. Ein Filter kann mit dem Befehl 'Filter' erstellt werden. Gibt TRUE zurück, wenn der Wert erfolgreich gesetzt wurde.";

    public override bool EndlessArgs => true;

    public override string EndSequence => ")";

    public override bool GetCodeBlockAfter => false;

    public override string Returns => VariableBool.ShortName_Plain;

    public override string StartSequence => "(";

    public override string Syntax => "CellSetFilter(Value, Column, Filter,...)";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "cellsetfilter" };

    public override DoItFeedback DoIt(CanDoFeedback infos, Script s, int line) {
        var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs, line);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar, line); }

        var allFi = Method_Filter.ObjectToFilter(attvar.Attributes, 2);
        if (allFi is null) { return new DoItFeedback("Fehler im Filter", line); }

        var columnToSet = allFi[0].Database.Column.Exists(((VariableString)attvar.Attributes[1]).ValueString);
        if (columnToSet == null) { return new DoItFeedback("Spalte nicht gefunden: " + ((VariableString)attvar.Attributes[4]).ValueString, line); }

        var r = RowCollection.MatchesTo(allFi);
        if (r == null || r.Count is 0 or > 1) {
            return DoItFeedback.Falsch(line);
        }

        r[0].CellSet(columnToSet, ((VariableString)attvar.Attributes[0]).ValueString);

        return r[0].CellGetString(columnToSet) == ((VariableString)attvar.Attributes[0]).ValueString ? DoItFeedback.Wahr(line) : DoItFeedback.Falsch(line);
    }

    #endregion
}