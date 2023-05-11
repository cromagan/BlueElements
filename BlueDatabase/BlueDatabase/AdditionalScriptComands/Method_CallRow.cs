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

using System.Collections.Generic;
using BlueScript;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueDatabase.AdditionalScriptComands.Method_Database;

namespace BlueDatabase.AdditionalScriptComands;

public class Method_CallRow : Method {

    #region Properties

    public override List<List<string>> Args => new() { StringVal, RowVar };

    public override string Description => "Führt das Skript bei der angegebenen Zeile aus.\r\n" +
        "Wenn die Zeile Null ist, wird kein Fehler ausgegeben.\r\n" +
        "Es werden keine Variablen aus dem Haupt-Skript übernommen oder zurückgegeben.\r\n" +
        "Um auf Datenbank-Variablen zugreifen zu können,\r\n" +
        "die vorher verändert wurden, muss WriteBackDBVariables zuvor ausgeführt werden.";

    public override bool EndlessArgs => false;
    public override string EndSequence => ");";
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.ChangeAnyDatabaseOrRow | MethodType.NeedLongTime;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";

    public override string Syntax => "CallRow(Scriptname, Row);";

    #endregion

    #region Methods

    public override List<string> Comand(VariableCollection? currentvariables) => new() { "callrow" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs, infos.Data);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var see = s.Variables.GetSystem("SetErrorEnabled");
        if (see is not VariableBool seet) { return new DoItFeedback(infos.Data, "SetErrorEnabled Variable nicht gefunden"); }
        if (seet.ValueBool) { return new DoItFeedback(infos.Data, "'CallRow' bei FehlerCheck Routinen nicht erlaubt."); }

        var row = Method_Row.ObjectToRow(attvar.Attributes[1]);

        if (row == null) {
            return new DoItFeedback(infos.Data, "Zeile nicht gefunden");
        }

        var vs = attvar.ValueString(0);
        s.Sub++;
        var s2 = row.ExecuteScript(null, vs, false, false, s.ChangeValues, 0);
        if (!s2.AllOk) {
            infos.Data.Protocol.AddRange(s2.Protocol);
            return new DoItFeedback(infos.Data, "'Subroutinen-Aufruf [" + vs + "]' wegen vorherhigem Fehler bei Zeile '" + row.CellFirstString() + "' abgebrochen");
        }
        s.Sub--;
        s.BreakFired = false;
        s.EndScript = false;
        return DoItFeedback.Null();
    }

    #endregion
}