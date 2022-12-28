// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;

internal class Method_Exception : Method {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableString.ShortName_Plain } };
    public override string Description => "Unterbricht das Skript mit einer Fehlermeldung.";
    public override bool EndlessArgs => false;
    public override string EndSequence => ");";
    public override bool GetCodeBlockAfter => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "Exception(\"Unbehandelter Programmcode!\");";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "Exception" };

    public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
        if (string.IsNullOrEmpty(infos.AttributText)) { return new DoItFeedback("Die Ausführung wurde absichtlich abgebrochen."); }
        var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
        return attvar.Attributes == null || attvar.Attributes.Count != 1 ? new DoItFeedback("Die Ausführung wurde absichtlich abgebrochen.")
            : new DoItFeedback("Exception: " + ((VariableString)attvar.Attributes[0]).ValueString);
    }

    #endregion
}