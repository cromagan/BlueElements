// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using System.Collections.Generic;
using BlueBasics;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.Converter;

namespace BlueScript.Methods;

internal class Method_SortNum : Method {

    #region Properties

    public override List<List<string>> Args => new() { ListStringVar, FloatVal };
    public override string Description => "Sortiert die Liste. Der Zahlenwert wird verwendet wenn der String nicht in eine Zahl umgewandelt werden kann.";
    public override bool EndlessArgs => false;
    public override string EndSequence => ");";
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.Standard;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";

    public override string Syntax => "SortNum(ListVariable, Defaultwert);";

    #endregion

    #region Methods

    public override List<string> Comand(VariableCollection? currentvariables) => new() { "sortnum" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        var attvar = SplitAttributeToVars(s, infos.AttributText, Args, EndlessArgs, infos.Data);
        if (!string.IsNullOrEmpty(attvar.ErrorMessage)) { return DoItFeedback.AttributFehler(infos.Data, this, attvar); }

        if (attvar.Attributes[0].ReadOnly) { return DoItFeedback.Schreibgschützt(infos.Data); }

        var nums = new List<double>();
        foreach (var txt in ((VariableListString)attvar.Attributes[0]).ValueList) {
            nums.Add(txt.IsNumeral() ? DoubleParse(txt) : ((VariableFloat)attvar.Attributes[1]).ValueNum);
        }

        nums.Sort();

        ((VariableListString)attvar.Attributes[0]).ValueList = nums.ConvertAll(i => i.ToString(Constants.Format_Float1));
        return DoItFeedback.Null();
    }

    #endregion
}