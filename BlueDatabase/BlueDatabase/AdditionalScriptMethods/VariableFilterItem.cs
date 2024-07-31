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

using BlueBasics.Interfaces;
using BlueDatabase;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript;

public class VariableFilterItem : Variable {

    #region Fields

    private FilterItem? _filter;

    private string _lastText = string.Empty;

    #endregion

    #region Constructors

    public VariableFilterItem(FilterItem value) : this(DummyName(), value, true, string.Empty) { }

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

    public override string MyClassId => ClassId;

    public override string ReadableText => _lastText;
    public override string SearchValue => ReadableText;
    public override bool ToStringPossible => false;

    #endregion

    #region Methods

    public override object Clone() {
        var v = new VariableFilterItem(KeyName);
        v.Parse(ToParseableString());
        return v;
    }

    public override void DisposeContent() {
        _filter?.Dispose();
        _filter = null;
    }

    public override DoItFeedback GetValueFrom(Variable variable, LogData ld) {
        if (variable is not VariableFilterItem v) { return DoItFeedback.VerschiedeneTypen(ld, this, variable); }
        if (ReadOnly) { return DoItFeedback.Schreibgschützt(ld); }
        FilterItem = v.FilterItem;
        return DoItFeedback.Null();
    }

    protected override Variable? NewWithThisValue(object? x) => null;

    protected override void SetValue(object? x) { }

    protected override (bool cando, object? result) TryParse(string txt, VariableCollection? vs, ScriptProperties? scp) => (false, null);

    private void GetText() {
        if (_filter == null || !_filter.IsOk()) {
            _lastText = "Filter: [ERROR]";
        } else {
            _lastText = "Filter: " + _filter.ReadableText();
        }
    }

    #endregion
}