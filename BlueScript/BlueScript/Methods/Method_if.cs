// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript.Methods;

internal class Method_if : Method {

    #region Fields

    public static readonly List<string> OderOder = new() { "||" };

    public static readonly List<string> UndUnd = new() { "&&" };

    /// <summary>
    /// Vergleichsopeatoren in der richtigen Rang-Reihenfolge
    // https://de.wikipedia.org/wiki/Operatorrangfolge
    /// </summary>
    public static readonly List<string> VergleichsOperatoren = new() { "==", "!=", ">=", "<=", "<", ">", "!", "&&", "||" };

    #endregion

    #region Properties

    public override List<List<string>> Args => new() { new() { VariableBool.ShortName_Plain } };
    public override string Description => "Nur wenn der Wert in der Klammer TRUE ist, wird der nachfolgende Codeblock ausgeführt. Es werden IMMER alle Vergleichsoperatoren aufgelöst. Deswegen sind Verschachtelungen mit Voricht zu verwenden - z.B. mir einem Exists-Befehl.";
    public override bool EndlessArgs => false;
    public override string EndSequence => ")";
    public override bool GetCodeBlockAfter => true;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "if (true) { Code zum Ausführen }";

    #endregion

    #region Methods

    public static bool? GetBool(string txt) {
        txt = txt.DeKlammere(true, false, false, true);

        //            if (txt.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase) ||
        //                txt.Value.Equals("false", StringComparison.InvariantCultureIgnoreCase)) {
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

    public override List<string> Comand(Script? s) => new() { "if" };

    public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
        var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar); }
        if (((VariableBool)attvar.Attributes[0]).ValueBool) {
            var (err, _) = s.Parse(infos.CodeBlockAfterText);
            if (!string.IsNullOrEmpty(err)) { return new DoItFeedback(err); }
        } else {
            s.Line += infos.LineBreakInCodeBlock;
        }
        return new DoItFeedback(string.Empty);
    }

    #endregion
}