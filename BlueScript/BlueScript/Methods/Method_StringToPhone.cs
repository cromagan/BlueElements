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

namespace BlueScript.Methods {

    internal class Method_StringToPhone : Method {

        #region Properties

        public override List<VariableDataType> Args => new() { VariableDataType.String_or_List };
        public override string Description => "Versucht, den String als internationale Telefonnummer zurückzugeben. Schlägt es fehl, wird nicht zurückgegeben.";
        public override bool EndlessArgs => false;
        public override string EndSequence => ")";
        public override bool GetCodeBlockAfter => false;
        public override VariableDataType Returns => VariableDataType.List;
        public override string StartSequence => "(";
        public override string Syntax => "StringToPhone(String)";

        #endregion

        #region Methods

        public override List<string> Comand(Script? s) => new() { "stringtophone" };

        public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar); }
            //var ts3 = attvar.Attributes[0].ValueListString;
            //for (var tz = 0; tz < ts3.Count; tz++) {
            //    ts3[tz] = CellItem.ValueReadable(null, ts3[tz], enShortenStyle.HTML, enBildTextVerhalten.Nur_Text, true);
            //}
            attvar.Attributes[0].Readonly = false;
            attvar.Attributes[0].Type = VariableDataType.Variable_List;
            //attvar.Attributes[0].ValueListString = ts3;
            //return new DoItFeedback(attvar.Attributes[0].ValueForReplace, string.Empty);
            return new DoItFeedback("Nicht fertig implementiert");
        }

        #endregion
    }
}