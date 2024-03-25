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

using BlueBasics;
using BlueDatabase.Enums;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using static BlueBasics.Converter;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
internal class Method_AutoCorrect : Method_Database {

    #region Properties

    public override List<List<string>> Args => [[Variable.Any_Variable]];
    public override string Command => "autocorrect";

    public override string Description => "Ändert den Wert der angegebenen Variablen so ab, wie es in die Zelle geschrieben werden würde.\r\n" +
        "Z.B: Autosort und Ersetzungen\r\n" +
        "Es können nur Variablen benutzt werden, die auch zu einer Spalte gehören.";

    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 1;
    public override MethodType MethodType => MethodType.Database;
    public override bool MustUseReturnValue => false;

    public override string Returns => string.Empty;

    public override string StartSequence => "(";
    public override string Syntax => "AutoCorrect(Column1, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, LastArgMinCount, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        for (var n = 0; n < attvar.Attributes.Count; n++) {
            var column = Column(scp, attvar, n);
            if (column == null || column.IsDisposed) { return new DoItFeedback(infos.Data, "Spalte in Datenbank nicht gefunden."); }
            var columnVar = attvar.Attributes[n];

            if (columnVar == null || columnVar.ReadOnly) { return new DoItFeedback(infos.Data, "Variable Schreibgeschützt."); }
            if (!column.Function.CanBeChangedByRules()) { return new DoItFeedback(infos.Data, "Spalte nicht veränderbar."); }

            var s = string.Empty;
            switch (columnVar) {
                case VariableFloat vf:
                    s = vf.ValueNum.ToStringFloat5();
                    break;

                case VariableListString vl:
                    s = vl.ValueList.JoinWithCr();
                    break;

                case VariableBool vb:
                    s = vb.ValueBool.ToPlusMinus();
                    break;

                case VariableString vs:
                    s = vs.ValueString;
                    break;

                default:
                    Develop.DebugPrint("Typ nicht erkannt: " + columnVar.MyClassId);
                    break;
            }

            s = column.AutoCorrect(s, false);

            switch (columnVar) {
                case VariableFloat vf:
                    vf.ValueNum = DoubleParse(s);
                    break;

                case VariableListString vl:
                    vl.ValueList = s.SplitByCrToList().ToList();
                    break;

                case VariableBool vb:
                    vb.ValueBool = s.FromPlusMinus();
                    break;

                case VariableString vs:
                    vs.ValueString = s;
                    break;

                default:
                    Develop.DebugPrint("Typ nicht erkannt: " + columnVar.MyClassId);
                    break;
            }
        }

        return DoItFeedback.Null();
    }

    #endregion
}