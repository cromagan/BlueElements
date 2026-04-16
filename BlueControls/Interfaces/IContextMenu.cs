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

using BlueControls.Classes.ItemCollectionList;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BlueControls.Interfaces;

/// <summary>
/// Interface, das zur Generierung von Kontextmenüs benötigt wird.
/// Die ganze Erstellung und Handling übernimmt dabei FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);
/// Dabei werden die hier angegebenen Routinen und Properties abgefragt.
/// CustomMenuItems werden VORAB in das Kontextmenü eingefügt, vor den internen Einträgen.
/// ContextMenuHotItem wird von ContextMenuShow gesetzt,
/// bevor GetContextMenuItems oder CustomMenuItems aufgerufen/ausgewertet werden.
/// </summary>
public interface IContextMenu {

    #region Properties

    /// <summary>
    /// Benutzerdefinierte Menü-Elemente, die VORAB im Kontextmenü angezeigt werden.
    /// </summary>
    public ReadOnlyCollection<AbstractListItem> CustomMenuItems { get; set; }

    /// <summary>
    /// Das Element, über dem das Kontextmenü geöffnet wurde.
    /// Wird von ContextMenuShow gesetzt.
    /// </summary>
    public object? ContextMenuHotItem { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Diese Routine wird aufgerufen, um die internen Kontextmenü-Einträge zu erstellen.
    /// Die benutzerdefinierten Einträge (CustomMenuItems) wurden bereits vorher eingefügt.
    /// Zugriff auf hotItem erfolgt über die Property ContextMenuHotItem.
    /// </summary>
    List<AbstractListItem>? GetContextMenuItems();

    #endregion
}