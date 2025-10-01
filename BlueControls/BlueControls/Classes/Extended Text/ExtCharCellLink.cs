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

using BlueBasics;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueTable;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.Extended_Text;

public class ExtCharCellLink : ExtChar {

    #region Fields

    private string _displayText;
    private string _htmlText;

    #endregion

    #region Constructors

    public ExtCharCellLink(ExtText parent, int styleFromPos) : base(parent, styleFromPos) { }

    internal ExtCharCellLink(ExtText parent, PadStyles style, BlueFont font, string tableName, string columnKey, string rowKey) : base(parent, style, font) {
        TableName = tableName;
        ColumnKey = columnKey;
        RowKey = rowKey;
        InitValues();
    }

    internal ExtCharCellLink(ExtText parent, int styleFromPos, string tableName, string columnKey, string rowKey) : base(parent, styleFromPos) {
        TableName = tableName;
        ColumnKey = columnKey;
        RowKey = rowKey;
        InitValues();
    }

    #endregion

    #region Properties

    public static string ClassId => "ExtCharCellLink";
    public string ColumnKey { get; private set; }

    public string RowKey { get; private set; }

    public string TableName { get; private set; }

    #endregion

    #region Methods

    public override void Draw(Graphics gr, Point posModificator, float zoom) {
        if (string.IsNullOrEmpty(_displayText)) { return; }

        var drawX = (Pos.X * zoom) + posModificator.X;
        var drawY = (Pos.Y * zoom) + posModificator.Y;

        try {
            this.GetFont().DrawString(gr, _displayText, drawX, drawY, zoom);
        } catch { }
    }

    public override string HtmlText() => _htmlText;

    public override bool IsLineBreak() => false;

    public override bool IsPossibleLineBreak() => false;

    public override bool IsSpace() => false;

    public override bool IsWordSeparator() => false;

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("Table", TableName);
        result.ParseableAdd("Column", ColumnKey);
        result.ParseableAdd("Row", RowKey);
        return result;
    }

    public override void ParseFinished(string parsed) {
        base.ParseFinished(parsed);
        InitValues();
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "table":
                TableName = value.FromNonCritical();
                return true;

            case "column":
                ColumnKey = value.FromNonCritical();
                return true;

            case "row":
                RowKey = value.FromNonCritical();
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string PlainText() => _displayText;

    protected override SizeF CalculateSize() {
        if (Font == null) { return new SizeF(0, 16); }
        if (string.IsNullOrEmpty(_displayText)) { return Font.CharSize(0f); }

        return Font.MeasureString(_displayText);
    }

    private string GetCellValue() {
        try {
            var tb = Table.Get(TableName, null);
            if (tb == null) { return $"[Table '{TableName}' not found]"; }

            var c = tb.Column[ColumnKey];
            if (c == null) { return $"[Column '{ColumnKey}' not found]"; }

            var r = tb.Row.SearchByKey(RowKey);
            if (r == null) { return $"[Row '{RowKey}' not found]"; }

            return r.CellGetString(c);
        } catch (System.Exception ex) {
            return $"[Error: {ex.Message}]";
        }
    }

    private void InitValues() {
        _htmlText = $"<CELLLINK={TableName}|{ColumnKey}|{RowKey}>";
        _displayText = GetCellValue();
    }

    #endregion
}