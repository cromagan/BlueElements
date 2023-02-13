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

internal class Method_Do : Method {

    #region Properties

    public override List<List<string>> Args => new();
    public override string Description => "Führt den Codeblock dauerhaft aus, bis der Befehl Break empfangen wurde. Variablen, die innerhalb des Codeblocks definiert wurden, sind ausserhalb des Codeblocks nicht mehr verfügbar.";
    public override bool EndlessArgs => false;
    public override string EndSequence => string.Empty;
    public override bool GetCodeBlockAfter => true;
    public override string Returns => string.Empty;
    public override string StartSequence => string.Empty;
    public override string Syntax => "Do { Break; }";

    #endregion

    #region Methods

    public override List<string> Comand(List<Variable>? currentvariables) => new() { "do" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(s, infos, this, attvar); }

        var du = 0;

        do {
            du++;
            if (du > 100000) { return new DoItFeedback(s, infos, "Do-Schleife nach 100.000 Durchläufen abgebrochen."); }

            var scx = Method_CallByFilename.CallSub(s, infos, infos.CodeBlockAfterText, false, infos.Line - 1, "Do-Schleife");
            if (!string.IsNullOrEmpty(scx.ErrorMessage)) { return scx; }

            if (s.BreakFired) { break; }
        } while (true);

        s.BreakFired = false;

        return DoItFeedback.Null(s, infos);
    }

    #endregion
}