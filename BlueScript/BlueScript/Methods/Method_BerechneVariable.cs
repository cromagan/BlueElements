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
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.Extensions;

namespace BlueScript.Methods;

internal class Method_BerechneVariable : Method {

    #region Fields

    private static readonly List<List<string>> Sargs = new() { new List<string> { Variable.Any_Plain } };

    #endregion

    #region Properties

    public static bool SEndlessArgs => false;

    public override List<List<string>> Args => Sargs;

    public override string Description => "Berechnet eine Variable. Der Typ der Variable und des Ergebnisses müssen übereinstimmen.";
    public override bool EndlessArgs => SEndlessArgs;
    public override string EndSequence => ";";
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.Standard;
    public override string Returns => string.Empty;
    public override string StartSequence => "=";
    public override string Syntax => "VariablenName = Berechung;";

    #endregion

    #region Methods

    /// <summary>
    ///
    /// </summary>
    /// <param name="infos"></param>
    /// <param name="scp"></param>
    /// <param name="newcommand">Erwartet wird: X=5;</param>
    /// <param name="varCol"></param>
    /// <param name="generateVariable"></param>
    /// <returns></returns>
    public static DoItFeedback VariablenBerechnung(CanDoFeedback infos, ScriptProperties scp, string newcommand, VariableCollection varCol, bool generateVariable) {
        //if (s.BerechneVariable == null) { return new DoItFeedback(infos.LogData, s, "Interner Fehler"); }

        var (pos, _) = NextText(newcommand, 0, Gleich, false, false, null);

        if (pos < 1 || pos > newcommand.Length - 2) { return new DoItFeedback(infos.Data, "Fehler mit = - Zeichen"); }

        var varnam = newcommand.Substring(0, pos);

        if (!Variable.IsValidName(varnam)) { return new DoItFeedback(infos.Data, varnam + " ist kein gültiger Variablen-Name"); }

        var vari = varCol.Get(varnam);
        if (generateVariable && vari != null) {
            return new DoItFeedback(infos.Data, "Variable " + varnam + " ist bereits vorhanden.");
        }
        if (!generateVariable && vari == null) {
            return new DoItFeedback(infos.Data, "Variable " + varnam + " nicht vorhanden.");
        }

        var value = newcommand.Substring(pos + 1, newcommand.Length - pos - 2);
        var attvar = SplitAttributeToVars(varCol, value, Sargs, SEndlessArgs, infos.Data, scp);

        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, new Method_BerechneVariable(), attvar); }

        if (attvar.Attributes[0] is VariableUnknown) {
            return DoItFeedback.AttributFehler(infos.Data, new Method_BerechneVariable(), attvar);
        }

        if (attvar.Attributes[0] is Variable v) {
            if (generateVariable) {
                v.KeyName = varnam.ToLower();
                v.ReadOnly = false;
                varCol.Add(v);
                return new DoItFeedback(v);
            }

            if (vari == null) {
                // es sollte generateVariable greifen, und hier gar nimmer ankommen. Aber um die IDE zu befriedigen
                return new DoItFeedback(infos.Data, "Interner Fehler");
            }

            return vari.GetValueFrom(v, infos.Data);
        }
        // attvar.Attributes[0] müsste immer eine Variable sein...
        return new DoItFeedback(infos.Data, "Interner Fehler");
    }

    public override List<string> Comand(VariableCollection? currentvariables) => currentvariables?.AllNames() ?? new();

    /// <summary>
    /// Berechnet z.B.   X = 5;
    /// Die Variable, die berechnet werden soll, muss bereits existieren - und wird auch auf Existenz und Datentyp geprüft.
    /// </summary>
    /// <param name="varCol"></param>
    /// <param name="infos"></param>
    /// <returns></returns>
    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) => VariablenBerechnung(infos, scp, infos.ComandText + infos.AttributText + ";", varCol, false);

    #endregion
}