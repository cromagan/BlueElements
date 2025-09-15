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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BlueTable;

public static class RowDrawDataExtensions {

    #region Methods

    public static RowData? Get(this List<RowData>? l, RowItem? row) => row == null ? null : (l?.FirstOrDefault(thisr => thisr?.Row == row));

    #endregion

    //public static RowData? Get(this List<RowData>? l, RowItem? row, string? chapter) {
    //    if (l == null || row == null) { return null; }

    //    chapter ??= string.Empty;

    //    return l.FirstOrDefault(thisr => thisr?.Row == row && thisr.Chapter == chapter);
    //}

    //public static List<RowItem> ToUniqueRowList(this List<RowData> l) {
    //    if (l == null) { return null; }

    //    var n = new List<RowItem>();

    //    foreach (var thisr in l) {
    //        if (thisr != null && thisr.Row != null) {
    //            n.AddIfNotExists(thisr.Row);
    //        }
    //    }
    //    return n;
    //}

    //public static int IndexOf(this List<clsRowDrawData> l, RowItem row) {
    //    for (var z = 0; z < l.Count; z++) {
    //        if (l[z].Row == row) { return z; }
    //    }
    //    return -1;
    //}
}

/// <summary>
/// Enthält Daten, wie eine Zeile angezeigt werden soll.
/// RowData kann mehrfach in einer Tabelle angezeigt werden.
/// Ein RowItem ist einzigartig, kann aber in mehreren RowData enthalten sein.
/// </summary>
public sealed class RowData : IComparable, IDisposableExtended {

    #region Constructors

    public RowData(RowItem row, string chapter) {
        Row = row;
        PinStateSortAddition = "2";
        Y = -1;
        RowChapter = chapter;
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
            if (thisViewItem.Column is { IsDisposed: false } tmpc && !Row.CellIsNullOrEmpty(tmpc)) {
                var renderer = thisViewItem.GetRenderer(style);
                DrawHeight = Math.Max(DrawHeight, renderer.ContentSize(Row.CellGetString(tmpc), tmpc.DoOpticalTranslation).Height);
            }
        }

        DrawHeight = Math.Min(DrawHeight, (int)(displayRectangleWoSlider.Height * 0.4));
        DrawHeight = Math.Max(DrawHeight, 18);
    }

    public string CompareKey() => PinStateSortAddition + ";" + RowChapter + ";" + AdditionalSort;

    public int CompareTo(object obj) {
        if (obj is RowData robj) {
            return string.Compare(CompareKey(), robj.CompareKey(), StringComparison.OrdinalIgnoreCase);
        }

        Develop.DebugPrint(ErrorType.Error, "Falscher Objecttyp!");
        return 0;
    }

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(disposing: true);
        //pp
        GC.SuppressFinalize(this);
    }

    public void GetDataFrom(RowData thisRowData) {
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

    public override string ToString() => Row.IsDisposed ? RowChapter + " -> null" : RowChapter + " -> " + Row.CellFirstString();

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