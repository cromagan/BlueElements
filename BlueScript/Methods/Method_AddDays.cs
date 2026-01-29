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
using BlueScript.Enums;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Globalization;

namespace BlueScript.Methods;


internal class Method_AddDays : Method {

    #region Properties

    public override List<List<string>> Args => [StringVal, FloatVal, StringVal];
    public override string Command => "adddays";
    public override List<string> Constants => [.. BlueBasics.ClassesStatic.Constants.DateTimeFormats];
    public override string Description => "Fügt dem Datum die angegeben Anzahl Tage hinzu.\r\nDabei können auch Gleitkommazahlen benutzt werden, so werden z.B. bei 0.25 nur 6 Stunden hinzugefügt.\r\nDer Rückgabwert als String und wird mit 'Format' festgelegt.\r\nBeispiel: dd.MM.yyyy HH:mm:ss.fff";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Variable;
    public override string StartSequence => "(";
    public override string Syntax => "AddDays(DateTimeString, Days, Format)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var d = attvar.ValueDateGet(0);

        if (d == null) {
            return new DoItFeedback("Der Wert '" + attvar.ReadableText(0) + "' wurde nicht als Zeitformat erkannt.", true, ld);
        }

        var nd = d.Value.AddDays(attvar.ValueNumGet(1));

        try {
            return new DoItFeedback(nd.ToString(attvar.ReadableText(2), CultureInfo.InvariantCulture));
        } catch {
            return new DoItFeedback("Der Umwandlungs-String '" + attvar.ReadableText(2) + "' ist fehlerhaft.", true, ld);
        }
    }

    #endregion
}