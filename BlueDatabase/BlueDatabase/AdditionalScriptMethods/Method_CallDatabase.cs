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

#nullable enable

using System.Collections.Generic;
using System.Diagnostics;
using BlueBasics.Enums;
using BlueScript.Enums;
using BlueScript.EventArgs;
using BlueScript.Interfaces;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
public class Method_CallDatabase : Method_Database, IUseableForButton {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, StringVal];
    public List<List<string>> ArgsForButton => [StringVal, StringVal, StringVal];
    public ButtonArgs ClickableWhen => ButtonArgs.Egal;
    public override string Command => "calldatabase";

    public override string Description => "Führt das Skript in der angegebenen Datenabank aus.\r\n" +
            "Die Attribute werden in eine List-Varible Attributes eingefügt und stehen im auszühenden Skript zur Verfügung.\r\n" +
        "Es werden keine Variablen aus dem Haupt-Skript übernommen oder zurückgegeben.\r\n" +
        "Um auf Datenbank-Variablen zugreifen zu können,\r\n" +
        "die vorher verändert wurden, muss WriteBackDBVariables zuvor ausgeführt werden.";

    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 0;
    public override MethodType MethodType => MethodType.ChangeAnyDatabaseOrRow | MethodType.NeedLongTime;
    public override bool MustUseReturnValue => false;
    public string NiceTextForUser => "Ein Skript einer anderen Datenbank ausführen";
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "CallDatabase(DatabaseName, Scriptname, Attribut0, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, LastArgMinCount, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var db = DatabaseOf(scp, attvar.ValueStringGet(0));
        if (db == null) { return new DoItFeedback(infos.Data, "Datenbank '" + attvar.ValueStringGet(0) + "' nicht gefunden"); }

        var m = db.EditableErrorReason(EditableErrorReasonType.EditAcut);
        if (!string.IsNullOrEmpty(m)) { return new DoItFeedback(infos.Data, "Datenbank-Meldung: " + m); }

        StackTrace stackTrace = new();
        if (stackTrace.FrameCount > 400) { return new DoItFeedback(infos.Data, "Stapelspeicherüberlauf"); }

        if (!scp.ChangeValues) { return new DoItFeedback(infos.Data, "CallDatabase im Testmodus deaktiviert."); }

        #region Attributliste erzeugen

        var a = new List<string>();
        for (var z = 2; z < attvar.Attributes.Count; z++) {
            if (attvar.Attributes[z] is VariableString vs1) { a.Add(vs1.ValueString); }
        }

        #endregion

        var f = db.ExecuteScript(null, attvar.ValueStringGet(1), true, null, a);

        if (!f.AllOk) {
            return new DoItFeedback(infos.Data, f.ProtocolText);
        }

        return DoItFeedback.Null();
    }

    public string TranslateButtonArgs(string arg1, string arg2, string arg3, string arg4, string filterarg, string rowarg) => arg1 + "," + arg2 + "," + arg3;

    #endregion
}