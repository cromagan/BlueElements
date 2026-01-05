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

using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Globalization;

namespace BlueScript.Methods;


internal class Method_ChangeDateTimeFormat : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, StringVal];
    public override string Command => "changedatetimeformat";
    public override List<string> Constants => [.. BlueBasics.Constants.DateTimeFormats];
    public override string Description => "Wandelt eine Zeitangabe-String in einen andern String um, der mittels des zweiten String definiert ist.\rBeispiel eines solchen Strings:  dd.MM.yyyy HH:mm:ss.fff\rAchtung: Groß-Kleinschreibung ist wichtig!";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "ChangeDateTimeFormat(DateTimeString, string)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var d = attvar.ValueDateGet(0);

        if (d == null) {
            return new DoItFeedback("Der Wert '" + attvar.ReadableText(0) + "' wurde nicht als Zeitformat erkannt.", true, ld);
        }

        try {
            return new DoItFeedback(d.Value.ToString(attvar.ReadableText(1), CultureInfo.InvariantCulture));
        } catch {
            return new DoItFeedback("Der Umwandlungs-String '" + attvar.ReadableText(1) + "' ist fehlerhaft.", true, ld);
        }
    }

    #endregion
}