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

using System.Collections.Generic;
using System.Linq;
using BlueScript.Structuren;
using Skript.Enums;
using static BlueBasics.Extensions;

namespace BlueScript.Methods {

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

        public override List<string> Comand(Script? s) => new() { "call" };

        public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
            //var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            //if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }

            if (string.IsNullOrEmpty(infos.AttributText)) { return new DoItFeedback("Kein Text angekommen."); }

            if (!Variable.IsValidName(infos.AttributText)) { return new DoItFeedback(infos.AttributText + " ist kein gültiger Subroutinen-Name."); }

            var such = new List<string> { "sub" + infos.AttributText.ToLower() + "()" };

            var (pos, _) = NextText(s.ReducedScriptText.ToLower(), 0, such, true, false, KlammernStd);

            if (pos < 0) { return new DoItFeedback("Subroutine " + infos.AttributText + " nicht definert."); }

            var (pos2, _) = NextText(s.ReducedScriptText.ToLower(), pos + 1, such, true, false, KlammernStd);
            if (pos2 > 0) { return new DoItFeedback("Subroutine " + infos.AttributText + " mehrfach definert."); }

            var weiterLine = s.Line;

            var (item1, item2) = GetCodeBlockText(s.ReducedScriptText, pos + such[0].Length);

            if (!string.IsNullOrEmpty(item2)) { return new DoItFeedback("Subroutine " + infos.AttributText + ": " + item2); }

            s.Line = s.ReducedScriptText.Substring(0, pos).Count(c => c == '¶') + 1;
            s.Sub++;

            var tmpv = new List<Variable>();
            tmpv.AddRange(s.Variablen);

            var (err, _) = s.Parse(item1);
            if (!string.IsNullOrEmpty(err)) { return new DoItFeedback("Subroutine " + infos.AttributText + ": " + err); }

            s.Variablen.Clear();
            s.Variablen.AddRange(tmpv);
            s.Sub--;

            if (s.Schleife < 0) { return new DoItFeedback("Subroutinen-Fehler"); }

            s.Line = weiterLine;

            return new DoItFeedback(string.Empty);
        }

        #endregion
    }
}