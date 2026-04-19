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

using BlueBasics;
using BlueBasics.Classes;
using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueTable.Enums;
using BlueTable.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BlueTable.Classes;

public sealed class UniqueValueDefinition : IParseable, IEditable, IHasTable, IEquatable<UniqueValueDefinition>, IReadableTextWithKey {

    #region Fields

    private readonly List<ColumnItem> _internal = [];

    #endregion

    #region Constructors

    public UniqueValueDefinition(Table table, string toParse) {
        Table = table;
        this.Parse(toParse);
    }

    public UniqueValueDefinition(Table table, List<ColumnItem> columns) {
        Table = table;
        foreach (var thisColumn in columns) {
            if (thisColumn is { IsDisposed: false }) { _internal.Add(thisColumn); }
        }
    }

    #endregion

    #region Properties

    public string CaptionForEditor => "Unique-Wert-Definition";

    public ReadOnlyCollection<ColumnItem> KeyColumns => _internal.AsReadOnly();
    public bool KeyIsCaseSensitive => false;

    public string KeyName => _internal.Count == 0 ? "Leer" : string.Join(";", _internal.Select(x => x.KeyName));

    public string QuickInfo => string.Empty;
    public Table Table { get; }

    #endregion

    #region Methods

    public bool Equals(UniqueValueDefinition? other) {
        if (other == null) { return false; }
        return _internal.Select(x => x.KeyName).SequenceEqual(other._internal.Select(x => x.KeyName), StringComparer.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj) => Equals(obj as UniqueValueDefinition);

    public override int GetHashCode() {
        unchecked {
            var hash = 17;
            foreach (var item in _internal) {
                hash = hash * 23 + (item.KeyName?.GetHashCode(StringComparison.OrdinalIgnoreCase) ?? 0);
            }
            return hash;
        }
    }

    string IEditable.IsNowEditable() {
        if (Table is not { IsDisposed: false } tb) { return "Tabelle verworfen."; }
        return tb.GrantWriteAccess(TableDataType.UniqueValues);
    }

    public List<string> ParseableItems() {
        List<string> result = [];
        result.ParseableAdd("Columns", _internal, true);
        return result;
    }

    public void ParseFinished(string parsed) {}

    public bool ParseThis(string key, string value) {
        switch (key) {
            case "identifier":
                if (value != "UniqueValueDefinition") { Develop.DebugError("Identifier fehlerhaft: " + value); }
                return true;

            case "column":
            case "columnkey":
            case "columnname":
                if (Table.Column[value] is { } c) { _internal.Add(c); }
                return true;

            case "columns":
                var cols = value.FromNonCritical().SplitBy("|");
                foreach (var thisc in cols) {
                    if (Table.Column[thisc] is { } c2) { _internal.Add(c2); }
                }
                return true;
        }
        return false;
    }

    public string ReadableText() => _internal.Count == 0 ? "(leer)" : string.Join(";", _internal.Select(x => x.Caption));

    public void Repair() {
        if (_internal.Count == 0) { return; }
        if (Table is not { IsDisposed: false } tb) { return; }
        if (!string.IsNullOrEmpty(tb.IsValueEditable(TableDataType.UniqueValues, TableChunk.Chunk_Master))) { return; }

        for (var i = 0; i < _internal.Count; i++) {
            if (_internal[i] is not { IsDisposed: false }) {
                _internal.RemoveAt(i);
                Repair();
                return;
            }
        }

        if (tb.Column.ChunkValueColumn is { } cvc && !_internal.Contains(cvc)) {
            _internal.Add(cvc);
        }
    }

    public QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Schloss, 16);

    public override string ToString() => ParseableItems().FinishParseable();

    #endregion
}