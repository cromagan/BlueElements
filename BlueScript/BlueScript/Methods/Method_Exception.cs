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
using System.Collections.Generic;

namespace BlueScript {

    internal class Method_Exception : Method {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.String };
        public override string Description => "Unterbricht das Skript mit einer Fehlermeldung.";
        public override bool EndlessArgs => false;
        public override string EndSequence => ");";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.Null;
        public override string StartSequence => "(";
        public override string Syntax => "Exception(\"Unbehandelter Programmcode!\");";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "Exception" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            if (string.IsNullOrEmpty(infos.AttributText)) { return new strDoItFeedback("Die Ausführung wurde absichtlich abgebrochen."); }
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            return attvar.Attributes == null || attvar.Attributes.Count != 1 ? new strDoItFeedback("Die Ausführung wurde absichtlich abgebrochen.")
                                                                             : new strDoItFeedback(attvar.Attributes[0].ValueString);
        }

        #endregion
    }
}