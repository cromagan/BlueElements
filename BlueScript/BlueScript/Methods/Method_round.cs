﻿// Authors:
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

using Skript.Enums;
using System;
using System.Collections.Generic;

namespace BlueScript {

    internal class Method_Round : Method {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.Numeral, enVariableDataType.Integer };
        public override string Description => "Rundet den Zahlenwert mathematisch korrekt.";
        public override bool EndlessArgs => false;
        public override string EndSequence => ")";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.Numeral;
        public override string StartSequence => "(";
        public override string Syntax => "Round(Value, Nachkommastellen)";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "round" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return strDoItFeedback.AttributFehler(this, attvar); }
            var n = (int)attvar.Attributes[1].ValueDouble;
            if (n < 0) { n = 0; }
            if (n > 10) { n = 10; }
            var val = Math.Round(attvar.Attributes[0].ValueDouble, n);
            return new strDoItFeedback(val.ToString(), enVariableDataType.Numeral);
        }

        #endregion
    }
}