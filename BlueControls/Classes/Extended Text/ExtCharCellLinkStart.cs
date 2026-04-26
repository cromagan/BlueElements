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
using BlueTable.AdditionalScriptMethods;
using BlueTable.Classes;
using BlueTable.Enums;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.Extended_Text;

public class ExtCharCellLinkStart : ExtChar, IParseable {

    #region Constructors

    public ExtCharCellLinkStart() { }

    internal ExtCharCellLinkStart(ExtText parent, List<string> overrideTags, string tableName, string columnKey, string rowKey) : base(parent, overrideTags) {
        TableName = tableName;
        ColumnKey = columnKey;
        RowKey = rowKey;
        ResolveAndStoreDisplayText();
    }

    internal ExtCharCellLinkStart(ExtText parent, int styleFromPos, string tableName, string columnKey, string rowKey) : base(parent, styleFromPos) {
        TableName = tableName;
        ColumnKey = columnKey;
        RowKey = rowKey;
        ResolveAndStoreDisplayText();
    }

    #endregion

    #region Properties

    public string CellValue { get; private set; } = string.Empty;
    public string ColumnKey { get; private set; } = string.Empty;
    public string DisplayText { get; private set; } = string.Empty;
    public string RowKey { get; private set; } = string.Empty;
    public string TableName { get; private set; } = string.Empty;
    internal override string? StructuralTag => "CELLLINK";

    #endregion

    #region Methods

    public override void Draw(Graphics gr, Point controlPos, Size controlSize, float zoom) { }

    public override string HtmlText() => Method_Linkify.GenerateHtmlCellLink(TableName, ColumnKey, RowKey, DisplayText);

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
        ResolveAndStoreDisplayText();
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

    public override string PlainText() => string.Empty;

    internal override void InitFromTag(ExtText parent, List<string> tags, string? attribut) {
        base.InitFromTag(parent, tags, attribut);

        if (!string.IsNullOrEmpty(attribut) && attribut.Contains('|')) {
            var t = attribut.Split('|');
            if (t.Length >= 3) {
                TableName = t[0].FromNonCritical();
                ColumnKey = t[1].FromNonCritical();
                RowKey = t[2].FromNonCritical();
                ResolveAndStoreDisplayText();
                return;
            }
        }

        this.Parse(attribut ?? string.Empty, '\0', '\0', ' ');
    }

    internal string ResolveDisplayText() {
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

    protected override SizeF CalculateSizeCanvas() => SizeF.Empty;

    private void ResolveAndStoreDisplayText() {
        DisplayText = ResolveDisplayText();
        if (!string.IsNullOrEmpty(CellValue) && string.IsNullOrEmpty(DisplayText)) {
            DisplayText = CellValue;
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