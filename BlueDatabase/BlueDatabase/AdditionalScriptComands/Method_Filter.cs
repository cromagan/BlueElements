﻿// Authors:
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
using BlueDatabase.Enums;
using BlueScript;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueDatabase.AdditionalScriptComands;

public class Method_Filter : Method_Database {

    #region Properties

    public override List<List<string>> Args => new() { StringVal, StringVal, StringVal, StringVal };
    public override string Description => "Erstellt einen Filter, der für andere Befehle (z.B. LookupFilter) verwendet werden kann. Aktuell werden nur die FilterTypen 'is' und 'isnot' unterstützt. Bei diesem Filter wird die Groß/Kleinschreibung ignoriert.";
    public override bool EndlessArgs => true;
    public override string EndSequence => ")";
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.IO | MethodType.NeedLongTime;
    public override string Returns => VariableFilterItem.ShortName_Variable;

    public override string StartSequence => "(";

    public override string Syntax => "Filter(Datenbank, Spalte, Filtertyp, Wert)";

    #endregion

    #region Methods

    public static List<FilterItem>? ObjectToFilter(List<Variable>? attributes, int ab) {
        if (attributes == null) { return null; }

        var allFi = new List<FilterItem>();

        for (var z = ab; z < attributes.Count; z++) {
            if (attributes[z] is not VariableFilterItem fi) { return null; } // new DoItFeedback(infos.LogData, s, "Kein Filter übergeben.");

            //var fi = new FilterItem(attributes[z].ObjectData());

            if (!fi.FilterItem.IsOk()) { return null; }// new DoItFeedback(infos.LogData, s, "Filter fehlerhaft"); }

            if (z > ab) {
                if (fi.FilterItem.Database != allFi[0].Database) { return null; }// new DoItFeedback(infos.LogData, s, "Filter über verschiedene Datenbanken wird nicht unterstützt."); }
            }
            allFi.Add(fi.FilterItem);
        }

        return allFi.Count < 1 ? null : allFi;
    }

    public override List<string> Comand(List<Variable> currentvariables) => new() { "filter" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs, infos.Data);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var db = DatabaseOf(s.Variables, ((VariableString)attvar.Attributes[0]).ValueString);
        if (db == null) { return new DoItFeedback(infos.Data, "Datenbank '" + ((VariableString)attvar.Attributes[0]).ValueString + "' nicht gefunden"); }

        #region Spalte ermitteln

        var filterColumn = db.Column.Exists(((VariableString)attvar.Attributes[1]).ValueString);
        if (filterColumn == null) { return new DoItFeedback(infos.Data, "Spalte '" + ((VariableString)attvar.Attributes[1]).ValueString + "' in Ziel-Datenbank nicht gefunden"); }

        #endregion

        #region Typ ermitteln

        FilterType filtertype;
        switch (((VariableString)attvar.Attributes[2]).ValueString.ToLower()) {
            case "is":
                filtertype = FilterType.Istgleich_GroßKleinEgal;
                break;

            case "isnot":
                filtertype = FilterType.Ungleich_MultiRowIgnorieren_GroßKleinEgal;
                break;

            default:
                return new DoItFeedback(infos.Data, "Filtertype unbekannt: " + ((VariableString)attvar.Attributes[2]).ValueString);
        }

        #endregion

        var fii = new FilterItem(filterColumn, filtertype, ((VariableString)attvar.Attributes[3]).ValueString);
        return new DoItFeedback(new VariableFilterItem(fii));
    }

    #endregion
}