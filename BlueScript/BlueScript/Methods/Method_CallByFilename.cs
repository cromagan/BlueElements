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
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.IO;

namespace BlueScript.Methods;

public class Method_CallByFilename : Method {

    #region Properties

    public override List<List<string>> Args => new() { StringVal, BoolVal };

    public override string Description => "Ruft eine Subroutine auf. Diese muss auf der Festplatte im UTF8-Format gespeichert sein.\r\n" +
                                            "Mit KeepVariables kann bestimmt werden, ob die Variablen aus der Subroutine behalten werden sollen.\r\n" +
                                            "Variablen aus der Hauptroutine können in der Subroutine geändert werden und werden zurück gegeben.";

    public override bool EndlessArgs => false;
    public override string EndSequence => ");";
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.IO;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "CallByFilename(Filename, KeepVariables);";

    #endregion

    #region Methods

    /// <summary>
    ///
    /// </summary>
    /// <param name="vs"></param>
    /// <param name="infos"></param>
    /// <param name="aufgerufenVon">Name der Funktion, z.B. Do-Schleife</param>
    /// <param name="reducedscripttext">Der Scripttext, der ausgeführt werden soll. Bereits standardisiert</param>
    /// <param name="keepVariables">Ob Variablen, die in dem Script erzeugt werden, nch außen getragen werden sollen</param>
    /// <param name="lineadd">Zb. bei einer Do Schleife, die Zeile, in der das Do steht. Bei Scripten aus dem Dateisytem 0</param>
    /// <param name="subname">Zb. bei einer Do Schleife, der gleich Wert wie in Infos.Logdata. Bei Scripten aus dem Dateisystem dessen Name</param>
    /// <param name="addMe"></param>
    /// <param name="lm"></param>
    /// <param name="allowedMethods"></param>
    /// <returns></returns>
    public static DoItFeedback CallSub(VariableCollection vs, CanDoFeedback infos, string aufgerufenVon, string reducedscripttext, bool keepVariables, int lineadd, string subname, VariableString? addMe, List<Method> lm, MethodType allowedMethods, bool changevalues, string scriptAttributes) {
        if (keepVariables) {
            if (addMe != null) { vs.Add(addMe); }

            var scx = Script.Parse(reducedscripttext, lineadd, subname, vs, lm, allowedMethods, changevalues, scriptAttributes);
            if (!scx.AllOk) {
                infos.Data.Protocol.AddRange(scx.Protocol);
                return new DoItFeedback(infos.Data, "'" + aufgerufenVon + "' wegen vorherhiger Fehler abgebrochen");
            }
        } else {
            var tmpv = new VariableCollection();
            tmpv.AddRange(vs);
            if (addMe != null) { tmpv.Add(addMe); }

            var scx = Script.Parse(reducedscripttext, lineadd, subname, vs, lm, allowedMethods, changevalues, scriptAttributes);
            if (!scx.AllOk) {
                infos.Data.Protocol.AddRange(scx.Protocol);
                return new DoItFeedback(infos.Data, "'" + aufgerufenVon + "' wegen vorherhiger Fehler abgebrochen");
            }

            vs.Clear();
            vs.AddRange(tmpv);
        }

        return DoItFeedback.Null();
    }

    public override List<string> Comand(VariableCollection? currentvariables) => new() { "callbyfilename" };

    public override DoItFeedback DoIt(VariableCollection vs, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(vs, infos, Args, EndlessArgs);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var vsx = attvar.ValueStringGet(0);
        string f;

        var addp = vs.GetString("AdditionalFilesPfad");

        try {
            if (FileExists(vsx)) {
                f = File.ReadAllText(vsx, Encoding.UTF8);
            } else if (FileExists(addp + vs)) {
                f = File.ReadAllText(addp + vs, Encoding.UTF8);
            } else {
                return new DoItFeedback(infos.Data, "Datei nicht gefunden: " + vsx);
            }
        } catch {
            return new DoItFeedback(infos.Data, "Fehler beim Lesen der Datei: " + vsx);
        }

        (f, string error) = Script.ReduceText(f);

        if (!string.IsNullOrEmpty(error)) {
            return new DoItFeedback(infos.Data, "Fehler in Datei " + vsx + ": " + error);
        }

        var v = CallSub(vs, infos, "Datei-Subroutinen-Aufruf [" + vsx + "]", f, attvar.ValueBoolGet(1), 0, vsx.FileNameWithSuffix(), null, infos.Methods, infos.AllowedMethods, infos.ChangeValues, infos.ScriptAttributes);
        v.BreakFired = false;
        v.EndScript = false;
        return v;
    }

    #endregion
}