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

using BlueBasics.Interfaces;
using BlueScript.Variables;
using BlueTable;

namespace BlueScript;

public class VariableFilterItem : Variable {

    #region Fields

    private FilterItem? _filter;
    private string _lastText = string.Empty;

    #endregion

    #region Constructors

    public VariableFilterItem(FilterItem value) : this(DummyName(), value, true, string.Empty) { }

    public VariableFilterItem() : this(string.Empty, null!, true, string.Empty) { }

    public VariableFilterItem(string name) : this(name, null!, true, string.Empty) { }

    public VariableFilterItem(string name, FilterItem value, bool ronly, string comment) : base(name, ronly, comment) {
        _filter = value;
        GetText();
    }

    #endregion

    #region Properties

    public static string ClassId => "fil";
    public static string ShortName_Variable => "*fil";
    public override int CheckOrder => 99;

    public FilterItem? FilterItem {
        get => _filter;
        private set {
            if (ReadOnly) { return; }
            _filter = value;
            GetText();
        }
    }

    public override bool GetFromStringPossible => false;
    public override bool IsNullOrEmpty => _filter == null || !_filter.IsOk();
    public override string ReadableText => _lastText;
    public override string SearchValue => ReadableText;
    public override bool ToStringPossible => false;
    public override string ValueForCell => string.Empty;

    #endregion

    #region Methods

    public override void DisposeContent() => _filter = null;

    public override string GetValueFrom(Variable variable) {
        if (variable is not VariableFilterItem v) { return VerschiedeneTypen(variable); }
        if (ReadOnly) { return Schreibgschützt(); }
        FilterItem = v.FilterItem;
        return string.Empty;
    }

    protected override void SetValue(object? x) { }

    protected override bool TryParseValue(string txt, out object? result) {
        result = null;
        return false;
    }

    private void GetText() => _lastText = _filter == null || !_filter.IsOk() ? "Filter: [ERROR]" : "Filter: " + _filter.ReadableText();

    #endregion
}