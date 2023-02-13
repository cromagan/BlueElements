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

public class Method_RowCount : Method_Database {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableFilterItem.ShortName_Variable } };
    public override string Description => "Zählt die Zeilen, die mit dem gegebenen Filter gefunden werden.";

    public override bool EndlessArgs => true;

    public override string EndSequence => ")";

    public override bool GetCodeBlockAfter => false;

    public override string Returns => VariableFloat.ShortName_Plain;
    public override string StartSequence => "(";

    public override string Syntax => "RowCount(Filter, ...)";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "rowcount" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(s, infos, this, attvar); }

        var allFi = Method_Filter.ObjectToFilter(attvar.Attributes, 0);
        if (allFi is null) { return new DoItFeedback(s, infos, "Fehler im Filter"); }

        var r = RowCollection.MatchesTo(allFi);

        return new DoItFeedback(s, infos);
    }

    #endregion
}