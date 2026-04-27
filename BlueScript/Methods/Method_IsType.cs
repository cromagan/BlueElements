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

namespace BlueScript.Methods;


internal sealed class Method_IsType : Method {

    #region Properties

    public static List<List<string>> Args => [[Variable.Any_Variable], StringVal];
    public static string Command => "istype";
    public static List<string> Constants => ["NUM", "LST", "STR", "BOL", "UKN"];
    public static string Description => "Prüft, ob der Variablenntyp dem hier angegeben Wert entspricht. Es wird keine Inhaltsprüfung ausgeführt!";

    public static int LastArgMinCount => -1;

    public static bool MustUseReturnValue => true;
    public static string Returns => VariableBool.ShortName_Plain;
    public static string StartSequence => "(";
    public static string Syntax => "isType(Variable, num/str/lst/bol/ukn)";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) => string.Equals(attvar.ReadableText(1), attvar.MyClassId(0), StringComparison.OrdinalIgnoreCase)
            ? DoItFeedback.Wahr()
            : DoItFeedback.Falsch();

    #endregion
}