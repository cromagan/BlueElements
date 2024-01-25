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

#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_Try : Method {

    #region Properties

    public override List<List<string>> Args => [];
    public override string Command => "try";

    public override string Description => "Führt den Codeblock innerhalb der geschweiften Klammern aus.\r\n" +
                                          "Tritt währenddessen ein Fehler auf, wird der Codeblock verlassen und die weiteren Befehle innerhalb des Codeblocks ignoriert.\r\n" +
                                          "Das Skript wird in jedem Fall nach dem Codeblock-Ende weiter ausgeführt.\r\n" +
                                          "Variablen, die innerhalb des Codeblocks definiert wurden, sind ausserhalb des Codeblocks nicht mehr verfügbar.";

     public override int LastArgMinCount => -1;
    public override bool GetCodeBlockAfter => true;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => string.Empty;
    public override string Syntax => "Try { }";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, LastArgMinCount, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }
        var scx = Method_CallByFilename.CallSub(varCol, scp, infos, "Try-Befehl", infos.CodeBlockAfterText, false, infos.Data.Line - 1, infos.Data.Subname, null);
        return new DoItFeedback(scx.BreakFired, scx.EndScript);
    }

    #endregion
}