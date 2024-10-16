// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
using System.Diagnostics.CodeAnalysis;
using BlueBasics;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_Sort : Method {

    #region Properties

    public override List<List<string>> Args => [ListStringVar, BoolVal];
    public override string Command => "sort";
    public override List<string> Constants => [];
    public override string Description => "Sortiert die Liste. Falls das zweite Attribut TRUE ist, werden Doubletten und leere Einträge entfernt.";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => false;
    public override string Returns => string.Empty;
    public override string StartSequence => "(";
    public override string Syntax => "Sort(ListVariable, EliminateDupes);";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.ReadOnly(0)) { return DoItFeedback.Schreibgschützt(ld); }

        var x = attvar.ValueListStringGet(0);
        if (attvar.ValueBoolGet(1)) {
            x = x.SortedDistinctList();
        } else {
            x.Sort();
        }

        if (attvar.Attributes[0] is not VariableListString vli) {
            return DoItFeedback.AttributFehler(ld, this, attvar);
        }

        vli.ValueList = x;
        return DoItFeedback.Null();
    }

    #endregion
}