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
using BlueControls.Classes;
using BlueTable.AdditionalScriptMethods;
using BlueTable.Classes;
using BlueTable.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.Extended_Text;

public class ExtCharCellLink : ExtChar {

    #region Fields

    private string _displayText = string.Empty;

    #endregion

    #region Constructors

    public ExtCharCellLink() {
    }

    internal ExtCharCellLink(ExtText parent, List<string> overrideTags, string tableName, string columnKey, string rowKey) : base(parent, overrideTags) {
        TableName = tableName;
        ColumnKey = columnKey;
        RowKey = rowKey;
        _displayText = ResolveDisplayText();
    }

    internal ExtCharCellLink(ExtText parent, int styleFromPos, string tableName, string columnKey, string rowKey) : base(parent, styleFromPos) {
        TableName = tableName;
        ColumnKey = columnKey;
        RowKey = rowKey;
        _displayText = ResolveDisplayText();
    }

    #endregion

    #region Properties

    public string CellValue { get; private set; } = string.Empty;
    public string ColumnKey { get; private set; } = string.Empty;
    public string RowKey { get; private set; } = string.Empty;
    public string TableName { get; private set; } = string.Empty;
    internal override string? StructuralTag => "CELLLINK";

    #endregion

    #region Methods

    public override void Draw(Graphics gr, Point controlPos, Size controlSize, float zoom) {
        if (string.IsNullOrEmpty(_displayText)) { return; }
        try {
            gr.FillRectangle(Brushes.LightGray, controlPos.X, controlPos.Y, controlSize.Width, controlSize.Height);
            Font?.DrawString(gr, _displayText, zoom, controlPos.X, controlPos.Y);
        } catch { }
    }

    public override string HtmlText() => Method_Linkify.GenerateHtmlCellLink(TableName, ColumnKey, RowKey, CellValue);

    public override bool IsLineBreak() => false;

    public override bool IsPossibleLineBreak() => false;

    public override bool IsSpace() => false;

    public override bool IsWordSeparator() => false;

    public override string PlainText() => _displayText;

    internal override void DrawWithFont(Graphics gr, Point controlPos, Size controlSize, float zoom, BlueFont font) {
        if (string.IsNullOrEmpty(_displayText)) { return; }
        try {
            gr.FillRectangle(Brushes.LightGray, controlPos.X, controlPos.Y, controlSize.Width, controlSize.Height);
            font.DrawString(gr, _displayText, zoom, controlPos.X, controlPos.Y);
        } catch { }
    }

    internal override void InitFromTag(ExtText parent, List<string> tags, string? attribut) {
        base.InitFromTag(parent, tags, attribut);
        var parts = (attribut + "|||").SplitBy("|");
        TableName = parts[0].FromNonCritical();
        ColumnKey = parts[1].FromNonCritical();
        var identifier = parts[2];
        if (identifier.StartsWith("val:", StringComparison.OrdinalIgnoreCase)) {
            var val = identifier[4..].Trim();
            if (val.IsEnclosedBy('\"', '\"')) { val = val[1..^1]; }
            CellValue = val.FromNonCritical();
            RowKey = ResolveRowKey();
        } else {
            RowKey = identifier;
        }
        _displayText = ResolveDisplayText();
    }

    protected override SizeF CalculateSizeCanvas() {
        if (Font == null) { return new SizeF(0, 16); }
        if (string.IsNullOrEmpty(_displayText)) { return Font.CharSize(0f); }
        return Font.MeasureString(_displayText);
    }

    private string ResolveDisplayText() {
        try {
            var tb = Table.Get(TableName, null);
            if (tb == null) { return !string.IsNullOrEmpty(CellValue) ? CellValue : $"[Table '{TableName}' not found]"; }

            var c = tb.Column[ColumnKey];
            if (c == null) { return !string.IsNullOrEmpty(CellValue) ? CellValue : $"[Column '{ColumnKey}' not found]"; }

            RowItem? r;
            if (!string.IsNullOrEmpty(CellValue) && string.IsNullOrEmpty(RowKey)) {
                r = tb.Row[new FilterItem(c, FilterType.Istgleich, CellValue)];
                if (r == null) { return CellValue; }
            } else {
                r = tb.Row.GetByKey(RowKey);
                if (r == null) { return !string.IsNullOrEmpty(CellValue) ? CellValue : $"[Row '{RowKey}' not found]"; }
            }

            return r.CellGetString(c);
        } catch (System.Exception ex) {
            return !string.IsNullOrEmpty(CellValue) ? CellValue : $"[Error: {ex.Message}]";
        }
    }

    private string ResolveRowKey() {
        if (!string.IsNullOrEmpty(RowKey)) { return RowKey; }
        if (string.IsNullOrEmpty(CellValue)) { return string.Empty; }
        try {
            var tb = Table.Get(TableName, null);
            if (tb is not { IsDisposed: false }) { return string.Empty; }
            var c = tb.Column[ColumnKey];
            if (c is not { IsDisposed: false }) { return string.Empty; }
            var found = tb.Row[new FilterItem(c, FilterType.Istgleich, CellValue)];
            return found?.KeyName ?? string.Empty;
        } catch { }
        return string.Empty;
    }

    #endregion
}