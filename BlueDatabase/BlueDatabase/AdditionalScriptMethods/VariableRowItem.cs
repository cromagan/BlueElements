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

using System;
using BlueDatabase;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.Interfaces.ParseableExtension;

namespace BlueScript;

public class VariableRowItem : Variable, IDisposable {

    #region Fields

    private RowItem? _row;

    #endregion

    #region Constructors

    public VariableRowItem(string name, RowItem? value, bool ronly, bool system, string comment) : base(name, ronly, system, comment) => _row = value;

    public VariableRowItem(RowItem? value) : this(DummyName(), value, true, false, string.Empty) { }

    public VariableRowItem(string name) : this(name, null, true, false, string.Empty) { }

    #endregion

    #region Properties

    public static string ClassId => "row";
    public static string ShortName_Variable => "*row";
    public override int CheckOrder => 99;
    public override bool GetFromStringPossible => false;
    public override bool IsNullOrEmpty => _row == null;
    public override bool MustDispose => true;
    public override string MyClassId => ClassId;

    public RowItem? RowItem {
        get => _row;
        private set {
            if (ReadOnly) { return; }
            _row = value;
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

    public void Dispose() {
        _row = null;
    }

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