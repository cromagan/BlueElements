#region BlueElements - a collection of useful tools, database and controls
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
#endregion

using BlueDatabase;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.Controls {
    public class clsRowDrawData {

        public RowItem Row;
        public bool Pinned;
        public int Y;
        public string Chapter;
        public bool Expanded;
        public Rectangle CaptionPos;
        public int DrawHeight;

        public clsRowDrawData(RowItem row) {
            Row = row;
            Pinned = false;
            Y = -1;
            Chapter = string.Empty;
            Expanded = true;
            DrawHeight = 0;
            CaptionPos = Rectangle.Empty;
        }
    }

    public static class clsRowDrawDataExtensions {

        public static clsRowDrawData Get(this List<clsRowDrawData> l, RowItem row) {

            foreach (var thisr in l) {
                if (thisr.Row == row) { return thisr; }
            }
            return null;
        }

        //public static int IndexOf(this List<clsRowDrawData> l, RowItem row) {
        //    for (var z = 0; z < l.Count; z++) {
        //        if (l[z].Row == row) { return z; }
        //    }
        //    return -1;
        //}
    }
}