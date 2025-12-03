// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

using BlueScript;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using BlueTable.Enums;
using System.Collections.Generic;

namespace BlueTable.AdditionalScriptMethods;

public class Method_CellGetKey : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [TableVar, StringVal, StringVal, StringVal, StringVal];
    public override string Command => "cellgetkey";
    public override List<string> Constants => [];
    public override string Description => "Sucht eine Zeile (KeyValue) und gibt den Inhalt einer Spalte (Column) als String zurück.\r\n\r\nAchtung: Das Laden einer Tabelle kann sehr Zeitintensiv sein, evtl. ImportLinked benutzen.\r\n\r\nWird der Wert nicht gefunden, wird NothingFoundValue zurück gegeben.\r\nIst der Wert mehrfach vorhanden, wird FoundToMuchValue zurückgegeben.\r\n\r\nÄhnliche Befehle: CellGetRow, ImportLinked";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.LongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "CellGetKey(Table, KeyValue, Column, NothingFoundValue, FoundToMuchValue)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.Attributes[0] is not VariableTable vtb || vtb.Table is not { IsDisposed: false } tb) { return new DoItFeedback("Tabelle nicht vorhanden", true, ld); }
        //if (tb != myDb && !tb.AreScriptsExecutable()) { return new DoItFeedback($"In der Tabelle '{attvar.ValueStringGet(0)}' sind die Skripte defekt", false, ld); }

        if (tb.Column.First is not { IsDisposed: false } cf) {
            return new DoItFeedback("Erste Spalte der Tabelle '" + attvar.ValueStringGet(0) + "' nicht gefunden", true, ld);
        }

        var returncolumn = tb.Column[attvar.ValueStringGet(2)];
        if (returncolumn == null) { return new DoItFeedback("Spalte nicht gefunden: " + attvar.ValueStringGet(2), true, ld); }
        returncolumn.AddSystemInfo("Value Used in Script", tb, scp.ScriptName);

        var r = RowCollection.MatchesTo(new FilterItem(cf, FilterType.Istgleich_GroßKleinEgal, attvar.ValueStringGet(1)));

        if (r.Count == 0) { return new DoItFeedback(attvar.ValueStringGet(3)); }
        if (r.Count > 1) { return new DoItFeedback(attvar.ValueStringGet(4)); }
        return new DoItFeedback(r[0].CellGetString(returncolumn));
    }

    #endregion
}