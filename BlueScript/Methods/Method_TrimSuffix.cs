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

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;


internal class Method_TrimSuffix : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "trimsuffix";
    public override List<string> Constants => [];
    public override string Description => "Entfernt die angegebenen Suffixe und evtl. übrige Leerzeichen. Die Suffixe werden nur entfernt, wenn vor dem Suffix ein Leerzeichen oder eine Zahl ist. Groß- und Kleinschreibung wird ignoriert.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "TrimSuffix(string, suffix, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var val = attvar.ValueStringGet(0);

        const string tmp = BlueBasics.ClassesStatic.Constants.Char_Numerals + " ";

        for (var z = 1; z < attvar.Attributes.Count; z++) {
            var suf = attvar.ValueStringGet(z).ToLowerInvariant();
            if (val.Length <= suf.Length) {
                continue;
            }

            if (val.ToLowerInvariant().EndsWith(suf)) {
                var c = val.Substring(val.Length - suf.Length - 1, 1);
                if (tmp.Contains(c)) {
                    return new DoItFeedback(val.Substring(0, val.Length - suf.Length).TrimEnd(' '));
                }
            }
        }

        return new DoItFeedback(val);
    }

    #endregion
}