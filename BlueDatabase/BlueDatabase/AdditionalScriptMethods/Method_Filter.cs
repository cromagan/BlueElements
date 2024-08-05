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

using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using BlueScript;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once ClassNeverInstantiated.Global
public class Method_Filter : Method_Database {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, StringVal, StringVal];
    public override string Command => "filter";

    public override List<string> Constants => ["IS", "ISNOT", "INSTR", "STARTSWITH", "BETWEEN"];

    public override string Description => "Erstellt einen Filter, der für andere Befehle (z.B. LookupFilter) verwendet werden kann.\r\n" +
                                             "Aktuell werden nur die FilterTypen 'is', 'isnot', 'startswith' und 'instr' unterstützt.\r\n" +
                                         "Bei diesem Filter wird die Groß/Kleinschreibung ignoriert.\r\n" +
                                         "Alternative: FilterInMyDB - erstellt einen Filter der aktuellen Datanbank und kann deswegen in Routinen benutzt werden, die schnell abgehandelt werden müssen.";

    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 1;
    public override MethodType MethodType => MethodType.Database | MethodType.IO;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableFilterItem.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "Filter(Datenbank, Spalte, Filtertyp, Wert)";

    #endregion

    #region Methods

    public static FilterCollection? ObjectToFilter(VariableCollection attributes, int ab) {
        var allFi = new List<FilterItem>();

        for (var z = ab; z < attributes.Count; z++) {
            if (attributes[z] is not VariableFilterItem fi) { return null; } // new DoItFeedback(infos.LogData, s, "Kein Filter übergeben.");

            //var fi = new FilterItem(attributes[z].ObjectData());

            if (!fi.FilterItem.IsOk()) { return null; }// new DoItFeedback(infos.LogData, s, "Filter fehlerhaft"); }

            if (z > ab) {
                if (fi.FilterItem.Database != allFi[0].Database) { return null; }// new DoItFeedback(infos.LogData, s, "Filter über verschiedene Datenbanken wird nicht unterstützt."); }
            }

            if (fi.FilterItem.Clone() is FilterItem fin) {
                // Müssen Clone sein. Die  Routine kann mehrfach ausgelöst werden und dann gehört der Filter bereits einer Collection an
                allFi.Add(fin);
            }
        }

        if (allFi.Count < 1) { return null; }

        var f = new FilterCollection(allFi[0].Database, "method_filter");
        f.AddIfNotExists(allFi);
        return f;
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

            default:
                return FilterType.AlwaysFalse;
        }
    }

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var db = DatabaseOf(scp, attvar.ValueStringGet(0));
        if (db == null) { return new DoItFeedback(ld, "Datenbank '" + attvar.ValueStringGet(0) + "' nicht gefunden"); }

        #region Spalte ermitteln

        var filterColumn = db.Column[attvar.ValueStringGet(1)];
        if (filterColumn == null) { return new DoItFeedback(ld, "Spalte '" + attvar.ValueStringGet(1) + "' in Ziel-Datenbank nicht gefunden"); }

        #endregion

        #region Typ ermitteln

        FilterType filtertype = StringToFilterType(attvar.ValueStringGet(2));

        if (filtertype == FilterType.AlwaysFalse) {
            return new DoItFeedback(ld, "Filtertype unbekannt: " + attvar.ValueStringGet(2));
        }

        #endregion

        var fii = new FilterItem(filterColumn, filtertype, attvar.ValueStringGet(3));

        if (!fii.IsOk()) {
            return new DoItFeedback(ld, "Filter konnte nicht erstellt werden: '" + fii.ErrorReason() + "'");
        }

        return new DoItFeedback(new VariableFilterItem(fii));
    }

    #endregion
}