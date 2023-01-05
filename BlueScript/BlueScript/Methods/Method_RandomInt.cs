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

using BlueBasics;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;

internal class Method_RandomInt : Method {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableFloat.ShortName_Plain } };
    public override string Description => "Gibt eine nicht negative Zufalls-Ganzzahl zurück,\rdie kleiner als das angegebene Maximum ist.";
    public override bool EndlessArgs => false;
    public override string EndSequence => ")";
    public override bool GetCodeBlockAfter => false;
    public override string Returns => VariableFloat.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "RandomInt(maxValue)";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "randomint" };

    public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
        var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);

        return !string.IsNullOrEmpty(attvar.ErrorMessage)
            ? DoItFeedback.AttributFehler(this, attvar)
            : new DoItFeedback(Constants.GlobalRND.Next(((VariableFloat)attvar.Attributes[0]).ValueInt));
    }

    #endregion
}