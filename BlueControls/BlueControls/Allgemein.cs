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

using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using System.Collections.Generic;

namespace BlueControls {

    public static class Allgemein {

        #region Methods

        public static t ParentControl<t>(this object o) {
            if (o is System.Windows.Forms.Control co) {
                do {
                    co = co.Parent;
                    switch (co) {
                        case null:
                            return default;

                        case t ctr:
                            return ctr;

                        default:
                            break;
                    }
                } while (true);
            }
            return default;
        }

        public static IContextMenu ParentControlWithCommands(this object o) {
            var par = o.ParentControl<IContextMenu>();
            if (par == null) { return null; }
            ItemCollectionList ThisContextMenu = new(enBlueListBoxAppearance.KontextMenu);
            ItemCollectionList UserMenu = new(enBlueListBoxAppearance.KontextMenu);
            List<string> tags = new();
            var Cancel = false;
            var Translate = true;
            par.GetContextMenuItems(null, ThisContextMenu, out var HotItem, tags, ref Cancel, ref Translate);
            if (Cancel) { return null; }
            ContextMenuInitEventArgs ec = new(HotItem, tags, UserMenu);
            par.OnContextMenuInit(ec);
            return ec.Cancel ? null
                : ThisContextMenu != null && ThisContextMenu.Count > 0 ? par
                : UserMenu.Count > 0 ? par
                : null;
        }

        #endregion
    }
}