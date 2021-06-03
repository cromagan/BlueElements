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

using BlueBasics;

namespace BlueDatabase {

    /// <summary>
    /// Print Views werden nicht immer benötigt. Deswegen werden sie als String gespeichert. Der richtige Typ wäre ItemCollectionPad.
    /// Noch dazu ist ItemCollectionPad in BlueConrolls verankert, das nur die Sichtbarmachung einen Sinn macht.
    /// Und diese Sichtbarmachung braucht braucht Controls für die Bearbeitung.
    /// </summary>
    public class LayoutCollection : ListExt<string> {

        // Info:
        // ExportDialog.AddLayoutsOff wandelt Layouts In Items um

        public int LayoutIDToIndex(string exportFormularID) {

            for (var z = 0; z < Count; z++) {
                if (this[z].Contains("ID=" + exportFormularID + ",")) { return z; }
            }

            return -1;
        }

        public void Check() {
            for (var z = 0; z < Count; z++) {
                if (!this[z].StartsWith("{ID=#")) {
                    this[z] = "{ID=#Converted" + z.ToString() + ", " + this[z].Substring(1);
                }
            }
        }
    }
}
