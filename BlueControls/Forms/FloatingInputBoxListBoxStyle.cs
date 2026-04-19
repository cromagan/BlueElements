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
using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Forms;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Forms;

public partial class FloatingInputBoxListBoxStyle : FloatingForm {

    #region Fields

    private bool _mouseWasDown;

    #endregion

    #region Constructors

    private FloatingInputBoxListBoxStyle(List<AbstractListItem> items, CheckBehavior checkBehavior, List<string>? check, int xpos, int ypos, int steuerWi, Control? connectedControl, bool translate, ListBoxAppearance controlDesign, Design itemDesign, bool autosort, bool removeAllowed, ReadOnlyCollection<AbstractListItem>? customContextMenuItems, object? hotItem) : base(connectedControl, (Design)controlDesign) {
        InitializeComponent();
        xpos -= Skin.PaddingSmal;
        ypos -= Skin.PaddingSmal;
        Generate_ListBox1(items, checkBehavior, check, steuerWi, AddType.None, translate, controlDesign, itemDesign, autosort, removeAllowed, customContextMenuItems);

        if (_connectedControl is IContextMenu cm) {
            lstbx.ContextMenuConnectedControl = cm;
            if (cm.CustomContextMenuItems != null) {
                lstbx.CustomContextMenuItems = cm.CustomContextMenuItems;
            }
        }

        Position_SetWindowIntoScreen(Generic.PointOnScreenNr(new Point(xpos, ypos)), xpos, ypos);
        Show();
        while (!string.IsNullOrEmpty(WindowsRemoteControl.LastMouseButton())) { Develop.DoEvents(); }
        var timer = new System.Threading.Timer(_ => {
            if (IsHandleCreated) { BeginInvoke(new Action(Timer1_Tick)); }
        }, null, 10, 10);
        Disposed += (_, _) => timer.Dispose();
    }

    #endregion

    #region Events

    public event EventHandler? Cancel;

    public event EventHandler<AbstractListItemEventArgs>? ItemClicked;

    public event EventHandler<AbstractListItemEventArgs>? ItemRemoved;

    #endregion

    #region Methods

    public static FloatingInputBoxListBoxStyle Show(List<AbstractListItem> items, CheckBehavior checkBehavior, List<string>? check, Control? connectedControl, bool translate, ListBoxAppearance controlDesign, Design itemDesign, bool autosort) => new(items, checkBehavior, check, Cursor.Position.X - 8, Cursor.Position.Y - 8, -1, connectedControl,
            translate, controlDesign, itemDesign, autosort, false, null, null);

    public static FloatingInputBoxListBoxStyle Show(List<AbstractListItem> items, CheckBehavior checkBehavior, List<string>? check, int xpos, int ypos, int steuerWi, Control? connectedControl, bool translate, ListBoxAppearance controlDesign, Design itemDesign, bool autosort) => new(items, checkBehavior, check, xpos, ypos, steuerWi, connectedControl, translate, controlDesign, itemDesign, autosort, false, null, null);

    public static FloatingInputBoxListBoxStyle Show(List<AbstractListItem> items, CheckBehavior checkBehavior, List<string>? check, Control? connectedControl, bool translate, ListBoxAppearance controlDesign, Design itemDesign, bool autosort, bool removeAllowed) => new(items, checkBehavior, check, Cursor.Position.X - 8, Cursor.Position.Y - 8, -1, connectedControl,
            translate, controlDesign, itemDesign, autosort, removeAllowed, null, null);

    public static FloatingInputBoxListBoxStyle Show(List<AbstractListItem> items, CheckBehavior checkBehavior, List<string>? check, Control? connectedControl, bool translate, ListBoxAppearance controlDesign, Design itemDesign, bool autosort, bool removeAllowed, object? hotItem) => new(items, checkBehavior, check, Cursor.Position.X - 8, Cursor.Position.Y - 8, -1, connectedControl,
            translate, controlDesign, itemDesign, autosort, removeAllowed, null, hotItem);

    public static FloatingInputBoxListBoxStyle ShowComboBoxDropDown(List<AbstractListItem> items, string check, int xpos, int ypos, int steuerWi, Control? connectedControl, bool translate, bool autosort, bool removeAllowed, ReadOnlyCollection<AbstractListItem>? customContextMenuItems) => new(items, CheckBehavior.SingleSelection, [check], xpos, ypos, steuerWi, connectedControl, translate, ListBoxAppearance.DropdownSelectbox, Design.Item_DropdownMenu, autosort, removeAllowed, null, null);

    public void Generate_ListBox1(List<AbstractListItem> items, CheckBehavior checkBehavior, List<string>? check, int minWidth, AddType addNewAllowed, bool translate, ListBoxAppearance controlDesign, Design itemDesign, bool autosort, bool removeAllowed, ReadOnlyCollection<AbstractListItem>? customContextMenuItems) {
        var (biggestItemX, _, heightAdded, _) = items.CanvasItemData(itemDesign);
        if (addNewAllowed != AddType.None) { heightAdded += 24; }
        lstbx.Appearance = controlDesign;
        lstbx.Translate = translate;
        lstbx.AutoSort = autosort;
        lstbx.RemoveAllowed = removeAllowed;
        lstbx.CustomContextMenuItems = customContextMenuItems;

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
        if (check != null) { lstbx.Check(check, true); }
        lstbx.CheckBehavior = checkBehavior;
    }

    public override void Refresh() {
        Develop.DebugPrint_InvokeRequired(InvokeRequired, true);
        base.Refresh();
        OnPaint(null);
    }

    protected override void Dispose(bool disposing) {
        if (disposing) { components?.Dispose(); }
        base.Dispose(disposing);
    }

    private void ListBox1_ItemClicked(object sender, AbstractListItemEventArgs e) {
        // Selectet Chanched bringt nix, da es ja drum geht, ob eine Node angeklickt wurde.
        // Nur Listboxen können überhaupt erst Checked werden!
        // Ob sie Checked wird, ist egal!

        // Einen Klick auf Überschriften einfach ignorieren, zB. kontextmenü
        if (!e.Item.IsClickable()) { return; }

        if (lstbx.Appearance is not ListBoxAppearance.Listbox and not ListBoxAppearance.Listbox_Boxes and not ListBoxAppearance.Gallery and not ListBoxAppearance.FileSystem and not ListBoxAppearance.ButtonList) {
            OnItemClicked(e); // Das Control.Tag hier ist eigentlich das HotItem
            if (!IsDisposed) { Close(); }
        }
    }

    private void ListBox1_ItemRemoved(object sender, AbstractListItemEventArgs e) => ItemRemoved?.Invoke(this, e);

    private void OnCancel() => Cancel?.Invoke(this, System.EventArgs.Empty);

    private void OnItemClicked(AbstractListItemEventArgs e) => ItemClicked?.Invoke(this, e);

    private void Timer1_Tick() {
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