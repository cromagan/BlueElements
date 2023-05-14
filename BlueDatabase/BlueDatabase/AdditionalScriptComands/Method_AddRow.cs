﻿// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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
using BlueScript;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueDatabase.AdditionalScriptComands;

public class Method_AddRow : Method_Database {

    #region Properties

    public override List<List<string>> Args => new() { StringVal, StringVal, BoolVal };

    public override string Description => "Lädt eine andere Datenbank (Database) und erstellt eine neue Zeile.\r\nKeyValue muss einen Wert enthalten- zur Not kann UniqueRowId() benutzt werden.";

    public override bool EndlessArgs => false;

    public override string EndSequence => ")";

    public override bool GetCodeBlockAfter => false;

    public override MethodType MethodType => MethodType.ChangeAnyDatabaseOrRow | MethodType.NeedLongTime;
    public override string Returns => VariableRowItem.ShortName_Variable;

    public override string StartSequence => "(";
    public override string Syntax => "AddRow(database, keyvalue, startScriptOfNewRow);";

    #endregion

    #region Methods

    public override List<string> Comand(VariableCollection? currentvariables) => new() { "addrow" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs, infos.Data);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var db = DatabaseOf(s.Variables, attvar.ValueString(0));
        if (db == null) { return new DoItFeedback(infos.Data, "Datenbank '" + attvar.ValueString(0) + "' nicht gefunden"); }

        var m = db.EditableErrorReason(EditableErrorReason.EditAcut);
        if (!string.IsNullOrEmpty(m)) { return new DoItFeedback(infos.Data, "Datenbank-Meldung: " + m); }

        if (string.IsNullOrEmpty(attvar.ValueString(1))) { return new DoItFeedback(infos.Data, "KeyValue muss einen Wert enthalten."); }
        //var r = db.Row[attvar.ValueString(1)];

        //if (r != null && !(attvar.ValueBool(2)) { return Method_Row?.RowToObject(r); }

        if (attvar.ValueBool(2)) {
            StackTrace stackTrace = new();

            if (stackTrace.FrameCount > 400) {
                return new DoItFeedback(infos.Data, "Stapelspeicherüberlauf");
            }
        }

        if (!s.ChangeValues) { return new DoItFeedback(infos.Data, "Zeile anlegen im Testmodus deaktiviert."); }

        var r = db.Row.GenerateAndAdd(db.Row.NextRowKey(), attvar.ValueString(1), attvar.ValueBool(2), true, "Script Command: Add Row");

        return Method_Row.RowToObjectFeedback(r);
    }

    #endregion
}