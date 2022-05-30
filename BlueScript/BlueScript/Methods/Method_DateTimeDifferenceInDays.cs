// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.Converter;

namespace BlueScript.Methods;

internal class Method_DateTimeDifferenceInDays : Method {

    #region Properties

    public override List<List<string>> Args => new() { new() { VariableDateTime.ShortName_Variable }, new() { VariableDateTime.ShortName_Variable } };
    public override string Description => "Gibt die Differnz in Tagen der beiden Datums als Gleitkommazahl zurück.\rErgebnis = DateTimeString1 - DateTimeString2";
    public override bool EndlessArgs => false;
    public override string EndSequence => ")";
    public override bool GetCodeBlockAfter => false;
    public override string Returns => VariableFloat.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "DateTimeDifferenceInDays(DateTimeString1, DateTimeString2)";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "datetimedifferenceindays" };

    public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
        var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar); }

        var d1 = ((VariableDateTime)attvar.Attributes[0]).ValueDate;
        var d2 = ((VariableDateTime)attvar.Attributes[1]).ValueDate;
        return new DoItFeedback(d1.Subtract(d2).TotalDays);
    }

    #endregion
}