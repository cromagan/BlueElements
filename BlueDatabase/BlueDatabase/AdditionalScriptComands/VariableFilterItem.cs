// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

using BlueDatabase;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript;

public class VariableFilterItem : Variable {

    #region Fields

    private FilterItem _filter;

    #endregion

    #region Constructors

    public VariableFilterItem(string name, FilterItem value, bool ronly, bool system, string coment) : base(name, ronly, system, coment) => _filter = value;

    public VariableFilterItem(string name) : this(name, null!, true, false, string.Empty) { }

    public VariableFilterItem(FilterItem value) : this(DummyName(), value, true, false, string.Empty) { }

    #endregion

    #region Properties

    public static string ShortName_Variable => "*fil";
    public override int CheckOrder => 99;

    public FilterItem FilterItem {
        get => _filter;
        set {
            if (Readonly) { return; }
            _filter = value;
        }
    }

    public override bool GetFromStringPossible => false;
    public override bool IsNullOrEmpty => _filter == null;
    public override string ShortName => "fil";
    public override bool ToStringPossible => false;

    #endregion

    #region Methods

    public override DoItFeedback GetValueFrom(Variable variable) {
        if (variable is not VariableFilterItem v) { return DoItFeedback.VerschiedeneTypen(this, variable); }
        if (Readonly) { return DoItFeedback.Schreibgschützt(); }
        FilterItem = v.FilterItem;
        return DoItFeedback.Null();
    }

    protected override bool TryParse(string txt, out Variable? succesVar, Script s) {
        succesVar = null;
        return false;
    }

    #endregion
}