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

using BlueBasics.Interfaces;
using BlueDatabase;

namespace BlueControls.Interfaces {

    /// <summary>
    /// Wird verwendet, wenn das PadItem Zeilen berechnen kann und dann das
    /// erzeugte Control es an Childs weiter geben kann.
    /// </summary>
    public interface ICalculateOneRowItemLevel : IReadableText, IItemToControl {

        #region Properties

        public Database? Database { get; set; }

        /// <summary>
        /// Laufende Nummer, bestimmt die Einfärbung
        /// </summary>
        public int Id { get; set; }

        #endregion
    }
}