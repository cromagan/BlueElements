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

using System.Collections.Generic;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript.Methods;

internal class Method_IsNullOrEmpty : Method {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { Variable.Any_Variable } };
    public override string Description => "Gibt TRUE zurück, wenn die Variable nicht existiert, fehlerhaft ist oder keinen Inhalt hat.";

    public override bool EndlessArgs => false;

    public override string EndSequence => ")";

    public override bool GetCodeBlockAfter => false;

    public override string Returns => VariableBool.ShortName_Plain;
    public override string StartSequence => "(";

    public override string Syntax => "isNullOrEmpty(Variable)";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "isnullorempty" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs);

        if (attvar.Attributes.Count == 0) {
            if (attvar.FehlerTyp != ScriptIssueType.VariableNichtGefunden) {
                return DoItFeedback.AttributFehler(s, infos, this, attvar);
            }

            return DoItFeedback.Wahr(s, infos);
        }

        if (attvar.Attributes[0].IsNullOrEmpty) { return DoItFeedback.Wahr(s, infos); }

        if (attvar.Attributes[0] is VariableUnknown) {
            return DoItFeedback.Wahr(s, infos);
        }

        return DoItFeedback.Falsch(s, infos);
    }

    #endregion
}