// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static BlueBasics.IO;

namespace BlueScript.Methods;

// ReSharper disable once ClassNeverInstantiated.Global
public class Method_CallByFilename : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, BoolVal, StringVal];
    public override string Command => "callbyfilename";
    public override List<string> Constants => [];

    public override string Description => "Ruft eine Subroutine auf. Diese muss auf der Festplatte im UTF8-Format gespeichert sein.\r\n" +
                                                "Mit KeepVariables kann bestimmt werden, ob die Variablen aus der Subroutine behalten werden sollen.\r\n" +
                                            "Variablen aus der Hauptroutine können in der Subroutine geändert werden und werden zurück gegeben.";

    public override bool GetCodeBlockAfter => false;

    public override int LastArgMinCount => 0;

    public override MethodType MethodType => MethodType.Standard;

    public override bool MustUseReturnValue => false;

    public override string Returns => string.Empty;

    public override string StartSequence => "(";

    public override string Syntax => "CallByFilename(Filename, KeepVariables, Attribute0, ...);";

    #endregion

    #region Methods

    /// <summary>
    ///
    /// </summary>
    /// <param name="scp"></param>
    /// <param name="ld"></param>
    /// <param name="aufgerufenVon">Name der Funktion, z.B. Do-Schleife</param>
    /// <param name="reducedscripttext">Der Scripttext, der ausgeführt werden soll. Bereits standardisiert</param>
    /// <param name="keepVariables">Ob Variablen, die in dem Script erzeugt werden, nch außen getragen werden sollen</param>
    /// <param name="lineadd">Zb. bei einer Do Schleife, die Zeile, in der das Do steht. Bei Scripten aus dem Dateisytem 0</param>
    /// <param name="subname">Zb. bei einer Do Schleife, der gleich Wert wie in Infos.Logdata. Bei Scripten aus dem Dateisystem dessen Name</param>
    /// <param name="addMe"></param>
    /// <param name="varCol"></param>
    /// <param name="attributes"></param>
    /// <returns></returns>
    public static DoItFeedback CallSub(VariableCollection varCol, ScriptProperties scp, LogData ld, string aufgerufenVon, string reducedscripttext, bool keepVariables, int lineadd, string subname, Variable? addMe, List<string>? attributes, string chainlog) {
        ScriptEndedFeedback scx;

        if (scp.Stufe > 10) {
            return new DoItFeedback("'" + subname + "' wird zu verschachtelt aufgerufen.", true, ld);
        }

        var scp2 = new ScriptProperties(scp, scp.AllowedMethods, scp.Stufe + 1, $"{scp.Chain}\\[{lineadd + 1}] {chainlog}");

        //Develop.MonitorMessage?.Invoke(subname, "Skript", "Skript: " + scp.Chain, scp.Stufe);

        if (keepVariables) {
            if (addMe != null) { _ = varCol.Add(addMe); }
            scx = Script.Parse(varCol, scp2, reducedscripttext, lineadd, subname, attributes);
        } else {
            var tmpv = new VariableCollection();
            _ = tmpv.AddRange(varCol);
            if (addMe != null) { _ = tmpv.Add(addMe); }

            scx = Script.Parse(tmpv, scp2, reducedscripttext, lineadd, subname, attributes);

            #region Kritische Variablen Disposen

            foreach (var thisVar in tmpv) {
                if (varCol.Get(thisVar.KeyName) == null) {
                    thisVar.DisposeContent();
                }
            }

            #endregion

            if (scx.Failed) {
                // Beim Abbruch sollen die aktuellen Variabeln angezeigt werden
                varCol.Clear();
                _ = varCol.AddRange(tmpv);
            }
        }

        if (scx.Failed) {
            ld.Protocol.AddRange(scx.Protocol);
            return new DoItFeedback("'" + aufgerufenVon + "' wegen vorheriger Fehler abgebrochen", true, ld);
        }

        return new DoItFeedback(scx.BreakFired, scx.EndScript);
    }

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var vs = attvar.ValueStringGet(0);
        string f;

        var addp = varCol.GetString("AdditionalFilesPfad");

        try {
            if (FileExists(vs)) {
                f = File.ReadAllText(vs, Encoding.UTF8);
            } else if (FileExists(addp + vs)) {
                f = File.ReadAllText(addp + vs, Encoding.UTF8);
            } else {
                return new DoItFeedback("Datei nicht gefunden: " + vs, true, ld);
            }
        } catch {
            return new DoItFeedback("Fehler beim Lesen der Datei: " + vs, true, ld);
        }

        (f, var error) = Script.ReduceText(f);

        if (!string.IsNullOrEmpty(error)) {
            return new DoItFeedback("Fehler in Datei " + vs + ": " + error, true, ld);
        }

        #region Attributliste erzeugen

        var a = new List<string>();
        for (var z = 2; z < attvar.Attributes.Count; z++) {
            if (attvar.Attributes[z] is VariableString vs1) { a.Add(vs1.ValueString); }
        }

        #endregion

        var scx = CallSub(varCol, scp, ld, "Datei-Subroutinen-Aufruf [" + vs + "]", f, attvar.ValueBoolGet(1), 0, vs.FileNameWithSuffix(), null, a, vs);
        if (scx.Failed) { return scx; }
        return DoItFeedback.Null(); // Aus der Subroutine heraus dürden keine Breaks/Return erhalten bleiben
    }

    #endregion
}