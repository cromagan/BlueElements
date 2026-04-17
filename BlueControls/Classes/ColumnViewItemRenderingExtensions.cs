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
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using BlueBasics;
using BlueBasics.ClassesStatic;
using BlueControls.Classes.ItemCollectionList.TableItems;
using BlueControls.Controls;
using BlueControls.Renderer;
using BlueTable.Classes;
using BlueTable.Enums;
using System;
using System.Runtime.CompilerServices;

namespace BlueControls.Classes;

public static class ColumnViewItemRenderingExtensions {

    #region Fields

    private static readonly ConditionalWeakTable<ColumnViewItem, RenderingData> _renderingData = new();
    private static readonly ConditionalWeakTable<ColumnViewCollection, CollectionRenderingData> _collectionRenderingData = new();

    #endregion

    #region Methods

    public static bool CollapsableEnabled(this ColumnViewItem cvi) => cvi.CanvasContentWidth() > 40 || !cvi.IsExpanded;

    public static int CanvasContentWidth(this ColumnViewItem cvi) {
        if (_renderingData.TryGetValue(cvi, out var data) && data.CanvasContentWidth is { } v) { return v; }

        var renderer = cvi.GetRenderer(cvi.SheetStyle);
        v = CalculateCanvasContentWith(cvi.Column, renderer);
        _renderingData.GetOrCreateValue(cvi).CanvasContentWidth = v;
        return v;
    }

    public static int ControlColumnWidth(this ColumnViewItem cvi) => GetRenderingData(cvi).ControlColumnWidth ?? 16;

    public static int ControlColumnRight(this ColumnViewItem cvi, float offsetX) => cvi.ControlColumnLeft(offsetX) + cvi.ControlColumnWidth();

    public static int ControlColumnLeft(this ColumnViewItem cvi, float offsetX) {
        if (cvi.Permanent) {
            return GetRenderingData(cvi).ControlColumnLeft;
        }

        return GetRenderingData(cvi).ControlColumnLeft + (int)offsetX;
    }

    public static void ComputeLocation(this ColumnViewItem cvi, ColumnViewCollection parent, int x, int tableviewWith, float zoom) {
        if (cvi.Column == null) { return; }

        GetRenderingData(cvi).ControlColumnLeft = x;
        GetRenderingData(cvi).ControlColumnWidth = ComputeControlColumnWidth(cvi, parent, tableviewWith, zoom);
    }

    public static Renderer_Abstract GetRenderer(this ColumnViewItem cvi, string style) {
        var data = GetRenderingData(cvi);
        if (data.Renderer != null) { return data.Renderer; }

        data.Renderer = TableView.RendererOf(cvi, style);
        return data.Renderer;
    }

    public static void Invalidate_CanvasContentWidth(this ColumnViewItem cvi) {
        if (_renderingData.TryGetValue(cvi, out var data)) {
            data.CanvasContentWidth = null;
        }
        cvi.InvalidateLayout();
    }

    public static void ComputeAllColumnPositions(this ColumnViewCollection cvc, int tableviewWith, float zoom) {
        if (!cvc._invalidated) { return; }
        var collData = _collectionRenderingData.GetOrCreateValue(cvc);
        collData.ControlColumnsPermanentWidth = 0;
        collData.ControlColumnsWidth = 0;
        if (cvc.IsDisposed) { return; }

        cvc._invalidated = false;
        var maxX = 0;

        foreach (var thisViewItem in cvc) {
            thisViewItem.ComputeLocation(cvc, maxX, tableviewWith, zoom);

            maxX = thisViewItem.ControlColumnRight(0);

            if (thisViewItem.Permanent) {
                collData.ControlColumnsPermanentWidth = Math.Max(maxX, collData.ControlColumnsPermanentWidth);
            }

            collData.ControlColumnsWidth = Math.Max(maxX, collData.ControlColumnsWidth);
        }
    }

    public static int ControlColumnsPermanentWidth(this ColumnViewCollection cvc) => _collectionRenderingData.TryGetValue(cvc, out var d) ? d.ControlColumnsPermanentWidth : 0;

    public static int ControlColumnsWidth(this ColumnViewCollection cvc) => _collectionRenderingData.TryGetValue(cvc, out var d) ? d.ControlColumnsWidth : 0;

    internal static int CalculateCanvasContentWith(ColumnItem? column, Renderer_Abstract renderer) {
        if (column is not { IsDisposed: false }) { return 16; }
        if (column.Table is not { IsDisposed: false } tb) { return 16; }
        if (column.FixedColumnWidth > 0) { return column.FixedColumnWidth; }

        var newContentWidth = 16;

        try {
            foreach (var thisRowItem in tb.Row) {
                var wx = renderer.ContentSize(thisRowItem.CellGetString(column), column.DoOpticalTranslation).Width;
                newContentWidth = Math.Max(newContentWidth, wx);
            }
        } catch {
            Develop.AbortAppIfStackOverflow();
            return CalculateCanvasContentWith(column, renderer);
        }

        return newContentWidth;
    }

    private static int ComputeControlColumnWidth(ColumnViewItem cvi, ColumnViewCollection? parent, int tableviewWith, float zoom) {
        var p16 = 16.CanvasToControl(zoom);
        var pa = 8.CanvasToControl(zoom);

        if (parent == null) { return p16; }

        if (cvi.Column is not { IsDisposed: false }) {
            GetRenderingData(cvi).ControlColumnWidth = p16;
            return p16;
        }

        if (parent.Count == 1) {
            GetRenderingData(cvi).ControlColumnWidth = tableviewWith;
            return tableviewWith;
        }

        var minw = p16 * (cvi.Column.Caption.CountString("\r") + 1) + pa;

        int cw;
        if (!cvi.IsExpanded) {
            cw = minw;
        } else {
            cw = cvi.ViewType == ViewType.PermanentColumn
                ? Math.Min(cvi.CanvasContentWidth().CanvasToControl(zoom) + pa, (int)(tableviewWith * 0.3))
                : Math.Min(cvi.CanvasContentWidth().CanvasToControl(zoom) + pa, (int)(tableviewWith * 0.6));
        }

        cw = Math.Max(cw, FilterBarListItem.AutoFilterSize.CanvasToControl(zoom));
        cw = Math.Max(cw, minw);
        GetRenderingData(cvi).ControlColumnWidth = cw;
        return cw;
    }

    private static RenderingData GetRenderingData(ColumnViewItem cvi) => _renderingData.GetOrCreateValue(cvi);

    #endregion

    #region Nested Types

    private sealed class RenderingData {

        #region Properties

        public int? CanvasContentWidth { get; set; }

        public int ControlColumnLeft { get; set; }

        public int? ControlColumnWidth { get; set; }

        public Renderer_Abstract? Renderer { get; set; }

        #endregion
    }

    private sealed class CollectionRenderingData {

        #region Properties

        public int ControlColumnsPermanentWidth { get; set; }

        public int ControlColumnsWidth { get; set; }

        #endregion
    }

    #endregion
}