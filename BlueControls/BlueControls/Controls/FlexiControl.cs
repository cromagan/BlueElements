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
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.Controls {

    [Designer(typeof(BasicDesigner))]
    [DefaultEvent("ValueChanged")]
    public partial class FlexiControl : GenericControl, IBackgroundNone, IInputFormat, ITranslateable {

        #region Fields

        public readonly string ValueId;

        protected bool _allinitialized = false;

        protected string _Caption = string.Empty;

        protected enÜberschriftAnordnung _CaptionPosition = enÜberschriftAnordnung.ohne;
        protected string _disabledReason = string.Empty;
        protected enEditTypeFormula _EditType = enEditTypeFormula.None;

        ///// <summary>
        ///// Wenn True, wird ValueChanged NICHT ausgelöst
        ///// </summary>
        protected bool _IsFilling;

        /// <summary>
        /// Speichert, wann die letzte Text-Änderung vorgenommen wurden.
        /// Wenn NULL, dann wurde bereits ein Event ausgelöst.
        /// </summary>
        protected DateTime? _LastTextChange;

        protected bool _MultiLine = false;
        protected bool _SpellChecking = false;
        protected bool _translateCaption = true;
        private enAdditionalCheck _AdditionalCheck = enAdditionalCheck.None;
        private string _AllowedChars = string.Empty;
        private Caption _CaptionObject;

        // None ist -1 und muss gesetzt sein!
        private int _ControlX = -1;

        private bool _FormatierungErlaubt;

        //private enVarType _Format = enVarType.Text;
        private Caption _InfoCaption;

        private string _InfoText = string.Empty;
        private string _Regex = string.Empty;
        private bool _ShowInfoWhenDisabled = false;
        private string _Suffix = string.Empty;
        private string _Value = string.Empty;

        #endregion

        #region Constructors

        public FlexiControl() : base(false, false) {
            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            _EditType = enEditTypeFormula.Line;
            Size = new Size(200, 8);
        }

        /// <summary>
        /// Einfacher Info Text. Wird nirgends mehr zurück gegeben.
        /// </summary>
        /// <param name="captionText"></param>
        public FlexiControl(string captionText) : base(false, false) {
            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            _EditType = enEditTypeFormula.None;
            _Caption = captionText;
            _CaptionPosition = enÜberschriftAnordnung.Links_neben_Dem_Feld;
            var s = BlueFont.MeasureString(_Caption, Skin.GetBlueFont(enDesign.Caption, enStates.Standard).Font());
            Size = new Size((int)(s.Width + 2), (int)(s.Height + 2));
        }

        #endregion

        #region Events

        public event EventHandler ButtonClicked;

        //public event EventHandler RemovingAll;
        public event EventHandler NeedRefresh;

        [Obsolete]
        public new event EventHandler TextChanged;

        public event EventHandler ValueChanged;

        #endregion

        #region Properties

        [DefaultValue(enAdditionalCheck.None)]
        public enAdditionalCheck AdditionalCheck {
            get => _AdditionalCheck;
            set {
                if (_AdditionalCheck == value) { return; }
                _AdditionalCheck = value;
                UpdateControls();
            }
        }

        [DefaultValue("")]
        public string AllowedChars {
            get => _AllowedChars;
            set {
                if (_AllowedChars == value) { return; }
                _AllowedChars = value;
                UpdateControls();
            }
        }

        [DefaultValue("")]
        public string Caption {
            get => _Caption;
            set {
                if (_Caption == value) { return; }
                RemoveAll(); // Controls and Events entfernen!
                _Caption = value;
            }
        }

        [DefaultValue(enÜberschriftAnordnung.ohne)]
        public enÜberschriftAnordnung CaptionPosition {
            get => _CaptionPosition;
            set {
                if (_CaptionPosition == value) { return; }
                RemoveAll(); // Controls and Events entfernen!
                _CaptionPosition = value;
            }
        }

        /// <summary>
        /// Ab welchen Wert in Pixel das Eingabesteuerelement beginnen darf.
        /// </summary>
        [DefaultValue(-1)]
        public int ControlX {
            get => _ControlX;
            set {
                if (_ControlX == value) { return; }
                RemoveAll(); // Controls and Events entfernen!
                _ControlX = value;
            }
        }

        [DefaultValue("")]
        public string DisabledReason {
            get => _disabledReason ?? string.Empty;
            set {
                value ??= string.Empty;
                //if (value == null) { value = string.Empty; }
                if (_disabledReason == null && string.IsNullOrEmpty(value)) { return; }
                if (_disabledReason == value) { return; }
                _disabledReason = value;
                foreach (Control ThisControl in Controls) {
                    ThisControl.Enabled = ThisControl == _InfoCaption || Enabled;
                }
                DoInfoTextCaption(_disabledReason);
                Invalidate();
            }
        }

        [DefaultValue(enEditTypeFormula.None)]
        public enEditTypeFormula EditType {
            get => _EditType;
            set {
                if (_EditType == value) { return; }
                RemoveAll(); // Controls and Events entfernen!
                _EditType = value;
            }
        }

        /// <summary>
        /// DisabledReason befüllen, um das Steuerelement zu disablen
        /// </summary>
        [DefaultValue(true)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool Enabled => !DesignMode && string.IsNullOrEmpty(_disabledReason);

        [DefaultValue("")]
        public string FileEncryptionKey { get; set; } = "";

        public override bool Focused {
            get {
                foreach (Control ThisControl in Controls) {
                    if (ThisControl.Focused) { return true; }
                }
                return base.Focused;
            }
        }

        [DefaultValue(false)]
        public bool FormatierungErlaubt {
            get => _FormatierungErlaubt;
            set {
                if (value == FormatierungErlaubt) { return; }
                _FormatierungErlaubt = value;
                UpdateControls();
            }
        }

        [DefaultValue("")]
        [Description("Zeigt rechts oben im Eck ein kleines Symbol an, dessen hier eingegebener Text angezeigt wird.")]
        public string InfoText {
            get => _InfoText;
            set {
                if (_InfoText == value) { return; }
                _InfoText = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Falls das Steuerelement Multiline unterstützt, wird dieser angezeigt
        /// </summary>
        [DefaultValue(false)]
        public bool MultiLine {
            get => _MultiLine;
            set {
                if (_MultiLine == value) { return; }
                _MultiLine = value;
                UpdateControls();
            }
        }

        [Browsable(false)]
        [DefaultValue("")]
        public string Prefix { get; set; } = string.Empty;

        [DefaultValue("")]
        public string Regex {
            get => _Regex;
            set {
                if (_Regex == value) { return; }
                _Regex = value;
                UpdateControls();
            }
        }

        [DefaultValue(false)]
        public bool ShowInfoWhenDisabled {
            get => _ShowInfoWhenDisabled;
            set {
                if (_ShowInfoWhenDisabled == value) { return; }
                _ShowInfoWhenDisabled = value;
                Invalidate();
            }
        }

        [DefaultValue(false)]
        public bool SpellChecking {
            get => _SpellChecking;
            set {
                if (_SpellChecking == value) { return; }
                _SpellChecking = value;
                UpdateControls();
            }
        }

        /// <summary>
        /// Falls das Steuerelement eine Suffix unterstützt, wird dieser angezeigt
        /// </summary>
        [DefaultValue("")]
        public string Suffix {
            get => _Suffix;
            set {
                if (_Suffix == value) { return; }
                _Suffix = value;
                UpdateControls();
            }
        }

        /// <summary>
        /// Value benutzen!
        /// </summary>
        [Obsolete]
        [DefaultValue("")]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string Text { get; set; }

        [DefaultValue(true)]
        bool ITranslateable.Translate {
            get => _translateCaption;
            set {
                if (_translateCaption == value) { return; }
                if (_CaptionObject is Caption c) { c.Translate = _translateCaption; }
            }
        }

        /// <summary>
        /// Zum setzen des Wertes muss ValueSet benutzt werden.
        /// </summary>
        [DefaultValue("")]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Value => _Value ?? string.Empty;

        #endregion

        #region Methods

        /// <summary>
        ///
        /// </summary>
        /// <param name="newvalue"></param>
        /// <param name="updateControls"></param>
        /// <param name="alwaysValueChanged">Steuerelemente, wie Button, Checkboxen, DropDownListen müssen hier TRUE setzen. Auch Texte, die in einem Stück gesetzt werden.</param>
        public void ValueSet(string newvalue, bool updateControls, bool alwaysValueChanged) {
            if (newvalue == null) { newvalue = string.Empty; }
            if (_Value == null && string.IsNullOrEmpty(newvalue)) { return; }
            if (_Value == newvalue) { return; }

            _LastTextChange = DateTime.UtcNow;
            _Value = newvalue;
            if (updateControls) { UpdateValueToControl(); }
            if (alwaysValueChanged || InvokeRequired || !Focused) { RaiseEventIfChanged(); }
        }

        /// <summary>
        /// Erstellt die Steuerelemente zur Bearbeitung und auch die Caption und alles was gebrauch wird.
        /// Die Events werden Registriert und auch der Wert gesetzt.
        /// </summary>
        protected GenericControl CreateSubControls() {
            if (_allinitialized) {
                Develop.DebugPrint(enFehlerArt.Warnung, "Bereits initialisiert");
                return null;
            }

            if (Width < 5 || Height < 5) {
                Develop.DebugPrint(enFehlerArt.Warnung, "Width / Height zu klein");
                return null;
            }

            _allinitialized = true;
            GenericControl c = null;
            switch (_EditType) {
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
                case enEditTypeFormula.Listbox:
                    c = Control_Create_ListBox();
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

                case enEditTypeFormula.SwapListBox:
                    c = Control_Create_SwapListBox();
                    break;

                default:
                    Develop.DebugPrint(_EditType);
                    return null;
            }
            UpdateControls();
            return c;
        }

        protected override void DrawControl(Graphics gr, enStates state) {
            // Enabled wurde verdeckt!
            if (!Enabled) { state = enStates.Standard_Disabled; }
            Skin.Draw_Back_Transparent(gr, ClientRectangle, this);
            //if (_Color.A != 0) {
            //    if (state.HasFlag(enStates.Standard_Disabled)) {
            //        var br = (byte)(_Color.GetBrightness() * 254);
            //        var lgb = new LinearGradientBrush(ClientRectangle, Color.FromArgb(br, br, br), Color.Transparent, LinearGradientMode.Horizontal);
            //        gr.FillRectangle(lgb, ClientRectangle);
            //    } else {
            //        var lgb = new LinearGradientBrush(ClientRectangle, _Color, Color.Transparent, LinearGradientMode.Horizontal);
            //        gr.FillRectangle(lgb, ClientRectangle);
            //    }
            //}
            if (!_allinitialized) { CreateSubControls(); }
            //if (_EditType == enEditTypeFormula.Listbox || _EditType == enEditTypeFormula.Listbox_1_Zeile || _EditType == enEditTypeFormula.Listbox_3_Zeilen) {
            //    ListBoxen(out var Main, out var Suggest);
            //    if (Suggest != null) {
            //        var tmpstate = state;
            //        if (tmpstate != enStates.Checked_Disabled) { tmpstate = enStates.Standard; }
            //        var R = new Rectangle {
            //            X = Main.Left - 1,
            //            Y = Main.Top - 1,
            //            Width = Main.Width + 2,
            //            Height = Height - Main.Top - 1
            //        };
            //        Skin.Draw_Border(gr, enDesign.ListBox, tmpstate, R);
            //    }
            //}
            if (!string.IsNullOrEmpty(_disabledReason)) {
                DoInfoTextCaption(_disabledReason);
            } else {
                if (state.HasFlag(enStates.Standard_Disabled)) {
                    DoInfoTextCaption("Übergeordnetes Steuerlement ist deaktiviert.");
                } else {
                    DoInfoTextCaption(string.Empty);
                }
            }
        }

        protected virtual void OnButtonClicked() => ButtonClicked?.Invoke(this, System.EventArgs.Empty);

        protected override void OnControlAdded(ControlEventArgs e) {
            base.OnControlAdded(e);
            switch (e.Control) {
                case ComboBox ComboBox:
                    ComboBox.TextChanged += ValueChanged_ComboBox;
                    ComboBox.LostFocus += TextEditControl_LostFocus;
                    break;

                case TextBox TextBox:
                    TextBox.TextChanged += ValueChanged_TextBox;
                    TextBox.LostFocus += TextEditControl_LostFocus;
                    break;

                case Caption _:
                case Line _:
                    break;

                case EasyPic _:
                    // Control.ImageChanged += EasyPicImageChanged;
                    // Einzig und alleine eigene Datenbank kann den dazugehörigen Wert generieren.
                    break;

                case SwapListBox SwapListBox:
                    SwapListBox.ItemAdded += SwapListBox_ItemAdded;
                    SwapListBox.ItemRemoved += SwapListBox_ItemRemoved;
                    break;

                case ListBox ListBox:
                    ListBox.ItemAdded += ListBox_ItemAdded;
                    ListBox.ItemRemoved += ListBox_ItemRemoved;
                    break;

                case Button Button:
                    switch (_EditType) {
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

        protected override void OnControlRemoved(ControlEventArgs e) {
            base.OnControlRemoved(e);
            switch (e.Control) {
                case ComboBox ComboBox:
                    ComboBox.TextChanged -= ValueChanged_ComboBox;
                    ComboBox.LostFocus -= TextEditControl_LostFocus;
                    break;

                case TextBox TextBox:
                    TextBox.TextChanged -= ValueChanged_TextBox;
                    TextBox.LostFocus -= TextEditControl_LostFocus;
                    break;

                case Caption _:
                case Line _:
                    break;

                case EasyPic _:
                    break;

                case ListBox ListBox:
                    ListBox.ItemAdded -= ListBox_ItemAdded;
                    ListBox.ItemRemoved -= ListBox_ItemRemoved;
                    break;

                case SwapListBox SwapListBox:
                    SwapListBox.ItemAdded -= SwapListBox_ItemAdded;
                    SwapListBox.ItemRemoved -= SwapListBox_ItemRemoved;
                    break;

                case Button Button:
                    switch (_EditType) {
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
            if (e.Control == _InfoCaption) { _InfoCaption = null; }
            if (e.Control == _CaptionObject) { _CaptionObject = null; }
        }

        protected virtual void OnNeedRefresh() => NeedRefresh?.Invoke(this, System.EventArgs.Empty);

        protected override void OnParentChanged(System.EventArgs e) {
            base.OnParentChanged(e);

            var parentForm = ParentForm();
            if (parentForm == null) { return; }

            parentForm.FormClosing += ParentForm_FormClosing;
        }

        protected override void OnQuickInfoChanged() {
            base.OnQuickInfoChanged();
            UpdateControls();
        }

        protected virtual void OnValueChanged() => ValueChanged?.Invoke(this, System.EventArgs.Empty);

        /// <summary>
        /// Entfernt alle Controls und löst dessen die Events auf. Setzt _allinitialized auf false.
        /// </summary>
        protected virtual void RemoveAll() {
            List<Control> l = new();
            for (var z = 0; z < Controls.Count; z++) { l.Add(Controls[z]); }

            foreach (var thisc in l) {
                thisc.Visible = false;
                if (thisc != _CaptionObject && thisc != _InfoCaption) {
                    thisc.Dispose(); // Dispose entfernt dass Control aus der Collection
                }
            }

            _allinitialized = false;
        }

        protected void StyleComboBox(ComboBox control, ItemCollectionList list, ComboBoxStyle style) {
            control.Enabled = Enabled;
            control.GetStyleFrom(this);
            control.DropDownStyle = style;
            control.Item.Clear();
            control.Item.AddClonesFrom(list);
            control.Item.Sort();
        }

        protected void StyleListBox(ListBox control, ColumnItem column) {
            control.Enabled = Enabled;
            control.Item.Clear();
            control.Item.CheckBehavior = enCheckBehavior.MultiSelection;
            if (column == null) { return; }
            ItemCollectionList Item = new();
            if (column.DropdownBearbeitungErlaubt) {
                ItemCollectionList.GetItemCollection(Item, column, null, enShortenStyle.Both, 10000);
                if (!column.DropdownWerteAndererZellenAnzeigen) {
                    bool again;
                    do {
                        again = false;
                        foreach (var ThisItem in Item) {
                            if (!column.DropDownItems.Contains(ThisItem.Internal)) {
                                again = true;
                                Item.Remove(ThisItem);
                                break;
                            }
                        }
                    } while (again);
                }
            }
            control.AddAllowed = enAddType.UserDef;
            control.FilterAllowed = false;
            control.MoveAllowed = false;
            switch (_EditType) {
                case enEditTypeFormula.Gallery:
                    control.Appearance = enBlueListBoxAppearance.Gallery;
                    control.RemoveAllowed = true;
                    break;

                case enEditTypeFormula.Listbox:
                    control.RemoveAllowed = true;
                    control.Appearance = enBlueListBoxAppearance.Listbox;
                    break;
            }
        }

        protected void StyleSwapListBox(SwapListBox Control, ColumnItem Column) {
            Control.Enabled = Enabled;
            Control.Item.Clear();
            Control.Item.CheckBehavior = enCheckBehavior.NoSelection;
            if (Column == null) { return; }
            ItemCollectionList Item = new();
            ItemCollectionList.GetItemCollection(Item, Column, null, enShortenStyle.Both, 10000);
            Control.SuggestionsAdd(Item);
            Control.AddAllowed = enAddType.UserDef;
        }

        protected void StyleTextBox(TextBox control) {
            control.Enabled = Enabled;
            control.GetStyleFrom(this);
            control.Verhalten = _MultiLine || Height > 20
                ? enSteuerelementVerhalten.Scrollen_mit_Textumbruch
                : enSteuerelementVerhalten.Scrollen_ohne_Textumbruch;
        }

        private void _IdleTimer_Tick(object sender, System.EventArgs e) {
            if (_LastTextChange == null) { return; }
            if (DateTime.UtcNow.Subtract((DateTime)_LastTextChange).TotalSeconds < 20) { return; }
            Focus(); // weitere Tastatureingabn verhindern. Z.B: wenn was mariert wird und dann entfernen gedrück wird. Wenn die Box neu sortiert wird, ist dsa ergebnis nicht schön
            RaiseEventIfChanged();
        }

        private void _InfoCaption_Click(object sender, System.EventArgs e) {
            foreach (var thisc in Controls) {
                if (thisc is ComboBox cbx) {
                    cbx.Focus();
                    cbx.ShowMenu(null, null);
                    return;
                }
            }
        }

        private void ColorButton_Click(object sender, System.EventArgs e) => Develop.DebugPrint_NichtImplementiert();

        private void ComandButton_Click(object sender, System.EventArgs e) {
            if (_EditType != enEditTypeFormula.Button) { return; }
            ValueSet(true.ToPlusMinus(), false, true); // Geklickt, wurde hiermit vermerkt
            OnButtonClicked();
        }

        /// <summary>
        /// Erstellt das Steuerelement. Die Events werden Registriert und auch der Wert gesetzt.
        /// </summary>
        private Button Control_Create_ButtonColor() {
            Button Control = new() {
                Enabled = Enabled,
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
        private Button Control_Create_ButtonComand() {
            Button Control = new() {
                Enabled = Enabled,
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
        private Button Control_Create_ButtonYesNo() {
            Button Control = new() {
                Enabled = Enabled,
                Name = "YesNoButton",
                ButtonStyle = enButtonStyle.Yes_or_No,
                Text = "",
                ImageCode = ""
            };
            UpdateValueToControl();
            StandardBehandlung(Control);
            return Control;
        }

        private void Control_Create_Caption() {
            if (_CaptionPosition == enÜberschriftAnordnung.ohne) { return; }
            if (_CaptionObject == null) {
                _CaptionObject = new Caption();
                Controls.Add(_CaptionObject);
            }
            _CaptionObject.Enabled = Enabled;
            _CaptionObject.TextAnzeigeVerhalten = enSteuerelementVerhalten.Text_Abschneiden; // nicht enSteuerelementVerhalten.Steuerelement_Anpassen! weil sonst beim einem Resize die Koordinaten geändert werden und das kann zum Ping Pong führen
            _CaptionObject.Text = _Caption.ReplaceLowerSign();
            _CaptionObject.Size = _CaptionObject.TextRequiredSize();
            _CaptionObject.Left = 0;
            _CaptionObject.Top = 0;
            _CaptionObject.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            _CaptionObject.Visible = _CaptionPosition != enÜberschriftAnordnung.Ohne_mit_Abstand;
            _CaptionObject.Translate = _translateCaption;
            _CaptionObject.BringToFront();
        }

        /// <summary>
        /// Erstellt das Steuerelement. Die Events werden Registriert und auch der Wert gesetzt.
        /// </summary>
        private ComboBox Control_Create_ComboBox() {
            ComboBox Control = new();
            StyleComboBox(Control, null, ComboBoxStyle.DropDownList);
            UpdateValueToControl();
            StandardBehandlung(Control);
            return Control;
        }

        /// <summary>
        /// Erstellt das Steuerelement. Die Events werden Registriert und auch der Wert gesetzt.
        /// </summary>
        private EasyPic Control_Create_EasyPic() {
            EasyPic Control = new() {
                Enabled = Enabled
            };
            StandardBehandlung(Control);
            UpdateValueToControl();
            return Control;
        }

        /// <summary>
        /// Erstellt das Steuerelement. Die Events werden Registriert und auch der Wert gesetzt.
        /// </summary>
        private Line Control_Create_Line() {
            Line Control = new() {
                Enabled = Enabled,
                Orientation = enOrientation.Waagerecht
            };
            StandardBehandlung(Control);
            return Control;
        }

        /// <summary>
        /// Erstellt das Steuerelement. Die Events werden Registriert und auch der Wert gesetzt.
        /// </summary>
        private ListBox Control_Create_ListBox() {
            ListBox Control = new() {
                Enabled = Enabled,
            };
            StyleListBox(Control, null);
            UpdateValueToControl();
            StandardBehandlung(Control);
            return Control;
        }

        // Nimmt Teilweise die Routinen der Listbox her
        private SwapListBox Control_Create_SwapListBox() {
            SwapListBox Control = new() {
                Enabled = Enabled,
            };
            StyleSwapListBox(Control, null);
            UpdateValueToControl();
            StandardBehandlung(Control);
            return Control;
        }

        /// <summary>
        /// Erstellt das Steuerelement. Die Events werden Registriert und auch der Wert gesetzt.
        /// </summary>
        private TextBox Control_Create_TextBox() {
            TextBox Control = new();
            StyleTextBox(Control);
            UpdateValueToControl();
            StandardBehandlung(Control);
            return Control;
        }

        private void DoInfoTextCaption(string disabledReason) {
            var txt = string.Empty;
            var symbol = string.Empty;
            if (string.IsNullOrEmpty(disabledReason) && string.IsNullOrEmpty(_InfoText)) {
                txt = string.Empty;
                symbol = string.Empty;
            } else if (!string.IsNullOrEmpty(disabledReason) && string.IsNullOrEmpty(_InfoText)) {
                symbol = "  <ImageCode=Schloss|10|||||150||20>";
                txt = disabledReason;
            } else if (string.IsNullOrEmpty(disabledReason) && !string.IsNullOrEmpty(_InfoText)) {
                symbol = "<ImageCode=Warnung|16>";
                txt = _InfoText;
            } else {
                symbol = "<ImageCode=Information|16>";
                txt = "<b>Der Wert kann nicht bearbeitet werden:</b><br>" + disabledReason + "<br><br><b>Enthält aber einen Fehler:</b><br>" + _InfoText;
            }
            if (!_ShowInfoWhenDisabled && !string.IsNullOrEmpty(disabledReason)) { txt = string.Empty; }
            if (!string.IsNullOrEmpty(txt) && _InfoCaption != null) {
                _InfoCaption.Left = Width - 18;
                _InfoCaption.Top = 0;
                _InfoCaption.QuickInfo = txt;
                _InfoCaption.Text = symbol;
                _InfoCaption.Visible = true;
                _InfoCaption.BringToFront();
                return;
            }
            if (string.IsNullOrEmpty(txt) && _InfoCaption == null) { return; }
            if (string.IsNullOrEmpty(txt)) {
                //Controls.Remove(_InfoCaption);
                //_InfoCaption.Click -= _InfoCaption_Click;
                //_InfoCaption.Dispose();
                //_InfoCaption = null;
                _InfoCaption.Visible = false;
            } else {
                _InfoCaption = new Caption {
                    Name = "Info",
                    QuickInfo = txt,
                    Enabled = true,
                    Text = symbol,
                    Width = 18,
                    Height = 18,
                    Left = Width - 18,
                    Top = 0,
                    Anchor = AnchorStyles.Right | AnchorStyles.Top,
                    Visible = true
                };
                Controls.Add(_InfoCaption);
                _InfoCaption.BringToFront();
                _InfoCaption.Click += _InfoCaption_Click;
                _InfoCaption.BringToFront();
            }
        }

        private void ListBox_ItemAdded(object sender, ListEventArgs e) {
            if (_IsFilling) { return; }
            ValueSet(((ListBox)sender).Item.ToListOfString().JoinWithCr(), false, true);
        }

        private void ListBox_ItemRemoved(object sender, System.EventArgs e) {
            if (_IsFilling) { return; }
            ValueSet(((ListBox)sender).Item.ToListOfString().JoinWithCr(), false, true);
        }

        private void ParentForm_FormClosing(object sender, FormClosingEventArgs e) => RaiseEventIfChanged(); // Versuchen, die Werte noch zurückzugeben

        private void RaiseEventIfChanged() {
            if (_LastTextChange == null) { return; }
            _LastTextChange = null;
            OnValueChanged();
        }

        /// <summary>
        /// Erstellt zuerst die Standard-Caption, dessen Events werden registriert.
        /// Kümmert sich dann um die Position des Controls im Bezug auf die Caption. Setzt die Sichtbarkeit, korrigiert Anachor und fügt das Control zu der Controll Collection hinzu.
        /// Konext-Menü-Events werden ebenfalls registriert, die andern Events werden nicht registriert und sollten nach dieser Rountine registert werden.
        /// </summary>
        /// <param name="Control"></param>
        private void StandardBehandlung(GenericControl Control) {
            Control_Create_Caption();
            switch (_CaptionPosition) {
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
            Control.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            Control.Visible = true;
            Controls.Add(Control);
            Invalidate();
            //DoInfoTextCaption();
        }

        private void SwapListBox_ItemAdded(object sender, ListEventArgs e) {
            if (_IsFilling) { return; }
            ValueSet(((SwapListBox)sender).Item.ToListOfString().JoinWithCr(), false, true);
        }

        private void SwapListBox_ItemRemoved(object sender, System.EventArgs e) {
            if (_IsFilling) { return; }
            ValueSet(((SwapListBox)sender).Item.ToListOfString().JoinWithCr(), false, true);
        }

        private void TextEditControl_LostFocus(object sender, System.EventArgs e) => RaiseEventIfChanged();

        private void UpdateControls() {
            foreach (Control Control in Controls) {
                if (Control != _InfoCaption) {
                    if (Control is GenericControl QI) { QI.QuickInfo = QuickInfo; }
                    Control.Enabled = Enabled;
                } else {
                    Control.Enabled = true;
                }

                if (Control is IInputFormat inf) { inf.GetStyleFrom(inf); }
            }
        }

        /// <summary>
        /// Setzt den aktuellen Wert, so dass es das Control anzeigt. Filling muss TRUE sein.
        /// </summary>
        private void UpdateValueTo_Button(Button Control) {
            //if (!_IsFilling) { Develop.DebugPrint(enFehlerArt.Fehler, "Filling und Creating False!"); }
            switch (_EditType) {
                case enEditTypeFormula.Ja_Nein_Knopf:
                    Control.Checked = _Value.FromPlusMinus();
                    break;

                case enEditTypeFormula.Button:
                    break;

                case enEditTypeFormula.Farb_Auswahl_Dialog:
                    Control.ImageCode = string.IsNullOrEmpty(_Value) ? "Fragezeichen|24" : "Kreis|24|||" + Color.FromArgb(int.Parse(_Value)).ToHTMLCode();
                    break;

                default:
                    Develop.DebugPrint_NichtImplementiert();
                    break;
            }
        }

        /// <summary>
        /// Setzt den aktuellen Wert, so dass es das Control anzeigt. Filling muss TRUE sein.
        /// </summary>
        private void UpdateValueTo_Caption() {
            //if (!_IsFilling) { Develop.DebugPrint(enFehlerArt.Fehler, "Filling muss TRUE sein!"); }
            //if (Column == null) { return; } // nur mögloch bei verbundenen Datenbanken
            if (_EditType != enEditTypeFormula.nur_als_Text_anzeigen) { return; } // und auch dann nur als reine Text anzeige
            if (_CaptionObject == null) { return; }
            _CaptionObject.Width = Width;
            _CaptionObject.Translate = false;
            _CaptionObject.Text = _Caption + " <i>" + _Value;
        }

        /// <summary>
        /// Setzt den aktuellen Wert, so dass es das Control anzeigt. Filling muss TRUE sein.
        /// </summary>
        private void UpdateValueTo_Combobox(ComboBox Control) =>
            //if (!_IsFilling) { Develop.DebugPrint(enFehlerArt.Fehler, "Filling muss TRUE sein!"); }
            Control.Text = _Value;

        /// <summary>
        /// Setzt den aktuellen Wert, so dass es das Control anzeigt. Filling  muss TRUE sein.
        /// </summary>
        private void UpdateValueTo_EasyPic(EasyPic Control) =>
            //if (!_IsFilling) { Develop.DebugPrint(enFehlerArt.Fehler, "Filling muss TRUE sein!"); }
            Control.FromFile(_Value);

        /// <summary>
        /// Setzt den aktuellen Wert, so dass es das Control anzeigt. Filling muss TRUE sein.
        /// </summary>
        private void UpdateValueTo_ListBox(ItemCollectionList Main) => Main.SetValuesTo(_Value.SplitAndCutByCRToList(), FileEncryptionKey);

        /// <summary>
        /// Setzt den aktuellen Wert, so dass es das Control anzeigt. Filling muss TRUE sein.
        /// </summary>
        private void UpdateValueTo_TextBox(TextBox Control) =>
            //if (!_IsFilling) { Develop.DebugPrint(enFehlerArt.Fehler, "Filling muss TRUE sein!"); }
            Control.Text = _Value;

        /// <summary>
        /// Setzt den aktuellen Wert, so dass es das Control anzeigt. Die Filling-Variable wird währenddessen umgesetzt.
        /// sollte vor StandardBehandlung kommen, da dort das Objekt gesetzt wird und dann die Handler generiert werden.
        /// </summary>
        private void UpdateValueToControl() {
            if (!_allinitialized) { CreateSubControls(); }
            _IsFilling = true;
            foreach (Control Control in Controls) {
                switch (Control) {
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
                        UpdateValueTo_ListBox(ListBox.Item);
                        break;

                    case SwapListBox SwapListBox:
                        UpdateValueTo_ListBox(SwapListBox.Item);
                        break;

                    case Button Button:
                        UpdateValueTo_Button(Button);
                        break;

                    case Caption _:
                        UpdateValueTo_Caption();
                        break;

                    case Line _:
                        if (!string.IsNullOrEmpty(_Value)) {
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

        private void ValueChanged_ComboBox(object sender, System.EventArgs e) {
            if (_IsFilling) { return; }
            ValueSet(((ComboBox)sender).Text, false, ((ComboBox)sender).DropDownStyle == ComboBoxStyle.DropDownList);
        }

        private void ValueChanged_TextBox(object sender, System.EventArgs e) {
            if (_IsFilling) { return; }
            ValueSet(((TextBox)sender).Text, false, false);
        }

        // TODO: Erstellen!//if (_EditType != enEditTypeFormula.Button) { return; }//CheckIfChanged("+");//OnButtonClicked();
        private void YesNoButton_CheckedChanged(object sender, System.EventArgs e) => ValueSet(((Button)sender).Checked.ToPlusMinus(), false, true);

        #endregion
    }
}