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


internal sealed class Method_Round : Method {

    #region Properties

    public static List<List<string>> Args => [FloatVal, FloatVal];
    public static string Command => "round";
    public static List<string> Constants => [];
    public static string Description => "Rundet den Zahlenwert mathematisch korrekt.";

    public static int LastArgMinCount => -1;

    public static bool MustUseReturnValue => true;
    public static string Returns => VariableDouble.ShortName_Plain;
    public static string StartSequence => "(";
    public static string Syntax => "Round(Value, Nachkommastellen)";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var n = (int)attvar.ValueNumGet(1);
        if (n < 0) { n = 0; }
        if (n > 10) { n = 10; }
        var val = Math.Round(attvar.ValueNumGet(0), n, MidpointRounding.AwayFromZero);
        return new DoItFeedback(val);
    }

    #endregion
}