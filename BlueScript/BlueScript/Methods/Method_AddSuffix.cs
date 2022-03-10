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

    internal class Method_AddSuffix : Method {

        #region Properties

        public override List<VariableDataType> Args => new() { VariableDataType.Variable_List, VariableDataType.String };
        public override string Description => "Fügt am Ende jedes Listenobjekts einen Text hinzu.";
        public override bool EndlessArgs => false;
        public override string EndSequence => ");";
        public override bool GetCodeBlockAfter => false;
        public override VariableDataType Returns => VariableDataType.Null;
        public override string StartSequence => "(";
        public override string Syntax => "AddSuffix(VariableListe, SuffixText)";

        #endregion

        #region Methods

        public override List<string> Comand(Script? s) => new() { "addsuffix" };

        public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar); }

            if (attvar.Attributes[0].Readonly) { return DoItFeedback.Schreibgschützt(); }

            var tmp = ((VariableListString)attvar.Attributes[0]).ValueList;

            for (var z = 0; z < tmp.Count; z++) {
                tmp[z] += ((VariableString)attvar.Attributes[1]).ValueString;
            }

            ((VariableListString)attvar.Attributes[0]).ValueList = tmp;

            return DoItFeedback.Null();
        }

        #endregion
    }
}