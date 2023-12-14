// Authors:
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

using System.Collections.Generic;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueDatabase.AdditionalScriptMethods.Method_Database;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
public class Method_CallRow : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, RowVar];

    public override string Command => "callrow";

    public override string Description => "Führt das Skript bei der angegebenen Zeile aus.\r\n" +
            "Wenn die Zeile Null ist, wird kein Fehler ausgegeben.\r\n" +
        "Es werden keine Variablen aus dem Haupt-Skript übernommen oder zurückgegeben.\r\n" +
        "Um auf Datenbank-Variablen zugreifen zu können,\r\n" +
        "die vorher verändert wurden, muss WriteBackDBVariables zuvor ausgeführt werden.";

    public override bool EndlessArgs => false;

    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.ChangeAnyDatabaseOrRow | MethodType.NeedLongTime;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";

    public override string Syntax => "CallRow(Scriptname, Row);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, EndlessArgs, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var see = varCol.GetSystem("SetErrorEnabled");
        if (see is not VariableBool seet) { return new DoItFeedback(infos.Data, "SetErrorEnabled Variable nicht gefunden"); }
        if (seet.ValueBool) { return new DoItFeedback(infos.Data, "'CallRow' bei FehlerCheck Routinen nicht erlaubt."); }

        var row = Method_Row.ObjectToRow(attvar.Attributes[1]);

        if (row == null || row.IsDisposed) {
            return new DoItFeedback(infos.Data, "Zeile nicht gefunden");
        }

        var vs = attvar.ValueStringGet(0);

        var s2 = row.ExecuteScript(null, vs, false, false, scp.ChangeValues, 0);
        if (!s2.AllOk) {
            infos.Data.Protocol.AddRange(s2.Protocol);
            return new DoItFeedback(infos.Data, "'Subroutinen-Aufruf [" + vs + "]' wegen vorherhigem Fehler bei Zeile '" + row.CellFirstString() + "' abgebrochen");
        }

        return DoItFeedback.Null();
    }

    #endregion
}