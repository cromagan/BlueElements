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
using System.IO;
using System.Text;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.IO;

namespace BlueScript.Methods;

public class Method_CallByFilename : Method {

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

    public static DoItFeedback CallSub(Script s, CanDoFeedback infos, string reducedscripttext, bool keepVariables, int lineadd, string name) {
        s.Sub++;

        if (keepVariables) {
            var scx = s.Parse(reducedscripttext, lineadd);
            if (!string.IsNullOrEmpty(scx.ErrorMessage)) { return new DoItFeedback("Fehler innerhalb " + name + ": " + scx.ErrorMessage, scx.LastlineNo); }
        } else {
            var tmpv = new List<Variable>();
            tmpv.AddRange(s.Variables);

            var scx = s.Parse(reducedscripttext, lineadd);
            if (!string.IsNullOrEmpty(scx.ErrorMessage)) { return new DoItFeedback("Fehler innerhalb " + name + ": " + scx.ErrorMessage, scx.LastlineNo); }

            s.Variables.Clear();
            s.Variables.AddRange(tmpv);
        }
        s.Sub--;

        if (s.Sub < 0) { return new DoItFeedback(infos, "Subroutinen-Fehler"); }

        return DoItFeedback.Null(infos);
    }

    public override List<string> Comand(List<Variable> currentvariables) => new() { "callbyfilename" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos, this, attvar); }

        var vs = (VariableString)attvar.Attributes[0];
        string f;

        try {
            if (FileExists(vs.ValueString)) {
                f = File.ReadAllText(vs.ValueString, Encoding.UTF8);
            } else if (FileExists(s.AdditionalFilesPath + vs.ValueString)) {
                f = File.ReadAllText(s.AdditionalFilesPath + vs.ValueString, Encoding.UTF8);
            } else {
                return new DoItFeedback(infos, "Datei nicht gefunden: " + vs.ValueString);
            }
        } catch {
            return new DoItFeedback(infos, "Fehler beim Lesen der Datei: " + vs.ValueString);
        }

        f = Script.ReduceText(f);

        return CallSub(s, infos, f, ((VariableBool)attvar.Attributes[1]).ValueBool, 0, vs.ValueString);
    }

    #endregion
}