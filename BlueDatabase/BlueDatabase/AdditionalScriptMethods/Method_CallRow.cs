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

using BlueScript.Enums;
using BlueScript.EventArgs;
using BlueScript.Interfaces;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
public class Method_CallRow : Method_Database, IUseableForButton {

    #region Properties

    public override List<List<string>> Args => [StringVal, RowVar, StringVal];

    public List<List<string>> ArgsForButton => [StringVal, StringVal];

    public List<string> ArgsForButtonDescription => ["Auszuführendes Skript", "Zusätzliches Attribut"];

    public ButtonArgs ClickableWhen => ButtonArgs.Genau_eine_Zeile;

    public override string Command => "callrow";

    public override string Description => "Führt das Skript bei der angegebenen Zeile aus.\r\n" +
            "Wenn die Zeile Null ist, wird kein Fehler ausgegeben.\r\n" +
        "Es werden keine Variablen aus dem Haupt-Skript übernommen oder zurückgegeben.\r\n" +
        "Kein Zugriff auf auf Datenbank-Variablen!";

    public override bool GetCodeBlockAfter => false;

    public override int LastArgMinCount => 0;

    public override MethodType MethodType => MethodType.Database | MethodType.ChangeAnyDatabaseOrRow  | MethodType.SpecialVariables;

    public override bool MustUseReturnValue => false;

    public string NiceTextForUser => "Ein Skript mit der eingehenden Zeile ausführen";

    public override string Returns => string.Empty;

    public override string StartSequence => "(";

    public override string Syntax => "CallRow(Scriptname, Row, Attribut0, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, LastArgMinCount, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        if (SetErrorAllowed(varCol)) { return new DoItFeedback(infos.Data, "'CallRow' bei FehlerCheck Routinen nicht erlaubt."); }

        var row = Method_Row.ObjectToRow(attvar.Attributes[1]);

        if (row == null || row.IsDisposed) {
            return new DoItFeedback(infos.Data, "Zeile nicht gefunden");
        }

        #region Attributliste erzeugen

        var a = new List<string>();
        for (var z = 2; z < attvar.Attributes.Count; z++) {
            if (attvar.Attributes[z] is VariableString vs1) { a.Add(vs1.ValueString); }
        }

        #endregion

        var vs = attvar.ValueStringGet(0);

        var s2 = row.ExecuteScript(null, vs, false, false, scp.ProduktivPhase, 0, a, true, false, true);
        if (!s2.AllOk) {
            infos.Data.Protocol.AddRange(s2.Protocol);
            return new DoItFeedback(infos.Data, "'Subroutinen-Aufruf [" + vs + "]' wegen vorherhigem Fehler bei Zeile '" + row.CellFirstString() + "' abgebrochen");
        }

        return DoItFeedback.Null();
    }

    public string TranslateButtonArgs(string arg1, string arg2, string arg3, string arg4, string filterarg, string rowarg) => arg1 + "," + rowarg + "," + arg2;

    #endregion
}