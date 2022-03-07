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

using System;
using System.Collections.Generic;
using BlueScript.Structuren;
using Skript.Enums;
using static BlueBasics.Constants;

namespace BlueScript.Methods {

    internal class Method_DateTimeNowUTC : Method {

        #region Properties

        public override List<enVariableDataType> Args => new();
        public override string Description => "Gibt die akutelle UTC-Uhrzeit im Format\r" + Format_Date7 + " zurück.";
        public override bool EndlessArgs => false;
        public override string EndSequence => ")";
        public override bool GetCodeBlockAfter => false;
        public override enVariableDataType Returns => enVariableDataType.String;
        public override string StartSequence => "(";
        public override string Syntax => "DateTimeUTCNow()";

        #endregion

        #region Methods

        public override List<string> Comand(Script? s) => new() { "datetimeutcnow" };

        public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            return !string.IsNullOrEmpty(attvar.ErrorMessage)
                ? DoItFeedback.AttributFehler(this, attvar)
                : new DoItFeedback(DateTime.UtcNow.ToString(Format_Date7), enVariableDataType.String);
        }

        #endregion
    }
}