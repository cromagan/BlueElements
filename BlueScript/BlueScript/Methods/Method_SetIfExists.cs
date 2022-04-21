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

using System.Collections.Generic;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript.Methods {

    internal class Method_SetIfExists : Method {

        #region Properties

        public override List<List<string>> Args => new() { new() { VariableString.ShortName_Variable, VariableListString.ShortName_Variable, VariableFloat.ShortName_Variable, VariableBool.ShortName_Variable }, new() { Variable.Any_Plain } };

        public override string Description => "Diese Routine setzt den ersten Wert, der keinen Fehler verursacht in die erste Variable.\r\nDabei müssen die Datentypen übereinstimmen.\r\nFalls einer der Werte eine Variable ist, die nicht existiert, wird diese einfach übergangen.";
        public override bool EndlessArgs => true;
        public override string EndSequence => ");";
        public override bool GetCodeBlockAfter => false;
        public override string Returns => string.Empty;
        public override string StartSequence => "(";
        public override string Syntax => "SetIfExists(Variable, Werte, ...);";

        #endregion

        #region Methods

        public override List<string> Comand(Script? s) => new() { "SetIfExists" };

        public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
            var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
            if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar); }

            if (attvar.Attributes[0].Readonly) { return DoItFeedback.Schreibgschützt(); }

            for (var z = 1; z < attvar.Attributes.Count; z++) {
                if (attvar.Attributes[z] is VariableUnknown) { continue; }

                if (attvar.Attributes[z].ShortName != attvar.Attributes[0].ShortName) { return new DoItFeedback("Variablentyp zur Ausgangsvariable unterschiedlich."); }

                switch (attvar.Attributes[z]) {
                    case VariableString vs:
                        ((VariableString)attvar.Attributes[0]).ValueString = vs.ValueString;
                        return DoItFeedback.Null();

                    case VariableBool vb:
                        ((VariableBool)attvar.Attributes[0]).ValueBool = vb.ValueBool;
                        return DoItFeedback.Null();

                    case VariableFloat vf:
                        ((VariableFloat)attvar.Attributes[0]).ValueNum = vf.ValueNum;
                        return DoItFeedback.Null();

                    case VariableListString vl:
                        ((VariableListString)attvar.Attributes[0]).ValueList = vl.ValueList;
                        return DoItFeedback.Null();
                }
            }

            return DoItFeedback.Null();
        }

        #endregion
    }
}