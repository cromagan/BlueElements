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

using System;
using System.Collections.Generic;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript.Methods;

internal class Method_IsType : Method {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { Variable.Any_Variable }, new List<string> { VariableString.ShortName_Plain } };
    public override string Description => "Prüft, ob der Variablenntyp dem hier angegeben Wert entspricht. Es wird keine Inhaltsprüfung ausgeführt!";
    public override bool EndlessArgs => false;
    public override string EndSequence => ");";
    public override bool GetCodeBlockAfter => false;
    public override string Returns => string.Empty;

    public override string StartSequence => "(";
    public override string Syntax => "isType(Variable, num / str / lst / bol / err / ukn)";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "istype" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(s, infos, this, attvar); }

        if (string.Equals(attvar.Attributes[1].ReadableText, attvar.Attributes[0].MyClassId, StringComparison.OrdinalIgnoreCase)) {
            return DoItFeedback.Wahr(s, infos);
        }
        return DoItFeedback.Falsch(s, infos);
    }

    #endregion
}