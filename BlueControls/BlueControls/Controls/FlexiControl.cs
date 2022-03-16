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
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.ItemCollection.ItemCollectionList;
using static BlueBasics.Converter;

namespace BlueControls.Controls {

    [Designer(typeof(BasicDesigner))]
    [DefaultEvent("ValueChanged")]
    public partial class FlexiControl : GenericControl, IBackgroundNone, IInputFormat, ITranslateable {

        #region Fields

        protected bool Allinitialized;

        ///// <summary>
        ///// Wenn True, wird ValueChanged NICHT ausgelöst
        ///// </summary>
        protected bool IsFilling;

        /// <summary>
        /// Speichert, wann die letzte Text-Änderung vorgenommen wurden.
        /// Wenn NULL, dann wurde bereits ein Event ausgelöst.
        /// </summary>
        protected DateTime? LastTextChange;

        protected bool TranslateCaption = true;
        private enAdditionalCheck _additionalCheck = enAdditionalCheck.None;
        private string _allowedChars = string.Empty;
        private string _caption = string.Empty;
        private Caption? _captionObject;
        private enÜberschriftAnordnung _captionPosition = enÜberschriftAnordnung.ohne;

        // None ist -1 und muss gesetzt sein!
        private int _controlX = -1;

        private string _disabledReason = string.Empty;
        private enEditTypeFormula _editType;
        private bool _formatierungErlaubt;

        //private enVarType _Format = enVarType.Text;
        private Caption? _infoCaption;

        private string _infoText = string.Empty;
        private bool _multiLine;
        private string _regex = string.Empty;
        private bool _showInfoWhenDisabled;
        private bool _spellChecking;
        private string _suffix = string.Empty;

        #endregion

        #region Constructors

        public FlexiControl() : base(false, false) {
            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            _editType = enEditTypeFormula.Line;
            Size = new Size(200, 8);
        }

        /// <summary>
        /// Einfacher Info Text. Wird nirgends mehr zurück gegeben.
        /// </summary>
        /// <param name="captionText"></param>
        public FlexiControl(string captionText) : base(false, false) {
            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            _editType = enEditTypeFormula.None;
            _caption = captionText;
            _captionPosition = enÜberschriftAnordnung.Links_neben_Dem_Feld;
            var s = BlueFont.MeasureString(_caption, Skin.GetBlueFont(enDesign.Caption, enStates.Standard).Font());
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
            get => _additionalCheck;
            set {
                if (_additionalCheck == value) { return; }
                _additionalCheck = value;
                UpdateControls();
            }
        }

        [DefaultValue("")]
        public string AllowedChars {
            get => _allowedChars;
            set {
                if (_allowedChars == value) { return; }
                _allowedChars = value;
                UpdateControls();
            }
        }

        [DefaultValue("")]
        public string Caption {
            get => _caption;
            set {
                if (_caption == value) { return; }
                RemoveAll(); // Controls and Events entfernen!
                _caption = value;
            }
        }

        [DefaultValue(enÜberschriftAnordnung.ohne)]
        public enÜberschriftAnordnung CaptionPosition {
            get => _captionPosition;
            set {
                if (_captionPosition == value) { return; }
                RemoveAll(); // Controls and Events entfernen!
                _captionPosition = value;
            }
        }

        /// <summary>
        /// Ab welchen Wert in Pixel das Eingabesteuerelement beginnen darf.
        /// </summary>
        [DefaultValue(-1)]
        public int ControlX {
            get => _controlX;
            set {
                if (_controlX == value) { return; }
                RemoveAll(); // Controls and Events entfernen!
                _controlX = value;
            }
        }

        [DefaultValue("")]
        public string DisabledReason {
            get => _disabledReason;
            set {
                //if (value == null) { value = string.Empty; }
                if (_disabledReason == null && string.IsNullOrEmpty(value)) { return; }
                if (_disabledReason == value) { return; }
                _disabledReason = value;
                foreach (Control thisControl in Controls) {
                    thisControl.Enabled = thisControl == _infoCaption || Enabled;
                }
                DoInfoTextCaption(_disabledReason);
                Invalidate();
            }
        }

        [DefaultValue(enEditTypeFormula.None)]
        public enEditTypeFormula EditType {
            get => _editType;
            set {
                if (_editType == value) { return; }
                RemoveAll(); // Controls and Events entfernen!
                _editType = value;
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
                foreach (Control thisControl in Controls) {
                    if (thisControl.Focused) { return true; }
                }
                return base.Focused;
            }
        }

        [DefaultValue(false)]
        public bool FormatierungErlaubt {
            get => _formatierungErlaubt;
            set {
                if (value == FormatierungErlaubt) { return; }
                _formatierungErlaubt = value;
                UpdateControls();
            }
        }

        [DefaultValue("")]
        [Description("Zeigt rechts oben im Eck ein kleines Symbol an, dessen hier eingegebener Text angezeigt wird.")]
        public string InfoText {
            get => _infoText;
            set {
                if (_infoText == value) { return; }
                _infoText = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Falls das Steuerelement Multiline unterstützt, wird dieser angezeigt
        /// </summary>
        [DefaultValue(false)]
        public bool MultiLine {
            get => _multiLine;
            set {
                if (_multiLine == value) { return; }
                _multiLine = value;
                UpdateControls();
            }
        }

        [Browsable(false)]
        [DefaultValue("")]
        public string Prefix { get; set; } = string.Empty;

        [DefaultValue("")]
        public string Regex {
            get => _regex;
            set {
                if (_regex == value) { return; }
                _regex = value;
                UpdateControls();
            }
        }

        [DefaultValue(false)]
        public bool ShowInfoWhenDisabled {
            get => _showInfoWhenDisabled;
            set {
                if (_showInfoWhenDisabled == value) { return; }
                _showInfoWhenDisabled = value;
                Invalidate();
            }
        }

        [DefaultValue(false)]
        public bool SpellChecking {
            get => _spellChecking;
            set {
                if (_spellChecking == value) { return; }
                _spellChecking = value;
                UpdateControls();
            }
        }

        /// <summary>
        /// Falls das Steuerelement eine Suffix unterstützt, wird dieser angezeigt
        /// </summary>
        [DefaultValue("")]
        public string Suffix {
            get => _suffix;
            set {
                if (_suffix == value) { return; }
                _suffix = value;
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
            get => TranslateCaption;
            set {
                if (TranslateCaption == value) { return; }
                if (_captionObject is Caption c) { c.Translate = TranslateCaption; }
            }
        }

        /// <summary>
        /// Zum setzen des Wertes muss ValueSet benutzt werden.
        /// </summary>
        [DefaultValue("")]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Value { get; private set; } = string.Empty;

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
            if (Value == null && string.IsNullOrEmpty(newvalue)) { return; }
            if (Value == newvalue) { return; }

            LastTextChange = DateTime.UtcNow;
            Value = newvalue;
            if (updateControls) { UpdateValueToControl(); }
            if (alwaysValueChanged || InvokeRequired || !Focused) { RaiseEventIfChanged(); }
        }

        /// <summary>
        /// Erstellt die Steuerelemente zur Bearbeitung und auch die Caption und alles was gebrauch wird.
        /// Die Events werden Registriert und auch der Wert gesetzt.
        /// </summary>
        protected GenericControl? CreateSubControls() {
            if (Allinitialized) {
                Develop.DebugPrint(enFehlerArt.Warnung, "Bereits initialisiert");
                return null;
            }

            if (Width < 5 || Height < 5) {
                Develop.DebugPrint(enFehlerArt.Warnung, "Width / Height zu klein");
                return null;
            }

            Allinitialized = true;
            GenericControl? c = null;
            switch (_editType) {
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
                    _captionPosition = enÜberschriftAnordnung.Links_neben_Dem_Feld;
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
                    Develop.DebugPrint(_editType);
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
            if (!Allinitialized) { CreateSubControls(); }
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
                case ComboBox comboBox:
                    comboBox.TextChanged += ValueChanged_ComboBox;
                    comboBox.LostFocus += TextEditControl_LostFocus;
                    break;

                case TextBox textBox:
                    textBox.TextChanged += ValueChanged_TextBox;
                    textBox.LostFocus += TextEditControl_LostFocus;
                    break;

                case Caption _:
                case Line:
                    break;

                case EasyPic:
                    // Control.ImageChanged += EasyPicImageChanged;
                    // Einzig und alleine eigene Datenbank kann den dazugehörigen Wert generieren.
                    break;

                case SwapListBox swapListBox:
                    swapListBox.ItemAdded += SwapListBox_ItemAdded;
                    swapListBox.ItemRemoved += SwapListBox_ItemRemoved;
                    break;

                case ListBox listBox:
                    listBox.ItemAdded += ListBox_ItemAdded;
                    listBox.ItemRemoved += ListBox_ItemRemoved;
                    break;

                case Button button:
                    switch (_editType) {
                        case enEditTypeFormula.Ja_Nein_Knopf:
                            button.CheckedChanged += YesNoButton_CheckedChanged;
                            break;

                        case enEditTypeFormula.Button:
                            button.Click += ComandButton_Click;
                            break;

                        case enEditTypeFormula.Farb_Auswahl_Dialog:
                            button.Click += ColorButton_Click;
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
                case ComboBox comboBox:
                    comboBox.TextChanged -= ValueChanged_ComboBox;
                    comboBox.LostFocus -= TextEditControl_LostFocus;
                    break;

                case TextBox textBox:
                    textBox.TextChanged -= ValueChanged_TextBox;
                    textBox.LostFocus -= TextEditControl_LostFocus;
                    break;

                case Caption _:
                case Line:
                    break;

                case EasyPic:
                    break;

                case ListBox listBox:
                    listBox.ItemAdded -= ListBox_ItemAdded;
                    listBox.ItemRemoved -= ListBox_ItemRemoved;
                    break;

                case SwapListBox swapListBox:
                    swapListBox.ItemAdded -= SwapListBox_ItemAdded;
                    swapListBox.ItemRemoved -= SwapListBox_ItemRemoved;
                    break;

                case Button button:
                    switch (_editType) {
                        case enEditTypeFormula.Ja_Nein_Knopf:
                            button.CheckedChanged -= YesNoButton_CheckedChanged;
                            break;

                        case enEditTypeFormula.Button:
                            button.Click -= ComandButton_Click;
                            break;

                        case enEditTypeFormula.Farb_Auswahl_Dialog:
                            button.Click -= ColorButton_Click;
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
            if (e.Control == _infoCaption) { _infoCaption = null; }
            if (e.Control == _captionObject) { _captionObject = null; }
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
                if (thisc != _captionObject && thisc != _infoCaption) {
                    thisc.Dispose(); // Dispose entfernt dass Control aus der Collection
                }
            }

            Allinitialized = false;
        }

        protected void StyleComboBox(ComboBox? control, ItemCollectionList? list, ComboBoxStyle style) {
            control.Enabled = Enabled;
            control.GetStyleFrom(this);
            control.DropDownStyle = style;
            control.Item.Clear();
            control.Item.AddClonesFrom(list);
            control.Item.Sort();
        }

        protected void StyleListBox(ListBox? control, ColumnItem? column) {
            control.Enabled = Enabled;
            control.Item.Clear();
            control.Item.CheckBehavior = enCheckBehavior.MultiSelection;
            if (column == null) { return; }
            ItemCollectionList item = new();
            if (column.DropdownBearbeitungErlaubt) {
                ItemCollectionList.GetItemCollection(item, column, null, enShortenStyle.Replaced, 10000);
                if (!column.DropdownWerteAndererZellenAnzeigen) {
                    bool again;
                    do {
                        again = false;
                        foreach (var thisItem in item) {
                            if (!column.DropDownItems.Contains(thisItem.Internal)) {
                                again = true;
                                item.Remove(thisItem);
                                break;
                            }
                        }
                    } while (again);
                }
            }
            control.AddAllowed = enAddType.UserDef;
            control.FilterAllowed = false;
            control.MoveAllowed = false;
            switch (_editType) {
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

        protected void StyleSwapListBox(SwapListBox? control, ColumnItem? column) {
            control.Enabled = Enabled;
            control.Item.Clear();
            control.Item.CheckBehavior = enCheckBehavior.NoSelection;
            if (column == null) { return; }
            ItemCollectionList item = new();
            ItemCollectionList.GetItemCollection(item, column, null, enShortenStyle.Replaced, 10000);
            control.SuggestionsAdd(item);
            control.AddAllowed = enAddType.UserDef;
        }

        protected void StyleTextBox(TextBox? control) {
            control.Enabled = Enabled;
            control.GetStyleFrom(this);
            control.Verhalten = _multiLine || Height > 20
                ? enSteuerelementVerhalten.Scrollen_mit_Textumbruch
                : enSteuerelementVerhalten.Scrollen_ohne_Textumbruch;
        }

        private void _IdleTimer_Tick(object sender, System.EventArgs e) {
            if (LastTextChange == null) { return; }
            if (DateTime.UtcNow.Subtract((DateTime)LastTextChange).TotalSeconds < 20) { return; }
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
            if (_editType != enEditTypeFormula.Button) { return; }
            ValueSet(true.ToPlusMinus(), false, true); // Geklickt, wurde hiermit vermerkt
            OnButtonClicked();
        }

        /// <summary>
        /// Erstellt das Steuerelement. Die Events werden Registriert und auch der Wert gesetzt.
        /// </summary>
        private Button Control_Create_ButtonColor() {
            Button control = new() {
                Enabled = Enabled,
                Name = "ColorButton",
                Checked = false,
                ButtonStyle = enButtonStyle.Button,
                Text = string.Empty
            };
            StandardBehandlung(control);
            return control;
        }

        /// <summary>
        /// Erstellt das Steuerelement. Die Events werden Registriert und auch der Wert gesetzt.
        /// </summary>
        private Button Control_Create_ButtonComand() {
            Button control = new() {
                Enabled = Enabled,
                Name = "ComandButton",
                Checked = false,
                ButtonStyle = enButtonStyle.Button,
                Text = _caption
            };
            StandardBehandlung(control);
            return control;
        }

        /// <summary>
        /// Erstellt das Steuerelement. Die Events werden Registriert und auch der Wert gesetzt.
        /// </summary>
        private Button Control_Create_ButtonYesNo() {
            Button control = new() {
                Enabled = Enabled,
                Name = "YesNoButton",
                ButtonStyle = enButtonStyle.Yes_or_No,
                Text = "",
                ImageCode = ""
            };
            UpdateValueToControl();
            StandardBehandlung(control);
            return control;
        }

        private void Control_Create_Caption() {
            if (_captionPosition == enÜberschriftAnordnung.ohne) { return; }
            if (_captionObject == null) {
                _captionObject = new Caption();
                Controls.Add(_captionObject);
            }
            _captionObject.Enabled = Enabled;
            _captionObject.TextAnzeigeVerhalten = enSteuerelementVerhalten.Text_Abschneiden; // nicht enSteuerelementVerhalten.Steuerelement_Anpassen! weil sonst beim einem Resize die Koordinaten geändert werden und das kann zum Ping Pong führen
            _captionObject.Text = _caption.ReplaceLowerSign();
            _captionObject.Size = _captionObject.TextRequiredSize();
            _captionObject.Left = 0;
            _captionObject.Top = 0;
            _captionObject.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            _captionObject.Visible = _captionPosition != enÜberschriftAnordnung.Ohne_mit_Abstand;
            _captionObject.Translate = TranslateCaption;
            _captionObject.BringToFront();
        }

        /// <summary>
        /// Erstellt das Steuerelement. Die Events werden Registriert und auch der Wert gesetzt.
        /// </summary>
        private ComboBox Control_Create_ComboBox() {
            ComboBox control = new();
            StyleComboBox(control, null, ComboBoxStyle.DropDownList);
            UpdateValueToControl();
            StandardBehandlung(control);
            return control;
        }

        /// <summary>
        /// Erstellt das Steuerelement. Die Events werden Registriert und auch der Wert gesetzt.
        /// </summary>
        private EasyPic Control_Create_EasyPic() {
            EasyPic control = new() {
                Enabled = Enabled
            };
            StandardBehandlung(control);
            UpdateValueToControl();
            return control;
        }

        /// <summary>
        /// Erstellt das Steuerelement. Die Events werden Registriert und auch der Wert gesetzt.
        /// </summary>
        private Line Control_Create_Line() {
            Line control = new() {
                Enabled = Enabled,
                Orientation = enOrientation.Waagerecht
            };
            StandardBehandlung(control);
            return control;
        }

        /// <summary>
        /// Erstellt das Steuerelement. Die Events werden Registriert und auch der Wert gesetzt.
        /// </summary>
        private ListBox Control_Create_ListBox() {
            ListBox control = new() {
                Enabled = Enabled
            };
            StyleListBox(control, null);
            UpdateValueToControl();
            StandardBehandlung(control);
            return control;
        }

        // Nimmt Teilweise die Routinen der Listbox her
        private SwapListBox Control_Create_SwapListBox() {
            SwapListBox control = new() {
                Enabled = Enabled
            };
            StyleSwapListBox(control, null);
            UpdateValueToControl();
            StandardBehandlung(control);
            return control;
        }

        /// <summary>
        /// Erstellt das Steuerelement. Die Events werden Registriert und auch der Wert gesetzt.
        /// </summary>
        private TextBox Control_Create_TextBox() {
            TextBox control = new();
            StyleTextBox(control);
            UpdateValueToControl();
            StandardBehandlung(control);
            return control;
        }

        private void DoInfoTextCaption(string disabledReason) {
            string txt;
            string symbol;
            if (string.IsNullOrEmpty(disabledReason) && string.IsNullOrEmpty(_infoText)) {
                txt = string.Empty;
                symbol = string.Empty;
            } else if (!string.IsNullOrEmpty(disabledReason) && string.IsNullOrEmpty(_infoText)) {
                symbol = "  <ImageCode=Schloss|10|||||150||20>";
                txt = disabledReason;
            } else if (string.IsNullOrEmpty(disabledReason) && !string.IsNullOrEmpty(_infoText)) {
                symbol = "<ImageCode=Warnung|16>";
                txt = _infoText;
            } else {
                symbol = "<ImageCode=Information|16>";
                txt = "<b>Der Wert kann nicht bearbeitet werden:</b><br>" + disabledReason + "<br><br><b>Enthält aber einen Fehler:</b><br>" + _infoText;
            }
            if (!_showInfoWhenDisabled && !string.IsNullOrEmpty(disabledReason)) { txt = string.Empty; }
            if (!string.IsNullOrEmpty(txt) && _infoCaption != null) {
                _infoCaption.Left = Width - 18;
                _infoCaption.Top = 0;
                _infoCaption.QuickInfo = txt;
                _infoCaption.Text = symbol;
                _infoCaption.Visible = true;
                _infoCaption.BringToFront();
                return;
            }
            if (string.IsNullOrEmpty(txt) && _infoCaption == null) { return; }
            if (string.IsNullOrEmpty(txt)) {
                //Controls.Remove(_InfoCaption);
                //_InfoCaption.Click -= _InfoCaption_Click;
                //_InfoCaption.Dispose();
                //_InfoCaption = null;
                _infoCaption.Visible = false;
            } else {
                _infoCaption = new Caption {
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
                Controls.Add(_infoCaption);
                _infoCaption.BringToFront();
                _infoCaption.Click += _InfoCaption_Click;
                _infoCaption.BringToFront();
            }
        }

        private void ListBox_ItemAdded(object sender, ListEventArgs e) {
            if (IsFilling) { return; }
            ValueSet(((ListBox)sender).Item.ToListOfString().JoinWithCr(), false, true);
        }

        private void ListBox_ItemRemoved(object sender, System.EventArgs e) {
            if (IsFilling) { return; }
            ValueSet(((ListBox)sender).Item.ToListOfString().JoinWithCr(), false, true);
        }

        private void ParentForm_FormClosing(object sender, FormClosingEventArgs e) => RaiseEventIfChanged(); // Versuchen, die Werte noch zurückzugeben

        private void RaiseEventIfChanged() {
            if (LastTextChange == null) { return; }
            LastTextChange = null;
            OnValueChanged();
        }

        /// <summary>
        /// Erstellt zuerst die Standard-Caption, dessen Events werden registriert.
        /// Kümmert sich dann um die Position des Controls im Bezug auf die Caption. Setzt die Sichtbarkeit, korrigiert Anachor und fügt das Control zu der Controll Collection hinzu.
        /// Konext-Menü-Events werden ebenfalls registriert, die andern Events werden nicht registriert und sollten nach dieser Rountine registert werden.
        /// </summary>
        /// <param name="control"></param>
        private void StandardBehandlung(GenericControl? control) {
            Control_Create_Caption();
            switch (_captionPosition) {
                case enÜberschriftAnordnung.ohne:
                    control.Left = 0;
                    control.Top = 0;
                    control.Width = Width;
                    control.Height = Height;
                    break;

                case enÜberschriftAnordnung.Links_neben_Dem_Feld:
                    control.Left = Math.Max(_controlX, _captionObject.Width);
                    control.Top = 0;
                    control.Width = Width - control.Left;
                    control.Height = Height;
                    if (_captionObject.Width < 4) { Develop.DebugPrint("Caption Width zu klein"); }
                    break;

                case enÜberschriftAnordnung.Über_dem_Feld:
                case enÜberschriftAnordnung.Ohne_mit_Abstand:
                    control.Left = 0;
                    control.Top = _captionObject.Height;
                    control.Width = Width;
                    control.Height = Height - _captionObject.Height;
                    if (_captionObject.Height < 4) { Develop.DebugPrint("Caption Height zu klein"); }
                    break;

                default:
                    Develop.DebugPrint(_captionPosition);
                    break;
            }
            control.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            control.Visible = true;
            Controls.Add(control);
            Invalidate();
            //DoInfoTextCaption();
        }

        private void SwapListBox_ItemAdded(object sender, ListEventArgs e) {
            if (IsFilling) { return; }
            ValueSet(((SwapListBox)sender).Item.ToListOfString().JoinWithCr(), false, true);
        }

        private void SwapListBox_ItemRemoved(object sender, System.EventArgs e) {
            if (IsFilling) { return; }
            ValueSet(((SwapListBox)sender).Item.ToListOfString().JoinWithCr(), false, true);
        }

        private void TextEditControl_LostFocus(object sender, System.EventArgs e) => RaiseEventIfChanged();

        private void UpdateControls() {
            foreach (Control control in Controls) {
                if (control != _infoCaption) {
                    if (control is GenericControl qi) { qi.QuickInfo = QuickInfo; }
                    control.Enabled = Enabled;
                } else {
                    control.Enabled = true;
                }

                if (control is IInputFormat inf) { inf.GetStyleFrom(inf); }
            }
        }

        /// <summary>
        /// Setzt den aktuellen Wert, so dass es das Control anzeigt. Filling muss TRUE sein.
        /// </summary>
        private void UpdateValueTo_Button(Button control) {
            //if (!_IsFilling) { Develop.DebugPrint(enFehlerArt.Fehler, "Filling und Creating False!"); }
            switch (_editType) {
                case enEditTypeFormula.Ja_Nein_Knopf:
                    control.Checked = Value.FromPlusMinus();
                    break;

                case enEditTypeFormula.Button:
                    break;

                case enEditTypeFormula.Farb_Auswahl_Dialog:
                    control.ImageCode = string.IsNullOrEmpty(Value) ? "Fragezeichen|24" : "Kreis|24|||" + Color.FromArgb(IntParse(Value)).ToHtmlCode();
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
            if (_editType != enEditTypeFormula.nur_als_Text_anzeigen) { return; } // und auch dann nur als reine Text anzeige
            if (_captionObject == null) { return; }
            _captionObject.Width = Width;
            _captionObject.Translate = false;
            _captionObject.Text = _caption + " <i>" + Value;
        }

        /// <summary>
        /// Setzt den aktuellen Wert, so dass es das Control anzeigt. Filling muss TRUE sein.
        /// </summary>
        private void UpdateValueTo_Combobox(TextBox control) =>
            //if (!_IsFilling) { Develop.DebugPrint(enFehlerArt.Fehler, "Filling muss TRUE sein!"); }
            control.Text = Value;

        /// <summary>
        /// Setzt den aktuellen Wert, so dass es das Control anzeigt. Filling  muss TRUE sein.
        /// </summary>
        private void UpdateValueTo_EasyPic(EasyPic control) =>
            //if (!_IsFilling) { Develop.DebugPrint(enFehlerArt.Fehler, "Filling muss TRUE sein!"); }
            control.FromFile(Value);

        /// <summary>
        /// Setzt den aktuellen Wert, so dass es das Control anzeigt. Filling muss TRUE sein.
        /// </summary>
        private void UpdateValueTo_ListBox(ItemCollectionList main) => main.SetValuesTo(Value.SplitAndCutByCrToList(), FileEncryptionKey);

        /// <summary>
        /// Setzt den aktuellen Wert, so dass es das Control anzeigt. Filling muss TRUE sein.
        /// </summary>
        private void UpdateValueTo_TextBox(TextBox control) =>
            //if (!_IsFilling) { Develop.DebugPrint(enFehlerArt.Fehler, "Filling muss TRUE sein!"); }
            control.Text = Value;

        /// <summary>
        /// Setzt den aktuellen Wert, so dass es das Control anzeigt. Die Filling-Variable wird währenddessen umgesetzt.
        /// sollte vor StandardBehandlung kommen, da dort das Objekt gesetzt wird und dann die Handler generiert werden.
        /// </summary>
        private void UpdateValueToControl() {
            if (!Allinitialized) { CreateSubControls(); }
            IsFilling = true;
            foreach (Control control in Controls) {
                switch (control) {
                    case ComboBox comboBox:
                        UpdateValueTo_Combobox(comboBox);
                        break;

                    case TextBox textBox:
                        UpdateValueTo_TextBox(textBox);
                        break;

                    case EasyPic easyPic:
                        UpdateValueTo_EasyPic(easyPic);
                        break;

                    case ListBox listBox:
                        UpdateValueTo_ListBox(listBox.Item);
                        break;

                    case SwapListBox swapListBox:
                        UpdateValueTo_ListBox(swapListBox.Item);
                        break;

                    case Button button:
                        UpdateValueTo_Button(button);
                        break;

                    case Caption _:
                        UpdateValueTo_Caption();
                        break;

                    case Line:
                        if (!string.IsNullOrEmpty(Value)) {
                            Develop.DebugPrint(enFehlerArt.Fehler, "Line kann keine Value erhalten: '" + Value + "'");
                        }
                        break;

                    default:
                        Develop.DebugPrint(Typ(control));
                        break;
                }
            }
            IsFilling = false;
        }

        private void ValueChanged_ComboBox(object sender, System.EventArgs e) {
            if (IsFilling) { return; }
            ValueSet(((ComboBox)sender).Text, false, ((ComboBox)sender).DropDownStyle == ComboBoxStyle.DropDownList);
        }

        private void ValueChanged_TextBox(object sender, System.EventArgs e) {
            if (IsFilling) { return; }
            ValueSet(((TextBox)sender).Text, false, false);
        }

        // TODO: Erstellen!//if (_EditType != enEditTypeFormula.Button) { return; }//CheckIfChanged("+");//OnButtonClicked();
        private void YesNoButton_CheckedChanged(object sender, System.EventArgs e) => ValueSet(((Button)sender).Checked.ToPlusMinus(), false, true);

        #endregion
    }
}