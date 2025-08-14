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

using BlueDatabase.Enums;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
public class Method_Lookup : Method_Database {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, StringVal, StringVal, StringVal];
    public override string Command => "lookup";
    public override List<string> Constants => [];
    public override string Description => "Lädt eine andere Datenbank (Database), sucht eine Zeile (KeyValue) und gibt den Inhalt einer Spalte (Column) als String zurück.\r\n\r\nAchtung: Das Laden einer Datenbank kann sehr Zeitintensiv sein, evtl. ImportLinked benutzen.\r\n\r\nWird der Wert nicht gefunden, wird NothingFoundValue zurück gegeben.\r\nIst der Wert mehrfach vorhanden, wird FoundToMuchValue zurückgegeben.\r\n\r\nÄhnliche Befehle: CellGetRow, ImportLinked";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.Database;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "Lookup(Database, KeyValue, Column, NothingFoundValue, FoundToMuchValue)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, CanDoFeedback ld){
        if (MyDatabase(scp) is not { IsDisposed: false } myDb) { return DoItFeedback.InternerFehler(ld); }

        var db = Database.Get(attvar.ValueStringGet(0), false, null);
        if (db == null) { return new DoItFeedback("Datenbank '" + attvar.ValueStringGet(0) + "' nicht gefunden", true, ld); }

        //if (db != myDb && !db.AreScriptsExecutable()) { return new DoItFeedback($"In der Datenbank '{attvar.ValueStringGet(0)}' sind die Skripte defekt", false, ld); }

        if (db.Column.First is not { IsDisposed: false } cf) {
            return new DoItFeedback("Erste Spalte der Datenbank '" + attvar.ValueStringGet(0) + "' nicht gefunden", true, ld);
        }

        var returncolumn = db.Column[attvar.ValueStringGet(2)];
        if (returncolumn == null) { return new DoItFeedback("Spalte nicht gefunden: " + attvar.ValueStringGet(2), true, ld); }
        returncolumn.AddSystemInfo("Value Used in Script", db, scp.ScriptName);

        var r = RowCollection.MatchesTo(new FilterItem(cf, FilterType.Istgleich_GroßKleinEgal, attvar.ValueStringGet(1)));

        if (r.Count == 0) { return new DoItFeedback(attvar.ValueStringGet(3), ld.EndPosition()); }
        if (r.Count > 1) { return new DoItFeedback(attvar.ValueStringGet(4), ld.EndPosition()); }
        return new DoItFeedback(r[0].CellGetString(returncolumn), ld.EndPosition());
    }

    #endregion
}