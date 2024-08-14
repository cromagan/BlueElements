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
using BlueDatabase.Interfaces;
using BlueDatabase.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using BlueBasics.Enums;
using System;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
public class Method_RowInvalidate : Method_Database, IUseableForButton {

    #region Properties

    public override List<List<string>> Args => [RowVar, FloatVal];
    public List<List<string>> ArgsForButton => [];
    public List<string> ArgsForButtonDescription => [];
    public ButtonArgs ClickableWhen => ButtonArgs.Genau_eine_Zeile;
    public override string Command => "rowinvalidate";
    public override List<string> Constants => [];
    public override string Description => "Stoßt an, dass die Zeile komplett neu durchgerechnet wird.\r\nAber nur, wenn das Letzte Durchrechnen länger her ist, als die angegebenen Tage.";

    public override bool GetCodeBlockAfter => false;

    public override int LastArgMinCount => -1;

    // Manipulates User deswegen, weil eine neue Zeile evtl. andere Rechte hat und dann stören kann.
    public override MethodType MethodType => MethodType.Database |  MethodType.ManipulatesUser;

    public override bool MustUseReturnValue => false;

    public string NiceTextForUser => "Eine Zeile neu durchrechnen lassen";

    // Auch nur zum Zeilen Anlegen
    public override string Returns => string.Empty;

    public override string StartSequence => "(";

    public override string Syntax => "RowInvalidate(Row, AgeInDays);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var myRow = Method_Row.ObjectToRow(attvar.Attributes[0]);
        if (myRow?.Database is not Database db || db.IsDisposed) { return new DoItFeedback(ld, "Fehler in der Zeile"); }

        if (db.Column.SysRowState is not ColumnItem srs) { return new DoItFeedback(ld, "Zeilen-Status-Spalte nicht gefunden"); }

        var m = CellCollection.EditableErrorReason(srs, myRow, EditableErrorReasonType.EditAcut, false, false, true, false);
        if (!string.IsNullOrEmpty(m)) { SetNotSuccesful(varCol); return new DoItFeedback(ld, "Datenbank-Meldung: " + m); }
        if (!scp.ProduktivPhase) { return new DoItFeedback(ld, "Zellen setzen Testmodus deaktiviert."); }

        if (myRow == MyRow(scp)) {
            return new DoItFeedback(ld, "Die eigene Zelle kann nicht invalidiert werden.");
        }

        //if (RowCollection.InvalidatedRows.Contains(myRow) || 
        //    RowCollection.DidRows.Contains(myRow) ||
        //    RowCollection.FailedRows.Contains(myRow)) {
        //    return DoItFeedback.Null();
        //}



        var mydb = MyDatabase(scp);
        if (mydb == null) { return new DoItFeedback(ld, "Interner Fehler"); }

        var d = attvar.ValueNumGet(1);

        if (d < 0.1) { return new DoItFeedback(ld, "Intervall zu kurz."); }

        var v = myRow.CellGetDateTime(srs);
        if (DateTime.UtcNow.Subtract(v).TotalDays < d) { return DoItFeedback.Null(); }

        myRow.InvalidateRowState($"Script-Befehl: 'RowInvalidate' der Tabelle {mydb.Caption}, Skript {scp.ScriptName}");
   
        return DoItFeedback.Null();
    }

    public string TranslateButtonArgs(List<string> args, string filterarg, string rowarg) => filterarg;

    #endregion
}