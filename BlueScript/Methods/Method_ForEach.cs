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

using BlueBasics;
using BlueBasics.Enums;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Diagnostics;

namespace BlueScript.Methods;

internal class Method_ForEach : Method {

    #region Properties

    public override List<List<string>> Args => [[VariableUnknown.ShortName_Plain], ListStringVar];
    public override string Command => "foreach";
    public override List<string> Constants => [];
    public override string Description => "Führt den Codeblock für jeden List-Eintrag aus.\r\nDer akuelle Eintrag wird in der angegebenen Variable abgelegt, diese darf noch nicht deklariert sein.\r\nMit Break kann die Schleife vorab verlassen werden.\r\nVariablen die innerhalb des Codeblocks definiert wurden, sind ausserhalb des Codeblocks nicht mehr verfügbar.\r\nDie Variable INDEX zeigt an, bei welchen Eintrag der Zeiger sich gerade befindet.";
    public override bool GetCodeBlockAfter => true;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "ForEach(NeueVariable, List) { }";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(Command, varCol, infos.AttributText, Args, LastArgMinCount, infos.LogData, scp);
        if (attvar.Failed) { return DoItFeedback.AttributFehler(infos.LogData, this, attvar); }

        var l = attvar.ValueListStringGet(1);

        var varnam = "value";

        if (attvar.Attributes[0] is VariableUnknown vkn) { varnam = vkn.Value; }

        if (!Variable.IsValidName(varnam)) { return new DoItFeedback(varnam + " ist kein gültiger Variablen-Name", true, infos.LogData); }

        var vari = varCol.GetByKey(varnam);
        if (vari != null) {
            return new DoItFeedback("Variable " + varnam + " ist bereits vorhanden.", true, infos.LogData);
        }

        ScriptEndedFeedback? scx = null;
        var scp2 = new ScriptProperties(scp, [.. scp.AllowedMethods, Method_Break.Method], scp.Stufe + 1, scp.Chain);

        var t = Stopwatch.StartNew();

        for (var index = 0; index < l.Count; index++) {
            var addme = new List<Variable>() {
            new VariableString(varnam, l[index], true, "Iterations-Variable"),
            new VariableDouble("Index", index, true, "Iterations-Variable")
            };

            scx = Method_CallByFilename.CallSub(varCol, scp2, "ForEach-Schleife", infos.CodeBlockAfterText, infos.LogData.Line - 1, infos.LogData.Subname, addme, null, "ForEach", infos.LogData);
            if (scx.Failed || scx.BreakFired || scx.ReturnFired) { break; }

            if (t.ElapsedMilliseconds > 1000) {
                t = Stopwatch.StartNew();
                Develop.Message?.Invoke(ErrorType.Info, null, "Skript", ImageCode.Skript, $"Skript: Durchlauf {index} von {l.Count} abschlossen ({l[index]})", scp.Stufe + 1);
            }
        }

        if (scx == null) {
            return new DoItFeedback(false, false, false, string.Empty, null, infos.LogData);
        }

        scx.ConsumeBreak();// Du muss die Breaks konsumieren, aber EndSkript muss weitergegeben werden
        return scx;
    }

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        // Dummy überschreibung.
        // Wird niemals aufgerufen, weil die andere DoIt Rourine überschrieben wurde.

        Develop.DebugPrint_NichtImplementiert(true);
        return DoItFeedback.Falsch();
    }

    #endregion
}