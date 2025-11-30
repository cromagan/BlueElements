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

using BlueBasics;
using BlueBasics.Interfaces;
using BlueControls.Enums;
using BlueControls.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BlueTable.Enums;
using System.Drawing.Drawing2D;
using BlueTable;

namespace BlueControls.ItemCollectionList;

public static class RowBackgroundListItemListItemExtensions {

    #region Methods

    public static RowListItem? Get(this List<AbstractListItem>? l, RowItem? row) => row == null ? null : l?.FirstOrDefault(thisr => thisr is RowListItem r && r.Row == row) as RowListItem;

    public static RowBackgroundListItem? Get(this List<AbstractListItem>? l, RowItem? row, string caption) {
        if (l == null) { return null; }

        if (l.GetByKey(RowBackgroundListItem.Key(row, caption)) is RowBackgroundListItem { } bli) {
            return bli;
        }

        return null;
    }

    #endregion
}

/// <summary>
/// Enthält Daten, wie eine Zeile angezeigt werden soll.
/// RowData kann mehrfach in einer Tabelle angezeigt werden.
/// Ein RowItem ist einzigartig, kann aber in mehreren RowData enthalten sein.
/// </summary>
public abstract class RowBackgroundListItem : AbstractListItem, IDisposableExtended, IStyleable {

    #region Constructors

    protected RowBackgroundListItem(string keyname, ColumnViewCollection? arrangement, string alignsToCaption) : base(keyname, true) {
        Arrangement = arrangement;
        AlignsToChapter = alignsToCaption;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Großschreibung
    /// </summary>
    public string AlignsToChapter {
        get;
        private set {
            value = value.ToUpperInvariant();
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public ColumnViewCollection? Arrangement {
        get;
        set {
            if (field == value) { return; }

            field = value;
            OnPropertyChanged();
        }
    }

    public int FixY {
        get;
        set {
            if (field == value) { return; }

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsDisposed {
        get;
        private set {
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    }

    public string SheetStyle {
        get;
        set {
            if (IsDisposed) { return; }

            if (field == value) { return; }

            field = value;
            OnPropertyChanged();
        }
    } = Constants.Win11;

    #endregion

    #region Methods

    public static string Key(RowItem? row, string chapter) {
        if (row == null) { return chapter.Trim('\\').ToUpperInvariant() + '\\'; }

        return chapter.Trim('\\').ToUpperInvariant() + "\\" + row.KeyName;
    }

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(disposing: true);
        //pp
        GC.SuppressFinalize(this);
    }

    //      viewItem.GetRenderer(SheetStyle).Draw(gr, toDrawd, cellInThisTableRow, positionInControl, cellInThisTableColumn.DoOpticalTranslation, (Alignment)cellInThisTableColumn.Align, _zoom);

    public virtual void Draw_Border(Graphics gr, ColumnLineStyle lin, float xPos, float top, float bottom) {
        DrawLine(gr, lin, xPos, xPos, top, bottom);
    }

    public virtual void Draw_LowerLine(Graphics gr, ColumnLineStyle lin, float left, float right, float bottom) {
        DrawLine(gr, lin, left, right, bottom, bottom);
    }

    public virtual void Draw_UpperLine(Graphics gr, ColumnLineStyle lin, float left, float right, float bottom) {
        DrawLine(gr, lin, left, right, bottom, bottom);
    }

    public virtual void DrawColumn(Graphics gr, ColumnViewItem viewItem, RectangleF positionInControl, float scale, TranslationType translate, float offsetX, float offsetY, States state) {
    }

    public void DrawLine(Graphics gr, ColumnLineStyle lin, float left, float right, float top, float bottom) {
        if (IsDisposed) { return; }

        try {
            switch (lin) {
                case ColumnLineStyle.Ohne:
                    break;

                case ColumnLineStyle.Dünn:
                    gr.DrawLine(Skin.PenLinieDünn, left, top, right, bottom);
                    break;

                case ColumnLineStyle.Kräftig:
                    gr.DrawLine(Skin.PenLinieKräftig, left, top, right, bottom);
                    break;

                case ColumnLineStyle.Dick:
                    gr.DrawLine(Skin.PenLinieDick, left, top, right, bottom);
                    break;

                case ColumnLineStyle.ShadowRight:
                    var c = Skin.Color_Border(Design.Table_Lines_thick, States.Standard);
                    gr.DrawLine(Skin.PenLinieKräftig, left, top, right, bottom);
                    gr.DrawLine(new Pen(Color.FromArgb(80, c.R, c.G, c.B)), left + 1, top, right + 1, bottom);
                    gr.DrawLine(new Pen(Color.FromArgb(60, c.R, c.G, c.B)), left + 2, top, right + 2, bottom);
                    gr.DrawLine(new Pen(Color.FromArgb(40, c.R, c.G, c.B)), left + 3, top, right + 3, bottom);
                    gr.DrawLine(new Pen(Color.FromArgb(20, c.R, c.G, c.B)), left + 4, top, right + 4, bottom);
                    break;

                default:
                    Develop.DebugPrint(lin);
                    break;
            }
        } catch { }
    }

    protected virtual void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                Arrangement = null;
                //Row = null;
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }

            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            // TODO: Große Felder auf NULL setzen
            IsDisposed = true;
        }
    }

    protected override void DrawExplicit(Graphics gr, Rectangle visibleArea, RectangleF positionInControl, Design itemdesign, States state, bool drawBorderAndBack, bool translate, float offsetX, float offsetY, float scale) {
        if (Arrangement == null) { return; }

        for (var du = 0; du < 2; du++) {
            foreach (var viewItem in Arrangement) {
                if ((viewItem.Permanent && du == 0) || (!viewItem.Permanent && du == 1)) { continue; }
                if (viewItem.Column == null) { continue; }

                var left = viewItem.ControlColumnLeft((int)offsetX);

                if (left > visibleArea.Width) { continue; }
                if (left + positionInControl.Width < 0) { continue; }

                var area = new Rectangle(left, (int)positionInControl.Top, viewItem.ControlColumnWidth ?? 0, (int)positionInControl.Height);

                if (Arrangement.Count == 1) { area = positionInControl.ToRect(); }

                gr.SmoothingMode = SmoothingMode.None;
                gr.FillRectangle(new SolidBrush(viewItem.BackColor_ColumnCell), area);

                var t = viewItem.Column.DoOpticalTranslation;
                if (!translate) { t = TranslationType.Original_Anzeigen; }

                DrawColumn(gr, viewItem, area, scale, t, offsetX, offsetY, state);
                Draw_Border(gr, viewItem.LineLeft, area.Left, area.Top, area.Bottom);
                Draw_Border(gr, viewItem.LineRight, area.Right, area.Top, area.Bottom);
                Draw_UpperLine(gr, ColumnLineStyle.Ohne, area.Right, area.Left, area.Top);
                Draw_LowerLine(gr, ColumnLineStyle.Dünn, area.Right, area.Left, area.Bottom - 1);
            }
        }
    }

    protected override string GetCompareKey() => KeyName;

    #endregion
}