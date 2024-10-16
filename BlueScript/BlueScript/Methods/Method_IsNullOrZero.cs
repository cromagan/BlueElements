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
using BlueBasics;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_IsNullOrZero : Method {

    #region Properties

    public override List<List<string>> Args => [[Variable.Any_Variable]];
    public override string Command => "isnullorzero";
    public override List<string> Constants => [];
    public override string Description => "Gibt TRUE zurück, wenn die Variable nicht existiert, fehlerhaft ist, keinen Inhalt hat, oder dem Zahlenwert 0 entspricht. Falls die Variable existiert, muss diese dem Typ Numeral entsprechen.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableBool.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "isNullOrZero(Variable)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, LastArgMinCount, infos.LogData, scp);

        if (attvar.Attributes.Count == 0) {
            if (attvar.FehlerTyp != ScriptIssueType.VariableNichtGefunden) {
                return DoItFeedback.AttributFehler(infos.LogData, this, attvar);
            }

            return DoItFeedback.Wahr();
        }

        var v = attvar.Attributes[0];
        if (v == null) { return DoItFeedback.InternerFehler(null); }

        if (v.IsNullOrEmpty) { return DoItFeedback.Wahr(); }
        if (v is VariableUnknown) { return DoItFeedback.Wahr(); }

        if (v is VariableFloat f) {
            if (f.ValueNum == 0) { return DoItFeedback.Wahr(); }
            return DoItFeedback.Falsch();
        }

        return new DoItFeedback(infos.LogData, "Variable existiert, ist aber nicht vom Datentyp Numeral.");
    }

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        // Dummy überschreibung.
        // Wird niemals aufgerufen, weil die andere DoIt Rourine überschrieben wurde.

        Develop.DebugPrint_NichtImplementiert(true);
        return DoItFeedback.Falsch();
    }

    #endregion
}