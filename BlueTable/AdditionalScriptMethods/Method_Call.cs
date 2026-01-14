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
using BlueScript;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueTable.AdditionalScriptMethods;

internal class Method_Call : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];

    public override string Command => "call";
    public override List<string> Constants => [];

    public override string Description => "Ruft eine Subroutine auf.\r\n" +
        "Variablen aus der Hauptroutine können in der Subroutine geändert werden und werden zurück gegeben.";

    public override bool GetCodeBlockAfter => false;

    public override int LastArgMinCount => 0;

    public override MethodType MethodLevel => MethodType.Standard;

    public override bool MustUseReturnValue => false;

    public override string Returns => VariableString.ShortName_Plain;

    public override string StartSequence => "(";

    public override string Syntax => "Call(SubName, Attribut0, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (MyTable(scp) is not { IsDisposed: false } myTb) { return DoItFeedback.InternerFehler(ld); }

        var vs = attvar.ValueStringGet(0);

        var sc = myTb.EventScript.GetByKey(vs);
        if (sc == null) { return new DoItFeedback("Skript nicht vorhanden: " + vs, true, ld); }

        var newat = sc.Attributes();
        foreach (var thisAt in scp.ScriptAttributes) {
            if (!newat.Contains(thisAt)) {
                return new DoItFeedback("Aufzurufendes Skript hat andere Bedingungen, " + thisAt + " fehlt.", true, ld);
            }
        }

        var (f, error) = Script.NormalizedText(sc.Script);

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

        var scx = Method_CallByFilename.CallSub(varCol, scp, f, 0, vs, null, a, vs, ld);
        scx.ConsumeBreakAndReturn();// Aus der Subroutine heraus dürden keine Breaks/Return erhalten bleiben
        if (scx.NeedsScriptFix) {
            Table.UpdateScript(sc, failedReason: scx.ProtocolText);
            return new DoItFeedback($"Unterskript '{sc.KeyName}' hat Fehler verursacht.", false, ld);
        }
        return scx;
    }

    #endregion
}