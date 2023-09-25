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

using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_Do : Method {

    #region Properties

    public override List<List<string>> Args => new();
    public override string Comand => "do";
    public override string Description => "Führt den Codeblock dauerhaft aus, bis der Befehl Break empfangen wurde. Variablen, die innerhalb des Codeblocks definiert wurden, sind ausserhalb des Codeblocks nicht mehr verfügbar.";
    public override bool EndlessArgs => false;
    public override bool GetCodeBlockAfter => true;
    public override MethodType MethodType => MethodType.Standard;
    public override string Returns => string.Empty;
    public override string StartSequence => string.Empty;
    public override string Syntax => "Do { Break; }";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, EndlessArgs, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var du = 0;

        var scp2 = new ScriptProperties(scp, scp.AllowedMethods | MethodType.Break);

        DoItFeedback scx;
        do {
            du++;
            if (du > 100000) { return new DoItFeedback(infos.Data, "Do-Schleife nach 100.000 Durchläufen abgebrochen."); }

            scx = Method_CallByFilename.CallSub(varCol, scp2, infos, "Do-Schleife", infos.CodeBlockAfterText, false, infos.Data.Line - 1, infos.Data.Subname, null);
            if (!scx.AllOk) { return scx; }

            if (scx.BreakFired || scx.EndScript) { break; }
        } while (true);

        return new DoItFeedback(false, scx.EndScript); // Du muss die Breaks konsumieren, aber EndSkript muss weitergegeben werden
    }

    #endregion
}