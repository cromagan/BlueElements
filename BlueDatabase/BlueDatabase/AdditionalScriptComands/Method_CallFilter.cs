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

using BlueBasics;
using BlueScript;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueDatabase.AdditionalScriptComands;

public class Method_CallFilter : MethodDatabase {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableString.ShortName_Plain }, new List<string> { VariableFilterItem.ShortName_Variable } };
    public override string Description => "Suchte Zeilen und ruft in dessen Datenbank ein Skript für jede Zeile aus. Über den Filten kann bestimmt werden, welche Zeilen es betrifft.";

    public override bool EndlessArgs => true;

    public override string EndSequence => ");";

    public override bool GetCodeBlockAfter => false;

    public override string Returns => string.Empty;

    public override string StartSequence => "(";

    public override string Syntax => "CallFilter(SubName, Filter, ...);";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "callfilter" };

    public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
        var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar); }

        var allFi = Method_Filter.ObjectToFilter(attvar.Attributes, 1);

        if (allFi is null || allFi.Count == 0) { return new DoItFeedback("Fehler im Filter"); }

        //var db = MyDatabase(s);
        if (allFi[0].Database == null) { return new DoItFeedback("Datenbankfehler!"); }

        var r = allFi[0].Database.Row.CalculateFilteredRows(allFi);
        if (r == null || r.Count == 0) { return new DoItFeedback(); }

        var vs = (VariableString)attvar.Attributes[0];
        //var sc = r[0].Database.EventScript.Get(vs.ValueString);

        var weiterLine = s.Line;

        foreach (var thisR in r) {
            if (r != null) {
                s.Line = 1;
                s.Sub++;
                _ = (thisR?.ExecuteScript(null, vs.ValueString, false, true, s.ChangeValues, 0));
                s.Sub--;
            }
        }

        if (s.Sub < 0) { return new DoItFeedback("Subroutinen-Fehler"); }

        s.Line = weiterLine;
        s.BreakFired = false;

        return DoItFeedback.Null();
    }

    #endregion
}