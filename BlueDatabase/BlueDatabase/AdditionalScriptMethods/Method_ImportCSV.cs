// Authors:
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

using BlueBasics.Enums;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
internal class Method_ImportCsv : Method_Database {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "importcsv";
    public override List<string> Constants => [];
    public override string Description => "Importiert den Inhalt, der als CSV vorliegen muss, in die Datenbank.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.Database;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "ImportCSV(CSVText, Separator);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var txt = attvar.ValueStringGet(0);
        var sep = attvar.ValueStringGet(1);

        var db = MyDatabase(scp);
        if (db == null) { return new DoItFeedback(ld, "Datenbankfehler!"); }

        var m = db.EditableErrorReason(EditableErrorReasonType.EditAcut);
        if (!string.IsNullOrEmpty(m)) { return new DoItFeedback(false, false, $"Datenbanksperre: {m}"); }

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(ld); }

        var sx = db.ImportCsv(txt, true, true, sep, false, false);

        return string.IsNullOrEmpty(sx) ? DoItFeedback.Null() : new DoItFeedback(ld, sx);
    }

    #endregion
}