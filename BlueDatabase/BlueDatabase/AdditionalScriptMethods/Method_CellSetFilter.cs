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

using System.Collections.Generic;
using BlueBasics;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedType.Global
public class Method_CellSetFilter : Method_Database {

    #region Properties

    public override List<List<string>> Args => [[VariableString.ShortName_Plain, VariableListString.ShortName_Plain, VariableFloat.ShortName_Plain], StringVal, FilterVar];
    public override string Command => "cellsetfilter";
    public override string Description => "Lädt eine andere Datenbank sucht eine Zeile mit einem Filter und setzt den Wert.\r\nEin Filter kann mit dem Befehl 'Filter' erstellt werden.\r\nGibt TRUE zurück, wenn genau der Wert erfolgreich gesetzt wurde.\r\nWenn automatische Korrektur-Routinen (z.B. Runden) den Wert ändern, wird ebenfalls false zurück gegeben.";
    public override bool EndlessArgs => true;
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.ChangeAnyDatabaseOrRow | MethodType.NeedLongTime;
    public override bool MustUseReturnValue => false;
    public override string Returns => VariableBool.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "CellSetFilter(Value, Column, Filter,...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, EndlessArgs, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var allFi = Method_Filter.ObjectToFilter(attvar.Attributes, 2);
        if (allFi is null || allFi.Count == 0) { return new DoItFeedback(infos.Data, "Fehler im Filter"); }

        var db = allFi.Database;
        if (db == null || db.IsDisposed) { return new DoItFeedback(infos.Data, "Datenbank verworfen."); }

        var columnToSet = db.Column.Exists(attvar.ValueStringGet(1));
        if (columnToSet == null) { return new DoItFeedback(infos.Data, "Spalte nicht gefunden: " + attvar.ValueStringGet(4)); }

        var r = allFi.Rows;
        if (r.Count is 0 or > 1) {
            return DoItFeedback.Falsch();
        }

        if (r[0] == MyRow(scp)) {
            return new DoItFeedback(infos.Data, "Die eigene Zelle kann nur über die Variabeln geändert werden.");
        }

        var value = string.Empty;
        if (attvar.Attributes[0] is VariableString vs) { value = vs.ValueString; }
        if (attvar.Attributes[0] is VariableListString vl) { value = vl.ValueList.JoinWithCr(); }
        if (attvar.Attributes[0] is VariableFloat vf) { value = vf.ValueForReplace; }

        value = columnToSet.AutoCorrect(value, true);

        r[0].CellSet(columnToSet, value);

        return r[0].CellGetString(columnToSet) == value ? DoItFeedback.Wahr() : DoItFeedback.Falsch();
    }

    #endregion
}