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
using System.Diagnostics.CodeAnalysis;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_ForEach : Method {

    #region Properties

    public override List<List<string>> Args => [[VariableUnknown.ShortName_Plain], ListStringVar];
    public override string Command => "foreach";
    public override string Description => "Führt den Codeblock für jeden List-Eintrag aus.\r\nDer akuelle Eintrag wird in der angegebenen Variable abgelegt.\r\nMit Break kann die Schleife vorab verlassen werden.\r\nVariablen die innerhalb des Codeblocks definiert wurden, sind ausserhalb des Codeblocks nicht mehr verfügbar.";
    public override bool EndlessArgs => false;

    public override bool GetCodeBlockAfter => true;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "ForEach(Variable, List) { }";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, EndlessArgs, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        var l = attvar.ValueListStringGet(1);

        var varnam = "value";

        if (attvar.Attributes[0] is VariableUnknown vkn) { varnam = vkn.Value; }

        if (!Variable.IsValidName(varnam)) { return new DoItFeedback(infos.Data, varnam + " ist kein gültiger Variablen-Name"); }

        var vari = varCol.Get(varnam);
        if (vari != null) {
            return new DoItFeedback(infos.Data, "Variable " + varnam + " ist bereits vorhanden.");
        }

        var scx = new DoItFeedback(false, false);
        var scp2 = new ScriptProperties(scp, scp.AllowedMethods | MethodType.Break);

        foreach (var thisl in l) {
            var nv = new VariableString(varnam, thisl, true, "Iterations-Variable");

            scx = Method_CallByFilename.CallSub(varCol, scp2, infos, "ForEach-Schleife", infos.CodeBlockAfterText, false, infos.Data.Line - 1, infos.Data.Subname, nv);
            if (!scx.AllOk) { return scx; }

            if (scx.BreakFired || scx.EndScript) { break; }
        }

        return new DoItFeedback(false, scx.EndScript); // Du muss die Breaks konsumieren, aber EndSkript muss weitergegeben werden
    }

    #endregion
}