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

using BlueScript.Enums;
using BlueScript.EventArgs;
using BlueScript.Interfaces;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using static BlueDatabase.AdditionalScriptMethods.Method_Database;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
public class Method_CallFilter : BlueScript.Methods.Method, IUseableForButton {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal, FilterVar];

    public List<List<string>> ArgsForButton => [StringVal, StringVal];

    public List<string> ArgsForButtonDescription => ["Auszuführendes Skript", "Attribut0"];

    public ButtonArgs ClickableWhen => ButtonArgs.Eine_oder_mehr_Zeilen;

    public override string Command => "callfilter";

    public override string Description => "Sucht Zeilen und ruft in dessen Datenbank ein Skript für jede Zeile aus.\r\n" +
                                                "Über den Filtern kann bestimmt werden, welche Zeilen es betrifft.\r\n" +
                                            "Es werden keine Variablen aus dem Haupt-Skript übernommen oder zurückgegeben.\r\n" +
                                            "Kein Zugriff auf auf Datenbank-Variablen!";

    public override bool GetCodeBlockAfter => false;

    public override int LastArgMinCount => 1;

    public override MethodType MethodType => MethodType.ChangeAnyDatabaseOrRow | MethodType.SpecialVariables;

    public override bool MustUseReturnValue => false;

    public string NiceTextForUser => "Ein Skript für jede Zeile der Filterung ausführen";

    public override string Returns => string.Empty;

    public override string StartSequence => "(";

    public override string Syntax => "CallFilter(SubName, Attribut0, Filter, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, LastArgMinCount, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        using var allFi = Method_Filter.ObjectToFilter(attvar.Attributes, 2);

        if (allFi is null || allFi.Count == 0) { return new DoItFeedback(infos.Data, "Fehler im Filter"); }
        if (allFi.Database is not Database db || db.IsDisposed) { return new DoItFeedback(infos.Data, "Datenbankfehler!"); }

        var r = allFi.Rows;
        if (r.Count == 0) { return DoItFeedback.Null(); }

        List<string> a = [attvar.ValueStringGet(1)];
        var vs = attvar.ValueStringGet(0);

        foreach (var thisR in r) {
            if (thisR != null && !thisR.IsDisposed) {
                //s.Sub++;
                var s2 = thisR.ExecuteScript(null, vs, false, true, scp.ProduktivPhase, 0, a, false, true);
                if (!s2.AllOk) {
                    infos.Data.Protocol.AddRange(s2.Protocol);
                    return new DoItFeedback(infos.Data, "'Subroutinen-Aufruf [" + vs + "]' wegen vorherhigem Fehler bei Zeile '" + thisR.CellFirstString() + "' abgebrochen");
                }
                //s.Sub--;
            }

            //s.BreakFired = false;
            //s.EndScript = false;
        }

        return DoItFeedback.Null();
    }

    public string TranslateButtonArgs(string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7, string arg8, string filterarg, string rowarg) => arg1 + "," + filterarg;

    #endregion
}