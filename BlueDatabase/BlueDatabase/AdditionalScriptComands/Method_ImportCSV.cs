// Authors:
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
using BlueScript;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueDatabase.AdditionalScriptComands;

internal class Method_ImportCSV : Method_Database {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableString.ShortName_Plain }, new List<string> { VariableString.ShortName_Plain } };
    public override string Description => "Importiert den Inhalt, der als CSV vorliegen muss, in die Datenbank.";
    public override bool EndlessArgs => false;
    public override string EndSequence => ");";
    public override bool GetCodeBlockAfter => false;

    public override string Returns => string.Empty;

    public override string StartSequence => "(";
    public override string Syntax => "ImportCSV(CSVText, Separator);";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "importcsv" };

    public override DoItFeedback DoIt(CanDoFeedback infos, Script s, int line) {
        var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs, line);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar, line); }

        var txt = ((VariableString)attvar.Attributes[0]).ValueString;
        var sep = ((VariableString)attvar.Attributes[1]).ValueString;

        var db = MyDatabase(s);

        if (db?.ReadOnly ?? true) { return new DoItFeedback("Datenbank schreibgeschützt.", line); }
        if (!s.ChangeValues) { return new DoItFeedback("Import im Testmodus deaktiviert.", line); }

        var sx = db?.Import(txt, true, true, sep, false, false, true);

        return new DoItFeedback(sx ?? "Datenbank nicht gefunden", line);
    }

    #endregion
}