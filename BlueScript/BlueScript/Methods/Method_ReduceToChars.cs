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
using BlueScript.Structures;
using BlueScript.Enums;
using static BlueBasics.Extensions;

namespace BlueScript.Methods {

    internal class Method_ReduceToChars : Method {

        #region Properties

        public override List<VariableDataType> Args => new() { VariableDataType.String, VariableDataType.String };
        public override string Description => "Entfernt aus dem Text alle Zeichen die nicht erlaubt sind";
        public override bool EndlessArgs => false;
        public override string EndSequence => ")";
        public override bool GetCodeBlockAfter => false;
        public override VariableDataType Returns => VariableDataType.String;
        public override string StartSequence => "(";
        public override string Syntax => "ReduceToChars(OriginalString, ErlaubteZeichenString)";

        #endregion

        #region Methods

        public override List<string> Comand(Script? s) => new() { "reducetochars" };

        public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            return !string.IsNullOrEmpty(attvar.ErrorMessage) ? DoItFeedback.AttributFehler(this, attvar)
                                                             : new DoItFeedback(attvar.Attributes[0].ValueString.ReduceToChars(attvar.Attributes[1].ValueString), VariableDataType.String);
        }

        #endregion
    }
}