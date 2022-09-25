// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BlueDatabase;

public static class SQL_RowDrawDataExtensions {

    #region Methods

    public static SQL_RowData? Get(this List<SQL_RowData>? l, SQL_RowItem? row) {
        return l?.FirstOrDefault(thisr => thisr?.Row == row);
    }

    public static SQL_RowData? Get(this List<SQL_RowData>? l, SQL_RowItem? row, string chapter) {
        if (l == null || row == null) { return null; }

        return l.FirstOrDefault(thisr => thisr?.Row == row && thisr.Chapter == chapter);
    }

    #endregion

    //public static List<SQL_RowItem> ToUniqueRowList(this List<SQL_RowData> l) {
    //    if (l == null) { return null; }

    //    var n = new List<SQL_RowItem>();

    //    foreach (var thisr in l) {
    //        if (thisr != null && thisr.Row != null) {
    //            n.AddIfNotExists(thisr.Row);
    //        }
    //    }
    //    return n;
    //}

    //public static int IndexOf(this List<clsRowDrawData> l, SQL_RowItem row) {
    //    for (var z = 0; z < l.Count; z++) {
    //        if (l[z].Row == row) { return z; }
    //    }
    //    return -1;
    //}
}

/// <summary>
/// Enthält Daten, wie eine Zeile angezeigt werden soll.
/// SQL_RowData kann mehrfach in einer Tabelle angezeigt werden.
/// Ein SQL_RowItem ist einzigartig, kann aber in mehreren SQL_RowData enthalten sein.
/// </summary>
public class SQL_RowData : IComparable {

    #region Fields

    public readonly string Chapter;
    public readonly SQL_RowItem? Row;
    public string AdditionalSort;
    public Rectangle CaptionPos;
    public int DrawHeight;
    public bool Expanded;
    public bool MarkYellow;
    public string PinStateSortAddition;
    public bool ShowCap;
    public int Y;

    #endregion

    #region Constructors

    public SQL_RowData(SQL_RowItem row) : this(row, string.Empty) { }

    public SQL_RowData(SQL_RowItem row, string chapter) {
        Row = row;
        PinStateSortAddition = "2";
        Y = -1;
        Chapter = chapter;
        Expanded = true;
        DrawHeight = 0;
        CaptionPos = Rectangle.Empty;
        AdditionalSort = string.Empty;
        ShowCap = false;
        MarkYellow = false;
    }

    #endregion

    #region Methods

    public string CompareKey() => PinStateSortAddition + ";" + Chapter.StarkeVereinfachung(" ") + ";" + AdditionalSort;

    public int CompareTo(object obj) {
        if (obj is SQL_RowData robj) {
            return string.Compare(CompareKey(), robj.CompareKey(), StringComparison.OrdinalIgnoreCase);
        }

        Develop.DebugPrint(FehlerArt.Fehler, "Falscher Objecttyp!");
        return 0;
    }

    public void GetDataFrom(SQL_RowData thisSQL_RowData) {
        if (Row != thisSQL_RowData.Row || Chapter != thisSQL_RowData.Chapter) {
            Develop.DebugPrint(FehlerArt.Warnung, "SQL_RowData Kopie fehlgeschlagen!");
        }

        PinStateSortAddition = thisSQL_RowData.PinStateSortAddition;
        Y = thisSQL_RowData.Y;

        Expanded = thisSQL_RowData.Expanded;
        DrawHeight = thisSQL_RowData.DrawHeight;
        CaptionPos = thisSQL_RowData.CaptionPos;
        AdditionalSort = thisSQL_RowData.AdditionalSort;
        ShowCap = thisSQL_RowData.ShowCap;
        MarkYellow = thisSQL_RowData.MarkYellow;
    }

    public override string ToString() => Row == null ? Chapter + " -> null" : Chapter + " -> " + Row.CellFirstString();

    #endregion
}