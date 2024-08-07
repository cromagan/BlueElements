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

using BlueBasics.Enums;
using BlueBasics;
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
    public override bool GetFromStringPossible => true;
    public override bool IsNullOrEmpty => _row == null;
    public override string MyClassId => ClassId;

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

    public override string ValueForReplace {
        get {
            if (_row is null || _row.Database is not Database db) {
                return "{ROW:?}";
            }

            return "{ROW:" + db.TableName + ";" + _row.KeyName + "}";
        }
    }

    #endregion

    #region Methods

    public override object Clone() {
        var v = new VariableRowItem(KeyName);
        v.Parse(ToParseableString());
        return v;
    }

    public override void DisposeContent() => _row = null;

    public override DoItFeedback GetValueFrom(Variable variable, LogData ld) {
        if (variable is not VariableRowItem v) { return DoItFeedback.VerschiedeneTypen(ld, this, variable); }
        if (ReadOnly) { return DoItFeedback.Schreibgschützt(ld); }
        RowItem = v.RowItem;
        return DoItFeedback.Null();
    }

    protected override Variable NewWithThisValue(object? x) {
        var v = new VariableRowItem(string.Empty);
        v.SetValue(x);
        return v;
    }

    protected override void SetValue(object? x) {
        if (x is null) {
            _row = null;
        } else if (x is RowItem r) {
            _row = r;
        } else {
            Develop.DebugPrint(FehlerArt.Fehler, "Variablenfehler!");
        }
    }

    protected override (bool cando, object? result) TryParse(string txt, VariableCollection? vs, ScriptProperties? scp) {
        if (txt.Length > 6 && txt.StartsWith("{ROW:") && txt.EndsWith("}")) {
            var t = txt.Substring(5, txt.Length - 6);

            if (t == "?") { return (true, null); }

            var tx = t.SplitBy(";");

            if (tx.Length != 2) { return (false, null); }

            var db = Database.GetByFilename(tx[0], false, null, true, string.Empty);

            if (db is null || db.IsDisposed) { return (false, null); }

            var row = db.Row.SearchByKey(tx[1]);

            if (row is null || row.IsDisposed) { return (false, null); }

            return (true, row);
        }

        return (false, null);
    }

    private void GetText() {
        if (_row == null) {
            _lastText = "Row: [NULL]";
        } else {
            _lastText = "Row: " + _row.CellFirstString();
        }
    }

    #endregion
}