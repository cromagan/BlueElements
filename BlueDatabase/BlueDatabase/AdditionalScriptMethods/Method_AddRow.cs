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

using BlueBasics.Enums;
using BlueScript;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Diagnostics;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
public class Method_AddRow : Method_Database {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "addrow";

    public override string Description => "Lädt eine andere Datenbank (Database) und erstellt eine neue Zeile.\r\n" +
                                          "Es wird immer eine neue Zeile erstellt!\r\n" +
                                          "KeyValue muss einen Wert enthalten - zur Not kann UniqueRowId() benutzt werden.";

    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.ChangeAnyDatabaseOrRow;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableRowItem.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "AddRow(database, keyvalue);";

    #endregion

    #region Methods

   public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var mydb = MyDatabase(scp);
        if (mydb == null) { return new DoItFeedback(ld, "Interner Fehler"); }

        var db = DatabaseOf(scp, attvar.ValueStringGet(0));
        if (db == null) { return new DoItFeedback(ld, "Datenbank '" + attvar.ValueStringGet(0) + "' nicht gefunden"); }

        var m = db.EditableErrorReason(EditableErrorReasonType.EditAcut);
        if (!string.IsNullOrEmpty(m)) { return new DoItFeedback(ld, "Datenbank-Meldung: " + m); }

        if (string.IsNullOrEmpty(attvar.ValueStringGet(1))) { return new DoItFeedback(ld, "KeyValue muss einen Wert enthalten."); }
        //var r = db.Row[attvar.ValueString(1)];

        //if (r != null && !(attvar.ValueBool(2)) { return Method_Row?.RowToObject(r); }

        if (attvar.ValueBoolGet(2)) {
            StackTrace stackTrace = new();

            if (stackTrace.FrameCount > 400) {
                return new DoItFeedback(ld, "Stapelspeicherüberlauf");
            }
        }

        if (!scp.ProduktivPhase) { return new DoItFeedback(ld, "Zeile anlegen im Testmodus deaktiviert."); }

        var r = db.Row.GenerateAndAdd(db.NextRowKey(), attvar.ValueStringGet(1), null, true, "Script-Befehl: 'AddRow' von " + mydb.Caption);

        return Method_Row.RowToObjectFeedback(r);
    }

    #endregion
}