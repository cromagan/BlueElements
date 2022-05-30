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

using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using System.Collections.Generic;
using BlueControls.ItemCollection.ItemCollectionList;

namespace BlueControls;

public static class Allgemein {

    #region Methods

    public static T? ParentControl<T>(this object o) {
        if (o is not System.Windows.Forms.Control co) {
            return default;
        }

        do {
            co = co.Parent;
            switch (co) {
                case null:
                    return default;

                case T ctr:
                    return ctr;
            }
        } while (true);
    }

    public static IContextMenu? ParentControlWithCommands(this object o) {
        var par = o.ParentControl<IContextMenu>();
        if (par == null) { return null; }
        ItemCollectionList? thisContextMenu = new(BlueListBoxAppearance.KontextMenu);
        ItemCollectionList userMenu = new(BlueListBoxAppearance.KontextMenu);
        List<string> tags = new();
        var cancel = false;
        var translate = true;
        par.GetContextMenuItems(null, thisContextMenu, out var hotItem, tags, ref cancel, ref translate);
        if (cancel) { return null; }
        ContextMenuInitEventArgs ec = new(hotItem, tags, userMenu);
        par.OnContextMenuInit(ec);
        return ec.Cancel ? null
            : thisContextMenu != null && thisContextMenu.Count > 0 ? par
            : userMenu.Count > 0 ? par
            : null;
    }

    #endregion
}