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

namespace BlueControls.Interfaces {

    public interface IRecursiveCheck {

        #region Methods

        /// <summary>
        /// Übrprüft ob eine Rekursivität besteht.
        /// Bei Level 0 ist man im eigenen Objekt - der Start -, da keine Prüfung anstoßen
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="currentLVL"></param>
        /// <returns></returns>
        public bool IsRecursiveWith(IRecursiveCheck obj);

        #endregion
    }
}