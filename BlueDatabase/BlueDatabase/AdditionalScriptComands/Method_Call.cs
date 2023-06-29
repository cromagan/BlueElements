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
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueDatabase.AdditionalScriptComands;

internal class Method_Call : Method_Database {

    #region Properties

    public override List<List<string>> Args => new() { StringVal, BoolVal };

    public override string Description => "Ruft eine Subroutine auf.\r\n" +
        "Mit KeepVariables kann bestimmt werden, ob die Variablen aus der Subroutine behalten werden sollen.\r\n" +
        "Variablen aus der Hauptroutine können in der Subroutine geändert werden und werden zurück gegeben.";

    public override bool EndlessArgs => false;
    public override string EndSequence => ");";
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.Database;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "Call(SubName, KeepVariables);";

    #endregion

    #region Methods

    public override List<string> Comand(VariableCollection? currentvariables) => new() { "call" };

    public override DoItFeedback DoIt(VariableCollection vs, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(vs, infos, Args, EndlessArgs);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var vsx = attvar.ValueStringGet(0);

        var db = MyDatabase(vs);
        if (db == null) { return new DoItFeedback(infos.Data, "Datenbankfehler!"); }

        var sc = db.EventScript.Get(vsx);
        if (sc == null) { return new DoItFeedback(infos.Data, "Skript nicht vorhanden: " + vs); }

        if (sc.Attributes() != infos.ScriptProperties.ScriptAttributes && infos.ScriptProperties.ScriptAttributes != "*") {
            return new DoItFeedback(infos.Data, "Aufzurufendes Skript hat andere Bedingungen.");
        }

        (string f, string error) = Script.ReduceText(sc.Script);

        if (!string.IsNullOrEmpty(error)) {
            return new DoItFeedback(infos.Data, "Fehler in Unter-Skript " + vs + ": " + error);
        }

        var scx = BlueScript.Methods.Method_CallByFilename.CallSub(vs, infos, "Subroutinen-Aufruf [" + vs + "]", f, attvar.ValueBoolGet(1), 0, vsx, null, infos.ScriptProperties);
        scx.BreakFired = false;
        scx.EndScript = false;

        return scx;
    }

    #endregion
}