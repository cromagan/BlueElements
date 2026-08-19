// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.ControlStrategies;
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
    private ControlStrategy? _strategy;

    #endregion

    #region Constructors

    public FlexiControl() : base(false, false, false) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();

        ControlStrategy = LineControlStrategy.ClassId;

        Size = new Size(200, 8);
        CreateSubControls();
    }

    /// <summary>
    /// Erzeugt eine fette Caption (isCaption) oder einen hervorgehobenen Info-Text.
    /// Der Text wird immer als Value an die Strategie übergeben.
    /// </summary>
    public FlexiControl(string captionText, int width, bool isCaption) : base(false, false, false) {
        ControlStrategy = isCaption ? CaptionControlStrategy.ClassId : TextControlStrategy.ClassId;

        Value = captionText;
        CaptionPosition = CaptionPosition.ohne;

        Size = BlueControls.Controls.Caption.RequiredTextSize(captionText, Design.Caption, Translate, width);
        CreateSubControls();
    }

    #endregion

    #region Events

    [DefaultValue(AdditionalCheck.None)]
    public event EventHandler? DropDownShowing;

    public event EventHandler? ExecuteCommand;

    public event EventHandler<ListItemEventArgs>? ItemRemoved;

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
                Invoke(new Action(() => { field = value; UpdateLayout(); }));
                return;
            }

            field = value;
            UpdateLayout();
        }
    } = "";

    [DefaultValue(CaptionPosition.Über_dem_Feld)]
    public CaptionPosition CaptionPosition {
        get;
        set {
            if (field == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; UpdateLayout(); }));
                return;
            }

            field = value;
            UpdateLayout();
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
    /// ClassId der ControlStrategie, die das Bearbeitungs-Control erzeugt.
    /// </summary>
    [DefaultValue("None")]
    public string ControlStrategy {
        get;
        set {
            if (field == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; RemoveStrategy(); }));
                return;
            }

            field = value;
            RemoveStrategy();
        }
    } = NoneControlStrategy.ClassId;

    /// <summary>
    /// Parameter der ControlStrategie als Json — analog zu RendererSettings beim Renderer.
    /// </summary>
    public JsonObject ControlStrategyParameter {
        get;
        set {
            if (field == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; RemoveStrategy(); }));
                return;
            }

            field = value;
            RemoveStrategy();
        }
    } = new();

    /// <summary>
    /// Ab welchen Wert in Pixel das Eingabesteuerelement beginnen darf.
    /// </summary>
    [DefaultValue(-1)]
    public int ControlX {
        get;
        set {
            if (field == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; UpdateLayout(); }));
                return;
            }

            field = value;
            UpdateLayout();
        }
    } = -1;

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ReadOnlyCollection<ListItem>? CustomContextMenuItems {
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
                Invoke(new Action(() => { field = value; DoEnabledState(); }));
                return;
            }

            field = value;
            DoEnabledState();
        }
    } = string.Empty;

    /// <summary>
    /// DisabledReason befüllen, um das Steuerelement zu disablen
    /// </summary>
    [DefaultValue(true)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool Enabled => !DesignMode && string.IsNullOrEmpty(DisabledReason);

    [DefaultValue("")]
    public string ForbiddenChars {
        get;
        set {
            if (field == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; _strategy?.ForbiddenChars = value; }));
                return;
            }

            field = value;
            _strategy?.ForbiddenChars = value;
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
    } = string.Empty;

    public bool Initializing { get; private set; }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<ListItem>? ListItems {
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

    [DefaultValue(0)]
    public int MinTextLength {
        get;
        set {
            if (field == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; _strategy?.MinTextLength = value; }));
                return;
            }

            field = value;
            _strategy?.MinTextLength = value;
        }
    }

    [DefaultValue(false)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool MoveAllowed {
        get;
        set {
            if (field == value) { return; }
            if (InvokeRequired) {
                Invoke(new Action(() => { field = value; _strategy?.MoveAllowed = value; }));
                return;
            }

            field = value;
            _strategy?.MoveAllowed = value;
        }
    }

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

    /// <summary>
    /// Aktive Strategie; wird beim ersten Zugriff automatisch erzeugt.
    /// Bleibt die Erzeugung erfolglos, die gecachte Prototyp-Instanz.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ControlStrategy Strategy {
        get {
            if (!Allinitialized && !Initializing && !IsDisposed) { CreateSubControls(); }
            return _strategy ?? ControlStrategies.ControlStrategy.Cached(ControlStrategy);
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

    [Obsolete("Value anstelle Text benutzen", true)]
    [DefaultValue("")]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new string Text { get; set; } = string.Empty;

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

    [DefaultValue("")]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Value {
        get;
        set {
            if (field == value) { return; }

            if (InvokeRequired) {
                Invoke(new Action(() => {
                    field = value;
                    _strategy?.Value = value;
                    OnValueChanged();
                }));
                return;
            }

            field = value; // Value muss hier gespeichtert werden, ansonsten verlieren Controls beim stragie-Wechsel den Wert
            _strategy?.Value = value;
            OnValueChanged();
        }
    } = string.Empty;

    #endregion

    #region Methods

    public void BeginUpdate() => _strategy?.BeginInit();

    public void EndUpdate() => _strategy?.EndInit();

    public Task HighlightWordsAsync(IReadOnlyList<string> words, string ownWord, CancellationToken cancellationToken) =>
                    _strategy?.HighlightWordsAsync(words, ownWord, cancellationToken) ?? Task.CompletedTask;

    public bool WasValueClicked() => _strategy?.WasValueClicked() ?? false;

    internal void InvokeNavigateToNext(NavigationDirection direction) => OnNavigateToNext(direction);

    protected override void Dispose(bool disposing) {
        try {
            if (disposing) {
                _strategy?.Dispose();
                _strategy = null;

                _captionObject = null;

                Allinitialized = false;

                var childControls = new List<System.Windows.Forms.Control>();
                foreach (System.Windows.Forms.Control c in Controls) { childControls.Add(c); }

                foreach (var c in childControls) {
                    if (c is IDisposable d) { d.Dispose(); }
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

    protected virtual void OnDropDownShowing() {
        if (IsDisposed) { return; }
        DropDownShowing?.Invoke(this, System.EventArgs.Empty);
    }

    protected virtual void OnExecuteCommand() {
        if (IsDisposed) { return; }
        ExecuteCommand?.Invoke(this, System.EventArgs.Empty);
    }

    protected virtual void OnItemRemoved(ListItemEventArgs e) {
        if (IsDisposed) { return; }
        ItemRemoved?.Invoke(this, e);
    }

    protected virtual void OnNavigateToNext(NavigationDirection direction) {
        if (IsDisposed) { return; }
        NavigateToNext?.Invoke(this, new NavigationDirectionEventArgs(direction));
    }

    protected override void OnQuickInfoChanged() {
        if (IsDisposed) { return; }
        base.OnQuickInfoChanged();
        if (_strategy is { } s) { s.QuickInfo = QuickInfo; }
    }

    protected override void OnSizeChanged(System.EventArgs e) {
        base.OnSizeChanged(e);
        if (_strategy is { } s) { s.ParentHeight = Height; }
    }

    protected virtual void OnValueChanged() {
        if (IsDisposed) { return; }
        ValueChanged?.Invoke(this, System.EventArgs.Empty);
    }

    private void _InfoCaption_Click(object? sender, System.EventArgs e) => _strategy?.HandleCaptionClick();

    private void Control_Create_Caption() {
        if (CaptionPosition == CaptionPosition.ohne) { return; }
        if (_captionObject is null) {
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

    /// <summary>
    /// Erstellt die Steuerelemente zur Bearbeitung und auch die Caption und alles was gebrauch wird.
    /// Die Events werden Registriert und auch der Wert gesetzt.
    /// </summary>
    private void CreateSubControls() {
        if (InvokeRequired) {
            Invoke(new Action(CreateSubControls));
            return;
        }
        if (IsDisposed) { return; }
        if (Allinitialized || Initializing) { return; }

        Initializing = true;

        try {
            if (Width < 5 || Height < 5) {
                Develop.DebugPrint("Width / Height zu klein");
                Allinitialized = true;
                return;
            }

            _strategy = ControlStrategies.ControlStrategy.CreateNew(ControlStrategy);

            var _strategyControl = _strategy.CreateControl();

            _strategy.BeginInit();
            _strategy.ControlStrategyParameter = ControlStrategyParameter;

            _strategy.AdditionalFormatCheck = AdditionalFormatCheck;
            _strategy.AddAllowed = AddAllowed;
            _strategy.AllowedChars = AllowedChars;
            _strategy.ForbiddenChars = ForbiddenChars;
            _strategy.AutoSort = AutoSort;
            _strategy.CheckBehavior = CheckBehavior;
            _strategy.MaxTextLength = MaxTextLength;
            _strategy.MinTextLength = MinTextLength;
            _strategy.MultiLine = MultiLine;
            _strategy.MoveAllowed = MoveAllowed;
            _strategy.RegexCheck = RegexCheck;
            _strategy.CustomVocabulary = CustomVocabulary;
            _strategy.Suffix = Suffix;
            _strategy.ParentHeight = Height;
            _strategy.QuickInfo = QuickInfo;
            _strategy.ListItems = ListItems;
            _strategy.CustomContextMenuItems = CustomContextMenuItems;
            _strategy.RaiseChangeDelay = RaiseChangeDelay;
            _strategy.TextInputAllowed = TextInputAllowed;

            Control_Create_Caption();
            PositionStrategyControl();
            Controls.Add(_strategyControl);

            _strategy.Value = Value;

            Allinitialized = true;
            _strategy.EndInit();

            // Value-Setter abonniert Events nur bei Wertänderung (Überspringt "" == "").
            // Daher hier sicherstellen, dass die Strategy-Events immer genau einmal abonniert sind.
            _strategy.UnsubscribeEvents();
            _strategy.SubscribeEvents();

            DoEnabledState();

            _strategy.ValueChanged += Strategy_ValueChanged;
            _strategy.NavigateToNext += Strategy_NavigateToNext;
            _strategy.ExecuteCommand += Strategy_ExecuteCommand;
            _strategy.DropDownShowing += Strategy_DropDownShowing;
            _strategy.ItemRemoved += Strategy_ItemRemoved;
        } catch {
            _strategy?.Dispose();
            _strategy = null;
            Allinitialized = true;
        } finally {
            Invalidate();
            Initializing = false;
        }
    }

    private void DoEnabledState() {
        var enabled = Enabled;

        foreach (System.Windows.Forms.Control thisControl in Controls) {
            thisControl.Enabled = thisControl == _infoCaption || enabled;
        }
        DoInfoTextCaption(DisabledReason);
        Invalidate();
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
        if (!string.IsNullOrEmpty(txt) && _infoCaption is not null) {
            _infoCaption.Left = Width - 18;
            _infoCaption.Top = 0;
            _infoCaption.QuickInfo = txt;
            _infoCaption.Text = symbol;
            _infoCaption.Visible = true;
            _infoCaption.BringToFront();
            return;
        }

        if (string.IsNullOrEmpty(txt) && _infoCaption is null) { return; }

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

    private void PositionStrategyControl() {
        if (_strategy is null) { return; }

        switch (CaptionPosition) {
            case CaptionPosition.ohne:
                _strategy.Left = 0;
                _strategy.Top = 0;
                _strategy.Width = Width;
                _strategy.Height = Height;
                break;

            case CaptionPosition.Links_neben_dem_Feld_unsichtbar:
            case CaptionPosition.Links_neben_dem_Feld:
                var s1 = BlueControls.Controls.Caption.RequiredTextSize(Caption, Design.Caption, Translate, -1);

                _strategy.Left = Math.Max(ControlX, s1.Width + Skin.PaddingSmal);
                _strategy.Top = 0;
                _strategy.Width = Width - _strategy.Left;
                _strategy.Height = Height;
                break;

            default:
                var s2 = BlueControls.Controls.Caption.RequiredTextSize(Caption, Design.Caption, Translate, -1);
                _strategy.Left = 0;
                _strategy.Top = Math.Max(ControlX, s2.Height);
                _strategy.Width = Width;
                _strategy.Height = Height - s2.Height;
                break;
        }
        _strategy.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom;
        _strategy.Visible = true;
    }

    /// <summary>
    /// Entfernt nur die Strategy und deren Control. Caption und InfoCaption bleiben erhalten.
    /// Setzt Allinitialized auf false, sodass beim nächsten Zeichnen die Strategy neu erstellt wird.
    /// </summary>
    private void RemoveStrategy() {
        if (_strategy is null) {
            Allinitialized = false;
            return;
        }
        _strategy.Dispose();
        _strategy = null;
        Allinitialized = false;
    }

    private void Strategy_DropDownShowing(object? sender, System.EventArgs e) => OnDropDownShowing();

    private void Strategy_ExecuteCommand(object? sender, System.EventArgs e) => OnExecuteCommand();

    private void Strategy_ItemRemoved(object? sender, ListItemEventArgs e) => OnItemRemoved(e);

    private void Strategy_NavigateToNext(object? sender, NavigationDirectionEventArgs e) => InvokeNavigateToNext(e.Direction);

    private void Strategy_ValueChanged(object? sender, TextEventArgs e) => Value = e.Text;

    /// <summary>
    /// Aktualisiert das Layout (Caption und Position des Strategy-Controls),
    /// ohne die Strategy zu zerstören.
    /// </summary>
    private void UpdateLayout() {
        if (CaptionPosition == CaptionPosition.ohne && _captionObject is { IsDisposed: false }) {
            _captionObject.Visible = false;
            _captionObject.Dispose();
            _captionObject = null;
        }

        Control_Create_Caption();
        PositionStrategyControl();
        Invalidate();
    }

    #endregion
}