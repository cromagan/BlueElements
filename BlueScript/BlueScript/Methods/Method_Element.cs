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

using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;

internal class Method_Element : Method {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableListString.ShortName_Variable }, new List<string> { VariableFloat.ShortName_Plain } };
    public override string Description => "Gibt ein das Element der Liste mit der Indexnummer als Text zurück. Die Liste beginnt mit dem Element 0.";
    public override bool EndlessArgs => false;
    public override string EndSequence => ")";
    public override bool GetCodeBlockAfter => false;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "Element(VariableListe, Indexnummer)";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "element" };

    public override DoItFeedback DoIt(CanDoFeedback infos, Script s, int line) {
        var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs, line);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar, line); }
        var i = ((VariableFloat)attvar.Attributes[1]).ValueInt;
        var list = ((VariableListString)attvar.Attributes[0]).ValueList;
        return i < 0 || i >= list.Count ? new DoItFeedback("Element nicht in Liste", line)
            : new DoItFeedback(list[i], string.Empty, line);
    }

    #endregion
}