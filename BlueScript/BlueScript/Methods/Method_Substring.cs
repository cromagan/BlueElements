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
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript.Methods;

internal class Method_Substring : Method {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableString.ShortName_Plain }, new List<string> { VariableFloat.ShortName_Plain }, new List<string> { VariableFloat.ShortName_Plain } };

    public override string Description => "Gibt einen Teilstring zurück. Ist der Start oder das Ende keine gültige Position, wird das bestmögliche zurückgegeben und kein Fehler ausgelöst. Subrtring(\"Hallo\", 2,2) gibt ll zurück.";
    public override bool EndlessArgs => false;
    public override string EndSequence => ")";
    public override bool GetCodeBlockAfter => false;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "Substring(String, Start, Anzahl)";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "substring" };

    public override DoItFeedback DoIt(CanDoFeedback infos, Script s, int line) {
        var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs, line);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar, line); }
        var st = ((VariableFloat)attvar.Attributes[1]).ValueInt;
        var en = ((VariableFloat)attvar.Attributes[2]).ValueInt;
        if (st < 0) {
            en += st;
            st = 0;
        }

        var t = ((VariableString)attvar.Attributes[0]).ValueString;

        if (st > t.Length) {
            return DoItFeedback.Null(line);
        }

        if (st + en > t.Length) {
            en = t.Length - st;
        }
        return new DoItFeedback(t.Substring(st, en), string.Empty, line);
    }

    #endregion
}