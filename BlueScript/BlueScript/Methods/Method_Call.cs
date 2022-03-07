﻿// Authors:
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

using Skript.Enums;
using System.Collections.Generic;
using System.Linq;
using BlueScript.Structuren;
using static BlueBasics.Extensions;

namespace BlueScript {

    internal class Method_Call : Method {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.Any };
        public override string Description => "Ruft eine Subroutine auf";
        public override bool EndlessArgs => false;
        public override string EndSequence => ");";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.Null;
        public override string StartSequence => "(";
        public override string Syntax => "Call(SubName);";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "call" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            //var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            //if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }

            if (string.IsNullOrEmpty(infos.AttributText)) { return new strDoItFeedback("Kein Text angekommen."); }

            if (!Variable.IsValidName(infos.AttributText)) { return new strDoItFeedback(infos.AttributText + " ist kein gültiger Subroutinen-Name."); }

            var such = new List<string> { "sub" + infos.AttributText.ToLower() + "()" };

            var (pos, _) = NextText(s.ReducedScriptText.ToLower(), 0, such, true, false, KlammernStd);

            if (pos < 0) { return new strDoItFeedback("Subroutine " + infos.AttributText + " nicht definert."); }

            var (pos2, _) = NextText(s.ReducedScriptText.ToLower(), pos + 1, such, true, false, KlammernStd);
            if (pos2 > 0) { return new strDoItFeedback("Subroutine " + infos.AttributText + " mehrfach definert."); }

            var weiterLine = s.Line;

            var (item1, item2) = GetCodeBlockText(s.ReducedScriptText, pos + such[0].Length);

            if (!string.IsNullOrEmpty(item2)) { return new strDoItFeedback("Subroutine " + infos.AttributText + ": " + item2); }

            s.Line = s.ReducedScriptText.Substring(0, pos).Count(c => c == '¶') + 1;
            s.Sub++;

            var tmpv = new List<Variable>();
            tmpv.AddRange(s.Variablen);

            var (err, _) = s.Parse(item1);
            if (!string.IsNullOrEmpty(err)) { return new strDoItFeedback("Subroutine " + infos.AttributText + ": " + err); }

            s.Variablen.Clear();
            s.Variablen.AddRange(tmpv);
            s.Sub--;

            if (s.Schleife < 0) { return new strDoItFeedback("Subroutinen-Fehler"); }

            s.Line = weiterLine;

            return new strDoItFeedback(string.Empty);
        }

        #endregion
    }
}