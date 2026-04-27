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

namespace BlueScript.Methods;


internal sealed class Method_DateTimeNowUTC : Method {

    #region Properties

    public static List<List<string>> Args => [StringVal];
    public static string Command => "datetimeutcnow";
    public static List<string> Constants => [.. BlueBasics.ClassesStatic.Constants.DateTimeFormats];
    public static string Description => "Gibt die akutelle UTC-Uhrzeit im angegebenen Format (z.B. dd.MM.yyyy HH:mm:ss.fff) zurück. ";

    public static int LastArgMinCount => -1;

    public static bool MustUseReturnValue => true;
    public static string Returns => VariableString.ShortName_Plain;
    public static string StartSequence => "(";
    public static string Syntax => "DateTimeUTCNow(format)";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        try {
            return new DoItFeedback(DateTime.UtcNow.ToString(attvar.ReadableText(0), CultureInfo.InvariantCulture));
        } catch {
            return new DoItFeedback("Der Umwandlungs-String '" + attvar.ReadableText(0) + "' ist fehlerhaft.", true, ld);
        }
    }

    #endregion
}