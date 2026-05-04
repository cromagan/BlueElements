// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Controls.FlexiControlStrategies;
using BlueControls.Designer_Support;
using BlueControls.EventArgs;
using BlueTable.Interfaces;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[DefaultEvent(nameof(ValueChanged))]
public partial class FlexiControl : GenericControl, IBackgroundNone, IInputFormat, ITranslateable {

    #region Fields

    private Caption? _captionObject;

    private Caption? _infoCaption;
    private FlexiStrategyBase? _strategy;

    #endregion

    #region Constructors

    public FlexiControl() : base(false, false, false) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();

        EditType = EditTypeFormula.Line;
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

        EditType = isCaption ? EditTypeFormula.als_Überschrift_anzeigen : EditTypeFormula.nur_als_Text_anzeigen;

        Caption = captionText;
        CaptionPosition = CaptionPosition.ohne;

        Size = BlueControls.Controls.Caption.RequiredTextSize(Caption, Design.Caption, Translate, width);
    }

    #endregion

    #region Events

    [DefaultValue(AdditionalCheck.None)]
    public event EventHandler? DropDownShowing;

    public event EventHandler? ExecuteComand;

    public event EventHandler<AbstractListItemEventArgs>? ItemRemoved;

    public event EventHandler<NavigationDirectionEventArgs>? NavigateToNext;

    [Obsolete("Value Changed benutzen", true)]
    public new event EventHandler? TextChanged;

    public event EventHandler? ValueChanged;

    #endregion

    #region Properties

    [DefaultValue(AddType.None)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public AddType AddAllowed {
        get;
        set {
            if (field == value) { return; }
            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; _strategy?.AddAllowed = value; }));
                return;
            }
            field = value;
            _strategy?.AddAllowed = value;
        }
    }

    [DefaultValue(AdditionalCheck.None)]
    public AdditionalCheck AdditionalFormatCheck {
        get;
        set {
            if (field == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; _strategy?.AdditionalFormatCheck = value; }));
                return;
            }

            field = value;
            _strategy?.AdditionalFormatCheck = value;
        }
    } = AdditionalCheck.None;

    public bool Allinitialized { get; private set; }

    [DefaultValue("")]
    public string AllowedChars {
        get;
        set {
            if (field == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; _strategy?.AllowedChars = value; }));
                return;
            }

            field = value;
            _strategy?.AllowedChars = value;
        }
    } = string.Empty;

    [DefaultValue(true)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool AutoSort {
        get;
        set {
            if (field == value) { return; }
            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; _strategy?.AutoSort = value; }));
                return;
            }
            field = value;
            _strategy?.AutoSort = value;
        }
    } = true;

    [DefaultValue("")]
    public string Caption {
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
    } = "";

    [DefaultValue(CaptionPosition.Über_dem_Feld)]
    public CaptionPosition CaptionPosition {
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
    } = CaptionPosition.ohne;

    [DefaultValue(CheckBehavior.MultiSelection)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public CheckBehavior CheckBehavior {
        get;
        set {
            if (field == value) { return; }
            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; _strategy?.CheckBehavior = value; }));
                return;
            }
            field = value;
            _strategy?.CheckBehavior = value;
        }
    } = CheckBehavior.MultiSelection;

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
    public ReadOnlyCollection<AbstractListItem>? CustomContextMenuItems {
        get;
        set {
            if (field == value) { return; }
            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; _strategy?.CustomContextMenuItems = value; }));
                return;
            }

            field = value;
            _strategy?.CustomContextMenuItems = value;
        }
    }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IReadOnlySet<string>? CustomVocabulary {
        get;
        set {
            if (field == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; _strategy?.CustomVocabulary = value; }));
                return;
            }

            field = value;
            _strategy?.CustomVocabulary = value;
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

    [DefaultValue(false)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DropdownAllowed {
        get;
        set {
            if (field == value) { return; }
            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; _strategy?.DropdownAllowed = value; }));
                return;
            }

            field = value;
            _strategy?.DropdownAllowed = value;
        }
    } = false;

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IReadOnlyList<string>? DropdownItems {
        get;
        set {
            if (field == value) { return; }
            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; _strategy?.DropdownItems = value; }));
                return;
            }

            field = value;
            _strategy?.DropdownItems = value;
        }
    }

    [DefaultValue(EditTypeFormula.None)]
    public EditTypeFormula EditType {
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
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string ImageCode {
        get;
        set {
            if (field == value) { return; }
            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; _strategy?.ImageCode = value; }));
                return;
            }

            field = value;
            _strategy?.ImageCode = value;
        }
    } = string.Empty;

    [DefaultValue("")]
    [Description("Zeigt rechts oben im Eck ein kleines Symbol an, dessen hier eingegebener Text angezeigt wird.")]
    public string InfoText {
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

    public bool Initializing { get; private set; }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<AbstractListItem>? ListItems {
        get;
        set {
            if (field == value) { return; }
            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; _strategy?.ListItems = value; }));
                return;
            }

            field = value;
            _strategy?.ListItems = value;
        }
    }

    [DefaultValue(4000)]
    public int MaxTextLength {
        get;
        set {
            if (field == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; _strategy?.MaxTextLength = value; }));
                return;
            }

            field = value;
            _strategy?.MaxTextLength = value;
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
                Invoke(new Action(() => { field = value; _strategy?.MultiLine = value; }));
                return;
            }

            field = value;
            _strategy?.MultiLine = value;
        }
    }

    [DefaultValue(1)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int RaiseChangeDelay {
        get;
        set {
            if (field == value) { return; }
            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; _strategy?.RaiseChangeDelay = value; }));
                return;
            }
            field = value;
            _strategy?.RaiseChangeDelay = value;
        }
    }

    [DefaultValue("")]
    public string RegexCheck {
        get;
        set {
            if (field == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; _strategy?.RegexCheck = value; }));
                return;
            }

            field = value;
            _strategy?.RegexCheck = value;
        }
    } = string.Empty;

    [DefaultValue(false)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool RemoveAllowed {
        get;
        set {
            if (field == value) { return; }
            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; _strategy?.RemoveAllowed = value; }));
                return;
            }

            field = value;
            _strategy?.RemoveAllowed = value;
        }
    }

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
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool ShowValuesOfOtherCellsInDropdown {
        get;
        set {
            if (field == value) { return; }
            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; _strategy?.ShowValuesOfOtherCellsInDropdown = value; }));
                return;
            }
            field = value;
            _strategy?.ShowValuesOfOtherCellsInDropdown = value;
        }
    } = false;

    [DefaultValue(false)]
    public bool SpellCheckingEnabled {
        get;
        set {
            if (field == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; _strategy?.SpellCheckingEnabled = value; }));
                return;
            }

            field = value;
            _strategy?.SpellCheckingEnabled = value;
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
                Invoke(new Action(() => { field = value; _strategy?.Suffix = value; }));
                return;
            }

            field = value;
            _strategy?.Suffix = value;
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
                Invoke(new Action(() => { field = value; _strategy?.TextFormatingAllowed = value; }));
                return;
            }

            field = value;
            _strategy?.TextFormatingAllowed = value;
        }
    } = false;

    [DefaultValue(false)]
    public bool TextInputAllowed {
        get;
        set {
            if (field == value) { return; }
            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; _strategy?.TextInputAllowed = value; }));
                return;
            }

            field = value;
            _strategy?.TextInputAllowed = value;
        }
    } = false;

    [DefaultValue(true)]
    public bool Translate {
        get;
        set {
            if (field == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; if (_captionObject is { IsDisposed: false } c) { c.Translate = value; } }));
                return;
            }

            field = value;

            if (_captionObject is { IsDisposed: false } c) { c.Translate = value; }
        }
    } = true;

    [DefaultValue(EditTypeTable.None)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public EditTypeTable UserEditDialogType {
        get;
        set {
            if (field == value) { return; }
            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; _strategy?.UserEditDialogType = value; }));
                return;
            }

            field = value;
            _strategy?.UserEditDialogType = value;
        }
    } = EditTypeTable.None;

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

    public void BeginUpdate() => _strategy?.BeginInit();

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

        _strategy = FlexiStrategyBase.GetStrategy(EditType);
        if (_strategy is null) {
            Allinitialized = true;
            Initializing = false;
            return;
        }

        _strategy.CreateControl();

        _strategy.BeginInit();
        _strategy.Caption = Caption;
        _strategy.ImageCode = ImageCode;

        _strategy.AdditionalFormatCheck = AdditionalFormatCheck;
        _strategy.AddAllowed = AddAllowed;
        _strategy.AllowedChars = AllowedChars;
        _strategy.AutoSort = AutoSort;
        _strategy.CheckBehavior = CheckBehavior;
        _strategy.MaxTextLength = MaxTextLength;
        _strategy.MultiLine = MultiLine;
        _strategy.RegexCheck = RegexCheck;
        _strategy.SpellCheckingEnabled = SpellCheckingEnabled;
        _strategy.TextFormatingAllowed = TextFormatingAllowed;
        _strategy.CustomVocabulary = CustomVocabulary;
        _strategy.Suffix = Suffix;
        _strategy.ParentHeight = Height;
        _strategy.QuickInfo = QuickInfo;
        _strategy.DropdownAllowed = DropdownAllowed;
        _strategy.DropdownItems = DropdownItems;
        _strategy.ListItems = ListItems;
        _strategy.CustomContextMenuItems = CustomContextMenuItems;
        _strategy.RaiseChangeDelay = RaiseChangeDelay;
        _strategy.ShowValuesOfOtherCellsInDropdown = ShowValuesOfOtherCellsInDropdown;
        _strategy.TextInputAllowed = TextInputAllowed;
        _strategy.UserEditDialogType = UserEditDialogType;

        StandardBehandlung(_strategy.Control);

        _strategy.Value = Value; // Abonniert die Events

        Allinitialized = true;
        Initializing = false;
        _strategy.EndInit();

        _strategy.ValueChanged += Strategy_ValueChanged;
        _strategy.NavigateToNext += Strategy_NavigateToNext;
        _strategy.ExecuteComand += Strategy_ExecuteComand;
        _strategy.DropDownShowing += Strategy_DropDownShowing;
        _strategy.ItemRemoved += Strategy_ItemRemoved;
    }

    public void EndUpdate() => _strategy?.EndInit();

    public Task HighlightWordsAsync(IReadOnlyList<string> words, string ownWord, CancellationToken cancellationToken) =>
                _strategy?.HighlightWordsAsync(words, ownWord, cancellationToken) ?? Task.CompletedTask;

    public bool WasValueClicked() => _strategy?.WasValueClicked() ?? false;

    internal void InvokeNavigateToNext(NavigationDirection direction) => OnNavigateToNext(direction);

    protected override void Dispose(bool disposing) {
        try {
            if (disposing) {
                RemoveAll();

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
        if (_strategy?.Control is GenericControl qi) { qi.QuickInfo = QuickInfo; }
    }

    protected virtual void OnValueChanged() => ValueChanged?.Invoke(this, System.EventArgs.Empty);

    /// <summary>
    /// Entfernt die Strategy und deren Control. InfoCaption bleibt erhalten.
    /// Setzt Allinitialized auf false, sodass beim nächsten Zeichnen neu erstellt wird.
    /// </summary>
    protected void RemoveAll() {
        if (_strategy is null && _captionObject is null) {
            Allinitialized = false;
            return;
        }

        if (_strategy is not null) {
            _strategy.ValueChanged -= Strategy_ValueChanged;
            _strategy.NavigateToNext -= Strategy_NavigateToNext;
            _strategy.ExecuteComand -= Strategy_ExecuteComand;
            _strategy.DropDownShowing -= Strategy_DropDownShowing;
            _strategy.ItemRemoved -= Strategy_ItemRemoved;
            _strategy.UnsubscribeEvents();

            if (_strategy.Control is { IsDisposed: false } control) {
                control.Visible = false;
                control.Dispose();
            }

            _strategy = null;
        }

        if (_captionObject is not null) {
            _captionObject.Visible = false;
            _captionObject.Dispose();
            _captionObject = null;
        }

        Allinitialized = false;
    }

    private void _InfoCaption_Click(object? sender, System.EventArgs e) {
        _strategy?.HandleCaptionClick();
    }

    private void Control_Create_Caption() {
        if (CaptionPosition == CaptionPosition.ohne) { return; }
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

        _captionObject.Text = CaptionPosition is CaptionPosition.Links_neben_dem_Feld_unsichtbar
                     or CaptionPosition.Über_dem_Feld_unsichtbar
            ? " "
            : Caption;

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
        if (string.IsNullOrEmpty(disabledReason) && string.IsNullOrEmpty(InfoText)) {
            txt = string.Empty;
            symbol = string.Empty;
        } else if (!string.IsNullOrEmpty(disabledReason) && string.IsNullOrEmpty(InfoText)) {
            symbol = "  <imagecode=Schloss|10|||||150||20>";
            txt = disabledReason;
        } else if (string.IsNullOrEmpty(disabledReason) && !string.IsNullOrEmpty(InfoText)) {
            symbol = "<imagecode=Warnung|16>";
            txt = InfoText;
        } else {
            symbol = "<imagecode=Information|16>";
            txt = "<b>Der Wert kann nicht bearbeitet werden:</b><br>" + disabledReason + "<br><br><b>Enthält aber einen Fehler:</b><br>" + InfoText;
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
    private void StandardBehandlung(System.Windows.Forms.Control? control) {
        if (control is not { }) { return; }

        Control_Create_Caption();
        switch (CaptionPosition) {
            case CaptionPosition.ohne:
                control.Left = 0;
                control.Top = 0;
                control.Width = Width;
                control.Height = Height;
                break;

            case CaptionPosition.Links_neben_dem_Feld_unsichtbar:
            case CaptionPosition.Links_neben_dem_Feld:
                var s1 = BlueControls.Controls.Caption.RequiredTextSize(Caption, Design.Caption, Translate, -1);

                control.Left = Math.Max(ControlX, s1.Width);
                control.Top = 0;
                control.Width = Width - control.Left;
                control.Height = Height;
                break;

            default:
                var s2 = BlueControls.Controls.Caption.RequiredTextSize(Caption, Design.Caption, Translate, -1);
                control.Left = 0;
                control.Top = Math.Max(ControlX, s2.Height);
                control.Width = Width;
                control.Height = Height - s2.Height;
                break;
        }
        control.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom;
        control.Visible = true;
        Controls.Add(control);
        Invalidate();
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

    #endregion
}