﻿// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
public class Method_CallRow : Method_Database, IUseableForButton {

    #region Properties

    public override List<List<string>> Args => [StringVal, RowVar, StringVal];
    public List<List<string>> ArgsForButton => [StringVal, StringVal];
    public List<string> ArgsForButtonDescription => ["Auszuführendes Skript", "Attribut0"];
    public ButtonArgs ClickableWhen => ButtonArgs.Genau_eine_Zeile;
    public override string Command => "callrow";
    public override List<string> Constants => [];

    public override string Description => "Führt das Skript bei der angegebenen Zeile aus.\r\n" +
            "Wenn die Zeile Null ist, wird kein Fehler ausgegeben.\r\n" +
        "Es werden keine Variablen aus dem Haupt-Skript übernommen oder zurückgegeben.\r\n" +
        "Kein Zugriff auf auf Datenbank-Variablen!";

    public override bool GetCodeBlockAfter => false;

    public override int LastArgMinCount => 0;

    public override MethodType MethodType => MethodType.SpecialVariables;

    public override bool MustUseReturnValue => false;

    public string NiceTextForUser => "Ein Skript mit der eingehenden Zeile ausführen";

    public override string Returns => VariableString.ShortName_Plain;

    public override string StartSequence => "(";

    public override string Syntax => "CallRow(Scriptname, Row, Attribut0, ...);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, CanDoFeedback ld){
        //if (MyDatabase(scp) is not { IsDisposed: false } myDb) { return DoItFeedback.InternerFehler(ld); }

        var row = Method_Row.ObjectToRow(attvar.Attributes[1]);
        if (row is not { IsDisposed: false }) { return new DoItFeedback("Zeile nicht gefunden", true, ld); }
        //if (row?.Database is not { IsDisposed: false } db) { return new DoItFeedback("Fehler in der Zeile", true, ld); }
        //if (db != myDb && !db.AreScriptsExecutable()) { return new DoItFeedback($"In der Datenbank '{db.Caption}' sind die Skripte defekt", false, ld); }

        //if (row == MyRow(scp)) {
        //    return new DoItFeedback("Skripte der eigenen Zeile mit Call aufrufen.", true, ld);
        //}

        #region Attributliste erzeugen

        var a = new List<string>();
        for (var z = 2; z < attvar.Attributes.Count; z++) {
            if (attvar.Attributes[z] is VariableString vs1) { a.Add(vs1.ValueString); }
        }

        #endregion

        var vs = attvar.ValueStringGet(0);

        var scx = row.ExecuteScript(null, vs, scp.ProduktivPhase, 0, a, false, true);
        if (scx.Failed) {
            return new DoItFeedback("'Subroutinen-Aufruf [" + vs + "]' wegen vorherhigem Fehler bei Zeile '" + row.CellFirstString() + "' abgebrochen", false, ld);
        }
        scx.ConsumeBreakAndReturn();
        return scx;
    }

    public string TranslateButtonArgs(List<string> args, string filterarg, string rowarg) => args[0] + "," + rowarg + "," + args[1];

    #endregion
}