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
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;

internal class Method_Do : Method {

    #region Properties

    public override List<List<string>> Args => [];
    public override string Command => "do";
    public override List<string> Constants => [];
    public override string Description => "Führt den Codeblock dauerhaft aus, bis der Befehl Break empfangen wurde. Variablen, die innerhalb des Codeblocks definiert wurden, sind ausserhalb des Codeblocks nicht mehr verfügbar.\r\nDie Variable INDEX zeigt an, bei welchen Eintrag der Zeiger sich gerade befindet.";
    public override bool GetCodeBlockAfter => true;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => string.Empty;
    public override string Syntax => "Do { Break; }";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(Command, varCol, infos.AttributText, Args, LastArgMinCount, infos.LogData, scp);
        if (attvar.Failed) { return DoItFeedback.AttributFehler(infos.LogData, attvar); }

        var index = -1;

        var scp2 = new ScriptProperties(scp, [.. scp.AllowedMethods, Method_Break.Method], scp.Stufe, scp.Chain);

        ScriptEndedFeedback scx;

        do {
            index++;
            if (index > 100000) { return new DoItFeedback("Do-Schleife nach 100.000 Durchläufen abgebrochen.", true, infos.LogData); }

            var addme = new List<Variable>() { new VariableDouble("Index", index, true, "Iterations-Variable") };

            scx = Method_CallByFilename.CallSub(varCol, scp2, infos.CodeBlockAfterText, infos.LogData.Line - 1, infos.LogData.Subname, addme, null, "Do", infos.LogData);
            if (scx.Failed || scx.BreakFired || scx.ReturnFired) { break; }
        } while (true);

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