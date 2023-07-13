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
using System.ComponentModel;
using BlueBasics;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_If : Method {

    #region Fields

    public static readonly List<string> OderOder = new() { "||" };
    public static readonly List<string> UndUnd = new() { "&&" };

    /// <summary>
    /// Vergleichsopeatoren in der richtigen Rang-Reihenfolge
    /// https://de.wikipedia.org/wiki/Operatorrangfolge
    /// </summary>
    public static readonly List<string> VergleichsOperatoren = new() { "==", "!=", ">=", "<=", "<", ">", "!", "&&", "||" };

    #endregion

    #region Properties

    public override List<List<string>> Args => new() { BoolVal };
    public override string Description => "Nur wenn der Wert in der Klammer TRUE ist, wird der nachfolgende Codeblock ausgeführt. Es werden IMMER alle Vergleichsoperatoren aufgelöst. Deswegen sind Verschachtelungen mit Voricht zu verwenden - z.B. mir einem Exists-Befehl.";
    public override bool EndlessArgs => false;
    public override string EndSequence => ")";
    public override bool GetCodeBlockAfter => true;
    public override MethodType MethodType => MethodType.Standard;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "if (true) { Code zum Ausführen }";

    #endregion

    #region Methods

    public static bool? GetBool(string txt) {
        txt = txt.DeKlammere(true, false, false, true);

        //            if (txt.Value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
        //                txt.Value.Equals("false", StringComparison.OrdinalIgnoreCase)) {
        //                if (Type is not VariableDataType.NotDefinedYet and not VariableDataType.Bool) { SetError("Variable ist kein Boolean"); return; }
        //                ValueString = txt.Value;
        //                Type = VariableDataType.Bool;
        //                Readonly = true;
        //                return;
        //            }

        switch (txt.ToLower()) {
            case "!false":
            case "true":
                return true;

            case "!true":
            case "false":
                return false;
        }

        return null;
    }

    public override List<string> Comand(VariableCollection? currentvariables) => new() { "if" };

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, EndlessArgs, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return new DoItFeedback(infos.Data, "Fehler innerhalb der runden Klammern des If-Befehls"); }

        if (attvar.ValueBoolGet(0)) {
            var scx = Method_CallByFilename.CallSub(varCol, scp, infos, "If-Befehl-Inhalt", infos.CodeBlockAfterText, false, infos.Data.Line - 1, infos.Data.Subname, null);
            if(!scx.AllOk) { return scx; }
            return new DoItFeedback(scx.BreakFired, scx.EndScript); // If muss die Breaks und Endsripts erhalten!
        }

        return DoItFeedback.Null();
    }

    #endregion
}