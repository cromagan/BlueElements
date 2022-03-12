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

using BlueBasics;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.ItemCollection.ItemCollectionList;

namespace BlueControls.Forms {

    /// <summary>
    /// Typischerweise für Dropdownmenüs oder dem KontextMenu
    /// </summary>
    public partial class FloatingInputBoxListBoxStyle : FloatingForm {

        #region Fields

        private bool _mouseWasDown;

        #endregion

        #region Constructors

        private FloatingInputBoxListBoxStyle() : base(enDesign.Form_QuickInfo) => InitializeComponent();

        private FloatingInputBoxListBoxStyle(ItemCollectionList? items, int xpos, int ypos, int steuerWi, object tag, Control? connectedControl, bool translate) : base(connectedControl, items.ControlDesign) {
            InitializeComponent();
            Tag = tag;
            // Design = Items.ControlDesign;
            xpos -= Skin.PaddingSmal;
            ypos -= Skin.PaddingSmal;
            Generate_ListBox1(items, steuerWi, enAddType.None, translate);
            //UnloadLostFocus = true;
            Position_SetWindowIntoScreen(Generic.PointOnScreenNr(new Point(xpos, ypos)), xpos, ypos);
            //Develop.DoEvents();
            Show();
            while (!string.IsNullOrEmpty(WindowsRemoteControl.LastMouseButton())) { Develop.DoEvents(); }
            //this.Focus();
            timer1.Enabled = true;
        }

        #endregion

        #region Events

        public event EventHandler Cancel;

        public event EventHandler<ContextMenuItemClickedEventArgs> ItemClicked;

        #endregion

        #region Methods

        public static void ContextMenuShow(IContextMenu control, MouseEventArgs? e) {
            Close(enBlueListBoxAppearance.KontextMenu);
            Close(control);

            ItemCollectionList thisContextMenu = new(enBlueListBoxAppearance.KontextMenu);
            ItemCollectionList userMenu = new(enBlueListBoxAppearance.KontextMenu);

            List<string> tags = new();
            var cancel = false;
            var translate = true;
            control.GetContextMenuItems(e, thisContextMenu, out var hotItem, tags, ref cancel, ref translate);
            if (cancel) { return; }

            ContextMenuInitEventArgs ec = new(hotItem, tags, userMenu);
            control.OnContextMenuInit(ec);
            if (ec.Cancel) { return; }
            if (!ec.Translate) { translate = false; }
            if (thisContextMenu.Count > 0 && userMenu.Count > 0) { thisContextMenu.AddSeparator(); }
            if (userMenu.Count > 0) { thisContextMenu.AddClonesFrom(userMenu); }

            var par = control.ParentControlWithCommands();
            if (thisContextMenu.Count > 0) {
                if (par != null) {
                    thisContextMenu.AddSeparator();
                    thisContextMenu.Add(enContextMenuComands.WeitereBefehle);
                }
                thisContextMenu.AddSeparator();
                thisContextMenu.Add(enContextMenuComands.Abbruch);
                List<object?> infos = new()
                {
                    userMenu,
                    hotItem,
                    tags,
                    control
                };
                var contextMenu = Show(thisContextMenu, infos, (Control)control, translate);
                contextMenu.ItemClicked += _ContextMenu_ItemClicked;
            } else {
                if (par != null) {
                    ContextMenuShow(par, e);
                }
            }
        }

        public static FloatingInputBoxListBoxStyle Show(ItemCollectionList? items, object tag, Control? connectedControl, bool translate) =>
            new(items, Cursor.Position.X - 8, Cursor.Position.Y - 8, -1, tag,
                connectedControl, translate);

        public static FloatingInputBoxListBoxStyle Show(ItemCollectionList? items, int xpos, int ypos, int steuerWi, object? tag, Control? connectedControl, bool translate) =>
            new(items, xpos, ypos, steuerWi, tag, connectedControl, translate);

        public void Generate_ListBox1(ItemCollectionList? items, int minWidth, enAddType addNewAllowed, bool translate) {
            //var itemsClone = (ItemCollectionList)ItemsOri.Clone();
            var (biggestItemX, _, heightAdded, _) = items.ItemData();
            if (addNewAllowed != enAddType.None) { heightAdded += 24; }
            lstbx.Appearance = (enBlueListBoxAppearance)items.ControlDesign;
            lstbx.Translate = translate;
            //if (data.Item4 == BlueBasics.Enums.enOrientation.Senkrecht)
            //{
            //    He += Skin.PaddingSmal * 2;
            //    He = Math.Max(He, 5 * 16 + Skin.PaddingSmal * 2 + 24);
            //    Wi = Math.Max(Wi, 250);
            //}
            //else
            //{
            //Wi = CInt(Wi * 1.05) 'Weil die Breite nur circa berechnet wird
            heightAdded++; // Um ja den Slider zu vermeiden!
            heightAdded = Math.Max(heightAdded, 16);
            biggestItemX = Math.Max(biggestItemX, 16);
            //}
            biggestItemX = Math.Max(biggestItemX, minWidth);
            var maxWi = (int)(Screen.PrimaryScreen.Bounds.Size.Width * 0.7);
            var maxHe = (int)(Screen.PrimaryScreen.Bounds.Size.Height * 0.7);
            if (biggestItemX > maxWi) { biggestItemX = maxWi; }
            if (heightAdded > maxHe) {
                heightAdded = maxHe;
                biggestItemX += 20;
            }
            Size = new Size(biggestItemX + (lstbx.Left * 2), heightAdded + (lstbx.Top * 2));
            lstbx.Item.CheckBehavior = items.CheckBehavior;
            lstbx.Item.AddClonesFrom(items);
        }

        public override void Refresh() {
            Develop.DebugPrint_InvokeRequired(InvokeRequired, true);
            base.Refresh();
            OnPaint(null);
        }

        private static void _ContextMenu_ItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
            var infos = (List<object>)e.HotItem;
            var userMmenu = (ItemCollectionList)infos[0];
            var hotItem = infos[1];
            var tags = (List<string>)infos[2];
            var ob = (IContextMenu)infos[3];
            Close(enBlueListBoxAppearance.KontextMenu);
            Close(ob);
            if (e.ClickedComand.ToLower() == "weiterebefehle") {
                var par = ob.ParentControlWithCommands();
                if (par != null) {
                    ContextMenuShow(par, null);
                }
                return;
            }
            if (e.ClickedComand.ToLower() == "abbruch") { return; }
            ContextMenuItemClickedEventArgs ex = new(e.ClickedComand, hotItem, tags);
            bool done;
            if (userMmenu[e.ClickedComand] == null) {
                done = ob.ContextMenuItemClickedInternalProcessig(sender, ex);
            } else {
                done = true; //keine Prüfung implementiert
                ob.OnContextMenuItemClicked(ex);
            }
            if (!done) {
                Develop.DebugPrint("Kontextmenu-Befehl nicht ausgeführt: " + e.ClickedComand);
            }
        }

        private void ListBox1_ItemClicked(object sender, BasicListItemEventArgs e) {
            // Selectet Chanched bringt nix, da es ja drum geht, ob eine Node angeklickt wurde.
            // Nur Listboxen können überhaupt erst Checked werden!
            // Ob sie Checked wird, ist egal!
            if (e.Item == null) { return; }

            // Einen Klick auf Überschriften einfach ignorieren, zB. kontextmenü
            if (!e.Item.IsClickable()) { return; }

            if (lstbx.Appearance is not enBlueListBoxAppearance.Listbox and not enBlueListBoxAppearance.Gallery and not enBlueListBoxAppearance.FileSystem) {
                OnItemClicked(new ContextMenuItemClickedEventArgs(e.Item.Internal, Tag, null)); // Das Tag hier ist eigentlich das HotItem
                if (!IsDisposed) { Close(); }
            }
        }

        private void OnCancel() => Cancel?.Invoke(this, System.EventArgs.Empty);

        private void OnItemClicked(ContextMenuItemClickedEventArgs e) => ItemClicked?.Invoke(this, e);

        private void timer1_Tick(object sender, System.EventArgs e) {
            var mouseIsDown = !string.IsNullOrEmpty(WindowsRemoteControl.LastMouseButton());
            if (mouseIsDown && !_mouseWasDown && !IsMouseInForm()) {
                // erster Klick ausserhalb des Forms
                Close();
                OnCancel();
                return;
            }
            if (_mouseWasDown && !mouseIsDown && IsMouseInForm()) {
                // Maus ausserhalb der Form ausgelassen
                _mouseWasDown = false;
                return;
            }
            if (mouseIsDown) { _mouseWasDown = true; }
        }

        #endregion
    }
}