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

using BlueBasics.Interfaces;
using BlueTable.Enums;
using BlueScript;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueTable.AdditionalScriptMethods;

// ReSharper disable once ClassNeverInstantiated.Global
public class Method_Filter : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [TableVar, StringVal, StringVal, StringVal];
    public override string Command => "filter";

    public override List<string> Constants => ["IS", "ISNOT", "INSTR", "STARTSWITH", "BETWEEN"];

    public override string Description => "Erstellt einen Filter, der für andere Befehle (z.B. LookupFilter) verwendet werden kann.\r\n" +
                                          "Aktuell werden nur die FilterTypen 'is', 'isnot', 'startswith', 'instr' und 'between' unterstützt.\r\n" +
                                          "Bei diesem Filter wird die Groß/Kleinschreibung ignoriert.\r\n" +
                                          "Bei Between müssen die Werte so Angegeben werden: 50|100";

    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableFilterItem.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "Filter(Table, Spalte, Filtertyp, Wert)";

    #endregion

    #region Methods

    public static (FilterCollection? allFi, string failedReason, bool needsScriptFix) ObjectToFilter(VariableCollection attributes, int ab, Table? sourcetable, string user, bool must) {
        var allFi = new List<FilterItem>();

        for (var z = ab; z < attributes.Count; z++) {
            if (attributes[z] is not VariableFilterItem fi) { return (null, $"Attribut {z + 1} ist kein Filter.", true); } // new DoItFeedback(infos.LogData, s, "Kein Filter übergeben.");

            if (fi.FilterItem is not { } fii) { return (null, $"Attribut {z + 1} enthält keinen Filter.", true); }

            if (fii.Column?.Table is { IsDisposed: false } db) {
                fii.Column.AddSystemInfo("Value Used in Script-Filter", sourcetable ?? db, user);

                if (db.IsDisposed) { return (null, "Tabellefehler!", false); }

                //if (db != sourcetable && !db.AreScriptsExecutable()) { return (null, $"In der Tabelle '{db.Caption}' sind die Skripte defekt", false); }
            }

            if (!fii.IsOk()) { return (null, $"Der Filter des Attributes {z + 1} ist fehlerhaft.", true); }// new DoItFeedback(infos.LogData, s, "Filter fehlerhaft"); }

            if (z > ab) {
                if (fii.Table != allFi[0].Table) { return (null, "Filter über verschiedene Tabellen wird nicht unterstützt.", true); }// new DoItFeedback(infos.LogData, s, "Filter über verschiedene Tabellen wird nicht unterstützt."); }
            }

            allFi.Add(fii);
        }

        if (allFi.Count < 1) {
            if (!must) { return (null, string.Empty, false); }
            return (null, "Fehler bei der Filtererstellung.", true);
        }

        var f = new FilterCollection(allFi[0].Table, "method_filter");
        f.AddIfNotExists(allFi);

        if (!string.IsNullOrEmpty(f.ErrorReason())) { return (null, f.ErrorReason(), true); }

        return (f, string.Empty, false);
    }

    public static FilterType StringToFilterType(string type) {
        switch (type.ToLowerInvariant()) {
            case "is":
                return FilterType.Istgleich_GroßKleinEgal;

            case "isnot":
                return FilterType.Ungleich_MultiRowIgnorieren_GroßKleinEgal;

            case "instr":
                return FilterType.Instr_GroßKleinEgal;

            case "startswith":
                return FilterType.BeginntMit_GroßKleinEgal;

            case "between":
                return FilterType.Between;

            default:
                return FilterType.AlwaysFalse;
        }
    }

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {

        if (attvar.Attributes[0] is not VariableTable vdb || vdb.Table is not { IsDisposed: false } db) { return new DoItFeedback("Tabelle nicht vorhanden", true, ld); }

        //if (db != myDb && !db.AreScriptsExecutable()) { return new DoItFeedback($"In der Tabelle '{attvar.ValueStringGet(0)}' sind die Skripte defekt", false, ld); }

        #region Spalte ermitteln

        var filterColumn = db.Column[attvar.ValueStringGet(1)];
        if (filterColumn == null) { return new DoItFeedback("Spalte '" + attvar.ValueStringGet(1) + "' in Ziel-Tabelle nicht gefunden", true, ld); }

        #endregion

        #region Typ ermitteln

        var filtertype = StringToFilterType(attvar.ValueStringGet(2));

        if (filtertype == FilterType.AlwaysFalse) {
            return new DoItFeedback("Filtertype unbekannt: " + attvar.ValueStringGet(2), true, ld);
        }

        #endregion

        var fii = new FilterItem(filterColumn, filtertype, attvar.ValueStringGet(3));

        if (!fii.IsOk()) {
            return new DoItFeedback("Filter konnte nicht erstellt werden: '" + fii.ErrorReason() + "'", true, ld);
        }

        filterColumn.AddSystemInfo("Filter in Script", db, scp.ScriptName);

        return new DoItFeedback(new VariableFilterItem(fii));
    }

    #endregion
}