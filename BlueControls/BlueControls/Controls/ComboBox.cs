﻿using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueControls.Designer_Support;
using BlueControls.DialogBoxes;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;

namespace BlueControls.Controls
{
    [DefaultEvent("Item_Click")]
    public partial class ComboBox : IBackgroundNone
    {



        private string Initialtext;


        public ComboBox()
        {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            Item = new ItemCollectionList(enBlueListBoxAppearance.DropdownSelectbox);

            Item.ItemAdded += _Item_ItemAdded;
            Item.ItemCheckedChanged += _Item_ItemCheckedChanged;
            Item.ItemRemoved += _Item_ItemRemoved;

            SetDoubleBuffering();
        }


        #region  Variablen 
        //<Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Never)>

        private System.Windows.Forms.ComboBoxStyle _DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
        private enComboboxStyle _DrawStyle = enComboboxStyle.Standard;
        private string _ImageCode = "";


        #endregion


        #region  Properties 

        [DefaultValue("")]
        [Category("Darstellung")]
        [Editor(typeof(QuickPicSelector), typeof(UITypeEditor))]
        public string ImageCode
        {
            get
            {
                if (_DrawStyle == enComboboxStyle.Standard)
                {
                    return string.Empty;
                }
                return _ImageCode;
            }
            set
            {

                if (_DrawStyle == enComboboxStyle.Standard) { value = string.Empty; }
                if (_ImageCode == value) { return; }

                _ImageCode = value;
                Invalidate();
            }
        }



        public ItemCollectionList Item { get; }

        [DefaultValue(System.Windows.Forms.ComboBoxStyle.DropDown)]
        public System.Windows.Forms.ComboBoxStyle DropDownStyle
        {
            get
            {
                return _DropDownStyle;
            }

            set
            {
                if (value == _DropDownStyle)
                {
                    return;
                }
                _DropDownStyle = value;
                Invalidate();
            }
        }

        [DefaultValue(enComboboxStyle.Standard)]
        public enComboboxStyle DrawStyle
        {
            get
            {
                return _DrawStyle;
            }
            set
            {
                if (_DrawStyle == value)
                {
                    return;
                }

                _DrawStyle = value;

                if (DrawStyle == enComboboxStyle.RibbonBar)
                {
                    SetAlternateStyle();
                }

                Invalidate();
            }
        }



        #endregion

        #region  Events 
        public event EventHandler<BasicListItemEventArgs> ItemClicked;
        #endregion



        private void DropDownMenu_Cancel(object sender, object MouseOver)
        {
            Item.UncheckAll();
            FloatingInputBoxListBoxStyle.Close(this);
            Focus();
        }


        private void DropDownMenu_ItemClicked(object sender, ContextMenuItemClickedEventArgs e)
        {

            Item.UncheckAll();

            FloatingInputBoxListBoxStyle.Close(this);

            if (!string.IsNullOrEmpty(e.ClickedComand.Internal()))
            {
                Text = e.ClickedComand.Internal();
                OnItemClicked(new BasicListItemEventArgs(Item[e.ClickedComand.Internal()]));

            }

            Focus();

        }

        private void OnItemClicked(BasicListItemEventArgs e)
        {
            ItemClicked?.Invoke(this, e);
        }



        protected override void DrawControl(Graphics GR, enStates vState)
        {

            if (_DrawStyle == enComboboxStyle.RibbonBar)
            {

                if (string.IsNullOrEmpty(Initialtext) && !string.IsNullOrEmpty(Text)) { Initialtext = Text; }

                SetAlternateStyle();
                RowItem tempVar = null;
                Button.DrawButton(this, GR, ref tempVar, enDesign.Ribbonbar_Button, vState, QuickImage.Get(_ImageCode), enAlignment.VerticalCenter_Left, true, null, Initialtext, DisplayRectangle);
                return;
            }





            BB.Enabled = Item.Count > 0;

            enDesign vType = 0;
            if (ParentType() == enPartentType.RibbonGroupBox || ParentType() == enPartentType.RibbonPage)
            {
                vType = enDesign.Ribbon_ComboBox_Textbox;
            }
            else
            {
                vType = enDesign.ComboBox_Textbox;
            }


            var i = Item[Text];
            if (i == null)
            {
                base.DrawControl(GR, vState);
                return;
            }

            i.Parent = Item; // Um den Stil zu wissen


            if (Focused() && _DropDownStyle == System.Windows.Forms.ComboBoxStyle.DropDown)
            {
                // Focused = Bearbeitung erwünscht, Cursor anzeigen und KEINE Items zeichnen
                base.DrawControl(GR, vState);
                return;
            }


            if (_DropDownStyle == System.Windows.Forms.ComboBoxStyle.DropDown)
            {
                if (i is TextListItem tempVar2)
                {
                    if (tempVar2.Symbol == null && tempVar2.IsClickable())
                    {
                        base.DrawControl(GR, vState);
                        return;
                    }
                }
            }


            // ERST HIER! Manche Routinen ändern die Größe des Textfeldes.
            if (_BitmapOfControl == null) { _BitmapOfControl = new Bitmap(ClientSize.Width, ClientSize.Height, PixelFormat.Format32bppPArgb); }
            var TMPGR = Graphics.FromImage(_BitmapOfControl);


            Skin.Draw_Back(TMPGR, vType, vState, DisplayRectangle, this, true);

            if (!FloatingInputBoxListBoxStyle.IsShowing(this))
            {
                // Nur wenn die Selectbox gerade Nicht angezeigt wird, um hin und her Konvertierungen zu vermeiden


                var r = i.Pos;
                var l = i.Parent.Appearance;

                i.Parent.Appearance = enBlueListBoxAppearance.ComboBox_Textbox;

                var ymod = -Convert.ToInt32((DisplayRectangle.Height - i.SizeUntouchedForListBox().Height) / 2.0);
                i.SetCoordinates(new Rectangle(Skin.PaddingSmal, -ymod, Width - 30, (int)i.SizeUntouchedForListBox().Height));


                i.Draw(TMPGR, 0, 0, vState, false, string.Empty);

                i.Parent.Appearance = l;
                i.SetCoordinates(r);


            }



            BB.Invalidate();
            Skin.Draw_Border(TMPGR, vType, vState, DisplayRectangle);

            GR.DrawImage(_BitmapOfControl, 0, 0);
            TMPGR.Dispose();

        }





        protected override void OnEnabledChanged(System.EventArgs e)
        {
            base.OnEnabledChanged(e);

            FloatingInputBoxListBoxStyle.Close(this);

            BB.Enabled = Enabled;
            BB.Invalidate();
        }



        private void BB_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (IsDisposed) { return; }
            if (!Enabled) { return; }
            //if (DropDownMenu != null) { return; }
            if (Item.Count == 0) { return; }
            int X, Y;



            if (sender is Button But)
            {
                X = System.Windows.Forms.Cursor.Position.X - But.MousePos().X - But.Location.X;
                Y = System.Windows.Forms.Cursor.Position.Y - But.MousePos().Y + Height; //Identisch
            }
            else
            {
                X = System.Windows.Forms.Cursor.Position.X - MousePos().X;
                Y = System.Windows.Forms.Cursor.Position.Y - MousePos().Y + Height; //Identisch
            }



            Item.UncheckAll();

            if (_DrawStyle != enComboboxStyle.RibbonBar && Item[Text] != null) { Item[Text].Checked = true; }

            var DropDownMenu = FloatingInputBoxListBoxStyle.Show(Item, X, Y, Width, null, this);
            DropDownMenu.Cancel += DropDownMenu_Cancel;
            DropDownMenu.ItemClicked += DropDownMenu_ItemClicked;




        }



        protected override void OnGotFocus(System.EventArgs e)
        {
            base.OnGotFocus(e);

            if (_DropDownStyle == System.Windows.Forms.ComboBoxStyle.DropDownList) { BB.Focus(); }
            FloatingInputBoxListBoxStyle.Close(this);

        }

        protected override void OnLostFocus(System.EventArgs e)
        {
            Invalidate();
            CheckLostFocus(e);
        }

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button != System.Windows.Forms.MouseButtons.Right)
            {
                // nicht bei rechts, ansonsten gibt's evtl. Kontextmenü (von der Textbox aus gesteuert) UND den Auswahldialog
                if (_DropDownStyle == System.Windows.Forms.ComboBoxStyle.DropDownList)
                {
                    BB_MouseUp(this, e);
                }
            }

        }


        private void BB_LostFocus(object sender, System.EventArgs e)
        {
            CheckLostFocus(e);
        }

        private void CheckLostFocus(System.EventArgs e)
        {

            try
            {
                if (BB == null) { return; }

                if (!BB.Focused && !Focused() && !FloatingInputBoxListBoxStyle.IsShowing(this)) { base.OnLostFocus(e); }

            }
            catch (Exception)
            {

            }





        }

        private void _Item_ItemAdded(object sender, ListEventArgs e)
        {
            if (IsDisposed) { return; }

            FloatingInputBoxListBoxStyle.Close(this);
            Invalidate();
        }

        private void _Item_ItemCheckedChanged(object sender, System.EventArgs e)
        {

            if (IsDisposed) { return; }

            FloatingInputBoxListBoxStyle.Close(this);
            Invalidate();
        }

        private void SetAlternateStyle()
        {
            Cursor = System.Windows.Forms.Cursors.Arrow;
            _DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            BB.Visible = false;
        }

        protected override void OnMouseEnter(System.EventArgs e)
        {
            base.OnMouseEnter(e);
            if (_DrawStyle == enComboboxStyle.Standard || !Enabled) { return; }
            Invalidate();
        }


        protected override void OnMouseLeave(System.EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_DrawStyle == enComboboxStyle.Standard || !Enabled) { return; }
            Invalidate();
        }


        protected override void OnTextChanged(System.EventArgs e)
        {
            base.OnTextChanged(e);
            FloatingInputBoxListBoxStyle.Close(this);
        }

        private void _Item_ItemRemoved(object sender, System.EventArgs e)
        {

            if (IsDisposed) { return; }


            FloatingInputBoxListBoxStyle.Close(this);
            Invalidate();
        }
    }
}
