#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2021 Christian Peter
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
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.ItemCollection;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
namespace BlueControls.Controls {
    [Designer(typeof(BasicDesigner))]
    [DefaultEvent("TextChanged")]
    public partial class ComboBox : TextBox {
        #region Constructor
        public ComboBox() : base() {
            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();
            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            _MouseHighlight = true;
            SetStyle(System.Windows.Forms.ControlStyles.ContainerControl, true);
            Item = new ItemCollectionList(enBlueListBoxAppearance.DropdownSelectbox);
            Item.ItemAdded += _Item_ItemAdded;
            Item.ItemCheckedChanged += _Item_ItemCheckedChanged;
            Item.ItemRemoved += _Item_ItemRemoved;
            btnDropDown.Left = Width - btnDropDown.Width;
            btnDropDown.Top = 0;
            btnDropDown.Height = Height;
        }
        #endregion
        #region  Variablen 
        private string _Initialtext;
        private bool _btnDropDownIsIn = false;
        private System.Windows.Forms.ComboBoxStyle _DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
        private enComboboxStyle _DrawStyle = enComboboxStyle.TextBox;
        private string _ImageCode = string.Empty;
        private string _LastClickedText = "####~~~~|||||";
        #endregion
        #region  Properties 
        [DefaultValue("")]
        [Category("Darstellung")]
        [Editor(typeof(QuickPicSelector), typeof(UITypeEditor))]
        public string ImageCode {
            get => _ImageCode;
            set {
                if (_ImageCode != value) {
                    _ImageCode = value;
                    Invalidate();
                }
                SetStyle();
            }
        }
        public ItemCollectionList Item { get; }
        [DefaultValue(System.Windows.Forms.ComboBoxStyle.DropDown)]
        public System.Windows.Forms.ComboBoxStyle DropDownStyle {
            get => _DropDownStyle;
            set {
                if (value != _DropDownStyle) {
                    _DropDownStyle = value;
                    Invalidate();
                }
                SetStyle();
            }
        }
        [DefaultValue(enComboboxStyle.TextBox)]
        public enComboboxStyle DrawStyle {
            get => _DrawStyle;
            set {
                if (_DrawStyle != value) {
                    _DrawStyle = value;
                    Invalidate();
                }
                SetStyle();
            }
        }
        #endregion
        #region  Events 
        public event EventHandler<BasicListItemEventArgs> ItemClicked;
        public event EventHandler DropDownShowing;
        #endregion
        private void DropDownMenu_Cancel(object sender, object MouseOver) {
            Item.UncheckAll();
            FloatingForm.Close(this);
            Focus();
        }
        private void DropDownMenu_ItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
            Item.UncheckAll();
            FloatingForm.Close(this);
            if (!string.IsNullOrEmpty(e.ClickedComand)) {
                _LastClickedText = e.ClickedComand;
                Text = e.ClickedComand;
                OnItemClicked(new BasicListItemEventArgs(Item[e.ClickedComand]));
            }
            Focus();
        }
        protected virtual void OnItemClicked(BasicListItemEventArgs e) => ItemClicked?.Invoke(this, e);
        protected override enDesign GetDesign() => ParentType() is enPartentType.RibbonGroupBox or enPartentType.RibbonPage
                                        ? enDesign.Ribbon_ComboBox_Textbox
                                        : enDesign.ComboBox_Textbox;

        protected override void DrawControl(Graphics gr, enStates state) {
            if (_DrawStyle != enComboboxStyle.TextBox) {
                if (string.IsNullOrEmpty(_Initialtext) && !string.IsNullOrEmpty(Text)) { _Initialtext = Text; }
                Button.DrawButton(this, gr, (enDesign)_DrawStyle, state, QuickImage.Get(_ImageCode), enAlignment.Horizontal_Vertical_Center, true, null, _Initialtext, DisplayRectangle, Translate);
                DoReturn();
                return;
            }
            btnDropDown.Enabled = Item.Count > 0;
            var vType = enDesign.ComboBox_Textbox;
            if (ParentType() is enPartentType.RibbonGroupBox or enPartentType.RibbonPage) {
                vType = enDesign.Ribbon_ComboBox_Textbox;
            }
            var i = Item[Text];
            if (i == null) {
                base.DrawControl(gr, state);
                DoReturn();
                return;
            }
            i.SetParent(Item); // Um den Stil zu wissen
            if (Focused() && _DropDownStyle == System.Windows.Forms.ComboBoxStyle.DropDown) {
                // Focused = Bearbeitung erwünscht, Cursor anzeigen und KEINE Items zeichnen
                base.DrawControl(gr, state);
                DoReturn();
                return;
            }
            if (_DropDownStyle == System.Windows.Forms.ComboBoxStyle.DropDown) {
                if (i is TextListItem tempVar2) {
                    if (tempVar2.Symbol == null && tempVar2.IsClickable()) {
                        base.DrawControl(gr, state);
                        DoReturn();
                        return;
                    }
                }
            }
            Skin.Draw_Back(gr, vType, state, DisplayRectangle, this, true);
            if (!FloatingForm.IsShowing(this)) {
                // Nur wenn die Selectbox gerade Nicht angezeigt wird, um hin und her Konvertierungen zu vermeiden
                var r = i.Pos;
                var ymod = -(int)((DisplayRectangle.Height - i.SizeUntouchedForListBox().Height) / 2.0);
                i.SetCoordinates(new Rectangle(Skin.PaddingSmal, -ymod, Width - 30, i.SizeUntouchedForListBox().Height));
                i.Draw(gr, 0, 0, enDesign.ComboBox_Textbox, enDesign.ComboBox_Textbox, state, false, string.Empty, Translate);
                i.SetCoordinates(r);
            }
            Skin.Draw_Border(gr, vType, state, DisplayRectangle);
            DoReturn();
            void DoReturn() =>
                //gr.DrawImage(_BitmapOfControl, 0, 0);
                //TMPGR.Dispose();
                btnDropDown.Invalidate();
        }
        internal bool WasThisValueClicked() => Text == _LastClickedText;
        protected override void OnEnabledChanged(System.EventArgs e) {
            base.OnEnabledChanged(e);
            FloatingForm.Close(this);
            btnDropDown.Enabled = Enabled;
            btnDropDown.Invalidate();
        }
        public void ShowMenu(object sender, System.Windows.Forms.MouseEventArgs ex) {
            if (_btnDropDownIsIn) { return; }
            if (IsDisposed) { return; }
            if (!Enabled) { return; }
            _btnDropDownIsIn = true;
            OnDropDownShowing();
            if (Item.Count == 0) { _btnDropDownIsIn = false; return; }
            int X, Y;
            if (sender is Button But) {
                X = System.Windows.Forms.Cursor.Position.X - But.MousePos().X - But.Location.X;
                Y = System.Windows.Forms.Cursor.Position.Y - But.MousePos().Y + Height; //Identisch
            } else {
                X = System.Windows.Forms.Cursor.Position.X - MousePos().X;
                Y = System.Windows.Forms.Cursor.Position.Y - MousePos().Y + Height; //Identisch
            }
            Item.UncheckAll();
            if (_DrawStyle != enComboboxStyle.RibbonBar && Item[Text] != null) { Item[Text].Checked = true; }
            var DropDownMenu = FloatingInputBoxListBoxStyle.Show(Item, X, Y, Width, null, this, Translate);
            DropDownMenu.Cancel += DropDownMenu_Cancel;
            DropDownMenu.ItemClicked += DropDownMenu_ItemClicked;
            _btnDropDownIsIn = false;
        }
        private void OnDropDownShowing() => DropDownShowing?.Invoke(this, System.EventArgs.Empty);
        protected override void OnGotFocus(System.EventArgs e) {
            if (_DropDownStyle == System.Windows.Forms.ComboBoxStyle.DropDownList) {
                btnDropDown.Focus();
            } else {
                base.OnGotFocus(e);
            }
            FloatingForm.Close(this);
        }
        protected override void OnLostFocus(System.EventArgs e) {
            Invalidate();
            CheckLostFocus(e);
        }
        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e) {
            if (e.Button != System.Windows.Forms.MouseButtons.Right) {
                // nicht bei rechts, ansonsten gibt's evtl. Kontextmenü (von der Textbox aus gesteuert) UND den Auswahldialog
                if (_DropDownStyle == System.Windows.Forms.ComboBoxStyle.DropDownList) {
                    ShowMenu(this, e);
                } else {
                    base.OnMouseUp(e);
                }
            } else {
                base.OnMouseUp(e);
            }
        }
        private void btnDropDown_LostFocus(object sender, System.EventArgs e) => CheckLostFocus(e);
        private void CheckLostFocus(System.EventArgs e) {
            try {
                if (btnDropDown == null) { return; }
                if (!btnDropDown.Focused && !Focused() && !FloatingForm.IsShowing(this)) { base.OnLostFocus(e); }
            } catch (Exception) {
            }
        }
        private void _Item_ItemAdded(object sender, ListEventArgs e) {
            if (IsDisposed) { return; }
            if (_btnDropDownIsIn) { return; }
            FloatingForm.Close(this);
            Invalidate();
        }
        private void _Item_ItemCheckedChanged(object sender, System.EventArgs e) {
            if (IsDisposed) { return; }
            if (_btnDropDownIsIn) { return; }
            FloatingForm.Close(this);
            Invalidate();
        }
        private void SetStyle() {
            if (DrawStyle != enComboboxStyle.TextBox) {
                Cursor = System.Windows.Forms.Cursors.Arrow;
                _DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                btnDropDown.Visible = false;
                // ImageCode = string.Empty; - Egal, wird eh ignoriert wenn es nicht gebraucht wird
            }
        }
        //protected override void OnMouseEnter(System.EventArgs e)
        //{
        //    base.OnMouseEnter(e);
        //    if (_DrawStyle == enComboboxStyle.TextBox || !Enabled) { return; }
        //    Invalidate();
        //}
        //protected override void OnMouseLeave(System.EventArgs e)
        //{
        //    base.OnMouseLeave(e);
        //    if (_DrawStyle == enComboboxStyle.TextBox || !Enabled) { return; }
        //    Invalidate();
        //}
        protected override void OnTextChanged(System.EventArgs e) {
            base.OnTextChanged(e);
            FloatingForm.Close(this);
        }
        private void _Item_ItemRemoved(object sender, System.EventArgs e) {
            if (IsDisposed) { return; }
            if (_btnDropDownIsIn) { return; }
            FloatingForm.Close(this);
            Invalidate();
        }
        public void SetTextIfItemExists(string text) {
            foreach (var thisItem in Item) {
                if (thisItem.Internal == text) {
                    Text = text;
                    return;
                }
            }
        }
    }
}
