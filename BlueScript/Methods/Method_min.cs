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
using static BlueBasics.ClassesStatic.Converter;

namespace BlueScript.Methods;


internal sealed class Method_Min : Method {

    #region Properties

    public static List<List<string>> Args => [[VariableDouble.ShortName_Plain, VariableString.ShortName_Plain, VariableListString.ShortName_Plain]];
    public static string Command => "min";
    

    public static string Description => "Gibt den den angegeben Werten den, mit dem niedrigsten Wert zurück.\r\n" +
                                            "Ein Text wird - wenn möglich - als Zahl interpretiert.\r\n" +
                                            "Ist das nicht möglich, wird der Text ignoriert.";


    public static int LastArgMinCount => 1;

    public static bool MustUseReturnValue => true;
    public static string Returns => VariableDouble.ShortName_Plain;
   
    public static string Syntax => "Min(Value1, Value2, ...)";

    #endregion

    #region Methods

    public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        var l = new List<double>();
        foreach (var thisvar in attvar.Attributes) {
            switch (thisvar) {
                case VariableDouble vf:
                    l.Add(vf.ValueNum);
                    break;

                case VariableString vs:
                    if (DoubleTryParse(vs.ValueString, out var r)) {
                        l.Add(r);
                    }
                    break;

                case VariableListString vl:
                    foreach (var thiss in vl.ValueList) {
                        if (DoubleTryParse(thiss, out var r2)) {
                            l.Add(r2);
                        }
                    }
                    break;
            }
        }

        return l.Count > 0 ? new DoItFeedback(l.Min()) : new DoItFeedback("Keine gültigen Werte angekommen", true, ld);
    }

    #endregion
}