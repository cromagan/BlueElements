// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueScript.Classes;
using BlueScript.Enums;
using BlueScript.Variables;
using BlueTable.AdditionalScriptVariables;
using System.Collections.Generic;
using System.Diagnostics;

namespace BlueTable.AdditionalScriptMethods;

public class Method_CallTable : Method_TableGeneric {

    #region Properties

    public override List<List<string>> Args => [TableVar, StringVal, StringVal];
    public override string Command => "calltable";
    public override List<string> Constants => [];

    public override string Description => "Führt das Skript in der angegebenen Tabelle aus.\r\n" +
            "Die Attribute werden in eine List-Varible Attributes eingefügt und stehen im auszührenden Skript zur Verfügung.\r\n" +
        "Es werden keine Variablen aus dem Haupt-Skript übernommen oder zurückgegeben.";

    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 0;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => false;

    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "CallTable(Table, Scriptname, Attribut0, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.Attributes[0] is not VariableTable vtb || vtb.Table is not { IsDisposed: false } tb) { return new DoItFeedback("Tabelle nicht vorhanden", true, ld); }
        if (tb == MyTable(scp)) { return new DoItFeedback("Befehl Call benutzen!", true, ld); }

        if (!tb.IsEditable(false)) { return new DoItFeedback($"Tabellensperre: {tb.IsNotEditableReason(false)}", false, ld); }

        var stackTrace = new StackTrace();
        if (stackTrace.FrameCount > 400) { return new DoItFeedback("Stapelspeicherüberlauf", true, ld); }

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(ld); }

        #region Attributliste erzeugen

        var a = new List<string>();
        for (var z = 2; z < attvar.Attributes.Count; z++) {
            if (attvar.Attributes[z] is VariableString vs1) { a.Add(vs1.ValueString); }
        }

        #endregion

        var scx = tb.ExecuteScript(null, attvar.ValueStringGet(1), scp.ProduktivPhase, null, a, true, true);
        scx.ConsumeBreakAndReturn();
        if (scx.NeedsScriptFix) {
            return new DoItFeedback($"Unterskript '{attvar.ValueStringGet(1)}' in '{tb.Caption}' hat Fehler verursacht.", false, ld);
        }
        return scx;
    }

    #endregion
}