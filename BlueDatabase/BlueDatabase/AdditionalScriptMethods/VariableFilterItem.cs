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

using System;
using BlueDatabase;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.Interfaces.IParseableExtension;

namespace BlueScript;

public class VariableFilterItem : Variable, IDisposable {

    #region Fields

    private FilterItem _filter;

    #endregion

    #region Constructors

    public VariableFilterItem(FilterItem value) : this(DummyName(), value, true, false, string.Empty) { }

    public VariableFilterItem(string name) : this(name, null!, true, false, string.Empty) { }

    private VariableFilterItem(string name, FilterItem value, bool ronly, bool system, string comment) : base(name, ronly, system, comment) => _filter = value;

    #endregion

    #region Properties

    public static string ClassId => "fil";
    public static string ShortName_Variable => "*fil";
    public override int CheckOrder => 99;

    public FilterItem FilterItem {
        get => _filter;
        private set {
            if (ReadOnly) { return; }
            _filter = value;
        }
    }

    public override bool GetFromStringPossible => false;
    public override bool IsNullOrEmpty => _filter == null;
    public override bool MustDispose => true;
    public override string MyClassId => ClassId;
    public override bool ToStringPossible => false;

    #endregion

    #region Methods

    public override object Clone() {
        var v = new VariableFilterItem(KeyName);
        v.Parse(ToString());
        return v;
    }

    public void Dispose() {
        _filter.Dispose();
    }

    public override DoItFeedback GetValueFrom(Variable variable, LogData ld) {
        if (variable is not VariableFilterItem v) { return DoItFeedback.VerschiedeneTypen(ld, this, variable); }
        if (ReadOnly) { return DoItFeedback.Schreibgschützt(ld); }
        FilterItem = v.FilterItem;
        return DoItFeedback.Null();
    }

    protected override Variable? NewWithThisValue(object x) => null;

    protected override void SetValue(object? x) { }

    protected override object? TryParse(string txt, VariableCollection? vs, ScriptProperties? scp) => null;

    #endregion
}