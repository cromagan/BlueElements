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
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using BlueScript;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueDatabase.AdditionalScriptMethods.Method_Database;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once ClassNeverInstantiated.Global
public class Method_Row : Method, IUseableForButton {

    #region Properties

    public override List<List<string>> Args => [FilterVar];

    public List<List<string>> ArgsForButton => [];

    public List<string> ArgsForButtonDescription => [];

    public ButtonArgs ClickableWhen => ButtonArgs.Egal;

    public override string Command => "row";

    public override List<string> Constants => [];

    public override string Description => "Sucht eine Zeile mittels dem gegebenen Filter.\r\n" +
                                          "Wird keine Zeile gefunden, wird ein leeres Zeilenobjekt erstellt. Es wird keine neue Zeile erstellt.\r\n" +
                                          "Mit RowIsNull kann abgefragt werden, ob die Zeile gefunden wurde.\r\n" +
                                          "Werden mehrere Zeilen gefunden, wird das Programm abgebrochen. Um das zu verhindern, kann RowCount benutzt werden.\r\n" +
                                          "Alternative: Der Befehl RowUnique kümmert sich darum, das immer eine Zeile zurückgegeben wird.";

    public override bool GetCodeBlockAfter => false;

    public override int LastArgMinCount => 1;

    public override MethodType MethodType => MethodType.Standard;

    public override bool MustUseReturnValue => true;

    public string NiceTextForUser => "Eine neue Zeile mit den eingehenden Filterwerten anlegen - auf jeden Fall!";

    public override string Returns => VariableRowItem.ShortName_Variable;

    public override string StartSequence => "(";

    public override string Syntax => "Row(Filter, ...)";

    #endregion

    #region Methods

    public static RowItem? ObjectToRow(Variable? attribute) => attribute is not VariableRowItem vro ? null : vro.RowItem;

    public static DoItFeedback RowToObjectFeedback(RowItem? row) => new(new VariableRowItem(row));

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        using var allFi = Method_Filter.ObjectToFilter(attvar.Attributes, 0);
        if (allFi is null) { return new DoItFeedback(ld, "Fehler im Filter"); }

        var r = allFi.Rows;

        if (r.Count > 1) { return new DoItFeedback(ld, "Datenbankfehler, zu viele Einträge gefunden. Zuvor Prüfen mit RowCount."); }

        if (r.Count is 0 or > 1) {
            return RowToObjectFeedback(null);
        }

        return RowToObjectFeedback(r[0]);
    }

    public string TranslateButtonArgs(List<string> args, string filterarg, string rowarg) => filterarg;

    #endregion
}