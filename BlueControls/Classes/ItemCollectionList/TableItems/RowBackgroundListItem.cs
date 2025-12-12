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
using BlueTable;
using BlueTable.Enums;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BlueControls.ItemCollectionList;

/// <summary>
/// Enthält Daten, wie eine Zeile angezeigt werden soll.
/// RowData kann mehrfach in einer Tabelle angezeigt werden.
/// Ein RowItem ist einzigartig, kann aber in mehreren RowData enthalten sein.
/// </summary>
public abstract class RowBackgroundListItem : AbstractListItem, IDisposableExtended, IStyleable {

    #region Fields

    public static readonly Brush GrayBrush = new SolidBrush(Color.FromArgb(80, 200, 200, 200));

    public static readonly Brush GrayBrush2 = new SolidBrush(Color.FromArgb(150, 255, 255, 255));

    #endregion

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
            Invalidate_UntrimmedCanvasSize();
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

    /// <summary>
    /// True: Erst Non Permanent, dann Permanent
    /// False: Alle der Reihe Nach
    /// </summary>
    protected abstract bool DoSpezialOrder { get; }

    #endregion

    #region Methods

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(disposing: true);
        //pp
        GC.SuppressFinalize(this);
    }

    //      viewItem.GetRenderer(SheetStyle).Draw(gr, toDrawd, cellInThisTableRow, positionControl, cellInThisTableColumn.DoOpticalTranslation, (Alignment)cellInThisTableColumn.Align, _zoom);

    public virtual void Draw_Border(Graphics gr, ColumnViewItem viewItem, ColumnLineStyle lin, float xPos, float top, float bottom) => DrawLine(gr, lin, xPos, xPos, top, bottom);

    public virtual void Draw_ColumnBackGround(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, States state) {
        gr.FillRectangle(new SolidBrush(viewItem.BackColor_ColumnCell), positionControl);
    }

    public virtual void Draw_ColumnContent(Graphics gr, ColumnViewItem viewItem, RectangleF positionControl, float scale, TranslationType translate, float offsetX, float offsetY, States state) {
    }

    public virtual void Draw_LowerLine(Graphics gr, ColumnViewItem viewItem, ColumnLineStyle lin, float left, float right, float bottom) => DrawLine(gr, lin, left, right, bottom, bottom);

    public virtual void Draw_UpperLine(Graphics gr, ColumnLineStyle lin, float left, float right, float bottom) => DrawLine(gr, lin, left, right, bottom, bottom);

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

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, Design itemdesign, States state, bool drawBorderAndBack, bool translate, float offsetX, float offsetY, float scale) {
        if (Arrangement == null) { return; }

        for (var du = 0; du < 2; du++) {
            foreach (var viewItem in Arrangement) {
                if (DoSpezialOrder && ((viewItem.Permanent && du == 0) || (!viewItem.Permanent && du == 1))) { continue; }
                if (viewItem.Column == null) { continue; }

                var left = viewItem.ControlColumnLeft((int)offsetX);

                if (left > visibleAreaControl.Width) { continue; }
                if (left + (viewItem.ControlColumnWidth ?? 0) < 0) { continue; }

                var area = new Rectangle(left, (int)positionControl.Top, viewItem.ControlColumnWidth ?? 0, (int)positionControl.Height);

                if (Arrangement.Count == 1) { area = positionControl.ToRect(); }

                var t = viewItem.Column.DoOpticalTranslation;
                if (!translate) { t = TranslationType.Original_Anzeigen; }

                if (!DoSpezialOrder) {
                    if (!viewItem.Permanent) {
                        area.X = Math.Max(area.X, Arrangement.ControlColumnsPermanentWidth);
                    }
                }

                gr.SmoothingMode = SmoothingMode.None;
                Draw_ColumnBackGround(gr, viewItem, area, state);
                Draw_Border(gr, viewItem, viewItem.LineLeft, area.Left, area.Top, area.Bottom);
                Draw_Border(gr, viewItem, viewItem.LineRight, area.Right, area.Top, area.Bottom);
                Draw_UpperLine(gr, ColumnLineStyle.Ohne, area.Right, area.Left, area.Top);
                Draw_LowerLine(gr, viewItem, ColumnLineStyle.Dünn, area.Right, area.Left, area.Bottom - 1);
                Draw_ColumnContent(gr, viewItem, area, scale, t, offsetX, offsetY, state);
            }

            if (!DoSpezialOrder) { return; }
        }
    }

    protected override string GetCompareKey() => KeyName;

    #endregion
}