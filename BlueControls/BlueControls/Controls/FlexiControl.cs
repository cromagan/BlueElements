﻿#region BlueElements - a collection of useful tools, database and controls
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

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueControls.Enums;
using System.Windows.Forms;
using BlueControls.Designer_Support;

namespace BlueControls.Controls
{
    [Designer(typeof(BasicDesigner))]
    [DefaultEvent("ValueChanged")]
    public partial class FlexiControl : GenericControl, IQuickInfo, IBackgroundBitmap
    {


        /// <summary>
        /// Wenn True, wird ValueChanged NICHT ausgelöst
        /// </summary>
        protected bool _IsFilling;

        public readonly string ValueId;
        private string _Value = string.Empty;
        protected enEditTypeFormula _EditType = enEditTypeFormula.None; // None ist -1 und muss gesetzt sein!
        protected enÜberschriftAnordnung _CaptionPosition = enÜberschriftAnordnung.ohne;
        protected string _Caption;

        private Bitmap _BitmapOfControl;
        private bool _GeneratingBitmapOfControl;

        private int _ControlX = -1;
        private Color _Color = Color.Transparent;
        private Caption _CaptionObject;
        private Caption _InfoCaption;
        private string _InfoText = string.Empty;
        bool _ShowInfoWhenDisabled = false;


        private bool _MultiLine = false;
        private string _Suffix = string.Empty;
        private enDataFormat _Format = enDataFormat.Text;

        protected bool _allinitialized = false;
        protected bool _enabled = false;
        protected bool _InstantChangedEvent = true;
        public event EventHandler RemovingAll;

        public event EventHandler NeedRefresh;
        public event EventHandler ButtonClicked;
        public event EventHandler ValueChanged;


        #region  Constructor 




        public FlexiControl()
        {
            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        }



        /// <summary>
        /// Erstellt ein einfaches Linien-Element.
        /// </summary>
        /// <param name="vType"></param>
        public FlexiControl(bool Horizontal)
        {
            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            _EditType = enEditTypeFormula.Line;
            Size = new Size(200, 8);

            if (!Horizontal) { Develop.DebugPrint_NichtImplementiert(); }
        }

        /// <summary>
        /// Erstellt einen Ja/Nein-Knopf.
        /// </summary>
        /// <param name="caption"></param>
        /// <param name="value"></param>
        public FlexiControl(string caption, bool value)
        {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            _EditType = enEditTypeFormula.Ja_Nein_Knopf;
            _Caption = caption + ":";
            ValueId = caption;
            _CaptionPosition = enÜberschriftAnordnung.Links_neben_Dem_Feld;

            var s = BlueFont.MeasureString(_Caption, Skin.GetBlueFont(enDesign.Caption, enStates.Standard).Font());

            Size = new Size((int)s.Width + 30, 22);

            CreateSubControls();
            this.Value = value.ToPlusMinus();

        }


        /// <summary>
        /// Einfacher Info Text. Wird nirgends mehr zurück gegeben.
        /// </summary>
        /// <param name="CaptionText"></param>
        public FlexiControl(string CaptionText)
        {
            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            _EditType = enEditTypeFormula.None;
            _Caption = CaptionText;
            _CaptionPosition = enÜberschriftAnordnung.Links_neben_Dem_Feld;
            ValueId = string.Empty;


            var s = BlueFont.MeasureString(_Caption, Skin.GetBlueFont(enDesign.Caption, enStates.Standard).Font());

            Size = new Size((int)(s.Width + 2), (int)(s.Height + 2));

        }

        /// <summary>
        /// Initialisiert das FlexControl als einfachen Button zum klicken
        /// </summary>
        /// <param name="CaptionText"></param>
        /// <param name="Pic"></param>
        public FlexiControl(string CaptionText, enImageCode Pic)
        {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            _EditType = enEditTypeFormula.Button;
            _Caption = CaptionText;
            ValueId = CaptionText;
            _CaptionPosition = enÜberschriftAnordnung.ohne;
            _Value = string.Empty;


            var s = BlueFont.MeasureString(_Caption, Skin.GetBlueFont(enDesign.Caption, enStates.Standard).Font());
            Size = new Size((int)s.Width + 50, 30);

            var c = CreateSubControls();

            ((Button)c).ImageCode = QuickImage.Get(Pic).ToString();

        }

        /// <summary>
        /// Initialisiert das FlexControl als Textbox
        /// </summary>
        /// <param name="CaptionText"></param>
        /// <param name="InitialValue"></param>
        /// <param name="Format"></param>
        /// <param name="TextLines"></param>
        public FlexiControl(string CaptionText, string InitialValue, enDataFormat Format, int TextLines)
        {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            _EditType = enEditTypeFormula.Textfeld;

            _MultiLine = false;


            if (TextLines >= 2)
            {
                _CaptionPosition = enÜberschriftAnordnung.Über_dem_Feld;
                Size = new Size(200, 16 + 24 * TextLines);
                _MultiLine = true;
            }
            else
            {
                _CaptionPosition = enÜberschriftAnordnung.Links_neben_Dem_Feld;
                Size = new Size(200, 24);
                _MultiLine = false;
            }


            _Caption = CaptionText + ":";
            ValueId = CaptionText;
            _Format = Format;
            _InstantChangedEvent = true;

            var c = CreateSubControls();

            StyleTextBox((TextBox)c, string.Empty, false);

            Value = InitialValue;
        }

        /// <summary>
        /// Erstellt eine Combobox
        /// </summary>
        /// <param name="CaptionText"></param>
        /// <param name="InitialValue"></param>
        /// <param name="list"></param>
        public FlexiControl(string CaptionText, string InitialValue, ItemCollectionList list)
        {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            _EditType = enEditTypeFormula.Textfeld_mit_Auswahlknopf;
            _Caption = CaptionText + ":";
            ValueId = CaptionText;
            _CaptionPosition = enÜberschriftAnordnung.Links_neben_Dem_Feld;
            //_Values = list;
            //_Value = cValue;



            list.Appearance = enBlueListBoxAppearance.ComboBox_Textbox;
            var s = BlueFont.MeasureString(_Caption, Skin.GetBlueFont(enDesign.Caption, enStates.Standard).Font());
            var x = Math.Max((int)(list.WidthOfBiggestItem(500) + 20 + s.Width), 200);
            var y = Math.Max((int)(list.HeightOfBiggestItem(100) + Skin.PaddingSmal * 2), 24);

            Size = new Size(x, y);


            var c = CreateSubControls();

            StyleComboBox((ComboBox)c, list, System.Windows.Forms.ComboBoxStyle.DropDownList);

            Value = InitialValue;
        }







        #region  QuickInfo 
        // Dieser Codeblock ist im Interface IQuickInfo herauskopiert und muss überall Identisch sein.
        private string _QuickInfo = "";
        [Category("Darstellung")]
        [DefaultValue("")]
        [Description("QuickInfo des Steuerelementes - im extTXT-Format")]
        public string QuickInfo
        {
            get
            {
                return _QuickInfo;
            }
            set
            {
                if (_QuickInfo != value)
                {
                    Forms.QuickInfo.Close();
                    _QuickInfo = value;
                    UpdateControls();
                }
            }
        }
        #endregion








        #endregion




        #region  Properties 

        [DefaultValue(false)]
        public bool ShowInfoWhenDisabled
        {
            get { return _ShowInfoWhenDisabled; }
            set
            {
                if (_ShowInfoWhenDisabled == value) { return; }
                _ShowInfoWhenDisabled = value;
                Invalidate();
            }
        }


        [DefaultValue(typeof(Color), "Transparent")]
        public Color Color
        {
            get
            {
                return _Color;
            }
            set
            {

                if (_Color == value) { return; }

                _Color = value;
                Invalidate();
            }
        }


        [DefaultValue(true)]
        public new bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                if (_enabled == value) { return; }
                _enabled = value;


                foreach (System.Windows.Forms.Control ThisControl in Controls)
                {
                    if (ThisControl.Name == "Info")
                    {
                        ThisControl.Enabled = true;
                    }
                    else
                    {
                        ThisControl.Enabled = value;
                    }
                }

                Invalidate();
            }
        }


        [DefaultValue("")]
        public string Value
        {
            get
            {
                if (_Value == null) { return string.Empty; }
                return _Value;
            }
            set
            {
                if (value == null) { value = string.Empty; }
                if (_Value == value) { return; }
                if (_IsFilling) { return; }
                _Value = value;
                UpdateValueToControl();
                OnValueChanged();
            }
        }


        /// <summary>
        /// Falls das Steuerelement eine Suffix unterstützt, wird dieser angezeigt
        /// </summary>
        [DefaultValue("")]
        public string Suffix
        {
            get
            {
                return _Suffix;
            }
            set
            {

                if (_Suffix == value) { return; }
                _Suffix = value;
                UpdateControls();
            }
        }

        /// <summary>
        /// Falls das Steuerelement Multiline unterstützt, wird dieser angezeigt
        /// </summary>
        [DefaultValue(false)]
        public bool MultiLine
        {
            get
            {
                return _MultiLine;
            }
            set
            {

                if (_MultiLine == value) { return; }
                _MultiLine = value;
                UpdateControls();
            }
        }



        /// <summary>
        /// Falls das Steuerelement eine InstantChangeEvent unterstützt, wird dieses umgesetzt
        /// </summary>
        [DefaultValue(true)]
        public bool InstantChangedEvent
        {
            get
            {
                return _InstantChangedEvent;
            }
            set
            {

                if (_InstantChangedEvent == value) { return; }
                _InstantChangedEvent = value;


                UpdateControls();

            }
        }


        /// <summary>
        /// Falls das Steuerelement eine Suffix unterstützt, wird dieser angezeigt
        /// </summary>
        [DefaultValue(enDataFormat.Text)]
        public enDataFormat Format
        {
            get
            {
                return _Format;
            }
            set
            {

                if (_Format == value) { return; }
                _Format = value;


                UpdateControls();

            }
        }










        [DefaultValue(enEditTypeFormula.None)]
        public enEditTypeFormula EditType
        {
            get
            {
                return _EditType;
            }
            set
            {
                if (_EditType == value) { return; }
                RemoveAll(); // Controls and Events entfernen!
                _EditType = value;
            }
        }



        /// <summary>
        /// Ab welchen Wert in Pixel das Eingabesteuerelement beginnen darf.
        /// </summary>
        [DefaultValue(-1)]
        public int ControlX
        {
            get
            {
                return _ControlX;
            }
            set
            {
                if (_ControlX == value) { return; }
                RemoveAll(); // Controls and Events entfernen!
                _ControlX = value;
            }
        }


        [DefaultValue("")]
        public string InfoText
        {
            get
            {
                return _InfoText;
            }
            set
            {
                if (_InfoText == value) { return; }
                _InfoText = value;
                Invalidate();
            }
        }




        [DefaultValue(enÜberschriftAnordnung.ohne)]
        public enÜberschriftAnordnung CaptionPosition
        {
            get
            {
                return _CaptionPosition;
            }
            set
            {
                if (_CaptionPosition == value) { return; }
                RemoveAll(); // Controls and Events entfernen!
                _CaptionPosition = value;
            }
        }

        [DefaultValue("")]
        public string Caption
        {
            get
            {
                return _Caption;
            }
            set
            {
                if (_Caption == value) { return; }
                RemoveAll(); // Controls and Events entfernen!
                _Caption = value;
            }
        }


        [DefaultValue("")]
        public string FileEncryptionKey { get; set; }


        #endregion






        protected override void DrawControl(Graphics gr, enStates state)
        {
            if (_BitmapOfControl == null) { _BitmapOfControl = new Bitmap(ClientSize.Width, ClientSize.Height, PixelFormat.Format32bppPArgb); }


            // Enabled wurde verdeckt!
            if (!Enabled) { state = enStates.Standard_Disabled; }


            var TMPGR = Graphics.FromImage(_BitmapOfControl);

            Skin.Draw_Back_Transparent(TMPGR, ClientRectangle, this);




            if (_Color.A != 0)
            {
                if (state.HasFlag(enStates.Standard_Disabled))
                {
                    var br = (byte)(_Color.GetBrightness() * 254);
                    var lgb = new LinearGradientBrush(ClientRectangle, Color.FromArgb(br, br, br), Color.Transparent, LinearGradientMode.Horizontal);
                    TMPGR.FillRectangle(lgb, ClientRectangle);
                }
                else
                {
                    var lgb = new LinearGradientBrush(ClientRectangle, _Color, Color.Transparent, LinearGradientMode.Horizontal);
                    TMPGR.FillRectangle(lgb, ClientRectangle);
                }
            }

            if (!_allinitialized) { CreateSubControls(); }


            if (_EditType == enEditTypeFormula.Listbox_6_Zeilen || _EditType == enEditTypeFormula.Listbox_1_Zeile || _EditType == enEditTypeFormula.Listbox_3_Zeilen)
            {
                ListBoxen(out var Main, out var Suggest);

                if (Suggest != null)
                {
                    var tmpstate = state;
                    if (tmpstate != enStates.Checked_Disabled) { tmpstate = enStates.Standard; }

                    var R = new Rectangle
                    {
                        X = Main.Left - 1,
                        Y = Main.Top - 1,
                        Width = Main.Width + 2,
                        Height = Height - Main.Top - 1
                    };

                    Skin.Draw_Border(TMPGR, enDesign.ListBox, tmpstate, R);
                }
            }

            gr.DrawImage(_BitmapOfControl, 0, 0);
            TMPGR.Dispose();

            DoInfoTextCaption(!state.HasFlag(enStates.Standard_Disabled));
        }

        protected override void InitializeSkin()
        {

        }

        /// <summary>
        /// Setzt den aktuellen Wert, so dass es das Control anzeigt. Die Filling-Variable wird währenddessen umgesetzt.
        /// sollte vor StandardBehandlung kommen, da dort das Objekt gesetzt wird und dann die Handler generiert werden.
        /// </summary>
        private void UpdateValueToControl()
        {
            if (!_allinitialized) { CreateSubControls(); }

            _IsFilling = true;

            foreach (System.Windows.Forms.Control Control in Controls)
            {
                switch (Control)
                {

                    case ComboBox ComboBox:
                        UpdateValueTo_Combobox(ComboBox);
                        break;

                    case TextBox TextBox:
                        UpdateValueTo_TextBox(TextBox);
                        break;

                    case EasyPic EasyPic:
                        UpdateValueTo_EasyPic(EasyPic);
                        break;

                    case ListBox ListBox:
                        if (ListBox.Name == "Main") { UpdateValueTo_ListBox(); }
                        break;

                    case Button Button:
                        UpdateValueTo_Button(Button);
                        break;

                    case Caption _:
                        UpdateValueTo_Caption();
                        break;

                    case Line _:
                        if (!string.IsNullOrEmpty(_Value))
                        {
                            Develop.DebugPrint(enFehlerArt.Fehler, "Line kann keine Value erhalten: '" + _Value + "'");
                        }
                        break;

                    default:
                        Develop.DebugPrint(Typ(Control));
                        break;
                }
            }

            _IsFilling = false;

        }






        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);

            switch (e.Control)
            {
                case ComboBox ComboBox:
                    //ComboBox.ItemClicked += ComboBoxItemClicked;
                    ComboBox.TextChanged += ValueChanged_ComboBox;
                    break;

                case TextBox TextBox:
                    TextBox.TextChanged += ValueChanged_TextBox;
                    break;

                case Caption _:
                case Line _:
                    break;

                case EasyPic _:
                    // Control.ImageChanged += EasyPicImageChanged;
                    // Einzig und alleine eigene Datenbank kann den dazugehörigen Wert generieren.
                    break;

                case ListBox ListBox:
                    ListBox.ItemClicked += ListBox_ItemClicked;
                    ListBox.ItemAdded += ListBox_ItemAdded;
                    ListBox.ItemRemoved += ListBox_ItemRemoved;
                    break;

                case Button Button:
                    switch (_EditType)
                    {
                        case enEditTypeFormula.Ja_Nein_Knopf:
                            Button.CheckedChanged += YesNoButton_CheckedChanged;
                            break;

                        case enEditTypeFormula.Button:
                            Button.Click += ComandButton_Click;
                            break;

                        case enEditTypeFormula.Farb_Auswahl_Dialog:
                            Button.Click += ColorButton_Click;
                            break;

                        default:
                            Develop.DebugPrint_NichtImplementiert();
                            break;
                    }
                    break;

                default:
                    Develop.DebugPrint(Typ(e.Control));
                    break;
            }
            UpdateControls();
        }


        protected override void OnControlRemoved(System.Windows.Forms.ControlEventArgs e)
        {
            base.OnControlRemoved(e);

            switch (e.Control)
            {
                case ComboBox ComboBox:
                    //ComboBox.ItemClicked -= ComboBoxItemClicked;
                    ComboBox.TextChanged -= ValueChanged_ComboBox;
                    break;

                case TextBox TextBox:
                    TextBox.TextChanged -= ValueChanged_TextBox;
                    break;

                case Caption _:
                case Line _:
                    break;

                case EasyPic _:
                    break;

                case ListBox ListBox:
                    ListBox.ItemClicked -= ListBox_ItemClicked;
                    ListBox.ItemAdded -= ListBox_ItemAdded;
                    ListBox.ItemRemoved -= ListBox_ItemRemoved;
                    break;

                case Button Button:
                    switch (_EditType)
                    {
                        case enEditTypeFormula.Ja_Nein_Knopf:
                            Button.CheckedChanged -= YesNoButton_CheckedChanged;
                            break;

                        case enEditTypeFormula.Button:
                            Button.Click -= ComandButton_Click;
                            break;

                        case enEditTypeFormula.Farb_Auswahl_Dialog:
                            Button.Click -= ColorButton_Click;
                            break;

                        default:
                            Develop.DebugPrint_NichtImplementiert();
                            break;
                    }
                    break;

                default:
                    Develop.DebugPrint(Typ(e.Control));
                    break;
            }

        }


        /// <summary>
        /// Entfernt alle Controls und löst dessen die Events auf. Setzt _allinitialized auf false.
        /// </summary>
        protected virtual void RemoveAll()
        {


            if (Controls.Count > 0) { OnRemovingAll(); }

            while (Controls.Count > 0)
            {
                Controls[0].Visible = false;
                Controls[0].Dispose(); // Dispose entfernt da Control aus der Collection
                                       // Controls.Remove(Controls[0]);
            }

            //foreach (System.Windows.Forms.Control ThisControl in Controls)
            //{
            //    //Control.Parent.Controls.Remove(Control);
            //    //  OnControlRemoved(new ControlEventArgs(Control));

            //}
            Controls.Clear();
            _allinitialized = false;
            Invalidate();
        }

        protected virtual void OnRemovingAll()
        {
            RemovingAll?.Invoke(this, System.EventArgs.Empty);
        }

        protected virtual void OnNeedRefresh()
        {
            NeedRefresh?.Invoke(this, System.EventArgs.Empty);
        }


        #region  Caption 


        private void Control_Create_Caption()
        {
            if (_CaptionPosition == enÜberschriftAnordnung.ohne) { return; }

            _CaptionObject = new Caption();
            _CaptionObject.Enabled = _enabled;

            Controls.Add(_CaptionObject);

            _CaptionObject.TextAnzeigeVerhalten = enSteuerelementVerhalten.Text_Abschneiden; // nicht enSteuerelementVerhalten.Steuerelement_Anpassen! weil sonst beim einem Resize die Koordinaten geändert werden und das kann zum Ping Pong führen
            _CaptionObject.Text = _Caption;
            _CaptionObject.Size = _CaptionObject.TextRequiredSize();

            _CaptionObject.Left = 0;
            _CaptionObject.Top = 0;

            _CaptionObject.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;

            if (_CaptionPosition == enÜberschriftAnordnung.Ohne_mit_Abstand)
            {

                _CaptionObject.Visible = false;
            }
            else
            {
                _CaptionObject.Visible = true;
            }





            _CaptionObject.BringToFront();

        }


        /// <summary>
        /// Setzt den aktuellen Wert, so dass es das Control anzeigt. Filling muss TRUE sein.
        /// </summary>
        private void UpdateValueTo_Caption()
        {
            if (!_IsFilling) { Develop.DebugPrint(enFehlerArt.Fehler, "Filling muss TRUE sein!"); }
            //if (Column == null) { return; } // nur mögloch bei verbundenen Datenbanken

            if (_EditType != enEditTypeFormula.nur_als_Text_anzeigen) { return; } // und auch dann nur als reine Text anzeige

            if (_CaptionObject == null) { return; }
            _CaptionObject.Width = Width;
            _CaptionObject.Translate = false;
            _CaptionObject.Text = _Caption + " <i>" + _Value;


        }



        #endregion


        #region  Line 
        /// <summary>
        /// Erstellt das Steuerelement. Die Events werden Registriert und auch der Wert gesetzt. 
        /// </summary>
        private Line Control_Create_Line()
        {
            var Control = new Line
            {
                Enabled = _enabled,
                Orientation = enOrientation.Waagerecht
            };
            StandardBehandlung(Control);

            return Control;
        }

        #endregion

        #region  EasyPic 




        /// <summary>
        /// Erstellt das Steuerelement. Die Events werden Registriert und auch der Wert gesetzt. 
        /// </summary>
        private EasyPic Control_Create_EasyPic()
        {
            var Control = new EasyPic();
            Control.Enabled = _enabled;

            StandardBehandlung(Control);
            UpdateValueToControl();

            return Control;
        }

        /// <summary>
        /// Setzt den aktuellen Wert, so dass es das Control anzeigt. Filling  muss TRUE sein.
        /// </summary>
        private void UpdateValueTo_EasyPic(EasyPic Control)
        {
            if (!_IsFilling) { Develop.DebugPrint(enFehlerArt.Fehler, "Filling muss TRUE sein!"); }
            Control.FromFile(_Value);
        }

        #endregion


        #region  ComboBox 


        //private void ComboBoxItemClicked(object sender, BasicListItemEventArgs e)
        //{
        //    Value = e.Item.Internal;
        //}


        /// <summary>
        /// Erstellt das Steuerelement. Die Events werden Registriert und auch der Wert gesetzt.
        /// </summary>
        private ComboBox Control_Create_ComboBox()
        {
            var Control = new ComboBox();

            StyleComboBox(Control, null, System.Windows.Forms.ComboBoxStyle.DropDownList);

            UpdateValueToControl();
            StandardBehandlung(Control);
            return Control;
        }

        protected void StyleComboBox(ComboBox Control, ItemCollectionList list, System.Windows.Forms.ComboBoxStyle Style)
        {
            Control.Enabled = _enabled;
            Control.Format = _Format;
            Control.Suffix = _Suffix;
            Control.DropDownStyle = Style;
            Control.Item.Clear();
            Control.Item.AddRange(list);
            Control.Item.Sort();
        }

        /// <summary>
        /// Setzt den aktuellen Wert, so dass es das Control anzeigt. Filling muss TRUE sein.
        /// </summary>
        private void UpdateValueTo_Combobox(ComboBox Control)
        {
            if (!_IsFilling) { Develop.DebugPrint(enFehlerArt.Fehler, "Filling muss TRUE sein!"); }
            Control.Text = _Value;
        }

        private void ValueChanged_ComboBox(object sender, System.EventArgs e)
        {
            //if (_IsCreating || _IsFilling) { return; }
            Value = ((ComboBox)sender).Text;
        }








        #endregion


        #region  ListBox 
        private void ListBox_ItemClicked(object sender, BasicListItemEventArgs e)
        {

            ListBoxen(out var Main, out var Suggest);

            if (Suggest == null) { return; } // Wenn keine Vorschlagbox vorhanden ist, dann müssen die Items zum entfernen angewählt werden

            e.Item.Checked = false; // von einer Box zur anderen darf keine Markierung erhalten bleiben

            if (sender == Main)
            {
                MoveItemBetweenList(Main, Suggest, e.Item.Internal);
            }
            else
            {
                MoveItemBetweenList(Suggest, Main, e.Item.Internal);
            }

        }


        protected void MoveItemBetweenList(ListBox Source, ListBox Target, string Internal)
        {
            var SourceItem = Source.Item[Internal];
            var TargetItem = Target.Item[Internal];

            if (SourceItem != null && TargetItem == null)
            {
                TargetItem = (BasicListItem)SourceItem.Clone();
                Target.Item.Add(TargetItem);
            }
            else if (SourceItem == null && TargetItem == null)
            {
                TargetItem = new TextListItem(Internal);
                Target.Item.Add(TargetItem);
            }


            Target.Item.Sort();

            if (SourceItem != null) { Source.Item.Remove(SourceItem); }
        }


        private void ListBox_ItemRemoved(object sender, System.EventArgs e)
        {
            ListBoxen(out var Main, out var Suggest);
            if (sender == Suggest) { return; }
            Value = Main.Item.ToListOfString().JoinWithCr();
        }

        private void ListBox_ItemAdded(object sender, ListEventArgs e)
        {

            ListBoxen(out var Main, out var Suggest);
            if (sender == Suggest)
            {

                if (Main.Item.Contains((BasicListItem)e.Item)) { return; } // Es soll gerade aus dem Main entfernt werden, also Wirklich im Suggest lassen
                if (Suggest.Item.Contains((BasicListItem)e.Item)) { return; } // Es soll gerade aus dem Main entfernt werden, also Wirklich im Suggest lassen

                ListBox_ItemClicked(Suggest, new BasicListItemEventArgs((BasicListItem)e.Item)); // Gleich nach oben schieben
                return;
            }
            Value = Main.Item.ToListOfString().JoinWithCr();
        }



        protected void ListBoxen(out ListBox Main, out ListBox Suggest)
        {

            Main = null;
            Suggest = null;

            foreach (System.Windows.Forms.Control Control in Controls)
            {

                if (Control is ListBox LB)
                {
                    switch (LB.Name)
                    {
                        case "Main":
                            Main = LB;
                            break;
                        case "Suggest":
                            Suggest = LB;
                            break;
                    }
                }
            }




        }




        /// <summary>
        /// Erstellt das Steuerelement. Die Events werden Registriert und auch der Wert gesetzt.
        /// </summary>
        /// <param name="MainBox"></param>
        /// <param name="List"></param>
        /// <returns></returns>
        private ListBox Control_Create_SuggestListBox(ListBox MainBox, ItemCollectionList List)
        {
            var Control = new ListBox();
            Control.Enabled = _enabled;
            Control.Item.Clear();
            Control.Item.AddRange(List);
            Control.Item.CheckBehavior = enCheckBehavior.NoSelection;

            Control.Name = "Suggest";

            Control.AddAllowed = enAddType.None;
            Control.MoveAllowed = false;
            Control.FilterAllowed = true;
            Control.RemoveAllowed = false;
            Control.Appearance = enBlueListBoxAppearance.Listbox;

            Control.Item.Sort();

            StandardBehandlung(Control);

            Control.Top = MainBox.Bottom;
            Control.Height = Height - Control.Top - 1;


            Control.Height = Control.Height - 2;
            Control.Left = 2;
            Control.Width = Control.Width - 4;

            Control.BringToFront();



            return Control;
        }

        /// <summary>
        /// Erstellt das Steuerelement. Die Events werden Registriert und auch der Wert gesetzt.
        /// </summary>
        private ListBox Control_Create_MainListBox()
        {
            var Control = new ListBox();
            Control.Enabled = _enabled;
            Control.Name = "Main";

            StyleListBox(Control, null);
            UpdateValueToControl();
            StandardBehandlung(Control);

            return Control;
        }


        protected void StyleListBox(ListBox Control, ColumnItem Column)
        {
            Control.Enabled = _enabled;

            Control.Item.Clear();
            Control.Item.CheckBehavior = enCheckBehavior.MultiSelection;

            if (Column == null) { return; }

            var Item = new ItemCollectionList();

            if (Column.DropdownBearbeitungErlaubt)
            {
                ItemCollectionList.GetItemCollection(Item, Column, null, enShortenStyle.Both, 10000);

                if (!Column.DropdownWerteAndererZellenAnzeigen)
                {
                    bool again;
                    do
                    {
                        again = false;
                        foreach (var ThisItem in Item)
                        {
                            if (!Column.DropDownItems.Contains(ThisItem.Internal))
                            {
                                again = true;
                                Item.Remove(ThisItem);
                                break;
                            }
                        }
                    } while (again);
                }
            }


            var FullSize = Item.Count == 0;
            Control.AddAllowed = enAddType.UserDef;

            if (FullSize)
            {
                Control.RemoveAllowed = true;
            }
            else
            {

                Control.RemoveAllowed = false;
            }


            Control.FilterAllowed = false;
            Control.MoveAllowed = false;


            var Zeil = 1;

            switch (_EditType)
            {
                case enEditTypeFormula.Gallery:
                    Control.Appearance = enBlueListBoxAppearance.Gallery;
                    break;

                case enEditTypeFormula.Listbox_1_Zeile:
                    Zeil = 1;
                    Control.Appearance = enBlueListBoxAppearance.Listbox;
                    break;

                case enEditTypeFormula.Listbox_3_Zeilen:
                    Zeil = 3;
                    Control.Appearance = enBlueListBoxAppearance.Listbox;
                    break;

                case enEditTypeFormula.Listbox_6_Zeilen:
                    Zeil = 6;
                    Control.Appearance = enBlueListBoxAppearance.Listbox;
                    break;
            }


            Control.Left = 2;
            Control.Width = Control.Width - 4;

            if (!FullSize)
            {
                Control.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
                Control.Height = 16 * Zeil + 24;

            }

            if (Item.Count > 0) { Control_Create_SuggestListBox(Control, Item); }
        }

        /// <summary>
        /// Setzt den aktuellen Wert, so dass es das Control anzeigt. Filling muss TRUE sein.
        /// </summary>
        private void UpdateValueTo_ListBox()
        {
            if (!_IsFilling) { Develop.DebugPrint(enFehlerArt.Fehler, "Filling muss TRUE sein!"); }
            // Das muss OnChanged von FlexiControlForCell übernehmen.

            ListBoxen(out var Main, out var Suggest);

            var _Soll = _Value.SplitByCRToList();
            var _Ist = Main.Item.ToListOfString();
            _Ist.Remove(_Soll);


            if (Suggest != null)
            {

                // Zu viele im Main zu den Suggests schieben
                foreach (var ThisString in _Ist)
                {
                    if (!_Soll.Contains(ThisString)) { MoveItemBetweenList(Main, Suggest, ThisString); }
                }

                // Evtl. Suggests in die Mainlist verschieben.
                foreach (var ThisString in _Soll)
                {
                    MoveItemBetweenList(Suggest, Main, ThisString);
                }
            }
            else
            {

                // Zu viele im Mains aus der Liste löschen
                foreach (var ThisString in _Ist)
                {
                    if (!_Soll.Contains(ThisString)) { Main.Item.Remove(ThisString); }
                }

                // und die Mains auffüllen
                foreach (var ThisString in _Soll)
                {
                    if (!_Ist.Contains(ThisString))
                    {

                        if (FileOperations.FileExists(ThisString))
                        {
                            if (ThisString.FileType() == enFileFormat.Image)
                            {
                                Main.Item.Add(new BitmapListItem(ThisString, ThisString.FileNameWithoutSuffix(), ThisString, FileEncryptionKey));
                            }
                            else
                            {
                                Main.Item.Add(new TextListItem(ThisString, ThisString.FileNameWithSuffix(), QuickImage.Get(ThisString.FileType(), 48)));
                            }
                        }
                        else
                        {
                            Main.Item.Add(ThisString);
                        }
                    }
                }
            }
        }

        #endregion

        #region  Button 



        private void ComandButton_Click(object sender, System.EventArgs e)
        {
            if (_EditType != enEditTypeFormula.Button) { return; }
            Value = "+"; // Geklickt, wurde hiermit vermerkt

            OnButtonClicked();
        }

        private void ColorButton_Click(object sender, System.EventArgs e)
        {

            Develop.DebugPrint_NichtImplementiert(); // TODO: Erstellen!
            //if (_EditType != enEditTypeFormula.Button) { return; }
            //CheckIfChanged("+");

            //OnButtonClicked();

        }

        protected virtual void OnButtonClicked()
        {
            ButtonClicked?.Invoke(this, System.EventArgs.Empty);
        }

        private void YesNoButton_CheckedChanged(object sender, System.EventArgs e)
        {
            Value = ((Button)sender).Checked.ToPlusMinus();
        }

        /// <summary>
        /// Erstellt das Steuerelement. Die Events werden Registriert und auch der Wert gesetzt.
        /// </summary>
        private Button Control_Create_ButtonComand()
        {
            var Control = new Button
            {
                Enabled = _enabled,
                Name = "ComandButton",
                Checked = false,
                ButtonStyle = enButtonStyle.Button,
                Text = _Caption,
            };

            StandardBehandlung(Control);

            return Control;
        }


        /// <summary>
        /// Erstellt das Steuerelement. Die Events werden Registriert und auch der Wert gesetzt.
        /// </summary>
        private Button Control_Create_ButtonColor()
        {
            var Control = new Button
            {
                Enabled = _enabled,
                Name = "ColorButton",
                Checked = false,
                ButtonStyle = enButtonStyle.Button,
                Text = string.Empty
            };

            StandardBehandlung(Control);

            return Control;
        }



        /// <summary>
        /// Erstellt das Steuerelement. Die Events werden Registriert und auch der Wert gesetzt. 
        /// </summary>
        private Button Control_Create_ButtonYesNo()
        {
            var Control = new Button
            {
                Enabled = _enabled,
                Name = "YesNoButton",
                ButtonStyle = enButtonStyle.Yes_or_No,
                Text = "",
                ImageCode = ""
            };

            UpdateValueToControl();
            StandardBehandlung(Control);

            return Control;
        }

        /// <summary>
        /// Setzt den aktuellen Wert, so dass es das Control anzeigt. Filling muss TRUE sein.
        /// </summary>
        private void UpdateValueTo_Button(Button Control)
        {

            if (!_IsFilling) { Develop.DebugPrint(enFehlerArt.Fehler, "Filling und Creating False!"); }

            switch (_EditType)
            {
                case enEditTypeFormula.Ja_Nein_Knopf:
                    Control.Checked = _Value.FromPlusMinus();
                    break;

                case enEditTypeFormula.Button:
                    break;

                case enEditTypeFormula.Farb_Auswahl_Dialog:
                    if (string.IsNullOrEmpty(_Value))
                    {
                        Control.ImageCode = "Fragezeichen|24";
                    }
                    else
                    {
                        Control.ImageCode = "Kreis|24|||" + Color.FromArgb(int.Parse(_Value)).ToHTMLCode();
                    }
                    break;

                default:
                    Develop.DebugPrint_NichtImplementiert();
                    break;
            }
        }


        #endregion

        #region  Textbox 


        /// <summary>
        /// Erstellt das Steuerelement. Die Events werden Registriert und auch der Wert gesetzt.
        /// </summary>
        private TextBox Control_Create_TextBox()
        {
            var Control = new TextBox();
            _InstantChangedEvent = true;
            StyleTextBox(Control, string.Empty, false);
            UpdateValueToControl();
            StandardBehandlung(Control);
            return Control;
        }




        protected void StyleTextBox(TextBox Control, string AllowedChars, bool SpellChecking)
        {
            Control.Enabled = _enabled;
            Control.Format = _Format;
            Control.Suffix = _Suffix;
            Control.AllowedChars = AllowedChars;
            Control.MultiLine = _MultiLine;
            Control.SpellChecking = SpellChecking;
            Control.InstantChangedEvent = _InstantChangedEvent;

            if (_MultiLine || Height > 20)
            {
                Control.Verhalten = enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            }
            else
            {
                Control.Verhalten = enSteuerelementVerhalten.Scrollen_ohne_Textumbruch;
            }

        }

        /// <summary>
        /// Setzt den aktuellen Wert, so dass es das Control anzeigt. Filling muss TRUE sein.
        /// </summary>
        private void UpdateValueTo_TextBox(TextBox Control)
        {
            if (!_IsFilling) { Develop.DebugPrint(enFehlerArt.Fehler, "Filling muss TRUE sein!"); }


            Control.InstantChangedEvent = true;
            Control.Text = _Value;
            Control.InstantChangedEvent = _InstantChangedEvent;
        }




        private void ValueChanged_TextBox(object sender, System.EventArgs e)
        {
            //if (_IsCreating || _IsFilling) { return; }
            Value = ((TextBox)sender).Text;
        }




        #endregion



        /// <summary>
        /// Erstellt zuerst die Standard-Caption, dessen Events werden registriert.
        /// Kümmert sich dann um die Position des Controls im Bezug auf die Caption. Setzt die Sichtbarkeit, korrigiert Anachor und fügt das Control zu der Controll Collection hinzu.
        /// Konext-Menü-Events werden ebenfalls registriert, die andern Events werden nicht registriert und sollten nach dieser Rountine registert werden.
        /// </summary>
        /// <param name="Control"></param>
        private void StandardBehandlung(GenericControl Control)
        {

            Control_Create_Caption();

            switch (_CaptionPosition)
            {
                case enÜberschriftAnordnung.ohne:
                    Control.Left = 0;
                    Control.Top = 0;
                    Control.Width = Width;
                    Control.Height = Height;
                    break;

                case enÜberschriftAnordnung.Links_neben_Dem_Feld:
                    Control.Left = Math.Max(_ControlX, _CaptionObject.Width);
                    Control.Top = 0;
                    Control.Width = Width - Control.Left;
                    Control.Height = Height;
                    if (_CaptionObject.Width < 4) { Develop.DebugPrint("Caption Width zu klein"); }
                    break;

                case enÜberschriftAnordnung.Über_dem_Feld:
                case enÜberschriftAnordnung.Ohne_mit_Abstand:
                    Control.Left = 0;
                    Control.Top = _CaptionObject.Height;
                    Control.Width = Width;
                    Control.Height = Height - _CaptionObject.Height;
                    if (_CaptionObject.Height < 4) { Develop.DebugPrint("Caption Height zu klein"); }

                    break;

                default:
                    Develop.DebugPrint(_CaptionPosition);
                    break;
            }

            Control.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom;

            Control.Visible = true;

            Controls.Add(Control);

            Invalidate();

            //DoInfoTextCaption();

        }


        protected virtual void OnValueChanged()
        {
            ValueChanged?.Invoke(this, System.EventArgs.Empty);
        }



        public Bitmap BitmapOfControl()
        {
            if (_GeneratingBitmapOfControl) { return null; }

            _GeneratingBitmapOfControl = true;

            if (_BitmapOfControl == null) { Refresh(); }
            _GeneratingBitmapOfControl = false;
            return _BitmapOfControl;
        }


        protected override void OnSizeChanged(System.EventArgs e)
        {
            if (_BitmapOfControl != null)
            {
                if (_BitmapOfControl.Width < Width || _BitmapOfControl.Height < Height)
                {
                    _BitmapOfControl.Dispose();
                    _BitmapOfControl = null;
                }
            }

            Invalidate();
            base.OnSizeChanged(e);
        }



        private void DoInfoTextCaption(bool tmpenabled)
        {
            var tmp = _InfoText;

            if (!_ShowInfoWhenDisabled && !tmpenabled) { tmp = string.Empty; }




            if (!string.IsNullOrEmpty(tmp) && _InfoCaption != null)
            {
                _InfoCaption.QuickInfo = tmp;
                return;
            }
            if (string.IsNullOrEmpty(tmp) && _InfoCaption == null) { return; }

            if (string.IsNullOrEmpty(tmp))
            {
                Controls.Remove(_InfoCaption);
                _InfoCaption.Click -= _InfoCaption_Click;
                _InfoCaption.Dispose();
                _InfoCaption = null;
            }
            else
            {
                _InfoCaption = new Caption
                {
                    Name = "Info",
                    QuickInfo = tmp,
                    Enabled = true,
                    Text = "<ImageCode=Warnung|16>",
                    Width = 18,
                    Height = 18,
                    Left = Width - 18,
                    Top = 0,
                    Anchor = System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Top,
                    Visible = true
                };
                Controls.Add(_InfoCaption);
                _InfoCaption.BringToFront();
                _InfoCaption.Click += _InfoCaption_Click;
            }


        }

        private void _InfoCaption_Click(object sender, System.EventArgs e)
        {

            foreach (var thisc in Controls)
            {
                if (thisc is ComboBox cbx)
                {
                    cbx.Focus();
                    cbx.ShowMenu(null, null);
                    return;
                }
            }


        }


        /// <summary>
        /// Erstellt die Steuerelemente zur Bearbeitung und auch die Caption und alles was gebrauch wird.
        /// Die Events werden Registriert und auch der Wert gesetzt.
        /// </summary>
        protected GenericControl CreateSubControls()
        {

            if (_allinitialized)
            {
                Develop.DebugPrint(enFehlerArt.Warnung, "Bereits initialisiert");
                return null;
            }


            if (Width < 5 || Height < 5)
            {
                Develop.DebugPrint(enFehlerArt.Warnung, "Width / Height zu klein");
                return null;
            }

            _allinitialized = true;


            GenericControl c = null;


            switch (_EditType)
            {
                case enEditTypeFormula.Line:
                    c = Control_Create_Line();
                    break;

                case enEditTypeFormula.Textfeld:
                    c = Control_Create_TextBox();
                    break;

                case enEditTypeFormula.EasyPic:
                    c = Control_Create_EasyPic();
                    break;

                case enEditTypeFormula.Gallery:
                case enEditTypeFormula.Listbox_1_Zeile:
                case enEditTypeFormula.Listbox_3_Zeilen:
                case enEditTypeFormula.Listbox_6_Zeilen:
                    c = Control_Create_MainListBox();
                    break;

                case enEditTypeFormula.Textfeld_mit_Auswahlknopf:
                    c = Control_Create_ComboBox();
                    break;

                case enEditTypeFormula.Ja_Nein_Knopf:
                    c = Control_Create_ButtonYesNo();
                    break;

                case enEditTypeFormula.None:
                    break;

                case enEditTypeFormula.nur_als_Text_anzeigen:
                    _CaptionPosition = enÜberschriftAnordnung.Links_neben_Dem_Feld;
                    Control_Create_Caption();
                    break;

                case enEditTypeFormula.Farb_Auswahl_Dialog:
                    c = Control_Create_ButtonColor();
                    break;

                case enEditTypeFormula.Button:
                    c = Control_Create_ButtonComand();
                    break;

                default:
                    Develop.DebugPrint(_EditType);
                    return null;
            }

            UpdateControls();

            return c;
        }





        private void UpdateControls()
        {


            foreach (System.Windows.Forms.Control Control in Controls)
            {


                if (Control != _InfoCaption)
                {
                    if (Control is IQuickInfo QI) { QI.QuickInfo = _QuickInfo; }
                    Control.Enabled = _enabled;
                }
                else
                {
                    Control.Enabled = true;
                }


                switch (Control)
                {

                    case ComboBox ComboBox:
                        ComboBox.Suffix = _Suffix;
                        ComboBox.Format = _Format;
                        ComboBox.InstantChangedEvent = _InstantChangedEvent;

                        break;

                    case TextBox TextBox:
                        TextBox.Suffix = _Suffix;
                        TextBox.Format = _Format;
                        TextBox.InstantChangedEvent = _InstantChangedEvent;
                        break;

                    case EasyPic _:
                        break;

                    case ListBox _:
                        //if (ListBox.Name == "Main") { UpdateValueTo_ListBox(); }
                        break;

                    case Button _:
                        //UpdateValueTo_Button(Button);
                        break;

                    case Caption _:
                        //UpdateValueTo_Caption();
                        break;

                    case Line _:
                        break;

                    default:
                        Develop.DebugPrint(Typ(Control));
                        break;
                }
            }
        }
    }
}
