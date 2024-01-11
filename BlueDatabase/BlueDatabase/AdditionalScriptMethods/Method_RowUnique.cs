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
using BlueScript;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueDatabase.AdditionalScriptMethods.Method_Database;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
public class Method_RowUnique : Method {

    #region Properties

    public override List<List<string>> Args => [FilterVar];
    public override string Command => "rowunique";

    public override string Description => "Sucht eine Zeile mittels dem gegebenen Filter.\r\n" +
                                          "Wird keine Zeile gefunden, wird eine neue Zeile erstellt.\r\n" +
                                          "Werden mehrere Zeilen gefunden, wird das Skript abgebrochen.\r\n" +
                                          "Kann keine neue Zeile erstellt werden, wird das Programm unterbrochen";

    public override bool EndlessArgs => true;
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.IO | MethodType.NeedLongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableRowItem.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "RowUnique( Filter, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, EndlessArgs, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var allFi = Method_Filter.ObjectToFilter(attvar.Attributes, 0);
        if (allFi is null || allFi.Count == 0) { return new DoItFeedback(infos.Data, "Fehler im Filter"); }

        var r = allFi.Rows;

        if (r.Count > 1) { return new DoItFeedback(infos.Data, "Datenbankfehler, zu viele Einträge gefunden. Zuvor Prüfen mit RowCount."); }

        if (r.Count == 0) {
            if (!scp.ChangeValues) { return new DoItFeedback(infos.Data, "Zeile anlegen im Testmodus deaktiviert."); }
            var nr = RowCollection.GenerateAndAdd(allFi, "Skript Befehl 'RowUnique'");
            if (nr == null) { return new DoItFeedback(infos.Data, "Neue Zeile konnte nicht erstellt werden"); }
            return Method_Row.RowToObjectFeedback(nr);
        }

        return Method_Row.RowToObjectFeedback(r[0]);
    }

    #endregion
}