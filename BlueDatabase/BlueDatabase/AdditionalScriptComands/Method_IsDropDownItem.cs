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

using BlueScript;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Linq;
using static BlueBasics.Extensions;

namespace BlueDatabase.AdditionalScriptComands;

internal class Method_IsDropDownItem : MethodDatabase {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableString.ShortName_Plain, VariableListString.ShortName_Plain }, new List<string> { Variable.Any_Variable } };
    public override string Description => "Prüft, ob der Inhalt oder die Inhalte der Variable im Dropdownmenu der Spalte vorkommt.\r\nEs werden nur fest eingegebene Dropdown-Werte berücksichtigt - keine 'Werte anderer Zellen'.\r\nEs wird streng auf die Groß/Kleinschreibung geachtet.";
    public override bool EndlessArgs => false;
    public override string EndSequence => ")";
    public override bool GetCodeBlockAfter => false;

    public override string Returns => VariableBool.ShortName_Plain;

    public override string StartSequence => "(";
    public override string Syntax => "IsDropDownItem(Value, Column)";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "isdropdownitem" };

    public override DoItFeedback DoIt(CanDoFeedback infos, Script s, int line) {
        var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs, line);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar); }

        var column = Column(s, attvar.Attributes[1].Name);
        if (column == null) { return new DoItFeedback("Spalte in Datenbank nicht gefunden"); }

        var tocheck = new List<string>();
        if (attvar.Attributes[0] is VariableListString vl) { tocheck.AddRange(vl.ValueList); }
        if (attvar.Attributes[0] is VariableString vs) { tocheck.Add(vs.ValueString); }

        tocheck = tocheck.SortedDistinctList();

        if (tocheck.Any(thisstring => !column.DropDownItems.Contains(thisstring))) {
            return DoItFeedback.Falsch();
        }

        return DoItFeedback.Wahr();
    }

    #endregion
}