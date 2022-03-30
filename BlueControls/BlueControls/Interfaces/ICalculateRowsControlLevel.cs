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

using BlueBasics;
using BlueBasics.Interfaces;
using BlueDatabase;

namespace BlueControls.Interfaces {

    /// <summary>
    /// Ähnlich zu ICalculateRowsItemLevel.
    /// Hier sind die Wert zu finden, wenn das Control Zeilen berechnen kann und
    /// diese an Childs weiter geben kann.
    /// </summary>
    public interface ICalculateRowsControlLevel {

        #region Properties

        public ListExt<System.Windows.Forms.Control> Childs { get; }

        #endregion
    }
}