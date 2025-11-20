// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;


internal class Method_EndsWith : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, BoolVal, StringVal];
    public override string Command => "endswith";
    public override List<string> Constants => [];
    public override string Description => "Prüft, ob der String mit einem der angegeben Strings endet.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBool.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "EndsWith(String, CaseSensitive, Value1, Value2, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        for (var z = 2; z < attvar.Attributes.Count; z++) {
            if (attvar.ValueBoolGet(1)) {
                if (attvar.ValueStringGet(0).EndsWith(attvar.ValueStringGet(z))) {
                    return DoItFeedback.Wahr();
                }
            } else {
                if (attvar.ValueStringGet(0).ToLowerInvariant().EndsWith(attvar.ValueStringGet(z).ToLowerInvariant())) {
                    return DoItFeedback.Wahr();
                }
            }
        }
        return DoItFeedback.Falsch();
    }

    #endregion
}