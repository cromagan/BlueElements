// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Classes.ItemCollectionList.TableItems;
using BlueControls.Controls.FlexiControlStrategies;
using BlueControls.Designer_Support;
using BlueControls.EventArgs;
using BlueControls.Renderer;
using BlueTable.Interfaces;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

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
    private ColumnItem? _lastStyledRealColumn;
    private FlexiStrategyBase? _strategy;

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

        Size = BlueControls.Controls.Caption.RequiredTextSize(_caption, Design.Caption, Translate, width);
    }

    #endregion

    #region Events

    public event EventHandler? DropDownShowing;

    public event EventHandler? ExecuteComand;

    //public event EventHandler? ButtonClicked;
    //public event EventHandler? NeedRefresh;
    public event EventHandler<AbstractListItemEventArgs>? ItemRemoved;

    public event EventHandler<NavigationDirectionEventArgs>? NavigateToNext;

    [Obsolete("Value Changed benutzen", true)]
    public new event EventHandler? TextChanged;

    public event EventHandler? ValueChanged;

    #endregion

    #region Properties

    [DefaultValue(AdditionalCheck.None)]
    public AdditionalCheck AdditionalFormatCheck {
        get;
        set {
            if (field == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; UpdateControls(); }));
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
                Invoke(new Action(() => { field = value; UpdateControls(); }));
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
                Invoke(new Action(() => { RemoveAll(); _caption = value; }));
                return;
            }

            RemoveAll(); // Controls and Events entfernen!
            _caption = value;
        }
    }

    [DefaultValue(CaptionPosition.Über_dem_Feld)]
    public CaptionPosition CaptionPosition {
        get => _captionPosition;
        set {
            if (_captionPosition == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => { RemoveAll(); _captionPosition = value; }));
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
                Invoke(new Action(() => { RemoveAll(); field = value; }));
                return;
            }

            RemoveAll(); // Controls and Events entfernen!
            field = value;
        }
    } = -1;

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IReadOnlySet<string>? CustomVocabulary {
        get;
        set {
            if (field == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; UpdateControls(); }));
                return;
            }

            field = value;
            UpdateControls();
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
        get;
        set {
            if (field == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; foreach (System.Windows.Forms.Control thisControl in Controls) { thisControl.Enabled = thisControl == _infoCaption || Enabled; } DoInfoTextCaption(field); Invalidate(); }));
                return;
            }

            field = value;
            foreach (System.Windows.Forms.Control thisControl in Controls) {
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
                Invoke(new Action(() => { RemoveAll(); _editType = value; }));
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
            foreach (System.Windows.Forms.Control thisControl in Controls) {
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
                Invoke(new Action(() => { _infoText = value; Invalidate(); }));
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
                Invoke(new Action(() => { field = value; UpdateControls(); }));
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
                Invoke(new Action(() => { field = value; UpdateControls(); }));
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
                Invoke(new Action(() => { field = value; UpdateControls(); }));
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
                Invoke(new Action(() => { field = value; Invalidate(); }));
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
                Invoke(new Action(() => { field = value; UpdateControls(); }));
                return;
            }

            field = value;
            UpdateControls();
        }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FlexiStrategyBase? Strategy => _strategy;

    /// <summary>
    /// Falls das Steuerelement eine Suffix unterstützt, wird dieser angezeigt
    /// </summary>
    [DefaultValue("")]
    public string Suffix {
        get;
        set {
            if (field == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; UpdateControls(); }));
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
            if (field == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; UpdateControls(); }));
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
                Invoke(new Action(() => { field = value; UpdateControls(); }));
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
    public string Value {
        get => _strategy?.Value ?? string.Empty;
        set => _strategy?.Value = value;
    }

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
            Develop.DebugPrint("Width / Height zu klein");
            Allinitialized = true;
            Initializing = false;
            return;
        }

        _strategy = FlexiStrategyBase.GetStrategy(_editType);
        _strategy.Caption = Caption;

        if (_editType == EditTypeFormula.als_Überschrift_anzeigen) {
            _captionPosition = CaptionPosition.ohne;
        }

        if (_editType == EditTypeFormula.nur_als_Text_anzeigen) {
            _captionPosition = CaptionPosition.Links_neben_dem_Feld;
        }

        if (_editType != EditTypeFormula.None) {
            _strategy?.CreateControl();
            if (_strategy is not null) {
                _strategy.ValueChanged += Strategy_ValueChanged;
                _strategy.NavigateToNext += Strategy_NavigateToNext;
                _strategy.ExecuteComand += Strategy_ExecuteComand;
                _strategy.DropDownShowing += Strategy_DropDownShowing;
                _strategy.ItemRemoved += Strategy_ItemRemoved;
            }
        }

        StandardBehandlung(_strategy);
        UpdateValueToControl();

        UpdateControls();
        Allinitialized = true;
        Initializing = false;
    }

    public T? GetControl<T>() where T : System.Windows.Forms.Control {
        try {
            if (InvokeRequired) {
                return Invoke(new Func<T?>(GetControl<T>));
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

    public void StyleFromColumn(ColumnItem? column) {
        if (column == _lastStyledRealColumn) { return; }
        _lastStyledRealColumn = column;

        if (column is { IsDisposed: false }) {
            var r = TableView.RendererOf(column, Constants.Win11);
            QuickInfo = RowListItem.QuickInfoText(column, string.Empty);
            CustomVocabulary = column.Table is { } t ? new HashSet<string>(t.DictionaryWords) : null;

            if (r is Renderer_TextOneLine rol) { Suffix = rol.Suffix; }
            if (r is Renderer_Number rn) { Suffix = rn.Suffix; }

            if (_strategy is null) { return; }
            _strategy.ListItems = ItemsOf(column, null, 10000, r).ToList();
            _strategy.UserEditDialogType = ColumnItem.UserEditDialogTypeInTable(column, false);
            _strategy.TextInputAllowed = column.EditableWithTextInput;
            _strategy.DropdownAllowed = column.EditableWithDropdown;
            _strategy.ShowValuesOfOtherCellsInDropdown = column.ShowValuesOfOtherCellsInDropdown;
            _strategy.DropdownItems = column.DropDownItems;
            _strategy.RaiseChangeDelay = column.HasAutoRepair ? 10 : 1;
        } else {
            if (_strategy is not null) {
                _strategy.ListItems = null;
                _strategy.TextInputAllowed = false;
                _strategy.DropdownAllowed = false;
                _strategy.ShowValuesOfOtherCellsInDropdown = false;
                _strategy.DropdownItems = null;
                _strategy.RaiseChangeDelay = 1;
            }
        }
    }

    internal void InvokeNavigateToNext(NavigationDirection direction) => OnNavigateToNext(direction);

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
        if (IsDisposed) { return; }
        base.DrawControl(gr, state);

        // Enabled wurde verdeckt!
        if (!Enabled) { state = States.Standard_Disabled; }
        Skin.Draw_Back_Transparent(gr, ClientRectangle, this);
        if (!Allinitialized) { CreateSubControls(); }
        if (!string.IsNullOrEmpty(DisabledReason)) {
            DoInfoTextCaption(DisabledReason);
        } else {
            DoInfoTextCaption(state.HasFlag(States.Standard_Disabled) ? "Übergeordnetes Steuerlement ist deaktiviert." : string.Empty);
        }
    }

    protected override void OnControlRemoved(System.Windows.Forms.ControlEventArgs e) {
        base.OnControlRemoved(e);
        if (e.Control is not { } c) { return; }

        UnsubscribeEvents(c);
        if (c == _infoCaption) { _infoCaption = null; }
        if (c == _captionObject) { _captionObject = null; }
    }

    protected virtual void OnExecuteComand() => ExecuteComand?.Invoke(this, System.EventArgs.Empty);

    protected virtual void OnNavigateToNext(NavigationDirection direction) => NavigateToNext?.Invoke(this, new NavigationDirectionEventArgs(direction));

    protected override void OnQuickInfoChanged() {
        base.OnQuickInfoChanged();
        UpdateControls();
    }

    protected virtual void OnValueChanged() => ValueChanged?.Invoke(this, System.EventArgs.Empty);

    /// <summary>
    /// Entfernt alle Controls und löst dessen die Events auf. Setzt Allinitialized auf false.
    /// </summary>
    protected void RemoveAll() {
        List<System.Windows.Forms.Control> l = [];
        for (var z = 0; z < Controls.Count; z++) { l.Add(Controls[z]); }

        foreach (var thisc in l) {
            UnsubscribeEvents(thisc);
            thisc.Visible = false;
            if (thisc != _captionObject && thisc != _infoCaption) {
                thisc.Dispose(); // Dispose entfernt dass Control aus der Collection
            }
        }

        Allinitialized = false;
    }

    private void _InfoCaption_Click(object? sender, System.EventArgs e) {
        if (GetControl<ComboBox>() is { IsDisposed: false } cbx) {
            cbx.Focus();
            cbx.ShowMenu(null, null);
        }
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
        _captionObject.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
        _captionObject.Visible = true;
        _captionObject.Translate = Translate;

        // nicht SteuerelementVerhalten.Steuerelement_Anpassen! weil sonst beim einem Resize die Koordinaten geändert werden und das kann zum Ping Pong führen
        // Text_abschneiden, wäre Cool, weil dann der Quickmode verfügbar ist

        _captionObject.Text = _captionPosition is CaptionPosition.Links_neben_dem_Feld_unsichtbar
                     or CaptionPosition.Über_dem_Feld_unsichtbar
            ? " "
            : _caption;

        _captionObject.FitSize();
        _captionObject.BringToFront();
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
                Anchor = System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Top,
                Visible = true
            };
            Controls.Add(_infoCaption);
            _infoCaption.BringToFront();
            _infoCaption.Click += _InfoCaption_Click;
            _infoCaption.BringToFront();
        }
    }

    /// <summary>
    /// Erstellt zuerst die Standard-Caption, dessen Events werden registriert.
    /// Kümmert sich dann um die CanvasPosition des Controls im Bezug auf die Caption. Setzt die Sichtbarkeit, korrigiert Anachor und fügt das Control zu der Controll Collection hinzu.
    /// Konext-Menü-Events werden ebenfalls registriert, die andern Events werden nicht registriert und sollten nach dieser Rountine registert werden.
    /// </summary>
    private void StandardBehandlung(FlexiStrategyBase? st) {
        if (st?.Control is not System.Windows.Forms.Control control) { return; }

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
                var s1 = BlueControls.Controls.Caption.RequiredTextSize(_caption, Design.Caption, Translate, -1);

                control.Left = Math.Max(ControlX, s1.Width);
                control.Top = 0;
                control.Width = Width - control.Left;
                control.Height = Height;
                break;

            default:
                var s2 = BlueControls.Controls.Caption.RequiredTextSize(_caption, Design.Caption, Translate, -1);
                control.Left = 0;
                control.Top = Math.Max(ControlX, s2.Height);
                control.Width = Width;
                control.Height = Height - s2.Height;
                break;
        }
        control.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom;
        control.Visible = true;
        Controls.Add(control);
        st.SubscribeEvents();
        Invalidate();
        //DoInfoTextCaption();
    }

    private void Strategy_DropDownShowing(object? sender, System.EventArgs e) => DropDownShowing?.Invoke(this, System.EventArgs.Empty);

    private void Strategy_ExecuteComand(object? sender, System.EventArgs e) => OnExecuteComand();

    private void Strategy_ItemRemoved(object? sender, AbstractListItemEventArgs e) => ItemRemoved?.Invoke(this, e);

    private void Strategy_NavigateToNext(object? sender, NavigationDirectionEventArgs e) => InvokeNavigateToNext(e.Direction);

    private void Strategy_ValueChanged(object? sender, System.EventArgs e) => OnValueChanged();

    private void UnsubscribeEvents(System.Windows.Forms.Control control) {
        if (control != null && _strategy?.Control == control) {
            _strategy.ValueChanged -= Strategy_ValueChanged;
            _strategy.NavigateToNext -= Strategy_NavigateToNext;
            _strategy.ExecuteComand -= Strategy_ExecuteComand;
            _strategy.DropDownShowing -= Strategy_DropDownShowing;
            _strategy.ItemRemoved -= Strategy_ItemRemoved;
            _strategy.UnsubscribeEvents();
        }
    }

    private void UpdateControls() {
        if (_captionObject is { IsDisposed: false } c) { c.Translate = Translate; }

        foreach (System.Windows.Forms.Control control in Controls) {
            if (control != _infoCaption) {
                if (control is GenericControl qi) { qi.QuickInfo = QuickInfo; }
                control.Enabled = Enabled;
            } else {
                control.Enabled = true;
            }

            if (control is IInputFormat inf) { inf.GetStyleFrom(this); }

            if (control is TextBox txb) { txb.Suffix = Suffix; txb.CustomVocabulary = CustomVocabulary; }
        }
    }

    /// <summary>
    /// Setzt den aktuellen Wert, so dass es das Control anzeigt. Die Filling-Variable wird währenddessen umgesetzt.
    /// sollte vor StandardBehandlung kommen, da dort das Objekt gesetzt wird und dann die Handler generiert werden.
    /// </summary>
    private void UpdateValueToControl() {
        if (!Allinitialized && !Initializing) { CreateSubControls(); }

        if (_strategy != null) {
            _strategy.Value = Value;
        }

        if (_editType == EditTypeFormula.nur_als_Text_anzeigen && _captionObject != null) {
            _captionObject.Width = Width;
            _captionObject.Translate = false;
            _captionObject.Text = _caption + " <i>" + Value;
        }
    }

    #endregion
}