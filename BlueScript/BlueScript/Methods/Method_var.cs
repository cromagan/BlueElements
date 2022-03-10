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
using BlueScript.Structures;
using BlueScript.Enums;

namespace BlueScript.Methods {

    internal class Method_Var : Method {

        #region Properties

        public override List<VariableDataType> Args => new() { VariableDataType.Any };
        public override string Description => "Erstellt eine neue Variable, der Typ wird automtisch bestimmt.";
        public override bool EndlessArgs => false;
        public override string EndSequence => ";";
        public override bool GetCodeBlockAfter => false;
        public override VariableDataType Returns => VariableDataType.Null;
        public override string StartSequence => "";
        public override string Syntax => "var VariablenName = Wert;";

        #endregion

        #region Methods

        public override List<string> Comand(Script? s) => new() { "var" };

        public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
            if (string.IsNullOrEmpty(infos.AttributText)) { return new DoItFeedback("Kein Text angekommen."); }

            return Method_BerechneVariable.VariablenBerechnung(infos.AttributText + ";", s, infos, true);

            //return s.BerechneVariable.DoitKomplett(infos.AttributText + ";", s, infos, true);
        }

        #endregion
    }
}