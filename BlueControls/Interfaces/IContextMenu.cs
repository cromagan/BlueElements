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

using BlueBasics.ClassesStatic;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Enums;
using BlueControls.Forms;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BlueBasics.Classes;
using BlueBasics.Enums;
using System.Windows.Forms;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;
using BlueBasics.Interfaces;
using System;
using BlueControls.EventArgs;

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
    /// Definert, ob das Default-Context-Menu angezeigt wird.
    /// </summary>
    public bool ContextMenuDefault { get; set; }

    /// <summary>
    /// Das Element, über dem das Kontextmenü geöffnet wurde.
    /// Wird von ContextMenuShow gesetzt.
    /// </summary>

    /// <summary>
    /// Benutzerdefinierte Menü-Elemente, die VORAB im Kontextmenü angezeigt werden, unabhängig von ContextMenuDefault
    /// </summary>
    public ReadOnlyCollection<AbstractListItem> CustomContextMenuItems { get; set; }

    #endregion

    #region Methods

    public void ContextMenuShow(object? hotItem) {
        FloatingForm.Close(ListBoxAppearance.KontextMenu);
        //FloatingForm.Close(this);
        Develop.SetUserDidSomething();

        var thisContextMenu = new List<AbstractListItem>();

        if (ContextMenuDefault && GetContextMenuItems(hotItem) is { } cmi && cmi.Count > 0) {
            thisContextMenu.AddRange(cmi);
        }

        if (CustomContextMenuItems != null) {
            if (thisContextMenu.Count > 0) { thisContextMenu.Add(Separator()); }
            thisContextMenu.AddRange(CustomContextMenuItems);
        }

        if (thisContextMenu.Count > 0) {
            thisContextMenu.Add(Separator());
            thisContextMenu.Add(ItemOf("Abbrechen", "Abbruch", QuickImage.Get(ImageCode.TasteESC)));
            Develop.SetUserDidSomething();

            // Sicherer Cast, falls das Interface mal in einer Nicht-Control-Klasse landet
            if (this is Control parentControl) {
                FloatingInputBoxListBoxStyle.Show(thisContextMenu, CheckBehavior.NoSelection, null, parentControl, true, ListBoxAppearance.KontextMenu, Design.Item_ContextMenu, false, false, hotItem);
            }
        }
    }

    public void ExecuteContextMenuComand(EventHandler<ContextMenuEventArgs> click, IHasKeyName? additional, object? hotItem) {
        click.Invoke(this, new ContextMenuEventArgs(ItemOf(additional?.KeyName ?? "Dummy"), hotItem));
    }

    /// <summary>
    /// Diese Routine wird aufgerufen, um die internen Kontextmenü-Einträge zu erstellen.
    /// Die benutzerdefinierten Einträge (CustomMenuItems) wurden bereits vorher eingefügt.
    /// Zugriff auf hotItem erfolgt über die Property ContextMenuHotItem.
    /// </summary>
    List<AbstractListItem>? GetContextMenuItems(object? hotItem);

    #endregion
}