﻿// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueDatabase {

    public static class clsRowDrawDataExtensions {

        #region Methods

        public static RowData Get(this List<RowData> l, RowItem row) {
            if (l == null) { return null; }

            foreach (var thisr in l) {
                if (thisr?.Row == row) { return thisr; }
            }
            return null;
        }

        public static RowData Get(this List<RowData> l, RowItem row, string chapter) {
            if (l == null) { return null; }

            foreach (var thisr in l) {
                if (thisr?.Row == row && thisr.Chapter == chapter) { return thisr; }
            }
            return null;
        }

        public static List<RowItem> ToUniqueRowList(this List<RowData> l) {
            if (l == null) { return null; }

            var n = new List<RowItem>();

            foreach (var thisr in l) {
                n.AddIfNotExists(thisr.Row);
            }
            return n;
        }

        #endregion

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
    public class RowData : IComparable, ICompareKey {

        #region Fields

        public readonly string Chapter;
        public readonly RowItem Row;
        public string AdditionalSort;
        public Rectangle CaptionPos;
        public int DrawHeight;
        public bool Expanded;
        public bool Pinned;
        public bool ShowCap;
        public int Y;

        #endregion

        #region Constructors

        public RowData(RowItem row) : this(row, string.Empty) { }

        public RowData(RowItem row, string chapter) {
            Row = row;
            Pinned = false;
            Y = -1;
            Chapter = chapter;
            Expanded = true;
            DrawHeight = 0;
            CaptionPos = Rectangle.Empty;
            AdditionalSort = string.Empty;
            ShowCap = false;
        }

        #endregion

        #region Methods

        public string CompareKey() {
            if (Pinned) {
                return "1;" + Chapter.ToUpper() + ";" + AdditionalSort;
            } else {
                return "2;" + Chapter.ToUpper() + ";" + AdditionalSort;
            }
        }

        public int CompareTo(object obj) {
            if (obj is RowData robj) {
                return CompareKey().CompareTo(robj.CompareKey());
            } else {
                Develop.DebugPrint(enFehlerArt.Fehler, "Falscher Objecttyp!");
                return 0;
            }
        }

        public override string ToString() {
            if (Row == null) {
                return Chapter + " -> null";
            } else {
                return Chapter + " -> " + Row.CellFirstString();
            }
        }

        #endregion
    }
}