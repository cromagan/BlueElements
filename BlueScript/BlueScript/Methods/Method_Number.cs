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

using System.Collections.Generic;
using BlueBasics;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript.Methods;

internal class Method_Number : Method {

    #region Properties

    public override List<List<string>> Args => new() { new() { VariableString.ShortName_Plain }, new() { VariableFloat.ShortName_Plain } };
    public override string Description => "Gibt den Text als Zahl zurück. Fall dies keine gültige Zahl ist, wird NaN-Value zurückgegeben.";
    public override bool EndlessArgs => false;
    public override string EndSequence => ")";
    public override bool GetCodeBlockAfter => false;
    public override string Returns => VariableFloat.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "Number(string, NaNValue)";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "number" };

    public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
        var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar); }

        if (attvar.Attributes[0] is VariableFloat vf) { return new DoItFeedback(vf.ValueNum); }

        if (attvar.Attributes[0] is VariableString vs) {
            if (Converter.DoubleTryParse(vs.ValueString, out var dbl)) {
                return new DoItFeedback(dbl);
            }
            //return new DoItFeedback("'" + vs.ValueString + "' kann nicht als Zahl interpretiert werden.");
        }

        return new DoItFeedback(attvar.Attributes[1]);
    }

    #endregion
}