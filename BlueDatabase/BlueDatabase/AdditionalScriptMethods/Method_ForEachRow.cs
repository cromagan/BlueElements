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

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, LastArgMinCount, infos.LogData, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.LogData, this, attvar); }

        var varnam = "value";

        if (attvar.Attributes[0] is VariableUnknown vkn) { varnam = vkn.Value; }

        if (!Variable.IsValidName(varnam)) { return new DoItFeedback(infos.LogData, varnam + " ist kein gültiger Variablen-Name"); }

        var vari = varCol.Get(varnam);
        if (vari != null) {
            return new DoItFeedback(infos.LogData, "Variable " + varnam + " ist bereits vorhanden.");
        }

        var (allFi, errorreason) = Method_Filter.ObjectToFilter(attvar.Attributes, 1, MyDatabase(scp), scp.ScriptName, true);

        if (allFi == null || !string.IsNullOrEmpty(errorreason)) { return new DoItFeedback(infos.LogData, $"Filter-Fehler: {errorreason}"); }

        var r = allFi.Rows;
        allFi.Dispose();

        var scx = new DoItFeedback(false, false, string.Empty);
        var scp2 = new ScriptProperties(scp, [.. scp.AllowedMethods, Method_Break.Method], scp.Stufe + 1, scp.Chain);

        foreach (var thisl in r) {
            var nv = new VariableRowItem(varnam, thisl, true, "Iterations-Variable");

            scx = Method_CallByFilename.CallSub(varCol, scp2, infos.LogData, "ForEachRow-Schleife", infos.CodeBlockAfterText, false, infos.LogData.Line - 1, infos.LogData.Subname, nv, null, "ForEachRow");
            if (!scx.AllOk) { return scx; }

            if (scx.BreakFired || scx.EndScript || !scx.Succesful) { break; }
        }

        return new DoItFeedback(false, scx.EndScript, scx.NotSuccesfulReason); // Du muss die Breaks konsumieren, aber EndSkript muss weitergegeben werden
    }

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        // Dummy überschreibung.
        // Wird niemals aufgerufen, weil die andere DoIt Rourine überschrieben wurde.

        Develop.DebugPrint_NichtImplementiert(true);
        return DoItFeedback.Falsch();
    }

    #endregion
}