﻿// Authors:
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

#nullable enable

using System.Collections.Generic;
using BlueScript.Structures;
using static BlueBasics.Constants;
using BlueScript.Variables;

namespace BlueScript.Methods;

internal class Method_DateTimeToString : Method {

    #region Properties

    public override List<List<string>> Args => new() { new() { VariableDateTime.ShortName_Variable }, new() { VariableString.ShortName_Plain } };

    public override string Description => "Wandelt eine Zeitangabe in einen String um, der mittels des zweiten String definiert ist.\rBeispiel eines solchen Strings:  " + Format_Date7 + "\rAchtung: Groß-Kleinschreibung ist wichtig!";
    public override bool EndlessArgs => false;
    public override string EndSequence => ")";
    public override bool GetCodeBlockAfter => false;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "DateTimeToString(DateTime, string)";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "datetimetostring" };

    public override DoItFeedback DoIt(CanDoFeedback infos, Script s) {
        var attvar = SplitAttributeToVars(infos.AttributText, s, Args, EndlessArgs);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(this, attvar); }
        //var ok = DateTimeTryParse(attvar.Attributes[0].ReadableText, out var d);
        //if (!ok) {
        //    return new DoItFeedback("Der Wert '" + attvar.Attributes[0].ReadableText + "' wurde nicht als Zeitformat erkannt.");
        //}
        //if (string.IsNullOrEmpty(d.ToString(attvar.Attributes[1].ReadableText))) {
        //    return new DoItFeedback("Kein Unwandlungs-String erhalten.");
        //}
        var d = ((VariableDateTime)attvar.Attributes[0]).ValueDate;

        try {
            return new DoItFeedback(d.ToString(attvar.Attributes[1].ReadableText), string.Empty);
        } catch {
            return new DoItFeedback("Der Umwandlungs-String '" + attvar.Attributes[1].ReadableText + "' ist fehlerhaft.");
        }
    }

    #endregion
}