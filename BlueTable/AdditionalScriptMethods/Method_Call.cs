// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics;
using BlueScript.Classes;
using BlueScript.Methods;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Diagnostics;

namespace BlueTable.AdditionalScriptMethods;

internal sealed class Method_Call : Method_TableGeneric {

    #region Properties

    public static List<List<string>> Args => [StringVal, StringVal];

    public static string Command => "call";
    public static List<string> Constants => [];

    public static string Description => "Ruft eine Subroutine auf.\r\n" +
        "Variablen aus der Hauptroutine können in der Subroutine geändert werden und werden zurück gegeben.";



    public static int LastArgMinCount => 0;





    public static string Returns => VariableString.ShortName_Plain;

    public static string StartSequence => "(";

    public static string Syntax => "Call(SubName, Attribut0, ...);";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (MyTable(scp) is not { IsDisposed: false } myTb) { return DoItFeedback.InternerFehler(ld); }

        var vs = attvar.ValueStringGet(0);

        var script = myTb.EventScript.GetByKey(vs);
        if (script == null) { return new DoItFeedback("Skript nicht vorhanden: " + vs, true, ld); }

        var newat = script.Attributes();
        foreach (var thisAt in scp.ScriptAttributes) {
            if (!newat.Contains(thisAt)) {
                return new DoItFeedback("Aufzurufendes Skript hat andere Bedingungen, " + thisAt + " fehlt.", true, ld);
            }
        }

        var (f, error) = Script.NormalizedText(script.Script);

        if (!string.IsNullOrEmpty(error)) {
            return new DoItFeedback("Fehler in Unter-Skript " + vs + ": " + error, true, ld);
        }

        #region Attributliste erzeugen

        var a = new List<string>();
        for (var z = 1; z < attvar.Attributes.Count; z++) {
            if (attvar.Attributes[z] is VariableString vs1) { a.Add(vs1.ValueString); }
        }

        #endregion

        //Diese Routine kann nicht benutzt werden, weil sie die Zeilenvariableen neu erstellt
        //        var scx = myDb.ExecuteScript(null, vs, scp.ProduktivPhase, null, a, true, true);

        var sw = Stopwatch.StartNew();

        var scx = Method_CallByFilename.CallSub(varCol, scp, f, 0, vs, null, a, vs, ld);
        myTb.UpdateScript(script, scx, sw, null, scx.Variables?.GetBoolean("Extended") ?? false, scp.ProduktivPhase, !scp.ProduktivPhase);
        scx.ConsumeBreakAndReturn();// Aus der Subroutine heraus dürden keine Breaks/Return erhalten bleiben
        if (scx.NeedsScriptFix) {
            return new DoItFeedback($"Unterskript '{script.KeyName}' hat Fehler verursacht.", false, ld);
        }
        return scx;
    }

    #endregion
}