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

namespace BlueDatabase.EventArgs {

    public class GenerateLayoutInternalEventargs : System.EventArgs {

        #region Constructors

        public GenerateLayoutInternalEventargs(RowItem row, string layoutID, string SaveTo) {
            Row = row;
            LayoutID = layoutID;
            Filename = SaveTo;
            Handled = false;
        }

        #endregion

        #region Properties

        //public bool DirectPrint { get; set; }
        //public bool DirectSave { get; set; }
        public string Filename { get; set; }

        public bool Handled { get; set; }
        public string LayoutID { get; set; }
        public RowItem Row { get; set; }

        #endregion
    }
}