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
using BlueScript;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueDatabase.AdditionalScriptComands;

public class Method_CallRow : Method_Database {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableString.ShortName_Plain }, new List<string> { VariableRowItem.ShortName_Variable } };

    public override string Description => "Führt das Skript bei der angegebenen Zeile aus.\r\n" +
        "Wenn die Zeile Null ist, wird kein Fehler ausgegeben.\r\n" +
        "Es werden keine Variablen aus dem Haupt-Skript übernommen oder zurückgegeben.\r\n" +
        "Um auf Datenbank-Variablen zugreifen zu können,\r\n" +
        "die vorher veränderr wurden, muss WriteBackDBVariables zuvor ausgeführt werden.";

    public override bool EndlessArgs => false;

    public override string EndSequence => ");";

    public override bool GetCodeBlockAfter => false;

    public override string Returns => string.Empty;
    public override string StartSequence => "(";

    public override string Syntax => "CallRow(Scriptname, Row);";

    #endregion

    #region Methods

    public override List<string> Comand(List<Variable> currentvariables) => new() { "callrow" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos, this, attvar); }

        var row = Method_Row.ObjectToRow(attvar.Attributes[1]);

        if (row == null) {
            return new DoItFeedback(infos, "Zeile nicht gefunden");
        }

        var vs = (VariableString)attvar.Attributes[0];
        s.Sub++;
        var s2 = row.ExecuteScript(null, vs.ValueString, false, false, s.ChangeValues, 0);
        if (!string.IsNullOrEmpty(s2.ErrorMessage)) { return new DoItFeedback(infos, "Subroutine '" + vs.ValueString + "': " + s2.ErrorMessage); }
        s.Sub--;
        return DoItFeedback.Null(infos);
    }

    #endregion
}