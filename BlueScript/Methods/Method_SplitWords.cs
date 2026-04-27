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

using BlueBasics;
using BlueScript.Classes;
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Methods;


internal sealed class Method_SplitWords : Method {

    #region Properties

    public static List<List<string>> Args => [StringVal];
    public static string Command => "splitwords";
    
    public static string Description => "Gibt eine Liste aller Wörter zurück.\r\nDie Liste ist nach die Zeichen-Länge der Wörter absteigend sortiert.\r\nJedes Wort ist nur einmal in der Liste.";

    

    public static bool MustUseReturnValue => true;
    public static string Returns => VariableListString.ShortName_Plain;
   
    public static string Syntax => "SplitWords(String)";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var txt = attvar.ValueStringGet(0);

        var list = txt.AllWords().SortedDistinctList();

        list.Sort((s1, s2) => s2.Length.CompareTo(s1.Length));

        return new DoItFeedback(list);
    }

    #endregion
}