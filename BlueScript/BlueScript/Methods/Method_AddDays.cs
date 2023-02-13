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

using System.Collections.Generic;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.Constants;

namespace BlueScript.Methods;

internal class Method_AddDays : Method {

    #region Properties

    public override List<List<string>> Args => new() { new List<string> { VariableDateTime.ShortName_Variable }, new List<string> { VariableFloat.ShortName_Plain } };
    public override string Description => "Fügt dem Datum die angegeben Anzahl Tage hinzu. Dabei können auch Gleitkommazahlen benutzt werden, so werden z.B. bei 0.25 nur 6 Stunden hinzugefügt. Der Rückgabwert erfolgt immer im Format " + Format_Date7;
    public override bool EndlessArgs => false;
    public override string EndSequence => ")";
    public override bool GetCodeBlockAfter => false;
    public override string Returns => VariableDateTime.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "AddDays(DateTimeString, Days)";

    #endregion

    #region Methods

    public override List<string>Comand(List<Variable>? currentvariables) => new() { "adddays" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(s, infos, this, attvar); }
        // var ok = DateTimeTryParse(attvar.Attributes[0].ReadableText, out var d);
        //if (!ok) {
        //    return new DoItFeedback(infos, s, "Der Wert '" + attvar.Attributes[0].ReadableText + "' wurde nicht als Zeitformat erkannt.");
        //}
        var d = ((VariableDateTime)attvar.Attributes[0]).ValueDate;
        d = d.AddDays(((VariableFloat)attvar.Attributes[1]).ValueNum);
        return new DoItFeedback(s, infos, d);
    }

    #endregion
}