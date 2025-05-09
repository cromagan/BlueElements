﻿// Authors:
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

using BlueBasics;
using BlueBasics.Enums;
using BlueDatabase;
using BlueScript.Structures;
using BlueScript.Variables;

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

    public VariableRowItem() : this(string.Empty, null, true, string.Empty) { }

    public VariableRowItem(RowItem? value) : this(DummyName(), value, true, string.Empty) { }

    public VariableRowItem(string name) : this(name, null, true, string.Empty) { }

    #endregion

    #region Properties

    public static string ClassId => "row";
    public static string ShortName_Variable => "*row";
    public override int CheckOrder => 99;
    public override bool GetFromStringPossible => true;
    public override bool IsNullOrEmpty => _row == null;

    /// <summary>
    /// Gibt den Text "Row: CellFirstString" zurück.
    /// </summary>
    public override string ReadableText => _lastText;

    public RowItem? RowItem {
        get => _row;
        private set {
            if (ReadOnly) { return; }
            _row = value;

            GetText();
        }
    }

    public override string SearchValue => ReadableText;

    public override bool ToStringPossible => true;

    public override string ValueForReplace => _row is null || _row.Database is not { IsDisposed: false } db ? "{ROW:?}" : "{ROW:" + db.TableName + ";" + _row.KeyName + "}";

    #endregion

    #region Methods

    public override void DisposeContent() => _row = null;

    public override DoItFeedback GetValueFrom(Variable variable, LogData ld) {
        if (variable is not VariableRowItem v) { return DoItFeedback.VerschiedeneTypen(ld, this, variable); }
        if (ReadOnly) { return DoItFeedback.Schreibgschützt(ld); }
        RowItem = v.RowItem;
        return DoItFeedback.Null();
    }

    protected override void SetValue(object? x) {
        if (x is null) {
            _row = null;
        } else if (x is RowItem r) {
            _row = r;
        } else {
            Develop.DebugPrint(ErrorType.Error, "Variablenfehler!");
        }
    }

    protected override (bool cando, object? result) TryParse(string txt, VariableCollection? vs, ScriptProperties? scp) {
        if (txt.Length > 6 && txt.StartsWith("{ROW:") && txt.EndsWith("}")) {
            var t = txt.Substring(5, txt.Length - 6);

            if (t == "?") { return (true, null); }

            var tx = t.SplitBy(";");

            if (tx.Length != 2) { return (false, null); }

            var db = Database.Get(tx[0], false, null);

            if (db is null || db.IsDisposed) { return (false, null); }

            var row = db.Row.SearchByKey(tx[1]);

            return row is null || row.IsDisposed ? ((bool cando, object? result))(false, null) : ((bool cando, object? result))(true, row);
        }

        return (false, null);
    }

    private void GetText() => _lastText = _row == null ? "Row: [NULL]" : "Row: " + _row.CellFirstString();

    #endregion
}