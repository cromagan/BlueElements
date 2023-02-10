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

using BlueScript;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueDatabase.AdditionalScriptComands;

public class Method_RowIsNull : MethodDatabase {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableRowItem.ShortName_Variable } };
    public override string Description => "Prüft, ob die übergebene Zeile NULL ist.";

    public override bool EndlessArgs => false;

    public override string EndSequence => ")";

    public override bool GetCodeBlockAfter => false;
    public override string Returns => VariableBool.ShortName_Plain;

    public override string StartSequence => "(";

    public override string Syntax => "RowIsNull(Row)";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "rowisnull" };

    public override DoItFeedback DoIt(CanDoFeedback infos, Script s, int line) {
        var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs, line);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar); }

        if (attvar.Attributes[0] is not VariableRowItem vr) { return new DoItFeedback("Kein Zeilenobjekt übergeben."); }

        //var r = Method_Row.ObjectToRow(attvar.Attributes[0]);

        return vr.RowItem == null ? DoItFeedback.Wahr() : DoItFeedback.Falsch();
    }

    #endregion
}