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

#nullable enable

using BlueBasics;
using BlueScript;
using BlueScript.Enums;
using BlueScript.EventArgs;
using BlueScript.Interfaces;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
internal class Method_Call : Method_Database, IUseableForButton {

    #region Properties

    public override List<List<string>> Args => [StringVal, BoolVal, StringVal];

    public List<List<string>> ArgsForButton => [StringVal, StringVal];

    public List<string> ArgsForButtonDescription => ["Auszuführendes Skript", "Zusätzliches Attribut"];

    public ButtonArgs ClickableWhen => ButtonArgs.Egal;

    public override string Command => "call";

    public override string Description => "Ruft eine Subroutine auf.\r\n" +
            "Mit KeepVariables kann bestimmt werden, ob die Variablen aus der Subroutine behalten werden sollen.\r\n" +
        "Variablen aus der Hauptroutine können in der Subroutine geändert werden und werden zurück gegeben.";

    public override bool GetCodeBlockAfter => false;

    public override int LastArgMinCount => 0;

    public override MethodType MethodType => MethodType.Database | MethodType.SpecialVariables;

    public override bool MustUseReturnValue => false;

    public string NiceTextForUser => "Ein Skript aus dieser Datenbank ausführen";

    public override string Returns => string.Empty;

    public override string StartSequence => "(";

    public override string Syntax => "Call(SubName, KeepVariables, Attribut0, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, LastArgMinCount, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var vs = attvar.ValueStringGet(0);

        var db = MyDatabase(scp);
        if (db == null) { return new DoItFeedback(infos.Data, "Datenbankfehler!"); }

        var sc = db.EventScript.Get(vs);
        if (sc == null) { return new DoItFeedback(infos.Data, "Skript nicht vorhanden: " + vs); }

        var newat = sc.Attributes();
        foreach (var thisAt in scp.ScriptAttributes) {
            if (!newat.Contains(thisAt)) {
                return new DoItFeedback(infos.Data, "Aufzurufendes Skript hat andere Bedingungen.");
            }
        }

        var (f, error) = Script.ReduceText(sc.ScriptText);

        if (!string.IsNullOrEmpty(error)) {
            return new DoItFeedback(infos.Data, "Fehler in Unter-Skript " + vs + ": " + error);
        }

        #region Attributliste erzeugen

        var a = new List<string>();
        for (var z = 2; z < attvar.Attributes.Count; z++) {
            if (attvar.Attributes[z] is VariableString vs1) { a.Add(vs1.ValueString); }
        }

        #endregion

        var scx = Method_CallByFilename.CallSub(varCol, scp, infos, "Subroutinen-Aufruf [" + vs + "]", f, attvar.ValueBoolGet(1), 0, vs, null, a);
        if (!scx.AllOk) { return scx; }
        return DoItFeedback.Null(); // Aus der Subroutine heraus dürden keine Breaks/Return erhalten bleiben
    }

    public string TranslateButtonArgs(string arg1, string arg2, string arg3, string arg4, string filterarg, string rowarg) => arg1 + "," + arg2;

    #endregion
}