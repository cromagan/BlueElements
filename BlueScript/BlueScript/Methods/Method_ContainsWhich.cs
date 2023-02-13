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
using System.Linq;
using System.Text.RegularExpressions;
using BlueBasics;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript.Methods;

internal class Method_ContainsWhitch : Method {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableString.ShortName_Plain, VariableListString.ShortName_Plain }, new List<string> { VariableBool.ShortName_Plain }, new List<string> { VariableString.ShortName_Plain, VariableListString.ShortName_Plain } };
    public override string Description => "Prüft ob eine der Zeichenketten als ganzes Wort vorkommt. Gibt dann als Liste alle gefundenen Strings zurück.";

    public override bool EndlessArgs => true;

    public override string EndSequence => ")";

    public override bool GetCodeBlockAfter => false;

    public override string Returns => VariableListString.ShortName_Plain;

    public override string StartSequence => "(";

    public override string Syntax => "ContainsWhich(String, CaseSensitive, Value1, Value2, ...)";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "containswhich" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(s, infos, this, attvar); }

        var found = new List<string>();

        #region Wortliste erzeugen

        var wordlist = new List<string>();

        for (var z = 2; z < attvar.Attributes.Count; z++) {
            if (attvar.Attributes[z] is VariableString vs) { wordlist.Add(vs.ValueString); }
            if (attvar.Attributes[z] is VariableListString vl) { wordlist.AddRange(vl.ValueList); }
        }
        wordlist = wordlist.SortedDistinctList();

        #endregion

        var rx = RegexOptions.IgnoreCase;
        if (((VariableBool)attvar.Attributes[1]).ValueBool) { rx = RegexOptions.None; }

        if (attvar.Attributes[0] is VariableString vs2) {
            foreach (var thisW in wordlist.Where(thisW => vs2.ValueString.ContainsWord(thisW, rx))) {
                _ = found.AddIfNotExists(thisW);
            }
        }

        if (attvar.Attributes[0] is VariableListString vl2) {
            foreach (var thiss in vl2.ValueList) {
                foreach (var thisW in wordlist.Where(thisW => thiss.ContainsWord(thisW, rx))) {
                    _ = found.AddIfNotExists(thisW);
                }
            }
        }

        return new DoItFeedback(s, infos, found);
    }

    #endregion
}