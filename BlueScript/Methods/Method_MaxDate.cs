// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueScript.Classes;
using BlueScript.Variables;
using System;
using System.Collections.Generic;
using System.Globalization;
using static BlueBasics.ClassesStatic.Converter;

namespace BlueScript.Methods;


internal sealed class Method_MaxDate : Method {

    #region Properties

    public static List<List<string>> Args => [StringVal, [VariableListString.ShortName_Plain, VariableString.ShortName_Plain]];
    public static string Command => "maxdate";
    
    public static string Description => "Gibt den den angegeben Werten den, mit dem höchsten Wert zurück.\r\nLeere Eingangswerte werden ignoriert.\r\nBeispiel für Format-String: dd.MM.yyyy HH:mm:ss.fff";

    public static int LastArgMinCount => 2;

    public static bool MustUseReturnValue => true;
    public static string Returns => VariableString.ShortName_Plain;
   
    public static string Syntax => "MaxDate(FormatString, Value1, Value2, ...)";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
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
                    return new DoItFeedback("Wert kann nicht als Datum interpretiert werden: " + thisw, true, ld);
                }

                if (da.Subtract(d).TotalDays > 0) {
                    d = da;
                }
            }
        }

        try {
            return new DoItFeedback(d.ToString(attvar.ReadableText(0), CultureInfo.InvariantCulture));
        } catch {
            return new DoItFeedback("Der Umwandlungs-String '" + attvar.ReadableText(1) + "' ist fehlerhaft.", true, ld);
        }
    }

    #endregion
}