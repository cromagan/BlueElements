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

#nullable enable

using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
public class Method_CellGetFilter : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, StringVal, FilterVar];
    public override string Command => "cellgetfilter";
    public override List<string> Constants => [];
    public override string Description => "Lädt eine andere Tabelle sucht eine Zeile mit einem Filter und gibt den Inhalt einer Spalte (ReturnColumn) als String zurück.\r\n\r\nAchtung: Das Laden einer Tabelle kann sehr Zeitintensiv sein, evtl. ImportLinked benutzen.\r\n\r\nWird der Wert nicht gefunden, wird NothingFoundValue zurück gegeben.\r\nIst der Wert mehrfach vorhanden, wird FoundToMuchValue zurückgegeben.\r\nEin Filter kann mit dem Befehl 'Filter' erstellt werden.\r\n\r\nÄhnlichr Befehle: CellGetRow, ImportLinked";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 1;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "CellGetFilter(ReturnColumn, NothingFoundValue, FoundToMuchValue, Filter, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (MyDatabase(scp) is not { IsDisposed: false } myDb) { return DoItFeedback.InternerFehler(ld); }

        var (allFi, errorreason, needsScriptFix) = Method_Filter.ObjectToFilter(attvar.Attributes, 3, myDb, scp.ScriptName, true);

        if (allFi == null || !string.IsNullOrEmpty(errorreason)) { return new DoItFeedback($"Filter-Fehler: {errorreason}", needsScriptFix, ld); }

        if (allFi.Database is not { IsDisposed: false } db) {
            allFi.Dispose();
            return new DoItFeedback("Datenbankfehler!", true, ld);
        }

        var r = allFi.Rows;
        allFi.Dispose();

        var returncolumn = db.Column[attvar.ValueStringGet(0)];
        if (returncolumn == null) { return new DoItFeedback("Spalte nicht gefunden: " + attvar.ValueStringGet(0), true, ld); }
        returncolumn.AddSystemInfo("Value Used in Script", db, scp.ScriptName);

        if (r.Count == 0) { return new DoItFeedback(attvar.ValueStringGet(1)); }
        if (r.Count > 1) { return new DoItFeedback(attvar.ValueStringGet(2)); }

        var v = RowItem.CellToVariable(returncolumn, r[0], true, false);
        if (v == null) { return new DoItFeedback($"Wert der Variable konnte nicht gelesen werden - ist die Spalte {returncolumn.KeyName} 'im Skript vorhanden'?", true, ld); }

        return new DoItFeedback(r[0].CellGetString(returncolumn));
    }

    #endregion
}