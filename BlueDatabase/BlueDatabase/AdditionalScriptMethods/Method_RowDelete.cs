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

using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
public class Method_RowDelete : Method_Database, IUseableForButton {

    #region Properties

    public override List<List<string>> Args => [FilterVar];

    public List<List<string>> ArgsForButton => [];

    public List<string> ArgsForButtonDescription => [];

    public ButtonArgs ClickableWhen => ButtonArgs.Eine_oder_mehr_Zeilen;

    public override string Command => "rowdelete";

    public override List<string> Constants => [];

    public override string Description => "Löscht die gefundenen Zeilen";

    public override bool GetCodeBlockAfter => false;

    public override int LastArgMinCount => 1;

    // Manipulates User deswegen, weil eine neue Zeile evtl. andere Rechte hat und dann stören kann.
    public override MethodType MethodType => MethodType.Database;

    public override bool MustUseReturnValue => false; // Auch nur zum Zeilen Anlegen

    public string NiceTextForUser => "Die gefunden Zeilen löschen";

    public override string Returns => VariableBool.ShortName_Plain;

    public override string StartSequence => "(";
    public override string Syntax => "RowDelete(Filter, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (MyDatabase(scp) is not { IsDisposed: false } myDb) { return DoItFeedback.InternerFehler(ld); }

        var (allFi, failedReason, needsScriptFix) = Method_Filter.ObjectToFilter(attvar.Attributes, 0, myDb, scp.ScriptName, true);
        if (allFi == null || !string.IsNullOrEmpty(failedReason)) { return new DoItFeedback($"Filter-Fehler: {failedReason}", needsScriptFix, ld); }

        if (!scp.ProduktivPhase) {
            allFi.Dispose();
            return DoItFeedback.TestModusInaktiv(ld);
        }

        var r = RowCollection.Remove(allFi, null, "Script Command: RowDelete");
        allFi.Dispose();
        return new DoItFeedback(r);
    }

    public string TranslateButtonArgs(List<string> args, string filterarg, string rowarg) => filterarg;

    #endregion
}