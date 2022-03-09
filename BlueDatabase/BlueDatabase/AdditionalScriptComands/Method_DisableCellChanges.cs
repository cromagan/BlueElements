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
using BlueScript;
using BlueScript.Structures;
using BlueScript.Enums;

namespace BlueDatabase.AdditionalScriptComands {

    public class Method_DisableCellChanges : MethodDatabase {

        #region Properties

        public override List<VariableDataType> Args => new();

        public override string Description => "Verhindert in dieser Datenbank, dass Änderungen, die im SKript vorgenommen wurden, zurückgespielt werden.\r\nSo können z.B. Variabelen für den Export verändert werden und es hat keine Auswirkungen\r\nauf diese Datenbank.";

        public override bool EndlessArgs => false;

        public override string EndSequence => ");";

        public override bool GetCodeBlockAfter => false;

        public override VariableDataType Returns => VariableDataType.Null;

        public override string StartSequence => "(";

        public override string Syntax => "DisableCellChanges();";

        #endregion

        #region Methods

        public override List<string> Comand(Script? s) => new() { "disablecellchanges" };

        public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar); }

            var ve = s.Variables.GetSystem("CellChangesEnabled");
            ve.Readonly = false;
            ve.ValueBool = false;
            ve.Readonly = true;

            return DoItFeedback.Null();
        }

        #endregion
    }
}