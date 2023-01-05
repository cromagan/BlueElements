// Authors:
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

using BlueScript;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueDatabase.AdditionalScriptComands;

public class Method_Filter : MethodDatabase {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableString.ShortName_Plain }, new List<string> { VariableString.ShortName_Plain }, new List<string> { VariableString.ShortName_Plain }, new List<string> { VariableString.ShortName_Plain } };
    public override string Description => "Erstellt einen Filter, der für andere Befehle (z.B. LookupFilter) verwendet werden kann. Aktuell wird nur der FilterTyp 'is' unterstützt. Bei diesem Filter wird die Groß/Kleinschreibung ignoriert.";

    public override bool EndlessArgs => true;

    public override string EndSequence => ")";

    public override bool GetCodeBlockAfter => false;

    public override string Returns => VariableFilterItem.ShortName_Variable;

    public override string StartSequence => "(";

    public override string Syntax => "Filter(Datenbank, Spalte, Filtertyp, Wert)";

    #endregion

    #region Methods

    public static List<FilterItem>? ObjectToFilter(List<Variable>? attributes, int ab) {
        if (attributes == null) { return null; }

        var allFi = new List<FilterItem>();

        for (var z = ab; z < attributes.Count; z++) {
            if (attributes[z] is not VariableFilterItem fi) { return null; } // new DoItFeedback("Kein Filter übergeben.");

            //var fi = new FilterItem(attributes[z].ObjectData());

            if (!fi.FilterItem.IsOk()) { return null; }// new DoItFeedback("Filter fehlerhaft"); }

            if (z > ab) {
                if (fi.FilterItem.Database != allFi[0].Database) { return null; }// new DoItFeedback("Filter über verschiedene Datenbanken wird nicht unterstützt."); }
            }
            allFi.Add(fi.FilterItem);
        }

        return allFi.Count < 1 ? null : allFi;
    }

    public override List<string> Comand(Script? s) => new() { "filter" };

    public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
        var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar); }

        var db = DatabaseOf(s, ((VariableString)attvar.Attributes[0]).ValueString);
        if (db == null) { return new DoItFeedback("Datenbank '" + ((VariableString)attvar.Attributes[0]).ValueString + "' nicht gefunden"); }

        #region Spalte ermitteln

        var filterColumn = db.Column.Exists(((VariableString)attvar.Attributes[1]).ValueString);
        if (filterColumn == null) { return new DoItFeedback("Spalte '" + ((VariableString)attvar.Attributes[1]).ValueString + "' in Ziel-Datenbank nicht gefunden"); }

        #endregion

        #region Typ ermitteln

        Enums.FilterType filtertype;
        switch (((VariableString)attvar.Attributes[2]).ValueString.ToLower()) {
            case "is":
                filtertype = Enums.FilterType.Istgleich_GroßKleinEgal;
                break;

            default:
                return new DoItFeedback("Filtertype unbekannt: " + ((VariableString)attvar.Attributes[2]).ValueString);
        }

        #endregion

        var fii = new FilterItem(filterColumn, filtertype, ((VariableString)attvar.Attributes[3]).ValueString);
        return new DoItFeedback(new VariableFilterItem(fii));
    }

    #endregion
}