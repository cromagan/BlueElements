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
using BlueBasics;
using BlueScript.Structures;
using BlueScript.Enums;

namespace BlueScript.Methods {

    internal class Method_Number : Method {

        #region Properties

        public override List<VariableDataType> Args => new() { VariableDataType.String, VariableDataType.Numeral };
        public override string Description => "Gibt den Text als Zahl zurück. Fall dies keine gültige Zahl ist, wird NaN-Value zurückgegeben.";
        public override bool EndlessArgs => false;
        public override string EndSequence => ")";
        public override bool GetCodeBlockAfter => false;
        public override VariableDataType Returns => VariableDataType.Numeral;
        public override string StartSequence => "(";
        public override string Syntax => "Number(string, NaNValue)";

        #endregion

        #region Methods

        public override List<string> Comand(Script? s) => new() { "number" };

        public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            return !string.IsNullOrEmpty(attvar.ErrorMessage) ? DoItFeedback.AttributFehler(this, attvar)
                 : ((VariableString)attvar.Attributes[0]).ValueString.IsNumeral() ? new DoItFeedback(((VariableString)attvar.Attributes[0]).ValueString, VariableDataType.Numeral)
                 : new DoItFeedback(((VariableString)attvar.Attributes[1]).ValueString, VariableDataType.Numeral);
        }

        #endregion
    }
}