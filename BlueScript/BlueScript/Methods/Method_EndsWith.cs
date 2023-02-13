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
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript.Methods;

internal class Method_EndsWith : Method {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableString.ShortName_Plain }, new List<string> { VariableBool.ShortName_Plain }, new List<string> { VariableString.ShortName_Plain } };
    public override string Description => "Prüft, ob der String mit einem der angegeben Strings endet.";
    public override bool EndlessArgs => true;
    public override string EndSequence => ")";
    public override bool GetCodeBlockAfter => false;
    public override string Returns => VariableBool.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "EndsWith(String, CaseSensitive, Value1, Value2, ...)";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "endswith" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(s, infos, this, attvar); }
        for (var z = 2; z < attvar.Attributes.Count; z++) {
            if (((VariableBool)attvar.Attributes[1]).ValueBool) {
                if (((VariableString)attvar.Attributes[0]).ValueString.EndsWith(((VariableString)attvar.Attributes[z]).ValueString)) {
                    return DoItFeedback.Wahr(s, infos);
                }
            } else {
                if (((VariableString)attvar.Attributes[0]).ValueString.ToLower().EndsWith(((VariableString)attvar.Attributes[z]).ValueString.ToLower())) {
                    return DoItFeedback.Wahr(s, infos);
                }
            }
        }
        return DoItFeedback.Falsch(s, infos);
    }

    #endregion
}