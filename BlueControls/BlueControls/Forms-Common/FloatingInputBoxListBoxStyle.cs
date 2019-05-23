#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
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
#endregion

using BlueBasics;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.ItemCollection;
using System;
using System.Drawing;

namespace BlueControls.DialogBoxes
{

    /// <summary>
    /// Typischerweise für Dropdownmenüs oder dem KontextMenu
    /// </summary>
    public partial class FloatingInputBoxListBoxStyle : BlueControls.Forms.FloatingForm
    {
        public FloatingInputBoxListBoxStyle()
        {
            InitializeComponent();
        }

        private FloatingInputBoxListBoxStyle(ItemCollectionList Items, int Xpos, int Ypos, int SteuerWi, object Tag, System.Windows.Forms.Control ConnectedControl, bool Translate) : base(ConnectedControl)

        {
            InitializeComponent();
            this.Tag = Tag;
            Design = Items.ControlDesign;

            Xpos -= GenericControl.Skin.PaddingSmal;
            Ypos -= GenericControl.Skin.PaddingSmal;

            Generate_ListBox1(Items, SteuerWi, enAddType.None, Translate);

            //UnloadLostFocus = true;

            Position_SetWindowIntoScreen(modAllgemein.PointOnScreenNr(new Point(Xpos, Ypos)), Xpos, Ypos);


            Develop.DoEvents();
            Show();


            while (!string.IsNullOrEmpty(modAllgemein.LastMouseButton())) { Develop.DoEvents(); }

            //this.Focus();

            timer1.Enabled = true;

        }



        public event EventHandler<ContextMenuItemClickedEventArgs> ItemClicked;
        public event EventHandler Cancel;


        private bool _MouseWasDown = false;


        public static FloatingInputBoxListBoxStyle Show(ItemCollectionList Items, object Tag, System.Windows.Forms.Control ConnectedControl, bool Translate)
        {
            return new FloatingInputBoxListBoxStyle(Items, System.Windows.Forms.Cursor.Position.X - 8, System.Windows.Forms.Cursor.Position.Y - 8, -1, Tag, ConnectedControl, Translate);
        }


        public static FloatingInputBoxListBoxStyle Show(ItemCollectionList Items, int Xpos, int Ypos, int SteuerWi, object Tag, System.Windows.Forms.Control ConnectedControl ,bool Translate)
        {
            return new FloatingInputBoxListBoxStyle(Items, Xpos, Ypos, SteuerWi, Tag, ConnectedControl, Translate);
        }

        #region  ListBox1 

        public void Generate_ListBox1(ItemCollectionList ItemsOri, int MinWidth, enAddType AddNewAllowed, bool Translate)
        {

            var itemsClone = (ItemCollectionList)ItemsOri.Clone();


            var He = (int)(itemsClone.HeigthOfAllItemsAdded(int.MaxValue));
            var Wi = (int)(itemsClone.WidthOfBiggestItem(int.MaxValue));

            if (AddNewAllowed != enAddType.None) { He += 24; }


            lstbx.Appearance = (enBlueListBoxAppearance)itemsClone.ControlDesign;
            lstbx.Translate = Translate;



            if (lstbx.Appearance == enBlueListBoxAppearance.Listbox ||
                lstbx.Appearance == enBlueListBoxAppearance.Gallery ||
                lstbx.Appearance == enBlueListBoxAppearance.FileSystem)
            {
                He += GenericControl.Skin.PaddingSmal * 2;
                He = Math.Max(He, 5 * 16 + GenericControl.Skin.PaddingSmal * 2 + 24);
                Wi = Math.Max(Wi, 250);
            }
            else
            {
                //Wi = CInt(Wi * 1.05) 'Weil die Breite nur circa berechnet wird
                He += 1; // Um ja den Slider zu vermeiden!
                He = Math.Max(He, 16);
                Wi = Math.Max(Wi, 16);
            }

            Wi = Math.Max(Wi, MinWidth);

            var MaxWi = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size.Width * 0.7);
            var MaxHe = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size.Height * 0.7);



            if (Wi > MaxWi)
            {
                Wi = MaxWi;
            }

            if (He > MaxHe)
            {
                He = MaxHe;
                Wi += 20;
            }


            this.Size = new Size(Wi + lstbx.Left * 2, He + lstbx.Top * 2);




            lstbx.Item.CheckBehavior = itemsClone.CheckBehavior;
            lstbx.Item.AddRange(itemsClone);


        }

        private void ListBox1_Item_Click(object sender, BasicListItemEventArgs e)
        {

            // Selectet Chanched bringt nix, da es ja drum geht, ob eine Node angeklickt wurde.
            // Nur Listboxen können überhaupt erst Checked werden!
            // Ob sie Checked wird, ist egal!



            if (e.Item != null)
            {

                // Einen Klick auf Überschriften einfach ignorieren, zB. kontextmenü
                if (!e.Item.IsClickable()) { return; }


                if (lstbx.Appearance != enBlueListBoxAppearance.Listbox && lstbx.Appearance != enBlueListBoxAppearance.Gallery && lstbx.Appearance != enBlueListBoxAppearance.FileSystem)
                {
                    OnItemClicked(new ContextMenuItemClickedEventArgs(this.Tag, e.Item));
                    if (!IsDisposed) { Close(); }
                    return;
                }
            }
        }

        private void OnItemClicked(ContextMenuItemClickedEventArgs e)
        {
            ItemClicked?.Invoke(this, e);
        }


        #endregion





        private void OnCancel()
        {
            Cancel?.Invoke(this, System.EventArgs.Empty);
        }

        private void timer1_Tick(object sender, System.EventArgs e)
        {

            var MouseIsDown = !string.IsNullOrEmpty(modAllgemein.LastMouseButton());

            if (MouseIsDown && !_MouseWasDown && !this.IsMouseInForm())
            {
                // erster Klick ausserhalb des Forms
                Close();
                OnCancel();
                return;
            }

            if (_MouseWasDown && !MouseIsDown && this.IsMouseInForm())
            {
                // Maus ausserhalb der Form ausgelassen
                _MouseWasDown = false;
                return;
            }

            if (MouseIsDown) { _MouseWasDown = true; }

        }

        public override void Refresh()
        {
            Develop.DebugPrint_InvokeRequired(InvokeRequired, true);
            base.Refresh();
            OnPaint(null);
        }


    }
}
