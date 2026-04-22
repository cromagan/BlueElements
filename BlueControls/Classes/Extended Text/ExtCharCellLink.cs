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

    private readonly List<ExtChar> _chars = [];
    private string _displayText = string.Empty;

    #endregion

    #region Constructors

    public ExtCharCellLink() { }

    internal ExtCharCellLink(ExtText parent, List<string> overrideTags, string tableName, string columnKey, string rowKey) : base(parent, overrideTags) {
        TableName = tableName;
        ColumnKey = columnKey;
        RowKey = rowKey;
        _displayText = ResolveDisplayText();
        BuildChars();
        SetSize(CalculateSizeCanvas());
    }

    internal ExtCharCellLink(ExtText parent, int styleFromPos, string tableName, string columnKey, string rowKey) : base(parent, styleFromPos) {
        TableName = tableName;
        ColumnKey = columnKey;
        RowKey = rowKey;
        _displayText = ResolveDisplayText();
        BuildChars();
        SetSize(CalculateSizeCanvas());
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
            DrawLineBackgrounds(gr, controlPos, zoom);
            foreach (var c in _chars) {
                var charPos = new Point(controlPos.X + c.PosCanvas.X.CanvasToControl(zoom), controlPos.Y + c.PosCanvas.Y.CanvasToControl(zoom));
                c.Draw(gr, charPos, controlSize, zoom);
            }
        } catch { }
    }

    public override string HtmlText() => Method_Linkify.GenerateHtmlCellLink(TableName, ColumnKey, RowKey, CellValue);

    public override bool IsLineBreak() => false;

    public override bool IsPossibleLineBreak() => false;

    public override bool IsSpace() => false;

    public override bool IsWordSeparator() => false;

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
        SetSize(CalculateSizeCanvas());
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

    public override string PlainText() => _displayText;

    internal override void DrawWithFont(Graphics gr, Point controlPos, Size controlSize, float zoom, BlueFont font) {
        if (_chars.Count == 0) { return; }
        try {
            DrawLineBackgrounds(gr, controlPos, zoom);
            foreach (var c in _chars) {
                var charPos = new Point(controlPos.X + c.PosCanvas.X.CanvasToControl(zoom), controlPos.Y + c.PosCanvas.Y.CanvasToControl(zoom));
                c.DrawWithFont(gr, charPos, controlSize, zoom, font);
            }
        } catch { }
    }

    internal override void InitFromTag(ExtText parent, List<string> tags, string? attribut) {
        base.InitFromTag(parent, tags, attribut);

        string parseContent = attribut ?? string.Empty;

        if (!string.IsNullOrEmpty(attribut) && attribut.Contains('|')) {
            var t = attribut.Split('|');

            // Sicherheit geht vor: Prüfen, ob wir wirklich 3 Teile haben
            if (t.Length >= 3) {
                var link = Method_Linkify.GenerateHtmlCellLink(t[0], t[1], t[2], string.Empty);

                // Nur schneiden, wenn der String lang genug ist, um Fehler zu vermeiden
                if (link.Length > 11) {
                    parseContent = link[10..^1];
                } else {
                    parseContent = link; // Fallback, falls der Link zu kurz ist
                }
            }
        }

        this.Parse(parseContent, '\0', '\0', ' ');
    }

    internal override bool HandlesOwnLayout => true;

    internal override (float ContinueX, float ContinueY, float MaxRight, float MaxBottom) ComputeCharLayout(float startX, float startY, float maxWidth, float lineStartX, float lineSpacing) {
        EnsureCharsBuilt();
        PosCanvas = new PointF(startX, startY);

        if (_chars.Count == 0) {
            SetSize(SizeF.Empty);
            return (startX, startY, startX, startY);
        }

        var (continueX, internalContY, maxRight, maxBottom) = ExtText.ComputeSubLayout(
            _chars, startX, startY, maxWidth, lineStartX, lineSpacing, true, null);

        for (var i = 0; i < _chars.Count; i++) {
            _chars[i].PosCanvas = new PointF(_chars[i].PosCanvas.X - startX, _chars[i].PosCanvas.Y - startY);
        }

        SetSize(new SizeF(maxRight - startX, maxBottom - startY));

        var isMultiLine = internalContY > startY + 0.5f;
        var contY = isMultiLine ? maxBottom : startY;
        return (maxRight, contY, maxRight, maxBottom);
    }

    protected override SizeF CalculateSizeCanvas() {
        EnsureCharsBuilt();
        if (_chars.Count == 0) { return SizeF.Empty; }
        float w = 0;
        float h = 0;
        foreach (var c in _chars) {
            w += c.SizeCanvas.Width;
            h = Math.Max(h, c.SizeCanvas.Height);
        }
        return new SizeF(w, h);
    }

    private void BuildChars() {
        _chars.Clear();
        if (string.IsNullOrEmpty(_displayText)) { return; }
        foreach (var c in _displayText) {
            _chars.Add(new ExtCharAscii(_parent, OverrideTags, c));
        }
        LayoutSubChars();
    }

    private void DrawLineBackgrounds(Graphics gr, Point controlPos, float zoom) {
        if (_chars.Count == 0) { return; }
        var lineStart = 0;
        var lineY = _chars[0].PosCanvas.Y;

        for (var i = 1; i <= _chars.Count; i++) {
            if (i == _chars.Count || Math.Abs(_chars[i].PosCanvas.Y - lineY) > 0.5f) {
                float minX = float.MaxValue, maxX = float.MinValue;
                float minY = float.MaxValue, maxY = float.MinValue;
                for (var j = lineStart; j < i; j++) {
                    minX = Math.Min(minX, _chars[j].PosCanvas.X);
                    maxX = Math.Max(maxX, _chars[j].PosCanvas.X + _chars[j].SizeCanvas.Width);
                    minY = Math.Min(minY, _chars[j].PosCanvas.Y);
                    maxY = Math.Max(maxY, _chars[j].PosCanvas.Y + _chars[j].SizeCanvas.Height);
                }
                gr.FillRectangle(Brushes.LightGray,
                    controlPos.X + minX.CanvasToControl(zoom),
                    controlPos.Y + minY.CanvasToControl(zoom),
                    (maxX - minX).CanvasToControl(zoom),
                    (maxY - minY).CanvasToControl(zoom));
                if (i < _chars.Count) {
                    lineStart = i;
                    lineY = _chars[i].PosCanvas.Y;
                }
            }
        }
    }

    private void EnsureCharsBuilt() {
        if (_chars.Count == 0 && !string.IsNullOrEmpty(_displayText)) {
            BuildChars();
        }
    }

    private void LayoutSubChars() {
        float x = 0;
        foreach (var c in _chars) {
            c.PosCanvas = new PointF(x, 0);
            x += c.SizeCanvas.Width;
        }
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