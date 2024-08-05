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

using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using static BlueDatabase.AdditionalScriptMethods.Method_Database;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedType.Global
public class Method_CellGetRow : BlueScript.Methods.Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, RowVar];
    public override string Command => "cellgetrow";
    public override List<string> Constants => [];
    public override string Description => "Gibt den Wert einer Zelle zurück\r\nÄhnlicher Befehl: Lookup";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableListString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "CellGetRow(Column, Row)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var row = Method_Row.ObjectToRow(attvar.Attributes[1]);
        if (row?.Database is not Database db || db.IsDisposed) { return new DoItFeedback(ld, "Fehler in der Zeile"); }

        if (db.Column[attvar.ValueStringGet(0)] is not ColumnItem c) { return new DoItFeedback(ld, "Spalte nicht gefunden: " + attvar.ValueStringGet(0)); }

        var v = RowItem.CellToVariable(c, row, true, false);
        if (v == null) { return new DoItFeedback(ld, $"Wert der Variable konnte nicht gelesen werden - ist die Spalte {c.KeyName} 'im Skript vorhanden'?"); }
        //if (v == null) {
        //    return new DoItFeedback(ld, "Wert der Variable konnte nicht gelesen werden: " + attvar.ValueStringGet(0));
        //}

        var l = new List<string>();

        if (v is VariableListString vl) {
            l.AddRange(vl.ValueList);
        } else if (v is VariableString vs) {
            var w = vs.ValueString;
            if (!string.IsNullOrEmpty(w)) { l.Add(w); }
        } else if (v is VariableFloat vf) {
            var w = vf.ValueForReplace;
            if (!string.IsNullOrEmpty(w)) { l.Add(w); }
        } else {
            return new DoItFeedback(ld, "Spaltentyp nicht unterstützt.");
        }

        return new DoItFeedback(l);
    }

    #endregion
}