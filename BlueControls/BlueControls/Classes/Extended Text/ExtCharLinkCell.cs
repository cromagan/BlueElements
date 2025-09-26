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
using System.Drawing;

namespace BlueControls.Extended_Text;

public class ExtCharLinkCell : ExtChar {

    #region Fields

    private readonly string _displayText;
    private readonly string _htmlText;

    #endregion

    #region Constructors

    internal ExtCharLinkCell(ExtText parent, PadStyles style, BlueFont font, string tableName, string columnKey, string rowKey) : base(parent, style, font) {
        TableName = tableName;
        ColumnKey = columnKey;
        RowKey = rowKey;
        _htmlText = $"<LINKCELL tableName=\"{tableName}\" columnKey=\"{columnKey}\" rowKey=\"{rowKey}\"></LINKCELL>";
        _displayText = GetCellValue();
    }

    internal ExtCharLinkCell(ExtText parent, int styleFromPos, string tableName, string columnKey, string rowKey) : base(parent, styleFromPos) {
        TableName = tableName;
        ColumnKey = columnKey;
        RowKey = rowKey;
        _htmlText = $"<LINKCELL tableName=\"{tableName}\" columnKey=\"{columnKey}\" rowKey=\"{rowKey}\"></LINKCELL>";
        _displayText = GetCellValue();
    }

    #endregion

    #region Properties

    public string ColumnKey { get; }
    public string RowKey { get; }
    public string TableName { get; }

    #endregion

    #region Methods

    public override void Draw(Graphics gr, Point posModificator, float zoom) {
        if (string.IsNullOrEmpty(_displayText)) { return; }

        var drawX = (Pos.X * zoom) + posModificator.X;
        var drawY = (Pos.Y * zoom) + posModificator.Y;

        try {
            this.GetFont().DrawString(gr, _displayText, drawX, drawY, zoom, StringFormat.GenericTypographic);
        } catch { }
    }

    public override string HtmlText() => _htmlText;

    public override bool IsLineBreak() => false;

    public override bool IsPossibleLineBreak() => false;

    public override bool IsSpace() => false;

    public override bool IsWordSeparator() => false;

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

            var r = tb.Row[RowKey];
            if (r == null) { return $"[Row '{RowKey}' not found]"; }

            return r.CellGetString(c);
        } catch (System.Exception ex) {
            return $"[Error: {ex.Message}]";
        }
    }

    #endregion
}