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
using BlueScript;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedMember.Global
internal class Method_ForEachRow : Method_Database {

    #region Properties

    public override List<List<string>> Args => [[VariableUnknown.ShortName_Plain], FilterVar];
    public override string Command => "foreachrow";
    public override List<string> Constants => [];
    public override string Description => "Führt den Codeblock für jede gefundene Zeile aus.\r\nDer akuelle Eintrag wird in der angegebenen Variable abgelegt, diese darf noch nicht deklariert sein.\r\nMit Break kann die Schleife vorab verlassen werden.\r\nVariablen die innerhalb des Codeblocks definiert wurden, sind ausserhalb des Codeblocks nicht mehr verfügbar.";
    public override bool GetCodeBlockAfter => true;
    public override int LastArgMinCount => 1;
    public override MethodType MethodType => MethodType.Database;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "ForEachRow(NeueVariable, Filter1, ...) { }";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback cdf, ScriptProperties scp) {
        if (MyDatabase(scp) is not { IsDisposed: false } myDb) { return DoItFeedback.InternerFehler(cdf); }

        var attvar = SplitAttributeToVars(varCol, cdf.AttributText, Args, LastArgMinCount, cdf, scp);
        if (attvar.Failed) { return DoItFeedback.AttributFehler(cdf, this, attvar); }

        var varnam = "value";

        if (attvar.Attributes[0] is VariableUnknown vkn) { varnam = vkn.Value; }

        if (!Variable.IsValidName(varnam)) { return new DoItFeedback(varnam + " ist kein gültiger Variablen-Name", true, cdf); }

        var vari = varCol.Get(varnam);
        if (vari != null) {
            return new DoItFeedback("Variable " + varnam + " ist bereits vorhanden.", true, cdf);
        }

        var (allFi, failedReason, needsScriptFix) = Method_Filter.ObjectToFilter(attvar.Attributes, 1, myDb, scp.ScriptName, true);

        if (allFi == null || !string.IsNullOrEmpty(failedReason)) { return new DoItFeedback($"Filter-Fehler: {failedReason}", needsScriptFix, cdf); }

        var r = allFi.Rows;
        allFi.Dispose();

        ScriptEndedFeedback? scx = null;
        var scp2 = new ScriptProperties(scp, [.. scp.AllowedMethods, Method_Break.Method]);

        foreach (var thisl in r) {
            var nv = new VariableRowItem(varnam, thisl, true, "Iterations-Variable");

            scx = Method_CallByFilename.CallSub(varCol, scp2, new CurrentPosition("ForEachRow-Schleife", 0), cdf.CodeBlockAfterText, nv, null);
            if (scx.Failed || scx.BreakFired || scx.ReturnFired) { break; }
        }

        if (scx == null) { return new DoItFeedback(); }

        scx.ConsumeBreak();
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