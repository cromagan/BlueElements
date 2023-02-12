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
using BlueScript;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueDatabase.AdditionalScriptComands;

public class Method_LookupFilter : Method_Database {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableString.ShortName_Plain }, new List<string> { VariableString.ShortName_Plain }, new List<string> { VariableString.ShortName_Plain }, new List<string> { VariableFilterItem.ShortName_Variable } };

    public override string Description => "Lädt eine andere Datenbank sucht eine Zeile mit einem Filter und gibt den Inhalt einer Spalte (ReturnColumn) als Liste zurück. Wird der Wert nicht gefunden, wird NothingFoundValue zurück gegeben. Ist der Wert mehrfach vorhanden, wird FoundToMuchValue zurückgegeben. Ein Filter kann mit dem Befehl 'Filter' erstellt werden.";

    public override bool EndlessArgs => true;

    public override string EndSequence => ")";

    public override bool GetCodeBlockAfter => false;

    public override string Returns => VariableListString.ShortName_Plain;

    public override string StartSequence => "(";

    //public Method_Lookup(Script parent) : base(parent) { }
    public override string Syntax => "LookupFilter(ReturnColumn, NothingFoundValue, FoundToMuchValue, Filter, ...)";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "lookupfilter" };

    public override DoItFeedback DoIt(CanDoFeedback infos, Script s, int line) {
        var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs, line);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar, line); }

        var allFi = Method_Filter.ObjectToFilter(attvar.Attributes, 3);

        if (allFi is null) { return new DoItFeedback("Fehler im Filter", line); }

        var returncolumn = allFi[0].Database.Column.Exists(((VariableString)attvar.Attributes[0]).ValueString);
        if (returncolumn == null) { return new DoItFeedback("Spalte nicht gefunden: " + ((VariableString)attvar.Attributes[0]).ValueString, line); }

        var l = new List<string>();

        var r = RowCollection.MatchesTo(allFi);
        if (r.Count == 0) {
            l.Add(((VariableString)attvar.Attributes[1]).ValueString);
            return new DoItFeedback(l, line);
        }
        if (r.Count > 1) {
            l.Add(((VariableString)attvar.Attributes[2]).ValueString);
            return new DoItFeedback(l, line);
        }

        var v = RowItem.CellToVariable(returncolumn, r[0]);
        if (v == null || v.Count != 1) { return new DoItFeedback("Wert konnte nicht erzeugt werden: " + ((VariableString)attvar.Attributes[4]).ValueString, line); }

        if (v[0] is VariableListString vl) {
            l.AddRange(vl.ValueList);
        } else if (v[0] is VariableString vs) {
            l.Add(vs.ValueString);
        } else {
            return new DoItFeedback("Spaltentyp nicht unterstützt.", line);
        }

        //  l.GenerateAndAdd(((VariableString)attvar.Attributes[2]).ValueString);

        return new DoItFeedback(l, line);
    }

    #endregion
}