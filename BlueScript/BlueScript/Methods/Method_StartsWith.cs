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
using BlueScript.Variables;

namespace BlueScript.Methods {

    internal class Method_StartsWith : Method {

        #region Properties

        public override List<List<string>> Args => new() { new() { VariableString.ShortName_Plain }, new() { VariableBool.ShortName_Plain }, new() { VariableString.ShortName_Plain } };

        public override string Description => "Prüft, ob der String mit einem der angegeben Strings startet.";
        public override bool EndlessArgs => true;
        public override string EndSequence => ")";
        public override bool GetCodeBlockAfter => false;
        public override string Returns => VariableBool.ShortName_Plain;
        public override string StartSequence => "(";
        public override string Syntax => "StartsWith(String, CaseSensitive, Value1, Value2, ...)";

        #endregion

        #region Methods

        public override List<string> Comand(Script? s) => new() { "startswith" };

        public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar); }
            for (var z = 2; z < attvar.Attributes.Count; z++) {
                if (((VariableBool)attvar.Attributes[1]).ValueBool) {
                    if (((VariableString)attvar.Attributes[0]).ValueString.StartsWith(((VariableString)attvar.Attributes[z]).ValueString)) {
                        return DoItFeedback.Wahr();
                    }
                } else {
                    if (((VariableString)attvar.Attributes[0]).ValueString.ToLower().StartsWith(((VariableString)attvar.Attributes[z]).ValueString.ToLower())) {
                        return DoItFeedback.Wahr();
                    }
                }
            }
            return DoItFeedback.Falsch();
        }

        #endregion
    }
}