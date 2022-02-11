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
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.Forms {

    /// <summary>
    /// Typischerweise für Dropdownmenüs oder dem KontextMenu
    /// </summary>
    public partial class FloatingInputBoxListBoxStyle : FloatingForm {

        #region Fields

        private bool _MouseWasDown = false;

        #endregion

        #region Constructors

        private FloatingInputBoxListBoxStyle() : base(enDesign.Form_QuickInfo) => InitializeComponent();

        private FloatingInputBoxListBoxStyle(ItemCollectionList Items, int Xpos, int Ypos, int SteuerWi, object Tag, System.Windows.Forms.Control ConnectedControl, bool Translate) : base(ConnectedControl, Items.ControlDesign) {
            InitializeComponent();
            this.Tag = Tag;
            // Design = Items.ControlDesign;
            Xpos -= Skin.PaddingSmal;
            Ypos -= Skin.PaddingSmal;
            Generate_ListBox1(Items, SteuerWi, enAddType.None, Translate);
            //UnloadLostFocus = true;
            Position_SetWindowIntoScreen(Generic.PointOnScreenNr(new Point(Xpos, Ypos)), Xpos, Ypos);
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

        public static void ContextMenuShow(IContextMenu Control, System.Windows.Forms.MouseEventArgs e) {
            Close(enBlueListBoxAppearance.KontextMenu);
            Close(Control);

            ItemCollectionList ThisContextMenu = new(enBlueListBoxAppearance.KontextMenu);
            ItemCollectionList UserMenu = new(enBlueListBoxAppearance.KontextMenu);

            List<string> tags = new();
            var Cancel = false;
            var Translate = true;
            Control.GetContextMenuItems(e, ThisContextMenu, out var HotItem, tags, ref Cancel, ref Translate);
            if (Cancel) { return; }

            ContextMenuInitEventArgs ec = new(HotItem, tags, UserMenu);
            Control.OnContextMenuInit(ec);
            if (ec.Cancel) { return; }
            if (!ec.Translate) { Translate = false; }
            if (ThisContextMenu.Count > 0 && UserMenu.Count > 0) { ThisContextMenu.AddSeparator(); }
            if (UserMenu.Count > 0) { ThisContextMenu.AddClonesFrom(UserMenu); }

            var par = Control.ParentControlWithCommands();
            if (ThisContextMenu.Count > 0) {
                if (par != null) {
                    ThisContextMenu.AddSeparator();
                    ThisContextMenu.Add(enContextMenuComands.WeitereBefehle);
                }
                ThisContextMenu.AddSeparator();
                ThisContextMenu.Add(enContextMenuComands.Abbruch);
                List<object> Infos = new()
                {
                    UserMenu,
                    HotItem,
                    tags,
                    Control
                };
                var _ContextMenu = Show(ThisContextMenu, Infos, (System.Windows.Forms.Control)Control, Translate);
                _ContextMenu.ItemClicked += _ContextMenu_ItemClicked;
            } else {
                if (par != null) {
                    ContextMenuShow(par, e);
                }
            }
        }

        public static FloatingInputBoxListBoxStyle Show(ItemCollectionList Items, object Tag, System.Windows.Forms.Control ConnectedControl, bool Translate) => new(Items, System.Windows.Forms.Cursor.Position.X - 8, System.Windows.Forms.Cursor.Position.Y - 8, -1, Tag, ConnectedControl, Translate);

        public static FloatingInputBoxListBoxStyle Show(ItemCollectionList Items, int Xpos, int Ypos, int SteuerWi, object Tag, System.Windows.Forms.Control ConnectedControl, bool Translate) => new(Items, Xpos, Ypos, SteuerWi, Tag, ConnectedControl, Translate);

        public void Generate_ListBox1(ItemCollectionList items, int MinWidth, enAddType AddNewAllowed, bool Translate) {
            //var itemsClone = (ItemCollectionList)ItemsOri.Clone();
            (var BiggestItemX, var _, var HeightAdded, var _) = items.ItemData();
            if (AddNewAllowed != enAddType.None) { HeightAdded += 24; }
            lstbx.Appearance = (enBlueListBoxAppearance)items.ControlDesign;
            lstbx.Translate = Translate;
            //if (data.Item4 == BlueBasics.Enums.enOrientation.Senkrecht)
            //{
            //    He += Skin.PaddingSmal * 2;
            //    He = Math.Max(He, 5 * 16 + Skin.PaddingSmal * 2 + 24);
            //    Wi = Math.Max(Wi, 250);
            //}
            //else
            //{
            //Wi = CInt(Wi * 1.05) 'Weil die Breite nur circa berechnet wird
            HeightAdded++; // Um ja den Slider zu vermeiden!
            HeightAdded = Math.Max(HeightAdded, 16);
            BiggestItemX = Math.Max(BiggestItemX, 16);
            //}
            BiggestItemX = Math.Max(BiggestItemX, MinWidth);
            var MaxWi = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size.Width * 0.7);
            var MaxHe = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size.Height * 0.7);
            if (BiggestItemX > MaxWi) { BiggestItemX = MaxWi; }
            if (HeightAdded > MaxHe) {
                HeightAdded = MaxHe;
                BiggestItemX += 20;
            }
            Size = new Size(BiggestItemX + (lstbx.Left * 2), HeightAdded + (lstbx.Top * 2));
            lstbx.Item.CheckBehavior = items.CheckBehavior;
            lstbx.Item.AddClonesFrom(items);
        }

        public override void Refresh() {
            Develop.DebugPrint_InvokeRequired(InvokeRequired, true);
            base.Refresh();
            OnPaint(null);
        }

        private static void _ContextMenu_ItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
            var Infos = (List<object>)e.HotItem;
            var UserMmenu = (ItemCollectionList)Infos[0];
            var HotItem = Infos[1];
            var Tags = (List<string>)Infos[2];
            var ob = (IContextMenu)Infos[3];
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
            ContextMenuItemClickedEventArgs ex = new(e.ClickedComand, HotItem, Tags);
            bool done;
            if (UserMmenu[e.ClickedComand] == null) {
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
            if (e.Item != null) {
                // Einen Klick auf Überschriften einfach ignorieren, zB. kontextmenü
                if (!e.Item.IsClickable()) { return; }
                if (lstbx.Appearance is not enBlueListBoxAppearance.Listbox and not enBlueListBoxAppearance.Gallery and not enBlueListBoxAppearance.FileSystem) {
                    OnItemClicked(new ContextMenuItemClickedEventArgs(e.Item.Internal, Tag, null)); // Das Tag hier ist eigentlich das HotItem
                    if (!IsDisposed) { Close(); }
                    return;
                }
            }
        }

        private void OnCancel() => Cancel?.Invoke(this, System.EventArgs.Empty);

        private void OnItemClicked(ContextMenuItemClickedEventArgs e) => ItemClicked?.Invoke(this, e);

        private void timer1_Tick(object sender, System.EventArgs e) {
            var MouseIsDown = !string.IsNullOrEmpty(WindowsRemoteControl.LastMouseButton());
            if (MouseIsDown && !_MouseWasDown && !IsMouseInForm()) {
                // erster Klick ausserhalb des Forms
                Close();
                OnCancel();
                return;
            }
            if (_MouseWasDown && !MouseIsDown && IsMouseInForm()) {
                // Maus ausserhalb der Form ausgelassen
                _MouseWasDown = false;
                return;
            }
            if (MouseIsDown) { _MouseWasDown = true; }
        }

        #endregion
    }
}