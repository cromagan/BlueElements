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
using BlueControls.ItemCollectionList;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using BlueBasics;

using BlueBasics.Enums;
using BlueBasics.EventArgs;

using BlueBasics.Interfaces;

using BlueBasics.MultiUserFile;
using BlueControls.BlueTableDialogs;
using BlueControls.CellRenderer;
using BlueControls.Designer_Support;

using BlueControls.Enums;

using BlueControls.EventArgs;
using BlueControls.Extended_Text;
using BlueControls.Forms;

using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;

using BlueTable;
using BlueTable.Enums;
using BlueTable.EventArgs;
using BlueTable.Interfaces;

using System;

using System.Collections.Concurrent;

using System.Collections.Generic;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

using System.Drawing;

using System.Drawing.Drawing2D;

using System.Linq;

using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static BlueBasics.Constants;
using static BlueBasics.Converter;
using static BlueBasics.Generic;
using static BlueBasics.IO;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using static BlueTable.Table;
using System.Runtime.Versioning;
using BlueControls;

namespace BlueTable;

public static class RowBackgroundListItemListItemExtensions {

    #region Methods

    public static RowDataListItem? Get(this List<AbstractListItem>? l, RowItem? row) => row == null ? null : l?.FirstOrDefault(thisr => thisr is RowDataListItem r && r.Row == row) as RowDataListItem;

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

    protected RowBackgroundListItem(string keyname, ColumnViewCollection? arrangement) : base(keyname, true) {
        Arrangement = arrangement;
    }

    #endregion

    #region Properties

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

    //      viewItem.GetRenderer(SheetStyle).Draw(gr, toDrawd, cellInThisTableRow, positionModified, cellInThisTableColumn.DoOpticalTranslation, (Alignment)cellInThisTableColumn.Align, _zoom);

    public virtual void DrawColumn(Graphics gr, Renderer_Abstract renderer, ColumnItem column, Rectangle positionModified, float scale, TranslationType translate, Alignment align) {
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

    protected override void DrawExplicit(Graphics gr, Rectangle visibleArea, Rectangle positionModified, Design itemdesign, States state, bool drawBorderAndBack, bool translate, float shiftX, float shiftY, float scale) {
        if (Arrangement == null) { return; }

        for (var du = 0; du < 2; du++) {
            foreach (var viewItem in Arrangement) {
                if ((viewItem.Permanent && du == 0) || (!viewItem.Permanent && du == 1)) { continue; }
                if (viewItem.Column == null) { continue; }

                var left = viewItem.Permanent ? viewItem.X * scale : (viewItem.X * scale) - shiftX;

                if (left > visibleArea.Width) { continue; }
                if (left + positionModified.Width < 0) { continue; }

                var area = new Rectangle((int)(left ?? 0), positionModified.Top, viewItem.DrawWidth(), positionModified.Height);

                if (Arrangement.Count == 1) { area = positionModified; }

                gr.SmoothingMode = SmoothingMode.None;
                gr.FillRectangle(new SolidBrush(viewItem.BackColor_ColumnCell), area);

                var t = viewItem.Column.DoOpticalTranslation;
                if (!translate) { t = TranslationType.Original_Anzeigen; }

                DrawColumn(gr, viewItem.GetRenderer(SheetStyle), viewItem.Column, area, scale, t, (Alignment)viewItem.Column.Align);
                Draw_Border(gr, viewItem.LineLeft, area.Left, area.Top, area.Bottom);
                Draw_Border(gr, viewItem.LineRight, area.Right, area.Top, area.Bottom);
            }
        }
    }

    protected override string GetCompareKey() => KeyName;

    private void Draw_Border(Graphics gr, ColumnLineStyle lin, float xPos, float top, float bottom) {
        if (IsDisposed) { return; }

        switch (lin) {
            case ColumnLineStyle.Ohne:
                break;

            case ColumnLineStyle.Dünn:
                gr.DrawLine(Skin.PenLinieDünn, xPos, top, xPos, bottom);
                break;

            case ColumnLineStyle.Kräftig:
                gr.DrawLine(Skin.PenLinieKräftig, xPos, top, xPos, bottom);
                break;

            case ColumnLineStyle.Dick:
                gr.DrawLine(Skin.PenLinieDick, xPos, top, xPos, bottom);
                break;

            case ColumnLineStyle.ShadowRight:
                var c = Skin.Color_Border(Design.Table_Lines_thick, States.Standard);
                gr.DrawLine(Skin.PenLinieKräftig, xPos, top, xPos, bottom);
                gr.DrawLine(new Pen(Color.FromArgb(80, c.R, c.G, c.B)), xPos + 1, top, xPos + 1, bottom);
                gr.DrawLine(new Pen(Color.FromArgb(60, c.R, c.G, c.B)), xPos + 2, top, xPos + 2, bottom);
                gr.DrawLine(new Pen(Color.FromArgb(40, c.R, c.G, c.B)), xPos + 3, top, xPos + 3, bottom);
                gr.DrawLine(new Pen(Color.FromArgb(20, c.R, c.G, c.B)), xPos + 4, top, xPos + 4, bottom);
                break;

            default:
                Develop.DebugPrint(lin);
                break;
        }
    }

    #endregion
}