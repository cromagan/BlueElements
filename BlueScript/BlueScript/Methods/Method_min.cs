// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

#nullable enable

using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Linq;
using static BlueBasics.Converter;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
internal class Method_Min : Method {

    #region Properties

    public override List<List<string>> Args => [[VariableDouble.ShortName_Plain, VariableString.ShortName_Plain, VariableListString.ShortName_Plain]];
    public override string Command => "min";
    public override List<string> Constants => [];

    public override string Description => "Gibt den den angegeben Werten den, mit dem niedrigsten Wert zurück.\r\n" +
                                            "Ein Text wird - wenn möglich - als Zahl interpretiert.\r\n" +
                                            "Ist das nicht möglich, wird der Text ignoriert.";

    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => 1;
    public override MethodType MethodLevel => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableDouble.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "Min(Value1, Value2, ...)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
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