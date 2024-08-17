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
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static BlueBasics.Converter;
using Orientation = BlueBasics.Enums.Orientation;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[DefaultEvent("ValueChanged")]
public partial class FlexiControl : GenericControl, IBackgroundNone, IInputFormat, ITranslateable {

    #region Fields

    private AdditionalCheck _additionalCheck = AdditionalCheck.None;
    private string _allowedChars = string.Empty;
    private string _caption = string.Empty;
    private Caption? _captionObject;
    private CaptionPosition _captionPosition = CaptionPosition.ohne;

    // None ist -1 und muss gesetzt sein!
    private int _controlX = -1;

    private string _disabledReason = string.Empty;
    private EditTypeFormula _editType;
    private bool _formatierungErlaubt;
    private Caption? _infoCaption;
    private string _infoText = string.Empty;
    private int _maxTextLenght = 4000;
    private bool _multiLine;
    private string _regex = string.Empty;
    private bool _showInfoWhenDisabled;
    private bool _spellChecking;
    private string _suffix = string.Empty;
    private bool _translateCaption = true;

    #endregion

    #region Constructors

    public FlexiControl() : base(false, false) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();

        _editType = EditTypeFormula.Line;
        Size = new Size(200, 8);
    }

    /// <summary>
    /// Einfacher Info Text. Wird nirgends mehr zurück gegeben.
    /// </summary>
    /// <param name="captionText"></param>
    /// <param name="width"></param>
    /// <param name="isCaption"></param>
    public FlexiControl(string captionText, int width, bool isCaption) : base(false, false) {
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

        if (isCaption) {
            _editType = EditTypeFormula.als_Überschrift_anzeigen;
        } else {
            _editType = EditTypeFormula.nur_als_Text_anzeigen;
        }

        _caption = captionText;
        _captionPosition = CaptionPosition.Links_neben_dem_Feld;

        //var s = ExtText.MeasureString(_caption, Design.Caption, States.Standard, width - 2);

        //var s = BlueFont.MeasureString(_caption, Skin.GetBlueFont(Design.Caption, States.Standard).Font());
        //  Size = new Size(s.Width + 2, s.Height + 2);

        Size = BlueControls.Controls.Caption.RequiredTextSize(_caption, Design.Caption, null, Translate, width);
    }

    #endregion

    #region Events

    public event EventHandler? ButtonClicked;

    [Obsolete("Value Changed benutzen", true)]
    public new event EventHandler? TextChanged;

    //public event EventHandler? ButtonClicked;
    //public event EventHandler? NeedRefresh;
    public event EventHandler? ValueChanged;

    #endregion

    #region Properties

    [DefaultValue(AdditionalCheck.None)]
    public AdditionalCheck AdditionalFormatCheck {
        get => _additionalCheck;
        set {
            if (_additionalCheck == value) { return; }
            _additionalCheck = value;
            UpdateControls();
        }
    }

    public bool Allinitialized { get; private set; }

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

    [DefaultValue(CaptionPosition.ohne)]
    public CaptionPosition CaptionPosition {
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

    /// <summary>
    /// Info wird nur angezeigt, wenn ShowInfoWhenDisabled True ist
    /// </summary>
    [DefaultValue("")]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string DisabledReason {
        get => _disabledReason;
        set {
            if (_disabledReason == value) { return; }
            _disabledReason = value;
            foreach (Control thisControl in Controls) {
                thisControl.Enabled = thisControl == _infoCaption || Enabled;
            }
            DoInfoTextCaption(_disabledReason);
            Invalidate();
        }
    }

    [DefaultValue(EditTypeFormula.None)]
    public EditTypeFormula EditType {
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

    public bool Initializing { get; private set; }

    ///// <summary>
    ///// Wenn True, wird ValueChanged NICHT ausgelöst
    ///// </summary>
    public bool IsFilling { get; protected set; }

    [DefaultValue(4000)]
    public int MaxTextLenght {
        get => _maxTextLenght;
        set {
            if (_maxTextLenght == value) { return; }
            _maxTextLenght = value;
            UpdateControls();
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
    public bool SpellCheckingEnabled {
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
    [Obsolete("Value anstelle Text benutzen", true)]
    [DefaultValue("")]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new string Text { get; set; } = string.Empty;

    [DefaultValue(true)]
    public bool Translate {
        get => _translateCaption;
        set {
            if (_translateCaption == value) { return; }
            _translateCaption = value;

            UpdateControls();
        }
    }

    /// <summary>
    /// Info: Zum Setzen des Wertes muss ValueSet benutzt werden.
    /// </summary>
    [DefaultValue("")]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Value { get; private set; } = string.Empty;

    #endregion

    #region Methods

    /// <summary>
    /// Erstellt die Steuerelemente zur Bearbeitung und auch die Caption und alles was gebrauch wird.
    /// Die Events werden Registriert und auch der Wert gesetzt.
    /// </summary>
    public void CreateSubControls() {
        if (Allinitialized || Initializing) { return; }

        Initializing = true;

        if (Width < 5 || Height < 5) {
            Develop.DebugPrint(FehlerArt.Warnung, "Width / Height zu klein");
            Allinitialized = true;
            Initializing = false;
            return;
        }

        switch (_editType) {
            case EditTypeFormula.Line:
                Control_Create_Line();
                break;

            case EditTypeFormula.Textfeld:
                Control_Create_TextBox();
                break;

            case EditTypeFormula.Listbox:
                Control_Create_ListBox();
                break;

            case EditTypeFormula.Textfeld_mit_Auswahlknopf:
                Control_Create_ComboBox();
                break;

            case EditTypeFormula.Ja_Nein_Knopf:
                Control_Create_ButtonYesNo();
                break;

            case EditTypeFormula.None:
                break;

            case EditTypeFormula.als_Überschrift_anzeigen:
                _captionPosition = CaptionPosition.ohne;
                Control_Create_GroupBox();
                break;

            case EditTypeFormula.nur_als_Text_anzeigen:
                _captionPosition = CaptionPosition.Links_neben_dem_Feld;
                Control_Create_Caption();
                break;

            case EditTypeFormula.Farb_Auswahl_Dialog:
                Control_Create_ButtonColor();
                break;

            case EditTypeFormula.Button:
                Control_Create_ButtonCommand();
                break;

            case EditTypeFormula.SwapListBox:
                Control_Create_SwapListBox();
                break;

                //default:
                //Develop.DebugPrint(_editType);
                //return null;
        }
        UpdateControls();
        Allinitialized = true;
        Initializing = false;
    }

    public Button? GetButton() {
        CreateSubControls();
        foreach (var thisc in Controls) {
            if (thisc is Button bt) { return bt; }
        }
        return null;
    }

    public ComboBox? GetComboBox() {
        CreateSubControls();
        foreach (var thisc in Controls) {
            if (thisc is ComboBox cbx) { return cbx; }
        }
        return null;
    }

    public TextBox? GetTextBox() {
        CreateSubControls();
        foreach (var thisc in Controls) {
            if (thisc is TextBox txb) { return txb; }
        }
        return null;
    }

    public void StyleComboBox(ComboBox? control, IEnumerable<AbstractListItem>? list, ComboBoxStyle style, bool removevalueIfNotExists) {
        if (control == null) { return; }

        //control.Enabled = Enabled;
        control.GetStyleFrom(this);
        control.DropDownStyle = style;
        control.ItemClear();
        control.ItemEditAllowed = string.Equals(Generic.UserGroup, Constants.Administrator, StringComparison.OrdinalIgnoreCase);
        if (list != null) {
            control.ItemAddRange(list);
        }

        if (removevalueIfNotExists) {
            if (control[Value] == null) {
                ValueSet(string.Empty, true);
            }
        }
    }

    public void StyleTextBox(TextBox? control) {
        if (control == null) { return; }
        control.GetStyleFrom(this);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="newvalue"></param>
    /// <param name="updateControls"></param>
    ///
    public void ValueSet(string? newvalue, bool updateControls) {
        if (IsDisposed) { return; }
        newvalue ??= string.Empty;

        if (Value == newvalue) { return; }

        Value = newvalue;
        if (updateControls) { UpdateValueToControl(); }
        OnValueChanged();
    }

    protected virtual void CommandButton_Click(object sender, System.EventArgs e) {
        if (_editType != EditTypeFormula.Button) { return; }
        //ValueSet(true.ToPlusMinus(), false); // Geklickt, wurde hiermit vermerkt
        OnButtonClicked();
    }

    protected override void Dispose(bool disposing) {
        try {
            if (disposing) {
                //if (ParentForm() is Form parentForm) {
                //    parentForm.FormClosing -= ParentForm_FormClosing;
                //}

                _infoText = string.Empty;
                //if (_BitmapOfControl != null) { _BitmapOfControl?.Dispose(); }
                //DoInfoTextButton(); // Events entfernen!
                RemoveAll(); // Events entfernen!

                foreach (var thisc in Controls) {
                    if (thisc is IDisposable d) { d.Dispose(); }
                }

                components?.Dispose();
            }
        } finally {
            base.Dispose(disposing);
        }
    }

    protected override void DrawControl(Graphics gr, States state) {
        // Enabled wurde verdeckt!
        if (!Enabled) { state = States.Standard_Disabled; }
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
            DoInfoTextCaption(state.HasFlag(States.Standard_Disabled) ? "Übergeordnetes Steuerlement ist deaktiviert." : string.Empty);
        }
    }

    protected ListBox? GetListBox() {
        CreateSubControls();
        foreach (var thisc in Controls) {
            if (thisc is ListBox lb) { return lb; }
        }
        return null;
    }

    protected virtual void OnButtonClicked() => ButtonClicked?.Invoke(this, System.EventArgs.Empty);

    protected override void OnControlAdded(ControlEventArgs e) {
        base.OnControlAdded(e);
        switch (e.Control) {
            case ComboBox comboBox:
                comboBox.TextChanged += ValueChanged_ComboBox;
                comboBox.RaiseChangeDelay = 1;
                break;

            case TextBox textBox:
                textBox.TextChanged += ValueChanged_TextBox;
                textBox.RaiseChangeDelay = 1;
                break;

            case GroupBox:
            case Caption _:
            case Line:
                break;

            case SwapListBox swapListBox:
                //swapListBox.ItemAdded += SwapListBox_ItemAdded;
                //swapListBox.ItemRemoved += SwapListBox_ItemRemoved;
                swapListBox.ItemCheckedChanged += SwapListBox_ItemCheckedChanged;
                break;

            case ListBox listBox:
                listBox.ItemCheckedChanged += ListBox_ItemCheckedChanged;
                //listBox.ItemRemoved += ListBox_ItemRemoved;
                break;

            case Button button:
                switch (_editType) {
                    case EditTypeFormula.Ja_Nein_Knopf:
                        button.CheckedChanged += YesNoButton_CheckedChanged;
                        break;

                    case EditTypeFormula.Button:
                        button.Click += CommandButton_Click;
                        break;

                    case EditTypeFormula.Farb_Auswahl_Dialog:
                        button.Click += ColorButton_Click;
                        break;

                    default:
                        Develop.DebugPrint_NichtImplementiert(true);
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
                break;

            case TextBox textBox:
                textBox.TextChanged -= ValueChanged_TextBox;
                break;

            case GroupBox:
            case Caption _:
            case Line:
                break;

            case ListBox listBox:
                listBox.ItemCheckedChanged -= ListBox_ItemCheckedChanged;
                //listBox.ItemRemoved -= ListBox_ItemRemoved;
                break;

            case SwapListBox swapListBox:
                //swapListBox.ItemAdded -= SwapListBox_ItemAdded;
                //swapListBox.ItemRemoved -= SwapListBox_ItemRemoved;
                swapListBox.ItemCheckedChanged -= SwapListBox_ItemCheckedChanged;
                break;

            case Button button:
                switch (_editType) {
                    case EditTypeFormula.Ja_Nein_Knopf:
                        button.CheckedChanged -= YesNoButton_CheckedChanged;
                        break;

                    case EditTypeFormula.Button:
                        button.Click -= CommandButton_Click;
                        break;

                    case EditTypeFormula.Farb_Auswahl_Dialog:
                        button.Click -= ColorButton_Click;
                        break;

                    default:
                        Develop.DebugPrint_NichtImplementiert(true);
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

    //protected virtual void OnNeedRefresh() => NeedRefresh?.Invoke(this, System.EventArgs.Empty);

    //protected override void OnParentChanged(System.EventArgs e) {
    //    base.OnParentChanged(e);

    //    if (ParentForm() is Form parentForm) {
    //        parentForm.FormClosing += ParentForm_FormClosing;
    //    }
    //}

    protected override void OnQuickInfoChanged() {
        base.OnQuickInfoChanged();
        UpdateControls();
    }

    protected virtual void OnValueChanged() => ValueChanged?.Invoke(this, System.EventArgs.Empty);

    /// <summary>
    /// Entfernt alle Controls und löst dessen die Events auf. Setzt _allinitialized auf false.
    /// </summary>
    protected virtual void RemoveAll() {
        List<Control> l = [];
        for (var z = 0; z < Controls.Count; z++) { l.Add(Controls[z]); }

        foreach (var thisc in l) {
            thisc.Visible = false;
            if (thisc != _captionObject && thisc != _infoCaption) {
                thisc.Dispose(); // Dispose entfernt dass Control aus der Collection
            }
        }

        Allinitialized = false;
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

    private void ColorButton_Click(object sender, System.EventArgs e) => Develop.DebugPrint_NichtImplementiert(false);

    /// <summary>
    /// Erstellt das Steuerelement. Die Events werden Registriert und auch der Wert gesetzt.
    /// </summary>
    private Button Control_Create_ButtonColor() {
        Button control = new() {
            Enabled = Enabled,
            Name = "ColorButton",
            Checked = false,
            ButtonStyle = ButtonStyle.Button,
            Text = string.Empty
        };
        StandardBehandlung(control);
        return control;
    }

    /// <summary>
    /// Erstellt das Steuerelement. Die Events werden Registriert und auch der Wert gesetzt.
    /// </summary>
    private Button Control_Create_ButtonCommand() {
        Button control = new() {
            Enabled = Enabled,
            Name = "CommandButton",
            Checked = false,
            ButtonStyle = ButtonStyle.Button,
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
            ButtonStyle = ButtonStyle.Yes_or_No,
            Text = string.Empty,
            ImageCode = string.Empty
        };
        StandardBehandlung(control);
        UpdateValueToControl();
        return control;
    }

    private void Control_Create_Caption() {
        if (_captionPosition == CaptionPosition.ohne) { return; }
        if (_captionObject == null) {
            _captionObject = new Caption();
            Controls.Add(_captionObject);
        }
        _captionObject.Enabled = Enabled;

        _captionObject.Left = 0;
        _captionObject.Top = 0;
        _captionObject.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        _captionObject.Visible = true; // _captionPosition != ÜberschriftAnordnung.Ohne_mit_Abstand;
        _captionObject.Translate = _translateCaption;

        // nicht SteuerelementVerhalten.Steuerelement_Anpassen! weil sonst beim einem Resize die Koordinaten geändert werden und das kann zum Ping Pong führen
        // Text_abschneiden, wäre Cool, weil dann der Quickmode verfügbar ist

        if (_captionPosition is CaptionPosition.Links_neben_dem_Feld_unsichtbar
                     or CaptionPosition.Über_dem_Feld_unsichtbar) {
            _captionObject.Text = " ";
        } else {
            _captionObject.Text = _caption;
        }

        if (_editType == EditTypeFormula.nur_als_Text_anzeigen) {
            // Kann alles sein, Beschriftung und was weiß ich.
            _captionObject.Size = BlueControls.Controls.Caption.RequiredTextSize(_captionObject.Text, Design.Caption, null, false, Width);
        } else {
            _captionObject.Size = _captionObject.RequiredTextSize();
        }

        _captionObject.BringToFront();
    }

    //public Size MeasureStringOfCaption(string text) => Skin.GetBlueFont(Design.Caption, States.Standard).MeasureString(text);

    /// <summary>
    /// Erstellt das Steuerelement. Die Events werden Registriert und auch der Wert gesetzt.
    /// </summary>

    private ComboBox Control_Create_ComboBox() {
        ComboBox control = new();
        StyleComboBox(control, null, ComboBoxStyle.DropDownList, false);
        StandardBehandlung(control);
        UpdateValueToControl();
        return control;
    }

    /// <summary>
    /// Erstellt das Steuerelement. Die Events werden Registriert und auch der Wert gesetzt.
    /// </summary>
    private GroupBox Control_Create_GroupBox() {
        GroupBox control = new() {
            Enabled = Enabled,
            GroupBoxStyle = GroupBoxStyle.NormalBold,
            Text = _caption
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
            Orientation = Orientation.Waagerecht
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
        control.ItemClear();
        control.CheckBehavior = CheckBehavior.MultiSelection;
        StandardBehandlung(control);
        UpdateValueToControl();
        return control;
    }

    // Nimmt Teilweise die Routinen der Listbox her

    private SwapListBox Control_Create_SwapListBox() {
        SwapListBox control = new() {
            Enabled = Enabled
        };
        control.UnCheck();
        StandardBehandlung(control);
        UpdateValueToControl();
        return control;
    }

    /// <summary>
    /// Erstellt das Steuerelement. Die Events werden Registriert und auch der Wert gesetzt.
    /// </summary>

    private TextBox Control_Create_TextBox() {
        TextBox control = new();
        StyleTextBox(control);
        StandardBehandlung(control);
        UpdateValueToControl();
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
            if (_infoCaption != null) { _infoCaption.Visible = false; }
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

    private void ListBox_ItemCheckedChanged(object sender, System.EventArgs e) {
        if (IsFilling) { return; }
        ValueSet(((ListBox)sender).Checked.JoinWithCr(), false);
    }

    // Versuchen, die Werte noch zurückzugeben
    /// <summary>
    /// Erstellt zuerst die Standard-Caption, dessen Events werden registriert.
    /// Kümmert sich dann um die Position des Controls im Bezug auf die Caption. Setzt die Sichtbarkeit, korrigiert Anachor und fügt das Control zu der Controll Collection hinzu.
    /// Konext-Menü-Events werden ebenfalls registriert, die andern Events werden nicht registriert und sollten nach dieser Rountine registert werden.
    /// </summary>
    /// <param name="control"></param>
    private void StandardBehandlung(Control control) {
        Control_Create_Caption();
        switch (_captionPosition) {
            case CaptionPosition.ohne:
                control.Left = 0;
                control.Top = 0;
                control.Width = Width;
                control.Height = Height;
                break;

            case CaptionPosition.Links_neben_dem_Feld_unsichtbar:
            case CaptionPosition.Links_neben_dem_Feld:
                var s1 = BlueControls.Controls.Caption.RequiredTextSize(_caption, Design.Caption, null, Translate, -1);

                control.Left = Math.Max(_controlX, s1.Width);
                control.Top = 0;
                control.Width = Width - control.Left;
                control.Height = Height;
                break;

            default:
                var s2 = BlueControls.Controls.Caption.RequiredTextSize(_caption, Design.Caption, null, Translate, -1);

                //var s = MeasureStringOfCaption(_caption).ToSize();
                control.Left = 0;
                control.Top = Math.Max(_controlX, s2.Height);
                control.Width = Width;
                control.Height = Height - s2.Height;
                break;
        }
        control.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        control.Visible = true;
        Controls.Add(control);
        Invalidate();
        //DoInfoTextCaption();
    }

    private void SwapListBox_ItemCheckedChanged(object sender, System.EventArgs e) {
        if (IsFilling) { return; }
        ValueSet(((SwapListBox)sender).Checked.JoinWithCr(), false);
    }

    private void UpdateControls() {
        if (_captionObject is { IsDisposed: false } c) { c.Translate = _translateCaption; }

        foreach (Control control in Controls) {
            if (control != _infoCaption) {
                if (control is GenericControl qi) { qi.QuickInfo = QuickInfo; }
                control.Enabled = Enabled;
            } else {
                control.Enabled = true;
            }

            if (control is IInputFormat inf) { inf.GetStyleFrom(this); }
        }
    }

    /// <summary>
    /// Setzt den aktuellen Wert, so dass es das Control anzeigt. Filling muss TRUE sein.
    /// </summary>
    private void UpdateValueTo_Button(Button control) {
        //if (!_IsFilling) { Develop.DebugPrint(enFehlerArt.Fehler, "Filling und Creating False!"); }
        switch (_editType) {
            case EditTypeFormula.Ja_Nein_Knopf:
                control.Checked = Value.FromPlusMinus();
                break;

            case EditTypeFormula.Button:
                break;

            case EditTypeFormula.Farb_Auswahl_Dialog:
                control.ImageCode = string.IsNullOrEmpty(Value) ? "Fragezeichen|24" : "Kreis|24|||" + Color.FromArgb(IntParse(Value)).ToHtmlCode();
                break;

            default:
                Develop.DebugPrint_NichtImplementiert(true);
                break;
        }
    }

    /// <summary>
    /// Setzt den aktuellen Wert, so dass es das Control anzeigt. Filling muss TRUE sein.
    /// </summary>
    private void UpdateValueTo_Caption() {
        //if (!_IsFilling) { Develop.DebugPrint(enFehlerArt.Fehler, "Filling muss TRUE sein!"); }
        //if (Column  ==null || Column .IsDisposed) { return; } // nur mögloch bei verbundenen Datenbanken
        if (_editType != EditTypeFormula.nur_als_Text_anzeigen) { return; } // und auch dann nur als reine Text anzeige
        if (_captionObject == null) { return; }
        _captionObject.Width = Width;
        _captionObject.Translate = false;
        _captionObject.Text = _caption + " <i>" + Value;
    }

    /// <summary>
    /// Setzt den aktuellen Wert, so dass es das Control anzeigt. Filling muss TRUE sein.
    /// </summary>

    private void UpdateValueTo_Combobox(TextBox control) => control.Text = Value;

    /// <summary>
    /// Setzt den aktuellen Wert, so dass es das Control anzeigt. Filling muss TRUE sein.
    /// </summary>
    private void UpdateValueTo_ListBox(ListBox main) {
        main.UncheckAll();
        main.Check(Value.SplitAndCutByCrToList());
    }

    private void UpdateValueTo_SwapListBox(SwapListBox main) => main.Check(Value.SplitAndCutByCrToList());

    /// <summary>
    /// Setzt den aktuellen Wert, so dass es das Control anzeigt. Filling muss TRUE sein.
    /// </summary>

    private void UpdateValueTo_TextBox(TextBox control) => control.Text = Value;

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

                case ListBox listBox:
                    UpdateValueTo_ListBox(listBox);
                    break;

                case SwapListBox swapListBox:
                    UpdateValueTo_SwapListBox(swapListBox);
                    break;

                case Button button:
                    UpdateValueTo_Button(button);
                    break;

                case Caption _:
                    UpdateValueTo_Caption();
                    break;

                case GroupBox:
                case Line:
                    //if (!string.IsNullOrEmpty(Value)) {
                    //    Develop.DebugPrint(FehlerArt.Fehler, "Line kann keine Value erhalten: '" + Value + "'");
                    //}
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
        ValueSet(((ComboBox)sender).Text, false);
    }

    private void ValueChanged_TextBox(object sender, System.EventArgs e) {
        if (IsFilling) { return; }
        ValueSet(((TextBox)sender).Text, false);
    }

    private void YesNoButton_CheckedChanged(object sender, System.EventArgs e) => ValueSet(((Button)sender).Checked.ToPlusMinus(), false);

    #endregion
}