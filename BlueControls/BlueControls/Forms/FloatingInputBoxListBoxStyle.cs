// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using BlueControls.ItemCollectionList;

namespace BlueControls.Forms;

/// <summary>
/// Typischerweise für Dropdownmenüs oder dem KontextMenu
/// </summary>
public partial class FloatingInputBoxListBoxStyle : FloatingForm {

    #region Fields

    private bool _mouseWasDown;

    #endregion

    #region Constructors

    private FloatingInputBoxListBoxStyle() : base(Design.Form_QuickInfo) => InitializeComponent();

    private FloatingInputBoxListBoxStyle(List<AbstractListItem> items, CheckBehavior checkBehavior, List<string>? check, int xpos, int ypos, int steuerWi, object? hotitem, Control? connectedControl, bool translate, ListBoxAppearance controlDesign, Design itemDesign, bool autosort) : base(connectedControl, (Design)controlDesign) {
        InitializeComponent();
        Tag = hotitem;
        // Design = Items.ControlDesign;
        xpos -= Skin.PaddingSmal;
        ypos -= Skin.PaddingSmal;
        Generate_ListBox1(items, checkBehavior, check, steuerWi, AddType.None, translate, controlDesign, itemDesign, autosort);
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

    public event EventHandler? Cancel;

    public event EventHandler<ContextMenuItemClickedEventArgs>? ItemClicked;

    #endregion

    #region Methods

    public static void ContextMenuShow(IContextMenu control, MouseEventArgs? e) {
        Close(ListBoxAppearance.KontextMenu);
        Close(control);

        var thisContextMenu = new List<AbstractListItem>();
        var userMenu = new List<AbstractListItem>();

        var translate = true;
        control.GetContextMenuItems(e, thisContextMenu, out var hotItem);

        ContextMenuInitEventArgs ec = new(hotItem, userMenu);
        control.OnContextMenuInit(ec);
        if (ec.Cancel) { return; }
        if (!ec.Translate) { translate = false; }
        if (thisContextMenu.Count > 0 && userMenu.Count > 0) { thisContextMenu.Add(Separator()); }
        if (userMenu.Count > 0) { thisContextMenu.AddRange(userMenu); }

        var par = control.ParentControlWithCommands();
        if (thisContextMenu.Count > 0) {
            if (par != null) {
                thisContextMenu.Add(Separator());
                thisContextMenu.Add(ItemOf(ContextMenuCommands.WeitereBefehle));
            }
            thisContextMenu.Add(Separator());
            thisContextMenu.Add(ItemOf(ContextMenuCommands.Abbruch));
            List<object?> infos =
            [
                userMenu,
                hotItem,
                control
            ];
            var contextMenu = Show(thisContextMenu, CheckBehavior.NoSelection, null, infos, (Control)control, translate, ListBoxAppearance.KontextMenu, Design.Item_KontextMenu, false);
            contextMenu.ItemClicked += _ContextMenu_ItemClicked;
        } else {
            if (par != null) {
                ContextMenuShow(par, e);
            }
        }
    }

    public static FloatingInputBoxListBoxStyle Show(List<AbstractListItem> items, CheckBehavior checkBehavior, List<string>? check, object tag, Control? connectedControl, bool translate, ListBoxAppearance controlDesign, Design itemDesign, bool autosort) =>
        new(items, checkBehavior, check, Cursor.Position.X - 8, Cursor.Position.Y - 8, -1, tag,
            connectedControl, translate, controlDesign, itemDesign, autosort);

    public static FloatingInputBoxListBoxStyle Show(List<AbstractListItem> items, CheckBehavior checkBehavior, List<string>? check, int xpos, int ypos, int steuerWi, object? tag, Control? connectedControl, bool translate, ListBoxAppearance controlDesign, Design itemDesign, bool autosort) =>
        new(items, checkBehavior, check, xpos, ypos, steuerWi, tag, connectedControl, translate, controlDesign, itemDesign, autosort);

    public void Generate_ListBox1(List<AbstractListItem> items, CheckBehavior checkBehavior, List<string>? check, int minWidth, AddType addNewAllowed, bool translate, ListBoxAppearance controlDesign, Design itemDesign, bool autosort) {
        var (biggestItemX, _, heightAdded, _) = BlueControls.Controls.ListBox.ItemData(items, itemDesign);
        if (addNewAllowed != AddType.None) { heightAdded += 24; }
        lstbx.Appearance = controlDesign;
        lstbx.Translate = translate;
        lstbx.AutoSort = autosort;

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
        lstbx.CheckBehavior = CheckBehavior.MultiSelection;
        lstbx.ItemAddRange(items);
        if (check != null) { lstbx.Check(check); }
        lstbx.CheckBehavior = checkBehavior;
    }

    public override void Refresh() {
        Develop.DebugPrint_InvokeRequired(InvokeRequired, true);
        base.Refresh();
        OnPaint(null);
    }

    private static void _ContextMenu_ItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
        if (e.HotItem == null) { return; }

        var infos = (List<object>)e.HotItem;
        var userMmenu = (List<AbstractListItem>)infos[0];
        var hotItem = infos[1];
        var ob = (IContextMenu)infos[2];
        Close(ListBoxAppearance.KontextMenu);
        Close(ob);
        if (e.Item.KeyName.ToLowerInvariant() == "weiterebefehle") {
            var par = ob.ParentControlWithCommands();
            if (par != null) {
                ContextMenuShow(par, null);
            }
            return;
        }
        if (e.Item.KeyName.ToLowerInvariant() == "abbruch") { return; }

        ContextMenuItemClickedEventArgs ex = new(e.Item, hotItem);
        bool done;
        if (userMmenu.Get(e.Item.KeyName) == null) {
            done = ob.ContextMenuItemClickedInternalProcessig(sender, ex);
        } else {
            done = true; //keine Prüfung implementiert
            ob.OnContextMenuItemClicked(ex);
        }
        if (!done) {
            Develop.DebugPrint("Kontextmenu-Befehl nicht ausgeführt: " + e.Item.KeyName);
        }
    }

    private void ListBox1_ItemClicked(object sender, AbstractListItemEventArgs e) {
        // Selectet Chanched bringt nix, da es ja drum geht, ob eine Node angeklickt wurde.
        // Nur Listboxen können überhaupt erst Checked werden!
        // Ob sie Checked wird, ist egal!

        // Einen Klick auf Überschriften einfach ignorieren, zB. kontextmenü
        if (!e.Item.IsClickable()) { return; }

        if (lstbx.Appearance is not ListBoxAppearance.Listbox and not ListBoxAppearance.Listbox_Boxes and not ListBoxAppearance.Gallery and not ListBoxAppearance.FileSystem) {
            OnItemClicked(new ContextMenuItemClickedEventArgs(e.Item, Tag)); // Das Control.Tag hier ist eigentlich das HotItem
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