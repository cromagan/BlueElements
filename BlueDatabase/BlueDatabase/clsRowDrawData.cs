// Authors:
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

        public static clsRowDrawData Get(this List<clsRowDrawData> l, RowItem row) {
            foreach (var thisr in l) {
                if (thisr?.Row == row) { return thisr; }
            }
            return null;
        }

        public static clsRowDrawData Get(this List<clsRowDrawData> l, RowItem row, string chapter) {
            foreach (var thisr in l) {
                if (thisr?.Row == row && thisr.Chapter == chapter) { return thisr; }
            }
            return null;
        }

        #endregion

        //public static int IndexOf(this List<clsRowDrawData> l, RowItem row) {
        //    for (var z = 0; z < l.Count; z++) {
        //        if (l[z].Row == row) { return z; }
        //    }
        //    return -1;
        //}
    }

    public class clsRowDrawData : IComparable, ICompareKey {

        #region Fields

        public string AdditionalSort;
        public Rectangle CaptionPos;
        public string Chapter;
        public int DrawHeight;
        public bool Expanded;
        public bool Pinned;
        public RowItem Row;
        public int Y;

        #endregion

        #region Constructors

        public clsRowDrawData(RowItem row) : this(row, false, string.Empty, string.Empty) { }

        public clsRowDrawData(RowItem row, bool pinned, string additionalSort, string chapter) {
            Row = row;
            Pinned = pinned;
            Y = -1;
            Chapter = chapter;
            Expanded = true;
            DrawHeight = 0;
            CaptionPos = Rectangle.Empty;
            AdditionalSort = additionalSort;
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
            if (obj is clsRowDrawData robj) {
                return CompareKey().CompareTo(robj.CompareKey());
            } else {
                Develop.DebugPrint(enFehlerArt.Fehler, "Falscher Objecttyp!");
                return 0;
            }
        }

        #endregion
    }
}