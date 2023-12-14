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

using System.Collections.Generic;
using System.Linq;
using BlueBasics.Interfaces;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.Extensions;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
internal class Method_MatchColumnFormat : Method_Database {

    #region Properties

    public override List<List<string>> Args => [[VariableString.ShortName_Plain, VariableListString.ShortName_Plain], [Variable.Any_Variable]];
    public override string Command => "matchcolumnformat";
    public override string Description => "Prüft, ob der Inhalt der Variable mit dem Format der angegebenen Spalte übereinstimmt. Leere Inhalte sind dabei TRUE.";
    public override bool EndlessArgs => false;
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.Database;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBool.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "MatchColumnFormat(Value, Column)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, EndlessArgs, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var column = Column(varCol, attvar, 1);
        if (column == null || column.IsDisposed) { return new DoItFeedback(infos.Data, "Spalte in Datenbank nicht gefunden"); }

        var tocheck = new List<string>();
        if (attvar.Attributes[0] is VariableListString vl) { tocheck.AddRange(vl.ValueList); }
        if (attvar.Attributes[0] is VariableString vs) { tocheck.Add(vs.ValueString); }

        tocheck = tocheck.SortedDistinctList();

        if (tocheck.Any(thisstring => !thisstring.IsFormat(column))) {
            return DoItFeedback.Falsch();
        }

        return DoItFeedback.Wahr();
    }

    #endregion
}