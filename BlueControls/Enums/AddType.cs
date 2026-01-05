// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

namespace BlueControls.Enums;

public enum AddType {

    /// <summary>
    /// Hinzu-Button wird nicht angezeigt.
    /// </summary>
    None = 0,

    /// <summary>
    /// Hinzu-Button wird angezeigt, und auf einen Klick dessen wird ein Item mittels einer Input-Box erstellt.
    /// </summary>
    Text = 1,

    ///// <summary>
    ///// Hinzu-Button wird angezeigt, und auf einen Klick dessen wird  ein Item mittels einer File-Select-Box erstellt.
    /////Die Original-Dateien werden nicht verändert.
    ///// </summary>
    //BinarysFromFileSystem = 2,

    /// <summary>
    /// Hinzu-Button wird angezeigt, und auf einen Klick dessen wird ein Item mittels einer List-Box erstellt.
    /// </summary>
    OnlySuggests = 3,

    /// <summary>
    /// Hinzu-Button wird angezeigt, und auf einen Klick dessen der Delegate AddMethode ausgeführt.
    /// </summary>
    UserDef = 4
}