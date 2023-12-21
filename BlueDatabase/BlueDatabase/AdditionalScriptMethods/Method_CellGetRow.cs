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
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedType.Global
public class Method_CellGetRow : Method_Database {

    #region Properties

    public override List<List<string>> Args => [StringVal, RowVar];
    public override string Command => "cellgetrow";
    public override string Description => "Gibt den Wert einer Zelle zurück\r\nÄhnlicher Befehl: Lookup";
    public override bool EndlessArgs => false;
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.Database | MethodType.NeedLongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableListString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "CellGetRow(Column, Row)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, EndlessArgs, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) {
            return DoItFeedback.AttributFehler(infos.Data, this, attvar);
        }

        var row = Method_Row.ObjectToRow(attvar.Attributes[1]);
        if (row?.Database is not Database db || db.IsDisposed) { return new DoItFeedback(infos.Data, "Fehler in der Zeile"); }

        var c = db.Column.Exists(attvar.ValueStringGet(0));
        if (c == null) { return new DoItFeedback(infos.Data, "Spalte nicht gefunden: " + attvar.ValueStringGet(0)); }

        var v = RowItem.CellToVariable(c, row);
        if (v == null || v.Count != 1) {
            return new DoItFeedback(infos.Data, "Wert der Variable konnte nicht gelesen werden: " + attvar.ValueStringGet(0));
        }

        var l = new List<string>();

        if (v[0] is VariableListString vl) {
            l.AddRange(vl.ValueList);
        } else if (v[0] is VariableString vs) {
            var w = vs.ValueString;
            if (!string.IsNullOrEmpty(w)) { l.Add(w); }
        } else {
            return new DoItFeedback(infos.Data, "Spaltentyp nicht unterstützt.");
        }

        return new DoItFeedback(l);
    }

    #endregion
}