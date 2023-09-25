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

using BlueBasics.Enums;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueDatabase.AdditionalScriptComands;

public class Method_WriteBackDBVariables : Method_Database {

    #region Properties

    public override List<List<string>> Args => new();

    public override string Comand => "writebackdbvariables";

    public override string Description => "Schreibt die aktuellen Datenbank-Variabeln vorzeitig zurück in die Datenbank.\r\n" +
                                    "So kann mit Routinen, die separate Skripte aufrufen, auf die Datenbank Variablen zugegriffen werden.";

    public override bool EndlessArgs => true;
  
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.Database | MethodType.NeedLongTime;
    public override string Returns => string.Empty;

    public override string StartSequence => "(";

    public override string Syntax => "WriteBackDBVariables();";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, EndlessArgs, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var db = MyDatabase(varCol);
        if (db == null) { return new DoItFeedback(infos.Data, "Datenbankfehler!"); }
        var m = db.EditableErrorReason(EditableErrorReasonType.EditAcut);
        if (!string.IsNullOrEmpty(m)) { return new DoItFeedback(infos.Data, "Datenbank-Meldung: " + m); }
        if (!scp.ChangeValues) { return new DoItFeedback(infos.Data, "Variabeln zurückschreiben im Testmodus deaktiviert."); }

        db.Variables = DatabaseAbstract.WriteBackDbVariables(varCol, db.Variables, "DB_");

        return DoItFeedback.Null();
    }

    #endregion
}