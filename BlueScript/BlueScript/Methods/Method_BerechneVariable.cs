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

using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using static BlueBasics.Extensions;

namespace BlueScript.Methods;

internal class Method_BerechneVariable : Method {

    #region Fields

    private static readonly List<List<string>> _args = new() { new List<string> { Variable.Any_Plain } };

    #endregion

    #region Properties

    public static bool SEndlessArgs => false;

    public override List<List<string>> Args => _args;

    public override string Description => "Berechnet eine Variable. Der Typ der Variable und des Ergebnisses müssen übereinstimmen.";
    public override bool EndlessArgs => SEndlessArgs;
    public override string EndSequence => ";";
    public override bool GetCodeBlockAfter => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "=";
    public override string Syntax => "VariablenName = Berechung;";

    #endregion

    #region Methods

    /// <summary>
    ///
    /// </summary>
    /// <param name="newcommand">Erwartet wird: X=5;</param>
    /// <param name="s"></param>
    /// <param name="generateVariable"></param>
    /// <returns></returns>
    public static DoItFeedback VariablenBerechnung(string newcommand, Script s, bool generateVariable) {
        //if (s.BerechneVariable == null) { return new DoItFeedback("Interner Fehler"); }

        var (pos, _) = NextText(newcommand, 0, Gleich, false, false, null);

        if (pos < 1 || pos > newcommand.Length - 2) { return new DoItFeedback("Fehler mit = - Zeichen"); }

        var varnam = newcommand.Substring(0, pos);

        if (!Variable.IsValidName(varnam)) { return new DoItFeedback(varnam + " ist kein gültiger Variablen-Name"); }

        var v = s.Variables.Get(varnam);
        if (generateVariable && v != null) {
            return new DoItFeedback("Variable " + varnam + " ist bereits vorhanden.");
        }
        if (!generateVariable && v == null) {
            return new DoItFeedback("Variable " + varnam + " nicht vorhanden.");
        }

        var value = newcommand.Substring(pos + 1, newcommand.Length - pos - 2);

        var attvar = SplitAttributeToVars(value, s, _args, SEndlessArgs);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(new Method_BerechneVariable(), attvar); }

        if (generateVariable) {
            attvar.Attributes[0].KeyName = varnam.ToLower();
            attvar.Attributes[0].ReadOnly = false;
            s.Variables.Add(attvar.Attributes[0]);
            return new DoItFeedback(attvar.Attributes[0]);
        }

        return v.GetValueFrom(attvar.Attributes[0]);
    }

    public override List<string> Comand(Script? s) => s == null ? new List<string>() : s.Variables.AllNames();

    /// <summary>
    /// Berechnet z.B.   X = 5;
    /// Die Variable, die berechnet werden soll, muss bereits existieren - und wird auch auf Existenz und Datentyp geprüft.
    /// </summary>
    /// <param name="infos"></param>
    /// <param name="s"></param>
    /// <returns></returns>
    public override DoItFeedback DoIt(CanDoFeedback infos, Script s) => VariablenBerechnung(infos.ComandText + infos.AttributText + ";", s, false);

    #endregion
}