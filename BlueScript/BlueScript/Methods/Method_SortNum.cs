// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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

using BlueBasics;
using Skript.Enums;
using System.Collections.Generic;
using static BlueBasics.Extensions;

namespace BlueScript {

    internal class Method_SortNum : Method {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.Variable_List, enVariableDataType.Numeral };

        public override string Description => "Sortiert die Liste. Der Zahlenwert wird verwendet wenn der String nicht in eine Zahl umgewandelt werden kann.";

        public override bool EndlessArgs => false;

        public override string EndSequence => ");";

        public override bool GetCodeBlockAfter => false;

        public override enVariableDataType Returns => enVariableDataType.Null;

        public override string StartSequence => "(";

        public override string Syntax => "SortNum(ListVariable, Defaultwert);";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "sortnum" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }

            var nums = new List<double>();
            foreach (var txt in attvar.Attributes[0].ValueListString) {
                if (txt.IsNumeral()) {
                    nums.Add(double.Parse(txt));
                } else {
                    nums.Add(attvar.Attributes[1].ValueDouble);
                }
            }

            nums.Sort();

            attvar.Attributes[0].ValueListString = nums.ConvertAll<string>(delegate (double i) { return i.ToString(); });
            return new strDoItFeedback();
        }

        #endregion
    }
}