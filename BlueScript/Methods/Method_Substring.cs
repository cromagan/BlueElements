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
using System.Collections.Generic;

namespace BlueScript.Methods;


internal sealed class Method_Substring : Method {

    #region Properties

    public static List<List<string>> Args => [StringVal, FloatVal, FloatVal];
    public static string Command => "substring";
    
    public static string Description => "Gibt einen Teilstring zurück. Ist der Start oder das Ende keine gültige Position, wird das bestmögliche zurückgegeben und kein Fehler ausgelöst. Subrtring(\"Hallo\", 2,2) gibt ll zurück.";

    

    public static bool MustUseReturnValue => true;
    public static string Returns => VariableString.ShortName_Plain;
   
    public static string Syntax => "Substring(String, Start, Anzahl)";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var st = attvar.ValueIntGet(1);
        var en = attvar.ValueIntGet(2);
        if (st < 0) {
            en += st;
            st = 0;
        }

        var t = attvar.ValueStringGet(0);

        if (st > t.Length) { return DoItFeedback.Null(); }

        if (st + en > t.Length) { en = t.Length - st; }
        return new DoItFeedback(t.Substring(st, en));
    }

    #endregion
}