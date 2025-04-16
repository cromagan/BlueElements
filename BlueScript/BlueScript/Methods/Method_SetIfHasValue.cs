// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
internal class Method_SetIfHasValue : Method {

    #region Properties

    public override List<List<string>> Args => [[VariableString.ShortName_Variable, VariableListString.ShortName_Variable, VariableFloat.ShortName_Variable, VariableBool.ShortName_Variable], [Variable.Any_Plain]];
    public override string Command => "setifhasvalue";
    public override List<string> Constants => [];
    public override string Description => "Diese Routine setzt den ersten Wert, der keinen Fehler verursacht und einen Wert enthält in die erste Variable.\r\nDabei müssen die Datentypen übereinstimmen.\r\nFalls einer der Werte ein Variable ist, die nicht existiert, wird diese einfach übergangen.\r\nAls 'kein Wert' wird bei Zahlen ebenfalls 0 gewertet.\r\nListen, die einen Eintrag haben (auch wenn dessen Wert leer ist), zählt >nicht< als kein Eintrag.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 1;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "SetIfHasValue(Variable, Werte, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.ReadOnly(0)) { return DoItFeedback.Schreibgschützt(ld); }

        for (var z = 1; z < attvar.Attributes.Count; z++) {
            if (attvar.Attributes[z] is VariableUnknown) { continue; }
            if (attvar.MyClassId(z) != attvar.MyClassId(0)) { return new DoItFeedback("Variablentyp zur Ausgangsvariable unterschiedlich.", true, ld); }

            switch (attvar.Attributes[z]) {
                case VariableFloat vf:
                    if (vf.ValueNum != 0) {
                        return attvar.ValueNumSet(0, vf.ValueNum, ld) is { } dif ? dif : DoItFeedback.Null();
                    }
                    break;

                case VariableString vs:
                    if (!string.IsNullOrEmpty(vs.ValueString)) {
                        return attvar.ValueStringSet(0, vs.ValueString, ld) is { } dif2 ? dif2 : DoItFeedback.Null();
                    }
                    break;

                case VariableBool vb:
                    if (attvar.ValueBoolSet(0, vb.ValueBool, ld) is { } dif3) { return dif3; }
                    return DoItFeedback.Null();

                case VariableListString vl:
                    if (attvar.ValueListStringSet(0, vl.ValueList, ld) is { } dif4) { return dif4; }
                    return DoItFeedback.Null();
            }
        }

        return DoItFeedback.Null();
    }

    #endregion
}