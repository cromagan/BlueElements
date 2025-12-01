// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using BlueBasics.Interfaces;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueTable;
using BlueTable.Enums;
using BlueTable.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static BlueBasics.Converter;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using Orientation = BlueBasics.Enums.Orientation;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[DefaultEvent(nameof(ValueChanged))]
public partial class FlexiControl : GenericControl, IBackgroundNone, IInputFormat, ITranslateable {

    #region Fields

    private string _caption = string.Empty;
    private Caption? _captionObject;
    private CaptionPosition _captionPosition = CaptionPosition.ohne;

    // None ist -1 und muss gesetzt sein!
    private EditTypeFormula _editType;

    private Caption? _infoCaption;
    private string _infoText = string.Empty;

    #endregion

    #region Constructors

    public FlexiControl() : base(false, false, false) {
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
    public FlexiControl(string captionText, int width, bool isCaption) : base(false, false, false) {
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.

        _editType = isCaption ? EditTypeFormula.als_Überschrift_anzeigen : EditTypeFormula.nur_als_Text_anzeigen;

        _caption = captionText;
        _captionPosition = CaptionPosition.Links_neben_dem_Feld;

        Size = BlueControls.Controls.Caption.RequiredTextSize(_caption, SteuerelementVerhalten.Scrollen_mit_Textumbruch, Design.Caption, null, Translate, width);
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
        get;
        set {
            if (field == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => AdditionalFormatCheck = value));
                return;
            }

            field = value;
            UpdateControls();
        }
    } = AdditionalCheck.None;

    public bool Allinitialized { get; private set; }

    [DefaultValue("")]
    public string AllowedChars {
        get;
        set {
            if (field == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => AllowedChars = value));
                return;
            }

            field = value;
            UpdateControls();
        }
    } = string.Empty;

    [DefaultValue("")]
    public string Caption {
        get => _caption;
        set {
            if (_caption == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => Caption = value));
                return;
            }

            RemoveAll(); // Controls and Events entfernen!
            _caption = value;
        }
    }

    [DefaultValue(CaptionPosition.ohne)]
    public CaptionPosition CaptionPosition {
        get => _captionPosition;
        set {
            if (_captionPosition == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => CaptionPosition = value));
                return;
            }

            RemoveAll(); // Controls and Events entfernen!
            _captionPosition = value;
        }
    }

    /// <summary>
    /// Ab welchen Wert in Pixel das Eingabesteuerelement beginnen darf.
    /// </summary>
    [DefaultValue(-1)]
    public int ControlX {
        get;
        set {
            if (field == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => ControlX = value));
                return;
            }

            RemoveAll(); // Controls and Events entfernen!
            field = value;
        }
    } = -1;

    /// <summary>
    /// Info wird nur angezeigt, wenn ShowInfoWhenDisabled True ist
    /// </summary>
    [DefaultValue("")]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string DisabledReason {
        get;
        set {
            if (field == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => DisabledReason = value));
                return;
            }

            field = value;
            foreach (Control thisControl in Controls) {
                thisControl.Enabled = thisControl == _infoCaption || Enabled;
            }
            DoInfoTextCaption(field);
            Invalidate();
        }
    } = string.Empty;

    [DefaultValue(EditTypeFormula.None)]
    public EditTypeFormula EditType {
        get => _editType;
        set {
            if (_editType == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => EditType = value));
                return;
            }

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
    public new bool Enabled => !DesignMode && string.IsNullOrEmpty(DisabledReason);

    public override bool Focused {
        get {
            foreach (Control thisControl in Controls) {
                if (thisControl.Focused) { return true; }
            }
            return base.Focused;
        }
    }

    [DefaultValue("")]
    [Description("Zeigt rechts oben im Eck ein kleines Symbol an, dessen hier eingegebener Text angezeigt wird.")]
    public string InfoText {
        get => _infoText;
        set {
            if (_infoText == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => InfoText = value));
                return;
            }

            _infoText = value;
            Invalidate();
        }
    }

    public bool Initializing { get; private set; }

    [DefaultValue(4000)]
    public int MaxTextLength {
        get;
        set {
            if (field == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => MaxTextLength = value));
                return;
            }

            field = value;
            UpdateControls();
        }
    } = 4000;

    /// <summary>
    /// Falls das Steuerelement Multiline unterstützt, wird dieser angezeigt
    /// </summary>
    [DefaultValue(false)]
    public bool MultiLine {
        get;
        set {
            if (field == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => MultiLine = value));
                return;
            }

            field = value;
            UpdateControls();
        }
    }

    [DefaultValue("")]
    public string RegexCheck {
        get;
        set {
            if (field == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => RegexCheck = value));
                return;
            }

            field = value;
            UpdateControls();
        }
    } = string.Empty;

    [DefaultValue(false)]
    public bool ShowInfoWhenDisabled {
        get;
        set {
            if (field == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => ShowInfoWhenDisabled = value));
                return;
            }

            field = value;
            Invalidate();
        }
    }

    [DefaultValue(false)]
    public bool SpellCheckingEnabled {
        get;
        set {
            if (field == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => SpellCheckingEnabled = value));
                return;
            }

            field = value;
            UpdateControls();
        }
    }

    /// <summary>
    /// Falls das Steuerelement eine Suffix unterstützt, wird dieser angezeigt
    /// </summary>
    [DefaultValue("")]
    public string Suffix {
        get;
        set {
            if (field == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => Suffix = value));
                return;
            }

            field = value;
            UpdateControls();
        }
    } = string.Empty;

    /// <summary>
    /// Value benutzen!
    /// </summary>
    [Obsolete("Value anstelle Text benutzen", true)]
    [DefaultValue("")]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new string Text { get; set; } = string.Empty;

    [DefaultValue(false)]
    public bool TextFormatingAllowed {
        get;
        set {
            if (value == TextFormatingAllowed) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => TextFormatingAllowed = value));
                return;
            }

            field = value;
            UpdateControls();
        }
    }

    [DefaultValue(true)]
    public bool Translate {
        get;
        set {
            if (field == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => Translate = value));
                return;
            }

            field = value;

            UpdateControls();
        }
    } = true;

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
        if (InvokeRequired) {
            Invoke(new Action(CreateSubControls));
            return;
        }

        if (Allinitialized || Initializing) { return; }

        Initializing = true;

        if (Width < 5 || Height < 5) {
            Develop.DebugPrint(ErrorType.Warning, "Width / Height zu klein");
            Allinitialized = true;
            Initializing = false;
            return;
        }

        Control? c = null;

        switch (_editType) {
            case EditTypeFormula.Line:
                c = Control_Create_Line();
                break;

            case EditTypeFormula.Textfeld:
                c = Control_Create_TextBox();
                break;

            case EditTypeFormula.Listbox:
                c = Control_Create_ListBox();
                break;

            case EditTypeFormula.Textfeld_mit_Auswahlknopf:
                c = Control_Create_ComboBox();
                break;

            case EditTypeFormula.Ja_Nein_Knopf:
                c = Control_Create_ButtonYesNo();
                break;

            case EditTypeFormula.None:
                break;

            case EditTypeFormula.als_Überschrift_anzeigen:
                _captionPosition = CaptionPosition.ohne;
                c = Control_Create_GroupBox();
                break;

            case EditTypeFormula.nur_als_Text_anzeigen:
                _captionPosition = CaptionPosition.Links_neben_dem_Feld;
                Control_Create_Caption();
                break;

            case EditTypeFormula.Farb_Auswahl_Dialog:
                c = Control_Create_ButtonColor();
                break;

            case EditTypeFormula.Button:
                c = Control_Create_ButtonCommand();
                break;

            case EditTypeFormula.SwapListBox:
                c = Control_Create_SwapListBox();
                break;
        }

        StandardBehandlung(c);
        UpdateValueToControl();

        UpdateControls();
        Allinitialized = true;
        Initializing = false;
    }

    public T? GetControl<T>() where T : Control {
        try {
            if (InvokeRequired) {
                return (T?)Invoke(new Func<T?>(GetControl<T>));
            }

            if (IsDisposed) { return null; }

            CreateSubControls();
            foreach (var control in Controls) {
                if (control is T typedControl) {
                    return typedControl;
                }
            }
            return null;
        } catch {
            return null;
        }
    }

    public void StyleComboBox(ComboBox? control, List<AbstractListItem>? list, ComboBoxStyle style, bool removevalueIfNotExists, int raiseChangeDelayinSec) {
        if (control == null) { return; }

        control.GetStyleFrom(this);
        control.RaiseChangeDelay = raiseChangeDelayinSec;
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

    public void StyleTextBox(TextBox? control, int raiseChangeDelayinSec) {
        if (control == null) { return; }
        control.GetStyleFrom(this);
        control.RaiseChangeDelay = raiseChangeDelayinSec;
        control.Verhalten = MultiLine || Height > 20
            ? SteuerelementVerhalten.Scrollen_mit_Textumbruch
            : SteuerelementVerhalten.Scrollen_ohne_Textumbruch;
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

    internal void StyleListBox(ListBox control, ColumnItem? column) {
        control.CheckBehavior = CheckBehavior.MultiSelection;
        if (column is not { IsDisposed: false }) { return; }

        var item = new List<AbstractListItem>();
        if (column.EditableWithDropdown) {
            var r = TableView.RendererOf(column, Constants.Win11);
            item.AddRange(ItemsOf(column, null, 10000, r, null));
            if (!column.ShowValuesOfOtherCellsInDropdown) {
                bool again;
                do {
                    again = false;
                    foreach (var thisItem in item) {
                        if (!column.DropDownItems.Contains(thisItem.KeyName)) {
                            again = true;
                            item.Remove(thisItem);
                            break;
                        }
                    }
                } while (again);
            }
        }
        control.ItemAddRange(item);

        switch (ColumnItem.UserEditDialogTypeInTable(column, false)) {
            case EditTypeTable.Textfeld:
                control.AddAllowed = AddType.Text;
                break;

            case EditTypeTable.Listbox:
                control.AddAllowed = AddType.OnlySuggests;
                break;

            default:
                control.AddAllowed = AddType.None;
                break;
        }

        control.MoveAllowed = false;
        switch (EditType) {
            //case EditTypeFormula.Gallery:
            //    control.Appearance = BlueListBoxAppearance.Gallery;
            //    control.RemoveAllowed = true;
            //    break;

            case EditTypeFormula.Listbox:
                control.RemoveAllowed = true;
                control.Appearance = ListBoxAppearance.Listbox;
                break;
        }
    }

    internal void StyleSwapListBox(SwapListBox control, ColumnItem? column) {
        //control.Enabled = Enabled;
        //control.UnCheck();
        control.SuggestionsClear();
        if (column is not { IsDisposed: false }) { return; }

        var r = TableView.RendererOf(column, Constants.Win11);

        var item = new List<AbstractListItem>();
        item.AddRange(ItemsOf(column, null, 10000, r, null));
        control.SuggestionsAdd(item);
        switch (ColumnItem.UserEditDialogTypeInTable(column, false)) {
            case EditTypeTable.Textfeld:
                control.AddAllowed = AddType.Text;
                break;

            case EditTypeTable.Listbox:
                control.AddAllowed = AddType.OnlySuggests;
                break;

            default:
                control.AddAllowed = AddType.None;
                break;
        }
    }

    protected void CommandButton_Click(object sender, System.EventArgs e) {
        if (_editType != EditTypeFormula.Button) { return; }
        OnButtonClicked();
    }

    protected override void Dispose(bool disposing) {
        try {
            if (disposing) {
                _infoText = string.Empty;
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
        //            ControlX = Main.Left - 1,
        //            Y = Main.Top - 1,
        //            Width = Main.Width + 2,
        //            Height = Height - Main.Top - 1
        //        };
        //        Skin.Draw_Border(gr, enDesign.ListBox, tmpstate, R);
        //    }
        //}
        if (!string.IsNullOrEmpty(DisabledReason)) {
            DoInfoTextCaption(DisabledReason);
        } else {
            DoInfoTextCaption(state.HasFlag(States.Standard_Disabled) ? "Übergeordnetes Steuerlement ist deaktiviert." : string.Empty);
        }
    }

    protected virtual void OnButtonClicked() => ButtonClicked?.Invoke(this, System.EventArgs.Empty);

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

    protected override void OnQuickInfoChanged() {
        base.OnQuickInfoChanged();
        UpdateControls();
    }

    protected virtual void OnValueChanged() => ValueChanged?.Invoke(this, System.EventArgs.Empty);

    /// <summary>
    /// Entfernt alle Controls und löst dessen die Events auf. Setzt Allinitialized auf false.
    /// </summary>
    protected void RemoveAll() {
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
        if (GetControl<ComboBox>() is { IsDisposed: false } cbx) {
            cbx.Focus();
            cbx.ShowMenu(null, null);
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
        control.Click += ColorButton_Click;
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
        control.Click += CommandButton_Click;
        return control;
    }

    private Button Control_Create_ButtonYesNo() {
        Button control = new() {
            Enabled = Enabled,
            Name = "YesNoButton",
            ButtonStyle = ButtonStyle.Yes_or_No,
            Text = string.Empty,
            ImageCode = string.Empty
        };
        control.CheckedChanged += YesNoButton_CheckedChanged;
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
        _captionObject.Translate = Translate;

        // nicht SteuerelementVerhalten.Steuerelement_Anpassen! weil sonst beim einem Resize die Koordinaten geändert werden und das kann zum Ping Pong führen
        // Text_abschneiden, wäre Cool, weil dann der Quickmode verfügbar ist

        _captionObject.Text = _captionPosition is CaptionPosition.Links_neben_dem_Feld_unsichtbar
                     or CaptionPosition.Über_dem_Feld_unsichtbar
            ? " "
            : _caption;

        if (_editType == EditTypeFormula.nur_als_Text_anzeigen) {
            // Kann alles sein, Beschriftung und was weiß ich.
            _captionObject.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            _captionObject.Size = BlueControls.Controls.Caption.RequiredTextSize(_captionObject.Text, _captionObject.TextAnzeigeVerhalten, Design.Caption, null, false, Width);
        } else {
            _captionObject.TextAnzeigeVerhalten = SteuerelementVerhalten.Text_Abschneiden;
            _captionObject.Size = _captionObject.RequiredTextSize();
        }

        _captionObject.BringToFront();
    }

    private ComboBox Control_Create_ComboBox() {
        ComboBox control = new();
        StyleComboBox(control, null, ComboBoxStyle.DropDownList, false, 1);
        control.TextChanged += ValueChanged_ComboBox;
        return control;
    }

    private GroupBox Control_Create_GroupBox() {
        GroupBox control = new() {
            Enabled = Enabled,
            GroupBoxStyle = GroupBoxStyle.NormalBold,
            Text = _caption
        };
        return control;
    }

    private Line Control_Create_Line() {
        Line control = new() {
            Enabled = Enabled,
            Orientation = Orientation.Waagerecht
        };
        return control;
    }

    private ListBox Control_Create_ListBox() {
        ListBox control = new() {
            Enabled = Enabled,
            CheckBehavior = CheckBehavior.MultiSelection
        };
        control.ItemClear();

        control.ItemCheckedChanged += ListBox_ItemCheckedChanged;
        return control;
    }

    private SwapListBox Control_Create_SwapListBox() {
        SwapListBox control = new() {
            Enabled = Enabled
        };
        control.UnCheck();
        control.ItemCheckedChanged += SwapListBox_ItemCheckedChanged;
        return control;
    }

    private TextBox Control_Create_TextBox() {
        TextBox control = new();
        StyleTextBox(control, 1);
        control.TextChanged += ValueChanged_TextBox;
        return control;
    }

    private void DoInfoTextCaption(string disabledReason) {
        if (InvokeRequired) {
            Invoke(new Action(() => DoInfoTextCaption(disabledReason)));
            return;
        }

        string txt;
        string symbol;
        if (string.IsNullOrEmpty(disabledReason) && string.IsNullOrEmpty(_infoText)) {
            txt = string.Empty;
            symbol = string.Empty;
        } else if (!string.IsNullOrEmpty(disabledReason) && string.IsNullOrEmpty(_infoText)) {
            symbol = "  <imagecode=Schloss|10|||||150||20>";
            txt = disabledReason;
        } else if (string.IsNullOrEmpty(disabledReason) && !string.IsNullOrEmpty(_infoText)) {
            symbol = "<imagecode=Warnung|16>";
            txt = _infoText;
        } else {
            symbol = "<imagecode=Information|16>";
            txt = "<b>Der Wert kann nicht bearbeitet werden:</b><br>" + disabledReason + "<br><br><b>Enthält aber einen Fehler:</b><br>" + _infoText;
        }
        if (!ShowInfoWhenDisabled && !string.IsNullOrEmpty(disabledReason)) { txt = string.Empty; }
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
            _infoCaption?.Visible = false;
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

    private void ListBox_ItemCheckedChanged(object sender, System.EventArgs e) => ValueSet(((ListBox)sender).Checked.JoinWithCr(), false);

    /// <summary>
    /// Erstellt zuerst die Standard-Caption, dessen Events werden registriert.
    /// Kümmert sich dann um die CanvasPosition des Controls im Bezug auf die Caption. Setzt die Sichtbarkeit, korrigiert Anachor und fügt das Control zu der Controll Collection hinzu.
    /// Konext-Menü-Events werden ebenfalls registriert, die andern Events werden nicht registriert und sollten nach dieser Rountine registert werden.
    /// </summary>
    /// <param name="control"></param>
    private void StandardBehandlung(Control? control) {
        if (control == null) { return; }

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
                var s1 = BlueControls.Controls.Caption.RequiredTextSize(_caption, SteuerelementVerhalten.Text_Abschneiden, Design.Caption, null, Translate, -1);

                control.Left = Math.Max(ControlX, s1.Width);
                control.Top = 0;
                control.Width = Width - control.Left;
                control.Height = Height;
                break;

            default:
                var s2 = BlueControls.Controls.Caption.RequiredTextSize(_caption, SteuerelementVerhalten.Text_Abschneiden, Design.Caption, null, Translate, -1);
                control.Left = 0;
                control.Top = Math.Max(ControlX, s2.Height);
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

    private void SwapListBox_ItemCheckedChanged(object sender, System.EventArgs e) => ValueSet(((SwapListBox)sender).Checked.JoinWithCr(), false);

    private void UpdateControls() {
        if (_captionObject is { IsDisposed: false } c) { c.Translate = Translate; }

        foreach (Control control in Controls) {
            if (control != _infoCaption) {
                if (control is GenericControl qi) { qi.QuickInfo = QuickInfo; }
                control.Enabled = Enabled;
            } else {
                control.Enabled = true;
            }

            if (control is IInputFormat inf) { inf.GetStyleFrom(this); }

            if (control is TextBox txb) { txb.Suffix = Suffix; }
        }
    }

    /// <summary>
    /// Setzt den aktuellen Wert, so dass es das Control anzeigt. Filling muss TRUE sein.
    /// </summary>
    private void UpdateValueTo_Button(Button control) {
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
        if (_editType != EditTypeFormula.nur_als_Text_anzeigen) { return; } // und auch dann nur als reine Text anzeige
        if (_captionObject == null) { return; }
        _captionObject.Width = Width;
        _captionObject.Translate = false;
        _captionObject.Text = _caption + " <i>" + Value;
    }

    private void UpdateValueTo_Combobox(ComboBox control) => control.Text = Value;

    /// <summary>
    /// Setzt den aktuellen Wert, so dass es das Control anzeigt. Filling muss TRUE sein.
    /// </summary>
    private void UpdateValueTo_ListBox(ListBox main) {
        main.UncheckAll();
        main.Check(Value.SplitAndCutByCr());
    }

    private void UpdateValueTo_SwapListBox(SwapListBox main) => main.Check(Value.SplitAndCutByCr());

    /// <summary>
    /// Setzt den aktuellen Wert, so dass es das Control anzeigt. Filling muss TRUE sein.
    /// </summary>

    private void UpdateValueTo_TextBox(TextBox control) => control.Text = Value;

    /// <summary>
    /// Setzt den aktuellen Wert, so dass es das Control anzeigt. Die Filling-Variable wird währenddessen umgesetzt.
    /// sollte vor StandardBehandlung kommen, da dort das Objekt gesetzt wird und dann die Handler generiert werden.
    /// </summary>

    private void UpdateValueToControl() {
        if (!Allinitialized && !Initializing) { CreateSubControls(); }

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
                    break;

                default:
                    Develop.DebugPrint(Typ(control));
                    break;
            }
        }
    }

    private void ValueChanged_ComboBox(object sender, System.EventArgs e) => ValueSet(((ComboBox)sender).Text, false);

    private void ValueChanged_TextBox(object sender, System.EventArgs e) => ValueSet(((TextBox)sender).Text, false);

    private void YesNoButton_CheckedChanged(object sender, System.EventArgs e) => ValueSet(((Button)sender).Checked.ToPlusMinus(), false);

    #endregion
}