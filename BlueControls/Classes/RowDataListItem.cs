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
using BlueBasics.Interfaces;
using BlueControls.Enums;
using BlueControls.ItemCollectionList;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BlueTable;

public static class RowDataListItemExtensions {

    #region Methods

    public static RowDataListItem? Get(this List<RowDataListItem>? l, RowItem? row) => row == null ? null : l?.FirstOrDefault(thisr => thisr?.Row == row);

    #endregion
}

/// <summary>
/// Enthält Daten, wie eine Zeile angezeigt werden soll.
/// RowData kann mehrfach in einer Tabelle angezeigt werden.
/// Ein RowItem ist einzigartig, kann aber in mehreren RowData enthalten sein.
/// </summary>
public sealed class RowDataListItem : AbstractListItem, IComparable, IDisposableExtended {

    #region Constructors

    public RowDataListItem(RowItem row, string chapter) : base(string.Empty, true) {
        Row = row;
        PinStateSortAddition = "2";
        Y = -1;
        RowChapter = string.Empty;
        if (!string.IsNullOrEmpty(chapter)) { RowChapter = chapter.Trim('\\') + '\\'; }
        Expanded = true;
        DrawHeight = 18;
        CaptionPos = Rectangle.Empty;
        AdditionalSort = string.Empty;
        ShowCap = false;
        MarkYellow = false;
        IsDisposed = false;
    }

    #endregion

    #region Properties

    public string AdditionalSort { get; set; }
    public Rectangle CaptionPos { get; set; }
    public int DrawHeight { get; private set; }
    public bool Expanded { get; set; }
    public bool IsDisposed { get; private set; }
    public bool MarkYellow { get; set; }
    public string PinStateSortAddition { get; set; }
    public override string QuickInfo => Row?.QuickInfo ?? string.Empty;
    public RowItem Row { get; }
    public string RowChapter { get; }
    public bool ShowCap { get; set; }
    public int Y { get; set; }

    #endregion

    #region Methods

    public void CalculateDrawHeight(ColumnViewCollection ca, Rectangle displayRectangleWoSlider, string style) {
        if (IsDisposed || Row.IsDisposed) { DrawHeight = 16; return; }

        DrawHeight = 18;

        foreach (var thisViewItem in ca) {
            if (thisViewItem.Column is { IsDisposed: false } tmpc) {
                var renderer = thisViewItem.GetRenderer(style);
                DrawHeight = Math.Max(DrawHeight, renderer.ContentSize(Row.CellGetString(tmpc), tmpc.DoOpticalTranslation).Height);
            }
        }

        DrawHeight = Math.Min(DrawHeight, (int)(displayRectangleWoSlider.Height * 0.4));
        DrawHeight = Math.Max(DrawHeight, 18);
    }

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(disposing: true);
        //pp
        GC.SuppressFinalize(this);
    }

    public void GetDataFrom(RowDataListItem thisRowData) {
        if (Row != thisRowData.Row || RowChapter != thisRowData.RowChapter) {
            Develop.DebugPrint(ErrorType.Warning, "RowData Kopie fehlgeschlagen!");
        }

        PinStateSortAddition = thisRowData.PinStateSortAddition;
        Y = thisRowData.Y;

        Expanded = thisRowData.Expanded;
        DrawHeight = thisRowData.DrawHeight;
        CaptionPos = thisRowData.CaptionPos;
        AdditionalSort = thisRowData.AdditionalSort;
        ShowCap = thisRowData.ShowCap;
        MarkYellow = thisRowData.MarkYellow;
    }

    public override int HeightForListBox(ListBoxAppearance style, int columnWidth, Design itemdesign) => throw new NotImplementedException();

    public override string ToString() => Row.IsDisposed ? RowChapter + " -> null" : RowChapter + " -> " + Row.CellFirstString();

    protected override Size ComputeSizeUntouchedForListBox(Design itemdesign) => throw new NotImplementedException();

    protected override void DrawExplicit(Graphics gr, Rectangle positionModified, Design itemdesign, States state, bool drawBorderAndBack, bool translate) => throw new NotImplementedException();

    protected override string GetCompareKey() => PinStateSortAddition + ";" + AdditionalSort + ";" + RowChapter;

    private void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                //Row = null;
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }

            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            // TODO: Große Felder auf NULL setzen
            IsDisposed = true;
        }
    }

    #endregion

    // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    // ~RowData()
    // {
    //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
    //     Dispose(disposing: false);
    // }
}