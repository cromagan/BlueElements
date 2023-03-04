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
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueDatabase.AdditionalScriptComands.Method_Database;

namespace BlueDatabase.AdditionalScriptComands;

public class Method_CellSetFilter : Method_Database {

    #region Properties

    public override List<List<string>> Args => new() { StringVal, StringVal, FilterVar };
    public override string Description => "Lädt eine andere Datenbank sucht eine Zeile mit einem Filter und setzt den Wert. Ein Filter kann mit dem Befehl 'Filter' erstellt werden. Gibt TRUE zurück, wenn der Wert erfolgreich gesetzt wurde.";
    public override bool EndlessArgs => true;
    public override string EndSequence => ")";
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.ChangeAnyDatabaseOrRow | MethodType.NeedLongTime;
    public override string Returns => VariableBool.ShortName_Plain;

    public override string StartSequence => "(";

    public override string Syntax => "CellSetFilter(Value, Column, Filter,...)";

    #endregion

    #region Methods

    public override List<string> Comand(List<Variable> currentvariables) => new() { "cellsetfilter" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs, infos.Data);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var allFi = Method_Filter.ObjectToFilter(attvar.Attributes, 2);
        if (allFi is null) { return new DoItFeedback(infos.Data, "Fehler im Filter"); }

        var columnToSet = allFi[0].Database.Column.Exists(((VariableString)attvar.Attributes[1]).ValueString);
        if (columnToSet == null) { return new DoItFeedback(infos.Data, "Spalte nicht gefunden: " + ((VariableString)attvar.Attributes[4]).ValueString); }

        var r = RowCollection.MatchesTo(allFi);
        if (r == null || r.Count is 0 or > 1) {
            return DoItFeedback.Falsch();
        }

        if (r[0] == MyRow(s.Variables)) {
            return new DoItFeedback(infos.Data, "Die eigene Zelle kann nur über die Variabeln geändert werden.");
        }

        r[0].CellSet(columnToSet, ((VariableString)attvar.Attributes[0]).ValueString);

        return r[0].CellGetString(columnToSet) == ((VariableString)attvar.Attributes[0]).ValueString ? DoItFeedback.Wahr() : DoItFeedback.Falsch();
    }

    #endregion
}