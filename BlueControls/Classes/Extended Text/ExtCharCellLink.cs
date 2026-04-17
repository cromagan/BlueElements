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
using BlueBasics.Interfaces;
using BlueControls.Classes;
using BlueTable.AdditionalScriptMethods;
using BlueTable.Classes;
using BlueTable.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.Extended_Text;

public class ExtCharCellLink : ExtChar, IParseable {

    #region Fields

    private readonly List<ExtCharAscii> _chars = [];
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
        if (_chars.Count == 0) { return; }
        try {
            var first = _chars[0];
            var last = _chars[^1];
            var bgW = (last.PosCanvas.X + last.SizeCanvas.Width - first.PosCanvas.X).CanvasToControl(zoom);
            var bgH = first.SizeCanvas.Height.CanvasToControl(zoom);
            gr.FillRectangle(Brushes.LightGray, controlPos.X, controlPos.Y, bgW, bgH);
        } catch { }
    }

    public override string HtmlText() => Method_Linkify.GenerateHtmlCellLink(TableName, ColumnKey, RowKey, CellValue);

    public override bool IsLineBreak() => false;

    public override bool IsPossibleLineBreak() => false;

    public override bool IsSpace() => false;

    public override bool IsWordSeparator() => false;

    public override string PlainText() => _displayText;

    public List<string> ParseableItems() {
        List<string> result = [];
        result.ParseableAdd("Table", TableName);
        result.ParseableAdd("Column", ColumnKey);
        result.ParseableAdd("Row", RowKey);
        if (!string.IsNullOrEmpty(CellValue)) {
            result.ParseableAdd("Alt", CellValue);
        }
        return result;
    }

    public void ParseFinished(string parsed) {
        if (!string.IsNullOrEmpty(CellValue) && string.IsNullOrEmpty(RowKey)) {
            RowKey = ResolveRowKey();
        }
        _displayText = ResolveDisplayText();
        BuildChars();
    }

    public bool ParseThis(string key, string value) {
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

            case "alt":
                CellValue = value.FromNonCritical();
                return true;

            default:
                return false;
        }
    }

    internal override void DrawWithFont(Graphics gr, Point controlPos, Size controlSize, float zoom, BlueFont font) {
        if (_chars.Count == 0) { return; }
        try {
            var first = _chars[0];
            var last = _chars[^1];
            var bgW = (last.PosCanvas.X + last.SizeCanvas.Width - first.PosCanvas.X).CanvasToControl(zoom);
            var bgH = first.SizeCanvas.Height.CanvasToControl(zoom);
            gr.FillRectangle(Brushes.LightGray, controlPos.X, controlPos.Y, bgW, bgH);
        } catch { }
    }

    internal override IEnumerable<ExtChar> GetChars() {
        if (_chars.Count == 0 && !string.IsNullOrEmpty(_displayText)) {
            BuildChars();
        }
        foreach (var c in _chars) {
            yield return c;
        }
    }

    internal override void InitFromTag(ExtText parent, List<string> tags, string? attribut) {
        base.InitFromTag(parent, tags, attribut);

        if (attribut != null && attribut.Contains('|')) {
            var t = attribut.Split('|');
            attribut = Method_Linkify.GenerateHtmlCellLink(t[0], t[1], t[2], string.Empty)[10..^1];
        }

        this.Parse(attribut ?? string.Empty, '\0', '\0', ' ');
    }

    private void BuildChars() {
        _chars.Clear();
        if (string.IsNullOrEmpty(_displayText)) { return; }
        foreach (var c in _displayText) {
            _chars.Add(new ExtCharAscii(_parent, OverrideTags, c));
        }
    }

    protected override SizeF CalculateSizeCanvas() {
        if (_chars.Count == 0) { return SizeF.Empty; }
        float w = 0;
        float h = 0;
        foreach (var c in _chars) {
            w += c.SizeCanvas.Width;
            h = Math.Max(h, c.SizeCanvas.Height);
        }
        return new SizeF(w, h);
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