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

using System.Collections.Generic;
using BlueBasics;
using BlueScript;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueDatabase.AdditionalScriptComands;

internal class Method_Call : Method_Database {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableString.ShortName_Plain }, new List<string> { VariableBool.ShortName_Plain } };

    public override string Description => "Ruft eine Subroutine auf.\r\n" +
        "Mit KeepVariables kann bestimmt werden, ob die Variablen aus der Subroutine behalten werden sollen.\r\n" +
        "Variablen aus der Hauptroutine können in der Subroutine geändert werden und werden zurück gegeben.";

    public override bool EndlessArgs => false;
    public override string EndSequence => ");";
    public override bool GetCodeBlockAfter => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "Call(SubName, KeepVariables);";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "call" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(s, infos, this, attvar); }

        var vs = (VariableString)attvar.Attributes[0];

        var db = MyDatabase(s);
        if (db == null) { return new DoItFeedback(s, infos, "Datenbankfehler!"); }

        var sc = db.EventScript.Get(vs.ValueString);

        if (sc == null) { return new DoItFeedback(s, infos, "Skript nicht vorhanden: " + vs.ValueString); }
        var f = Script.ReduceText(sc.Script);

        //var weiterLine = s.Line;
        //s.Line = 1;
        s.Sub++;

        if (((VariableBool)attvar.Attributes[1]).ValueBool) {
            var scx = s.Parse(f, 0);
            if (!string.IsNullOrEmpty(scx.ErrorMessage)) { return new DoItFeedback(s, infos, "Subroutine '" + vs.ValueString + "' Zeile " + scx.LastlineNo + ": " + scx.ErrorMessage); }
        } else {
            var tmpv = new List<Variable>();
            tmpv.AddRange(s.Variables);

            var scx = s.Parse(f, 0);
            if (!string.IsNullOrEmpty(scx.ErrorMessage)) { return new DoItFeedback(s, infos, "Subroutine '" + vs.ValueString + "' Zeile " + scx.LastlineNo + ": " + scx.ErrorMessage); }

            s.Variables.Clear();
            s.Variables.AddRange(tmpv);
        }
        s.Sub--;

        if (s.Sub < 0) { return new DoItFeedback(s, infos, "Subroutinen-Fehler"); }

        //s.Line = weiterLine;
        s.BreakFired = false;

        return DoItFeedback.Null(s, infos);
    }

    #endregion
}