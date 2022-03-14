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

using BlueScript.Enums;
using BlueScript.Structures;
using System.Collections.Generic;

using System.Linq;
using static BlueBasics.Extensions;

using BlueScript.Methods;

namespace BlueScript.Variables {

    public class VariableSub : Variable {

        #region Constructors

        public VariableSub(string name, string subName, int subOnLine, string subCode, bool system, string coment) : base(name, true, system, coment) {
            SubName = subName.RestoreCriticalVariableChars();
            SubOnLine = subOnLine;
            SubCode = subCode;
        }

        /// <summary>
        /// Wichtig für: GetEnumerableOfType<Variable>("NAME");
        /// </summary>
        /// <param name="name"></param>
        public VariableSub(string value) : this(DummyName(), value, 0, string.Empty, false, string.Empty) { }

        public VariableSub(string name, string subName, int subOnLine, string subCode) : this(name, subName, subOnLine, subCode, false, string.Empty) { }

        #endregion

        #region Properties

        public override int CheckOrder => 4;
        public override bool GetFromStringPossible => true;
        public override bool IsNullOrEmpty => string.IsNullOrEmpty(SubName);

        /// <summary>
        /// Gleichgesetzt mit ValueString
        /// </summary>
        public override string ReadableText => SubName;

        public override string ShortName => "sub";
        public string SubCode { get; private set; } = string.Empty;
        public string SubName { get; private set; } = string.Empty;
        public int SubOnLine { get; private set; } = 0;
        public override bool ToStringPossible => false;
        public override VariableDataType Type => VariableDataType.Sub;
        public override string ValueForReplace => SubName.RemoveCriticalVariableChars();

        #endregion

        #region Methods

        public override DoItFeedback GetValueFrom(Variable variable) {
            if (variable is not VariableSub v) { return DoItFeedback.VerschiedeneTypen(this, variable); }
            if (Readonly) { return DoItFeedback.Schreibgschützt(); }
            SubName = v.SubName;
            return DoItFeedback.Null();
        }

        protected override bool TryParse(string txt, out Variable? succesVar, Script s) {
            succesVar = null;

            if (string.IsNullOrEmpty(txt)) { return false; }

            if (!Variable.IsValidName(txt)) { return false; }

            var such = new List<string> { "sub" + txt.ToLower() + "()" };

            var (pos, _) = NextText(s.ReducedScriptText.ToLower(), 0, such, true, false, KlammernStd);

            if (pos < 0) { return false; }

            var (pos2, _) = NextText(s.ReducedScriptText.ToLower(), pos + 1, such, true, false, KlammernStd);
            if (pos2 > 0) { return false; }//return new DoItFeedback("Subroutine " + infos.AttributText + " mehrfach definert.");
            var weiterLine = s.Line;

            var (item1, item2) = Method.GetCodeBlockText(s.ReducedScriptText, pos + such[0].Length);

            if (!string.IsNullOrEmpty(item2)) { return false; }//new DoItFeedback("Subroutine " + infos.AttributText + ": " + item2); }

            var subOnLine = s.ReducedScriptText.Substring(0, pos).Count(c => c == '¶') + 1;

            succesVar = new VariableSub(Variable.DummyName(), txt, subOnLine, item1);
            return true;
        }

        #endregion
    }
}