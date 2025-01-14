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

using System;
using System.Collections.Generic;
using System.Globalization;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.Converter;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
internal class Method_MaxDate : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, [VariableListString.ShortName_Plain, VariableString.ShortName_Plain]];
    public override string Command => "maxdate";
    public override List<string> Constants => [];
    public override string Description => "Gibt den den angegeben Werten den, mit dem höchsten Wert zurück.\r\nLeere Eingangswerte werden ignoriert.\r\nBeispiel für Format-String: dd.MM.yyyy HH:mm:ss.fff";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 2;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "MaxDate(FormatString, Value1, Value2, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var d = new DateTime(0);

        var l = new List<string>();

        for (var z = 1; z < attvar.Attributes.Count; z++) {
            if (attvar.Attributes[z] is VariableString vs) { l.Add(vs.ValueString); }
            if (attvar.Attributes[z] is VariableListString vl) { l.AddRange(vl.ValueList); }
        }

        foreach (var thisw in l) {
            if (!string.IsNullOrEmpty(thisw)) {
                var ok = DateTimeTryParse(thisw, out var da);

                if (!ok) {
                    return new DoItFeedback(ld, "Wert kann nicht als Datum interpretiert werden: " + thisw);
                }

                if (da.Subtract(d).TotalDays > 0) {
                    d = da;
                }
            }
        }

        try {
            return new DoItFeedback(d.ToString(attvar.ReadableText(0), CultureInfo.InvariantCulture));
        } catch {
            return new DoItFeedback(ld, "Der Umwandlungs-String '" + attvar.ReadableText(1) + "' ist fehlerhaft.");
        }
    }

    #endregion
}