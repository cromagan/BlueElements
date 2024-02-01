﻿// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
using System.Diagnostics.CodeAnalysis;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_Var : Method {

    #region Properties

    public override List<List<string>> Args => [[Variable.Any_Plain]];
    public override string Command => "var";
    public override string Description => "Erstellt eine neue Variable, der Typ wird automatisch bestimmt.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => string.Empty;
    public override string Syntax => "var VariablenName = Wert;";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        if (string.IsNullOrEmpty(infos.AttributText)) { return new DoItFeedback(infos.Data, "Kein Text angekommen."); }

        return VariablenBerechnung(varCol, infos, scp, infos.AttributText + ";", true);
    }

    #endregion
}