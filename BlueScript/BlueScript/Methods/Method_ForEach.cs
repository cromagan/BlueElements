// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

using BlueBasics;
using BlueBasics.Enums;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Diagnostics;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
internal class Method_ForEach : Method {

    #region Properties

    public override List<List<string>> Args => [[VariableUnknown.ShortName_Plain], ListStringVar];
    public override string Command => "foreach";
    public override List<string> Constants => [];
    public override string Description => "Führt den Codeblock für jeden List-Eintrag aus.\r\nDer akuelle Eintrag wird in der angegebenen Variable abgelegt, diese darf noch nicht deklariert sein.\r\nMit Break kann die Schleife vorab verlassen werden.\r\nVariablen die innerhalb des Codeblocks definiert wurden, sind ausserhalb des Codeblocks nicht mehr verfügbar.";
    public override bool GetCodeBlockAfter => true;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "ForEach(NeueVariable, List) { }";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback cdf, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, Args, LastArgMinCount, cdf, scp);
        if (attvar.Failed) { return DoItFeedback.AttributFehler(cdf, this, attvar); }

        var l = attvar.ValueListStringGet(1);

        var varnam = "value";

        if (attvar.Attributes[0] is VariableUnknown vkn) { varnam = vkn.Value; }

        if (!Variable.IsValidName(varnam)) { return new DoItFeedback(varnam + " ist kein gültiger Variablen-Name", true, cdf); }

        var vari = varCol.Get(varnam);
        if (vari != null) {
            return new DoItFeedback("Variable " + varnam + " ist bereits vorhanden.", true, cdf);
        }

        ScriptEndedFeedback? scx = null;
        var scp2 = new ScriptProperties(scp, [.. scp.AllowedMethods, Method_Break.Method]);

        var t = Stopwatch.StartNew();
        var count = 0;

        foreach (var thisl in l) {
            count++;
            var nv = new VariableString(varnam, thisl, true, "Iterations-Variable");

            scx = Method_CallByFilename.CallSub(varCol, scp2, new CanDoFeedback("ForEach-Schleife", cdf.Position, cdf.Protocol, cdf.Chain, cdf.FailedReason, cdf.NeedsScriptFix, cdf.CodeBlockAfterText, string.Empty), null, null);
            if (scx.Failed || scx.BreakFired || scx.ReturnFired) { break; }

            if (t.ElapsedMilliseconds > 1000) {
                t = Stopwatch.StartNew();
                Develop.Message?.Invoke(ErrorType.Info, null, "Skript", ImageCode.Skript, $"Skript: Durchlauf {count} von {l.Count} abschlossen ({thisl})", cdf.Stufe + 1);
            }
        }

        if (scx == null) {
            return new DoItFeedback(cdf.Subname, cdf.Position, cdf.Protocol, cdf.Chain, false, false, false, string.Empty, null);
        }

        scx.ConsumeBreak();// Du muss die Breaks konsumieren, aber EndSkript muss weitergegeben werden
        return scx;
    }

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, CanDoFeedback ld) {
        // Dummy überschreibung.
        // Wird niemals aufgerufen, weil die andere DoIt Rourine überschrieben wurde.

        Develop.DebugPrint_NichtImplementiert(true);
        return DoItFeedback.Falsch(ld.EndPosition());
    }

    #endregion
}