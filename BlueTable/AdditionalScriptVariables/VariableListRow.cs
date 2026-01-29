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

using BlueScript.Variables;
using BlueTable.Classes;
using System.Collections.Generic;

namespace BlueTable.AdditionalScriptVariables;

public class VariableListRow : Variable {

    #region Fields

    public static readonly List<string> ListRowVar = [ShortName_Variable];
    private List<RowItem> _list;

    #endregion

    #region Constructors

    public VariableListRow(string name, IReadOnlyCollection<RowItem>? value, bool ronly, string comment) : base(name, ronly, comment) {
        _list = [];
        if (value != null) {
            _list.AddRange(value);
        }
    }

    public VariableListRow() : this(string.Empty, null, true, string.Empty) { }

    public VariableListRow(string name) : this(name, null, true, string.Empty) { }

    public VariableListRow(IReadOnlyCollection<RowItem>? value) : this(DummyName(), value, true, string.Empty) { }

    public VariableListRow(IEnumerable<RowItem> value) : this([.. value]) { }

    #endregion

    #region Properties

    public static string ClassId => "lsr";
    public static string ShortName_Plain => "lsr";
    public static string ShortName_Variable => "*lsr";
    public override int CheckOrder => 99;
    public override bool GetFromStringPossible => false;
    public override bool IsNullOrEmpty => _list.Count == 0;
    public override string SearchValue => ReadableText;
    public override bool ToStringPossible => false;
    public override string ValueForCell => string.Empty;

    public List<RowItem> ValueList {
        get => _list;
        set {
            if (ReadOnly) { return; }

            _list = value;
        }
    }

    #endregion

    #region Methods

    public override void DisposeContent() => _list.Clear();

    public override string GetValueFrom(Variable variable) {
        if (variable is not VariableListRow v) { return VerschiedeneTypen(variable); }
        if (ReadOnly) { return Schreibgschützt(); }
        ValueList = v.ValueList;
        return string.Empty;
    }

    protected override void SetValue(object? x) { }

    protected override bool TryParseValue(string txt, out object? result) {
        result = null;
        return false;
    }

    #endregion
}