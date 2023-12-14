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
using System.Diagnostics;
using BlueBasics.Enums;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
public class Method_CallDatabase : Method_Database {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, StringVal];

    public override string Command => "calldatabase";

    public override string Description => "Führt das Skript in der angegebenen Datenabank aus.\r\n" +
            "Die Attribute werden in eine List-Varible Attributes eingefügt und stehen im auszühenden Skript zur Verfügung.\r\n" +
        "Es werden keine Variablen aus dem Haupt-Skript übernommen oder zurückgegeben.\r\n" +
        "Um auf Datenbank-Variablen zugreifen zu können,\r\n" +
        "die vorher verändert wurden, muss WriteBackDBVariables zuvor ausgeführt werden.";

    public override bool EndlessArgs => true;

    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.ChangeAnyDatabaseOrRow | MethodType.NeedLongTime;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";

    public override string Syntax => "CallDatabase(DatabaseName, Scriptname, Attribut0, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, EndlessArgs, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var see = varCol.GetSystem("SetErrorEnabled");
        if (see is not VariableBool seet) { return new DoItFeedback(infos.Data, "SetErrorEnabled Variable nicht gefunden"); }
        if (seet.ValueBool) { return new DoItFeedback(infos.Data, "'CallDatabase' bei FehlerCheck Routinen nicht erlaubt."); }

        var db = DatabaseOf(varCol, attvar.ValueStringGet(0));
        if (db == null) { return new DoItFeedback(infos.Data, "Datenbank '" + attvar.ValueStringGet(0) + "' nicht gefunden"); }

        var m = db.EditableErrorReason(EditableErrorReasonType.EditAcut);
        if (!string.IsNullOrEmpty(m)) { return new DoItFeedback(infos.Data, "Datenbank-Meldung: " + m); }

        StackTrace stackTrace = new();
        if (stackTrace.FrameCount > 400) { return new DoItFeedback(infos.Data, "Stapelspeicherüberlauf"); }

        if (!scp.ChangeValues) { return new DoItFeedback(infos.Data, "CallDatabase im Testmodus deaktiviert."); }

        var f = db.ExecuteScript(null, attvar.ValueStringGet(1), true, null, null);

        if (f.AllOk) {
            return new DoItFeedback(infos.Data, f.ProtocolText);
        }

        return DoItFeedback.Null();
    }

    #endregion
}