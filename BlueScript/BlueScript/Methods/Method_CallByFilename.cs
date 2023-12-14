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

using System.Collections.Generic;
using System.IO;
using System.Text;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.IO;

namespace BlueScript.Methods;

// ReSharper disable once ClassNeverInstantiated.Global
public class Method_CallByFilename : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, BoolVal];

    public override string Command => "callbyfilename";

    public override string Description => "Ruft eine Subroutine auf. Diese muss auf der Festplatte im UTF8-Format gespeichert sein.\r\n" +
                                                "Mit KeepVariables kann bestimmt werden, ob die Variablen aus der Subroutine behalten werden sollen.\r\n" +
                                            "Variablen aus der Hauptroutine können in der Subroutine geändert werden und werden zurück gegeben.";

    public override bool EndlessArgs => false;

    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.IO;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "CallByFilename(Filename, KeepVariables);";

    #endregion

    #region Methods

    /// <summary>
    ///
    /// </summary>
    /// <param name="infos"></param>
    /// <param name="aufgerufenVon">Name der Funktion, z.B. Do-Schleife</param>
    /// <param name="reducedscripttext">Der Scripttext, der ausgeführt werden soll. Bereits standardisiert</param>
    /// <param name="keepVariables">Ob Variablen, die in dem Script erzeugt werden, nch außen getragen werden sollen</param>
    /// <param name="lineadd">Zb. bei einer Do Schleife, die Zeile, in der das Do steht. Bei Scripten aus dem Dateisytem 0</param>
    /// <param name="subname">Zb. bei einer Do Schleife, der gleich Wert wie in Infos.Logdata. Bei Scripten aus dem Dateisystem dessen Name</param>
    /// <param name="addMe"></param>
    /// <param name="varCol"></param>
    /// <returns></returns>
    public static DoItFeedback CallSub(VariableCollection varCol, ScriptProperties scp, CanDoFeedback infos, string aufgerufenVon, string reducedscripttext, bool keepVariables, int lineadd, string subname, VariableString? addMe) {
        ScriptEndedFeedback scx;

        if (keepVariables) {
            if (addMe != null) { varCol.Add(addMe); }
            scx = Script.Parse(varCol, scp, reducedscripttext, lineadd, subname);
        } else {
            var tmpv = new VariableCollection();
            tmpv.AddRange(varCol);
            if (addMe != null) { tmpv.Add(addMe); }

            scx = Script.Parse(tmpv, scp, reducedscripttext, lineadd, subname);

            if (!scx.AllOk) {
                // Beim Abbruch sollen die aktuellen Variabeln angezeigt werden
                varCol.Clear();
                varCol.AddRange(tmpv);
            }
        }

        if (!scx.AllOk) {
            infos.Data.Protocol.AddRange(scx.Protocol);
            return new DoItFeedback(infos.Data, "'" + aufgerufenVon + "' wegen vorherhiger Fehler abgebrochen");
        }

        return new DoItFeedback(scx.BreakFired, scx.EndScript);
    }

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, EndlessArgs, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var vs = attvar.ValueStringGet(0);
        string f;

        var addp = varCol.GetString("AdditionalFilesPfad");

        try {
            if (FileExists(vs)) {
                f = File.ReadAllText(vs, Encoding.UTF8);
            } else if (FileExists(addp + vs)) {
                f = File.ReadAllText(addp + vs, Encoding.UTF8);
            } else {
                return new DoItFeedback(infos.Data, "Datei nicht gefunden: " + vs);
            }
        } catch {
            return new DoItFeedback(infos.Data, "Fehler beim Lesen der Datei: " + vs);
        }

        (f, var error) = Script.ReduceText(f);

        if (!string.IsNullOrEmpty(error)) {
            return new DoItFeedback(infos.Data, "Fehler in Datei " + vs + ": " + error);
        }

        var scx = CallSub(varCol, scp, infos, "Datei-Subroutinen-Aufruf [" + vs + "]", f, attvar.ValueBoolGet(1), 0, vs.FileNameWithSuffix(), null);
        if (!scx.AllOk) { return scx; }
        return DoItFeedback.Null(); // Aus der Subroutine heraus dürden keine Breaks/Return erhalten bleiben
    }

    #endregion
}