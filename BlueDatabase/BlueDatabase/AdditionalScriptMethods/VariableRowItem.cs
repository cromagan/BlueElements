﻿// Authors:
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

using BlueDatabase;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.Interfaces.ParseableExtension;

namespace BlueScript;

public class VariableRowItem : Variable {

    #region Fields

    private string _lastText = string.Empty;
    private RowItem? _row;

    #endregion

    #region Constructors

    public VariableRowItem(string name, RowItem? value, bool ronly, string comment) : base(name, ronly, comment) {
        _row = value;
        GetText();
    }

    public VariableRowItem(RowItem? value) : this(DummyName(), value, true, string.Empty) { }

    public VariableRowItem(string name) : this(name, null, true, string.Empty) { }

    #endregion

    #region Properties

    public static string ClassId => "row";

    public static string ShortName_Variable => "*row";

    public override int CheckOrder => 99;

    public override bool GetFromStringPossible => false;

    public override bool IsNullOrEmpty => _row == null;

    public override string MyClassId => ClassId;

    public override string ReadableText => _lastText;

    public RowItem? RowItem {
        get => _row;
        private set {
            if (ReadOnly) { return; }
            _row = value;

            GetText();
        }
    }

    private void GetText() {
        if (_row == null) {
            _lastText = "Row: [NULL]";
        } else {
            _lastText = "Row: " + _row.CellFirstString();
        }
    }

    public override bool ToStringPossible => false;

    #endregion

    #region Methods

    public override object Clone() {
        var v = new VariableRowItem(KeyName);
        v.Parse(ToString());
        return v;
    }

    public override void DisposeContent() => _row = null;

    public override DoItFeedback GetValueFrom(Variable variable, LogData ld) {
        if (variable is not VariableRowItem v) { return DoItFeedback.VerschiedeneTypen(ld, this, variable); }
        if (ReadOnly) { return DoItFeedback.Schreibgschützt(ld); }
        RowItem = v.RowItem;
        return DoItFeedback.Null();
    }

    protected override Variable? NewWithThisValue(object x) => null;

    protected override void SetValue(object? x) { }

    protected override object? TryParse(string txt, VariableCollection? vs, ScriptProperties? scp) => null;

    #endregion
}