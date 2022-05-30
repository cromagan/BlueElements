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

using System.Collections.Generic;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript.Methods;

internal class Method_Sub : Method {

    #region Properties

    public override List<List<string>> Args => new() { new() { VariableSub.ShortName_Plain } };
    public override string Description => "Bezeichnet den Start einer Subroutine.";
    public override bool EndlessArgs => false;
    public override string EndSequence => "()";
    public override bool GetCodeBlockAfter => true;
    public override string Returns => string.Empty;
    public override string StartSequence => "";
    public override string Syntax => "Sub SubName() {Code }";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "sub" };

    public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
        if (string.IsNullOrEmpty(infos.AttributText)) { return new DoItFeedback("Kein Text angekommen."); }
        if (!Variable.IsValidName(infos.AttributText)) { return new DoItFeedback(infos.AttributText + " ist kein gültiger Subroutinen-Name."); }

        //Subroutinen werden einfach übersprungen
        s.Line += infos.LineBreakInCodeBlock;
        return new DoItFeedback(string.Empty);
    }

    #endregion
}