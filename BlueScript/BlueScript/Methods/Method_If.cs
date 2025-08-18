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
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;

// ReSharper disable once ClassNeverInstantiated.Global
public class Method_If : Method {

    #region Fields

    public static readonly List<string> OderOder = ["||"];
    public static readonly List<string> UndUnd = ["&&"];

    /// <summary>
    /// Vergleichsopeatoren in der richtigen Rang-Reihenfolge
    /// https://de.wikipedia.org/wiki/Operatorrangfolge
    /// </summary>
    public static readonly List<string> VergleichsOperatoren = ["==", "!=", ">=", "<=", "<", ">", "!", "&&", "||"];

    #endregion

    #region Properties

    public override List<List<string>> Args => [BoolVal];
    public override string Command => "if";
    public override List<string> Constants => [];
    public override string Description => "Nur wenn der Wert in der Klammer TRUE ist, wird der nachfolgende Codeblock ausgeführt. Es werden IMMER alle Vergleichsoperatoren aufgelöst. Deswegen sind Verschachtelungen mit Voricht zu verwenden - z.B. mir einem Exists-Befehl.";
    public override bool GetCodeBlockAfter => true;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "if (true) { Code zum Ausführen }";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var m = new List<Method>();
        foreach (var thism in scp.AllowedMethods) {
            if (!thism.MethodType.HasFlag(MethodType.SpecialVariables)) {
                m.Add(thism);
            }
        }

        var scpt = new ScriptProperties(scp, m, scp.Stufe + 1, scp.Chain);

        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, LastArgMinCount, infos.LogData, scpt);
        if (attvar.Failed) { return new DoItFeedback("Fehler innerhalb der runden Klammern des If-Befehls", true, infos.LogData); }

        if (attvar.ValueBoolGet(0)) {
            var scx = Method_CallByFilename.CallSub(varCol, scp, "If-Befehl-Inhalt", infos.CodeBlockAfterText, infos.LogData.Line - 1, infos.LogData.Subname, null, null, "If", infos.LogData);
            return scx; // If muss die Breaks und Endsripts erhalten!
        }

        return DoItFeedback.Null();
    }

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        // Dummy überschreibung.
        // Wird niemals aufgerufen, weil die andere DoIt Rourine überschrieben wurde.

        Develop.DebugPrint_NichtImplementiert(true);
        return DoItFeedback.Falsch();
    }

    #endregion
}