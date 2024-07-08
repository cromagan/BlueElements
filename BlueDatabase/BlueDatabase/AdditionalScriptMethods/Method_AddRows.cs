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

using BlueBasics;
using BlueBasics.Enums;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Diagnostics;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
public class Method_AddRows : Method_Database {

    #region Properties

    public override List<List<string>> Args => [StringVal, ListStringVar];
    public override string Command => "addrows";

    public override string Description => "Lädt eine andere Datenbank (Database) und erstellt eine neue Zeilen.\r\n" +
                                          "Es werden nur neue Zeilen erstellt, die nicht vorhanden sind!\r\n" +
                                          "Leere KeyValues werden übersprungen.";

    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.ChangeAnyDatabaseOrRow;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "AddRows(database, keyvalues);";

    #endregion

    #region Methods

   public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var mydb = MyDatabase(scp);
        if (mydb == null) { return new DoItFeedback(ld, "Interner Fehler"); }

        var db = DatabaseOf(scp, attvar.ValueStringGet(0));
        if (db == null) { return new DoItFeedback(ld, "Datenbank '" + attvar.ValueStringGet(0) + "' nicht gefunden"); }

        var m = db.EditableErrorReason(EditableErrorReasonType.EditAcut);
        if (!string.IsNullOrEmpty(m)) { return new DoItFeedback(ld, "Datenbank-Meldung: " + m); }

        var keys = attvar.ValueListStringGet(1);
        keys = keys.SortedDistinctList();

        StackTrace stackTrace = new();
        if (stackTrace.FrameCount > 400) {
            return new DoItFeedback(ld, "Stapelspeicherüberlauf");
        }

        if (!scp.ProduktivPhase) { return new DoItFeedback(ld, "Zeile anlegen im Testmodus deaktiviert."); }

        foreach (var thisKey in keys) {
            if (db.Row[thisKey] is null) {
                _ = db.Row.GenerateAndAdd(db.NextRowKey(), thisKey, null, true, "Script-Befehl: 'AddRows' von " + mydb.Caption);
            }
        }

        return DoItFeedback.Null();
    }

    #endregion
}