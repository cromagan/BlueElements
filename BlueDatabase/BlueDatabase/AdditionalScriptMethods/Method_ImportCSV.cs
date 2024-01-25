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
using BlueBasics.Enums;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
internal class Method_ImportCsv : Method_Database {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "importcsv";
    public override string Description => "Importiert den Inhalt, der als CSV vorliegen muss, in die Datenbank.";
     public override int LastArgMinCount => -1;

    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.Database | MethodType.ChangeAnyDatabaseOrRow | MethodType.IO | MethodType.NeedLongTime;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "ImportCSV(CSVText, Separator);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, LastArgMinCount, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var txt = attvar.ValueStringGet(0);
        var sep = attvar.ValueStringGet(1);

        var db = MyDatabase(scp);
        if (db == null) { return new DoItFeedback(infos.Data, "Datenbankfehler!"); }

        var m = db.EditableErrorReason(EditableErrorReasonType.EditAcut);
        if (!string.IsNullOrEmpty(m)) { return new DoItFeedback(infos.Data, "Datenbank-Meldung: " + m); }

        if (!scp.ChangeValues) { return new DoItFeedback(infos.Data, "Import im Testmodus deaktiviert."); }

        var sx = db.Import(txt, true, true, sep, false, false);

        if (string.IsNullOrEmpty(sx)) {
            return DoItFeedback.Null();
        }

        return new DoItFeedback(infos.Data, sx);
    }

    #endregion
}