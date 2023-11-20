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
using static BlueBasics.Constants;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_AddDays : Method {

    #region Properties

    public override List<List<string>> Args => new() { DateTimeVar, FloatVal };
    public override string Command => "adddays";
    public override string Description => "Fügt dem Datum die angegeben Anzahl Tage hinzu. Dabei können auch Gleitkommazahlen benutzt werden, so werden z.B. bei 0.25 nur 6 Stunden hinzugefügt. Der Rückgabwert erfolgt immer im Format " + Format_Date7;
    public override bool EndlessArgs => false;
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.Standard;
    public override string Returns => VariableDateTime.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "AddDays(DateTimeString, Days)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, EndlessArgs, infos.Data, scp);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }
        // var ok = DateTimeTryParse(attvar.ReadableText(0), out var d);
        //if (!ok) {
        //    return new DoItFeedback(infos.LogData, s, "Der Wert '" + attvar.ReadableText(0) + "' wurde nicht als Zeitformat erkannt.");
        //}
        var d = attvar.ValueDateGet(0);
        d = d.AddDays(attvar.ValueNumGet(1));
        return new DoItFeedback(d);
    }

    #endregion
}