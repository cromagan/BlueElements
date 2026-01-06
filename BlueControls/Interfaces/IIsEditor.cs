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

using BlueBasics.Interfaces;
using System;

namespace BlueControls.Interfaces;

/// <summary>
/// Definiet das Objekt als Editor für eine Klasse des Typs IEditable, das in EditorFor definiert wird.
/// Benötigt einen parameterlosen Konstruktor.
/// Für ein Grundgerüs kann EditorEasy verwendet werden - Objekte dieses Types können als UserControl verwendet werden,
/// oder automatisch eine Form geöffnet werden.
/// InputBoxEditorExtension.Edit kann dabei benutzt werden.
/// </summary>
public interface IIsEditor {

    #region Properties

    /// <summary>
    /// Gibt den Typ des IEditable an, den dieser Editor bearbeiten kann.
    /// </summary>
    Type? EditorFor { get; }

    IEditable? ToEdit { set; }

    #endregion
}