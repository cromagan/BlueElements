// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

#nullable enable

using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;

internal class Method_SetIfHasValue : Method {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableString.ShortName_Variable, VariableListString.ShortName_Variable, VariableFloat.ShortName_Variable }, new List<string> { Variable.Any_Plain } };

    public override string Description => "Diese Routine setzt den ersten Wert, der keinen Fehler verursacht und einen Wert enthält in die erste Variable.\r\nDabei müssen die Datentypen übereinstimmen.\r\nFalls einer der Werte ein Variable ist, die nicht existiert, wird diese einfach übergangen.\r\nAls 'kein Wert' wird bei Zahlen ebenfalls 0 gewertet.\r\nListen, die einen Eintrag haben (auch wenn dessen Wert leer ist), zählt >nicht< als kein Eintrag.";
    public override bool EndlessArgs => true;
    public override string EndSequence => ");";
    public override bool GetCodeBlockAfter => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "SetIfHasValue(Variable, Werte, ...);";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "SetIfHasValue" };

    public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
        var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar); }

        if (attvar.Attributes[0].Readonly) { return DoItFeedback.Schreibgschützt(); }

        for (var z = 1; z < attvar.Attributes.Count; z++) {
            if (attvar.Attributes[z] is VariableUnknown) { continue; }
            if (attvar.Attributes[z].ShortName != attvar.Attributes[0].ShortName) { return new DoItFeedback("Variablentyp zur Ausgangsvariable unterschiedlich."); }

            switch (attvar.Attributes[z]) {
                case VariableFloat vf:
                    if (vf.ValueNum != 0) {
                        ((VariableFloat)attvar.Attributes[0]).ValueNum = vf.ValueNum;
                        return DoItFeedback.Null();
                    }
                    break;

                case VariableString vs:
                    if (!string.IsNullOrEmpty(vs.ValueString)) {
                        ((VariableString)attvar.Attributes[0]).ValueString = vs.ValueString;
                        return DoItFeedback.Null();
                    }
                    break;

                case VariableBool vs:

                    ((VariableBool)attvar.Attributes[0]).ValueBool = vs.ValueBool;
                    return DoItFeedback.Null();

                case VariableListString vl:
                    if (vl.ValueList != null) {
                        ((VariableListString)attvar.Attributes[0]).ValueList = vl.ValueList;
                        return DoItFeedback.Null();
                    }
                    break;
            }
        }

        return DoItFeedback.Null();
    }

    #endregion
}