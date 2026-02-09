// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics;
using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using BlueTable.Classes;
using System.Collections.Generic;

namespace BlueTable.AdditionalScriptMethods;

public class Method_CellGetRow : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [StringVal, RowVar];
    public override string Command => "cellgetrow";
    public override List<string> Constants => [];
    public override string Description => "Gibt den Wert einer Zelle zurück\r\nÄhnlicher Befehl: Lookup";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "CellGetRow(Column, Row)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.ValueRowGet(1) is not { IsDisposed: false } row) { return new DoItFeedback("Zeile nicht gefunden", true, ld); }
        if (row.Table is not { IsDisposed: false } tb) { return new DoItFeedback("Fehler in der Zeile", true, ld); }

        if (row == BlockedRow(scp)) {
            return new DoItFeedback("Zugriff der Werte der eigenen Zeile nur über Variablen möglich.", true, ld);
        }

        //if (db != myDb && !db.AreScriptsExecutable()) { return new DoItFeedback($"In der Tabelle '{db.Caption}' sind die Skripte defekt", false, ld); }

        if (tb.Column[attvar.ValueStringGet(0)] is not { IsDisposed: false } c) { return new DoItFeedback("Spalte nicht gefunden: " + attvar.ValueStringGet(0), true, ld); }

        var v = RowItem.CellToVariable(c, row, true, false);
        if (v == null) { return new DoItFeedback($"Wert der Variable konnte nicht gelesen werden - ist die Spalte '{c.KeyName} 'im Skript vorhanden'?", true, ld); }

        var l = new List<string>();

        if (v is VariableListString vl) {
            l.AddRange(vl.ValueList);
        } else if (v is VariableString vs) {
            var w = vs.ValueString;
            if (!string.IsNullOrEmpty(w)) { l.Add(w); }
        } else if (v is VariableDouble vf) {
            var w = vf.ValueForReplace;
            if (!string.IsNullOrEmpty(w)) { l.Add(w); }
        } else {
            return new DoItFeedback("Spaltentyp nicht unterstützt.", true, ld);
        }

        return new DoItFeedback(l.JoinWithCr());
    }

    #endregion
}