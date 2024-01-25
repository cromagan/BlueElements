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
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueDatabase.AdditionalScriptMethods.Method_Database;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
public class Method_RowCount : Method {

    #region Properties

    public override List<List<string>> Args => [FilterVar];
    public override string Command => "rowcount";
    public override string Description => "Zählt die Zeilen, die mit dem gegebenen Filter gefunden werden.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 1;
    public override MethodType MethodType => MethodType.IO | MethodType.NeedLongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableFloat.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "RowCount(Filter, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, LastArgMinCount, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var allFi = Method_Filter.ObjectToFilter(attvar.Attributes, 0);
        if (allFi is null) { return new DoItFeedback(infos.Data, "Fehler im Filter"); }

        var r = allFi.Rows;

        return new DoItFeedback(r.Count);
    }

    #endregion
}