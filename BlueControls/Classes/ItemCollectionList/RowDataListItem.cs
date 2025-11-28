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
using BlueBasics.Enums;
using BlueControls.CellRenderer;
using BlueControls.Enums;
using BlueTable.Enums;
using System;
using System.Drawing;

namespace BlueTable;

/// <summary>
/// Enthält Daten, wie eine Zeile angezeigt werden soll.
/// RowData kann mehrfach in einer Tabelle angezeigt werden.
/// Ein RowItem ist einzigartig, kann aber in mehreren RowData enthalten sein.
/// </summary>
public sealed class RowDataListItem : RowBackgroundListItem {

    #region Constructors

    public RowDataListItem(RowItem row, string alignsToCaption, ColumnViewCollection? arrangement) : base(Key(row, alignsToCaption), arrangement) {
        Row = row;
        MarkYellow = false;
        AlignsToCaption = alignsToCaption;
    }

    #endregion

    #region Properties

    public string AlignsToCaption {
        get;
        set {
            value = value.ToUpperInvariant();
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public bool MarkYellow {
        get;
        set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public override string QuickInfo => Row.GetQuickInfo();
    public RowItem Row { get; }

    #endregion

    #region Methods

    public override void DrawColumn(Graphics gr, ColumnViewItem viewItem, RectangleF positionModified, float scale, TranslationType translate, float shiftX, float shiftY) {
        base.DrawColumn(gr, viewItem, positionModified, scale, translate, shiftX, shiftY);

        if (viewItem.Column == null) { return; }

        if (!viewItem.Column.SaveContent) {
            Row.CheckRow();
        }

        var toDrawd = Row.CellGetString(viewItem.Column);
        viewItem.GetRenderer(SheetStyle).Draw(gr, toDrawd, Row, positionModified.ToRect(), translate, (Alignment)viewItem.Column.Align, scale);
    }

    public override int HeightForListBox(ListBoxAppearance style, int columnWidth, Design itemdesign) => SizeUntouchedForListBox(itemdesign).Height;

    protected override Size ComputeSizeUntouchedForListBox(Design itemdesign) {
        if (IsDisposed || Row.IsDisposed || Arrangement is null) { return new(16, 16); }

        var drawHeight = 18;

        foreach (var thisViewItem in Arrangement) {
            if (thisViewItem.Column is { IsDisposed: false } tmpc) {
                var renderer = thisViewItem.GetRenderer(SheetStyle);
                drawHeight = Math.Max(drawHeight, renderer.ContentSize(Row.CellGetString(tmpc), tmpc.DoOpticalTranslation).Height);
            }
        }

        drawHeight = Math.Min(drawHeight, 200);
        drawHeight = Math.Max(drawHeight, 18);

        return new(100, drawHeight);
    }

    #endregion
}