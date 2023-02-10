﻿// Authors:
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

using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;

internal class Method_IsNullOrZero : Method {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { Variable.Any_Variable } };
    public override string Description => "Gibt TRUE zurück, wenn die Variable nicht existiert, fehlerhaft ist, keinen Inhalt hat, oder dem Zahlenwert 0 entspricht. Falls die Variable existiert, muss diese dem Typ Numeral entsprechen.";
    public override bool EndlessArgs => false;
    public override string EndSequence => ")";
    public override bool GetCodeBlockAfter => false;
    public override string Returns => VariableBool.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "isNullOrZero(Variable)";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "isnullorzero" };

    public override DoItFeedback DoIt(CanDoFeedback infos, Script s, int line) {
        var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs, line);

        if (attvar.Attributes.Count == 0) {
            if (attvar.FehlerTyp != ScriptIssueType.VariableNichtGefunden) {
                return DoItFeedback.AttributFehler(this, attvar);
            }

            return DoItFeedback.Wahr();
        }

        if (attvar.Attributes[0].IsNullOrEmpty) { return DoItFeedback.Wahr(); }
        if (attvar.Attributes[0] is VariableUnknown) { return DoItFeedback.Wahr(); }

        if (attvar.Attributes[0] is VariableFloat f) {
            if (f.ValueNum == 0) { return DoItFeedback.Wahr(); }

            return DoItFeedback.Falsch();
        }
        return new DoItFeedback("Variable existiert, ist aber nicht vom Datentyp Numeral.");
        //if (attvar.Attributes == null) {
        //    if (attvar.FehlerTyp != ScriptIssueType.VariableNichtGefunden) {
        //        return DoItFeedback.AttributFehler(this, attvar);
        //    } else {
        //        return DoItFeedback.Wahr();
        //    }
        //} else {
        //    if (string.IsNullOrEmpty(((VariableString)attvar.Attributes[0]).ValueString)) {
        //        return DoItFeedback.Wahr();
        //    } else {
        //        if (attvar.Attributes[0].Type is VariableDataType.Null or VariableDataType.Error
        //            or VariableDataType.NotDefinedYet) {
        //            return DoItFeedback.Wahr();
        //        } else {
        //            if (attvar.Attributes[0] is not VariableFloat) {
        //                return new DoItFeedback("Variable existiert, ist aber nicht vom Datentyp Numeral.");
        //            } else {
        //                if (((VariableFloat)attvar.Attributes[0]).ValueNum == 0) {
        //                    return DoItFeedback.Wahr();
        //                } else {
        //                    return DoItFeedback.Falsch();
        //                }
        //            }
        //        }
        //    }
        //}
    }

    #endregion
}