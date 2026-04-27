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
using System.Linq;
using static BlueBasics.Extensions;

namespace BlueScript.Methods;


internal sealed class Method_CountString : Method {

    #region Properties

    public static List<List<string>> Args => [[VariableString.ShortName_Variable, VariableListString.ShortName_Variable], StringVal];
    public static string Command => "countstring";
    

    public static string Description => "Ist das erste Argument ein Text, wird gezählt, wie oft der Suchstring im Text vorkommt.\r\n" +
        "Ist es eine Liste, wird gezählt, wie oft ein Listeneintrag dem Text entspricht.\r\n" +
        "Achtung: Groß/Kleinschreibung wird beachtet!";


    

    public static bool MustUseReturnValue => true;
    public static string Returns => VariableDouble.ShortName_Plain;
   
    public static string Syntax => "CountString(Text/Liste, Suchstring)";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        switch (attvar.Attributes[0]) {
            case VariableString vs:
                return new DoItFeedback(vs.ValueString.CountString(attvar.ValueStringGet(1)));

            case VariableListString vl:
                return new DoItFeedback(vl.ValueList.Count(s => s == attvar.ReadableText(1)));
        }

        return DoItFeedback.InternerFehler(ld);
    }

    #endregion
}