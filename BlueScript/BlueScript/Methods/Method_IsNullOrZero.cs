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

    internal class Method_IsNullOrZero : Method {

        #region Properties

        public override List<enVariableDataType> Args => new() { enVariableDataType.Variable_Any };
        public override string Description => "Gibt TRUE zurück, wenn die Variable nicht existiert, fehlerhaft ist, keinen Inhalt hat, oder dem Zahlenwert 0 entspricht. Falls die Variable existiert, muss diese dem Typ Numeral entsprechen.";
        public override bool EndlessArgs => false;
        public override string EndSequence => ")";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.Bool;
        public override string StartSequence => "(";
        public override string Syntax => "isNullOrZero(Variable)";

        #endregion

        #region Methods

        public override List<string> Comand(Script s) => new() { "isnullorzero" };

        public override strDoItFeedback DoIt(strCanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            return attvar.Attributes == null
                ? attvar.FehlerTyp != enSkriptFehlerTyp.VariableNichtGefunden ? strDoItFeedback.AttributFehler(this, attvar) : strDoItFeedback.Wahr()
                : string.IsNullOrEmpty(attvar.Attributes[0].ValueString) ? strDoItFeedback.Wahr()
                : attvar.Attributes[0].Type is enVariableDataType.Null or enVariableDataType.Error or enVariableDataType.NotDefinedYet ? strDoItFeedback.Wahr()
                : attvar.Attributes[0].Type != enVariableDataType.Numeral ? new strDoItFeedback("Variable existiert, ist aber nicht vom Datentyp Numeral.")
                : attvar.Attributes[0].ValueDouble == 0 ? strDoItFeedback.Wahr()
                : strDoItFeedback.Falsch();
        }

        #endregion
    }
}