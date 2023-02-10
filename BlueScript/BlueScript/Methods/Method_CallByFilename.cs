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

using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.IO;
using static BlueBasics.IO;

namespace BlueScript.Methods;

internal class Method_CallByFilename : Method {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableString.ShortName_Plain }, new List<string> { VariableBool.ShortName_Plain } };

    public override string Description => "Ruft eine Subroutine auf. Diese muss auf der Festplatte im UTF8-Format gespeichert sein.\r\n" +
                                            "Mit KeepVariables kann bestimmt werden, ob die Variablen aus der Subroutine behalten werden sollen.\r\n" +
                                            "Variablen aus der Hauptroutine können in der Subroutine geändert werden und werden zurück gegeben.";

    public override bool EndlessArgs => false;
    public override string EndSequence => ");";
    public override bool GetCodeBlockAfter => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "CallByFilename(Filename, KeepVariables);";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "callbyfilename" };

    public override DoItFeedback DoIt(CanDoFeedback infos, Script s, int line) {
        var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs, line);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar, line); }

        var vs = (VariableString)attvar.Attributes[0];
        string f;

        try {
            if (FileExists(vs.ValueString)) {
                f = File.ReadAllText(vs.ValueString, System.Text.Encoding.UTF8);
            } else if (FileExists(s.AdditionalFilesPath + vs.ValueString)) {
                f = File.ReadAllText(s.AdditionalFilesPath + vs.ValueString, System.Text.Encoding.UTF8);
            } else {
                return new DoItFeedback("Datei nicht gefunden: " + vs.ValueString, line);
            }
        } catch {
            return new DoItFeedback("Fehler beim Lesen der Datei: " + vs.ValueString, line);
        }

        f = Script.ReduceText(f);

        //var weiterLine = s.Line;

        //s.Line = 1;

        s.Sub++;

        if (((VariableBool)attvar.Attributes[1]).ValueBool) {
            var scx = s.Parse(f, 1);
            if (!string.IsNullOrEmpty(scx.ErrorMessage)) { return new DoItFeedback("Subroutine " + vs.ValueString + ": " + scx.ErrorMessage, line); }
        } else {
            var tmpv = new List<Variable>();
            tmpv.AddRange(s.Variables);

            var scx = s.Parse(f, 1);
            if (!string.IsNullOrEmpty(scx.ErrorMessage)) { return new DoItFeedback("Subroutine " + vs.ValueString + ": " + scx.ErrorMessage, line); }

            s.Variables.Clear();
            s.Variables.AddRange(tmpv);
        }
        s.Sub--;

        if (s.Sub < 0) { return new DoItFeedback("Subroutinen-Fehler", line); }

        //s.Line = weiterLine;
        s.BreakFired = false;

        return DoItFeedback.Null(line);
    }

    #endregion
}